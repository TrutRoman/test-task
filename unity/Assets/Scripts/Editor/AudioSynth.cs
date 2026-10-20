using System;
using System.IO;
using System.Text;
using UnityEngine;

public enum Waveform { Sine, Triangle, Bell }

// A tiny synthesizer working on mono float buffers.
// Writes wrap around the end of a buffer: that is how the music loop stays seamless.
// One-shot sounds use buffers long enough that nothing wraps.
public static class AudioSynth
{
    public const int SampleRate = 44100;

    public static float[] CreateBuffer(float seconds)
    {
        return new float[Mathf.RoundToInt(seconds * SampleRate)];
    }

    // A note with an exponential pitch glide from frequency to glideTo over its length.
    // Envelope: linear attack, exponential decay (time constant), linear release after the length.
    public static void AddTone(float[] buffer, float start, float length, float frequency, float glideTo, Waveform wave,
        float amplitude, float attack, float decay, float release)
    {
        RequireEnvelope(attack, decay, release);

        int first = Mathf.RoundToInt(start * SampleRate);
        int count = Mathf.RoundToInt((length + release) * SampleRate);
        double phase = 0.0;
        for (int i = 0; i < count; i++)
        {
            float time = i / (float)SampleRate;
            float pitch = frequency * Mathf.Pow(glideTo / frequency, Mathf.Clamp01(time / length));
            phase += pitch / SampleRate;
            float sample = Oscillate(wave, phase) * Envelope(time, length, attack, decay, release);
            buffer[(first + i) % buffer.Length] += sample * amplitude;
        }
    }

    // White noise through a one-pole low-pass filter: a lower cutoff sounds softer and duller.
    public static void AddNoise(float[] buffer, float start, float length, float cutoff, float amplitude,
        float attack, float decay, float release, int seed)
    {
        RequireEnvelope(attack, decay, release);

        var random = new System.Random(seed);
        float smoothing = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate);
        float filtered = 0f;
        int first = Mathf.RoundToInt(start * SampleRate);
        int count = Mathf.RoundToInt((length + release) * SampleRate);
        for (int i = 0; i < count; i++)
        {
            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            filtered += smoothing * (white - filtered);
            float time = i / (float)SampleRate;
            buffer[(first + i) % buffer.Length] += filtered * Envelope(time, length, attack, decay, release) * amplitude;
        }
    }

    // Scales the buffer so its loudest sample reaches peak.
    public static void Normalize(float[] buffer, float peak)
    {
        float loudest = 0f;
        foreach (float sample in buffer) loudest = Mathf.Max(loudest, Mathf.Abs(sample));
        if (loudest == 0f) throw new InvalidOperationException("Cannot normalize a silent buffer.");

        float gain = peak / loudest;
        for (int i = 0; i < buffer.Length; i++) buffer[i] *= gain;
    }

    // 16-bit PCM, mono.
    public static byte[] EncodeWav(float[] samples)
    {
        const int bytesPerSample = 2;
        int dataSize = samples.Length * bytesPerSample;

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * bytesPerSample);
        writer.Write((short)bytesPerSample);
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);
        foreach (float sample in samples) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
        writer.Flush();
        return stream.ToArray();
    }

    static float Oscillate(Waveform wave, double phase)
    {
        float cycle = (float)(phase - Math.Floor(phase));
        float angle = cycle * Mathf.PI * 2f;
        return wave switch
        {
            Waveform.Sine => Mathf.Sin(angle),
            Waveform.Triangle => 1f - 4f * Mathf.Abs(cycle - 0.5f),
            // A sine with two quiet overtones: a soft music-box tone.
            Waveform.Bell => (Mathf.Sin(angle) + 0.3f * Mathf.Sin(angle * 2f) + 0.12f * Mathf.Sin(angle * 3f)) / 1.42f,
            _ => throw new ArgumentOutOfRangeException(nameof(wave)),
        };
    }

    static float Envelope(float time, float length, float attack, float decay, float release)
    {
        float rise = Mathf.Clamp01(time / attack);
        float fall = Mathf.Exp(-Mathf.Max(0f, time - attack) / decay);
        float end = time <= length ? 1f : 1f - (time - length) / release;
        return rise * fall * end;
    }

    static void RequireEnvelope(float attack, float decay, float release)
    {
        if (attack <= 0f || decay <= 0f || release <= 0f)
            throw new ArgumentException("Attack, decay and release must be positive.");
    }
}
