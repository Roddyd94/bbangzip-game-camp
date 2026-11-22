using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(SplineContainer), typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public sealed class SplineBuilder : MonoBehaviour
{
    [SerializeField, Tooltip("로컬 Z축이 길이 방향인 모델 프리팹입니다. 메시의 Read/Write를 활성화하세요.")]
    private GameObject segmentPrefab;

    [SerializeField, Tooltip("스플라인 단면에서 모델의 가로 및 높이 오프셋입니다.")]
    private Vector2 segmentOffset;

    private SplineContainer splineContainer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh generatedMesh;
    private Mesh previousVisualMesh;
    private Mesh previousCollisionMesh;
    private Material[] previousMaterials;
    private bool rebuildRequested;

    private void OnEnable()
    {
        splineContainer = GetComponent<SplineContainer>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        Spline.Changed += OnSplineChanged;
        rebuildRequested = true;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
        ClearGeneratedMesh();
    }

    private void OnValidate()
    {
        rebuildRequested = true;
    }

    private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
    {
        if (splineContainer.Splines.Count == 0 || spline != splineContainer.Spline)
            return;

        rebuildRequested = true;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
#endif
    }

    private void Update()
    {
        if (rebuildRequested)
            RebuildTrack();
    }

    [ContextMenu("Rebuild Track")]
    private void RebuildFromInspector() => RebuildTrack();

    public bool RebuildTrack()
    {
        rebuildRequested = false;
        if (!isActiveAndEnabled || splineContainer == null)
            return false;

        if (segmentPrefab == null || splineContainer.Splines.Count == 0 || splineContainer.Spline.Count < 2)
        {
            ClearGeneratedMesh();
            return false;
        }

        if (!TryBuildMesh(splineContainer.Spline, out Mesh mesh, out Material[] materials))
        {
            ClearGeneratedMesh();
            return false;
        }

        if (generatedMesh == null)
        {
            previousVisualMesh = meshFilter.sharedMesh;
            previousCollisionMesh = meshCollider.sharedMesh;
            previousMaterials = meshRenderer.sharedMaterials;
        }

        Mesh oldMesh = generatedMesh;
        generatedMesh = mesh;
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterials = materials;
        meshCollider.sharedMesh = mesh;
        ReleaseMesh(oldMesh);
        return true;
    }

    private bool TryBuildMesh(Spline spline, out Mesh mesh, out Material[] materialSlots)
    {
        mesh = null;
        materialSlots = null;

        float splineLength = spline.GetLength();
        if (!(splineLength > 0.001f) || float.IsInfinity(splineLength))
            return false;

        var sourceVertices = new List<Vector3>();
        var sourceNormals = new List<Vector3>();
        var sourceUv = new List<Vector2>();
        var materials = new List<Material>();
        var sourceTriangles = new List<List<int>>();
        Transform root = segmentPrefab.transform;
        foreach (MeshFilter filter in segmentPrefab.GetComponentsInChildren<MeshFilter>())
        {
            Mesh source = filter.sharedMesh;
            if (source == null || !filter.TryGetComponent(out MeshRenderer renderer))
                continue;

            if (!source.isReadable)
            {
                Debug.LogWarning($"Enable Read/Write on track segment mesh '{source.name}'.", this);
                return false;
            }

            Matrix4x4 toSegment = root.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            Matrix4x4 toSegmentNormals = toSegment.inverse.transpose;
            int vertexStart = sourceVertices.Count;
            Vector3[] positions = source.vertices;
            Vector3[] normals = source.normals;
            Vector2[] uv = source.uv;
            for (int i = 0; i < positions.Length; i++)
            {
                sourceVertices.Add(toSegment.MultiplyPoint3x4(positions[i]));
                sourceNormals.Add(i < normals.Length ? toSegmentNormals.MultiplyVector(normals[i]).normalized : Vector3.zero);
                sourceUv.Add(i < uv.Length ? uv[i] : Vector2.zero);
            }

            Material[] slots = renderer.sharedMaterials;
            bool mirrored = toSegment.determinant < 0f;
            for (int submesh = 0; submesh < source.subMeshCount; submesh++)
            {
                if (source.GetTopology(submesh) != MeshTopology.Triangles)
                    continue;

                Material material = submesh < slots.Length ? slots[submesh] : null;
                int slot = materials.IndexOf(material);
                if (slot < 0)
                {
                    slot = materials.Count;
                    materials.Add(material);
                    sourceTriangles.Add(new List<int>());
                }

                int[] indices = source.GetTriangles(submesh);
                for (int i = 0; i < indices.Length; i += 3)
                {
                    sourceTriangles[slot].Add(vertexStart + indices[i]);
                    sourceTriangles[slot].Add(vertexStart + indices[i + (mirrored ? 2 : 1)]);
                    sourceTriangles[slot].Add(vertexStart + indices[i + (mirrored ? 1 : 2)]);
                }
            }
        }

        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;
        foreach (Vector3 position in sourceVertices)
        {
            minZ = Mathf.Min(minZ, position.z);
            maxZ = Mathf.Max(maxZ, position.z);
        }

        float segmentLength = maxZ - minZ;
        if (!(segmentLength > 0.001f) || float.IsInfinity(segmentLength) || materials.Count == 0)
            return false;

        int segmentCount = Mathf.Max(spline.Closed ? 3 : 1, Mathf.RoundToInt(splineLength / segmentLength));
        int verticesPerSegment = sourceVertices.Count;
        var vertices = new Vector3[segmentCount * verticesPerSegment];
        var deformedNormals = new Vector3[vertices.Length];
        var uvs = new Vector2[vertices.Length];
        var triangles = new List<int>[materials.Count];
        for (int slot = 0; slot < triangles.Length; slot++)
            triangles[slot] = new List<int>();

        const float seamTolerance = 0.00001f;
        var alongSegment = new float[verticesPerSegment];
        for (int i = 0; i < verticesPerSegment; i++)
        {
            float along = (sourceVertices[i].z - minZ) / segmentLength;
            alongSegment[i] = along < seamTolerance ? 0f : along > 1f - seamTolerance ? 1f : along;
        }

        // 같은 길이 좌표를 가진 버텍스는 위치와 방향을 한 번만 평가합니다.
        var frames = new Dictionary<float, Matrix4x4>();
        for (int segment = 0; segment < segmentCount; segment++)
        {
            int vertexStart = segment * verticesPerSegment;
            for (int i = 0; i < verticesPerSegment; i++)
            {
                float t = (segment + alongSegment[i]) / segmentCount;
                if (spline.Closed && t == 1f)
                    t = 0f;

                if (!frames.TryGetValue(t, out Matrix4x4 frame))
                {
                    Vector3 center = (Vector3)spline.EvaluatePosition(t);
                    const float sampleOffset = 0.0001f;
                    float next = spline.Closed ? Mathf.Repeat(t + sampleOffset, 1f) : Mathf.Clamp01(t + sampleOffset);
                    float previous = spline.Closed ? Mathf.Repeat(t - sampleOffset, 1f) : Mathf.Clamp01(t - sampleOffset);
                    Vector3 forward = (Vector3)(spline.EvaluatePosition(next) - spline.EvaluatePosition(previous));
                    if (forward.sqrMagnitude < 0.00000001f)
                        forward = (Vector3)spline.EvaluateTangent(t);
                    if (forward.sqrMagnitude < 0.00000001f)
                        return false;

                    Vector3 up = (Vector3)spline.EvaluateUpVector(t);
                    if (Vector3.Cross(forward.normalized, up).sqrMagnitude < 0.000001f)
                        up = Mathf.Abs(forward.normalized.y) > 0.99f ? Vector3.forward : Vector3.up;
                    frame = Matrix4x4.TRS(center, Quaternion.LookRotation(forward, up), Vector3.one);
                    frames.Add(t, frame);
                }

                Vector3 position = sourceVertices[i];
                vertices[vertexStart + i] = frame.MultiplyPoint3x4(new Vector3(position.x + segmentOffset.x, position.y + segmentOffset.y, 0f));
                deformedNormals[vertexStart + i] = frame.MultiplyVector(sourceNormals[i]);
                uvs[vertexStart + i] = sourceUv[i];
            }

            // 맞닿은 모듈의 끝면은 제거하고 열린 트랙의 양 끝면만 남깁니다.
            bool omitStart = spline.Closed || segment > 0;
            bool omitEnd = spline.Closed || segment < segmentCount - 1;
            for (int slot = 0; slot < triangles.Length; slot++)
            {
                List<int> source = sourceTriangles[slot];
                for (int i = 0; i < source.Count; i += 3)
                {
                    int a = source[i];
                    int b = source[i + 1];
                    int c = source[i + 2];
                    float start = alongSegment[a];
                    if (start == alongSegment[b] && start == alongSegment[c]
                        && ((omitStart && start == 0f) || (omitEnd && start == 1f)))
                        continue;

                    triangles[slot].Add(vertexStart + a);
                    triangles[slot].Add(vertexStart + b);
                    triangles[slot].Add(vertexStart + c);
                }
            }
        }

        mesh = new Mesh { name = "Generated Track", hideFlags = HideFlags.DontSave };
        if (vertices.Length > 65535)
            mesh.indexFormat = IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.subMeshCount = triangles.Length;
        for (int slot = 0; slot < triangles.Length; slot++)
            mesh.SetTriangles(triangles[slot], slot, false);

        if (sourceNormals.Contains(Vector3.zero))
            mesh.RecalculateNormals();
        else
            mesh.normals = deformedNormals;

        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        materialSlots = materials.ToArray();
        return true;
    }

    private void ClearGeneratedMesh()
    {
        if (generatedMesh == null)
            return;

        if (meshFilter.sharedMesh == generatedMesh)
        {
            meshFilter.sharedMesh = previousVisualMesh;
            meshRenderer.sharedMaterials = previousMaterials;
        }
        if (meshCollider.sharedMesh == generatedMesh)
            meshCollider.sharedMesh = previousCollisionMesh;

        ReleaseMesh(generatedMesh);
        generatedMesh = null;
    }

    private static void ReleaseMesh(Mesh mesh)
    {
        if (mesh == null)
            return;

        if (Application.isPlaying)
            Destroy(mesh);
        else
            DestroyImmediate(mesh);
    }
}
