// All mixing levels in one place (0..1). The volume slider scales everything on top of them
// through AudioListener.volume. Music stays well below the effects.
public static class SoundLevels
{
    public const float Music = 0.2f;
    public const float Ui = 0.55f;
    // Save, load, pause and resume.
    public const float Feedback = 0.6f;
    public const float Footsteps = 0.35f;
    public const float Jump = 0.5f;
    public const float Land = 0.55f;

    // Each step is played up to ±6 % higher or lower, so repeated steps do not sound identical.
    public const float FootstepPitchVariation = 0.06f;
}
