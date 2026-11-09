using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace SplintIt
{
    public enum Sfx { Click, Good, Bad, Success, Fail, Tick }

    /// <summary>Tiny procedural sound + haptic helper so the demo has feedback without any audio assets.
    /// Replace with real clips later; keep the Play(Sfx) call sites.</summary>
    public static class Feedback
    {
        static readonly Dictionary<Sfx, AudioClip> cache = new Dictionary<Sfx, AudioClip>();
        public static float Volume = 0.8f;

        public static void Play(Sfx sfx)
        {
            if (!cache.TryGetValue(sfx, out var clip) || clip == null)
            {
                clip = Build(sfx);
                cache[sfx] = clip;
            }
            var cam = Camera.main;
            AudioSource.PlayClipAtPoint(clip, cam != null ? cam.transform.position : Vector3.zero, Volume);
        }

        public static void Haptic(float amplitude = 0.4f, float seconds = 0.08f)
        {
            Send(XRNode.LeftHand, amplitude, seconds);
            Send(XRNode.RightHand, amplitude, seconds);
        }

        static void Send(XRNode node, float amplitude, float seconds)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            if (d.isValid && d.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
                d.SendHapticImpulse(0, amplitude, seconds);
        }

        static AudioClip Build(Sfx sfx)
        {
            // each note: frequency Hz, duration s
            float[][] notes;
            switch (sfx)
            {
                case Sfx.Click:   notes = new[] { new[] { 880f, 0.05f } }; break;
                case Sfx.Good:    notes = new[] { new[] { 660f, 0.07f }, new[] { 880f, 0.10f } }; break;
                case Sfx.Bad:     notes = new[] { new[] { 220f, 0.15f }, new[] { 180f, 0.18f } }; break;
                case Sfx.Success: notes = new[] { new[] { 523f, 0.10f }, new[] { 659f, 0.10f }, new[] { 784f, 0.22f } }; break;
                case Sfx.Fail:    notes = new[] { new[] { 330f, 0.15f }, new[] { 262f, 0.25f } }; break;
                default:          notes = new[] { new[] { 1000f, 0.03f } }; break;
            }

            const int rate = 44100;
            float total = 0f;
            foreach (var n in notes) total += n[1];
            var data = new float[Mathf.CeilToInt(total * rate)];
            int pos = 0;
            foreach (var n in notes)
            {
                int len = Mathf.CeilToInt(n[1] * rate);
                for (int i = 0; i < len && pos < data.Length; i++, pos++)
                {
                    float t = (float)i / len;
                    float env = Mathf.Clamp01(Mathf.Min(t * 12f, (1f - t) * 6f)); // quick fade in/out, no clicks
                    data[pos] = Mathf.Sin(2f * Mathf.PI * n[0] * i / rate) * 0.35f * env;
                }
            }
            var clip = AudioClip.Create("sfx_" + sfx, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
