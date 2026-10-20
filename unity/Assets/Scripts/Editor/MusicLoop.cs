// A calm 30-second loop: soft chord pads, a bass note on beats 1 and 3, and a music-box
// arpeggio in the C major pentatonic. The buffer is rendered as a ring (see AudioSynth),
// so notes that ring past the end continue at the start and the loop has no seam.
public static class MusicLoop
{
    const float BeatsPerMinute = 96f;
    const int BeatsPerBar = 4;
    const int Bars = 12;
    const float Beat = 60f / BeatsPerMinute;
    const float Bar = Beat * BeatsPerBar;
    const float Eighth = Beat / 2f;
    const float C3 = 130.81f;
    const float Peak = 0.6f;

    // C, Am, F, G — chord tones as semitones from C3, one chord per bar.
    static readonly int[][] Chords =
    {
        new[] { 0, 4, 7 },
        new[] { -3, 0, 4 },
        new[] { -7, -3, 0 },
        new[] { -5, -1, 2 },
    };

    static readonly int[] Pentatonic = { 0, 2, 4, 7, 9 };

    // How likely an eighth note of the arpeggio sounds in each pass through the progression:
    // sparse at first, fuller, then in between.
    static readonly float[] ArpeggioDensity = { 0.45f, 0.8f, 0.6f };

    public static float[] Create()
    {
        float[] buffer = AudioSynth.CreateBuffer(Bar * Bars);
        var random = new System.Random(42);
        for (int bar = 0; bar < Bars; bar++)
        {
            int[] chord = Chords[bar % Chords.Length];
            float start = bar * Bar;
            AddPad(buffer, chord, start);
            AddBass(buffer, chord[0], start);
            AddArpeggio(buffer, chord, start, ArpeggioDensity[bar / Chords.Length], random);
        }
        AudioSynth.Normalize(buffer, Peak);
        return buffer;
    }

    // Each chord tone is doubled slightly out of tune for a warm, slowly beating pad.
    static void AddPad(float[] buffer, int[] chord, float start)
    {
        foreach (int semitone in chord)
        {
            float frequency = Note(semitone + 12);
            AudioSynth.AddTone(buffer, start, Bar, frequency, frequency, Waveform.Sine, 0.07f, 0.5f, 60f, 1f);
            AudioSynth.AddTone(buffer, start, Bar, frequency * 1.003f, frequency * 1.003f, Waveform.Sine, 0.05f, 0.5f, 60f, 1f);
        }
    }

    static void AddBass(float[] buffer, int root, float start)
    {
        // Octave 3 (roughly 87–130 Hz): low enough to carry the chord, high enough for laptop speakers.
        float frequency = Note(root);
        for (int beat = 0; beat < BeatsPerBar; beat += 2)
            AudioSynth.AddTone(buffer, start + beat * Beat, Beat * 1.8f, frequency, frequency, Waveform.Sine, 0.16f, 0.01f, 0.6f, 0.2f);
    }

    // The first eighth always plays the chord root; the others are random pentatonic notes two octaves up.
    static void AddArpeggio(float[] buffer, int[] chord, float start, float density, System.Random random)
    {
        int eighths = BeatsPerBar * 2;
        for (int step = 0; step < eighths; step++)
        {
            if (step > 0 && random.NextDouble() > density) continue;

            int semitone = step == 0 ? chord[0] + 24 : Pentatonic[random.Next(Pentatonic.Length)] + 24;
            float frequency = Note(semitone);
            AudioSynth.AddTone(buffer, start + step * Eighth, Eighth * 1.5f, frequency, frequency, Waveform.Bell, 0.12f, 0.005f, 0.35f, 0.15f);
        }
    }

    static float Note(int semitonesFromC3)
    {
        return C3 * UnityEngine.Mathf.Pow(2f, semitonesFromC3 / 12f);
    }
}
