using System;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class TutorialManager : MonoBehaviour
    {
        public enum TutorialStep { CollectCoat, ReadNotebook, Completed }
        public TutorialStep Step { get; private set; }
        public event Action Changed;
        private LabGame game;
        private bool welcomeShown;
        public Vector3 Target => game.classroom.TransformPoint(Step == TutorialStep.CollectCoat
            ? (Vector3)(game.lockerPosition + new Vector2(0, -1f)) : (Vector3)game.teacherDeskPosition);
        public string Objective => Step == TutorialStep.CollectCoat ? "აიღე ხალათი მბზინავ კარადაში."
            : Step == TutorialStep.ReadNotebook ? "მასწავლებლის მაგიდასთან პირველი ცდა გელოდება."
            : Curriculum.NextAction;

        public void Initialize(LabGame lab)
        {
            game = lab;
            var data = GameSaveManager.Instance.Data;
            Step = data.tutorialCompleted ? TutorialStep.Completed
                : data.starterCoatCollected ? TutorialStep.ReadNotebook : TutorialStep.CollectCoat;
        }

        private void Start()
        {
            game.Profile.Begin(ShowWelcome);
        }
        private void ShowWelcome()
        {
            if (Step == TutorialStep.Completed || welcomeShown) return;
            welcomeShown = true;
            game.Guide.Queue("welcome");
            return;
#pragma warning disable CS0162
            Dialogue("კეთილი იყოს შენი მობრძანება ლაბორატორიაში!\n\nგამოიყენე WASD ან ისრები. " +
                (Step == TutorialStep.CollectCoat
                    ? "მიდი მბზინავ კარადასთან და აიღე დამცავი ხალათი. ლაბორატორიაში უსაფრთხოება პირველ ადგილზეა."
                    : "ხალათი უკვე გაქვს. მიდი წინა მაგიდასთან და დაიწყე წყლის მიღების დავალება."));
        }

        public bool Allows(InteractableStation station)
        {
            if (Step == TutorialStep.Completed || station.stationType == StationType.TeacherNPC) return true;
            if (Step == TutorialStep.CollectCoat) return station.supplyCloset;
            return station.stationType == StationType.TutorialStation;
        }
        public void CollectCoat()
        {
            if (Step != TutorialStep.CollectCoat) return;
            var save = GameSaveManager.Instance;
            save.Data.starterCoatCollected = true;
            save.UnlockCosmetic("coat_white");
            save.Data.equippedLabCoatID = "coat_white";
            save.Commit();
            Step = TutorialStep.ReadNotebook;
            Changed?.Invoke();
            AudioManager.Instance?.Milestone();
            game.Guide.Queue("coat");
            return;
            Dialogue("ხალათი მოირგე!\n\nახლა მიდი მასწავლებლის მაგიდასთან და დააჭირე [E]. " +
                "ჩვენი პირველი დავალებაა წყალბადისა და ჟანგბადისგან წყლის მიღება.");
        }
        /// <summary>Finishes the opening at the teacher's desk; the caller starts the first hands-on lesson.</summary>
        public void CompleteIntro()
        {
            if (Step != TutorialStep.ReadNotebook) return;
            var save = GameSaveManager.Instance;
            save.Data.tutorialCompleted = true;
            save.RecordLesson(10);
            Step = TutorialStep.Completed;
            Changed?.Invoke();
            AudioManager.Instance?.Milestone();
            game.HUD.Notify("+10 ქიმქოინი • ლაბორატორია შენია!");
        }
#pragma warning restore CS0162
        public void Dialogue(string message)
        {
            var window = LabUI.Window("პირველი დღე ლაბორატორიაში", 980, 570);
            var split = LabUI.Row(window.Body, "პირველი დღე", 24);
            LabUI.FlexibleHeight(split.gameObject);
            var portrait = LabUI.Card(split.transform, "მასწავლებელი", LabUI.Green, 22);
            LabUI.Size(portrait.gameObject, 350, 250, 0);
            var art = LabUI.Object(portrait.transform, "პორტრეტი", typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
            art.sprite = LabUI.Theme != null ? LabUI.Theme.teacherPortrait : null;
            if (LabUI.Theme != null && LabUI.Theme.teacherPortraitMaterial != null)
                art.material = LabUI.Theme.teacherPortraitMaterial;
            art.preserveAspect = true;
            if (art.sprite == null) art.sprite = LabIcons.Get(LabIcon.Flask);
            LabUI.Size(art.gameObject, 190);
            var name = LabUI.Text(portrait.transform, "შენი მასწავლებელი", 21, UnityEngine.Color.white);
            name.alignment = TextAlignmentOptions.Center;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.enableAutoSizing = true;
            name.fontSizeMin = 14;
            name.fontSizeMax = 21;
            LabUI.Size(name.gameObject, 34);
            var caption = LabUI.Text(portrait.transform, "ერთად აღმოვაჩინოთ ქიმია!", 16, new UnityEngine.Color(0.86f, 0.85f, 1));
            caption.alignment = TextAlignmentOptions.Center;
            LabUI.Size(caption.gameObject, 44);

            var content = LabUI.Column(split.transform, "მითითება", 15);
            LabUI.Size(content.gameObject, 350, -1, 1);
            var steps = LabUI.Row(content.transform, "პროგრესის ნაბიჯები");
            LabUI.Pill(steps.transform, "ხალათი", Step == TutorialStep.CollectCoat ? LabUI.Mint : LabUI.Green);
            LabUI.Pill(steps.transform, "რვეული", Step == TutorialStep.ReadNotebook ? LabUI.Mint : LabUI.Muted);
            LabUI.Pill(steps.transform, "ჯილდო", LabUI.Muted);
            var heading = LabUI.Text(content.transform, Step == TutorialStep.CollectCoat
                ? "მზად ხარ აღმოსაჩენად?" : "ახლა შენი პირველი რეაქცია!", 30);
            heading.fontStyle = FontStyles.Bold;
            LabUI.Size(heading.gameObject, 76);
            var text = LabUI.Text(content.transform, message, 21);
            LabUI.FlexibleHeight(text.gameObject);
            var reward = LabUI.Pill(content.transform, "+10 ქიმქოინი  •  ახალი ელემენტები", LabUI.Mint);
            LabUI.Size(reward.transform.parent.gameObject, 34);
            var controls = LabUI.Row(window.Body, "მოქმედება");
            var help = LabUI.Text(controls.transform, "WASD / ისრები — მოძრაობა    E — მოქმედება", 17, LabUI.Muted);
            LabUI.Size(help.gameObject, 50, -1, 1);
            var button = LabUI.Button(controls.transform, Step == TutorialStep.CollectCoat ? "დავიწყოთ!" : "ცდისკენ!", window.Close, 52);
            LabUI.Size(button.gameObject, 52, 210, 0);
        }
    }
}
