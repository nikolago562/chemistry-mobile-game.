using System.Linq;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class GameHUD : MonoBehaviour
    {
        private TMP_Text coins, streak, combo, title, objective, taskTitle, toast, waypoint, xp;
        private Image ring;
        private int seenXP = -1;
        private Image progress;
        private Image toastCard;
        private Image waypointCard;
        private LabGame game;
        private float toastUntil;
        private float displayedCoins;
        private int targetCoins;
        private int visibleCoins = -1;
        private Camera worldCamera;
        private RectTransform statsRect;

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
            xp = Stat(stats.transform, LabIcon.Quest, LabUI.Mint, "დღის მიზანი");
            GoalRing((Image)xp.transform.parent.GetComponent<Image>());
            streak = Stat(stats.transform, LabIcon.Flame, LabUI.Coral, "დღიური სერია");
            combo = Stat(stats.transform, LabIcon.Flask, LabUI.Mint, "სწორი პასუხების სერია");

            stats.childForceExpandWidth = false;
            statsRect = (RectTransform)stats.transform;
            // One objective and one progress bar; menus live in the world, so there is no dock.
            var tracker = LabUI.Card(LabUI.Root, "აქტიური მიზანი", LabUI.Paper, 14);
            Anchor(tracker.rectTransform, new Vector2(0, 1), new Vector2(16, -82), new Vector2(300, 112));
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
            LabUI.Size(objective.gameObject, 44);
            progress = LabUI.Progress(tracker.transform, 0, LabUI.Mint);

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
            waypointCard.rectTransform.sizeDelta = new Vector2(CompactWidth, 44);
            waypointGroup = waypointCard.gameObject.AddComponent<CanvasGroup>();
            waypointGroup.alpha = 0;
            waypointGroup.blocksRaycasts = false;
            // Label on the left (only in the wide card), a pointer on the right that turns toward the target.
            waypoint = LabUI.Text(waypointCard.transform, "", 18, Color.white);
            waypoint.alignment = TextAlignmentOptions.Center;
            waypoint.textWrappingMode = TextWrappingModes.NoWrap;
            LabUI.Fill(waypoint.rectTransform, 7);
            waypoint.rectTransform.offsetMax = new Vector2(-40, -7);
            waypoint.alpha = 0;
            pointer = LabUI.Text(waypointCard.transform, "→", 26, LabUI.Gold);
            pointer.alignment = TextAlignmentOptions.Center;
            pointer.fontStyle = FontStyles.Bold;
            // Anchored to the card's right edge but pivoting on its own centre, so it turns in place
            // (CompactWidth / 2 from the edge = the middle of the small arrow-only tile).
            Anchor(pointer.rectTransform, new Vector2(1, 0.5f), new Vector2(-CompactWidth / 2, 0), new Vector2(40, 40));
            pointer.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            waypointCard.raycastTarget = false;
            waypointCard.gameObject.SetActive(false);
            panels["identity"] = Compact(identity.rectTransform);
            panels["stats"] = Compact((RectTransform)stats.transform);
            panels["tracker"] = Compact(tracker.rectTransform);
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

        /// <summary>A ring around the goal icon fills with today's XP; clicking the card picks the goal.</summary>
        private void GoalRing(Image card)
        {
            var badge = card.transform.GetChild(0);
            var sprite = PixelArt.Make(48, 48, (x, y) =>
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(23.5f, 23.5f));
                return d <= 23.5f && d >= 19.5f ? Color.white : Color.clear;
            });
            foreach (bool track in new[] { true, false })
            {
                var image = LabUI.Panel(badge, track ? "რგოლის ფონი" : "მიზნის რგოლი", track ? new Color(1, 1, 1, 0.15f) : LabUI.Mint);
                image.sprite = sprite; image.raycastTarget = false;
                Anchor(image.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(42, 42));
                if (track) continue;
                image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Radial360;
                image.fillOrigin = (int)Image.Origin360.Top; image.fillClockwise = true;
                ring = image;
            }
            card.raycastTarget = true;
            card.gameObject.AddComponent<Button>().onClick.AddListener(PickGoal);
        }

        public void PickGoal()
        {
            if (LabUI.AnyOpen) return;
            var save = GameSaveManager.Instance;
            var window = LabUI.Window("დღის მიზანი", 600, 400);
            var intro = LabUI.Text(window.Body, "რამდენი XP გინდა ყოველდღე? XP-ს იღებ გაკვეთილებით, ვარჯიშით, ტესტებითა და აღმოჩენებით.", 17, LabUI.Ink);
            LabUI.Size(intro.gameObject, 56);
            string[] names = { "მსუბუქი", "ჩვეულებრივი", "სერიოზული" };
            for (int i = 0; i < names.Length; i++)
            {
                int goal = GameSaveManager.XpGoals[i];
                string label = names[i] + "  •  " + goal + " XP დღეში";
                System.Action pick = () => { save.Data.xpGoal = goal; save.Commit(); Refresh(); window.Close(); Notify("დღის მიზანი: " + goal + " XP."); };
                if (goal == save.Data.xpGoal) LabUI.Button(window.Body, label, pick, 54); else LabUI.Secondary(window.Body, label, pick, 54);
            }
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
            // Mission and lesson lookups run five times a second; only the card's position follows every frame.
            if (Time.unscaledTime >= nextWaypoint)
            {
                nextWaypoint = Time.unscaledTime + 0.2f;
                bool tutorial = game.Tutorial.Step != TutorialManager.TutorialStep.Completed;
                // After the tutorial the arrow keeps pointing at the station of the next experiment, until you are there.
                bool inClass = game.Areas != null && game.Areas.Current == "classroom" && game.Personal != null && !game.Personal.Inside;
                // A running mission step points at its own target; otherwise the tutorial or next lesson.
                waypointTarget = Missions.Target ?? game.Tutorial.Target;
                bool mission = Missions.Steering;
                waypointWanted = tutorial || (mission || Curriculum.Next != null) && inClass &&
                    Vector2.Distance(game.player.transform.position, waypointTarget) > 4;
                waypointLabel = game.Tutorial.Step == TutorialManager.TutorialStep.CollectCoat ? "დამცავი ხალათი" : mission ? "მისია" : tutorial ? "პირველი ცდა" : "ახალი ცდა";
            }
            // Fade in and out instead of popping.
            float dt = Time.unscaledDeltaTime;
            bool show = waypointWanted && !LabUI.AnyOpen;
            waypointGroup.alpha = Mathf.MoveTowards(waypointGroup.alpha, show ? 1 : 0, dt * 5);
            bool visible = waypointGroup.alpha > 0.001f;
            if (waypointCard.gameObject.activeSelf != visible) waypointCard.gameObject.SetActive(visible);
            if (!visible) { waypointShown = false; return; }

            Vector3 point = worldCamera.WorldToViewportPoint(waypointTarget);
            Vector3 feet = worldCamera.WorldToViewportPoint(game.player.transform.position);
            Vector2 size = ((RectTransform)LabUI.Root).rect.size;
            // Kept inside the middle of the screen, clear of the top-left mission card and top-right coins.
            var goal = new Vector2(Mathf.Clamp(point.x, 0.08f, 0.92f) * size.x - size.x / 2,
                Mathf.Clamp(point.y, 0.12f, 0.70f) * size.y - size.y / 2);
            waypointPosition = waypointShown ? Vector2.Lerp(waypointPosition, goal, 1 - Mathf.Exp(-8 * dt)) : goal;
            waypointCard.rectTransform.anchoredPosition = waypointPosition + Vector2.up * Mathf.Sin(Time.unscaledTime * 3) * 3;

            // The pointer turns smoothly toward the target, seen from the player.
            float wanted = Mathf.Atan2((point.y - feet.y) * size.y, (point.x - feet.x) * size.x) * Mathf.Rad2Deg;
            pointerAngle = waypointShown ? Mathf.LerpAngle(pointerAngle, wanted, 1 - Mathf.Exp(-10 * dt)) : wanted;
            pointer.rectTransform.localRotation = Quaternion.Euler(0, 0, pointerAngle);

            // Far away: just the arrow. Closer: it widens into the labelled card (with hysteresis so it doesn't flicker).
            float distance = Vector2.Distance(game.player.transform.position, waypointTarget);
            waypointWide = distance < (waypointWide ? 10f : 8.5f);
            waypointBlend = Mathf.MoveTowards(waypointBlend, waypointWide ? 1 : 0, dt * 3);
            float s = Mathf.SmoothStep(0, 1, waypointBlend);
            waypointCard.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(CompactWidth, WideWidth, s), 44);
            waypoint.alpha = Mathf.Clamp01(s * 2 - 1);
            if (!ReferenceEquals(waypointLabel, shownLabel)) { shownLabel = waypointLabel; waypoint.text = waypointLabel; }
            waypointShown = true;
        }
        private const float CompactWidth = 48, WideWidth = 200;
        private float nextWaypoint, pointerAngle, waypointBlend;
        private bool waypointWanted, waypointShown, waypointWide;
        private Vector2 waypointPosition;
        private Vector3 waypointTarget;
        private string waypointLabel, shownLabel;
        private TMP_Text pointer;
        private CanvasGroup waypointGroup;
        private readonly System.Collections.Generic.Dictionary<string, RectTransform> panels = new System.Collections.Generic.Dictionary<string, RectTransform>();
        private Image highlight;
        private RectTransform highlighted;
        public const float Scale = 0.78f;
        /// <summary>The objective card, for the tutorial pointer.</summary>
        public RectTransform Tracker => panels.TryGetValue("tracker", out var rect) ? rect : null;

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
            int daily = save.Data.dailyXP, goal = Mathf.Max(1, save.Data.xpGoal);
            xp.text = daily + " / " + goal + " XP";
            ring.fillAmount = Mathf.Clamp01(daily / (float)goal);
            ring.color = daily >= goal ? LabUI.Gold : LabUI.Mint;
            if (seenXP >= 0 && seenXP < goal && daily >= goal)
            {
                Notify("დღის მიზანი შესრულდა! " + goal + " XP • სერია: " + save.Data.currentDailyStreak + " დღე");
                AudioManager.Instance?.Milestone();
            }
            seenXP = daily;
            combo.text = save.Combo >= 3 ? "×" + save.ComboMultiplier.ToString("0.0", CultureInfo.InvariantCulture) : save.Combo.ToString();
            title.text = game.Rankings.Title;
            // Streak and combo only matter on the advanced floor; a beginner sees just the coins.
            bool advanced = Curriculum.Complete;
            streak.transform.parent.gameObject.SetActive(advanced);
            combo.transform.parent.gameObject.SetActive(advanced);
            statsRect.sizeDelta = new Vector2(advanced ? 760 : 380, statsRect.sizeDelta.y);
            bool tutorial = game.Tutorial.Step != TutorialManager.TutorialStep.Completed;
            int found = Mixing.Results.Count(r => LabProgress.Owns(r.ID));
            taskTitle.text = tutorial ? "პირველი დღე ლაბორატორიაში" : "შეკვეთები " + (Orders.PerDay - Orders.Left) + "/" + Orders.PerDay + " • აღმოჩენები " + found + "/" + Mixing.Results.Length;
            objective.text = game.Tutorial.Objective;
            float amount = tutorial ? 0.1f : (Orders.PerDay - Orders.Left) / (float)Orders.PerDay;
            if (Missions.Steering)
            {
                taskTitle.text = "მისია • " + Missions.Current.Title;
                objective.text = Missions.Step.Text();
                amount = (Missions.StepIndex + 0.15f) / Missions.Current.Steps.Length;
            }
            else if (game.Personal != null && game.Personal.Inside)
            {
                taskTitle.text = "შენი ლაბორატორია";
                objective.text = "მიდი მბზინავ ხელსაწყოსთან და სცადე ცდა.";
            }
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
