using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public enum StationType { TutorialStation, QuizDesk, VisualExperimentDesk, CosmeticShopStation, LeaderboardBoard, TeacherNPC, Classmate }
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class InteractableStation : MonoBehaviour
    {
        public StationType stationType;
        public string stationPrompt = "[E] ექსპერიმენტის დაწყება";
        public bool supplyCloset;
        /// <summary>Prompt height above the station; negative keeps the default.</summary>
        public float promptHeight = -1;
        public bool inRange { get; private set; }
        private CanvasGroup prompt;
        private LabGame game;

        public void Initialize(LabGame lab)
        {
            game = lab;
            var root = new GameObject("მინიშნება", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0, promptHeight >= 0 ? promptHeight : supplyCloset ? 3.35f : 2.6f);
            root.transform.localScale = Vector3.one / 48f;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(520, 50);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 15;
            prompt = root.GetComponent<CanvasGroup>();
            prompt.alpha = 0;
            var label = LabUI.Text(root.transform, stationPrompt, 22, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            LabUI.WorldLabel(label);
            LabUI.Fill(label.rectTransform);
            GetComponent<BoxCollider2D>().isTrigger = true;
        }
        private void Update()
        {
            if (game == null) return;
            bool allowed = game.Tutorial.Allows(this);
            prompt.alpha = Mathf.MoveTowards(prompt.alpha, inRange && allowed && !LabUI.AnyOpen ? 1 : 0, Time.deltaTime * 8);
            if (supplyCloset && game.Tutorial.Step == TutorialManager.TutorialStep.CollectCoat && inRange && allowed && !LabUI.AnyOpen)
            {
                game.Tutorial.CollectCoat();
                return;
            }
            if (inRange && allowed && !LabUI.AnyOpen && Input.GetKeyDown(KeyCode.E))
                game.Interact(this);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) inRange = true;
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) inRange = false;
        }
    }
}

