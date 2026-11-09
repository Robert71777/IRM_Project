using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SplintIt
{
    /// <summary>Idle sway, speech bubble and the injury indicator. The injury is shown with a symbol
    /// ("!" then "OK"), never with graphic detail. Swap the gray placeholder body freely:
    /// only swayRoot, the bubble and the badge are referenced here.</summary>
    public class NpcReaction : MonoBehaviour
    {
        public enum Mood { Hurt, Steadier, Relieved }

        public Transform swayRoot;
        public GameObject speechBubble;
        public TMP_Text speechText;
        public TMP_Text badgeText;
        public Image badgeBackground;
        public Color hurtColor = new Color(1f, 0.55f, 0.1f);
        public Color reliefColor = new Color(0.2f, 0.8f, 0.5f);
        public Renderer bandRenderer;
        public Material bandHurtMaterial;
        public Material bandReliefMaterial;

        Mood mood;
        float speechUntil;

        void Awake()
        {
            if (speechBubble != null) speechBubble.SetActive(false);
            SetMood(Mood.Hurt);
        }

        public void SetMood(Mood m)
        {
            mood = m;
            bool relieved = m == Mood.Relieved;
            if (badgeText != null) badgeText.text = relieved ? "OK" : "!";
            if (badgeBackground != null) badgeBackground.color = relieved ? reliefColor : hurtColor;
            if (bandRenderer != null && bandHurtMaterial != null && bandReliefMaterial != null)
                bandRenderer.sharedMaterial = relieved ? bandReliefMaterial : bandHurtMaterial;
        }

        public void Say(string line, float seconds = 4f)
        {
            if (speechBubble == null || speechText == null || string.IsNullOrEmpty(line)) return;
            speechText.text = line;
            speechBubble.SetActive(true);
            speechUntil = Time.time + seconds;
        }

        void Update()
        {
            if (swayRoot != null)
            {
                float amp = mood == Mood.Hurt ? 1.2f : (mood == Mood.Steadier ? 0.7f : 0.3f);
                float speed = mood == Mood.Hurt ? 1.8f : 1.0f;
                swayRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * speed) * amp);
            }
            if (speechBubble != null && speechBubble.activeSelf && Time.time > speechUntil)
                speechBubble.SetActive(false);
        }
    }
}
