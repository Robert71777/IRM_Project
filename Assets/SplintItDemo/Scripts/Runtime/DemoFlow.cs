using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace SplintIt
{
    public enum FlowState { Menu, Playing, Paused, Result }

    /// <summary>Menu -> briefing -> round -> result, plus pause, tracking-loss recovery and comfort.
    /// All references are assigned by HikingDemoSceneBuilder; buttons are wired here at runtime.</summary>
    [DefaultExecutionOrder(-100)]
    public class DemoFlow : MonoBehaviour
    {
        [Header("Core")]
        public SplintBuildController build;
        public RoundTimer timer;
        public NpcReaction npc;
        public TrackingGuard tracking;
        public ComfortSettings comfort;
        public string scenarioName = "Hiking";

        [Header("Panels")]
        public GameObject menuPanel;
        public GameObject roundPanel;
        public GameObject resultPanel;
        public GameObject pausePanel;

        [Header("Menu")]
        public TMP_Text menuBriefing;
        public TMP_Text menuNotice;
        public Button guidedButton;
        public Button evaluationButton;

        [Header("Round")]
        public TMP_Text objectiveText;
        public TMP_Text checklistText;
        public TMP_Text statusText;
        public TMP_Text toastText;
        public Button hintButton;
        public Button checkButton;
        public Button pauseButton;
        public Button returnItemsButton;

        [Header("Result")]
        public TMP_Text resultTitle;
        public TMP_Text resultBody;
        public Button retryButton;
        public Button otherModeButton;
        public TMP_Text otherModeLabel;
        public Button resultMenuButton;

        [Header("Pause")]
        public TMP_Text pauseNotice;
        public Button resumeButton;
        public Button restartButton;
        public TMP_Text restartLabel;
        public Button quitButton;
        public TMP_Text quitLabel;
        public Button seatedButton;
        public TMP_Text seatedLabel;
        public Button heightUpButton;
        public Button heightDownButton;

        FlowState state;
        GameMode lastMode = GameMode.Guided;
        bool pendingRestart, pendingQuit, headsetAway, menuHeld;
        float toastUntil;
        int lastStep;

        static readonly string[] NpcLines =
        {
            "", "Okay, that gives my arm something firm.", "That's softer. Better!",
            "Almost there... hold it snug.", "That feels steady. Can you check it?"
        };

        // ------------------------------------------------------------------ lifecycle

        void Awake()
        {
            SaveSystem.Load();
            AudioListener.volume = SaveSystem.Data.volume;
            Time.timeScale = 1f;

            Wire(guidedButton, () => StartRound(GameMode.Guided));
            Wire(evaluationButton, () => StartRound(GameMode.Evaluation));
            Wire(hintButton, ShowHint);
            Wire(checkButton, () => Finish(false));
            Wire(pauseButton, Pause);
            Wire(returnItemsButton, () => build.ReturnLooseItems());
            Wire(retryButton, () => StartRound(lastMode));
            Wire(otherModeButton, () => StartRound(lastMode == GameMode.Guided ? GameMode.Evaluation : GameMode.Guided));
            Wire(resultMenuButton, ShowMenu);
            Wire(resumeButton, Resume);
            Wire(restartButton, OnRestart);
            Wire(quitButton, OnQuit);
            Wire(seatedButton, () => { if (comfort) comfort.ToggleSeated(); UpdatePauseLabels(); });
            Wire(heightUpButton, () => { if (comfort) comfort.AdjustHeight(0.05f); UpdatePauseLabels(); });
            Wire(heightDownButton, () => { if (comfort) comfort.AdjustHeight(-0.05f); UpdatePauseLabels(); });
        }

        void Start()
        {
            timer.Expired += OnTimerExpired;
            build.ProgressChanged += RefreshRoundUI;
            build.Toast += OnToast;
            if (tracking != null)
            {
                tracking.PresenceChanged += OnPresenceChanged;
                tracking.FocusLost += () => { if (state == FlowState.Playing) Pause(); };
            }
            if (menuBriefing != null)
                menuBriefing.text = "Hiking trail\nYour friend slipped and hurt their forearm. Find useful objects and build an improvised arm splint.\n\nSELECT -> POSITION -> PROTECT -> SECURE -> EVALUATE\n\nGrab with the grip button or pinch. Point and press the trigger to click buttons.";
            ShowMenu();
        }

        static void Wire(Button b, UnityEngine.Events.UnityAction a)
        {
            if (b != null) b.onClick.AddListener(() => { Feedback.Play(Sfx.Click); a(); });
        }

        void Update()
        {
            if (toastText != null && toastText.text.Length > 0 && Time.unscaledTime > toastUntil) toastText.text = "";
            PollMenuButton();
        }

        void PollMenuButton()
        {
            var d = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (!d.isValid || !d.TryGetFeatureValue(CommonUsages.menuButton, out bool pressed)) return;
            if (pressed && !menuHeld)
            {
                menuHeld = true;
                if (state == FlowState.Playing) Pause();
                else if (state == FlowState.Paused) Resume();
            }
            else if (!pressed) menuHeld = false;
        }

        // ------------------------------------------------------------------ states

        void ShowMenu()
        {
            Time.timeScale = 1f;
            build.EndRound();
            state = FlowState.Menu;
            SetPanels(true, false, false, false);
            if (menuNotice != null)
                menuNotice.text = SaveSystem.Message.Length > 0 ? SaveSystem.Message : "Guided: no timer, highlights and hints.   Evaluation: " + Mathf.RoundToInt(timer.durationSeconds) + " s timer, no hints.";
            npc.SetMood(NpcReaction.Mood.Hurt);
            npc.Say("Ow... my arm! Can you help me?", 6f);
        }

        void StartRound(GameMode mode)
        {
            Time.timeScale = 1f;
            lastMode = mode;
            pendingRestart = pendingQuit = false;
            build.BeginRound(mode);
            lastStep = 0;
            state = FlowState.Playing;
            SetPanels(false, true, false, false);
            if (hintButton != null) hintButton.interactable = mode == GameMode.Guided;
            npc.SetMood(NpcReaction.Mood.Hurt);
            npc.Say(mode == GameMode.Guided ? "Thanks for helping. Take your time." : "It really hurts... please hurry!", 5f);
            if (toastText != null) toastText.text = "";
            RefreshRoundUI();
        }

        void OnTimerExpired()
        {
            if (state == FlowState.Playing) Finish(true);
        }

        void Finish(bool timedOut)
        {
            if (state != FlowState.Playing) return;
            var r = build.Evaluate(timedOut);
            build.EndRound();
            state = FlowState.Result;
            if (r.mode == GameMode.Evaluation) SaveSystem.RecordEvaluation(r, scenarioName);
            ShowResult(r);
            SetPanels(false, false, true, false);
            Feedback.Play(r.success ? Sfx.Success : Sfx.Fail);
            Feedback.Haptic(0.6f, 0.2f);
            npc.SetMood(r.success ? NpcReaction.Mood.Relieved : NpcReaction.Mood.Hurt);
            npc.Say(r.success ? "Thank you! That feels so much better." : "It still hurts... let's try again.", 6f);
        }

        void ShowResult(EvaluationResult r)
        {
            resultTitle.text = r.success ? "Arm stabilized!" : (r.timedOut ? "Time is up" : "Splint not complete yet");
            var sb = new StringBuilder();
            sb.AppendLine("Scenario: " + scenarioName + "    Mode: " + r.mode);
            int t = Mathf.FloorToInt(r.secondsUsed);
            sb.Append("Time: " + (t / 60).ToString("00") + ":" + (t % 60).ToString("00") + "    Mistakes: " + r.mistakes);
            if (r.mode == GameMode.Evaluation && r.success) sb.Append("    Stars: " + r.stars + "/3");
            if (r.mode == GameMode.Guided) sb.Append("    Hints used: " + r.hintsUsed);
            sb.AppendLine();
            if (r.mode == GameMode.Guided) sb.AppendLine("Guided practice is not saved as an evaluation record.");
            if (r.passed.Count > 0) { sb.AppendLine("\nDone right:"); foreach (var s in r.passed) sb.AppendLine("  + " + s); }
            if (r.problems.Count > 0) { sb.AppendLine("\nTo fix:"); foreach (var s in r.problems) sb.AppendLine("  - " + s); }
            if (r.nextSteps.Count > 0) { sb.AppendLine("\nNext time:"); foreach (var s in r.nextSteps) sb.AppendLine("  > " + s); }
            resultBody.text = sb.ToString();
            if (otherModeLabel != null) otherModeLabel.text = lastMode == GameMode.Guided ? "TRY EVALUATION" : "BACK TO GUIDED";
        }

        void SetPanels(bool menu, bool round, bool result, bool pause)
        {
            if (menuPanel) menuPanel.SetActive(menu);
            if (roundPanel) roundPanel.SetActive(round);
            if (resultPanel) resultPanel.SetActive(result);
            if (pausePanel) pausePanel.SetActive(pause);
        }

        // ------------------------------------------------------------------ in-round UI

        void RefreshRoundUI()
        {
            if (objectiveText != null) objectiveText.text = build.GetObjective();
            if (checklistText != null) checklistText.text = build.GetChecklistText();
            if (statusText != null)
                statusText.text = "Mistakes: " + build.Mistakes + (build.Mode == GameMode.Guided ? "     Hints used: " + build.HintsUsed : "");
            int step = build.CurrentStepIndex;
            if (state == FlowState.Playing && step > lastStep && step < NpcLines.Length) npc.Say(NpcLines[step]);
            lastStep = step;
        }

        void ShowHint()
        {
            if (state != FlowState.Playing) return;
            OnToast("Hint: " + build.GetHint(), true);
        }

        void OnToast(string message, bool positive)
        {
            if (toastText == null) return;
            toastText.text = (positive ? "[OK] " : "[!] ") + message;
            toastUntil = Time.unscaledTime + 5f;
        }

        // ------------------------------------------------------------------ pause / tracking / comfort

        void Pause()
        {
            if (state != FlowState.Playing) return;
            state = FlowState.Paused;
            Time.timeScale = 0f;
            timer.SetPaused(true);
            build.SetPaused(true);
            pendingRestart = pendingQuit = false;
            if (pauseNotice != null && !headsetAway) pauseNotice.text = "Round paused. Timer, objects and the patient are frozen.";
            UpdatePauseLabels();
            if (pausePanel) pausePanel.SetActive(true);
        }

        void Resume()
        {
            if (state != FlowState.Paused) return;
            if (headsetAway || (tracking != null && !tracking.Present)) { UpdatePauseLabels(); return; }
            Time.timeScale = 1f;
            timer.SetPaused(false);
            build.SetPaused(false);
            state = FlowState.Playing;
            if (pausePanel) pausePanel.SetActive(false);
        }

        void OnRestart()
        {
            if (!pendingRestart) { pendingRestart = true; pendingQuit = false; UpdatePauseLabels(); return; }
            StartRound(lastMode);
        }

        void OnQuit()
        {
            if (!pendingQuit) { pendingQuit = true; pendingRestart = false; UpdatePauseLabels(); return; }
            ShowMenu();
        }

        void OnPresenceChanged(bool present)
        {
            headsetAway = !present;
            if (!present)
            {
                if (state == FlowState.Playing) Pause();
                if (pauseNotice != null) pauseNotice.text = "Headset removed or tracking lost. Put the headset back on, then press RESUME.";
            }
            else if (state == FlowState.Paused && pauseNotice != null)
            {
                pauseNotice.text = "Tracking restored. Check your surroundings, then press RESUME.";
            }
            UpdatePauseLabels();
        }

        void UpdatePauseLabels()
        {
            if (restartLabel != null) restartLabel.text = pendingRestart ? "TAP AGAIN TO RESTART" : "RESTART";
            if (quitLabel != null) quitLabel.text = pendingQuit ? "TAP AGAIN TO LEAVE" : "MAIN MENU";
            if (resumeButton != null) resumeButton.interactable = !headsetAway;
            if (seatedLabel != null && comfort != null) seatedLabel.text = comfort.Describe();
        }
    }
}
