using System;
using UnityEngine;

// Procedural animation of the low-poly character. It only reads the CharacterController
// (velocity, isGrounded, height), so the movement code knows nothing about visuals.
[RequireComponent(typeof(CharacterController))]
public class CharacterAnimator : MonoBehaviour
{
    // Short ungrounded moments, like walking down stairs, should not switch to the air pose.
    const float AirborneDelay = 0.1f;
    const float MinAirTimeForLanding = 0.25f;
    const float JumpVelocityThreshold = 1f;
    const float MinWalkSpeed = 0.2f;
    const float LandingDuration = 0.25f;
    const float FullCycle = Mathf.PI * 2f;

    [Header("Rig")]
    [SerializeField] Transform hips;
    [SerializeField] Transform torso;
    [SerializeField] Transform head;
    [SerializeField] Transform thighLeft;
    [SerializeField] Transform thighRight;
    [SerializeField] Transform shinLeft;
    [SerializeField] Transform shinRight;
    [SerializeField] Transform armLeft;
    [SerializeField] Transform armRight;
    [SerializeField] Transform scarfTail;

    [Header("Proportions (m)")]
    [SerializeField] float legLength = 0.8f;
    [SerializeField] float crouchHipHeight = 0.3f;

    [Header("Motion")]
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float strideLength = 1.6f;
    [SerializeField] float legSwing = 32f;
    [SerializeField] float kneeBend = 55f;
    [SerializeField] float armSwing = 45f;
    [SerializeField] float runLean = 14f;
    [SerializeField] float crouchLean = 35f;
    [SerializeField] float landingDip = 0.15f;
    [SerializeField] float smoothing = 12f;

    public event Action Stepped;
    public event Action Jumped;
    public event Action Landed;

    CharacterController controller;
    float standHeight;
    float phase;
    float speedBlend;
    float crouchBlend;
    float airBlend;
    float airTime;
    float landingTimer;
    bool wasGrounded = true;

    void Awake()
    {
        if (hips == null || torso == null || head == null) throw new MissingReferenceException("CharacterAnimator: body rig is not assigned.");
        if (thighLeft == null || thighRight == null || shinLeft == null || shinRight == null) throw new MissingReferenceException("CharacterAnimator: leg rig is not assigned.");
        if (armLeft == null || armRight == null || scarfTail == null) throw new MissingReferenceException("CharacterAnimator: arm or scarf rig is not assigned.");

        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        // PlayerController sets the standing height in its Awake.
        standHeight = controller.height;
    }

    // LateUpdate reads the velocity of this frame's Move from PlayerController.Update.
    void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        Vector3 velocity = controller.velocity;
        float speed = new Vector2(velocity.x, velocity.z).magnitude;
        bool grounded = UpdateGrounded(velocity.y, deltaTime);

        float blend = 1f - Mathf.Exp(-smoothing * deltaTime);
        speedBlend = Mathf.Lerp(speedBlend, grounded ? Mathf.Clamp01(speed / runSpeed) : 0f, blend);
        crouchBlend = Mathf.Lerp(crouchBlend, controller.height < standHeight ? 1f : 0f, blend);
        airBlend = Mathf.Lerp(airBlend, grounded ? 0f : 1f, blend);
        landingTimer = Mathf.Max(0f, landingTimer - deltaTime);
        if (grounded && speed > MinWalkSpeed) AdvancePhase(speed * deltaTime);

        ApplyPose();
    }

    // Returns whether the character counts as grounded and raises Jumped and Landed.
    bool UpdateGrounded(float verticalSpeed, float deltaTime)
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            if (!wasGrounded && airTime >= MinAirTimeForLanding) Land();
            airTime = 0f;
        }
        else
        {
            if (wasGrounded && verticalSpeed > JumpVelocityThreshold) Jumped?.Invoke();
            airTime += deltaTime;
        }
        wasGrounded = isGrounded;
        return isGrounded || airTime < AirborneDelay;
    }

    void Land()
    {
        landingTimer = LandingDuration;
        Landed?.Invoke();
    }

    // One full cycle covers two steps; a foot lands at each swing extreme (phase π/2 and 3π/2).
    void AdvancePhase(float distance)
    {
        float before = phase;
        float after = phase + distance / strideLength * FullCycle;
        if (Crossed(before, after, 0.5f) || Crossed(before, after, 1.5f) || Crossed(before, after, 2.5f)) Stepped?.Invoke();
        phase = Mathf.Repeat(after, FullCycle);
    }

    static bool Crossed(float before, float after, float halfTurns)
    {
        float mark = halfTurns * Mathf.PI;
        return before < mark && after >= mark;
    }

    void ApplyPose()
    {
        float swing = Mathf.Sin(phase);
        float landing = Mathf.Sin(Mathf.PI * landingTimer / LandingDuration) * landingDip;
        float baseHipHeight = Mathf.Lerp(legLength, crouchHipHeight, crouchBlend) - landing;

        // Two equal leg segments: bending the thigh forward by θ and the knee back by 2θ keeps the foot under the hips.
        float bend = Mathf.Acos(Mathf.Clamp01(baseHipHeight / legLength)) * Mathf.Rad2Deg;
        float leftReach = PoseLeg(thighLeft, shinLeft, bend, swing, 30f);
        float rightReach = PoseLeg(thighRight, shinRight, bend, -swing, -10f);

        // On the ground the hips sit as high as the longer leg reaches, so the planted foot touches the floor
        // and the body bobs by itself during a stride. In the air the legs just hang.
        float hipHeight = Mathf.Lerp(Mathf.Max(leftReach, rightReach), baseHipHeight, airBlend);
        hips.localPosition = new Vector3(0f, hipHeight, 0f);

        float armRaise = Mathf.Lerp(8f, 70f, airBlend);
        float armForward = crouchBlend * 30f;
        PoseLimb(armLeft, -swing * armSwing * speedBlend + armForward, -armRaise);
        PoseLimb(armRight, swing * armSwing * speedBlend + armForward, armRaise);

        float lean = runLean * speedBlend + crouchLean * crouchBlend;
        float twist = swing * 6f * speedBlend;
        torso.localRotation = Quaternion.Euler(lean, twist, 0f);
        head.localRotation = Quaternion.Euler(-lean * 0.3f, -twist, 0f);

        float scarfAngle = 10f + speedBlend * 55f + airBlend * 35f + Mathf.Sin(phase * 2f) * 6f * speedBlend;
        scarfTail.localRotation = Quaternion.Euler(scarfAngle, 0f, 0f);
    }

    // swing > 0 moves this leg forward; the leg that swings back lifts its foot.
    // In the air one leg goes forward and the other back (airForward).
    // Returns how far below the hips the foot ends up.
    float PoseLeg(Transform thigh, Transform shin, float bend, float swing, float airForward)
    {
        float walkForward = swing * legSwing * speedBlend;
        float walkKnee = Mathf.Max(0f, -swing) * kneeBend * speedBlend;

        float forward = Mathf.Lerp(bend + walkForward, airForward, airBlend);
        float knee = Mathf.Lerp(bend * 2f + walkKnee, 45f, airBlend);
        PoseLimb(thigh, forward, 0f);
        shin.localRotation = Quaternion.Euler(knee, 0f, 0f);

        float segment = legLength / 2f;
        return segment * Mathf.Cos(forward * Mathf.Deg2Rad) + segment * Mathf.Cos((forward - knee) * Mathf.Deg2Rad);
    }

    // Limbs hang down from their pivot: a positive forward angle swings the end towards +Z,
    // a positive outward angle swings it towards +X.
    static void PoseLimb(Transform limb, float forward, float outward)
    {
        limb.localRotation = Quaternion.Euler(-forward, 0f, outward);
    }
}
