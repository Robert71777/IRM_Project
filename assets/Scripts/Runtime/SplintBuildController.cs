using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SplintIt
{
    /// <summary>The rules of the round. Everything about completion is DERIVED from what is currently in each
    /// zone (see SKILL.md), so picking an item back up is always a safe undo - there are no stored "done" flags.
    /// Loop: SELECT -> POSITION -> PROTECT -> SECURE -> EVALUATE.
    /// Order: support first, then padding, then the two straps. A distractor placed on the prep board or the
    /// arm, or an out-of-order placement, counts as a mistake (with a one-sentence explanation) and the item
    /// is returned. A strap needs ~1 s of stillness after release to become "secured".</summary>
    public class SplintBuildController : MonoBehaviour
    {
        [Header("Wired by HikingDemoSceneBuilder")]
        public SplintItem[] items;
        public PlacementZone prepZone;
        public PlacementZone supportZone;
        public PlacementZone paddingZone;
        public PlacementZone strapUpperZone;
        public PlacementZone strapLowerZone;
        public RoundTimer timer;

        [Header("Tuning")]
        [Tooltip("How long a released strap must hold still inside its zone before it is 'secured'.")]
        public float strapSettleSeconds = 1f;
        public const int RequiredStraps = 2;
        [Tooltip("Stars: 3 needs no mistakes and at least this fraction of time left.")]
        public float threeStarTimeFraction = 0.40f;

        // ---- events the UI listens to ----
        public event Action ProgressChanged;
        public event Action<string, bool> Toast;    // (message, positive)

        // ---- round state ----
        public GameMode Mode { get; private set; }
        public int Mistakes { get; private set; }
        public int HintsUsed { get; private set; }
        public bool Active { get; private set; }

        bool paused;

        // supply "home" positions so returned items go back to the table, not their random drop spot
        readonly Dictionary<SplintItem, Vector3> home = new Dictionary<SplintItem, Vector3>();
        // straps that have been released in their zone and are counting down to "secured"
        readonly Dictionary<SplintItem, float> settling = new Dictionary<SplintItem, float>();

        void Awake()
        {
            if (items != null)
                foreach (var it in items)
                {
                    if (it == null) continue;
                    home[it] = it.transform.position;
                    it.Released += OnItemReleased;
                    it.PickedUp += OnItemPickedUp;
                }
            EndRound();
        }

        // ------------------------------------------------------------------ round lifecycle

        public void BeginRound(GameMode mode)
        {
            Mode = mode;
            Mistakes = 0;
            HintsUsed = 0;
            Active = true;
            paused = false;
            settling.Clear();

            foreach (var it in Items())
            {
                it.ResetToStart();
                it.SetInteractable(true);
            }
            ClearZones();

            if (timer != null)
            {
                if (mode == GameMode.Evaluation) timer.StartCountdown();
                else timer.StartUntimed();
            }

            RefreshGuidance();
            ProgressChanged?.Invoke();
        }

        public void EndRound()
        {
            Active = false;
            paused = false;
            settling.Clear();
            if (timer != null) timer.Stop();
            foreach (var it in Items())
            {
                it.SetHint(false);
                it.SetInteractable(false);
            }
            ShowAllGhosts(false);
        }

        public void SetPaused(bool value)
        {
            paused = value;
            foreach (var it in Items())
            {
                it.SetInteractable(!value && Active);
                if (value) it.Stabilize();      // re-stabilise so nothing drifts while frozen
            }
        }

        // ------------------------------------------------------------------ item events

        void OnItemPickedUp(SplintItem item)
        {
            settling.Remove(item);
            ProgressChanged?.Invoke();
            RefreshGuidance();
        }

        void OnItemReleased(SplintItem item)
        {
            if (!Active || paused) return;

            PlacementZone zone = FindZoneUnder(item);

            // dropped nowhere useful (or fell on the floor): send it home to the supply table
            if (zone == null)
            {
                if (item.transform.position.y < 0.3f) SendHome(item, silent: true);
                ProgressChanged?.Invoke();
                return;
            }

            if (zone.kind == ZoneKind.Prep)
            {
                HandlePrepRelease(item);
                return;
            }

            HandleArmRelease(item, zone);
        }

        void HandlePrepRelease(SplintItem item)
        {
            if (item.role == ItemRole.Distractor)
            {
                Mistake(item.displayName + " does not help here: " + ExplainBad(item));
                SendHome(item);
                return;
            }
            // useful item resting on the prep board
            prepZone.ShowGhost(false);
            item.CurrentZone = prepZone;
            item.State = ItemState.InPrepArea;
            item.SnapTo(new Vector3(item.transform.position.x, prepZone.transform.position.y, item.transform.position.z), item.transform.rotation);
            Feedback.Play(Sfx.Good);
            ProgressChanged?.Invoke();
            RefreshGuidance();
        }

        void HandleArmRelease(SplintItem item, PlacementZone zone)
        {
            // wrong kind of object for this zone
            if (!zone.Accepts(item))
            {
                if (item.role == ItemRole.Distractor) Mistake(item.displayName + " cannot go on the arm: " + ExplainBad(item));
                else Mistake(item.displayName + " does not belong in that spot.");
                SendHome(item);
                return;
            }

            // order: padding only after support; straps only after padding
            if (zone.kind == ZoneKind.Padding && !SupportPlaced)
            {
                Mistake("Place the rigid support along the arm before adding padding.");
                ReturnToPrep(item);
                return;
            }
            if ((zone.kind == ZoneKind.StrapUpper || zone.kind == ZoneKind.StrapLower) && !PaddingPlaced)
            {
                Mistake("Add soft padding before securing the straps.");
                ReturnToPrep(item);
                return;
            }

            // support orientation must roughly follow the forearm
            if (zone.kind == ZoneKind.Support && !zone.OrientationOk(item))
            {
                item.AlignLengthTo(zone.transform.forward);
            }

            // occupy the zone, snap neatly
            zone.Occupy(item);
            Quaternion snapRot = (zone.kind == ZoneKind.Support) ? AlignedRotation(item, zone) : item.transform.rotation;
            item.SnapTo(zone.transform.position, snapRot);
            zone.ShowGhost(false);

            if (zone.kind == ZoneKind.StrapUpper || zone.kind == ZoneKind.StrapLower)
            {
                // partially wrapped now; it becomes secured after holding still
                item.State = ItemState.PartiallyWrapped;
                item.SetWrapped(true);
                settling[item] = Time.time + strapSettleSeconds;
                Feedback.Play(Sfx.Good);
            }
            else
            {
                item.State = ItemState.Positioned;
                Feedback.Play(Sfx.Good);
                Feedback.Haptic(0.3f, 0.06f);
            }

            ProgressChanged?.Invoke();
            RefreshGuidance();
        }

        Quaternion AlignedRotation(SplintItem item, PlacementZone zone)
        {
            Vector3 current = item.transform.TransformDirection(item.lengthAxis);
            Vector3 target = zone.transform.forward;
            if (Vector3.Dot(current, target) < 0f) target = -target;
            return Quaternion.FromToRotation(current, target) * item.transform.rotation;
        }

        void Update()
        {
            if (!Active || paused || settling.Count == 0) return;

            List<SplintItem> promoted = null;
            foreach (var kv in settling)
            {
                var item = kv.Key;
                if (item == null || item.IsHeld || item.CurrentZone == null) continue;
                if (Time.time >= kv.Value)
                {
                    item.State = ItemState.Secured;
                    (promoted ??= new List<SplintItem>()).Add(item);
                }
            }
            if (promoted != null)
            {
                foreach (var it in promoted)
                {
                    settling.Remove(it);
                    Feedback.Play(Sfx.Good);
                    Feedback.Haptic(0.4f, 0.08f);
                }
                ProgressChanged?.Invoke();
                RefreshGuidance();
            }
        }

        // ------------------------------------------------------------------ derived state (never stored)

        bool SupportPlaced => supportZone.Occupant != null && supportZone.Occupant.State == ItemState.Positioned;
        bool SupportOriented => SupportPlaced && supportZone.OrientationOk(supportZone.Occupant);
        bool PaddingPlaced => paddingZone.Occupant != null && paddingZone.Occupant.State == ItemState.Positioned;
        bool StrapSecured(PlacementZone z) => z.Occupant != null && z.Occupant.State == ItemState.Secured;
        int StrapsSecured => (StrapSecured(strapUpperZone) ? 1 : 0) + (StrapSecured(strapLowerZone) ? 1 : 0);
        bool AllStrapsSecured => StrapsSecured >= RequiredStraps;

        /// <summary>0 = pick support, 1 = add padding, 2 = secure straps, 3 = ready to check.</summary>
        public int CurrentStepIndex
        {
            get
            {
                if (!SupportPlaced) return 0;
                if (!PaddingPlaced) return 1;
                if (!AllStrapsSecured) return 2;
                return 3;
            }
        }

        // ------------------------------------------------------------------ evaluation

        public EvaluationResult Evaluate(bool timedOut)
        {
            var r = new EvaluationResult
            {
                mode = Mode,
                mistakes = Mistakes,
                hintsUsed = HintsUsed,
                timedOut = timedOut,
                secondsUsed = timer != null ? timer.Elapsed : 0f
            };

            bool materials = SupportPlaced && PaddingPlaced && StrapsSecured >= 1;
            bool support = SupportPlaced && SupportOriented;
            bool padding = PaddingPlaced;
            bool straps = AllStrapsSecured;

            if (support) r.passed.Add("Rigid support placed along the forearm.");
            else r.problems.Add(SupportPlaced ? "The support is placed but not aligned with the arm." : "No rigid support along the arm.");

            if (padding) r.passed.Add("Soft padding between the arm and the support.");
            else r.problems.Add("No padding to protect the arm from the hard support.");

            if (straps) r.passed.Add("Two straps secured, above and below the injury.");
            else r.problems.Add(StrapsSecured + " of " + RequiredStraps + " straps secured (hold each one still for a moment).");

            if (!materials) r.problems.Add("Some required materials are missing.");

            r.success = support && padding && straps && !timedOut;

            if (timedOut && !r.success) r.problems.Add("The timer ran out before the splint was complete.");

            // next-step guidance always actionable
            if (!r.success)
            {
                if (!SupportPlaced) r.nextSteps.Add("Start with the Branch: lay it along the forearm.");
                else if (!SupportOriented) r.nextSteps.Add("Rotate the support so it runs along the arm (within ~35 deg).");
                else if (!PaddingPlaced) r.nextSteps.Add("Put the Jacket on the arm as padding before the straps.");
                else if (!AllStrapsSecured) r.nextSteps.Add("Place the Belt and Scarf on the two strap spots and hold each still ~1 s.");
            }

            r.stars = Stars(r);
            return r;
        }

        int Stars(EvaluationResult r)
        {
            if (Mode != GameMode.Evaluation || !r.success) return 0;
            float frac = (timer != null && timer.durationSeconds > 0f) ? timer.Remaining / timer.durationSeconds : 0f;
            if (r.mistakes == 0 && frac >= threeStarTimeFraction) return 3;
            if (r.mistakes <= 2) return 2;
            return 1;
        }

        // ------------------------------------------------------------------ UI text

        public string GetObjective()
        {
            switch (CurrentStepIndex)
            {
                case 0: return "1/4  Lay a rigid support along the forearm.";
                case 1: return "2/4  Add soft padding over the arm.";
                case 2: return "3/4  Secure two straps (above and below the injury).";
                default: return "4/4  Ready - press CHECK SPLINT.";
            }
        }

        public string GetChecklistText()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Line(SupportPlaced && SupportOriented, "Rigid support along the arm"));
            sb.AppendLine(Line(PaddingPlaced, "Soft padding on the arm"));
            sb.AppendLine(Line(StrapSecured(strapUpperZone), "Upper strap secured"));
            sb.Append(Line(StrapSecured(strapLowerZone), "Lower strap secured"));
            return sb.ToString();
        }

        static string Line(bool done, string label)
        {
            return (done ? "[x] " : "[ ] ") + label;
        }

        public string GetHint()
        {
            HintsUsed++;
            ProgressChanged?.Invoke();
            switch (CurrentStepIndex)
            {
                case 0: return "The Branch is stiff and long - lay it along the forearm.";
                case 1: return "The Jacket is soft - drape it over the arm as padding.";
                case 2: return "Use the Belt and the Scarf on the two marked spots; hold each still a moment.";
                default: return "Everything is in place - press CHECK SPLINT.";
            }
        }

        // ------------------------------------------------------------------ helpers

        public void ReturnLooseItems()
        {
            foreach (var it in Items())
            {
                if (it.IsHeld) continue;
                if (it.CurrentZone == null) SendHome(it, silent: true);
            }
            ProgressChanged?.Invoke();
            if (Toast != null) Toast("Loose items returned to the supply table.", true);
        }

        void Mistake(string explanation)
        {
            Mistakes++;
            Feedback.Play(Sfx.Bad);
            Feedback.Haptic(0.5f, 0.12f);
            Toast?.Invoke(explanation, false);
            ProgressChanged?.Invoke();
        }

        void SendHome(SplintItem item, bool silent = false)
        {
            Vector3 target = home.TryGetValue(item, out var h) ? h : item.transform.position;
            if (item.CurrentZone != null) item.CurrentZone.Remove(item);
            settling.Remove(item);
            item.ReturnTo(target);
            RefreshGuidance();
            if (!silent) ProgressChanged?.Invoke();
        }

        void ReturnToPrep(SplintItem item)
        {
            if (item.CurrentZone != null) item.CurrentZone.Remove(item);
            settling.Remove(item);
            Vector3 p = prepZone.transform.position;
            item.SnapTo(p, item.transform.rotation);
            item.CurrentZone = prepZone;
            item.State = ItemState.InPrepArea;
            RefreshGuidance();
            ProgressChanged?.Invoke();
        }

        PlacementZone FindZoneUnder(SplintItem item)
        {
            Vector3 p = item.transform.position;
            PlacementZone best = null;
            float bestSqr = float.MaxValue;
            foreach (var z in Zones())
            {
                if (z == null || !z.Contains(p)) continue;
                float d = (p - z.transform.position).sqrMagnitude;
                // arm zones win over the broad prep zone when both contain the point
                float bias = z.kind == ZoneKind.Prep ? 1000f : 0f;
                if (d + bias < bestSqr) { bestSqr = d + bias; best = z; }
            }
            return best;
        }

        void ClearZones()
        {
            foreach (var z in Zones())
                if (z != null && z.Occupant != null) z.Remove(z.Occupant);
        }

        void RefreshGuidance()
        {
            bool guided = Mode == GameMode.Guided && Active && !paused;

            // target outlines on the next-needed arm zone(s)
            int step = CurrentStepIndex;
            ShowAllGhosts(false);
            if (guided)
            {
                if (step == 0) supportZone.ShowGhost(true);
                else if (step == 1) paddingZone.ShowGhost(true);
                else if (step == 2)
                {
                    if (!StrapSecured(strapUpperZone)) strapUpperZone.ShowGhost(true);
                    if (!StrapSecured(strapLowerZone)) strapLowerZone.ShowGhost(true);
                }
            }

            // floating hint arrows over the item(s) needed next
            foreach (var it in Items())
            {
                bool hint = guided && NeededNext(it, step);
                it.SetHint(hint && !it.IsHeld && it.CurrentZone == null);
            }
        }

        bool NeededNext(SplintItem it, int step)
        {
            switch (step)
            {
                case 0: return it.role == ItemRole.RigidSupport;
                case 1: return it.role == ItemRole.Padding;
                case 2: return it.role == ItemRole.Strap;
                default: return false;
            }
        }

        void ShowAllGhosts(bool on)
        {
            supportZone?.ShowGhost(on);
            paddingZone?.ShowGhost(on);
            strapUpperZone?.ShowGhost(on);
            strapLowerZone?.ShowGhost(on);
            prepZone?.ShowGhost(on);
        }

        string ExplainBad(SplintItem item)
        {
            return string.IsNullOrEmpty(item.whyNotSuitable) ? "it is not suitable for a splint." : item.whyNotSuitable;
        }

        IEnumerable<SplintItem> Items()
        {
            if (items == null) yield break;
            foreach (var it in items) if (it != null) yield return it;
        }

        IEnumerable<PlacementZone> Zones()
        {
            yield return supportZone;
            yield return paddingZone;
            yield return strapUpperZone;
            yield return strapLowerZone;
            yield return prepZone;
        }
    }
}
