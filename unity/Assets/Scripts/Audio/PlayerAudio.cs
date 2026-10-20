using UnityEngine;

// Footsteps, jump and landing sounds. They follow CharacterAnimator events,
// so every step sounds exactly when a foot lands.
[RequireComponent(typeof(CharacterAnimator), typeof(CharacterController), typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    // Above this horizontal speed (m/s) the run step sound is used: walking is 4, running 7.
    [SerializeField] float runStepSpeed = 5.5f;

    [Header("Clips")]
    [SerializeField] AudioClip walkStep;
    [SerializeField] AudioClip runStep;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip land;

    CharacterAnimator characterAnimator;
    CharacterController controller;
    AudioSource source;

    void Awake()
    {
        if (walkStep == null || runStep == null || jump == null || land == null) throw new MissingReferenceException("PlayerAudio: a clip is not assigned.");

        characterAnimator = GetComponent<CharacterAnimator>();
        controller = GetComponent<CharacterController>();
        source = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        characterAnimator.Stepped += PlayStep;
        characterAnimator.Jumped += PlayJump;
        characterAnimator.Landed += PlayLand;
    }

    void OnDisable()
    {
        characterAnimator.Stepped -= PlayStep;
        characterAnimator.Jumped -= PlayJump;
        characterAnimator.Landed -= PlayLand;
    }

    void PlayStep()
    {
        Vector3 velocity = controller.velocity;
        float speed = new Vector2(velocity.x, velocity.z).magnitude;
        AudioClip clip = speed > runStepSpeed ? runStep : walkStep;

        source.pitch = 1f + Random.Range(-SoundLevels.FootstepPitchVariation, SoundLevels.FootstepPitchVariation);
        source.PlayOneShot(clip, SoundLevels.Footsteps);
    }

    void PlayJump()
    {
        source.pitch = 1f;
        source.PlayOneShot(jump, SoundLevels.Jump);
    }

    void PlayLand()
    {
        source.pitch = 1f;
        source.PlayOneShot(land, SoundLevels.Land);
    }
}
