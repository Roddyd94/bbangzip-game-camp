using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BowlingRequest
{
    Confirm,
    Back,
    Restart,
    ChangeCamera,
    SetStageValue
}

public sealed class BowlingInput : MonoBehaviour
{

    public event Action<BowlingRequest, float?> Requested;

    [SerializeField] private InputActionReference adjustReference;
    [SerializeField] private InputActionReference confirmReference;
    [SerializeField] private InputActionReference backReference;
    [SerializeField] private InputActionReference restartReference;
    [SerializeField] private InputActionReference cameraReference;
    private InputActionMap bowlingActions;
    private InputAction adjustAction;
    private InputAction confirmAction;
    private InputAction backAction;
    private InputAction restartAction;
    private InputAction cameraAction;

    public float Adjustment
    {
        get
        {
            Vector2 adjustment = adjustAction.ReadValue<Vector2>();
            return Mathf.Abs(adjustment.x) >= Mathf.Abs(adjustment.y) ? adjustment.x : adjustment.y;
        }
    }

    private void Awake()
    {
        adjustAction = adjustReference.action;
        confirmAction = confirmReference.action;
        backAction = backReference.action;
        restartAction = restartReference.action;
        cameraAction = cameraReference.action;
        bowlingActions = adjustAction.actionMap;
    }

    private void OnEnable()
    {
        confirmAction.performed += OnConfirm;
        backAction.performed += OnBack;
        restartAction.performed += OnRestart;
        cameraAction.performed += OnCamera;
        bowlingActions.Enable();
    }

    private void OnDisable()
    {
        bowlingActions.Disable();
        confirmAction.performed -= OnConfirm;
        backAction.performed -= OnBack;
        restartAction.performed -= OnRestart;
        cameraAction.performed -= OnCamera;
    }

    private void OnConfirm(InputAction.CallbackContext context) { Requested?.Invoke(BowlingRequest.Confirm, null); }
    private void OnBack(InputAction.CallbackContext context) { Requested?.Invoke(BowlingRequest.Back, null); }
    private void OnRestart(InputAction.CallbackContext context) { Requested?.Invoke(BowlingRequest.Restart, null); }
    private void OnCamera(InputAction.CallbackContext context) { Requested?.Invoke(BowlingRequest.ChangeCamera, null); }
}
