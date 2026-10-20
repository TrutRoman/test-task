using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float distance = 5f;
    [SerializeField] float pivotHeight = 1.6f;
    [SerializeField] float minPitch = -20f;
    [SerializeField] float maxPitch = 70f;
    [SerializeField] float collisionRadius = 0.2f;

    InputAction lookAction;
    float sensitivity;
    float yaw;
    float pitch = 15f;

    void Awake()
    {
        if (target == null) throw new MissingReferenceException("ThirdPersonCamera: target is not assigned.");

        lookAction = InputSystem.actions.FindAction("Player/Look", throwIfNotFound: true);
        sensitivity = GameSettings.Sensitivity;
        yaw = target.eulerAngles.y;
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
    }

    void LateUpdate()
    {
        // Mouse delta is already per frame, so it is not scaled by deltaTime.
        Vector2 look = lookAction.ReadValue<Vector2>() * sensitivity;
        yaw += look.x;
        pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Vector3 back = rotation * Vector3.back;
        transform.SetPositionAndRotation(pivot + back * GetUnblockedDistance(pivot, back), rotation);
    }

    float GetUnblockedDistance(Vector3 pivot, Vector3 direction)
    {
        bool blocked = Physics.SphereCast(pivot, collisionRadius, direction, out RaycastHit hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        return blocked ? hit.distance : distance;
    }
}
