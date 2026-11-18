using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Conversation with the teacher. The camera zooms in on her, her lines are typed out one
    /// letter at a time with a short voice blip per syllable, and she offers quests or explains
    /// chemistry topics. E / Space / Enter / click skips the typing or moves on; 1-9 pick an answer.
    /// </summary>
    /// <summary>Anyone who can be talked to through the dialogue box.</summary>
    public interface ISpeaker
    {
        Transform transform { get; }
        bool Talking { get; set; }
        float Voice { get; }
        void Pulse();
    }

    public sealed class TeacherDialogueUI : MonoBehaviour
    {
        private struct Choice
        {
            public string Label; public Action Act;
            public Choice(string label, Action act) { Label = label; Act = act; }
        }

        private sealed class Topic
        {
            public readonly string Title; public readonly string[] Pages;
            public Topic(string title, params string[] pages) { Title = title; Pages = pages; }
        }

        private static readonly Topic[] Topics =
        {
            new Topic("რა არის ატომი?",
                "ატომი ნივთიერების უმცირესი ნაწილაკია, რომელიც ელემენტის თვისებებს ინარჩუნებს.",
                "ატომის ცენტრში ბირთვია, ბირთვში კი პროტონები და ნეიტრონებია. ბირთვის გარშემო პაწაწინა ელექტრონები მოძრაობს.",
                "პროტონების რაოდენობა ატომურ ნომერს გვიჩვენებს. წყალბადს ერთი პროტონი აქვს, ნახშირბადს ექვსი, ჟანგბადს რვა."),
            new Topic("მოლეკულა და ფორმულა",
                "მოლეკულა ერთმანეთთან დაკავშირებული ატომების ჯგუფია.",
                "H2O ნიშნავს: ორი წყალბადის ატომი და ერთი ჟანგბადის ატომი. ელემენტის შემდეგ პატარა რიცხვს ინდექსი ჰქვია.",
                "ფორმულის წინ დიდი რიცხვი კოეფიციენტია. 2H2O ორ წყლის მოლეკულას ნიშნავს."),
            new Topic("ქიმიური რეაქცია",
                "რეაქციისას ატომები არ ქრება და არც ახლიდან ჩნდება. ისინი უბრალოდ ახლებურად ერთდება.",
                "საწყის ნივთიერებებს რეაგენტები ჰქვია, მიღებულს კი პროდუქტები. მათ შორის ისარს ვწერთ.",
                "მაგალითად, 2H2 + O2 → 2H2O. წყალბადი და ჟანგბადი ერთად წყალს ქმნის!"),
            new Topic("განტოლების დაბალანსება",
                "მასის შენახვის კანონი ამბობს: ისრის ორივე მხარეს თითოეული ელემენტის ატომების რაოდენობა ტოლი უნდა იყოს.",
                "ჯერ ორივე მხარეს ატომები დაითვალე. შემდეგ მხოლოდ კოეფიციენტები შეცვალე. ინდექსს ნუ შეეხები, თორემ სულ სხვა ნივთიერება გამოვა!",
                "2H2 + O2 → 2H2O: მარცხნივ ოთხი წყალბადი და ორი ჟანგბადია, მარჯვნივაც ოთხი წყალბადი და ორი ჟანგბადი. დაბალანსებულია!"),
            new Topic("მჟავები და ფუძეები",
                "მჟავები წყალში წყალბადის იონებს, H+, გამოყოფს. ფუძეები კი ჰიდროქსიდის იონებს, OH-.",
                "pH შკალა ჩვეულებრივ 0-დან 14-მდეა. 7-ზე ნაკლები მჟავაა, 7 ნეიტრალურია, 7-ზე მეტი კი ფუძე.",
                "როცა მჟავა და ფუძე ერთმანეთს ანეიტრალებს, მარილი და წყალი მიიღება."),
            new Topic("პერიოდული ცხრილი",
                "პერიოდულ ცხრილში ელემენტები ატომური ნომრის მიხედვით ლაგდება.",
                "ერთ სვეტში, ანუ ჯგუფში, მყოფ ელემენტებს მსგავსი თვისებები აქვს.",
                "მაგალითად, ლითიუმი, ნატრიუმი და კალიუმი ერთ ჯგუფშია. სამივე წყალთან ძალიან აქტიურად რეაგირებს."),
            new Topic("ლაბორატორიის უსაფრთხოება",
                "ლაბორატორიაში ყოველთვის ატარე ხალათი და დამცავი სათვალე.",
                "ნივთიერება არასდროს გასინჯო და პირდაპირ არ დაყნოსო. სუნი ხელის მოძრაობით მიიზიდე შენკენ.",
                "და დაიმახსოვრე: მჟავის განზავებისას მჟავა წყალს დაუმატე და არა პირიქით."),
        };

        private const float LetterDelay = 0.05f;
        private const float FocusZoom = 6.5f;
        private LabGame game;
        private TeacherNPC teacher;
        private ISpeaker speaker;
        private AudioSource voice;
        private AudioClip blip;

        private LabWindow window;
        private TMP_Text line;
        private TMP_Text hint;
        private RectTransform choiceRoot;
        private readonly List<Button> choiceButtons = new List<Button>();

        private string[] pages = Array.Empty<string>();
        private int page;
        private Choice[] choices;
        private Action finished;
        private int shown, total;
        private float nextLetter;
        private float acceptInputAfter;
        private bool questNotified;

        public bool IsOpen => window != null;
        public TeacherNPC Teacher => teacher;
        private Action<int> pageHook, pendingHook;
        private Transform cameraMarker;
        private Camera snapCamera;
        private GameObject snapFrame;
        private TMP_Text snapCaption;
        private RawImage snapPicture;

        /// <summary>Types a sequence of lines and calls <paramref name="hook"/> as each line begins.</summary>
        public void Tour(Action then, string[] lines, Action<int> hook)
        {
            pendingHook = hook;
            Say(then, lines);
        }

        /// <summary>Pans the camera to a world point while talking; null returns it to the speaker.</summary>
        public void PointAt(Vector3? world, float zoom = 4.6f)
        {
            var camera = Camera.main != null ? Camera.main.GetComponent<ClassroomCamera>() : null;
            if (camera == null || speaker == null) return;
            if (world == null) { camera.Focus(speaker.transform, new Vector2(0, -0.5f), FocusZoom); return; }
            if (cameraMarker == null) cameraMarker = new GameObject("კამერის მიზანი").transform;
            cameraMarker.position = world.Value;
            camera.Focus(cameraMarker, new Vector2(0, -1.4f), zoom);
        }

        /// <summary>Shows a small live snapshot of a far-away place above the dialogue box.</summary>
        public void Snap(Vector3? world, string caption)
        {
            if (snapFrame == null) return;
            snapFrame.SetActive(world != null);
            if (snapCamera == null)
            {
                var go = new GameObject("სურათის კამერა", typeof(Camera));
                snapCamera = go.GetComponent<Camera>();
                var main = Camera.main;
                snapCamera.orthographic = true; snapCamera.orthographicSize = 3.4f;
                snapCamera.cullingMask = main.cullingMask;
                snapCamera.clearFlags = CameraClearFlags.SolidColor;
                snapCamera.backgroundColor = new Color(0.16f, 0.18f, 0.17f);
                snapCamera.depth = main.depth - 5;
                snapCamera.targetTexture = new RenderTexture(512, 288, 16) { filterMode = FilterMode.Point };
            }
            snapPicture.texture = snapCamera.targetTexture;
            snapCamera.enabled = world != null;
            if (world != null)
            {
                snapCamera.transform.position = new Vector3(world.Value.x, world.Value.y, -10);
                snapCaption.text = caption;
            }
        }

        public void Initialize(LabGame lab, TeacherNPC npc)
        {
            game = lab; teacher = npc;
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.volume = 0.45f;
            blip = MakeBlip();
            questNotified = TeacherQuests.Done(TeacherQuests.Active);
            GameSaveManager.Instance.Changed += OnSaveChanged;
        }

        private void OnDestroy()
        {
            if (GameSaveManager.Instance != null) GameSaveManager.Instance.Changed -= OnSaveChanged;
        }

        private void OnSaveChanged()
        {
            TeacherQuests.Track();
            var quest = TeacherQuests.Active;
            bool done = TeacherQuests.Done(quest);
            if (done && !questNotified && game.HUD != null)
                game.HUD.Notify("დავალება \"" + quest.Title + "\" შესრულებულია! დაბრუნდი მასწავლებელთან ჯილდოსთვის.");
            questNotified = done;
        }

        // ---------- Conversation flow ----------

        public void Talk()
        {
            if (IsOpen) return;
            Begin(teacher, "მასწავლებელი");

            var data = GameSaveManager.Instance.Data;
            string name = string.IsNullOrWhiteSpace(data.playerName) ? "ჩემო მოსწავლე" : data.playerName;
            if (!data.tutorialCompleted)
            {
                Say(MainMenu, "გამარჯობა, " + name + "! მე შენი ქიმიის მასწავლებელი ვარ.",
                    game.Tutorial.Step == TutorialManager.TutorialStep.CollectCoat
                        ? "სანამ რამეს შევეხებით, მბზინავი კარადიდან დამცავი ხალათი აიღე. უსაფრთხოება პირველ ადგილზეა."
                        : "ხალათი გიხდება. ახლა ჩემს მაგიდასთან მიდი და [E] დააჭირე. ერთად წყალს მივიღებთ.");
                return;
            }
            if (!data.teacherMet)
            {
                data.teacherMet = true;
                GameSaveManager.Instance.Commit();
                Say(MainMenu, name + ", პირველი ცდა კარგად გამოგივიდა.",
                    "ჩემთან ყოველთვის შეგიძლია მოხვიდე. დავალებას მოგცემ, ან რაც არ გესმის, აგიხსნი.");
                return;
            }
            var quest = TeacherQuests.Active;
            if (TeacherQuests.Done(quest))
            {
                Say(MainMenu, "კარგია, " + name + "! დავალება \"" + quest.Title + "\" შესრულებულია.");
                return;
            }
            string[] greetings =
            {
                "გამარჯობა, " + name + "! რა გაინტერესებს დღეს?",
                "ჰმმ... ჩემი კოლბები ისევ ბუყბუყებს. მითხარი, რით დაგეხმარო?",
                "ლაბორატორიაში ყოველთვის რაღაც ახალი ხდება. რა გნებავს, " + name + "?",
                "ქიმია ყველგანაა, " + name + ", ჩაის ჭიქაშიც კი. რით დაგეხმარო?",
            };
            Say(MainMenu, greetings[UnityEngine.Random.Range(0, greetings.Length)]);
        }

        /// <summary>Opens the dialogue box for any speaker; <paramref name="opening"/> starts the conversation.</summary>
        public void Converse(ISpeaker who, string title, Action opening)
        {
            if (IsOpen || who == null) return;
            Begin(who, title);
            opening?.Invoke();
        }
        public void Line(Action then, params string[] lines) => Say(then, lines);
        public void Offer(string question, params (string label, Action act)[] options) =>
            Ask(question, options.Select(o => new Choice(o.label, o.act)).ToArray());
        public void End() { if (window != null) window.Close(); }

        private void Begin(ISpeaker who, string title)
        {
            speaker = who;
            BuildWindow(title);
            var camera = Camera.main != null ? Camera.main.GetComponent<ClassroomCamera>() : null;
            if (camera != null) camera.Focus(who.transform, new Vector2(0, -0.5f), FocusZoom);
            who.Talking = true;
            who.Pulse();
            StepAside();
            SetPlayerLabel(false);
        }

        private void MainMenu()
        {
            var quest = TeacherQuests.Active;
            var list = new List<Choice>();
            var lesson = TeacherGuide.Next;
            if (lesson != null) list.Add(new Choice("გაკვეთილი: " + lesson.Title, () => TeacherGuide.Play(game, lesson, MainMenu)));
            var research = ResearchProjects.Submittable;
            if (research != null) list.Add(new Choice("კვლევის ჩაბარება", () => SubmitResearch(research)));
            if (SchoolCalendar.IsInspectionDay && !LabProgress.Data.claimedEvents.Contains("inspection:" + SchoolCalendar.Week))
                list.Add(new Choice("ინსპექცია", Inspection));
            if (TeacherQuests.Done(quest)) list.Add(new Choice("დავალების ჩაბარება!", TurnIn));
            list.Add(new Choice(quest == null ? "დავალება მინდა" : "ჩემი დავალება", Quests));
            list.Add(new Choice("ამიხსენით რამე", () => ExplainMenu("რა აგიხსნა? აირჩიე თემა:")));
            if (GameSaveManager.Instance.Data.guideLessons.Count > 0) list.Add(new Choice("გაკვეთილის გამეორება", ReplayMenu));
            list.Add(new Choice("ნახვამდის", Goodbye));
            Ask("აბა, რას ვაკეთებთ?", list.ToArray());
        }

        private void Quests()
        {
            var data = GameSaveManager.Instance.Data;
            if (!data.tutorialCompleted)
            {
                Say(MainMenu, "ჯერ პირველი ცდა დავასრულოთ. ჩემს მაგიდასთან დაგელოდები!");
                return;
            }
            var quest = TeacherQuests.Active;
            if (quest != null)
            {
                if (TeacherQuests.Done(quest)) { TurnIn(); return; }
                Say(null, "შენი მიმდინარე დავალებაა \"" + quest.Title + "\". " + quest.Goal + ". პროგრესი: "
                    + TeacherQuests.Progress(quest) + " / " + quest.Target + ".");
                Ask("ნუ ჩქარობ, მაგრამ არც გადადო! გააგრძელებ?",
                    new Choice("გავაგრძელებ!", MainMenu),
                    new Choice("სხვა დავალება მინდა", () =>
                    {
                        TeacherQuests.Abandon();
                        Say(Offers, "კარგი, ეს სხვა დროისთვის გადავდოთ.");
                    }));
                return;
            }
            Offers();
        }

        private void Offers()
        {
            var offers = TeacherQuests.Offers(game);
            var list = offers.Select(q => new Choice(q.Title + "  ·  +" + q.Reward, () => Pitch(q))).ToList();
            list.Add(new Choice("ჯერ არა", MainMenu));
            Ask(offers.Count == 2 ? "ჰმმ... ორი დავალება მაქვს შენთვის. რომელს აირჩევ?"
                : "ჰმმ... აი, რა დავალებები მაქვს შენთვის. ერთი აირჩიე:", list.ToArray());
        }

        private void Pitch(TeacherQuest quest)
        {
            Say(null, quest.Pitch);
            Ask("ჯილდო " + quest.Reward + " ქიმქოინია. იღებ დავალებას?",
                new Choice("ვიღებ!", () =>
                {
                    TeacherQuests.Accept(quest);
                    AudioManager.Instance?.Milestone();
                    game.HUD?.Notify("ახალი დავალება: " + quest.Title + " — " + quest.Goal + ".");
                    Say(MainMenu, "კარგი. როცა დაასრულებ, ჩემთან მოდი და ჯილდოს მიიღებ.");
                }),
                new Choice("სხვა მაჩვენეთ", Offers));
        }

        private void TurnIn()
        {
            var quest = TeacherQuests.Active;
            int reward = TeacherQuests.TurnIn();
            if (reward <= 0) { MainMenu(); return; }
            game.HUD?.Notify("დავალება \"" + quest.Title + "\" ჩაბარებულია! +" + reward + " ქიმქოინი.");
            Say(MainMenu, "კარგი ნამუშევარია. აი, შენი ჯილდო: " + reward + " ქიმქოინი.",
                "ასე თუ გააგრძელებ, მალე ნამდვილი ქიმიკოსი გახდები.");
        }

        private void ExplainMenu(string question)
        {
            var list = Topics.Select(t => new Choice(t.Title, () => Say(() => ExplainMenu("კიდევ რამე გაინტერესებს?"), t.Pages))).ToList();
            list.Add(new Choice("საკმარისია, მადლობა!", MainMenu));
            Ask(question, list.ToArray());
        }

        private void ReplayMenu()
        {
            var done = TeacherGuide.Lessons.Where(l => TeacherGuide.Done(l.ID)).ToList();
            var list = done.Select(l => new Choice(l.Title, () => TeacherGuide.Play(game, l, MainMenu))).ToList();
            list.Add(new Choice("უკან", MainMenu));
            Ask("რომელი გაკვეთილი გავიმეოროთ?", list.ToArray());
        }

        private void SubmitResearch(ProjectProgress state)
        {
            var project = ResearchProjects.Get(state.id);
            string reward = ResearchProjects.Submit(state);
            if (reward == null) { MainMenu(); return; }
            game.HUD?.Notify("პროექტი \"" + project.Title + "\" ჩაბარებულია! " + reward);
            Say(MainMenu, "\"" + project.Title + "\" კარგად შესრულებული ნაშრომია.",
                state.mistakes == 0 ? "ერთი შეცდომაც არ დაგიშვია. ჯილდო: " + reward
                    : "შეცდომები სწავლის ნაწილია და მათგან ბევრი რამ ისწავლე. ჯილდო: " + reward,
                "ეს ნაშრომი სამეცნიერო ბაზრობაზეც შეგიძლია წარადგინო. გამოფენის დარბაზში დერეფნიდან შეხვალ.");
        }

        private void Inspection()
        {
            var data = LabProgress.Data;
            data.claimedEvents.Add("inspection:" + SchoolCalendar.Week);
            int placed = data.labPlacements.Count;
            bool tidy = data.pendingSpills == 0;
            if (tidy && placed >= 2)
            {
                LabProgress.AddReputation(12);
                LabProgress.Collect("badge:tidy");
                GameSaveManager.Instance.Commit();
                Say(MainMenu, "ინსპექციის დროა. ყველაფერი სუფთაა და აღჭურვილობა თავის ადგილზეა.", "ძალიან კარგია. +12 რეპუტაცია.");
            }
            else
            {
                LabProgress.AddReputation(3);
                GameSaveManager.Instance.Commit();
                Say(MainMenu, "ინსპექციის დროა! " + (tidy ? "სუფთაა, მაგრამ აღჭურვილობა ცოტაა. დადგი მინიმუმ ორი ნივთი." : "სადღაც დაღვრილი სითხე დაგრჩა. ავარიები [E]-ით დაასუფთავე."),
                    "არაუშავს, შემდეგ კვირას უკეთ გამოგივა. +3 რეპუტაცია მცდელობისთვის.");
            }
        }

        private void Goodbye()
        {
            string[] lines =
            {
                "ნახვამდის! და არ დაგავიწყდეს: უსაფრთხოება პირველ ადგილზეა!",
                "წარმატებები! ცდები ფრთხილად ჩაატარე.",
                "კარგი, წადი, აღმოაჩინე რამე საინტერესო!",
            };
            Say(() => window?.Close(), lines[UnityEngine.Random.Range(0, lines.Length)]);
        }

        // ---------- Staging ----------

        private Rigidbody2D playerBody;
        private Vector2 stepFrom, stepTo;
        private float stepStart = -1;
        private const float StepTime = 0.3f;

        /// <summary>
        /// If the player stands in front of the teacher they would hide her during the close-up,
        /// so they take a step to her side (or back) and turn to face her.
        /// </summary>
        private void StepAside()
        {
            playerBody = game.player.GetComponent<Rigidbody2D>();
            var feet = game.player.GetComponent<BoxCollider2D>();
            Vector2 t = speaker.transform.position, p = playerBody.position;
            stepStart = -1;
            if (Mathf.Abs(p.x - t.x) < 1.2f && p.y > t.y - 2.2f && p.y < t.y + 2.6f)
            {
                float side = p.x < t.x ? -1 : 1;
                Vector2[] spots = { t + new Vector2(1.5f * side, -0.35f), t + new Vector2(-1.5f * side, -0.35f), t + new Vector2(0, -2.35f) };
                foreach (var spot in spots)
                {
                    bool blocked = Physics2D.OverlapBoxAll(spot + feet.offset, feet.size + Vector2.one * 0.25f, 0)
                        .Any(c => !c.isTrigger && c.attachedRigidbody != playerBody);
                    if (blocked) continue;
                    stepFrom = p; stepTo = spot; stepStart = Time.unscaledTime;
                    p = spot;
                    break;
                }
            }
            Vector2 look = t - p;
            if (look.sqrMagnitude > 0.01f) game.player.SetMovement(look.normalized);
        }

        private void SetPlayerLabel(bool visible)
        {
            var label = game.player.transform.Find("მოთამაშის წოდება");
            if (label != null) label.gameObject.SetActive(visible);
        }

        private void UpdateStep()
        {
            if (stepStart < 0 || playerBody == null) return;
            float k = Mathf.Clamp01((Time.unscaledTime - stepStart) / StepTime);
            playerBody.position = Vector2.Lerp(stepFrom, stepTo, Mathf.SmoothStep(0, 1, k));
            if (k >= 1) { stepStart = -1; Physics2D.SyncTransforms(); }
        }

        // ---------- Typewriter ----------

        /// <summary>Types out the pages one after another, then runs <paramref name="then"/>.</summary>
        private void Say(Action then, params string[] lines)
        {
            if (!IsOpen) return;
            pageHook = pendingHook; pendingHook = null;
            ClearChoices();
            choices = null;
            finished = then;
            var queued = new List<string>();
            // Chained Say(null, ...) + Ask(...) calls queue up rather than replacing each other.
            if (pendingChain) { queued.AddRange(pages.Skip(page)); }
            queued.AddRange(lines);
            pages = queued.ToArray();
            page = 0;
            pendingChain = then == null;
            BeginPage();
        }

        private bool pendingChain;

        /// <summary>Types the question, then shows numbered answer buttons.</summary>
        private void Ask(string question, params Choice[] options)
        {
            Say(null, question);
            pendingChain = false;
            choices = options;
        }

        private void BeginPage()
        {
            line.text = pages[page];
            line.maxVisibleCharacters = 0;
            line.ForceMeshUpdate();
            total = line.textInfo.characterCount;
            shown = 0;
            nextLetter = Time.unscaledTime + 0.12f;
            hint.text = "";
            Resize(0);
            pageHook?.Invoke(page);
        }

        private bool Typing => shown < total;

        private void Update()
        {
            if (!IsOpen) return;
            UpdateStep();
            if (Typing)
            {
                while (Typing && Time.unscaledTime >= nextLetter)
                {
                    char c = line.textInfo.characterInfo[shown].character;
                    shown++;
                    line.maxVisibleCharacters = shown;
                    nextLetter += Pause(c);
                    if (char.IsLetterOrDigit(c) && shown % 2 == 1) Speak();
                }
                if (!Typing) PageComplete();
            }
            if (Time.unscaledTime < acceptInputAfter) return;

            if (choiceButtons.Count > 0)
            {
                for (int i = 0; i < choiceButtons.Count && i < 9; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                    { choiceButtons[i].onClick.Invoke(); return; }
                return;
            }
            bool advance = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0);
            if (!advance) return;
            if (Typing)
            {
                shown = total;
                line.maxVisibleCharacters = total;
                PageComplete();
                return;
            }
            if (page < pages.Length - 1) { page++; BeginPage(); return; }
            if (choices == null)
            {
                var next = finished;
                finished = null;
                if (next != null) next();
                else window?.Close();
            }
        }

        private static float Pause(char c)
        {
            switch (c)
            {
                case '.': case '!': case '?': return 0.34f;
                case ',': case ';': case ':': case '—': return 0.16f;
                case '\n': return 0.2f;
                default: return LetterDelay;
            }
        }

        private void PageComplete()
        {
            acceptInputAfter = Mathf.Max(acceptInputAfter, Time.unscaledTime + 0.12f);
            bool last = page >= pages.Length - 1;
            if (last && choices != null) { ShowChoices(choices); hint.text = ""; }
            else hint.text = "[E] / Space — შემდეგი";
        }

        private void Speak()
        {
            voice.pitch = UnityEngine.Random.Range(0.94f, 1.08f) * (speaker != null ? speaker.Voice : 1);
            voice.PlayOneShot(blip);
            speaker?.Pulse();
        }

        /// <summary>A short square-wave "bip", the classic retro talking sound.</summary>
        private static AudioClip MakeBlip()
        {
            const int rate = 44100;
            int length = Mathf.RoundToInt(rate * 0.06f);
            var samples = new float[length];
            double phase = 0;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                float frequency = 640f - 1400f * t; // a slight downward glide sounds more like a voice
                phase += frequency / rate;
                float square = phase % 1 < 0.5 ? 1f : -1f;
                float triangle = (float)(4 * Math.Abs(phase % 1 - 0.5) - 1);
                float envelope = Mathf.Clamp01(t / 0.004f) * Mathf.Exp(-t * 38f);
                samples[i] = (square * 0.55f + triangle * 0.45f) * envelope * 0.5f;
            }
            var clip = AudioClip.Create("მასწავლებლის ხმა", length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // ---------- Layout ----------

        private const float Width = 1060, BaseHeight = 252, RowHeight = 54;

        private void BuildWindow(string title)
        {
            window = LabUI.Window(title, Width, BaseHeight);
            window.Closed += OnClosed;
            // Light veil instead of the usual dark backdrop so the zoomed-in teacher stays visible.
            window.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.06f, 0.12f);
            var card = window.Card;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0);
            card.pivot = new Vector2(0.5f, 0);
            card.anchoredPosition = new Vector2(0, 22);

            line = LabUI.Text(window.Body, "", 23, LabUI.Ink);
            line.richText = false;
            LabUI.Size(line.gameObject, 96);

            var grid = LabUI.Object(window.Body, "პასუხები", typeof(GridLayoutGroup));
            choiceRoot = (RectTransform)grid.transform;
            var layout = grid.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2((Width - 56 - 24) / 3f, 44);
            layout.spacing = new Vector2(12, 10);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            LabUI.Size(grid, 0);

            // Snapshot frame floating above the top-right corner of the dialogue box.
            var frame = LabUI.Panel(window.Card, "სურათი", LabUI.Navy);
            frame.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = frame.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-10, 12); rect.sizeDelta = new Vector2(420, 262);
            LabUI.Shadow(frame);
            var picture = LabUI.Object(frame.transform, "ხედი", typeof(RawImage)).GetComponent<RawImage>();
            picture.raycastTarget = false;
            snapPicture = picture;
            var pictureRect = picture.rectTransform;
            pictureRect.anchorMin = Vector2.zero; pictureRect.anchorMax = Vector2.one;
            pictureRect.offsetMin = new Vector2(8, 34); pictureRect.offsetMax = new Vector2(-8, -8);
            snapCaption = LabUI.Text(frame.transform, "", 16, LabUI.Paper);
            snapCaption.alignment = TextAlignmentOptions.Center;
            var captionRect = snapCaption.rectTransform;
            captionRect.anchorMin = Vector2.zero; captionRect.anchorMax = new Vector2(1, 0); captionRect.pivot = new Vector2(0.5f, 0);
            captionRect.anchoredPosition = new Vector2(0, 4); captionRect.sizeDelta = new Vector2(-16, 28);
            snapFrame = frame.gameObject;
            snapFrame.SetActive(false);

            hint = LabUI.Text(window.Body, "", 15, LabUI.Muted);
            hint.alignment = TextAlignmentOptions.Right;
            LabUI.Size(hint.gameObject, 20);
            acceptInputAfter = Time.unscaledTime + 0.25f;
        }

        private void ShowChoices(Choice[] options)
        {
            ClearChoices();
            for (int i = 0; i < options.Length; i++)
            {
                var choice = options[i];
                string label = (i < 9 ? (i + 1) + ".  " : "") + choice.Label;
                var button = i == options.Length - 1 && options.Length > 1
                    ? LabUI.Secondary(choiceRoot, label, () => Choose(choice))
                    : LabUI.Button(choiceRoot, label, () => Choose(choice));
                choiceButtons.Add(button);
            }
            Resize(Mathf.CeilToInt(options.Length / 3f));
        }

        private void Choose(Choice choice)
        {
            if (Time.unscaledTime < acceptInputAfter) return;
            acceptInputAfter = Time.unscaledTime + 0.15f;
            ClearChoices();
            choice.Act?.Invoke();
        }

        private void ClearChoices()
        {
            foreach (var button in choiceButtons) if (button != null) Destroy(button.gameObject);
            choiceButtons.Clear();
            if (window != null) Resize(0);
        }

        private void Resize(int rows)
        {
            float gridHeight = rows > 0 ? rows * 44 + (rows - 1) * 10 : 0;
            LabUI.Size(choiceRoot.gameObject, gridHeight);
            window.Card.sizeDelta = new Vector2(Width, BaseHeight + (rows > 0 ? gridHeight + 14 - 20 : 0));
            if (hint != null) hint.gameObject.SetActive(rows == 0);
        }

        private void OnClosed()
        {
            window = null;
            choiceButtons.Clear();
            pendingChain = false;
            pageHook = pendingHook = null;
            if (snapCamera != null) snapCamera.enabled = false;
            game.HUD?.Highlight("");
            if (speaker != null) speaker.Talking = false;
            speaker = null;
            SetPlayerLabel(true);
            var camera = Camera.main != null ? Camera.main.GetComponent<ClassroomCamera>() : null;
            if (camera != null) camera.Release();
        }
    }
}
