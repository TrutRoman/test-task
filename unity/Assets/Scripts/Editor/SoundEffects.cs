// Recipes for the one-shot sound effects. Each one is normalized to its own peak,
// which sets how strong it is relative to the others; SoundLevels mixes them in the game.
public static class SoundEffects
{
    // Note frequencies (Hz).
    const float C5 = 523.25f, E5 = 659.25f, G5 = 783.99f, A5 = 880f, B5 = 987.77f, C6 = 1046.5f;

    // A very short, soft high tick.
    public static float[] Hover()
    {
        float[] buffer = AudioSynth.CreateBuffer(0.08f);
        AudioSynth.AddTone(buffer, 0f, 0.02f, 1320f, 1480f, Waveform.Sine, 1f, 0.002f, 0.015f, 0.04f);
        return Finish(buffer, 0.4f);
    }

    // A rounded pluck with a quiet octave on top.
    public static float[] Click()
    {
        float[] buffer = AudioSynth.CreateBuffer(0.12f);
        AudioSynth.AddTone(buffer, 0f, 0.03f, 520f, 470f, Waveform.Triangle, 1f, 0.001f, 0.025f, 0.05f);
        AudioSynth.AddTone(buffer, 0f, 0.03f, 1040f, 940f, Waveform.Sine, 0.4f, 0.001f, 0.02f, 0.05f);
        return Finish(buffer, 0.8f);
    }

    // Soft, dull noise with a low thump.
    public static float[] WalkStep()
    {
        float[] buffer = AudioSynth.CreateBuffer(0.16f);
        AudioSynth.AddNoise(buffer, 0f, 0.03f, 700f, 1f, 0.002f, 0.04f, 0.06f, 11);
        AudioSynth.AddTone(buffer, 0f, 0.03f, 120f, 70f, Waveform.Sine, 0.8f, 0.002f, 0.03f, 0.05f);
        return Finish(buffer, 0.8f);
    }

    // Shorter and brighter than a walking step.
    public static float[] RunStep()
    {
        float[] buffer = AudioSynth.CreateBuffer(0.13f);
        AudioSynth.AddNoise(buffer, 0f, 0.025f, 1600f, 1f, 0.001f, 0.025f, 0.05f, 12);
        AudioSynth.AddTone(buffer, 0f, 0.025f, 150f, 90f, Waveform.Sine, 0.7f, 0.001f, 0.02f, 0.04f);
        return Finish(buffer, 0.85f);
    }

    // A rising sweep with a little air.
    public static float[] Jump()
    {
        float[] buffer = AudioSynth.CreateBuffer(0.32f);
        AudioSynth.AddTone(buffer, 0f, 0.16f, 280f, 640f, Waveform.Triangle, 0.7f, 0.005f, 0.12f, 0.08f);
        AudioSynth.AddNoise(buffer, 0f, 0.1f, 2500f, 0.25f, 0.01f, 0.06f, 0.05f, 13);
        return Finish(buffer, 0.7f);
    }

    // A falling low thump with a puff of dull noise.
    public static float[] Land()
    {
        float[] buffer = AudioSynth.CreateBuffer(0.32f);
        AudioSynth.AddTone(buffer, 0f, 0.08f, 95f, 50f, Waveform.Sine, 1f, 0.002f, 0.07f, 0.12f);
        AudioSynth.AddNoise(buffer, 0f, 0.06f, 500f, 0.8f, 0.002f, 0.05f, 0.1f, 14);
        return Finish(buffer, 0.9f);
    }

    // Two rising bell notes.
    public static float[] Save()
    {
        float[] buffer = AudioSynth.CreateBuffer(1f);
        Bell(buffer, 0f, E5);
        Bell(buffer, 0.1f, B5);
        return Finish(buffer, 0.7f);
    }

    // A rising major arpeggio.
    public static float[] Load()
    {
        float[] buffer = AudioSynth.CreateBuffer(1.2f);
        Bell(buffer, 0f, C5);
        Bell(buffer, 0.07f, E5);
        Bell(buffer, 0.14f, G5);
        Bell(buffer, 0.21f, C6);
        return Finish(buffer, 0.7f);
    }

    public static float[] Pause()
    {
        return TwoNotes(A5, E5);
    }

    public static float[] Resume()
    {
        return TwoNotes(E5, A5);
    }

    static float[] TwoNotes(float first, float second)
    {
        float[] buffer = AudioSynth.CreateBuffer(0.55f);
        AudioSynth.AddTone(buffer, 0f, 0.04f, first, first, Waveform.Sine, 1f, 0.004f, 0.15f, 0.2f);
        AudioSynth.AddTone(buffer, 0.11f, 0.04f, second, second, Waveform.Sine, 1f, 0.004f, 0.15f, 0.2f);
        return Finish(buffer, 0.6f);
    }

    static void Bell(float[] buffer, float start, float frequency)
    {
        AudioSynth.AddTone(buffer, start, 0.05f, frequency, frequency, Waveform.Bell, 1f, 0.003f, 0.35f, 0.4f);
    }

    static float[] Finish(float[] buffer, float peak)
    {
        AudioSynth.Normalize(buffer, peak);
        return buffer;
    }
}
