using UnityEngine;

public class DelayH : MonoBehaviour
{
    public GameObject target;
    public Camera PlayerCamera;

    public float InitialFOV;
    public float FinalFOV;
    public float InitialDistance;
    public float FinalDistance;

    [Header("Feel / Damping")]
    [Tooltip("Lower = snappier, higher = heavier/laggier")]
    [SerializeField] private float fovSmoothTime = 0.35f;
    [SerializeField] private float distanceSmoothTime = 0.55f; // slightly different from FOV so they don't move in sync
    [SerializeField] private float maxFovSpeed = Mathf.Infinity;
    [SerializeField] private float maxDistanceSpeed = Mathf.Infinity;

    private Vector3 toTarget;
    private Vector3 playerForward;
    private Vector3 localPos;

    // targets we're smoothing toward
    private float targetFOV;
    private float targetDistance;

    // current smoothed values
    private float currentFOV;
    private float currentDistance;

    // velocity refs required by SmoothDamp
    private float fovVelocity;
    private float distanceVelocity;

    void Start()
    {
        // initialize so we don't snap/lerp-from-zero on the first frame
        currentFOV = PlayerCamera != null ? PlayerCamera.fieldOfView : InitialFOV;
        currentDistance = PlayerCamera != null ? PlayerCamera.transform.localPosition.z : InitialDistance;
        targetFOV = currentFOV;
        targetDistance = currentDistance;
    }

    void Update()
    {
        if (target == null || PlayerCamera == null) return;

        toTarget = (target.transform.position - transform.position).normalized;
        playerForward = transform.forward;

        // Dot product: 1 = facing target, -1 = facing away
        float dot = Vector3.Dot(playerForward, toTarget);

        // Remap dot from [-1, 1] to [0, 1]
        float t = (dot + 1f) / 2f;

        // These are the "ideal" instantaneous values based on look direction
        targetFOV = Mathf.Lerp(InitialFOV, FinalFOV, t);
        targetDistance = Mathf.Lerp(InitialDistance, FinalDistance, t);

        // Smoothly chase those targets instead of snapping to them —
        // different smooth times make FOV and distance drift out of sync,
        // giving a heavier, less mechanical feel
        currentFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref fovVelocity, fovSmoothTime, maxFovSpeed);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, distanceSmoothTime, maxDistanceSpeed);

        PlayerCamera.fieldOfView = currentFOV;

        localPos = PlayerCamera.transform.localPosition;
        localPos.z = currentDistance;
        PlayerCamera.transform.localPosition = localPos;
    }
}