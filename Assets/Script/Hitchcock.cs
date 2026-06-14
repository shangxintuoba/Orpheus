using UnityEngine;

public class Hitchcock : MonoBehaviour
{
    public GameObject target;
    public Camera PlayerCamera;
    public float InitialFOV;
    public float FinalFOV;
    public float InitialDistance;
    public float FinalDistance;

    private Vector3 toTarget;
    private Vector3 playerForward;
    private Vector3 localPos;

    void Start()
    {

    }

    void Update()
    {
        if (target == null || PlayerCamera == null) return;
        toTarget = (target.transform.position - transform.position).normalized;
        playerForward = transform.forward;

        // Dot product: 1 = facing target, -1 = facing away
        float dot = Vector3.Dot(playerForward, toTarget);

        // Remap dot from [-1, 1] to [0, 1] for lerp
        float t = (dot + 1f) / 2f;

        // Lerp FOV and camera local Z position
        PlayerCamera.fieldOfView = Mathf.Lerp(InitialFOV, FinalFOV, t);

        localPos = PlayerCamera.transform.localPosition;
        localPos.z = Mathf.Lerp(InitialDistance, FinalDistance, t);
        PlayerCamera.transform.localPosition = localPos;
    }
}