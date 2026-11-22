using UnityEngine;

[RequireComponent(typeof(KartMotor), typeof(Rigidbody))]
public sealed class KartAudio : MonoBehaviour
{
    [SerializeField] private AudioClip engineLoop;
    [SerializeField] private AudioClip surfaceLoop;
    [SerializeField] private AudioClip impactClip;

    private Rigidbody body;
    private KartMotor motor;
    private AudioSource engineSource;
    private AudioSource surfaceSource;
    private AudioSource impactSource;
    private float nextImpactAt;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        motor = GetComponent<KartMotor>();
        engineSource = CreateSource(engineLoop, true);
        engineSource.volume = 0.16f;
        surfaceSource = CreateSource(surfaceLoop, true);
        surfaceSource.volume = 0f;
        impactSource = CreateSource(null, false);
    }

    private void OnEnable()
    {
        engineSource.Play();
        surfaceSource.Play();
    }

    private void OnDisable()
    {
        engineSource.Stop();
        surfaceSource.Stop();
        impactSource.Stop();
    }

    private void Update()
    {
        float speed = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
        float speedRatio = Mathf.Clamp01(speed / 18f);
        float load = Mathf.Abs(motor.ThrottleInput);
        float blend = 1f - Mathf.Exp(-9f * Time.deltaTime);
        engineSource.pitch = Mathf.Lerp(engineSource.pitch, 0.82f + 1.12f * speedRatio + 0.2f * load, blend);
        engineSource.volume = Mathf.Lerp(engineSource.volume, 0.16f + 0.12f * load + 0.1f * speedRatio, blend);

        float slip = Mathf.InverseLerp(0.15f, 5f, motor.TireSlipSpeed);
        float roadVolume = motor.HasWheelContact ? 0.14f * speedRatio + 0.36f * slip : 0f;
        surfaceSource.volume = Mathf.Lerp(surfaceSource.volume, roadVolume, blend);
        surfaceSource.pitch = 0.85f + 0.5f * speedRatio;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (impactClip == null || Time.time < nextImpactAt || collision.contactCount == 0)
            return;

        float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.GetContact(0).normal));
        if (speed < 1.2f)
            return;

        nextImpactAt = Time.time + 0.14f;
        impactSource.PlayOneShot(impactClip, Mathf.Lerp(0.2f, 0.9f, Mathf.InverseLerp(1.2f, 9f, speed)));
    }

    private AudioSource CreateSource(AudioClip clip, bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0.25f;
        source.minDistance = 5f;
        source.maxDistance = 45f;
        return source;
    }
}
