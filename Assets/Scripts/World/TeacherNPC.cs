using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// The chemistry teacher in the classroom: idle breathing, a lively bob and sway while she
    /// talks (each voice blip gives a little hop), and a floating "!" / "?" when a quest is
    /// waiting to be taken or handed in.
    /// </summary>
    public sealed class TeacherNPC : MonoBehaviour, ISpeaker
    {
        public bool Talking { get; set; }
        public float Voice => 1f;
        private LabGame game;
        private Transform body;
        private SpriteRenderer sprite;
        private TMP_Text marker;
        private CanvasGroup nameplate;
        private float hop;
        private float talkBlend;

        public void Initialize(LabGame lab, SpriteRenderer art, TMP_Text questMarker, CanvasGroup label)
        {
            game = lab; sprite = art; body = art.transform; marker = questMarker; nameplate = label;
        }

        /// <summary>Called for each voiced syllable.</summary>
        public void Pulse() => hop = 1;

        private void Update()
        {
            float t = Time.unscaledTime;
            talkBlend = Mathf.MoveTowards(talkBlend, Talking ? 1 : 0, Time.unscaledDeltaTime * 4);
            hop = Mathf.MoveTowards(hop, 0, Time.unscaledDeltaTime * 9);
            float bounce = Mathf.Sin(hop * Mathf.PI) * 0.09f * talkBlend;
            float breathe = Mathf.Sin(t * 2.1f) * 0.012f;
            float stretch = breathe + bounce * 0.6f;
            body.localScale = new Vector3(1 - stretch * 0.5f, 1 + stretch, 1);
            body.localPosition = new Vector3(0, bounce, 0);
            // Gentle side-to-side sway, as if she gestures while explaining.
            body.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 3.3f) * 3.2f * talkBlend);
            if (Talking && game != null && game.player != null)
                sprite.flipX = game.player.transform.position.x < transform.position.x - 0.2f;

            if (nameplate != null) nameplate.alpha = 1 - talkBlend;
            if (marker == null) return;
            var data = GameSaveManager.Instance.Data;
            var quest = TeacherQuests.Active;
            string symbol = !data.tutorialCompleted ? "" : quest == null ? "!" : TeacherQuests.Done(quest) ? "?" : "";
            if (marker.text != symbol) marker.text = symbol;
            marker.color = symbol == "?" ? LabUI.Mint : LabUI.Gold;
            marker.alpha = 1 - talkBlend;
            marker.rectTransform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(t * 2.6f)) * 9f, 0);
        }
    }
}
