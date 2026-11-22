using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Beginner lessons (teach → demo → guided practice → free practice → small test → reward),
    /// the learning map and the practice hubs of the classroom stations.
    /// </summary>
    public sealed class CurriculumUI : MonoBehaviour
    {
        public enum Step { Teach, Demo, Guided, Free, Test, Reward }

        private LabGame game;
        private LabWindow window, map, hub;
        private RectTransform stage;
        private Transform controls;
        private TMP_Text feedback;
        private Button next;
        private Coroutine demo;
        private Practice practice;
        private int[] values;
        private bool practiceOnly, answered;
        private int from, to, question, tries, lessonMistakes, xpAtStart;
        private float startedAt;
        private Image feedbackCard, progressFill;
        private string[] keys = new string[0];
        private readonly HashSet<int> sorted = new HashSet<int>();
        private readonly Dictionary<int, int> misses = new Dictionary<int, int>();
        private readonly HashSet<string> wrong = new HashSet<string>();
        private readonly List<Button> choiceButtons = new List<Button>();
        private static Sprite circle;

        public Lesson Lesson { get; private set; }
        public Step Current { get; private set; }
        public bool IsOpen => window != null;
        public bool MapOpen => map != null;
        public bool HubOpen => hub != null;
        public bool Solved { get; private set; }
        public string Feedback => feedback != null ? feedback.text : "";
        public LessonQuestion Question => question < keys.Length ? Curriculum.Resolve(keys[question]).question : null;
        public List<string> Rewards { get; private set; }
        private MixBenchUI bench;
        public MixBenchUI Bench => bench != null ? bench : bench = gameObject.AddComponent<MixBenchUI>();

        public void Initialize(LabGame lab) => game = lab;

        // ================================================================== starting a lesson

        /// <summary>The teacher explains the lesson in the dialogue box, then the workbook opens.</summary>
        public void Begin(Lesson lesson)
        {
            if (lesson == null) return;
            if (!Curriculum.Available(lesson)) { game.HUD.Notify(Curriculum.LockedText + " " + Curriculum.Requirement(lesson)); return; }
            if (map != null) map.Close();
            if (hub != null) hub.Close();
            // Play first: the activity opens straight away; explanations come after the player has tried it.
            if (game.Dialogue.IsOpen) game.Dialogue.End();
            OpenWorkbook(lesson);
        }

        /// <summary>Opens the hands-on part of a lesson. Station practice starts straight at the free exercise.</summary>
        public void OpenWorkbook(Lesson lesson, bool practiceOnlyMode = false)
        {
            if (lesson == null) return;
            if (window != null) window.Close();
            Lesson = lesson; practiceOnly = practiceOnlyMode && !lesson.IsReview;
            sorted.Clear(); misses.Clear(); wrong.Clear(); Rewards = null;
            lessonMistakes = 0; startedAt = Time.unscaledTime; xpAtStart = GameSaveManager.Instance.Data.totalXP;
            var created = LabUI.Window(practiceOnly ? "ვარჯიში • " + lesson.Title : "თავი " + lesson.Chapter + " • " + Curriculum.Label(lesson), 1100, 680);
            created.Closed = () => { if (window == created) { window = null; StopDemo(); } };
            window = created;
            int n = lesson.Items.Length;
            int guidedEnd = Mathf.Min(n, 2 + (n - 1) / 2);
            if (practiceOnly) for (int i = 0; i < guidedEnd; i++) sorted.Add(i);
            Go(practiceOnly ? Step.Free : lesson.IsReview ? Step.Test : Step.Demo);
        }

        public void Close() { if (window != null) window.Close(); }

        private void Go(Step step)
        {
            StopDemo();
            Current = step; Solved = false; practice = null; tries = 0;
            foreach (Transform child in window.Body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            choiceButtons.Clear();
            if (step == Step.Test) { keys = Lesson.IsReview ? Curriculum.ReviewKeys(Lesson) : Lesson.Questions.Select((q, i) => Curriculum.Key(Lesson, i)).ToArray(); question = 0; answered = false; }
            Header();
            if (step == Step.Test) BuildQuestion();
            else if (step == Step.Reward) BuildReward();
            else BuildPractice();
        }

        private Step[] Steps => Lesson.IsReview ? new[] { Step.Teach, Step.Test, Step.Reward } : practiceOnly ? new[] { Step.Free } : new[] { Step.Demo, Step.Guided, Step.Free, Step.Reward };
        /// <summary>How far through the lesson the player is, 0..1 (quiz questions count inside their step).</summary>
        private float Progress(bool solved)
        {
            var steps = Steps;
            int index = Array.IndexOf(steps, Current);
            float done = index + (solved ? 1 : 0);
            if (Current == Step.Test && keys.Length > 0) done = index + (question + (answered ? 1f : 0f)) / keys.Length;
            if (Current == Step.Reward) done = steps.Length;
            return Mathf.Clamp01(done / steps.Length);
        }

        private void Header()
        {
            var row = LabUI.Row(window.Body, "გაკვეთილის პროგრესი", 14);
            LabUI.Size(row.gameObject, 30);
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childForceExpandHeight = false;
            var label = LabUI.Text(row.transform, Curriculum.StepNames[(int)Current], 16, LabUI.Green);
            label.fontStyle = FontStyles.Bold; label.enableAutoSizing = true; label.fontSizeMin = 11; label.fontSizeMax = 16;
            LabUI.Size(label.gameObject, 26, 200, 0);
            progressFill = LabUI.Progress(row.transform, Progress(false), LabUI.Mint, 18);
            LabUI.Size(progressFill.transform.parent.gameObject, 18, -1, 1);
        }

        /// <summary>Slides the lesson bar forward instead of jumping.</summary>
        private IEnumerator Grow(Image fill, float to)
        {
            float from = fill.rectTransform.anchorMax.x;
            for (float t = 0; t < 0.35f && fill != null; t += Time.unscaledDeltaTime)
            {
                fill.rectTransform.anchorMax = new Vector2(Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, t / 0.35f)), 1);
                yield return null;
            }
            if (fill != null) fill.rectTransform.anchorMax = new Vector2(to, 1);
        }

        /// <summary>The bottom banner: feedback on the left, the continue button on the right.</summary>
        private void Banner(string text, string nextLabel)
        {
            feedbackCard = LabUI.Card(window.Body, "შედეგი", LabUI.Soft, 10);
            LabUI.Size(feedbackCard.gameObject, 92);
            var row = LabUI.Row(feedbackCard.transform, "ბანერი", 14);
            row.childAlignment = TextAnchor.MiddleLeft;
            LabUI.FlexibleHeight(row.gameObject);
            feedback = LabUI.Text(row.transform, text, 17, LabUI.Ink);
            feedback.enableAutoSizing = true; feedback.fontSizeMin = 12; feedback.fontSizeMax = 17;
            LabUI.Size(feedback.gameObject, 70, -1, 1);
            next = LabUI.Button(row.transform, nextLabel, () => Advance(), 54);
            LabUI.Size(next.gameObject, 54, 230, 0);
        }

        // ================================================================== practice

        private void BuildPractice()
        {
            bool sort = Lesson.Kind == PracticeKind.Sort;
            int stepIndex = Current == Step.Demo ? 0 : Current == Step.Guided ? 1 : 2;
            int n = Lesson.Items.Length, guidedEnd = Mathf.Min(n, 2 + (n - 1) / 2);
            from = stepIndex == 0 ? 0 : stepIndex == 1 ? 2 : guidedEnd;
            to = stepIndex == 0 ? Mathf.Min(2, n) : stepIndex == 1 ? guidedEnd : n;
            if (!sort) practice = Lesson.Steps[stepIndex];
            string goal = sort
                ? (Current == Step.Demo ? "ნახე, როგორ დავალაგებ პირველ მაგალითებს." : Current == Step.Guided ? "დაალაგე შემდეგი მაგალითები. მინიშნება: " + Lesson.Hint : "ახლა დამოუკიდებლად დაალაგე დარჩენილი მაგალითები.")
                : practice.Goal;
            var goalText = LabUI.Text(window.Body, (Current == Step.Demo ? "მასწავლებელი აჩვენებს • " : Current == Step.Guided ? "ვარჯიში მინიშნებით • " : "დამოუკიდებელი ვარჯიში • ") + goal, 19, LabUI.Ink);
            LabUI.Size(goalText.gameObject, 50);
            goalText.enableAutoSizing = true; goalText.fontSizeMin = 13; goalText.fontSizeMax = 19;

            var split = LabUI.Row(window.Body, "ვარჯიში", 18);
            LabUI.FlexibleHeight(split.gameObject);
            split.childForceExpandHeight = true;
            var panel = LabUI.Panel(split.transform, "სცენა", LabUI.Navy);
            LabUI.Size(panel.gameObject, 280, 470, 0);
            LabUI.FlexibleHeight(panel.gameObject);
            panel.gameObject.AddComponent<RectMask2D>();
            stage = panel.rectTransform;
            var column = LabUI.Column(split.transform, "მართვა", 8);
            LabUI.Size(column.gameObject, 280, -1, 1);
            controls = column.transform;

            Banner("", Current == Step.Demo ? "გამოტოვება →" : "გაგრძელება →");

            var footer = LabUI.Row(window.Body, "ქმედებები", 10);
            LabUI.Size(LabUI.Secondary(footer.transform, "რა ხდება აქ?", () => Say(string.Join("\n", Lesson.Teach), LabUI.Ink), 42).gameObject, 42, 230, 0);
            LabUI.Size(LabUI.Secondary(footer.transform, "რვეული [N]", () => game.Notebook.Open(1), 42).gameObject, 42, 160, 0);
            if (Current == Step.Free && !sort) LabUI.Size(LabUI.Button(footer.transform, "შემოწმება", Check, 42).gameObject, 42, 180, 0);

            if (sort)
            {
                if (Current == Step.Demo) demo = StartCoroutine(SortDemo());
                else { next.interactable = false; Say(Current == Step.Guided ? "აირჩიე სწორი ჯგუფი ყოველი მაგალითისთვის." : "შეცდომის შემთხვევაში მინიშნებას მოგცემ.", LabUI.Ink); }
                RefreshSort();
            }
            else
            {
                values = (practice.Start ?? Min()).ToArray();
                if (Current == Step.Demo) demo = StartCoroutine(BuildDemo());
                else
                {
                    next.interactable = false;
                    Say(Current == Step.Guided ? "შეცვალე რიცხვები. ყოველი ცვლილების შემდეგ მინიშნებას დაინახავ." : "როცა მზად იქნები, დააჭირე „შემოწმებას“.", LabUI.Ink);
                }
                BuildCounters();
                Draw();
            }
        }

        /// <summary>Gives a text line a fixed layout height and lets it shrink to fit.</summary>
        private static TMP_Text Fit(TMP_Text text, float height)
        {
            LabUI.Size(text.gameObject, height);
            text.enableAutoSizing = true; text.fontSizeMin = 10; text.fontSizeMax = text.fontSize;
            return text;
        }

        private static readonly Color RightBanner = new Color32(214, 240, 220, 255), WrongBanner = new Color32(250, 225, 214, 255);
        private void Say(string text, Color color)
        {
            if (feedback == null) return;
            feedback.text = text; feedback.color = color;
            if (feedbackCard == null) return;
            var tint = color == LabUI.Green ? RightBanner : color == LabUI.Coral ? WrongBanner : LabUI.Soft;
            if (feedbackCard.color != tint) { feedbackCard.color = tint; StartCoroutine(Pop(feedbackCard.transform)); }
        }
        private static IEnumerator Pop(Transform target)
        {
            for (float t = 0; t < 0.18f && target != null; t += Time.unscaledDeltaTime)
            {
                target.localScale = Vector3.one * (0.96f + 0.04f * Mathf.SmoothStep(0, 1, t / 0.18f));
                yield return null;
            }
            if (target != null) target.localScale = Vector3.one;
        }
        private void MarkSolved(string text)
        {
            Solved = true;
            Say(text, LabUI.Green);
            if (next != null) { next.interactable = true; next.GetComponentInChildren<TMP_Text>().text = "გაგრძელება →"; }
            if (progressFill != null) StartCoroutine(Grow(progressFill, Progress(true)));
            AudioManager.Instance?.Correct();
        }

        // Enter presses the continue button, like the big green button in Duolingo.
        private void Update()
        {
            if (window != null && next != null && next.interactable && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) next.onClick.Invoke();
        }
        /// <summary>What the player built, in the teacher's words (open goals accept several answers).</summary>
        private string Result() => practice.Named?.Invoke(values) ?? practice.Done;

        /// <summary>The teacher reacts to how the step was solved, not only that it was.</summary>
        private string Praise()
        {
            if (practice != null && values != null && !values.SequenceEqual(practice.Solution)) return "ესეც სწორი პასუხია! ";
            if (Current == Step.Free && tries == 0) return "პირველივე ცდაზე! ";
            if (tries >= 2) return "არ დანებდი და მიაგენი — ასე მუშაობენ ნამდვილი ქიმიკოსები. ";
            if (tries == 1) return "ერთი შეცდომა და მაშინვე გაასწორე — ყოჩაღ! ";
            return "სწორია! ";
        }

        /// <summary>Moves to the next step when the current one allows it (the demo can always be skipped).</summary>
        public bool Advance()
        {
            if (window == null) return false;
            if (Current == Step.Demo && !Solved)
            {
                StopDemo();
                if (Lesson.Kind == PracticeKind.Sort) for (int i = from; i < to; i++) sorted.Add(i);
            }
            else if ((Current == Step.Guided || Current == Step.Free) && !Solved) return false;
            switch (Current)
            {
                case Step.Demo: Go(Step.Guided); break;
                case Step.Guided: Go(Step.Free); break;
                case Step.Free:
                    if (practiceOnly) { GameSaveManager.Instance.AddXP(Curriculum.RepeatXP); game.HUD.Notify("ყოჩაღ! ვარჯიში დასრულდა. +" + Curriculum.RepeatXP + " XP"); window.Close(); }
                    // Lessons end on what the player built; the quiz questions live on in chapter reviews.
                    else { keys = new string[0]; Go(Step.Reward); }
                    break;
                case Step.Test:
                    if (!answered) return false;
                    question++;
                    if (question < keys.Length) RebuildQuestion(); else Go(Step.Reward);
                    break;
                case Step.Reward:
                    var upcoming = Curriculum.Next;
                    window.Close();
                    // The next activity starts here only if it lives at the same station; otherwise its station lights up.
                    if (upcoming == null) break;
                    var station = Curriculum.ChapterOf(upcoming).Station;
                    if (station == Curriculum.ChapterOf(Lesson).Station) Begin(upcoming);
                    else game.HUD.Notify("ახალი ცდა გაიხსნა: " + Curriculum.ChapterOf(upcoming).StationName + "!");
                    break;
            }
            return true;
        }

        // ------------------------------------------------------------ sorting

        private IEnumerator SortDemo()
        {
            Say("ყურადღებით დააკვირდი...", LabUI.Ink);
            for (int i = from; i < to; i++)
            {
                yield return new WaitForSecondsRealtime(1.1f);
                sorted.Add(i);
                var item = Lesson.Items[i];
                Say("„" + item.Label + "“ → " + Lesson.Bins[item.Bin] + ". " + item.Why, LabUI.Green);
                RefreshSort();
            }
            yield return new WaitForSecondsRealtime(0.6f);
            demo = null;
            MarkSolved(Feedback + "\nახლა შენი ჯერია!");
        }

        /// <summary>Places an item (index into the lesson's items) into a group.</summary>
        public void Sort(int item, int bin)
        {
            if (window == null || Lesson.Kind != PracticeKind.Sort || Current == Step.Demo || item < from || item >= to || sorted.Contains(item)) return;
            var entry = Lesson.Items[item];
            if (bin != entry.Bin)
            {
                misses[item] = misses.TryGetValue(item, out int m) ? m + 1 : 1;
                tries++; lessonMistakes++;
                AudioManager.Instance?.Wrong();
                bool reveal = Current == Step.Guided || misses[item] > 1;
                Say("„" + entry.Label + "“ — არა „" + Lesson.Bins[bin] + "“. " + (reveal ? entry.Why : "მინიშნება: " + Lesson.Hint) + "\nსცადე თავიდან.", LabUI.Coral);
                return;
            }
            sorted.Add(item);
            AudioManager.Instance?.Click();
            Say("სწორია! " + entry.Why, LabUI.Green);
            RefreshSort();
            if (Enumerable.Range(from, to - from).All(sorted.Contains)) MarkSolved(Praise() + entry.Why + "\nყველა მაგალითი დალაგებულია!");
        }

        private void RefreshSort()
        {
            foreach (Transform child in stage) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var bins = LabUI.Row(stage, "ჯგუფები", 8);
            LabUI.Fill((RectTransform)bins.transform, 10);
            bins.childForceExpandHeight = true; bins.childForceExpandWidth = true; bins.childAlignment = TextAnchor.UpperLeft;
            for (int b = 0; b < Lesson.Bins.Length; b++)
            {
                var column = LabUI.Card(bins.transform, Lesson.Bins[b], new Color32(48, 78, 66, 255), 8);
                LabUI.Size(column.gameObject, 200, -1, 1);
                column.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
                var title = LabUI.Text(column.transform, Lesson.Bins[b], 16, LabUI.Gold);
                title.fontStyle = FontStyles.Bold; title.alignment = TextAlignmentOptions.Center;
                title.enableAutoSizing = true; title.fontSizeMin = 11; title.fontSizeMax = 16;
                LabUI.Size(title.gameObject, 26);
                foreach (int i in sorted.Where(i => Lesson.Items[i].Bin == b).OrderBy(i => i))
                {
                    var chip = LabUI.Text(column.transform, Lesson.Items[i].Label, 14, LabUI.Paper);
                    chip.alignment = TextAlignmentOptions.Center;
                    chip.enableAutoSizing = true; chip.fontSizeMin = 10; chip.fontSizeMax = 14;
                    LabUI.Size(chip.gameObject, 22);
                }
            }
            foreach (Transform child in controls) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            for (int i = from; i < to; i++)
            {
                int item = i;
                var entry = Lesson.Items[i];
                var row = LabUI.Row(controls, entry.Label, 6);
                LabUI.Size(row.gameObject, 44);
                var label = LabUI.Text(row.transform, entry.Label, 16, sorted.Contains(i) ? LabUI.Green : LabUI.Ink);
                label.enableAutoSizing = true; label.fontSizeMin = 11; label.fontSizeMax = 16;
                LabUI.Size(label.gameObject, 44, -1, 1);
                if (sorted.Contains(i))
                {
                    var done = LabUI.Text(row.transform, "→ " + Lesson.Bins[entry.Bin], 15, LabUI.Green);
                    LabUI.Size(done.gameObject, 44, 170, 0);
                    continue;
                }
                for (int b = 0; b < Lesson.Bins.Length; b++)
                {
                    int bin = b;
                    var button = LabUI.Secondary(row.transform, Lesson.Bins[b], () => Sort(item, bin), 40);
                    LabUI.Size(button.gameObject, 40, Lesson.Bins.Length > 2 ? 104 : 140, 0);
                    button.interactable = Current != Step.Demo;
                }
            }
        }

        // ------------------------------------------------------------ counters

        private int[] Min() => practice.Min ?? practice.Counters.Select(_ => Lesson.Kind == PracticeKind.Equation ? 1 : 0).ToArray();
        private int[] Max() => practice.Max ?? practice.Counters.Select(_ => Lesson.Kind switch
        {
            PracticeKind.Atom => 12, PracticeKind.Molecule => 6, PracticeKind.Equation => 6, PracticeKind.Element => 30,
            PracticeKind.Ions => 4, PracticeKind.Pairs => 3, PracticeKind.Scale => 14, _ => 10,
        }).ToArray();

        private string CounterName(int i)
        {
            string name = practice.Counters[i];
            if (Lesson.Kind == PracticeKind.Molecule) return name + " • " + ElementName(name);
            if (Lesson.Kind == PracticeKind.Equation) return name + "-ის კოეფიციენტი";
            return name;
        }

        private void BuildCounters()
        {
            foreach (Transform child in controls) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var min = Min(); var max = Max();
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                var row = LabUI.Row(controls, practice.Counters[i], 8);
                LabUI.Size(row.gameObject, 44);
                bool fixedValue = min[i] == max[i];
                var caption = LabUI.Text(row.transform, CounterName(i) + ": " + values[i] + (fixedValue ? "  (ფიქსირებული)" : ""), 18, LabUI.Ink);
                caption.enableAutoSizing = true; caption.fontSizeMin = 12; caption.fontSizeMax = 18;
                LabUI.Size(caption.gameObject, 44, -1, 1);
                var minus = LabUI.Secondary(row.transform, "−", () => Adjust(index, -1), 42);
                var plus = LabUI.Secondary(row.transform, "+", () => Adjust(index, 1), 42);
                LabUI.Size(minus.gameObject, 42, 52, 0); LabUI.Size(plus.gameObject, 42, 52, 0);
                minus.interactable = plus.interactable = Current != Step.Demo && !fixedValue;
            }
        }

        /// <summary>Changes a counter by <paramref name="delta"/> within its limits.</summary>
        public void Adjust(int counter, int delta)
        {
            if (window == null || practice == null || Current == Step.Demo || counter < 0 || counter >= values.Length || Solved && Current == Step.Free) return;
            values[counter] = Mathf.Clamp(values[counter] + delta, Min()[counter], Max()[counter]);
            BuildCounters();
            Draw();
            if (Current != Step.Guided) { Say("როცა მზად იქნები, დააჭირე „შემოწმებას“.", LabUI.Ink); return; }
            string problem = Problem();
            if (problem == null) MarkSolved(Praise() + Result());
            else Say("მინიშნება: " + problem, LabUI.Coral);
        }

        /// <summary>Free practice: checks the current answer and names what is still wrong.</summary>
        public void Check()
        {
            if (window == null || practice == null || Current != Step.Free || Solved) return;
            string problem = Problem();
            if (problem == null) MarkSolved(Praise() + Result());
            else { tries++; lessonMistakes++; AudioManager.Instance?.Wrong(); Say("ჯერ არა. " + problem + "\nშეცვალე და კიდევ სცადე.", LabUI.Coral); }
        }

        private IEnumerator BuildDemo()
        {
            Say("ყურადღებით დააკვირდი...", LabUI.Ink);
            yield return new WaitForSecondsRealtime(0.6f);
            while (true)
            {
                int i = 0;
                while (i < values.Length && values[i] == practice.Solution[i]) i++;
                if (i >= values.Length) break;
                values[i] += Math.Sign(practice.Solution[i] - values[i]);
                BuildCounters();
                Draw();
                yield return new WaitForSecondsRealtime(0.4f);
            }
            demo = null;
            MarkSolved(practice.Done + "\nახლა შენი ჯერია!");
        }

        private void StopDemo()
        {
            if (demo != null) StopCoroutine(demo);
            demo = null;
            if (practice != null && Current == Step.Demo && values != null) { values = practice.Solution.ToArray(); if (controls != null) { BuildCounters(); Draw(); } }
        }

        /// <summary>Null when the exercise is solved, otherwise what was counted wrong.</summary>
        private string Problem()
        {
            if (practice.Check != null) return practice.Check(values);
            switch (Lesson.Kind)
            {
                case PracticeKind.Equation: return EquationProblem();
                case PracticeKind.Ions:
                {
                    int charge = values[0] * practice.Charges[0] + values[1] * practice.Charges[1];
                    if (values[0] == 0 || values[1] == 0) return "ორივე სახის იონი გჭირდება: დადებითიც და უარყოფითიც.";
                    if (charge > 0) return "ჯამური მუხტი +" + charge + "-ია. დაამატე უარყოფითი იონი ან მოაკელი დადებითი.";
                    if (charge < 0) return "ჯამური მუხტი " + charge + "-ია. დაამატე დადებითი იონი ან მოაკელი უარყოფითი.";
                    return Gcd(values[0], values[1]) > 1 ? "მუხტები ბალანსშია, მაგრამ უმცირესი რაოდენობები გამოიყენე." : null;
                }
                case PracticeKind.Beaker:
                {
                    float c = Concentration();
                    if (Mathf.Abs(c - practice.Target) < 0.001f) return null;
                    return "ახლა C = n / V = " + (values[0] / 10f).ToString("0.0") + " / " + (values[1] / 10f).ToString("0.0") + " = " + c.ToString("0.00") + " მოლ/ლ. საჭიროა " + practice.Target.ToString("0.00") + " მოლ/ლ — " +
                        (c < practice.Target ? "მეტი ნივთიერება ან ნაკლები მოცულობა." : "ნაკლები ნივთიერება ან მეტი წყალი.");
                }
                case PracticeKind.Element:
                {
                    int z = values[0], target = practice.Solution[0];
                    if (z == target) return null;
                    var e = ElementBy(z);
                    return "ახლა არჩეულია " + (e != null ? e.georgianName + " (" + e.symbol + ", " + z + ")" : "ნომერი " + z) + ". " + (z < target ? "საჭირო ელემენტი უფრო შორსაა — გაზარდე ნომერი." : "საჭირო ელემენტი უფრო ადრეა — შეამცირე ნომერი.");
                }
                case PracticeKind.Scale:
                {
                    int ph = values[0], target = practice.Solution[0];
                    if (ph == target) return null;
                    return "pH " + ph + " — " + PHName(ph) + ". " + (ph < target ? "საჭირო ხსნარი ნაკლებად მჟავეა — გაზარდე pH." : "საჭირო ხსნარი უფრო მჟავეა — შეამცირე pH.");
                }
                case PracticeKind.Pairs:
                {
                    int pairs = values[0], target = practice.Solution[0];
                    if (pairs == target) return null;
                    return "საერთო წყვილები: გაქვს " + pairs + ". " + (pairs < target ? "ატომებს ჯერ კიდევ აკლიათ ელექტრონები — დაამატე წყვილი." : "ზედმეტი წყვილია — ატომებს ამდენი ელექტრონი არ აკლიათ.");
                }
                default:
                    for (int i = 0; i < values.Length; i++)
                        if (values[i] != practice.Solution[i]) return CounterName(i) + ": გაქვს " + values[i] + ", საჭიროა " + practice.Solution[i] + ".";
                    return null;
            }
        }

        private string EquationProblem()
        {
            if (values.Any(v => v < 1)) return "თითოეულ ნივთიერებას მინიმუმ 1 კოეფიციენტი სჭირდება.";
            var (left, right) = Sides();
            foreach (string element in left.Keys.Union(right.Keys))
            {
                int l = left.TryGetValue(element, out int a) ? a : 0, r = right.TryGetValue(element, out int b) ? b : 0;
                if (l != r) return element + ": მარცხნივ " + l + " ატომია, მარჯვნივ — " + r + ". შეცვალე კოეფიციენტი, ინდექსი კი უცვლელი დატოვე.";
            }
            return values.Aggregate(Gcd) > 1 ? "განტოლება ბალანსშია, მაგრამ უმცირესი მთელი რიცხვები გამოიყენე." : null;
        }

        private int LeftCount => practice.Formula.Split(new[] { "->" }, StringSplitOptions.None)[0].Split('+').Length;
        private string SideText(int start, int end) =>
            string.Join(" + ", Enumerable.Range(start, end - start).Select(i => (values[i] == 1 ? "" : values[i].ToString()) + practice.Counters[i]));
        private (Dictionary<string, int> left, Dictionary<string, int> right) Sides() =>
            (ChemistryChallenges.Atoms(SideText(0, LeftCount)), ChemistryChallenges.Atoms(SideText(LeftCount, values.Length)));
        private float Concentration() => values[0] / Mathf.Max(1f, values[1]);
        private static int Gcd(int a, int b) { a = Math.Abs(a); b = Math.Abs(b); while (b != 0) (a, b) = (b, a % b); return a; }
        private double DropsPH() => ChemistrySandbox.PH(values[0], values[1], 20);
        private static string PHName(int ph) => ph < 7 ? (ph <= 3 ? "ძლიერი მჟავა" : "სუსტი მჟავა") : ph == 7 ? "ნეიტრალური" : ph >= 11 ? "ძლიერი ფუძე" : "სუსტი ფუძე";

        // ------------------------------------------------------------ drawing

        private void Draw()
        {
            foreach (Transform child in stage) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            switch (Lesson.Kind)
            {
                case PracticeKind.Atom: DrawAtom(); break;
                case PracticeKind.Molecule: DrawMolecule(); break;
                case PracticeKind.Equation: DrawEquation(); break;
                case PracticeKind.Element: DrawElement(); break;
                case PracticeKind.Ions: DrawIons(); break;
                case PracticeKind.Pairs: DrawPairs(); break;
                case PracticeKind.Beaker: DrawBeaker(); break;
                case PracticeKind.Drops: DrawDrops(); break;
                case PracticeKind.Scale: DrawScale(); break;
            }
        }

        private static Sprite Circle() => circle ??= PixelArt.Make(32, 32, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15.5f ? Color.white : Color.clear);
        private Image Shape(string name, Vector2 point, Vector2 size, Color color, bool round = false)
        {
            var image = LabUI.Panel(stage, name, color);
            if (round) image.sprite = Circle();
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
            image.rectTransform.anchoredPosition = point;
            image.rectTransform.sizeDelta = size;
            image.raycastTarget = false;
            return image;
        }
        private void Ball(string symbol, Vector2 point, float diameter, Color color)
        {
            var image = Shape(symbol, point, Vector2.one * diameter, color, true);
            var text = LabUI.Text(image.transform, symbol, diameter > 40 ? 20 : 13, Color.white);
            text.alignment = TextAlignmentOptions.Center;
            LabUI.Fill(text.rectTransform);
        }
        private void Label(string value, Vector2 point, float size = 18, Color? color = null, float width = 440)
        {
            var text = LabUI.Text(stage, value, size, color ?? Color.white);
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true; text.fontSizeMin = 11; text.fontSizeMax = size;
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = Vector2.one * 0.5f;
            text.rectTransform.anchoredPosition = point;
            text.rectTransform.sizeDelta = new Vector2(width, 40);
        }
        private void Line(Vector2 a, Vector2 b, Color color, float thickness = 5)
        {
            var delta = b - a;
            var image = Shape("ხაზი", (a + b) / 2, new Vector2(delta.magnitude, thickness), color);
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private ElementCollectionSO.Element ElementBy(int z) => game.elements != null ? game.elements.elements.FirstOrDefault(e => e.atomicNumber == z) : null;
        private string ElementName(string symbol) => game.elements?.elements.FirstOrDefault(e => e.symbol == symbol)?.georgianName ?? symbol;
        private static Color AtomColor(string symbol) => symbol switch
        {
            "H" => LabUI.Mint, "O" => LabUI.Coral, "C" => new Color32(120, 126, 118, 255), "N" => new Color32(96, 120, 200, 255),
            "Na" => new Color32(190, 140, 60, 255), "Cl" => new Color32(120, 190, 90, 255), _ => LabUI.Green,
        };

        private void DrawAtom()
        {
            int p = values[0], n = values[1], e = values[2];
            var element = ElementBy(p);
            Label(element != null ? element.symbol + " • " + element.georgianName : "ატომი", new Vector2(0, 128), 20, LabUI.Gold);
            float[] radii = { 52, 82, 110 };
            foreach (float radius in radii)
                for (int i = 0; i < 40; i++)
                {
                    float angle = i * Mathf.PI / 20;
                    Shape("შრე", new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, Vector2.one * 2, new Color32(103, 113, 96, 255), true);
                }
            for (int i = 0; i < p + n; i++)
            {
                float angle = i * 2.4f, radius = Mathf.Sqrt(i) * 6;
                Shape(i < p ? "პროტონი" : "ნეიტრონი", new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, Vector2.one * 13, i < p ? LabUI.Coral : LabUI.Gold, true);
            }
            int[] capacity = { 2, 8, 8 };
            for (int i = 0, placed = 0, shell = 0; i < e && shell < 3; i++)
            {
                if (placed >= capacity[shell]) { shell++; placed = 0; if (shell >= 3) break; }
                float angle = placed * Mathf.PI * 2 / capacity[shell] + shell * 0.3f;
                Ball("−", new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radii[shell], 18, LabUI.Mint);
                placed++;
            }
            int charge = p - e;
            Label("მასური რიცხვი: " + (p + n) + "    მუხტი: " + (charge > 0 ? "+" : "") + charge + (charge == 0 ? " (ნეიტრალური)" : " (იონი)"), new Vector2(0, -132), 17);
            Label("წითელი — პროტონი • ოქროსფერი — ნეიტრონი • მწვანე — ელექტრონი", new Vector2(0, -156), 12, new Color32(188, 193, 180, 255));
        }

        private static readonly string[] FormulaOrder = { "Na", "C", "N", "H", "O", "Cl" };
        private void DrawMolecule()
        {
            var counts = practice.Counters.Select((s, i) => (s, n: values[i])).Where(c => c.n > 0).OrderBy(c => Array.IndexOf(FormulaOrder, c.s)).ToList();
            string formula = string.Concat(counts.Select(c => c.s + (c.n > 1 ? c.n.ToString() : "")));
            Label(formula.Length > 0 ? "შენი ფორმულა: " + formula : "დაამატე პირველი ატომი", new Vector2(0, 125), 22, LabUI.Gold);
            var atoms = counts.SelectMany(c => Enumerable.Repeat(c.s, c.n)).ToList();
            for (int i = 0; i < atoms.Count; i++)
            {
                int row = i / 6, col = i % 6, inRow = Mathf.Min(6, atoms.Count - row * 6);
                var point = new Vector2((col - (inRow - 1) / 2f) * 64, 30 - row * 70);
                if (col > 0) Line(point - new Vector2(64, 0), point, new Color32(192, 207, 228, 255), 4);
                Ball(atoms[i], point, atoms[i].Length > 1 ? 52 : 46, AtomColor(atoms[i]));
            }
            Label("ფორმულაში ინდექსი = ატომების რაოდენობა", new Vector2(0, -130), 15, new Color32(188, 193, 180, 255));
        }

        private void DrawEquation()
        {
            int split = LeftCount;
            Label(SideText(0, split) + "  →  " + SideText(split, values.Length), new Vector2(0, 118), 26, LabUI.Gold);
            var (left, right) = Sides();
            var elements = left.Keys.Union(right.Keys).ToList();
            Label("ატომების დათვლა", new Vector2(0, 70), 16, new Color32(188, 193, 180, 255));
            for (int i = 0; i < elements.Count; i++)
            {
                int l = left.TryGetValue(elements[i], out int a) ? a : 0, r = right.TryGetValue(elements[i], out int b) ? b : 0;
                var color = l == r ? LabUI.Mint : LabUI.Coral;
                float y = 30 - i * 48;
                Shape("მწკრივი", new Vector2(0, y), new Vector2(400, 40), new Color(color.r, color.g, color.b, 0.22f));
                Label(elements[i] + ":   მარცხნივ " + l + "   |   მარჯვნივ " + r + (l == r ? "   — ტოლია" : "   — არ ემთხვევა"), new Vector2(0, y), 19, color);
            }
        }

        private void DrawElement()
        {
            int z = values[0];
            for (int offset = -2; offset <= 2; offset++)
            {
                var e = ElementBy(z + offset);
                if (e == null) continue;
                bool centre = offset == 0;
                var size = centre ? new Vector2(150, 150) : new Vector2(70, 70);
                var point = new Vector2(offset * 105 + Math.Sign(offset) * 30, centre ? 20 : 20);
                var tile = Shape(e.symbol, point, size, centre ? e.color : new Color(e.color.r, e.color.g, e.color.b, 0.45f));
                var text = LabUI.Text(tile.transform, (centre ? "<size=18>" + e.atomicNumber + "</size>\n<size=54>" + e.symbol + "</size>\n<size=16>" + e.georgianName + "</size>" : e.atomicNumber + "\n" + e.symbol), centre ? 20 : 15, Color.white);
                text.alignment = TextAlignmentOptions.Center; text.lineSpacing = -10;
                LabUI.Fill(text.rectTransform, 4);
            }
            var current = ElementBy(z);
            Label("ატომური ნომერი " + z + (current != null ? " • " + current.georgianName : ""), new Vector2(0, -125), 19);
        }

        private void DrawIons()
        {
            for (int k = 0; k < 2; k++)
                for (int i = 0; i < values[k]; i++)
                    Ball(practice.Counters[k], new Vector2((i - (values[k] - 1) / 2f) * 74, k == 0 ? 70 : -15), 58, k == 0 ? new Color32(190, 140, 60, 255) : LabUI.Green);
            int charge = values[0] * practice.Charges[0] + values[1] * practice.Charges[1];
            Label("ჯამური მუხტი: " + (charge > 0 ? "+" : "") + charge + (charge == 0 ? " — ნეიტრალური ნაერთი" : ""), new Vector2(0, -105), 21, charge == 0 ? LabUI.Mint : LabUI.Coral);
            Label("საწინააღმდეგო მუხტები ერთმანეთს იზიდავს", new Vector2(0, -140), 14, new Color32(188, 193, 180, 255));
        }

        private void DrawPairs()
        {
            string symbol = practice.Formula;
            int pairs = values[0];
            for (int i = 0; i < pairs; i++)
            {
                float y = (i - (pairs - 1) / 2f) * 22;
                Line(new Vector2(-50, y), new Vector2(50, y), new Color32(192, 207, 228, 255), 6);
                Ball("•", new Vector2(-12, y + 9), 10, LabUI.Mint);
                Ball("•", new Vector2(12, y + 9), 10, LabUI.Mint);
            }
            Ball(symbol, new Vector2(-90, 0), 84, AtomColor(symbol));
            Ball(symbol, new Vector2(90, 0), 84, AtomColor(symbol));
            string bond = pairs == 1 ? "–" : pairs == 2 ? "=" : pairs == 3 ? "≡" : " ";
            Label("ბმა: " + symbol + bond + symbol + "   •   საერთო წყვილები: " + pairs, new Vector2(0, 118), 20, LabUI.Gold);
            Label("ერთი ხაზი = ერთი საერთო ელექტრონული წყვილი", new Vector2(0, -120), 14, new Color32(188, 193, 180, 255));
        }

        private void DrawBeaker()
        {
            float n = values[0] / 10f, v = values[1] / 10f, c = Concentration();
            Label("C = n / V = " + n.ToString("0.0") + " / " + v.ToString("0.0") + " = " + c.ToString("0.00") + " მოლ/ლ", new Vector2(0, 132), 20, LabUI.Gold);
            Shape("ჭიქა", new Vector2(0, -15), new Vector2(230, 220), new Color32(183, 203, 219, 255));
            Shape("ჭიქის შიდა მხარე", new Vector2(0, -10), new Vector2(214, 214), LabUI.Navy);
            float height = v / 2f * 200;
            Shape("ხსნარი", new Vector2(0, -117 + height / 2), new Vector2(210, height), Color.Lerp(new Color(0.55f, 0.82f, 0.95f), LabUI.Green, Mathf.Clamp01(c / 2)));
            for (int i = 0; i < values[0] * 4; i++)
                Shape("ნაწილაკი", new Vector2(-95 + i * 47 % 190, -112 + (i * 31 % 97) / 97f * Mathf.Max(4, height - 10)), Vector2.one * 7, Color.white, true);
            Label("მიზანი: " + practice.Target.ToString("0.00") + " მოლ/ლ  •  თეთრი წერტილები — გახსნილი ნივთიერება", new Vector2(0, -150), 13, new Color32(188, 193, 180, 255));
        }

        private void DrawDrops()
        {
            double ph = DropsPH();
            var tint = ExperimentGamesUI.Indicator((float)ph);
            Label("pH ≈ " + ph.ToString("0.0") + " • " + PHName((int)Math.Round(ph)), new Vector2(0, 130), 21, LabUI.Gold);
            Shape("ჭიქა", new Vector2(0, 10), new Vector2(180, 170), new Color32(186, 204, 216, 255));
            Shape("ხსნარი", new Vector2(0, 0), new Vector2(164, 135), tint);
            Label("მჟავა: " + values[0] + " წვეთი   •   ფუძე: " + values[1] + " წვეთი", new Vector2(0, 0), 15);
            DrawStrip(-112, (float)ph);
        }

        private void DrawScale()
        {
            int ph = values[0];
            Label("pH " + ph + " — " + PHName(ph), new Vector2(0, 110), 26, LabUI.Gold);
            Shape("ნიმუში", new Vector2(0, 30), new Vector2(120, 90), ExperimentGamesUI.Indicator(ph));
            DrawStrip(-70, ph);
        }

        private void DrawStrip(float y, float ph)
        {
            for (int i = 0; i <= 14; i++) Shape("pH ფერი", new Vector2(-196 + i * 28, y), new Vector2(26, 26), ExperimentGamesUI.Indicator(i));
            Shape("მაჩვენებელი", new Vector2(-196 + Mathf.Clamp(ph, 0, 14) * 28, y + 24), new Vector2(6, 18), Color.white);
            Label("0   მჟავა          7          ფუძე   14", new Vector2(0, y - 32), 15);
        }

        // ================================================================== small test

        private void BuildQuestion()
        {
            answered = false;
            var q = Question;
            var info = LabUI.Row(window.Body, "კითხვის ინფორმაცია", 10);
            LabUI.Size(info.gameObject, 30);
            LabUI.Pill(info.transform, (Lesson.IsReview ? "შემაჯამებელი ტესტი" : "პატარა ტესტი") + " • კითხვა " + (question + 1) + " / " + keys.Length, LabUI.Green);
            LabUI.Pill(info.transform, "შეცდომა არაუშავს: აგიხსნი და თავიდან სცდი", new Color32(150, 156, 140, 255));
            var panel = LabUI.Panel(window.Body, "კითხვა", LabUI.Navy);
            LabUI.Size(panel.gameObject, 110);
            var prompt = LabUI.Text(panel.transform, q.Prompt, 24, LabUI.Paper);
            LabUI.Fill(prompt.rectTransform, 16);
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.enableAutoSizing = true; prompt.fontSizeMin = 15; prompt.fontSizeMax = 24;
            var order = Enumerable.Range(0, q.Choices.Length).OrderBy(_ => UnityEngine.Random.value).ToArray();
            for (int k = 0; k < order.Length; k++)
            {
                int index = order[k];
                var button = LabUI.Button(window.Body, (char)('ა' + k) + ")  " + q.Choices[index], () => Answer(index), 52);
                button.GetComponent<Image>().color = new Color32(55, 91, 77, 255);
                button.name = "პასუხი " + index;
                choiceButtons.Add(button);
            }
            Banner("აირჩიე პასუხი. შეცდომის შემთხვევაში მოკლედ აგიხსნი, რატომაა ასე.", question + 1 < keys.Length ? "გაგრძელება →" : "დასრულება →");
            var footer = LabUI.Row(window.Body, "ქმედებები", 10);
            LabUI.Size(LabUI.Secondary(footer.transform, "მინიშნება", Hint, 42).gameObject, 42, 180, 0);
            LabUI.Size(LabUI.Secondary(footer.transform, "რვეული [N]", () => game.Notebook.Open(1), 42).gameObject, 42, 160, 0);
            next.interactable = false;
        }

        private void RebuildQuestion()
        {
            foreach (Transform child in window.Body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            choiceButtons.Clear();
            Header();
            BuildQuestion();
        }

        public void Hint() { if (Question != null && !answered) Say("მინიშნება: " + Question.Hint, LabUI.Ink); }

        /// <summary>Answers the current question; <paramref name="choice"/> indexes the original choices (0 is right).</summary>
        public void Answer(int choice)
        {
            var q = Question;
            if (window == null || Current != Step.Test || q == null || answered || choice < 0 || choice >= q.Choices.Length) return;
            if (choice != 0)
            {
                wrong.Add(keys[question]);
                lessonMistakes++;
                AudioManager.Instance?.Wrong();
                foreach (var b in choiceButtons) if (b.name == "პასუხი " + choice) b.interactable = false;
                Say("ჯერ არა. " + q.Why[choice] + "\nმინიშნება: " + q.Hint + " სცადე სხვა პასუხი.", LabUI.Coral);
                return;
            }
            answered = true;
            foreach (var b in choiceButtons) b.interactable = false;
            AudioManager.Instance?.Correct();
            Say("სწორია! " + q.Why[0], LabUI.Green);
            next.interactable = true;
            if (progressFill != null) StartCoroutine(Grow(progressFill, Progress(false)));
        }

        // ================================================================== reward

        private void BuildReward()
        {
            Rewards = Curriculum.Finish(Lesson, keys, wrong, lessonMistakes == 0);
            AudioManager.Instance?.Milestone(); AudioManager.Instance?.Coin();
            int answers = Lesson.IsReview ? keys.Length : Lesson.Kind == PracticeKind.Sort ? Mathf.Max(1, Lesson.Items.Length - 2) : 2;
            int seconds = Mathf.RoundToInt(Time.unscaledTime - startedAt);
            var tiles = LabUI.Row(window.Body, "შედეგები", 16);
            LabUI.Size(tiles.gameObject, 92);
            Tile(tiles.transform, "სულ XP", "+" + (GameSaveManager.Instance.Data.totalXP - xpAtStart), LabUI.Gold);
            Tile(tiles.transform, lessonMistakes == 0 ? "უშეცდომოდ!" : "სიზუსტე", answers * 100 / (answers + lessonMistakes) + "%", LabUI.Green);
            Tile(tiles.transform, "დრო", seconds / 60 + ":" + (seconds % 60).ToString("00"), new Color32(70, 140, 200, 255));
            int score = keys.Count(k => !wrong.Contains(k));
            var title = LabUI.Text(window.Body, Lesson.IsReview ? "თავი „" + Curriculum.ChapterOf(Lesson).Title + "“ დასრულებულია!" : "გაკვეთილი დასრულებულია!", 30, LabUI.Green);
            title.fontStyle = FontStyles.Bold; LabUI.Size(title.gameObject, 44);
            var summary = LabUI.Text(window.Body, !Lesson.IsReview ? "შენ თავად გააკეთე! აი, რა აღმოაჩინე: " + Lesson.Teach[0]
                : "პირველივე ცდით სწორი პასუხები: " + score + " / " + keys.Length +
                (score < keys.Length ? "  •  შეცდომებს მასწავლებელი მოგვიანებით გაგიმეორებს." : "  •  უშეცდომოდ! ბრწყინვალეა."), 18, LabUI.Ink);
            summary.enableAutoSizing = true; summary.fontSizeMin = 12; summary.fontSizeMax = 18;
            LabUI.Size(summary.gameObject, 52);
            var row = LabUI.Row(window.Body, "ჯილდო და ჩანაწერი", 16);
            LabUI.FlexibleHeight(row.gameObject);
            row.childForceExpandHeight = true;
            var gifts = LabUI.Card(row.transform, "ჯილდოები", LabUI.Navy, 16);
            LabUI.Size(gifts.gameObject, 200, 470, 0);
            var giftsTitle = Fit(LabUI.Text(gifts.transform, "შენ მიიღე", 20, LabUI.Gold), 30); giftsTitle.fontStyle = FontStyles.Bold;
            foreach (string reward in Rewards) Fit(LabUI.Text(gifts.transform, "• " + reward, 17, LabUI.Paper), 44);
            var note = LabUI.Card(row.transform, "რვეული", LabUI.Soft, 16);
            LabUI.Size(note.gameObject, 200, -1, 1);
            var noteTitle = Fit(LabUI.Text(note.transform, Lesson.IsReview ? "რა ისწავლე ამ თავში" : "რვეულის ჩანაწერი", 20, LabUI.Green), 30); noteTitle.fontStyle = FontStyles.Bold;
            Fit(LabUI.Text(note.transform, Lesson.IsReview ? string.Join("\n", Curriculum.Of(Lesson.Chapter).Where(l => !l.IsReview).Select(l => "• " + l.Title)) : Lesson.Note, 16, LabUI.Ink), 150);
            var footer = LabUI.Row(window.Body, "ქმედებები", 10);
            LabUI.Size(LabUI.Secondary(footer.transform, "სასწავლო რუკა [M]", () => { window.Close(); OpenMap(); }, 44).gameObject, 44, 220, 0);
            LabUI.Size(LabUI.Secondary(footer.transform, "დახურვა", () => window.Close(), 44).gameObject, 44, 150, 0);
            var upcoming = Curriculum.Next;
            next = LabUI.Button(footer.transform, upcoming != null ? "შემდეგი გაკვეთილი: " + upcoming.Title + " →" : "რთული გამოწვევები გაიხსნა! →", () =>
            {
                if (upcoming != null) Advance();
                else { window.Close(); game.HUD.Notify("რთული გამოწვევების მაგიდა კლასის ქვედა მარცხენა კუთხეშია."); }
            }, 44);
        }

        /// <summary>A coloured tile with a title strip, like the end-of-lesson stats in Duolingo.</summary>
        private static void Tile(Transform parent, string title, string value, Color color)
        {
            var tile = LabUI.Panel(parent, title, color);
            LabUI.Size(tile.gameObject, 92, -1, 1);
            var column = LabUI.Column(tile.transform, "შიგთავსი", 0);
            LabUI.Fill((RectTransform)column.transform, 4);
            var head = LabUI.Text(column.transform, title, 15, Color.white);
            head.alignment = TextAlignmentOptions.Center; head.fontStyle = FontStyles.Bold;
            LabUI.Size(head.gameObject, 26);
            var body = LabUI.Panel(column.transform, "მნიშვნელობა", Color.white);
            LabUI.FlexibleHeight(body.gameObject);
            var number = LabUI.Text(body.transform, value, 28, color);
            number.alignment = TextAlignmentOptions.Center; number.fontStyle = FontStyles.Bold;
            LabUI.Fill(number.rectTransform);
        }

        // ================================================================== learning map

        public void ToggleMap() { if (map != null) map.Close(); else OpenMap(); }

        /// <summary>One colour per chapter, used for its banner and finished lesson circles.</summary>
        private static readonly Color32[] ChapterColors =
        {
            new Color32(88, 170, 90, 255), new Color32(70, 140, 210, 255), new Color32(150, 100, 200, 255), new Color32(40, 160, 160, 255),
            new Color32(230, 140, 50, 255), new Color32(220, 90, 90, 255), new Color32(60, 170, 220, 255), new Color32(210, 90, 160, 255),
        };
        private static readonly Color Locked = new Color32(196, 198, 188, 255);
        /// <summary>Sideways offsets that make the lesson path wind left and right, like Duolingo's.</summary>
        private static readonly float[] Zigzag = { 0, 70, 110, 70, 0, -70, -110, -70 };

        /// <summary>The learning path: chapter banners and a winding line of round lesson buttons.</summary>
        public void OpenMap()
        {
            if (map != null) { LabUI.BringToFront(map); return; }
            var created = LabUI.Window("სასწავლო გზა", 1120, 670);
            created.Closed = () => { if (map == created) map = null; };
            map = created;
            var next = Curriculum.Next;
            var progressRow = LabUI.Row(map.Body, "პროგრესი", 12);
            LabUI.Size(progressRow.gameObject, 26);
            LabUI.Size(LabUI.Text(progressRow.transform, "პროგრესი: " + Curriculum.Percent + "%  •  წოდება: " + Curriculum.Title + "  •  სულ XP: " + GameSaveManager.Instance.Data.totalXP, 17, LabUI.Ink).gameObject, 26, 620, 0);
            var bar = LabUI.Progress(progressRow.transform, Curriculum.Percent / 100f, LabUI.Mint, 12);
            LabUI.Size(bar.transform.parent.gameObject, 12, -1, 1);

            var split = LabUI.Row(map.Body, "რუკის ნაწილები", 18);
            LabUI.FlexibleHeight(split.gameObject);
            split.childForceExpandHeight = true;
            var left = LabUI.Column(split.transform, "ახლა", 10);
            LabUI.Size(left.gameObject, 300, 360, 0);
            var now = LabUI.Card(left.transform, "მიმდინარე გაკვეთილი", LabUI.Navy, 14);
            var heading = Fit(LabUI.Text(now.transform, next != null ? "ახლა: " + Curriculum.Label(next) : "თეორიის გზა დასრულებულია!", 20, LabUI.Gold), 30);
            heading.fontStyle = FontStyles.Bold;
            Fit(LabUI.Text(now.transform, next != null ? next.Why : "რთული გამოწვევები, კვირის რეიტინგი და სამეცნიერო ბაზრობა შენ გელოდება.", 15, LabUI.Paper), 62);
            Fit(LabUI.Text(now.transform, next != null ? "სად: " + Curriculum.ChapterOf(next).StationName : "რთული გამოწვევების მაგიდა კლასის ქვედა მარცხენა კუთხეშია.", 15, LabUI.Mint), 26);
            if (next != null) LabUI.Button(now.transform, "გაკვეთილის დაწყება →", () => Begin(next), 46);
            LabUI.Secondary(now.transform, "რვეული [N]", () => game.Notebook.Open(1), 40);
            var mistakes = Curriculum.Mistakes.Take(3).ToList();
            if (mistakes.Count > 0)
            {
                var card = LabUI.Card(left.transform, "შეცდომები", new Color32(246, 222, 205, 255), 12);
                Fit(LabUI.Text(card.transform, "გასამეორებელი შეცდომები", 17, new Color32(151, 62, 45, 255)), 24).fontStyle = FontStyles.Bold;
                foreach (var (lesson, q) in mistakes)
                {
                    var row = LabUI.Row(card.transform, "შეცდომა", 8);
                    LabUI.Size(Fit(LabUI.Text(row.transform, q.Prompt, 13, LabUI.Ink), 40).gameObject, 40, -1, 1);
                    var l = lesson;
                    LabUI.Size(LabUI.Secondary(row.transform, "გამეორება", () => { map.Close(); Begin(l); }, 36).gameObject, 36, 110, 0);
                }
            }

            var scroll = LabUI.Scroll(split.transform);
            LabUI.Size(scroll.parent.parent.gameObject, -1, -1, 1);
            RectTransform current = null;
            for (int c = 1; c <= Curriculum.Chapters.Length; c++)
            {
                var chapter = Curriculum.Chapters[c - 1];
                Color color = ChapterColors[c - 1];
                bool reached = Curriculum.Of(c).Any(l => Curriculum.Available(l));
                var unit = LabUI.Panel(scroll, chapter.Title, reached ? color : Locked);
                LabUI.Size(unit.gameObject, 64);
                var unitText = LabUI.Text(unit.transform, "<b>თავი " + c + " • " + chapter.Title + "</b>\n<size=14>" + chapter.StationName + (Curriculum.ChapterDone(c) ? "  •  დასრულებულია" : "") + "</size>", 19, Color.white);
                LabUI.Fill(unitText.rectTransform, 12);
                var lessons = Curriculum.Of(c);
                for (int j = 0; j < lessons.Length; j++)
                {
                    var node = Node(scroll, lessons[j], color, Zigzag[j % Zigzag.Length]);
                    if (lessons[j] == next) current = node;
                }
            }
            var finale = LabUI.Panel(scroll, "რთული გამოწვევები", Curriculum.Complete ? (Color)LabUI.Gold : Locked);
            LabUI.Size(finale.gameObject, 64);
            LabUI.Fill(LabUI.Text(finale.transform, "<b>რთული გამოწვევები</b>\n<size=14>" + (Curriculum.Complete ? "გაიხსნა! მაგიდა კლასის ქვედა მარცხენა კუთხეშია." : "გაიხსნება, როცა მთელ გზას გაივლი.") + "</size>", 19, Color.white).rectTransform, 12);
            if (current != null) StartCoroutine(ScrollTo(scroll, current));
        }

        /// <summary>A round lesson button: coloured when done, pulsing when current, grey and locked otherwise.</summary>
        private RectTransform Node(Transform parent, Lesson lesson, Color color, float offset)
        {
            bool done = Curriculum.Done(lesson.ID), current = lesson == Curriculum.Next;
            var row = LabUI.Row(parent, lesson.ID, 0);
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childForceExpandWidth = false;
            LabUI.Size(row.gameObject, current ? 126 : 112);
            var spacer = LabUI.Object(row.transform, "გადახრა");
            LabUI.Size(spacer, 1, Mathf.Abs(offset) * 2, 0);
            var column = LabUI.Column(row.transform, "გაკვეთილი", 4);
            column.childAlignment = TextAnchor.UpperCenter;
            column.childForceExpandWidth = false;
            LabUI.Size(column.gameObject, -1, 240, 0);
            if (offset < 0) spacer.transform.SetAsLastSibling();
            float size = current ? 88 : 74;
            var circle = LabUI.Panel(column.transform, "წრე", done || current ? color : Locked);
            circle.sprite = Circle();
            LabUI.Size(circle.gameObject, size, size, 0);
            circle.raycastTarget = true;
            var button = circle.gameObject.AddComponent<Button>();
            button.targetGraphic = circle;
            button.onClick.AddListener(() => { AudioManager.Instance?.Click(); if (Curriculum.Available(lesson) && map != null) map.Close(); Begin(lesson); });
            circle.gameObject.AddComponent<ButtonMotion>();
            var icon = LabUI.Icon(circle.transform, lesson.IsReview ? LabIcon.Trophy : done ? LabIcon.Check : current ? LabIcon.Flask : LabIcon.Lock, done ? (Color)LabUI.Gold : Color.white);
            LabUI.Fill(icon.rectTransform, size * 0.24f);
            icon.raycastTarget = false;
            var title = LabUI.Text(column.transform, current ? "ახლა: " + lesson.Title : lesson.Title, 13, current ? new Color32(176, 120, 38, 255) : done ? LabUI.Ink : LabUI.Muted);
            title.alignment = TextAlignmentOptions.Center;
            if (current) title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true; title.fontSizeMin = 10; title.fontSizeMax = 13;
            LabUI.Size(title.gameObject, 30, 240, 0);
            if (current) StartCoroutine(Pulse(circle.transform));
            return (RectTransform)circle.transform;
        }

        private IEnumerator Pulse(Transform target)
        {
            while (target != null && map != null)
            {
                target.localScale = Vector3.one * (1 + 0.06f * Mathf.Sin(Time.unscaledTime * 4));
                yield return null;
            }
        }

        /// <summary>Opens the path scrolled so the current lesson sits in the middle.</summary>
        private static IEnumerator ScrollTo(RectTransform content, RectTransform target)
        {
            yield return null;
            if (content == null || target == null) yield break;
            Canvas.ForceUpdateCanvases();
            var scroll = content.GetComponentInParent<ScrollRect>();
            float view = scroll.viewport.rect.height, total = content.rect.height;
            if (total <= view) yield break;
            float y = -content.InverseTransformPoint(target.position).y - view / 2;
            scroll.verticalNormalizedPosition = 1 - Mathf.Clamp01(y / (total - view));
        }

        // ================================================================== station hubs

        /// <summary>A classroom station: practice for its lessons and the matching lab model.</summary>
        public void OpenStation(StationType type, string name)
        {
            var first = Curriculum.StationLesson(type);
            if (first == null) return;
            if (!Curriculum.StationOpen(type)) { game.HUD.Notify(Curriculum.LockedText + " საჭიროა: „" + first.Title + "“."); return; }
            if (hub != null) hub.Close();
            var created = LabUI.Window(name, 980, 620);
            created.Closed = () => { if (hub == created) hub = null; };
            hub = created;
            var chapters = Curriculum.Chapters.Select((c, i) => (c, i: i + 1)).Where(p => p.c.Station == type).ToList();
            var intro = LabUI.Text(hub.Body, "ამ მაგიდაზე ივარჯიშებ: " + string.Join(", ", chapters.Select(p => p.c.Title)) + ". ნასწავლი გაკვეთილები ნებისმიერ დროს გაიმეორე.", 17, LabUI.Muted);
            LabUI.Size(intro.gameObject, 46);
            var scroll = LabUI.Scroll(hub.Body);
            foreach (var (chapter, number) in chapters)
            {
                var title = LabUI.Text(scroll, number + ". " + chapter.Title, 19, LabUI.Green); title.fontStyle = FontStyles.Bold;
                foreach (var lesson in Curriculum.Of(number))
                {
                    var record = Curriculum.Record(lesson.ID);
                    var row = LabUI.Row(scroll, lesson.ID, 8);
                    bool done = record?.done == true, current = lesson == Curriculum.Next;
                    var text = LabUI.Text(row.transform, Curriculum.Label(lesson) + (done ? "" : current ? "  —  ახლა" : "  —  დაკეტილია"), 16, done || current ? LabUI.Ink : LabUI.Muted);
                    LabUI.Size(text.gameObject, 38, -1, 1);
                    var l = lesson;
                    if (done && !lesson.IsReview) LabUI.Size(LabUI.Button(row.transform, "ვარჯიში", () => { hub.Close(); OpenWorkbook(l, true); }, 38).gameObject, 38, 120, 0);
                    if (done || current) LabUI.Size(LabUI.Secondary(row.transform, done ? "გამეორება" : "დაწყება", () => { hub.Close(); Begin(l); }, 38).gameObject, 38, 130, 0);
                }
            }
            var footer = LabUI.Row(hub.Body, "მოდელები", 10);
            LabUI.Size(footer.gameObject, 46);
            switch (type)
            {
                case StationType.AtomTable: Model(footer.transform, "ატომების მოდელი", "atom"); break;
                case StationType.MoleculeTable: Model(footer.transform, "მოლეკულების მოდელი", "molecule"); break;
                case StationType.SolutionStation: Model(footer.transform, "ხსნარების მოდელი", "solution"); break;
                case StationType.AcidBaseStation: Model(footer.transform, "მჟავა-ფუძის მოდელი", "ph"); break;
                case StationType.ElementStation:
                    LabUI.Button(footer.transform, "პერიოდული ცხრილი [Tab]", () => { hub.Close(); game.Periodic.Open(); }, 44); break;
                case StationType.ReactionTable:
                    LabUI.Button(footer.transform, "ვიზუალური ექსპერიმენტი", () =>
                    {
                        if (!Curriculum.Done("c6_conserve")) { game.HUD.Notify(Curriculum.LockedText + " საჭიროა გაკვეთილი: „" + Curriculum.Get("c6_conserve").Title + "“."); return; }
                        hub.Close(); game.Quiz.Open(visual: true);
                    }, 44);
                    LabUI.Button(footer.transform, "შერევის მაგიდა", () => { hub.Close(); Bench.Open(); }, 44); break;
            }
            LabUI.Size(LabUI.Secondary(footer.transform, "სასწავლო რუკა [M]", () => { hub.Close(); OpenMap(); }, 44).gameObject, 44, 220, 0);
        }

        private void Model(Transform parent, string label, string id) =>
            LabUI.Button(parent, label, () =>
            {
                if (!Curriculum.ActivityOpen(id)) { game.HUD.Notify(Curriculum.LockedText + " " + Curriculum.Requirement(id)); return; }
                hub.Close(); game.Personal.Activities.Open(id);
            }, 44);
    }
}
