using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Small downward speed that keeps the controller pressed to the ground, so isGrounded stays reliable.
    const float GroundedVerticalVelocity = -2f;

    [Header("Speed (m/s)")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float turnSpeed = 720f;

    [Header("Jump and gravity")]
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float gravity = 20f;

    [Header("Crouch (controller height, m)")]
    [SerializeField] float standHeight = 2f;
    [SerializeField] float crouchHeight = 1f;

    [Header("References")]
    [SerializeField] Transform cameraTransform;

    CharacterController controller;
    InputAction moveAction;
    InputAction sprintAction;
    InputAction crouchAction;
    InputAction jumpAction;
    float verticalVelocity;
    bool isCrouching;

    void Awake()
    {
        if (cameraTransform == null) throw new MissingReferenceException("PlayerController: cameraTransform is not assigned.");

        controller = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
        sprintAction = InputSystem.actions.FindAction("Player/Sprint", throwIfNotFound: true);
        crouchAction = InputSystem.actions.FindAction("Player/Crouch", throwIfNotFound: true);
        jumpAction = InputSystem.actions.FindAction("Player/Jump", throwIfNotFound: true);
        SetHeight(standHeight);
    }

    void Update()
    {
        UpdateCrouch();
        Vector3 direction = GetMoveDirection();
        UpdateVerticalVelocity();

        Vector3 velocity = direction * GetSpeed() + Vector3.up * verticalVelocity;
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
        StopAtCeiling(flags);
        FaceDirection(direction);
    }

    public void Teleport(Vector3 position)
    {
        // An enabled CharacterController keeps its own physics position and would undo a plain transform change.
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
        verticalVelocity = 0f;
    }

    void UpdateCrouch()
    {
        bool wantsCrouch = crouchAction.IsPressed();
        if (wantsCrouch == isCrouching) return;
        if (!wantsCrouch && !HasHeadroom()) return;

        isCrouching = wantsCrouch;
        SetHeight(isCrouching ? crouchHeight : standHeight);
    }

    bool HasHeadroom()
    {
        // The cast starts inside our own capsule; SphereCast ignores colliders it overlaps at the start.
        Vector3 origin = transform.position + Vector3.up * (crouchHeight - controller.radius);
        float distance = standHeight - crouchHeight;
        return !Physics.SphereCast(origin, controller.radius, Vector3.up, out _, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    void SetHeight(float height)
    {
        controller.height = height;
        controller.center = Vector3.up * (height / 2f);
    }

    Vector3 GetMoveDirection()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Quaternion cameraYaw = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        return Vector3.ClampMagnitude(cameraYaw * new Vector3(input.x, 0f, input.y), 1f);
    }

    float GetSpeed()
    {
        if (isCrouching) return crouchSpeed;
        return sprintAction.IsPressed() ? runSpeed : walkSpeed;
    }

    void UpdateVerticalVelocity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = GroundedVerticalVelocity;

        if (controller.isGrounded && !isCrouching && jumpAction.WasPressedThisFrame())
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

        verticalVelocity -= gravity * Time.deltaTime;
    }

    void StopAtCeiling(CollisionFlags flags)
    {
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;
    }

    void FaceDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }
}
