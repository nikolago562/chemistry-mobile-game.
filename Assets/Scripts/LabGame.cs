using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class LabGame : MonoBehaviour
    {
        public ChemistryQuestionSO[] questions;
        public CosmeticItemSO[] cosmetics;
        public ElementCollectionSO elements;
        public TMP_FontAsset interfaceFont;
        public LabThemeSO visualTheme;
        public Sprite teacherSprite;
        public Material teacherMaterial;
        public Vector2 lockerPosition = new Vector2(61, 9.2f);
        /// <summary>The lesson table (chapter one is played here), right of the teacher's own desk so their prompts never overlap.</summary>
        public Vector2 teacherDeskPosition = LessonTable;
        public static readonly Vector2 LessonTable = new Vector2(40.1f, 8.7f);
        public PlayerController player;
        public Transform classroom;
        public AudioClip[] sounds;
        public TutorialManager Tutorial { get; private set; }
        public CharacterCreationUI Profile { get; private set; }
        public PersonalLab Personal { get; private set; }
        public ChemMinigameUI Quiz { get; private set; }
        public ChemChallengeUI Challenges { get; private set; }
        public GameHUD HUD { get; private set; }
        public WeeklyLeaderboardManager Rankings { get; private set; }
        public FormulaNotebookUI Notebook { get; private set; }
        public PeriodicTableUI Periodic { get; private set; }
        public CosmeticShopUI Shop { get; private set; }
        public LabBoardUI Boards { get; private set; }
        public VisualExperiment Visuals { get; private set; }
        public TeacherDialogueUI Dialogue { get; private set; }
        public static LabGame Instance { get; private set; }
        public SchoolAreas Areas { get; private set; }
        public LabAtmosphere Atmosphere { get; private set; }
        public ResearchUI Research { get; private set; }
        public CollectionUI Collection { get; private set; }
        public JournalUI Journal { get; private set; }
        public ExperimentGamesUI Games { get; private set; }
        public SafeMishaps Mishaps { get; private set; }
        public TeacherGuide Guide { get; private set; }
        public CurriculumUI Lessons { get; private set; }
        public ActOne ActOne { get; private set; }
        public readonly System.Collections.Generic.List<SchoolNPC> Classmates = new System.Collections.Generic.List<SchoolNPC>();

        private void Awake()
        {
            Instance = this;
            // The scene still stores the old spot on the teacher's own desk; the lesson table lives here now.
            teacherDeskPosition = LessonTable;
            AddRuntimeCosmetics();
            gameObject.AddComponent<GameSaveManager>();

            Rankings = gameObject.AddComponent<WeeklyLeaderboardManager>();
            LabUI.Initialize(interfaceFont, visualTheme);
            Profile = gameObject.AddComponent<CharacterCreationUI>();
            Profile.Initialize(this);
            Guide = gameObject.AddComponent<TeacherGuide>();
            Guide.Initialize(this);
            Tutorial = gameObject.AddComponent<TutorialManager>();
            Tutorial.Initialize(this);
            Notebook = gameObject.AddComponent<FormulaNotebookUI>();
            Periodic = gameObject.AddComponent<PeriodicTableUI>(); Periodic.Initialize(this);
            Shop = gameObject.AddComponent<CosmeticShopUI>(); Shop.Initialize(this);
            Boards = gameObject.AddComponent<LabBoardUI>(); Boards.Initialize(this);
            Quiz = gameObject.AddComponent<ChemMinigameUI>(); Quiz.Initialize(this);
            Challenges = gameObject.AddComponent<ChemChallengeUI>(); Challenges.Initialize(this);
            HUD = gameObject.AddComponent<GameHUD>(); HUD.Initialize(this);
            Visuals = gameObject.AddComponent<VisualExperiment>();
            Visuals.Initialize(classroom.TransformPoint(new Vector3(36.9f, 11.15f, 0)));
            var furnishings = GetComponent<ChemistryClassroom>();
            if (furnishings != null) furnishings.Build(this);
            gameObject.AddComponent<AmbientAudio>().Initialize(this);
            Atmosphere = gameObject.AddComponent<LabAtmosphere>();
            Atmosphere.Initialize(this);
            Research = gameObject.AddComponent<ResearchUI>(); Research.Initialize(this);
            Collection = gameObject.AddComponent<CollectionUI>(); Collection.Initialize(this);
            Journal = gameObject.AddComponent<JournalUI>(); Journal.Initialize(this);
            Games = gameObject.AddComponent<ExperimentGamesUI>(); Games.Initialize(this);
            Lessons = gameObject.AddComponent<CurriculumUI>(); Lessons.Initialize(this);
            player.gameObject.AddComponent<PlayerCosmetics>().Initialize(this);
            if (!GameSaveManager.Instance.Data.tutorialCompleted)
                player.transform.localPosition = new Vector3(46.5f, 6.8f, 0);

            CreateStation("დამცავი აღჭურვილობის კარადა", lockerPosition.x, lockerPosition.y, StationType.CosmeticShopStation, "[E] დამცავი აღჭურვილობა", true);
            CreateStation("მასწავლებლის მაგიდა", teacherDeskPosition.x, teacherDeskPosition.y, StationType.TutorialStation);
            // Each bench teaches one part of the curriculum; the advanced desk opens last.
            CreateStation("ატომების მაგიდა", 37.5f, 1, StationType.AtomTable);
            CreateStation("ელემენტების სადგური", 52, 1, StationType.ElementStation);
            CreateStation("რეაქციების მაგიდა", 37.5f, -7, StationType.ReactionTable);
            CreateStation("მოლეკულების მაგიდა", 52, -7, StationType.MoleculeTable);
            CreateStation("ხსნარების სადგური", 37.5f, -15, StationType.SolutionStation);
            CreateStation("მჟავა-ფუძის მაგიდა", 52, -15, StationType.AcidBaseStation);
            CreateStation("რთული გამოწვევების მაგიდა", 37.5f, -22, StationType.QuizDesk);
            CreateStation("კოსმეტიკის თარო", 63, -10, StationType.CosmeticShopStation, "[E] ლაბორატორიის მაღაზია");
            CreateStation("კვირის რეიტინგის დაფა", 55, 8.5f, StationType.LeaderboardBoard, "[E] კვირის რეიტინგი");
            CreateTeacher();
            CreateChalkboardLettering();
            Personal = gameObject.AddComponent<PersonalLab>();
            Personal.Initialize(this);
            Areas = gameObject.AddComponent<SchoolAreas>();
            Areas.Initialize(this);
            Mishaps = gameObject.AddComponent<SafeMishaps>();
            Mishaps.Initialize(this);
            ActOne = gameObject.AddComponent<ActOne>();
            ActOne.Initialize(this);
            foreach (var classmate in ChemistryGame.Classmates.All) Classmates.Add(SchoolNPC.Spawn(this, classmate));
            HUD.AddLifeSimPanel();
            WelcomeBack();

            if (!string.IsNullOrEmpty(GameSaveManager.Instance.Status)) HUD.Notify(GameSaveManager.Instance.Status);
            if (PerfWalk.Requested) gameObject.AddComponent<PerfWalk>().Initialize(this);
        }

        private readonly System.Collections.Generic.Dictionary<StationType, Vector3> stationPoints = new System.Collections.Generic.Dictionary<StationType, Vector3>();
        /// <summary>World position of the first station of this type (where its lessons are played).</summary>
        public Vector3 StationPoint(StationType kind) => stationPoints.TryGetValue(kind, out var p) ? p : classroom.TransformPoint(teacherDeskPosition);

        private void CreateStation(string name, float x, float y, StationType kind,
            string prompt = "[E] ექსპერიმენტის დაწყება", bool closet = false)
        {
            var go = new GameObject(name, typeof(BoxCollider2D), typeof(InteractableStation));
            go.transform.SetParent(classroom, false);
            go.transform.localPosition = new Vector3(x, y);
            go.GetComponent<BoxCollider2D>().size = new Vector2(3.4f, 2.7f);
            var station = go.GetComponent<InteractableStation>();
            station.stationType = kind;
            station.stationPrompt = prompt;
            station.supplyCloset = closet;
            if (!closet && !stationPoints.ContainsKey(kind)) stationPoints[kind] = go.transform.position;
            if (kind != StationType.CosmeticShopStation && kind != StationType.TutorialStation)
            {
                station.stationPrompt = "[E] " + name;
                station.promptText = () => "[E] " + name + (Curriculum.LessonAt(kind) ? " • ახალი ცდა!" : !Curriculum.StationOpen(kind) ? " • დაკეტილია" : kind == StationType.QuizDesk ? " ★ არჩევითი" : "");
            }
            else if (kind == StationType.TutorialStation)
                station.promptText = () => Curriculum.LessonAt(kind) || Tutorial.Step == TutorialManager.TutorialStep.ReadNotebook ? "[E] ახალი ცდა!" : "[E] სასწავლო რუკა";
            if (closet)
            {
                var art = new GameObject("კარადა", typeof(SpriteRenderer));
                art.transform.SetParent(go.transform, false);
                art.transform.localPosition = Vector3.zero;
                var interaction = go.GetComponent<BoxCollider2D>();
                interaction.size = new Vector2(2.25f, 2f);
                interaction.offset = new Vector2(0, -1f);
                var blocker = new GameObject("კარადის მყარი ნაწილი", typeof(BoxCollider2D));
                blocker.transform.SetParent(go.transform, false);
                blocker.GetComponent<BoxCollider2D>().size = new Vector2(1.75f, 1.8f);
                blocker.GetComponent<BoxCollider2D>().offset = new Vector2(0, 0.9f);
                var renderer = art.GetComponent<SpriteRenderer>();
                renderer.sprite = PixelArt.Closet();
                renderer.sortingOrder = 6;
                DepthOccluders.Add(go.transform.position, 1.25f, 1.0f, 3.3f);
            }
            // After the closet art exists, so its sprite takes the highlight tint.
            station.Initialize(this);
            if (kind == StationType.ReactionTable)
                Visuals.AddApparatus(go.transform.position + new Vector3(0.8f, 3f));
        }

        private void CreateTeacher()
        {
            var npc = new GameObject("ქიმიის მასწავლებელი");
            npc.transform.SetParent(classroom, false);
            // In front of her own desk (the back-wall bench with the tubes and the colour-changing flask).
            npc.transform.localPosition = new Vector3(35.1f, 8.6f);
            // The sprite lives on a child so talking animations never scale the labels.
            var body = new GameObject("სხეული", typeof(SpriteRenderer));
            body.transform.SetParent(npc.transform, false);
            var renderer = body.GetComponent<SpriteRenderer>();
            renderer.sprite = teacherSprite != null ? teacherSprite : player.GetComponentInChildren<SpriteRenderer>().sprite;
            // Drawn over her desk (6) and its glassware (7): she stands in front of them.
            renderer.sortingOrder = 8;
            renderer.color = Color.white;
            if (teacherMaterial != null) renderer.sharedMaterial = teacherMaterial;
            // Solid feet so the player walks around her. No depth zone: she moves along the desk,
            // and the desk's own zone already covers the floor behind it.
            var feet = npc.AddComponent<BoxCollider2D>();
            feet.size = new Vector2(0.8f, 0.35f);
            feet.offset = new Vector2(0, 0.15f);

            var canvas = new GameObject("მასწავლებლის სახელი", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            canvas.transform.SetParent(npc.transform, false);
            canvas.transform.localPosition = new Vector3(0, 2.25f);
            canvas.transform.localScale = Vector3.one / 60f;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 40);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvas.GetComponent<Canvas>().sortingOrder = 11;
            var name = LabUI.Text(canvas.transform, "მასწავლებელი", 20, Color.white);
            name.alignment = TextAlignmentOptions.Center;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            LabUI.WorldLabel(name);
            LabUI.Fill(name.rectTransform);

            var markerCanvas = new GameObject("დავალების ნიშანი", typeof(RectTransform), typeof(Canvas));
            markerCanvas.transform.SetParent(npc.transform, false);
            markerCanvas.transform.localPosition = new Vector3(0, 2.85f);
            markerCanvas.transform.localScale = Vector3.one / 60f;
            markerCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 50);
            markerCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            markerCanvas.GetComponent<Canvas>().sortingOrder = 11;
            var marker = LabUI.Text(markerCanvas.transform, "", 36, LabUI.Gold);
            marker.alignment = TextAlignmentOptions.Center;
            LabUI.WorldLabel(marker, LabUI.Gold);
            LabUI.Fill(marker.rectTransform);

            var teacher = npc.AddComponent<TeacherNPC>();
            teacher.Initialize(this, renderer, marker, canvas.GetComponent<CanvasGroup>());

            var talk = new GameObject("მასწავლებელთან საუბარი", typeof(BoxCollider2D), typeof(InteractableStation));
            talk.transform.SetParent(npc.transform, false);
            var zone = talk.GetComponent<BoxCollider2D>();
            zone.size = new Vector2(2.8f, 2.6f);
            zone.offset = new Vector2(0, 0.3f);
            var station = talk.GetComponent<InteractableStation>();
            station.stationType = StationType.TeacherNPC;
            station.stationPrompt = "[E] დალაპარაკება";
            station.promptHeight = 3.45f;
            station.Initialize(this);

            Dialogue = gameObject.AddComponent<TeacherDialogueUI>();
            Dialogue.Initialize(this, teacher);
        }

        /// <summary>Exclusive cosmetics earned through projects, the science fair and the travelling researcher.</summary>
        private void AddRuntimeCosmetics()
        {
            var list = new System.Collections.Generic.List<CosmeticItemSO>(cosmetics ?? new CosmeticItemSO[0]);
            void Add(string id, string name, string text, CosmeticCategory category, Color color)
            {
                if (list.Exists(c => c != null && c.itemID == id)) return;
                var item = ScriptableObject.CreateInstance<CosmeticItemSO>();
                item.itemID = id; item.displayName = name; item.description = text; item.category = category;
                item.color = color; item.weeklyExclusive = true; item.price = 0; item.name = name;
                list.Add(item);
            }
            Add("goggles_crystal", "კრისტალის სათვალე", "ჯილდო კრისტალების გაზრდისთვის.", CosmeticCategory.SafetyGoggles, new Color32(170, 210, 250, 255));
            Add("coat_fair_gold", "ბაზრობის ოქროს ხალათი", "სამეცნიერო ბაზრობის ჩემპიონის ხალათი.", CosmeticCategory.LabCoat, new Color32(239, 194, 102, 255));
            Add("hat_researcher", "მკვლევრის ქუდი", "პროფესორ ლევანის ექსპედიციიდან.", CosmeticCategory.HairOrHat, new Color32(150, 110, 70, 255));
            // Curriculum and advanced-floor rewards.
            Add("hat_alchemist", "ალქიმიკოსის ქუდი", "ჯილდო თავისთვის „ქიმიის დასაწყისი“.", CosmeticCategory.HairOrHat, new Color32(96, 74, 150, 255));
            Add("goggles_atom", "ატომის სათვალე", "ჯილდო თავისთვის „ატომები“.", CosmeticCategory.SafetyGoggles, new Color32(239, 150, 110, 255));
            Add("coat_periodic", "პერიოდული ცხრილის ხალათი", "ჯილდო თავისთვის „ელემენტები“.", CosmeticCategory.LabCoat, new Color32(120, 170, 210, 255));
            Add("coat_reaction", "რეაქციების ხალათი", "ჯილდო თავისთვის „რეაქციები“.", CosmeticCategory.LabCoat, new Color32(230, 150, 90, 255));
            Add("coat_master", "ოსტატის ხალათი", "რთული გამოწვევების მე-3 საფეხურის ჯილდო.", CosmeticCategory.LabCoat, new Color32(60, 70, 110, 255));
            Add("hat_professor", "პროფესორის ქუდი", "რთული გამოწვევების მე-4 საფეხურის ჯილდო.", CosmeticCategory.HairOrHat, new Color32(40, 44, 60, 255));
            cosmetics = list.ToArray();
        }

        /// <summary>Greets returning players with what changed since their last visit.</summary>
        private void WelcomeBack()
        {
            var data = GameSaveManager.Instance.Data;
            if (!data.tutorialCompleted) return;
            string today = LabProgress.Today;
            if (data.lastVisitDay == today) return;
            bool returning = data.lastVisitDay != "";
            data.lastVisitDay = today;
            if (PersonalDecor.Calm >= 6) LabProgress.AddReputation(2);
            var fair = ResearchProjects.SettleFairs();
            int waiting = ResearchProjects.Active.Count(p => ResearchProjects.Stage(p)?.Kind == StageKind.Wait && ResearchProjects.Ready(p) && !string.IsNullOrEmpty(p.readyDay));
            GameSaveManager.Instance.Commit();
            string message = (returning ? "კეთილი იყოს შენი დაბრუნება! " : "") + SchoolCalendar.DateText + " • " + SchoolCalendar.PartName + ". " +
                (waiting > 0 ? "ღამით ცდა დასრულდა — ლაბორატორიაში შეამოწმე. " : "") + (fair ?? "დღის ამბები განცხადებების დაფაზეა.");
            HUD.Notify(message);
        }

        private void CreateChalkboardLettering()
        {
            var board = new GameObject("დაფის ფორმულები", typeof(RectTransform), typeof(Canvas));
            board.transform.SetParent(classroom, false);
            board.transform.localPosition = new Vector3(36, 13.4f);
            board.transform.localScale = Vector3.one / 48;
            board.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 92);
            board.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            board.GetComponent<Canvas>().sortingOrder = 4;
            var formula = LabUI.Text(board.transform, "<size=32>H2O</size>\nაღმოაჩინე ქიმია", 17, new Color(0.8f, 1, 0.87f));
            formula.alignment = TextAlignmentOptions.Center;
            LabUI.Fill(formula.rectTransform);
        }
        private void Update()
        {
            if (!GameSaveManager.Instance.Data.profileCompleted) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { LabUI.CloseTop(); return; }
            if (Dialogue != null && Dialogue.IsOpen) return;
            if (Input.GetKeyDown(KeyCode.Tab)) { OpenMenu(0); return; }
            if (LabUI.Typing) return;
            if (Input.GetKeyDown(KeyCode.L)) OpenMenu(5);
            if (Input.GetKeyDown(KeyCode.R)) OpenMenu(6);
            if (Input.GetKeyDown(KeyCode.C)) OpenMenu(7);
            if (Input.GetKeyDown(KeyCode.J)) OpenMenu(8);
            if (Input.GetKeyDown(KeyCode.Tab)) OpenMenu(0);
            if (Input.GetKeyDown(KeyCode.N)) OpenMenu(1);
            if (Input.GetKeyDown(KeyCode.B)) OpenMenu(2);
            if (Input.GetKeyDown(KeyCode.M)) OpenMenu(9);
            if (GameSaveManager.FreshStartEachLaunch) PlaytestKeys();
        }

        /// <summary>Playtest only (save wiped each launch): F1–F8 jump to a chapter's first lesson, F9 skips the current lesson.</summary>
        private void PlaytestKeys()
        {
            Lesson target = null;
            for (int c = 0; c < Curriculum.Chapters.Length; c++)
                if (Input.GetKeyDown(KeyCode.F1 + c)) target = Curriculum.Of(c + 1)[0];
            if (Input.GetKeyDown(KeyCode.F9) && Curriculum.Next != null) target = Curriculum.Lessons.ElementAtOrDefault(System.Array.IndexOf(Curriculum.Lessons, Curriculum.Next) + 1);
            if (target == null && !Input.GetKeyDown(KeyCode.F9)) return;
            if (Tutorial.Step != TutorialManager.TutorialStep.Completed) { HUD.Notify("ჯერ შესავალი დაასრულე (ხალათი და მასწავლებელი), მერე გადადი თავებზე."); return; }
            for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();
            Curriculum.SkipTo(target);
            HUD.Notify("სატესტო გადასვლა • " + Curriculum.NextAction);
        }
        public void OpenMenu(int menu)
        {
            if (!GameSaveManager.Instance.Data.profileCompleted) return;
            if (Tutorial.Step != TutorialManager.TutorialStep.Completed && menu != 1)
            {
                HUD.Notify("ჯერ მასწავლებელთან პირველი ცდა დაასრულე.");
                return;
            }
            switch (menu)
            {
                case 0: Periodic.Toggle(); break;
                case 1: Notebook.Toggle(); break;
                case 2: Shop.Toggle(); break;
                case 3:
                    if (Curriculum.Complete) Boards.OpenRankings();
                    else HUD.Notify(Curriculum.LockedText + " კვირის რეიტინგი რთულ გამოწვევებთან ერთად გაიხსნება.");
                    break;
                case 4: Boards.OpenQuests(); break;
                case 5:
                    if (Personal.Inside) Personal.Workshop.Toggle();
                    else HUD.Notify("პირად ლაბორატორიაში შესასვლელად კლასის ზედა კართან მიდი.");
                    break;
                case 6: Research.Toggle(); break;
                case 7: Collection.Toggle(); break;
                case 8: Journal.Toggle(); break;
                case 9: Lessons.ToggleMap(); break;
            }
        }
        public void Interact(InteractableStation station)
        {
            if (station.stationType != StationType.TeacherNPC) Visuals.SetDesk(station.transform.position);
            switch (station.stationType)
            {
                case StationType.TutorialStation:
                    if (Tutorial.Step == TutorialManager.TutorialStep.ReadNotebook) { Tutorial.CompleteIntro(); Lessons.Begin(Curriculum.Next); }
                    else if (Curriculum.LessonAt(StationType.TutorialStation)) Lessons.Begin(Curriculum.Next);
                    else Lessons.OpenMap();
                    break;
                case StationType.QuizDesk:
                case StationType.VisualExperimentDesk: Challenges.Open(); break;
                case StationType.AtomTable:
                case StationType.ElementStation:
                case StationType.MoleculeTable:
                case StationType.ReactionTable:
                case StationType.SolutionStation:
                case StationType.AcidBaseStation:
                    if (Curriculum.LessonAt(station.stationType)) Lessons.Begin(Curriculum.Next);
                    else Lessons.OpenStation(station.stationType, station.name);
                    break;
                case StationType.CosmeticShopStation: OpenMenu(2); break;
                case StationType.LeaderboardBoard: OpenMenu(3); break;
                case StationType.TeacherNPC: Dialogue.Teacher.Attend(Dialogue.Talk); break;
                case StationType.Classmate: station.GetComponentInParent<SchoolNPC>()?.Talk(); break;
            }
        }
    }
}






