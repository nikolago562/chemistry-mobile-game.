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
        public Vector3 Target => Step == TutorialStep.CollectCoat ? game.classroom.TransformPoint(game.lockerPosition + new Vector2(0, -1f))
            : game.StationPoint(StationType.ReactionTable);
        public string Objective => Step == TutorialStep.CollectCoat ? "აიღე ხალათი მბზინავ კარადაში." : Orders.Objective;

        public void Initialize(LabGame lab)
        {
            game = lab;
            var data = GameSaveManager.Instance.Data;
            // The coat is the whole intro now; older saves that stopped after it count as finished.
            if (data.starterCoatCollected) data.tutorialCompleted = true;
            Step = data.tutorialCompleted ? TutorialStep.Completed : TutorialStep.CollectCoat;
        }

        private void Start()
        {
            if (Step == TutorialStep.CollectCoat) gameObject.AddComponent<CoatArrows>().Initialize(game);
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
            foreach (var (id, count) in Mixing.StarterKit) LabProgress.Give(id, count);
            Step = TutorialStep.ReadNotebook;
            CompleteIntro();
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
            game.HUD.Notify("+10 ქიმქოინი და საწყისი ნაკრები • რეაქციების მაგიდა გელოდება!");
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

    /// <summary>Yellow floor arrows that chase toward the coat closet, plus one bobbing above it, until the coat is taken.</summary>
    public sealed class CoatArrows : MonoBehaviour
    {
        private LabGame game;
        private readonly System.Collections.Generic.List<SpriteRenderer> trail = new System.Collections.Generic.List<SpriteRenderer>();
        private SpriteRenderer marker;
        private Vector3 markerBase;
        private static Sprite arrow;

        public void Initialize(LabGame lab)
        {
            game = lab;
            Vector2 from = new Vector2(48.5f, 7.2f), to = game.lockerPosition + new Vector2(-1.8f, -1.1f);
            float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
            for (int i = 0; i < 6; i++)
            {
                var art = Draw("ისარი ხალათისკენ", Vector2.Lerp(from, to, i / 5f), 4);
                art.transform.localRotation = Quaternion.Euler(0, 0, angle);
                trail.Add(art);
            }
            marker = Draw("ისარი კარადაზე", game.lockerPosition + new Vector2(0, 3.7f), 12);
            marker.transform.localRotation = Quaternion.Euler(0, 0, -90);
            marker.transform.localScale = Vector3.one * 1.6f;
            markerBase = marker.transform.localPosition;
        }

        private SpriteRenderer Draw(string name, Vector2 local, int order)
        {
            arrow ??= PixelArt.Make(14, 12, (x, y) =>
            {
                // A right-pointing chevron arrow with a dark rim so it reads on any floor colour.
                bool head = x >= 7 && Mathf.Abs(y - 5.5f) <= 13 - x;
                bool shaft = x < 8 && y >= 4 && y <= 7;
                bool rimHead = x >= 6 && Mathf.Abs(y - 5.5f) <= 14 - x;
                bool rimShaft = x < 8 && y >= 3 && y <= 8;
                if (head || shaft) return new Color32(255, 214, 64, 255);
                return rimHead || rimShaft ? new Color32(120, 78, 20, 255) : Color.clear;
            });
            var art = LaboratoryArt.Draw(game.classroom, name, local, arrow, order);
            return art;
        }

        private void Update()
        {
            bool show = game.Tutorial.Step == TutorialManager.TutorialStep.CollectCoat;
            if (!show) { foreach (var a in trail) Destroy(a.gameObject); Destroy(marker.gameObject); Destroy(this); return; }
            float t = Time.time;
            for (int i = 0; i < trail.Count; i++)
            {
                // A wave runs along the trail toward the closet.
                float wave = Mathf.Max(0, Mathf.Sin(t * 4 - i * 0.9f));
                trail[i].color = new Color(1, 1, 1, 0.35f + 0.65f * wave);
                trail[i].transform.localScale = Vector3.one * (1 + 0.2f * wave);
            }
            marker.transform.localPosition = markerBase + Vector3.up * Mathf.Abs(Mathf.Sin(t * 3)) * 0.35f;
        }
    }
}
