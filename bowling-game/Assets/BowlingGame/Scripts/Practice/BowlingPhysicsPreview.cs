using UnityEngine;
using UnityEngine.InputSystem;

// 물리 동작 확인용입니다. 점수·턴 규칙과 분리해서 사용합니다.
public sealed class BowlingPhysicsPreview : MonoBehaviour
{
    [SerializeField] private InputActionReference resetReference;
    [SerializeField] private InputActionReference launchReference;
    [SerializeField] private InputActionReference aimReference;
    [SerializeField] private BowlingUiText uiText;
    [SerializeField] private PhysicsMaterial ballMaterial;
    [SerializeField] private PhysicsMaterial pinMaterial;
    [SerializeField] private PhysicsMaterial laneMaterial;
    [SerializeField] private Rigidbody ball;
    [SerializeField] private Rigidbody[] pins;
    [SerializeField] private Camera cameraController;
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float aimOffset = 0.025f;
    [SerializeField] private bool autoReplay = true;

    private Vector3[] pinPositions;
    private Quaternion[] pinRotations;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Vector3 pinCenter;
    private InputActionMap previewActions;
    private InputAction resetAction;
    private InputAction launchAction;
    private InputAction aimAction;
    private float aimInput;
    private float elapsed;
    private bool launched;
    private bool launchPending;


    public int FallenPins
    {
        get
        {
            int count = 0;

            foreach (var pin in pins)
            {
                if (Vector3.Angle(pin.rotation * Quaternion.Euler(90f, 0f, 0f) * Vector3.up, Vector3.up) > 25f || pin.position.y < -0.1f)
                    ++count;
            }

            return count;
        }
    }

    private void Awake()
    {
        resetAction = resetReference.action;
        launchAction = launchReference.action;
        aimAction = aimReference.action;
        previewActions = resetAction.actionMap;
    }

    private void OnEnable()
    {
        resetAction.performed += OnReset;
        launchAction.performed += OnLaunch;
        aimAction.performed += OnAim;
        aimAction.canceled += OnAim;
        previewActions.Enable();
    }

    private void OnDisable()
    {
        previewActions.Disable();
        resetAction.performed -= OnReset;
        launchAction.performed -= OnLaunch;
        aimAction.performed -= OnAim;
        aimAction.canceled -= OnAim;
        aimInput = 0f;
    }

    private void OnReset(InputAction.CallbackContext context) => ResetSimulation();
    private void OnLaunch(InputAction.CallbackContext context) => Launch();
    private void OnAim(InputAction.CallbackContext context) => aimInput = context.ReadValue<float>();

    private void Start()
    {
        // 함께 열려 있는 작업 씬은 Play 중에만 숨깁니다. 저장된 씬에는 영향을 주지 않습니다.
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene != gameObject.scene)
            {
                foreach (var root in scene.GetRootGameObjects())
                    root.SetActive(false);
            }
        }

        pinPositions = new Vector3[pins.Length];
        pinRotations = new Quaternion[pins.Length];
        spawnPosition = ball.position;
        spawnRotation = ball.rotation;

        for (int i = 0; i < pins.Length; ++i)
        {
            pinPositions[i] = pins[i].position;
            pinRotations[i] = pins[i].rotation;
            pinCenter += pinPositions[i];
        }

        pinCenter /= pins.Length;

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.contactOffset = 0.001f;
            collider.sharedMaterial = collider.attachedRigidbody == ball ? ballMaterial : collider.attachedRigidbody != null ? pinMaterial : laneMaterial;
        }

        foreach (var body in GetComponentsInChildren<Rigidbody>())
        {
            body.solverIterations = 12;
            body.solverVelocityIterations = 4;
        }

        ResetSimulation();
    }

    public void ResetSimulation()
    {
        if (pinPositions == null)
            return;

        ResetBody(ball, spawnPosition, spawnRotation);

        for (int i = 0; i < pins.Length; ++i)
            ResetBody(pins[i], pinPositions[i], pinRotations[i]);

        Physics.SyncTransforms();
        elapsed = 0f;
        launched = false;
        launchPending = false;
        UpdateCamera();
    }

    private static void ResetBody(Rigidbody body, Vector3 position, Quaternion rotation)
    {
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = position;
        body.rotation = rotation;
        body.WakeUp();
    }

    public void Launch()
    {
        if (!launched)
            launchPending = true;
    }

    private void FixedUpdate()
    {
        if (!launchPending)
            return;

        launchPending = false;
        launched = true;
        Vector3 target = pinPositions[0] + Vector3.right * aimOffset;
        Vector3 direction = target - ball.position;
        direction.y = 0f;
        direction.Normalize();
        ball.linearVelocity = direction * launchSpeed;
        ball.angularVelocity = Vector3.Cross(Vector3.up, direction) * (launchSpeed / 0.109f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        aimOffset = Mathf.Clamp(aimOffset + aimInput * Time.deltaTime * 0.2f, -0.45f, 0.45f);

        if (autoReplay && !launched && elapsed >= 1.5f)
            Launch();

        if (autoReplay && elapsed >= 10f)
            ResetSimulation();
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        float z = Mathf.Max(ball.position.z + 2.8f, pinCenter.z + 3.6f);
        cameraController.transform.position = new Vector3(pinCenter.x + 0.8f, 1.75f, z);
        cameraController.transform.LookAt(new Vector3(pinCenter.x, 0.18f, Mathf.Max(ball.position.z - 1.2f, pinCenter.z)));
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(16, 16, 410, 150), uiText.physicsTitle);
        GUI.Label(new Rect(30, 44, 390, 24), uiText.physicsControls);
        GUI.Label(new Rect(30, 70, 390, 24), string.Format(uiText.physicsStatusFormat, FallenPins, pins.Length, aimOffset));
        autoReplay = GUI.Toggle(new Rect(30, 98, 160, 24), autoReplay, uiText.physicsAutoReplay);
        
        if (GUI.Button(new Rect(200, 98, 95, 28), uiText.physicsLaunch))
            Launch();

        if (GUI.Button(new Rect(305, 98, 95, 28), uiText.physicsReset))
            ResetSimulation();
    }
}

