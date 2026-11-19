using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    internal static class Ui
    {
        public static void Clear(Transform root) { foreach (Transform child in root) { child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject); } }
        public static TMP_Text Line(Transform parent, string text, float size, Color color, float height, bool bold = false)
        {
            var label = LabUI.Text(parent, text, size, color);
            if (bold) label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = true; label.fontSizeMin = Mathf.Min(12, size); label.fontSizeMax = size;
            LabUI.Size(label.gameObject, height);
            return label;
        }
        public static Button Tab(Transform parent, string text, bool active, Action pressed)
        {
            var button = active ? LabUI.Button(parent, text, pressed, 40) : LabUI.Secondary(parent, text, pressed, 40);
            LabUI.Size(button.gameObject, 40, -1, 1);
            return button;
        }
    }

    // =====================================================================
    /// <summary>Research board: accept projects, see every stage and push the current one forward.</summary>
    public sealed class ResearchUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private string selected;
        private TMP_Text status;
        public bool IsOpen => window != null;
        public void Initialize(LabGame lab) => game = lab;
        public void Toggle() { if (window != null) window.Close(); else Open(); }

        public void Open(string project = null)
        {
            if (project != null) selected = project;
            selected ??= ResearchProjects.Active.FirstOrDefault()?.id ?? ResearchProjects.All[0].ID;
            if (window == null)
            {
                window = LabUI.Window("კვლევების დაფა", 1120, 650);
                window.Closed = () => window = null;
            }
            Render();
        }

        private void Render()
        {
            Ui.Clear(window.Body);
            var split = LabUI.Row(window.Body, "კვლევები", 18); LabUI.FlexibleHeight(split.gameObject);
            var list = LabUI.Card(split.transform, "პროექტები", LabUI.Soft, 12); LabUI.Size(list.gameObject, 480, 330, 0);
            Ui.Line(list.transform, "კვლევითი პროექტები", 19, LabUI.Ink, 28, true);
            var scroll = LabUI.Scroll(list.transform);
            foreach (var project in ResearchProjects.All)
            {
                var state = ResearchProjects.State(project.ID);
                bool unlocked = ResearchProjects.Unlocked(project);
                string tag = !unlocked ? "დაკეტილია" : state == null ? "ახალი" : !state.completed ? "მიმდინარე • " + (state.stage + 1) + "/" + project.Stages.Length : "დასრულებულია ×" + state.timesCompleted;
                var id = project.ID;
                var button = LabUI.Button(scroll, project.Title + "\n<size=13>" + tag + "</size>", () => { selected = id; Render(); }, 56);
                button.GetComponent<Image>().color = selected == id ? Color.Lerp(project.Color, LabUI.Ink, 0.45f) : unlocked ? LabUI.Navy : LabUI.Muted;
            }
            var detail = LabUI.Column(split.transform, "დეტალები", 8); LabUI.Size(detail.gameObject, 480, -1, 1);
            var p = ResearchProjects.Get(selected);
            var s = ResearchProjects.State(selected);
            var title = Ui.Line(detail.transform, p.Title, 26, Color.Lerp(p.Color, LabUI.Ink, 0.55f), 36, true);
            Ui.Line(detail.transform, p.Summary, 16, LabUI.Ink, 50);
            Ui.Line(detail.transform, "ჯილდო: " + p.Coins + " ქიმქოინი • " + p.Reputation + " რეპუტაცია" + (SchoolCalendar.ThemeProject == p.ID ? " • თვის თემა: ბონუსი!" : "") +
                "  •  ოსტატობა: " + LabProgress.Stars(LabProgress.MasteryLevel("project:" + p.ID)), 15, LabUI.Muted, 24);
            var stages = LabUI.Card(detail.transform, "ეტაპები", LabUI.Paper, 10); LabUI.FlexibleHeight(stages.gameObject);
            for (int i = 0; i < p.Stages.Length; i++)
            {
                var stage = p.Stages[i];
                bool done = s != null && (s.completed || i < s.stage);
                bool isCurrent = s != null && !s.completed && i == s.stage;
                string text = (i + 1) + ". " + stage.Title + ": " + stage.Text;
                var line = Ui.Line(stages.transform, done ? "<s>" + text + "</s>" : isCurrent ? "→ " + text : text, 14, done ? LabUI.Muted : isCurrent ? LabUI.Coral : LabUI.Ink, 22, isCurrent);
            }
            status = Ui.Line(detail.transform, "", 16, LabUI.Green, 40);
            var actions = LabUI.Row(detail.transform, "მოქმედებები", 8); LabUI.Size(actions.gameObject, 48);
            if (!ResearchProjects.Unlocked(p))
            {
                status.text = "იხსნება რეპუტაციის დონეზე: " + LabProgress.ReputationTitles[p.ReputationLevel] + ". შეასრულე სხვა პროექტები და დაეხმარე კლასელებს.";
                return;
            }
            if (s == null || s.completed)
            {
                if (s != null) status.text = "დასრულებულია! შედეგი შეგიძლია სამეცნიერო ბაზრობაზე წარადგინო ან ოსტატობისთვის თავიდან ჩაატარო.";
                Button(actions.transform, s == null ? "პროექტის აღება" : "ხელახლა დაწყება", () =>
                {
                    if (ResearchProjects.Accept(p.ID)) { game.HUD.Notify("ახალი კვლევა: " + p.Title); AudioManager.Instance?.Milestone(); }
                    Render();
                });
                return;
            }
            var current = ResearchProjects.Stage(s);
            status.text = ResearchProjects.Requirement(s) + (s.mistakes > 0 ? "  •  შეცდომები: " + s.mistakes + " (ესეც სწავლაა!)" : "");
            switch (current.Kind)
            {
                case StageKind.Gather:
                case StageKind.Equipment:
                case StageKind.Wait:
                    Button(actions.transform, current.Kind == StageKind.Gather ? "მასალის გამოყენება" : current.Kind == StageKind.Equipment ? "შემოწმება" : "შედეგის შემოწმება", () =>
                    {
                        bool ok = ResearchProjects.TryAdvance(s, out var message);
                        Render(); status.text = message; status.color = ok ? LabUI.Green : LabUI.Coral;
                    });
                    if (current.Kind == StageKind.Equipment && game.Personal.Inside)
                        Button(actions.transform, "ლაბორატორიის მოწყობა", () => { window.Close(); game.Personal.Workshop.Open(); }, true);
                    break;
                case StageKind.Experiment:
                    Button(actions.transform, "ცდის დაწყება", () =>
                    {
                        if (!game.Personal.Inside) { status.text = "ცდა შენს პირად ლაბორატორიაში ჩაატარე: ზედა კარი კლასში."; status.color = LabUI.Coral; return; }
                        window.Close(); game.Games.Open(current.Target, p.ID);
                    });
                    break;
                case StageKind.Observe:
                    Ui.Line(detail.transform, current.Text, 16, LabUI.Ink, 30, true);
                    for (int i = 0; i < current.Options.Length; i++)
                    {
                        int option = i;
                        var answer = LabUI.Secondary(detail.transform, current.Options[i], () =>
                        {
                            bool right = ResearchProjects.Answer(s, option);
                            Render();
                            status.text = (right ? "სწორია! " : "არა ზუსტად, მაგრამ შეცდომა სწავლის ნაწილია. ") + current.Lesson;
                            status.color = right ? LabUI.Green : LabUI.Coral;
                            if (right) AudioManager.Instance?.Correct(); else AudioManager.Instance?.Wrong();
                        }, 38);
                        LabUI.Size(answer.gameObject, 38);
                    }
                    break;
                case StageKind.Submit:
                    status.text = "მზადაა! წაიღე შედეგი მასწავლებელთან ან გამოფენის დარბაზის ბაზრობის სტენდთან.";
                    break;
            }
        }

        private static void Button(Transform parent, string text, Action pressed, bool secondary = false)
        {
            var button = secondary ? LabUI.Secondary(parent, text, pressed, 46) : LabUI.Button(parent, text, pressed, 46);
            LabUI.Size(button.gameObject, 46, -1, 1);
        }
    }

    // =====================================================================
    /// <summary>Collections, materials and long-term progress in one journal.</summary>
    public sealed class CollectionUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private string tab = "Mineral";
        public bool IsOpen => window != null;
        public void Initialize(LabGame lab) => game = lab;
        public void Toggle() { if (window != null) window.Close(); else Open(CollectionKind.Mineral); }

        private static readonly (string id, string name)[] Tabs =
        {
            ("Mineral", "მინერალები"), ("Sample", "ნიმუშები"), ("Photo", "ფოტოები"), ("Reaction", "რეაქციები"), ("Fact", "ფაქტები"), ("Badge", "ნიშნები"),
            ("FurnitureSet", "ავეჯის ნაკრები"), ("Seasonal", "სეზონური"), ("Award", "ჯილდოები"), ("materials", "მასალები"), ("progress", "პროგრესი"),
        };

        public void Open(CollectionKind kind) => OpenTab(kind.ToString());
        public void OpenTab(string id)
        {
            tab = id;
            if (window == null) { window = LabUI.Window("კოლექცია და პროგრესი", 1120, 650); window.Closed = () => window = null; }
            Render();
        }

        private void Render()
        {
            Ui.Clear(window.Body);
            for (int row = 0; row < 2; row++)
            {
                var tabs = LabUI.Row(window.Body, "ჩანართები", 6); LabUI.Size(tabs.gameObject, 40);
                foreach (var t in Tabs.Skip(row * 6).Take(6)) { var id = t.id; Ui.Tab(tabs.transform, t.name, tab == id, () => { tab = id; Render(); }); }
            }
            var content = LabUI.Scroll(window.Body);
            if (tab == "progress") { Progress(content); return; }
            if (tab == "materials") { Materials(content); return; }
            var kind = (CollectionKind)Enum.Parse(typeof(CollectionKind), tab);
            if (kind == CollectionKind.Reaction) { Reactions(content); return; }
            var items = LabProgress.Items(kind).ToList();
            Ui.Line(content, "შეგროვებულია: " + items.Count(i => LabProgress.Owns(i.ID)) + " / " + items.Count, 17, LabUI.Muted, 26);
            foreach (var item in items) Card(content, item, LabProgress.Owns(item.ID));
        }

        private static void Card(Transform parent, CollectionItem item, bool owned)
        {
            var card = LabUI.Card(parent, item.Name, owned ? LabUI.Soft : new Color(0.9f, 0.9f, 0.88f), 10);
            var row = LabUI.Row(card.transform, "ჩანაწერი", 12); LabUI.Size(row.gameObject, 48);
            var swatch = LabUI.Panel(row.transform, "ფერი", owned ? item.Color : new Color(0.75f, 0.75f, 0.75f)); LabUI.Size(swatch.gameObject, 40, 40, 0);
            var text = LabUI.Text(row.transform, owned ? "<b>" + item.Name + "</b>\n<size=14>" + item.Description + "</size>" : "<b>???</b>\n<size=14>" + (item.Kind == CollectionKind.Badge || item.Kind == CollectionKind.FurnitureSet ? item.Description : "ჯერ არ გაქვს აღმოჩენილი.") + "</size>", 17, owned ? LabUI.Ink : LabUI.Muted);
            text.enableAutoSizing = true; text.fontSizeMin = 11; text.fontSizeMax = 17;
            LabUI.Size(text.gameObject, 48, -1, 1);
        }

        private void Reactions(Transform content)
        {
            var questions = game.questions.Where(q => q != null).ToList();
            Ui.Line(content, "აღმოჩენილი რეაქციები: " + questions.Count(q => LabProgress.Owns("reaction:" + q.questionID)) + " / " + questions.Count, 17, LabUI.Muted, 26);
            foreach (var q in questions)
            {
                bool owned = LabProgress.Owns("reaction:" + q.questionID);
                Card(content, new CollectionItem(CollectionKind.Reaction, q.questionID, q.equationPrompt.Replace("?", q.correctAnswer), q.CategoryName + " • " + q.hintText, LabUI.Mint), owned);
            }
        }

        private void Materials(Transform content)
        {
            Ui.Line(content, "შენი მასალები. მცენარეები სათბურშია, დანარჩენი სათავსოში.", 17, LabUI.Muted, 26);
            foreach (var m in LabProgress.Materials)
                Card(content, new CollectionItem(CollectionKind.Sample, m.ID, m.Name + " × " + LabProgress.Has(m.ID), "წყარო: " + m.Source, m.Color), true);
        }

        private void Progress(Transform content)
        {
            var data = LabProgress.Data;
            Ui.Line(content, "ლაბორატორიის რეპუტაცია: " + LabProgress.ReputationTitle + " (" + data.reputation + ")", 20, LabUI.Green, 30, true);
            LabUI.Progress(content, LabProgress.ReputationProgress, LabUI.Mint, 10);
            Ui.Line(content, "პროექტების ოსტატობა", 18, LabUI.Ink, 28, true);
            foreach (var p in ResearchProjects.All)
                Ui.Line(content, p.Title + ": " + LabProgress.Stars(LabProgress.MasteryLevel("project:" + p.ID)), 15, LabUI.Ink, 22);
            Ui.Line(content, "რეაქციის კატეგორიები", 18, LabUI.Ink, 28, true);
            string[] categories = { "შეერთება", "დაშლა", "სტექიომეტრია", "წვა", "მჟავა და ფუძე", "ელემენტის აღმოჩენა" };
            for (int i = 0; i < categories.Length; i++)
                Ui.Line(content, categories[i] + ": " + LabProgress.Stars(LabProgress.MasteryLevel("category:" + (ReactionCategory)i)), 15, LabUI.Ink, 22);
            Ui.Line(content, "აღჭურვილობის ოსტატობა", 18, LabUI.Ink, 28, true);
            foreach (var id in ExperimentGamesUI.GameIDs)
                Ui.Line(content, ExperimentGamesUI.Title(id) + ": " + LabProgress.Stars(LabProgress.MasteryLevel("equipment:" + id)), 15, LabUI.Ink, 22);
            Ui.Line(content, "მეგობრობა", 18, LabUI.Ink, 28, true);
            foreach (var c in Classmates.All)
            {
                var state = Classmates.State(c.ID);
                Ui.Line(content, c.Name + " • " + c.Role + ": " + Classmates.FriendshipNames[Classmates.Level(state)] + " (" + state.friendship + ") • ამბავი " + state.story + "/" + c.Story.Length + " • თხოვნები: " + state.requestsDone, 15, LabUI.Ink, 22);
            }
            Ui.Line(content, "სამეცნიერო ბაზრობები", 18, LabUI.Ink, 28, true);
            if (data.fairEntries.Count == 0) Ui.Line(content, "ჯერ არ მიგიღია მონაწილეობა.", 15, LabUI.Muted, 22);
            foreach (var f in data.fairEntries.OrderByDescending(f => f.week).Take(8))
                Ui.Line(content, f.week + ": " + ResearchProjects.Get(f.projectID)?.Title + " • " + f.score + " ქულა • " + f.rank + " ადგილი" + (f.settled ? "" : " (შედეგები კვირის ბოლოს)"), 15, LabUI.Ink, 22);
        }

        // ---------- storage room ----------
        public void OpenStorage()
        {
            if (window != null) window.Close();
            window = LabUI.Window("სათავსო • მასალები", 1000, 620);
            window.Closed = () => window = null;
            RenderStorage();
        }

        private void RenderStorage()
        {
            Ui.Clear(window.Body);
            var data = LabProgress.Data;
            bool picked = data.storagePickupDay == LabProgress.Today;
            Ui.Line(window.Body, picked ? "დღის მასალები უკვე აიღე. დამატებით შეგიძლია იყიდო." : "ქეთევანმა დღის მასალები მოამზადა. აიღე უფასოდ!", 18, LabUI.Green, 30);
            var pickup = LabUI.Button(window.Body, picked ? "ხვალ ახალი მიწოდება იქნება" : "დღის მასალების აღება (3 ნივთი)", () =>
            {
                data.storagePickupDay = LabProgress.Today;
                var pool = LabProgress.Materials.Where(m => m.Price > 0 && m.ID != "sample").ToList();
                var random = new System.Random(LabProgress.Today.GetHashCode());
                var got = Enumerable.Range(0, 3).Select(_ => pool[random.Next(pool.Count)]).ToList();
                foreach (var m in got) LabProgress.Give(m.ID, 1);
                GameSaveManager.Instance.Commit();
                AudioManager.Instance?.Coin();
                game.HUD.Notify("მიიღე: " + string.Join(", ", got.Select(m => m.Name)));
                RenderStorage();
            }, 48);
            pickup.interactable = !picked;
            var content = LabUI.Scroll(window.Body);
            foreach (var m in LabProgress.Materials.Where(m => m.Price > 0))
            {
                var card = LabUI.Card(content, m.Name, LabUI.Soft, 8);
                var row = LabUI.Row(card.transform, m.Name, 10); LabUI.Size(row.gameObject, 40);
                var swatch = LabUI.Panel(row.transform, "ფერი", m.Color); LabUI.Size(swatch.gameObject, 34, 34, 0);
                var text = LabUI.Text(row.transform, m.Name + "  •  გაქვს: " + LabProgress.Has(m.ID), 17); LabUI.Size(text.gameObject, 40, -1, 1);
                var material = m;
                var buy = LabUI.Button(row.transform, material.Price + " ქიმქოინი", () =>
                {
                    if (data.chemCoins < material.Price) { game.HUD.Notify("ქიმქოინები არ გყოფნის."); return; }
                    data.chemCoins -= material.Price; LabProgress.Give(material.ID, 1); GameSaveManager.Instance.Commit();
                    AudioManager.Instance?.Click(); RenderStorage();
                }, 40);
                LabUI.Size(buy.gameObject, 40, 170, 0);
            }
        }

        public void ShowFact(CollectionItem fact)
        {
            if (window != null) window.Close();
            window = LabUI.Window("ახალი ფაქტი", 720, 380);
            window.Closed = () => window = null;
            Ui.Line(window.Body, fact.Name, 28, LabUI.Green, 44, true);
            Ui.Line(window.Body, fact.Description, 21, LabUI.Ink, 140);
            LabUI.Button(window.Body, "კოლექციაში ჩაწერა", () => window.Close(), 48);
        }
    }

    // =====================================================================
    /// <summary>Noticeboard: today's events, small daily tasks, seasonal souvenirs and the science fair.</summary>
    public sealed class JournalUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private float nextCheck;
        public bool IsOpen => window != null;
        public void Initialize(LabGame lab) => game = lab;
        public void Toggle() { if (window != null) window.Close(); else Open(); }

        private void Update()
        {
            if (Time.unscaledTime < nextCheck || GameSaveManager.Instance == null) return;
            nextCheck = Time.unscaledTime + 10;
            var message = ResearchProjects.SettleFairs();
            if (message != null) game.HUD.Notify(message);
        }

        public void Open()
        {
            if (window == null) { window = LabUI.Window("განცხადებების დაფა • " + SchoolCalendar.DateText, 1080, 640); window.Closed = () => window = null; }
            Ui.Clear(window.Body);
            var data = LabProgress.Data;
            Ui.Line(window.Body, SchoolCalendar.PartName + " • " + SchoolCalendar.WeatherName + " • " + SchoolCalendar.SeasonName + " • რეპუტაცია: " + LabProgress.ReputationTitle, 17, LabUI.Muted, 26);
            var split = LabUI.Row(window.Body, "დაფა", 16); LabUI.FlexibleHeight(split.gameObject);
            var events = LabUI.Card(split.transform, "მოვლენები", LabUI.Soft, 12); LabUI.Size(events.gameObject, 430, -1, 1);
            Ui.Line(events.transform, "დღევანდელი მოვლენები", 19, LabUI.Ink, 28, true);
            var scroll = LabUI.Scroll(events.transform);
            foreach (var e in SchoolCalendar.TodayEvents())
            {
                var card = LabUI.Card(scroll, e.Title, LabUI.Paper, 8);
                Ui.Line(card.transform, "<color=#" + ColorUtility.ToHtmlStringRGB(e.Color * 0.7f) + "><b>" + e.Title + "</b></color>\n<size=14>" + e.Text + "</size>", 17, LabUI.Ink, 58);
            }
            var tasks = LabUI.Card(split.transform, "დღის საქმეები", LabUI.Soft, 12); LabUI.Size(tasks.gameObject, 430, 430, 0);
            Ui.Line(tasks.transform, "დღის პატარა საქმეები", 19, LabUI.Ink, 28, true);
            string today = LabProgress.Today;
            int talked = data.npcs.Count(n => n.lastTalkDay == today);
            Task(tasks.transform, "სათბურის მორწყვა", data.greenhouseWateredDay == today);
            Task(tasks.transform, "დღის მასალები სათავსოდან", data.storagePickupDay == today);
            Task(tasks.transform, "ესაუბრე სამ კლასელს (" + Mathf.Min(3, talked) + "/3)", talked >= 3);
            Task(tasks.transform, "ლაბორატორიის მცენარეების მორწყვა", data.labPlantsWateredDay == today || !data.labPlacements.Any(p => p.itemID == "plant" || p.itemID == "ficus"));
            Task(tasks.transform, "ავარიების დასუფთავება", data.pendingSpills == 0);
            Task(tasks.transform, "კვლევის ერთი ეტაპის წინსვლა", ResearchProjects.Active.Any(p => p.notes.Count > 0) || data.projects.Any(p => p.completed));
            var actions = LabUI.Row(tasks.transform, "ღილაკები", 8); LabUI.Size(actions.gameObject, 44);
            string seasonal = "seasonal:" + SchoolCalendar.SeasonID;
            var souvenir = LabUI.Button(actions.transform, LabProgress.Owns(seasonal) ? "სუვენირი აღებულია" : "სეზონური სუვენირი", () =>
            {
                if (LabProgress.Collect(seasonal)) { if (!data.ownedLabItems.Contains("season")) data.ownedLabItems.Add("season"); GameSaveManager.Instance.Commit(); AmbientAudio.Instance?.Sparkle(); }
                Open();
            }, 42);
            souvenir.interactable = !LabProgress.Owns(seasonal);
            LabUI.Size(souvenir.gameObject, 42, -1, 1);
            string competition = "competition:" + SchoolCalendar.Week;
            if (SchoolCalendar.IsCompetitionDay)
            {
                bool claimed = data.claimedEvents.Contains(competition);
                var claim = LabUI.Button(actions.transform, claimed ? "შეჯიბრი მოგებულია" : "შეჯიბრი: " + Mathf.Min(5, data.dailyQuizzesSolved) + "/5", () =>
                {
                    if (data.dailyQuizzesSolved < 5 || data.claimedEvents.Contains(competition)) return;
                    data.claimedEvents.Add(competition); data.chemCoins += 40; LabProgress.AddReputation(10); LabProgress.Collect("award:competition");
                    GameSaveManager.Instance.Commit(); Open();
                }, 42);
                claim.interactable = !claimed && data.dailyQuizzesSolved >= 5;
                LabUI.Size(claim.gameObject, 42, -1, 1);
            }
            var fair = LabUI.Secondary(window.Body, "სამეცნიერო ბაზრობის ცხრილი", OpenFair, 44);
            LabUI.Size(fair.gameObject, 44);
        }

        private static void Task(Transform parent, string text, bool done) =>
            Ui.Line(parent, done ? "<s>" + text + "</s>  • შესრულდა" : "•  " + text, 16, done ? LabUI.Muted : LabUI.Ink, 30);

        public void OpenFair()
        {
            if (!Curriculum.TierOpen(4)) { game.HUD.Notify(Curriculum.LockedText + " სამეცნიერო ბაზრობა რთული გამოწვევების მე-4 საფეხურზე იხსნება."); return; }
            if (window != null) window.Close();
            window = LabUI.Window("სამეცნიერო ბაზრობა • კვირა " + SchoolCalendar.Week, 980, 620);
            window.Closed = () => window = null;
            var entry = ResearchProjects.ThisWeek;
            if (entry == null)
            {
                Ui.Line(window.Body, "ამ კვირის ბაზრობაზე ერთი დასრულებული პროექტი წარადგინე. შედეგები კვირის ბოლოს ცხადდება.", 18, LabUI.Ink, 50);
                var submittable = ResearchProjects.Submittable;
                if (submittable != null)
                    LabUI.Button(window.Body, "ჯერ ჩააბარე: " + ResearchProjects.Get(submittable.id).Title, () =>
                    {
                        string reward = ResearchProjects.Submit(submittable);
                        if (reward != null) game.HUD.Notify("პროექტი ჩაბარებულია! " + reward);
                        OpenFair();
                    }, 46);
                var done = ResearchProjects.All.Where(p => ResearchProjects.State(p.ID)?.timesCompleted > 0).ToList();
                if (done.Count == 0) Ui.Line(window.Body, "ჯერ დასრულებული პროექტი არ გაქვს. კვლევების დაფა [R] გელოდება!", 17, LabUI.Coral, 40);
                foreach (var p in done)
                {
                    var project = p;
                    LabUI.Secondary(window.Body, "წარადგინე: " + p.Title + " (ოსტატობა " + LabProgress.Stars(LabProgress.MasteryLevel("project:" + p.ID)) + ")", () =>
                    {
                        var result = ResearchProjects.EnterFair(project.ID);
                        if (result != null) { AudioManager.Instance?.Milestone(); game.HUD.Notify("წარდგენილია! წინასწარი ადგილი: " + result.rank); }
                        OpenFair();
                    }, 44);
                }
                return;
            }
            Ui.Line(window.Body, "შენი პროექტი: " + ResearchProjects.Get(entry.projectID)?.Title + " • " + entry.score + " ქულა", 20, LabUI.Green, 32, true);
            var table = LabUI.Card(window.Body, "ცხრილი", LabUI.Soft, 12); LabUI.FlexibleHeight(table.gameObject);
            var rows = ResearchProjects.Rivals.Select(r => (name: r, score: ResearchProjects.RivalScore(r, entry.week))).Append(("შენ", entry.score)).OrderByDescending(r => r.Item2).ToList();
            for (int i = 0; i < rows.Count; i++)
                Ui.Line(table.transform, (i + 1) + ".  " + rows[i].Item1 + "  —  " + rows[i].Item2 + " ქულა", 19, rows[i].Item1 == "შენ" ? LabUI.Coral : LabUI.Ink, 32, rows[i].Item1 == "შენ");
            Ui.Line(window.Body, entry.settled ? "შედეგები დამტკიცებულია." : "საბოლოო შედეგები და ჯილდოები კვირის დასრულების შემდეგ. პირველი ადგილი: ოქროს მედალი, ექსკლუზიური ხალათი და თასი!", 16, LabUI.Muted, 40);
        }
    }
}
