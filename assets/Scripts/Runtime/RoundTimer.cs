using System;
using TMPro;
using UnityEngine;

namespace SplintIt
{
    /// <summary>Countdown (Evaluation) or free stopwatch (Guided). Text always carries a symbol,
    /// never colour alone: normal / (!) low / || paused / (X) expired.</summary>
    public class RoundTimer : MonoBehaviour
    {
        public float durationSeconds = 150f;
        public float lowTimeSeconds = 30f;
        public TMP_Text[] outputs;

        public TimerState State { get; private set; } = TimerState.Idle;
        public bool Timed { get; private set; }
        public float Elapsed { get; private set; }
        public float Remaining => Mathf.Max(0f, durationSeconds - Elapsed);
        public event Action Expired;

        bool paused;
        int lastWholeSecond = -1;

        public void StartCountdown() { Begin(true); }
        public void StartUntimed() { Begin(false); }

        void Begin(bool timed)
        {
            Timed = timed;
            Elapsed = 0f;
            paused = false;
            lastWholeSecond = -1;
            State = TimerState.Normal;
            Refresh();
        }

        public void Stop()
        {
            State = TimerState.Idle;
            paused = false;
            Refresh();
        }

        public void SetPaused(bool value)
        {
            if (State == TimerState.Idle || State == TimerState.Expired) return;
            paused = value;
            Refresh();
        }

        void Update()
        {
            if (State != TimerState.Normal && State != TimerState.Low) return;
            if (paused) return;

            Elapsed += Time.deltaTime;
            if (Timed)
            {
                if (Remaining <= 0f)
                {
                    State = TimerState.Expired;
                    Refresh();
                    Expired?.Invoke();
                    return;
                }
                if (Remaining <= lowTimeSeconds)
                {
                    State = TimerState.Low;
                    int s = Mathf.CeilToInt(Remaining);
                    if (s != lastWholeSecond) { lastWholeSecond = s; Feedback.Play(Sfx.Tick); }
                }
            }
            Refresh();
        }

        static string Mmss(int total) { return (total / 60).ToString("00") + ":" + (total % 60).ToString("00"); }
        string Remain() { return Mmss(Mathf.CeilToInt(Remaining)); }

        string Describe()
        {
            if (State == TimerState.Idle) return "TIME --:--";
            if (paused) return Timed ? "|| PAUSED  " + Remain() : "|| PAUSED";
            if (State == TimerState.Expired) return "(X) TIME UP  00:00";
            if (!Timed) return "NO TIME LIMIT  " + Mmss(Mathf.FloorToInt(Elapsed));
            if (State == TimerState.Low) return "(!) LOW TIME  " + Remain();
            return "TIME  " + Remain();
        }

        void Refresh()
        {
            if (outputs == null) return;
            string text = Describe();
            foreach (var o in outputs) if (o != null) o.text = text;
        }
    }
}
