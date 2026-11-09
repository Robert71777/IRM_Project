using System.Collections.Generic;

namespace SplintIt
{
    public enum GameMode { Guided, Evaluation }
    public enum ItemRole { RigidSupport, Padding, Strap, Distractor }
    public enum ItemState { Available, Grabbed, InPrepArea, Positioned, PartiallyWrapped, Secured, PositionedIncorrectly }
    public enum ZoneKind { Prep, Support, Padding, StrapUpper, StrapLower }
    public enum TimerState { Idle, Normal, Low, Expired }

    /// <summary>Outcome of one round, shown on the result screen and (Evaluation only) saved locally.</summary>
    public class EvaluationResult
    {
        public bool success;
        public bool timedOut;
        public int mistakes;
        public int hintsUsed;
        public int stars;               // 0 when not applicable (Guided or failed)
        public float secondsUsed;
        public GameMode mode;
        public List<string> passed = new List<string>();
        public List<string> problems = new List<string>();
        public List<string> nextSteps = new List<string>();
    }
}
