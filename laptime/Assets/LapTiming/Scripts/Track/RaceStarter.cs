using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public sealed class RaceStarter : MonoBehaviour
{
    [SerializeField] private Rigidbody playerKart;
    [SerializeField] private KartCommandDriver aiKartPrefab;
    [SerializeField] private SplineContainer track;
    [SerializeField] private BehaviorGraph aiBehavior;
    [SerializeField] private RaceCountdown countdown;

    private KartCommandDriver playerDriver;
    private KartCommandDriver aiDriver;
    private KartAiDriveSource aiSource;
    private BehaviorGraphAgent aiBehaviorAgent;
    private bool replayPending;
    private NavMeshData navigationData;
    private NavMeshDataInstance navigationInstance;

    private void Awake()
    {
        playerDriver = playerKart.GetComponent<KartCommandDriver>();
    }

    private IEnumerator Start()
    {
        playerDriver.SetPlayerInputEnabled(false);
        Vector3 gridOffset = playerKart.rotation * Vector3.right * 2.2f;
        aiDriver = Instantiate(aiKartPrefab, playerKart.position + gridOffset, playerKart.rotation);
        aiDriver.name = "AI Kart";
        aiDriver.SetPlayerInputEnabled(false);
        foreach (Camera kartCamera in aiDriver.GetComponentsInChildren<Camera>())
        {
            kartCamera.enabled = false;
            kartCamera.tag = "Untagged";
            if (kartCamera.TryGetComponent(out AudioListener listener))
                listener.enabled = false;
        }

        aiSource = aiDriver.gameObject.AddComponent<KartAiDriveSource>();
        aiSource.Configure(track);

        playerDriver.UseExternalTick();
        aiDriver.UseExternalTick();
        playerDriver.BeginRecording();
        aiDriver.BeginRecording();

        // The spline renderer creates its mesh at runtime.
        yield return null;
        BuildRoadNavigation();
        yield return countdown.Play(ReleaseKarts);
    }

    private void ReleaseKarts()
    {
        aiSource.ResetProgress();
        aiDriver.UseDriveSource(aiSource);
        if (aiBehavior != null)
        {
            aiBehaviorAgent = aiDriver.gameObject.AddComponent<BehaviorGraphAgent>();
            aiBehaviorAgent.Graph = aiBehavior;
        }

        playerDriver.SetPlayerInputEnabled(true);
    }

    private void FixedUpdate()
    {
        if (aiDriver == null)
            return;

        if (replayPending)
        {
            // Restore both bodies before either kart performs its first suspension raycasts.
            playerDriver.PrepareTick();
            aiDriver.PrepareTick();
            Physics.SyncTransforms();
            replayPending = false;
        }

        float step = Time.fixedDeltaTime;
        playerDriver.ExecuteTick(step);
        aiDriver.ExecuteTick(step);
    }

    internal bool PlayRaceReplay()
    {
        playerDriver.SetPlayerInputEnabled(false);
        if (aiDriver == null)
            return false;

        KartReplay playerReplay = playerDriver.EndRecording();
        KartReplay aiReplay = aiDriver.EndRecording();
        if (playerReplay == null || aiReplay == null || playerReplay.commands.Count == 0
            || playerReplay.commands.Count != aiReplay.commands.Count
            || !Mathf.Approximately(playerReplay.fixedStep, Time.fixedDeltaTime)
            || !Mathf.Approximately(aiReplay.fixedStep, Time.fixedDeltaTime))
            return false;

        if (aiBehaviorAgent != null)
            aiBehaviorAgent.enabled = false;

        aiDriver.SetPlayerInputEnabled(false);
        bool playerStarted = playerDriver.PlayReplay(playerReplay);
        bool aiStarted = aiDriver.PlayReplay(aiReplay);
        replayPending = playerStarted && aiStarted;
        return replayPending;
    }

    private void BuildRoadNavigation()
    {
        if (!track.TryGetComponent(out MeshFilter road) || NavMesh.GetSettingsCount() == 0)
            return;

        if (road.sharedMesh == null && track.TryGetComponent(out SplineBuilder builder))
            builder.RebuildTrack();
        if (road.sharedMesh == null)
            return;

        var sources = new List<NavMeshBuildSource>
        {
            new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = road.sharedMesh,
                transform = road.transform.localToWorldMatrix,
                area = 0
            }
        };
        Bounds bounds = track.GetComponent<MeshRenderer>().bounds;
        bounds.Expand(4f);
        navigationData = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), sources, bounds,
            Vector3.zero, Quaternion.identity);
        if (navigationData != null)
            navigationInstance = NavMesh.AddNavMeshData(navigationData);
    }

    private void OnDestroy()
    {
        if (navigationInstance.valid)
            navigationInstance.Remove();
        if (navigationData != null)
            Destroy(navigationData);
    }
}
