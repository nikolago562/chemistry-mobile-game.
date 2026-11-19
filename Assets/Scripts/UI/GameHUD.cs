using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class GameHUD : MonoBehaviour
    {
        private TMP_Text coins, streak, combo, title, objective, taskTitle, toast, waypoint;
        private Image progress;
        private Image toastCard;
        private Image waypointCard;
        private LabGame game;
        private float toastUntil;
        private float displayedCoins;
        private int targetCoins;
        private int visibleCoins = -1;
        private Camera worldCamera;

        public void Initialize(LabGame lab)
        {
            game = lab;
            worldCamera = Camera.main;
            var identity = LabUI.Panel(LabUI.Root, "მკვლევრის ბარათი", LabUI.Navy);
            Anchor(identity.rectTransform, new Vector2(0, 1), new Vector2(16, -14), new Vector2(300, 76));
            LabUI.Shadow(identity);
            var badge = LabUI.Panel(identity.transform, "პროფილი", LabUI.Green);
            Anchor(badge.rectTransform, new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(54, 54));
            var icon = LabUI.Icon(badge.transform, LabIcon.Atom, Color.white);
            LabUI.Fill(icon.rectTransform, 9);
            var name = LabUI.Text(identity.transform, "ქიმიის ლაბორატორია", 20, Color.white);
            PlaceText(name.rectTransform, new Vector2(78, -12), new Vector2(210, 27));
            name.fontStyle = FontStyles.Bold;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.enableAutoSizing = true;
            name.fontSizeMin = 12;
            name.fontSizeMax = 18;
            title = LabUI.Text(identity.transform, "", 14, new Color32(187, 211, 180, 255));
            PlaceText(title.rectTransform, new Vector2(78, -42), new Vector2(210, 23));
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.enableAutoSizing = true;
            title.fontSizeMin = 10;
            title.fontSizeMax = 14;

            var stats = LabUI.Row(LabUI.Root, "რესურსები", 10);
            Anchor((RectTransform)stats.transform, new Vector2(1, 1), new Vector2(-16, -14), new Vector2(570, 68));
            stats.childForceExpandWidth = true;
            coins = Stat(stats.transform, LabIcon.Coin, LabUI.Gold, "ქიმქოინები");
            streak = Stat(stats.transform, LabIcon.Flame, LabUI.Coral, "დღიური სერია");
            combo = Stat(stats.transform, LabIcon.Flask, LabUI.Mint, "სწორი პასუხების სერია");

            var tracker = LabUI.Card(LabUI.Root, "აქტიური მიზანი", LabUI.Paper, 18);
            Anchor(tracker.rectTransform, new Vector2(0, 1), new Vector2(16, -82), new Vector2(300, 174));
            LabUI.Shadow(tracker);
            taskTitle = LabUI.Text(tracker.transform, "", 15, LabUI.Green);
            taskTitle.fontStyle = FontStyles.Bold;
            taskTitle.textWrappingMode = TextWrappingModes.NoWrap;
            taskTitle.enableAutoSizing = true;
            taskTitle.fontSizeMin = 10;
            taskTitle.fontSizeMax = 15;
            LabUI.Size(taskTitle.gameObject, 22);
            objective = LabUI.Text(tracker.transform, "", 18);
            objective.enableAutoSizing = true;
            objective.fontSizeMin = 14;
            objective.fontSizeMax = 18;
            LabUI.Size(objective.gameObject, 66);
            progress = LabUI.Progress(tracker.transform, 0, LabUI.Mint);
            var clue = LabUI.Text(tracker.transform, "[WASD] მოძრაობა   [E] მოქმედება", 13, LabUI.Muted);
            LabUI.Size(clue.gameObject, 18);

            var dock = LabUI.Panel(LabUI.Root, "მენიუს პანელი", LabUI.Navy);
            Anchor(dock.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(1250, 66));
            LabUI.Shadow(dock);
            var row = LabUI.Row(dock.transform, "ნავიგაცია", 8);
            LabUI.Fill((RectTransform)row.transform, 10);
            DockButton(row.transform, LabIcon.Check, "გაკვეთილები", "M", 9, LabUI.Gold);
            DockButton(row.transform, LabIcon.Atom, "ელემენტები", "Tab", 0, LabUI.Mint);
            DockButton(row.transform, LabIcon.Book, "რვეული", "N", 1, LabUI.Paper);
            DockButton(row.transform, LabIcon.Coat, "მაღაზია", "B", 2, LabUI.Coral);
            DockButton(row.transform, LabIcon.Trophy, "რეიტინგი", "", 3, LabUI.Gold);
            DockButton(row.transform, LabIcon.Quest, "დავალებები", "", 4, LabUI.Paper);
            DockButton(row.transform, LabIcon.Flask, "ჩემი ლაბი", "L", 5, LabUI.Mint);
            DockButton(row.transform, LabIcon.Book, "კვლევები", "R", 6, LabUI.Coral);
            DockButton(row.transform, LabIcon.Atom, "კოლექცია", "C", 7, LabUI.Gold);
            DockButton(row.transform, LabIcon.Quest, "დაფა", "J", 8, LabUI.Mint);

            toastCard = LabUI.Panel(LabUI.Root, "შეტყობინება", LabUI.Navy);
            Anchor(toastCard.rectTransform, new Vector2(0.5f, 1), new Vector2(-135, -24), new Vector2(340, 88));
            LabUI.Shadow(toastCard);
            toast = LabUI.Text(toastCard.transform, "", 16, LabUI.Paper);
            toast.enableAutoSizing = true;
            toast.fontSizeMin = 12;
            toast.fontSizeMax = 16;
            LabUI.Fill(toast.rectTransform, 16);
            toast.alignment = TextAlignmentOptions.Center;
            toastCard.gameObject.SetActive(false);

            waypointCard = LabUI.Panel(LabUI.Root, "მიზნის მაჩვენებელი", LabUI.Green);
            // Isolate the moving marker so it cannot rebuild every static HUD graphic.
            waypointCard.gameObject.AddComponent<Canvas>();
            waypointCard.rectTransform.sizeDelta = new Vector2(235, 44);
            waypoint = LabUI.Text(waypointCard.transform, "", 18, Color.white);
            waypoint.alignment = TextAlignmentOptions.Center;
            LabUI.Fill(waypoint.rectTransform, 7);
            waypointCard.raycastTarget = false;
            panels["identity"] = Compact(identity.rectTransform);
            panels["stats"] = Compact((RectTransform)stats.transform);
            panels["tracker"] = Compact(tracker.rectTransform);
            panels["dock"] = Compact(dock.rectTransform);
            toastCard.rectTransform.localScale = Vector3.one * 0.85f;
            GameSaveManager.Instance.Changed += Refresh;
            game.Tutorial.Changed += Refresh;
            displayedCoins = GameSaveManager.Instance.Data.chemCoins;
            Refresh();
        }

        private TMP_Text Stat(Transform parent, LabIcon kind, Color accent, string label)
        {
            var card = LabUI.Panel(parent, label, LabUI.Navy);
            LabUI.Size(card.gameObject, 68, -1, 1);
            var badge = LabUI.Panel(card.transform, "ნიშნის ფონი", new Color32(61, 83, 65, 255));
            Anchor(badge.rectTransform, new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(40, 44));
            var icon = LabUI.Icon(badge.transform, kind, accent);
            Anchor(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(32, 32));
            var caption = LabUI.Text(card.transform, label, 12, new Color32(187, 211, 180, 255));
            caption.enableAutoSizing = true;
            caption.fontSizeMin = 10;
            caption.fontSizeMax = 12;
            caption.textWrappingMode = TextWrappingModes.NoWrap;
            PlaceText(caption.rectTransform, new Vector2(50, -10), new Vector2(125, 22));
            var value = LabUI.Text(card.transform, "", 23, Color.white);
            PlaceText(value.rectTransform, new Vector2(50, -32), new Vector2(125, 28));
            value.fontStyle = FontStyles.Bold;
            return value;
        }

        private void DockButton(Transform parent, LabIcon icon, string label, string key, int menu, Color accent)
        {
            var button = LabUI.Button(parent, "", () => game.OpenMenu(menu), 50);
            button.GetComponent<Image>().color = new Color32(52, 82, 65, 255);
            var graphic = LabUI.Icon(button.transform, icon, accent);
            Anchor(graphic.rectTransform, new Vector2(0, 0.5f), new Vector2(7, 0), new Vector2(32, 32));
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = label + (key.Length > 0 ? "\n<size=11><color=#BBD3B4>[" + key + "]</color></size>" : "");
            text.fontSize = text.fontSizeMax = 15;
            // Ten buttons share the dock: shrink a label rather than break a word.
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.fontSizeMin = 9;
            text.rectTransform.offsetMin = new Vector2(39, 5);
            text.rectTransform.offsetMax = new Vector2(-7, -5);
        }

        private void Update()
        {
            if (lifeSim != null && Time.unscaledTime > lifeSimRefresh)
            {
                lifeSimRefresh = Time.unscaledTime + 1;
                string place = game.Personal != null && game.Personal.Inside ? "პირადი ლაბორატორია • " + PersonalDecor.CalmName
                    : game.Areas != null ? SchoolAreas.NameOf(game.Areas.Current) : "";
                lifeSim.text = SchoolCalendar.PartName + " • " + SchoolCalendar.WeatherName + " • " + place + " • " + LabProgress.ReputationTitle;
            }
            displayedCoins = Mathf.MoveTowards(displayedCoins, targetCoins, Time.unscaledDeltaTime * Mathf.Max(25, Mathf.Abs(targetCoins - displayedCoins) * 8));
            int roundedCoins = Mathf.RoundToInt(displayedCoins);
            if (visibleCoins != roundedCoins) { visibleCoins = roundedCoins; coins.SetText("{0}", roundedCoins); }
            toastCard.gameObject.SetActive(Time.unscaledTime < toastUntil);
            bool show = game.Tutorial.Step != TutorialManager.TutorialStep.Completed && !LabUI.AnyOpen;
            waypointCard.gameObject.SetActive(show);
            if (!show) return;
            Vector3 point = worldCamera.WorldToViewportPoint(game.Tutorial.Target);
            Vector2 size = ((RectTransform)LabUI.Root).rect.size;
            var position = new Vector2(Mathf.Clamp(point.x, 0.1f, 0.9f) * size.x - size.x / 2,
                Mathf.Clamp(point.y, 0.20f, 0.80f) * size.y - size.y / 2);
            waypointCard.rectTransform.anchoredPosition = position + Vector2.up * Mathf.Sin(Time.unscaledTime * 3) * 4;
            string arrow = point.x > 1 ? " →" : point.x < 0 ? " ←" : point.y > 1 ? " ↑" : " ↓";
            waypoint.text = (game.Tutorial.Step == TutorialManager.TutorialStep.CollectCoat ? "დამცავი ხალათი" : "რვეული") + arrow;
        }
        private readonly System.Collections.Generic.Dictionary<string, RectTransform> panels = new System.Collections.Generic.Dictionary<string, RectTransform>();
        private Image highlight;
        private RectTransform highlighted;
        public const float Scale = 0.78f;

        /// <summary>Shrinks a HUD panel toward its corner and lets it fade when the player walks behind it.</summary>
        private RectTransform Compact(RectTransform rect)
        {
            rect.localScale = Vector3.one * Scale;
            rect.gameObject.AddComponent<HudFade>().Initialize(game);
            return rect;
        }

        /// <summary>Pulses a frame around a HUD panel ("stats", "tracker", "dock", "status"); "" clears it.</summary>
        public void Highlight(string id)
        {
            highlighted = id != null && panels.TryGetValue(id, out var rect) ? rect : null;
            if (highlight == null)
            {
                highlight = LabUI.Panel(LabUI.Root, "ჩვენება", LabUI.Gold);
                highlight.raycastTarget = false;
                highlight.gameObject.AddComponent<Canvas>().overrideSorting = true;
                highlight.GetComponent<Canvas>().sortingOrder = 60;
            }
            highlight.gameObject.SetActive(highlighted != null);
        }

        private void LateUpdate()
        {
            if (highlight == null || highlighted == null) return;
            var corners = new Vector3[4];
            highlighted.GetWorldCorners(corners);
            var root = (RectTransform)LabUI.Root;
            Vector2 min = root.InverseTransformPoint(corners[0]), max = root.InverseTransformPoint(corners[2]);
            var rect = highlight.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = max - min + Vector2.one * 14;
            rect.anchoredPosition = (min + max) / 2;
            highlight.color = new Color(LabUI.Gold.r, LabUI.Gold.g, LabUI.Gold.b, 0.25f + 0.25f * Mathf.Sin(Time.unscaledTime * 6));
            highlight.transform.SetAsLastSibling();
        }

        private TMP_Text lifeSim;
        private float lifeSimRefresh;
        /// <summary>Small status line: time of day, weather, place, reputation and the calm of the lab.</summary>
        public void AddLifeSimPanel()
        {
            var card = LabUI.Panel(LabUI.Root, "დღის მდგომარეობა", LabUI.Navy);
            Anchor(card.rectTransform, new Vector2(1, 1), new Vector2(-16, -74), new Vector2(560, 32));
            panels["status"] = Compact(card.rectTransform);
            lifeSim = LabUI.Text(card.transform, "", 14, LabUI.Paper);
            lifeSim.alignment = TextAlignmentOptions.Center;
            lifeSim.enableAutoSizing = true; lifeSim.fontSizeMin = 10; lifeSim.fontSizeMax = 14;
            lifeSim.textWrappingMode = TextWrappingModes.NoWrap;
            LabUI.Fill(lifeSim.rectTransform, 6);
        }

        public void Notify(string message)
        {
            toastCard.transform.SetAsLastSibling();
            toast.text = message;
            toastUntil = Time.unscaledTime + 5;
        }
        public void Refresh()
        {
            var save = GameSaveManager.Instance;
            targetCoins = save.Data.chemCoins;
            streak.text = save.Data.currentDailyStreak + " დღე";
            combo.text = save.Combo >= 3 ? "×" + save.ComboMultiplier.ToString("0.0", CultureInfo.InvariantCulture) : save.Combo.ToString();
            title.text = game.Rankings.Title;
            bool tutorial = game.Tutorial.Step != TutorialManager.TutorialStep.Completed;
            bool learning = !tutorial && !Curriculum.Complete;
            taskTitle.text = tutorial ? "პირველი დღე ლაბორატორიაში" : learning ? "სწავლის გზა • " + Curriculum.Percent + "%" : "დღის მიზანი";
            objective.text = tutorial ? game.Tutorial.Objective : learning ? Curriculum.NextAction : "ამოხსენი 3 დავალება და მიიღე 10 ქიმქოინი.";
            if (game.Personal != null && game.Personal.Inside)
            {
                taskTitle.text = "შენი ლაბორატორია";
                objective.text = "[L] მაღაზია და მოწყობა\n[E] ცდის დაწყება\nზედა კარი — კლასში დაბრუნება";
            }
            float amount = tutorial ? (game.Tutorial.Step == TutorialManager.TutorialStep.ReadNotebook ? 0.5f : 0.1f)
                : learning ? Curriculum.Percent / 100f : Mathf.Clamp01(save.Data.dailyQuizzesSolved / 3f);
            progress.rectTransform.anchorMax = new Vector2(amount, 1);
        }
        private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
        private static void PlaceText(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
        private void OnDestroy()
        {
            if (GameSaveManager.Instance != null) GameSaveManager.Instance.Changed -= Refresh;
            if (game != null && game.Tutorial != null) game.Tutorial.Changed -= Refresh;
        }
    }

    /// <summary>Makes a HUD panel see-through while the player (or what the camera is showing) is behind it.</summary>
    public sealed class HudFade : MonoBehaviour
    {
        private LabGame game;
        private CanvasGroup group;
        private RectTransform rect;
        private readonly Vector3[] corners = new Vector3[4];
        public void Initialize(LabGame lab)
        {
            game = lab;
            rect = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        }
        private void LateUpdate()
        {
            var camera = Camera.main;
            if (camera == null || game.player == null) return;
            rect.GetWorldCorners(corners);
            var panel = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
            Vector3 feet = game.player.transform.position;
            Vector2 a = camera.WorldToScreenPoint(feet + new Vector3(-0.7f, -0.2f)), b = camera.WorldToScreenPoint(feet + new Vector3(0.7f, 2.9f));
            var body = Rect.MinMaxRect(a.x, a.y, b.x, b.y);
            float target = panel.Overlaps(body) ? 0.22f : 1f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 4);
        }
    }
}
