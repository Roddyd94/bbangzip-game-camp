using UnityEngine;

public class BowlingPinSetter : MonoBehaviour
{
    [SerializeField]
    private Rigidbody[] pinBodies;
    [SerializeField]
    private Collider[] pinColliders;
    [SerializeField]
    private BowlingLane lane;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string runCycleTrigger;
    [SerializeField]
    private string runSweepTrigger;
    [SerializeField]
    private string resetTrigger;
    [SerializeField]
    private string parkedState;
    [SerializeField]
    private Transform deckLiftBone;

    private Transform[] pinParents;
    private Vector3[] startPositions;
    private Quaternion[] startRotations;
    private Vector3[] preservedPositions;
    private Quaternion[] preservedRotations;
    private BowlingRollDecision? pendingSetup;

    private ushort PreservedPinMask =>
        pendingSetup is { ShouldResetRack: false } decision
        ? decision.NextRollStandingPinMask
        : (ushort)0;

    private void Awake()
    {
        pinParents = new Transform[pinBodies.Length];
        startPositions = new Vector3[pinBodies.Length];
        startRotations = new Quaternion[pinBodies.Length];
        preservedPositions = new Vector3[pinBodies.Length];
        preservedRotations = new Quaternion[pinBodies.Length];

        for (int i = 0; i < pinBodies.Length; ++i)
        {
            pinParents[i] = pinBodies[i].transform.parent;
            startPositions[i] = pinBodies[i].position;
            startRotations[i] = pinBodies[i].rotation;
        }
    }

    private void OnEnable()
    {
        lane.PinSetupRequested += OnPinSetupRequested;
    }

    private void OnDisable()
    {
        lane.PinSetupRequested -= OnPinSetupRequested;
        pendingSetup = null;
    }

    public void ResetPins()
    {
        pendingSetup = null;
        animator.ResetTrigger(runCycleTrigger);
        animator.ResetTrigger(runSweepTrigger);
        animator.ResetTrigger(resetTrigger);
        animator.Play(parkedState, 0, 0f);
        animator.Update(0f);

        for (int i = 0; i < pinBodies.Length; ++i)
            RestorePin(i, startPositions[i], startRotations[i]);

        Physics.SyncTransforms();
    }

    private void RestorePin(int index, Vector3 position, Quaternion rotation)
    {
        Rigidbody pinBody = pinBodies[index];
        pinBody.transform.SetParent(pinParents[index], true);
        pinBody.gameObject.SetActive(true);
        pinBody.isKinematic = false;
        pinBody.linearVelocity = Vector3.zero;
        pinBody.angularVelocity = Vector3.zero;
        pinBody.position = position;
        pinBody.rotation = rotation;
        pinBody.transform.SetPositionAndRotation(position, rotation);
        pinBody.detectCollisions = true;
        pinColliders[index].enabled = true;
        pinBody.WakeUp();
    }

    private void OnPinSetupRequested(BowlingRollDecision decision)
    {
        pendingSetup = decision;
        ushort preservedPinMask = PreservedPinMask;
        for (int i = 0; i < pinBodies.Length; ++i)
        {
            if ((preservedPinMask & (1 << i)) == 0)
                continue;

            preservedPositions[i] = pinBodies[i].position;
            preservedRotations[i] = pinBodies[i].rotation;
        }

        animator.SetTrigger(preservedPinMask == 0 ? runSweepTrigger : runCycleTrigger);
    }

    private void GripStandingPins()
    {
        if (!pendingSetup.HasValue)
            return;

        ushort preservedPinMask = PreservedPinMask;
        for (int i = 0; i < pinBodies.Length; ++i)
        {
            if ((preservedPinMask & (1 << i)) == 0)
                continue;

            pinBodies[i].isKinematic = true;
            pinBodies[i].detectCollisions = false;
            pinBodies[i].transform.SetParent(deckLiftBone, true);
        }
    }

    private void ClearSweptPins()
    {
        if (!pendingSetup.HasValue)
            return;

        ushort preservedPinMask = PreservedPinMask;
        for (int i = 0; i < pinBodies.Length; ++i)
        {
            if ((preservedPinMask & (1 << i)) == 0)
                pinBodies[i].gameObject.SetActive(false);
        }
    }

    private void PlacePins()
    {
        if (pendingSetup is not { } decision)
            return;

        ushort preservedPinMask = PreservedPinMask;
        for (int i = 0; i < pinBodies.Length; ++i)
        {
            if (decision.ShouldResetRack)
                RestorePin(i, startPositions[i], startRotations[i]);
            else if ((preservedPinMask & (1 << i)) != 0)
                RestorePin(i, preservedPositions[i], preservedRotations[i]);
        }
        Physics.SyncTransforms();
    }

    private void FinishSweep()
    {
        ClearSweptPins();
        PlacePins();
        FinishPinSetup();
    }

    private void FinishPinSetup()
    {
        if (!pendingSetup.HasValue)
            return;

        pendingSetup = null;
        lane.CompletePinSetup();
    }
}
