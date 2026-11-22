using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public enum StationType
    {
        TutorialStation, QuizDesk, VisualExperimentDesk, CosmeticShopStation, LeaderboardBoard, TeacherNPC, Classmate,
        AtomTable, ElementStation, MoleculeTable, ReactionTable, SolutionStation, AcidBaseStation, ScienceFairDesk
    }
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class InteractableStation : MonoBehaviour
    {
        public StationType stationType;
        public string stationPrompt = "[E] ექსპერიმენტის დაწყება";
        public bool supplyCloset;
        /// <summary>Prompt height above the station; negative keeps the default.</summary>
        public float promptHeight = -1;
        /// <summary>Optional live prompt, e.g. to show that a station is still locked.</summary>
        public System.Func<string> promptText;
        public bool inRange { get; private set; }
        private CanvasGroup prompt;
        private TMP_Text label;
        private LabGame game;
        private Focus focus;

        public void Initialize(LabGame lab)
        {
            game = lab;
            var zone = GetComponent<BoxCollider2D>();
            focus = Focus.Attach(gameObject, zone.size, zone.offset, Kind, Objective, GetComponentsInChildren<SpriteRenderer>());
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
            label = LabUI.Text(root.transform, stationPrompt, 22, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            LabUI.WorldLabel(label);
            LabUI.Fill(label.rectTransform);
            GetComponent<BoxCollider2D>().isTrigger = true;
        }
        private void Update()
        {
            if (game == null) return;
            bool allowed = game.Tutorial.Allows(this);
            focus.Near = inRange && allowed;
            bool focused = focus.IsFocused;
            prompt.alpha = Mathf.MoveTowards(prompt.alpha, focused && !LabUI.AnyOpen ? 1 : 0, Time.deltaTime * 8);
            if (promptText != null && focused) label.text = promptText();
            if (supplyCloset && game.Tutorial.Step == TutorialManager.TutorialStep.CollectCoat && inRange && allowed && !LabUI.AnyOpen)
            {
                game.Tutorial.CollectCoat();
                return;
            }
            if (focused && !LabUI.AnyOpen && TouchControls.Interact)
                game.Interact(this);
        }
        private Glow Kind()
        {
            if (game == null || !game.Tutorial.Allows(this)) return Glow.Hidden;
            switch (stationType)
            {
                case StationType.TeacherNPC:
                case StationType.TutorialStation:
                case StationType.Classmate: return Glow.Teacher;
                case StationType.CosmeticShopStation: return supplyCloset && !GameSaveManager.Instance.Data.starterCoatCollected ? Glow.Equipment : Glow.Reward;
                case StationType.LeaderboardBoard: return Curriculum.Complete ? Glow.Reward : Glow.Hidden;
                case StationType.QuizDesk:
                case StationType.VisualExperimentDesk: return Curriculum.TierOpen(1) ? Glow.Reward : Glow.Hidden;
                default: return Curriculum.StationOpen(stationType) ? Glow.Equipment : Glow.Hidden;
            }
        }
        /// <summary>The closet during the coat step, the desk for the first experiment, then whichever station holds the next lesson.</summary>
        private bool Objective()
        {
            if (game == null) return false;
            // A running mission owns the objective: only its target glows.
            if (Missions.Steering) return stationType != StationType.TeacherNPC && Missions.IsTarget(transform.position);
            var step = game.Tutorial.Step;
            if (supplyCloset) return step == TutorialManager.TutorialStep.CollectCoat;
            return step == TutorialManager.TutorialStep.Completed && stationType == StationType.ReactionTable;
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

