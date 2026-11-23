using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Lessons as a Duolingo-style deck of short, varied cards (pick, true/false, choice, mark-all,
    /// drag-to-group, hands-on builder), the learning map and the practice hubs of the classroom stations.
    /// </summary>
    public sealed class CurriculumUI : MonoBehaviour
    {
        /// <summary>The exercise kinds a lesson mixes, so consecutive cards ask for different actions.</summary>
        public enum Ex { Pick, TrueFalse, Choice, SelectAll, Drag, Build, Reward }

        /// <summary>One exercise card in a lesson's deck.</summary>
        private sealed class Card
        {
            public Ex Kind;
            /// <summary>Sort items the card uses (Pick, TrueFalse, SelectAll, Drag).</summary>
            public int[] Items = new int[0];
            /// <summary>SelectAll: the group to mark. TrueFalse on an item: the group the statement claims.</summary>
            public int Bin;
            /// <summary>Question key (Choice, or TrueFalse on a question) and, for TrueFalse, the choice the statement claims.</summary>
            public string Key;
            public int Claim;
            /// <summary>Build: index into the lesson's practices (1 = guided with live hints, 2 = free with a check).</summary>
            public int Step;
            public int[] Order;
            public Card Copy() => (Card)MemberwiseClone();
        }

        /// <summary>Drag a card onto a group; a plain tap still works through the chip's button.</summary>
        private sealed class DragChip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public CurriculumUI Owner;
            public int Item;
            public void OnBeginDrag(PointerEventData e)
            {
                GetComponent<LayoutElement>().ignoreLayout = true;
                transform.SetAsLastSibling();
                transform.localScale = Vector3.one * 1.08f;
            }
            public void OnDrag(PointerEventData e) => transform.position = e.position; // overlay canvas: screen = world
            public void OnEndDrag(PointerEventData e)
            {
                int bin = Owner.BinAt(e.position);
                if (bin >= 0) Owner.Drop(Item, bin); else Owner.Refresh();
            }
        }

        private static readonly Color Chosen = new Color32(206, 232, 246, 255), ChosenText = new Color32(40, 110, 160, 255), BinColor = new Color32(48, 78, 66, 255);
        private static readonly string[] Praises = { "ყოჩაღ!", "ზუსტად!", "შესანიშნავია!", "ნამდვილი ქიმიკოსი ხარ!", "ასე გააგრძელე!", "ბრწყინვალეა!" };
        private static readonly string[] Comforts = { "არაუშავს — ახლა უკვე იცი.", "კარგი ცდა! ეს ბოლოს კიდევ ერთხელ შეგხვდება.", "შეცდომებზე ვსწავლობთ." };

        private LabGame game;
        private LabWindow window, map, hub;
        private RectTransform stage;
        private Transform controls, play;
        private TMP_Text feedback, bubble, primaryLabel;
        private Button primary;
        private Practice practice;
        private int[] values;
        private bool practiceOnly;
        private int tries, lessonMistakes, answers, streak, xpAtStart, cardIndex, selected = -1, holding = -1;
        private float startedAt;
        private Image feedbackCard, progressFill;
        private readonly List<Card> deck = new List<Card>();
        private readonly HashSet<int> picked = new HashSet<int>();
        private readonly HashSet<string> wrong = new HashSet<string>();
        private readonly List<RectTransform> bins = new List<RectTransform>();
        private static Sprite circle;

        public Lesson Lesson { get; private set; }
        public Ex Current { get; private set; }
        public bool IsOpen => window != null;
        public bool MapOpen => map != null;
        public bool HubOpen => hub != null;
        /// <summary>The current card is answered (right or wrong) and the lesson can move on.</summary>
        public bool Solved { get; private set; }
        public bool Correct { get; private set; }
        public string Feedback => feedback != null ? feedback.text : "";
        public int Cards => deck.Count;
        private Card Now => cardIndex < deck.Count ? deck[cardIndex] : null;
        public int[] CardItems => Now?.Items ?? new int[0];
        public int CardBin => Now?.Bin ?? -1;
        public int CardClaim => Now?.Claim ?? -1;
        public LessonQuestion Question => Now?.Key != null ? Curriculum.Resolve(Now.Key).question : null;
        public List<string> Rewards { get; private set; }
        private MixBenchUI bench;
        public MixBenchUI Bench => bench != null ? bench : bench = gameObject.AddComponent<MixBenchUI>();
        private KitBenchUI tables;
        /// <summary>The atom, molecule and pH tables share one build-and-discover window.</summary>
        public KitBenchUI Tables { get { if (tables == null) { tables = gameObject.AddComponent<KitBenchUI>(); tables.Initialize(game); } return tables; } }

        public void Initialize(LabGame lab) => game = lab;

        // ================================================================== starting a lesson

        public void Begin(Lesson lesson)
        {
            if (lesson == null) return;
            if (!Curriculum.Available(lesson)) { game.HUD.Notify(Curriculum.LockedText + " " + Curriculum.Requirement(lesson)); return; }
            if (map != null) map.Close();
            if (hub != null) hub.Close();
            if (game.Dialogue.IsOpen) game.Dialogue.End();
            OpenWorkbook(lesson);
        }

        /// <summary>Opens a lesson's card deck. Station practice is a single hands-on card.</summary>
        public void OpenWorkbook(Lesson lesson, bool practiceOnlyMode = false)
        {
            if (lesson == null) return;
            if (window != null) window.Close();
            Lesson = lesson; practiceOnly = practiceOnlyMode && !lesson.IsReview;
            wrong.Clear(); Rewards = null;
            lessonMistakes = answers = streak = 0; startedAt = Time.unscaledTime; xpAtStart = GameSaveManager.Instance.Data.totalXP;
            var created = LabUI.Window(practiceOnly ? "ვარჯიში • " + lesson.Title : "თავი " + lesson.Chapter + " • " + Curriculum.Label(lesson), 1220, 710);
            created.Closed = () => { if (window == created) { window = null; OpenChest(); } };
            window = created;
            Chest = null;
            Deal();
            cardIndex = 0;
            Show();
        }

        public void Close() { if (window != null) window.Close(); }

        /// <summary>Builds the deck from the lesson's data, alternating kinds so no two cards in a row feel alike.</summary>
        private void Deal()
        {
            deck.Clear();
            var l = Lesson;
            var keys = l.IsReview ? Curriculum.ReviewKeys(l) : Enumerable.Range(0, l.Questions.Length).Select(i => Curriculum.Key(l, i)).ToArray();
            void Ask(int i, bool truth) { if (i < keys.Length) deck.Add(truth ? TruthCard(keys[i]) : new Card { Kind = Ex.Choice, Key = keys[i] }); }
            if (l.IsReview) { for (int i = 0; i < keys.Length; i++) Ask(i, i % 2 == 1); return; }
            if (l.Kind == PracticeKind.Sort)
            {
                var order = Enumerable.Range(0, l.Items.Length).OrderBy(_ => UnityEngine.Random.value).ToArray();
                if (practiceOnly) { deck.Add(new Card { Kind = Ex.Drag, Items = order.Take(6).ToArray() }); return; }
                int b = l.Items[order[1]].Bin;
                deck.Add(new Card { Kind = Ex.Pick, Items = new[] { order[0] } });
                deck.Add(new Card { Kind = Ex.TrueFalse, Items = new[] { order[1] }, Bin = UnityEngine.Random.value < 0.5f ? b : (b + UnityEngine.Random.Range(1, l.Bins.Length)) % l.Bins.Length });
                Ask(0, false);
                var marked = order.Skip(2).Take(6).ToArray();
                if (marked.Length > 0) deck.Add(new Card { Kind = Ex.SelectAll, Items = marked, Bin = l.Items[marked[0]].Bin });
                Ask(1, true);
                deck.Add(new Card { Kind = Ex.Drag, Items = order.Skip(6).Concat(order).Distinct().Take(5).ToArray() });
                Ask(2, false);
                return;
            }
            int free = Mathf.Max(0, l.Steps.Length - 1), guided = Mathf.Min(1, free);
            if (practiceOnly) { deck.Add(new Card { Kind = Ex.Build, Step = free }); return; }
            deck.Add(new Card { Kind = Ex.Build, Step = guided });
            Ask(0, true);
            Ask(1, false);
            if (free != guided) deck.Add(new Card { Kind = Ex.Build, Step = free });
            Ask(2, false);
        }

        private static Card TruthCard(string key)
        {
            var q = Curriculum.Resolve(key).question;
            return new Card { Kind = Ex.TrueFalse, Key = key, Claim = UnityEngine.Random.value < 0.5f ? 0 : UnityEngine.Random.Range(1, q.Choices.Length) };
        }

        /// <summary>Lays out the current card: progress, teacher bubble, play area and the bottom check/continue bar.</summary>
        private void Show()
        {
            Current = Now?.Kind ?? Ex.Reward;
            Solved = Correct = false; practice = null; tries = 0; selected = holding = -1;
            picked.Clear(); bins.Clear(); play = null; bubble = null;
            foreach (Transform child in window.Body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            if (Current == Ex.Reward) { BuildReward(); return; }
            if (Current == Ex.Choice && Now.Order == null) Now.Order = Enumerable.Range(0, Question.Choices.Length).OrderBy(_ => UnityEngine.Random.value).ToArray();
            Header();
            if (Current == Ex.Build) BuildCard();
            else
            {
                Teacher(Instruction());
                var column = LabUI.Column(window.Body, "ვარჯიში", 12);
                column.childAlignment = TextAnchor.UpperCenter;
                LabUI.FlexibleHeight(column.gameObject);
                play = column.transform;
            }
            Banner();
            Say(Current == Ex.Build ? (Guided ? "შეცვალე რიცხვები — მინიშნებას მაშინვე დაინახავ." : "როცა მზად იქნები, დააჭირე „შემოწმებას“.")
                : Current == Ex.Drag ? "ან შეეხე ბარათს, მერე — ჯგუფს." : "", LabUI.Ink);
            Refresh();
            StartCoroutine(Pop(play));
        }

        private float Progress => deck.Count == 0 ? 1 : Mathf.Clamp01((cardIndex + (Solved ? 1f : 0f)) / deck.Count);

        private void Header()
        {
            var row = LabUI.Row(window.Body, "გაკვეთილის პროგრესი", 14);
            LabUI.Size(row.gameObject, 30);
            progressFill = LabUI.Progress(row.transform, Progress, LabUI.Mint, 22);
            LabUI.Size(progressFill.transform.parent.gameObject, 22, -1, 1);
            if (streak >= 2) LabUI.Pill(row.transform, streak + " ზედიზედ!", new Color32(226, 150, 40, 255), 180).fontSize = 20;
        }

        /// <summary>The teacher's portrait with a speech bubble, like Duolingo's characters.</summary>
        private void Teacher(string text)
        {
            var row = LabUI.Row(window.Body, "მასწავლებელი", 14);
            LabUI.Size(row.gameObject, 104);
            var frame = LabUI.Panel(row.transform, "პორტრეტი", LabUI.Green);
            LabUI.Size(frame.gameObject, 100, 100, 0);
            var art = LabUI.Object(frame.transform, "სახე", typeof(Image)).GetComponent<Image>();
            var theme = LabUI.Theme;
            art.sprite = theme != null && theme.teacherPortrait != null ? theme.teacherPortrait : LabIcons.Get(LabIcon.Atom);
            if (theme != null && theme.teacherPortrait != null && theme.teacherPortraitMaterial != null) art.material = theme.teacherPortraitMaterial;
            art.preserveAspect = true; art.raycastTarget = false;
            LabUI.Fill(art.rectTransform, 6);
            var cloud = LabUI.Panel(row.transform, "ბუშტი", Color.white);
            LabUI.Size(cloud.gameObject, 100, -1, 1);
            bubble = LabUI.Text(cloud.transform, text, 24, LabUI.Ink);
            LabUI.Fill(bubble.rectTransform, 14);
            bubble.alignment = TextAlignmentOptions.MidlineLeft;
            bubble.enableAutoSizing = true; bubble.fontSizeMin = 17; bubble.fontSizeMax = 24;
        }

        private string Instruction()
        {
            // The lesson's key facts arrive one per card at the start, next to the task they help with.
            string lead = !Lesson.IsReview && !practiceOnly && cardIndex < Lesson.Teach.Length ? Lesson.Teach[cardIndex] + "\n" : "";
            string ask = Current switch
            {
                Ex.Pick => "რომელ ჯგუფს ეკუთვნის?",
                Ex.TrueFalse => "მართალია თუ მცდარი?",
                Ex.Choice => "აირჩიე სწორი პასუხი.",
                Ex.SelectAll => "მონიშნე ყველაფერი ჯგუფიდან „" + Lesson.Bins[Now.Bin] + "“.",
                Ex.Drag => "გადაათრიე თითოეული ბარათი თავის ჯგუფში.",
                _ => "",
            };
            return lead + "<b>" + ask + "</b>";
        }

        /// <summary>The bottom bar: feedback on the left, one big check/continue button on the right.</summary>
        private void Banner()
        {
            feedbackCard = LabUI.Card(window.Body, "შედეგი", LabUI.Soft, 12);
            LabUI.Size(feedbackCard.gameObject, 104);
            var row = LabUI.Row(feedbackCard.transform, "ბანერი", 16);
            LabUI.FlexibleHeight(row.gameObject);
            feedback = LabUI.Text(row.transform, "", 22, LabUI.Ink);
            feedback.enableAutoSizing = true; feedback.fontSizeMin = 15; feedback.fontSizeMax = 22;
            LabUI.Size(feedback.gameObject, 78, -1, 1);
            primary = LabUI.Button(row.transform, "შემოწმება", Primary, 72);
            LabUI.Size(primary.gameObject, 72, 300, 0);
            primaryLabel = Big(primary, 26);
        }

        private static TMP_Text Big(Button button, float size)
        {
            var label = button.GetComponentInChildren<TMP_Text>();
            label.fontSizeMax = size; label.fontSizeMin = Mathf.Round(size * 0.65f);
            return label;
        }

        private void Primary() { if (Solved) Advance(); else Check(); }

        /// <summary>Redraws the play area after any change (selection, drop, counter, answer).</summary>
        private void Refresh()
        {
            if (window == null || play == null) return;
            if (Current == Ex.Build) { BuildCounters(); Draw(); }
            else
            {
                foreach (Transform child in play) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
                bins.Clear();
                switch (Current)
                {
                    case Ex.Pick: ShowPick(); break;
                    case Ex.TrueFalse: ShowTruth(); break;
                    case Ex.Choice: ShowChoice(); break;
                    case Ex.SelectAll: ShowMarks(); break;
                    case Ex.Drag: ShowDrag(); break;
                }
            }
            if (primary == null) return;
            primary.interactable = Solved || Current == Ex.Choice && selected >= 0 || Current == Ex.SelectAll && picked.Count > 0 || Current == Ex.Build && !Guided;
            primaryLabel.text = Solved ? "გაგრძელება →" : "შემოწმება";
            primary.GetComponent<Image>().color = Solved && !Correct ? LabUI.Coral : LabUI.Green;
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

        /// <summary>Settles the current card. Wrong pick/choice cards come back at the end of the deck, as in Duolingo.</summary>
        private void Settle(bool right, string text)
        {
            Solved = true; Correct = right;
            if (right) { answers++; streak++; AudioManager.Instance?.Correct(); }
            else
            {
                lessonMistakes++; streak = 0; AudioManager.Instance?.Wrong();
                if (Now.Key != null) wrong.Add(Now.Key);
                if (Current != Ex.Build && Current != Ex.Drag) deck.Add(Now.Copy());
            }
            Say(text, right ? LabUI.Green : LabUI.Coral);
            if (right) { Juice.Sparkle(feedbackCard.transform, 14); if (streak >= 3) Juice.Confetti(feedbackCard.transform, 2, 220); }
            else if (play != null) play.gameObject.AddComponent<Wobble>();
            if (bubble != null)
                bubble.text = right ? (streak >= 3 ? "<b>" + streak + " ზედიზედ!</b> " : "") + Praises[UnityEngine.Random.Range(0, Praises.Length)]
                    : Comforts[UnityEngine.Random.Range(0, Comforts.Length)];
            Refresh();
            if (progressFill != null) StartCoroutine(Grow(progressFill, Progress));
        }

        // Enter presses the big bottom button, like Duolingo's.
        private void Update()
        {
            if (window != null && primary != null && primary.interactable && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) primary.onClick.Invoke();
        }

        /// <summary>Moves on once the current card is answered; after the last card comes the reward.</summary>
        public bool Advance()
        {
            if (window == null) return false;
            if (Current == Ex.Reward)
            {
                var upcoming = Curriculum.Next;
                window.Close();
                // The next activity starts here only if it lives at the same station; otherwise its station lights up.
                if (upcoming == null) return true;
                if (Curriculum.ChapterOf(upcoming).Station == Curriculum.ChapterOf(Lesson).Station) Begin(upcoming);
                else game.HUD.Notify("ახალი ცდა გაიხსნა: " + Curriculum.ChapterOf(upcoming).StationName + "!");
                return true;
            }
            if (!Solved) return false;
            cardIndex++;
            if (cardIndex >= deck.Count && practiceOnly)
            {
                GameSaveManager.Instance.AddXP(Curriculum.RepeatXP);
                game.HUD.Notify("ყოჩაღ! ვარჯიში დასრულდა. +" + Curriculum.RepeatXP + " XP");
                window.Close();
                return true;
            }
            Show();
            return true;
        }

        /// <summary>Choice and mark-all cards commit their selection; free builds are checked.</summary>
        public void Check()
        {
            if (window == null || Solved) return;
            switch (Current)
            {
                case Ex.Choice: if (selected >= 0) Answer(selected); break;
                case Ex.SelectAll: if (picked.Count > 0) CommitMarks(); break;
                case Ex.Build:
                    if (Guided || practice == null) return;
                    string problem = Problem();
                    if (problem == null) Settle(true, Praise() + Built());
                    else { tries++; lessonMistakes++; streak = 0; AudioManager.Instance?.Wrong(); Say("ჯერ არა. " + problem + "\nშეცვალე და კიდევ სცადე.", LabUI.Coral); }
                    break;
            }
        }

        // ------------------------------------------------------------ pick one group

        private void ShowPick()
        {
            var item = Lesson.Items[Now.Items[0]];
            BigCard(item.Label, 130, 46);
            var row = LabUI.Row(play, "ჯგუფები", 16);
            LabUI.Size(row.gameObject, 86);
            for (int b = 0; b < Lesson.Bins.Length; b++)
            {
                int bin = b;
                var button = LabUI.Button(row.transform, Lesson.Bins[b], () => Pick(bin), 86);
                Big(button, 28);
                if (Solved) button.GetComponent<Image>().color = b == item.Bin ? LabUI.Green : b == selected ? LabUI.Coral : LabUI.Muted;
            }
        }

        public void Pick(int bin)
        {
            if (window == null || Current != Ex.Pick || Solved || bin < 0 || bin >= Lesson.Bins.Length) return;
            var item = Lesson.Items[Now.Items[0]];
            selected = bin;
            Settle(bin == item.Bin, (bin == item.Bin ? "" : "„" + item.Label + "“ — " + Lesson.Bins[item.Bin] + ". ") + item.Why);
        }

        /// <summary>A big white card with one centred line, the thing the player is judging.</summary>
        private void BigCard(string text, float height, float size)
        {
            var card = LabUI.Panel(play, "ბარათი", Color.white);
            LabUI.Size(card.gameObject, height);
            var label = LabUI.Text(card.transform, text, size, LabUI.Ink);
            label.alignment = TextAlignmentOptions.Center; label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = true; label.fontSizeMin = 20; label.fontSizeMax = size;
            LabUI.Fill(label.rectTransform, 14);
        }

        // ------------------------------------------------------------ true or false

        private bool StatementTrue => Now.Key == null ? Lesson.Items[Now.Items[0]].Bin == Now.Bin : Now.Claim == 0;
        private string Statement()
        {
            if (Now.Key == null) return "„" + Lesson.Items[Now.Items[0]].Label + "“ — " + Lesson.Bins[Now.Bin];
            var q = Question;
            return "<size=80%>" + q.Prompt + "</size>\n" + q.Choices[Now.Claim];
        }

        private void ShowTruth()
        {
            BigCard(Statement(), 160, 34);
            var row = LabUI.Row(play, "მართალია თუ მცდარი", 16);
            LabUI.Size(row.gameObject, 86);
            foreach (bool claim in new[] { true, false })
            {
                bool c = claim;
                var button = LabUI.Button(row.transform, claim ? "მართალია" : "მცდარია", () => Truth(c), 86);
                Big(button, 30);
                var image = button.GetComponent<Image>();
                image.color = claim ? LabUI.Green : LabUI.Coral;
                if (Solved && (selected == 1) != claim) image.color = LabUI.Muted;
            }
        }

        public void Truth(bool claim)
        {
            if (window == null || Current != Ex.TrueFalse || Solved) return;
            selected = claim ? 1 : 0;
            string why;
            if (Now.Key == null)
            {
                var item = Lesson.Items[Now.Items[0]];
                why = (StatementTrue ? "მართალია. " : "მცდარია: „" + item.Label + "“ — " + Lesson.Bins[item.Bin] + ". ") + item.Why;
            }
            else
            {
                var q = Question;
                why = Now.Claim == 0 ? "მართალია. " + q.Why[0] : "მცდარია. " + q.Why[Now.Claim] + "\nსწორი პასუხი: „" + q.Choices[0] + "“.";
            }
            Settle(claim == StatementTrue, why);
        }

        // ------------------------------------------------------------ multiple choice

        private void ShowChoice()
        {
            var q = Question;
            var prompt = LabUI.Text(play, q.Prompt, 28, LabUI.Ink);
            prompt.fontStyle = FontStyles.Bold; prompt.alignment = TextAlignmentOptions.Center;
            prompt.enableAutoSizing = true; prompt.fontSizeMin = 19; prompt.fontSizeMax = 28;
            LabUI.Size(prompt.gameObject, 66);
            foreach (int index in Now.Order)
            {
                int i = index;
                var button = LabUI.Secondary(play, q.Choices[i], () => Select(i), 60);
                var label = Big(button, 24);
                var image = button.GetComponent<Image>();
                if (Solved && i == 0) { image.color = LabUI.Green; label.color = Color.white; }
                else if (Solved && i == selected) { image.color = LabUI.Coral; label.color = Color.white; }
                else if (i == selected) { image.color = Chosen; label.color = ChosenText; }
            }
        }

        /// <summary>Highlights a choice; the bottom button then checks it.</summary>
        public void Select(int choice)
        {
            if (window == null || Current != Ex.Choice || Solved || Question == null || choice < 0 || choice >= Question.Choices.Length) return;
            selected = choice;
            Refresh();
        }

        /// <summary>Answers the current choice card; <paramref name="choice"/> indexes the original choices (0 is right).</summary>
        public void Answer(int choice)
        {
            var q = Question;
            if (window == null || Current != Ex.Choice || Solved || q == null || choice < 0 || choice >= q.Choices.Length) return;
            selected = choice;
            Settle(choice == 0, choice == 0 ? q.Why[0] : "სწორი პასუხი: „" + q.Choices[0] + "“. " + q.Why[choice]);
        }

        // ------------------------------------------------------------ mark all of one group

        private void ShowMarks()
        {
            var grid = LabUI.Object(play, "ბარათები", typeof(GridLayoutGroup)).GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(370, 78); grid.spacing = new Vector2(14, 14);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;
            foreach (int index in Now.Items)
            {
                int i = index;
                bool belongs = Lesson.Items[i].Bin == Now.Bin, on = picked.Contains(i);
                var button = LabUI.Secondary(grid.transform, Lesson.Items[i].Label, () => Toggle(i), 78);
                var label = Big(button, 26);
                var image = button.GetComponent<Image>();
                if (Solved && (belongs || on)) { image.color = belongs && on ? LabUI.Green : belongs ? LabUI.Gold : LabUI.Coral; label.color = Color.white; }
                else if (!Solved && on) { image.color = Chosen; label.color = ChosenText; }
            }
        }

        public void Toggle(int item)
        {
            if (window == null || Current != Ex.SelectAll || Solved || !Now.Items.Contains(item)) return;
            if (!picked.Remove(item)) picked.Add(item);
            Refresh();
        }

        private void CommitMarks()
        {
            var lines = Now.Items.Where(i => picked.Contains(i) != (Lesson.Items[i].Bin == Now.Bin)).Select(i =>
                (picked.Contains(i) ? "„" + Lesson.Items[i].Label + "“ — " + Lesson.Bins[Lesson.Items[i].Bin] + ". " : "გამოგრჩა: „" + Lesson.Items[i].Label + "“. ") + Lesson.Items[i].Why).ToList();
            Settle(lines.Count == 0, lines.Count == 0 ? "ყველა იპოვე! " + Lesson.Items[Now.Items.First(i => Lesson.Items[i].Bin == Now.Bin)].Why : string.Join("\n", lines.Take(2)));
        }

        // ------------------------------------------------------------ drag into groups

        private void ShowDrag()
        {
            var row = LabUI.Row(play, "ჯგუფები", 14);
            LabUI.Size(row.gameObject, 176);
            row.childForceExpandWidth = true; row.childForceExpandHeight = true;
            for (int b = 0; b < Lesson.Bins.Length; b++)
            {
                int bin = b;
                var placed = Now.Items.Where(i => picked.Contains(i) && Lesson.Items[i].Bin == b).Select(i => Lesson.Items[i].Label);
                var box = LabUI.Button(row.transform, "", () => Drop(holding, bin), 176);
                box.GetComponent<Image>().color = holding >= 0 ? new Color32(62, 112, 92, 255) : BinColor;
                var label = Big(box, 24);
                label.text = "<b><color=#EFC266>" + Lesson.Bins[b] + "</color></b>\n" + string.Join("  •  ", placed);
                label.alignment = TextAlignmentOptions.Top;
                bins.Add((RectTransform)box.transform);
            }
            var chips = LabUI.Row(play, "ბარათები", 12);
            LabUI.Size(chips.gameObject, 78);
            chips.childAlignment = TextAnchor.MiddleCenter;
            foreach (int index in Now.Items.Where(i => !picked.Contains(i)))
            {
                int i = index;
                var chip = LabUI.Secondary(chips.transform, Lesson.Items[i].Label, () => Hold(i), 74);
                LabUI.Size(chip.gameObject, 74, 210, 0);
                var label = Big(chip, 24);
                if (i == holding) { chip.GetComponent<Image>().color = Chosen; label.color = ChosenText; }
                var drag = chip.gameObject.AddComponent<DragChip>();
                drag.Owner = this; drag.Item = i;
            }
        }

        private void Hold(int item) { if (Current != Ex.Drag || Solved) return; holding = holding == item ? -1 : item; Refresh(); }
        private int BinAt(Vector2 screen)
        {
            for (int b = 0; b < bins.Count; b++) if (bins[b] != null && RectTransformUtility.RectangleContainsScreenPoint(bins[b], screen, null)) return b;
            return -1;
        }

        /// <summary>Places a card into a group; a wrong group bounces it back with the reason.</summary>
        public void Drop(int item, int bin)
        {
            if (window == null || Current != Ex.Drag || Solved || !Now.Items.Contains(item) || picked.Contains(item) || bin < 0 || bin >= Lesson.Bins.Length) { Refresh(); return; }
            holding = -1;
            var entry = Lesson.Items[item];
            if (bin != entry.Bin)
            {
                lessonMistakes++; streak = 0; AudioManager.Instance?.Wrong();
                Say("„" + entry.Label + "“ — არა „" + Lesson.Bins[bin] + "“. " + entry.Why, LabUI.Coral);
                Refresh();
                return;
            }
            picked.Add(item);
            if (bin < bins.Count) Juice.Sparkle(bins[bin], 12);
            if (Now.Items.All(picked.Contains)) { Settle(true, "ყველა ბარათი თავის ადგილზეა! " + entry.Why); return; }
            answers++; AudioManager.Instance?.Click();
            Say("სწორია! " + entry.Why, LabUI.Green);
            Refresh();
        }

        // ------------------------------------------------------------ hands-on builder

        private bool Guided => Current == Ex.Build && Now != null && Now.Step == 1 && !practiceOnly;

        private void BuildCard()
        {
            practice = Lesson.Steps[Now.Step];
            values = (practice.Start ?? Min()).ToArray();
            var split = LabUI.Row(window.Body, "ვარჯიში", 18);
            LabUI.FlexibleHeight(split.gameObject);
            split.childForceExpandHeight = true;
            play = split.transform;
            var panel = LabUI.Panel(split.transform, "სცენა", LabUI.Navy);
            LabUI.Size(panel.gameObject, 300, 500, 1);
            LabUI.FlexibleHeight(panel.gameObject);
            panel.gameObject.AddComponent<RectMask2D>();
            stage = panel.rectTransform;
            var column = LabUI.Column(split.transform, "მართვა", 10);
            LabUI.Size(column.gameObject, 300, 520, 0);
            controls = column.transform;
        }

        /// <summary>What the player built, in the teacher's words (open goals accept several answers).</summary>
        private string Built() => practice.Named?.Invoke(values) ?? practice.Done;

        /// <summary>The teacher reacts to how the step was solved, not only that it was.</summary>
        private string Praise()
        {
            if (practice != null && values != null && !values.SequenceEqual(practice.Solution)) return "ესეც სწორი პასუხია! ";
            if (!Guided && tries == 0) return "პირველივე ცდაზე! ";
            if (tries >= 2) return "არ დანებდი და მიაგენი — ასე მუშაობენ ნამდვილი ქიმიკოსები. ";
            if (tries == 1) return "ერთი შეცდომა და მაშინვე გაასწორე — ყოჩაღ! ";
            return "სწორია! ";
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
            var goal = LabUI.Text(controls, practice.Goal, 24, LabUI.Ink);
            goal.fontStyle = FontStyles.Bold;
            goal.enableAutoSizing = true; goal.fontSizeMin = 17; goal.fontSizeMax = 24;
            LabUI.Size(goal.gameObject, 110);
            var min = Min(); var max = Max();
            float height = Mathf.Clamp(270f / Mathf.Max(1, values.Length), 48, 68);
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                var row = LabUI.Row(controls, practice.Counters[i], 10);
                LabUI.Size(row.gameObject, height);
                bool fixedValue = min[i] == max[i];
                var caption = LabUI.Text(row.transform, CounterName(i) + ": <b>" + values[i] + "</b>" + (fixedValue ? "  (ფიქსირებული)" : ""), 23, LabUI.Ink);
                caption.enableAutoSizing = true; caption.fontSizeMin = 16; caption.fontSizeMax = 23;
                LabUI.Size(caption.gameObject, height, -1, 1);
                var minus = LabUI.Secondary(row.transform, "−", () => Adjust(index, -1), height);
                var plus = LabUI.Secondary(row.transform, "+", () => Adjust(index, 1), height);
                LabUI.Size(minus.gameObject, height, 76, 0); LabUI.Size(plus.gameObject, height, 76, 0);
                Big(minus, 34); Big(plus, 34);
                minus.interactable = plus.interactable = !fixedValue && !Solved;
            }
        }

        /// <summary>Changes a counter by <paramref name="delta"/> within its limits; guided cards hint after every change.</summary>
        public void Adjust(int counter, int delta)
        {
            if (window == null || practice == null || Current != Ex.Build || Solved || counter < 0 || counter >= values.Length) return;
            values[counter] = Mathf.Clamp(values[counter] + delta, Min()[counter], Max()[counter]);
            Refresh();
            if (!Guided) { Say("როცა მზად იქნები, დააჭირე „შემოწმებას“.", LabUI.Ink); return; }
            string problem = Problem();
            if (problem == null) Settle(true, Praise() + Built());
            else Say("მინიშნება: " + problem, LabUI.Coral);
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
            var tint = ChemistrySandbox.Indicator((float)ph);
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
            Shape("ნიმუში", new Vector2(0, 30), new Vector2(120, 90), ChemistrySandbox.Indicator(ph));
            DrawStrip(-70, ph);
        }

        private void DrawStrip(float y, float ph)
        {
            for (int i = 0; i <= 14; i++) Shape("pH ფერი", new Vector2(-196 + i * 28, y), new Vector2(26, 26), ChemistrySandbox.Indicator(i));
            Shape("მაჩვენებელი", new Vector2(-196 + Mathf.Clamp(ph, 0, 14) * 28, y + 24), new Vector2(6, 18), Color.white);
            Label("0   მჟავა          7          ფუძე   14", new Vector2(0, y - 32), 15);
        }

        // ================================================================== reward

        private string[] AskedKeys => deck.Where(c => c.Key != null).Select(c => c.Key).Distinct().ToArray();

        /// <summary>The lesson chest: usually a few coins, sometimes a bench ingredient, rarely two precious jars.</summary>
        public sealed class Loot { public string Material = null, Name = ""; public int Count, Coins; public bool Rare; public Color Color = Color.white; }
        private static readonly string[] Common = { "vinegar", "soda", "salt", "sugar", "chalk", "yeast", "nail" };
        private static readonly string[] Precious = { "cuso4", "peroxide", "hcl", "naoh", "zinc", "copper" };
        public const float RareChance = 0.08f, IngredientChance = 0.25f;
        public Loot Chest { get; private set; }
        public bool ChestOpen { get; private set; }
        private Image chestArt;
        private TMP_Text chestLabel;
        private RectTransform chestCard;

        public static Loot Roll(float roll)
        {
            if (roll >= RareChance + IngredientChance) return new Loot { Coins = UnityEngine.Random.Range(3, 9) };
            bool rare = roll < RareChance;
            var pool = rare ? Precious : Common;
            var reagent = Mixing.Get(pool[UnityEngine.Random.Range(0, pool.Length)]);
            return new Loot { Material = reagent.ID, Name = reagent.Name, Color = reagent.Color, Rare = rare, Count = rare ? 2 : UnityEngine.Random.Range(1, 3), Coins = rare ? 10 : 0 };
        }

        /// <summary>Opens the chest and pays what is inside. Closing the window first pays it quietly.</summary>
        public void OpenChest()
        {
            if (Chest == null || ChestOpen) return;
            ChestOpen = true;
            var save = GameSaveManager.Instance;
            save.Data.chemCoins += Chest.Coins;
            if (Chest.Material != null) LabProgress.Give(Chest.Material, Chest.Count);
            save.Commit();
            if (chestArt == null) return;
            chestArt.sprite = Juice.Chest(true);
            AudioManager.Instance?.Coin();
            if (Chest.Rare) AudioManager.Instance?.Milestone();
            Juice.Confetti(chestCard, Chest.Rare ? 8 : 3, Chest.Rare ? 420 : 160);
            // The prize floats out of the chest: a jar for ingredients, a coin otherwise.
            var prize = LabUI.Object(chestCard, "მონაპოვარი", typeof(RectTransform));
            Place((RectTransform)prize.transform, new Vector2(0, 0.5f), new Vector2(230, 0), new Vector2(84, 96));
            if (Chest.Material != null)
            {
                foreach (bool liquid in new[] { true, false })
                {
                    var part = LabUI.Object(prize.transform, "ქილა", typeof(Image)).GetComponent<Image>();
                    part.sprite = MixBenchUI.Glass(liquid); part.color = liquid ? Chest.Color : Color.white; part.raycastTarget = false;
                    LabUI.Fill(part.rectTransform);
                }
            }
            else LabUI.Fill(LabUI.Icon(prize.transform, LabIcon.Coin, Color.white).rectTransform, 8);
            StartCoroutine(Juice.PopIn(prize.transform, 0.05f));
            chestLabel.rectTransform.offsetMin = new Vector2(300, 12);
            chestLabel.text = Chest.Material == null ? "<size=120%><b>+" + Chest.Coins + " ქიმქოინი</b></size>\nბონუსი ყულაბაში!"
                : (Chest.Rare ? "<color=#E29628><b>იშვიათი მონაპოვარი!</b></color>\n" : "<b>ახალი ინგრედიენტი!</b>\n") +
                  Chest.Name + " ×" + Chest.Count + (Chest.Coins > 0 ? "  +  " + Chest.Coins + " ქიმქოინი" : "") + "\n<size=75%>შერევის მაგიდაზე გელოდება</size>";
            StartCoroutine(Pop(chestCard));
        }

        private void BuildReward()
        {
            var keys = AskedKeys;
            var save = GameSaveManager.Instance;
            int coinsBefore = save.Data.chemCoins;
            Rewards = Curriculum.Finish(Lesson, keys, wrong, lessonMistakes == 0);
            int coins = save.Data.chemCoins - coinsBefore, xp = save.Data.totalXP - xpAtStart;
            Chest = Roll(UnityEngine.Random.value); ChestOpen = false;
            AudioManager.Instance?.Milestone();
            int seconds = Mathf.RoundToInt(Time.unscaledTime - startedAt), score = keys.Count(k => !wrong.Contains(k));
            int accuracy = answers * 100 / Mathf.Max(1, answers + lessonMistakes);
            string head = Lesson.IsReview ? "თავი „" + Curriculum.ChapterOf(Lesson).Title + "“ დასრულებულია!" : lessonMistakes == 0 ? "უშეცდომო გაკვეთილი!" : "გაკვეთილი დასრულებულია!";
            Teacher("<size=125%><b><color=#E29628>" + head + "</color></b></size>\n" +
                (Lesson.IsReview ? "პირველივე ცდით სწორი: " + score + " / " + keys.Length : Praises[UnityEngine.Random.Range(0, Praises.Length)] + " შენ თავად გააკეთე!"));

            var tiles = LabUI.Row(window.Body, "შედეგები", 18);
            LabUI.Size(tiles.gameObject, 150);
            Prize(tiles.transform, LabIcon.Flame, "XP", "+", xp, "", new Color32(226, 150, 40, 255), 0.15f);
            Prize(tiles.transform, LabIcon.Coin, "ქიმქოინი", "+", coins, "", new Color32(214, 166, 60, 255), 0.45f);
            Prize(tiles.transform, LabIcon.Check, lessonMistakes == 0 ? "უშეცდომოდ!" : "სიზუსტე", "", accuracy, "%", LabUI.Green, 0.75f);

            // The mystery chest: tap to open; it rocks until opened.
            var chest = LabUI.Button(window.Body, "", OpenChest, 120);
            chest.GetComponent<Image>().color = new Color32(255, 236, 190, 255);
            chestCard = (RectTransform)chest.transform;
            chestArt = LabUI.Object(chest.transform, "ზარდახშა", typeof(Image)).GetComponent<Image>();
            chestArt.sprite = Juice.Chest(false); chestArt.preserveAspect = true; chestArt.raycastTarget = false;
            Place(chestArt.rectTransform, new Vector2(0, 0.5f), new Vector2(110, 4), new Vector2(120, 104));
            chestLabel = chest.GetComponentInChildren<TMP_Text>();
            chestLabel.text = "<b>საჩუქრის ზარდახშა</b>\n<size=80%>შეეხე და გახსენი!</size>";
            chestLabel.color = new Color32(122, 68, 36, 255); chestLabel.alignment = TextAlignmentOptions.Left;
            chestLabel.fontSizeMax = 28; chestLabel.fontSizeMin = 16;
            chestLabel.rectTransform.offsetMin = new Vector2(200, 12);
            StartCoroutine(Juice.PopIn(chest.transform, 1.1f));
            StartCoroutine(Juice.Jiggle(chestArt.transform, () => ChestOpen));

            // Anything else (notebook page, elements, badges...) as short pills; XP and coins are on the tiles.
            var extras = LabUI.Row(window.Body, "სხვა ჯილდოები", 10);
            LabUI.Size(extras.gameObject, 40);
            foreach (string reward in Rewards.Skip(2).Prepend("დრო " + seconds / 60 + ":" + (seconds % 60).ToString("00")))
            {
                var pill = LabUI.Pill(extras.transform, reward, LabUI.Navy);
                LabUI.Size(pill.transform.parent.gameObject, 40, -1, 1);
                pill.enableAutoSizing = true; pill.fontSizeMin = 12; pill.fontSizeMax = 18;
            }

            var footer = LabUI.Row(window.Body, "ქმედებები", 12);
            LabUI.Size(footer.gameObject, 64);
            Big(LabUI.Secondary(footer.transform, "სასწავლო რუკა", () => { window.Close(); OpenMap(); }, 64), 22);
            Big(LabUI.Secondary(footer.transform, "დახურვა", () => window.Close(), 64), 22);
            var upcoming = Curriculum.Next;
            primary = LabUI.Button(footer.transform, upcoming != null ? "შემდეგი: " + upcoming.Title + " →" : "რთული გამოწვევები გაიხსნა! →", () =>
            {
                if (upcoming != null) Advance();
                else { window.Close(); game.HUD.Notify("რთული გამოწვევების მაგიდა კლასის ქვედა მარცხენა კუთხეშია."); }
            }, 64);
            LabUI.Size(primary.gameObject, 64, 520, 0);
            primaryLabel = Big(primary, 24);
            Juice.Confetti(window.Card, 6);
        }

        /// <summary>A big reward tile: an icon and a number that pops in and counts up.</summary>
        private void Prize(Transform parent, LabIcon icon, string title, string prefix, int value, string suffix, Color color, float delay)
        {
            var tile = LabUI.Panel(parent, title, color);
            LabUI.Size(tile.gameObject, 150, -1, 1);
            var head = LabUI.Text(tile.transform, title, 22, Color.white);
            head.alignment = TextAlignmentOptions.Center; head.fontStyle = FontStyles.Bold;
            head.rectTransform.anchorMin = new Vector2(0, 1); head.rectTransform.anchorMax = Vector2.one; head.rectTransform.pivot = new Vector2(0.5f, 1);
            head.rectTransform.sizeDelta = new Vector2(0, 36); head.rectTransform.anchoredPosition = new Vector2(0, -2);
            var body = LabUI.Panel(tile.transform, "მნიშვნელობა", Color.white);
            body.rectTransform.anchorMin = Vector2.zero; body.rectTransform.anchorMax = Vector2.one;
            body.rectTransform.offsetMin = new Vector2(6, 8); body.rectTransform.offsetMax = new Vector2(-6, -40);
            var art = LabUI.Icon(body.transform, icon, Color.white);
            Place(art.rectTransform, new Vector2(0, 0.5f), new Vector2(54, 0), new Vector2(72, 72));
            var number = LabUI.Text(body.transform, prefix + value + suffix, 48, color);
            number.alignment = TextAlignmentOptions.Center; number.fontStyle = FontStyles.Bold;
            LabUI.Fill(number.rectTransform);
            number.rectTransform.offsetMin = new Vector2(96, 0);
            StartCoroutine(Juice.PopIn(tile.transform, delay));
            StartCoroutine(Juice.CountUp(number, prefix, value, suffix, delay + 0.2f));
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
            Fit(LabUI.Text(now.transform, next != null ? next.Why : "რთული გამოწვევები, კვირის რეიტინგი და სამეცნიერო ბაზრობა შენ გელოდება.", 18, LabUI.Paper), 80);
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
                var unitText = LabUI.Text(unit.transform, "<b>თავი " + c + " • " + chapter.Title + "</b>\n<size=14>" + chapter.StationName + (Curriculum.ChapterDone(c) ? "  •  დასრულებულია" : "") + "</size>", 21, Color.white);
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
            title.fontSize = 17;
            title.alignment = TextAlignmentOptions.Center;
            if (current) title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true; title.fontSizeMin = 13; title.fontSizeMax = 17;
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

        /// <summary>
        /// A classroom table, styled like the mixing bench: big toy tiles on top (the hands-on model),
        /// then the table's optional lessons as round-cornered tiles, one coloured block per chapter.
        /// </summary>
        public void OpenStation(StationType type, string name)
        {
            if (Curriculum.StationLesson(type) == null) return;
            if (hub != null) hub.Close();
            var created = LabUI.Window(name, 1180, 680);
            created.Closed = () => { if (hub == created) hub = null; };
            hub = created;
            var chapters = Curriculum.Chapters.Select((c, i) => (c, i: i + 1)).Where(p => p.c.Station == type).ToList();
            Color tint = ChapterColors[chapters[0].i - 1];

            var toys = LabUI.Row(hub.Body, "სათამაშოები", 12);
            LabUI.Size(toys.gameObject, 112);
            switch (type)
            {
                case StationType.AtomTable: Toy(toys.transform, Kits.Atoms.Title, "ააწყვე ატომები — ზედმეტი ნეიტრონი აფეთქდება!", LabIcon.Atom, tint, () => { hub.Close(); Tables.Open(Kits.Atoms); }); break;
                case StationType.MoleculeTable: Toy(toys.transform, Kits.Molecules.Title, "შეაერთე ატომები ნამდვილ მოლეკულებად", LabIcon.Flask, tint, () => { hub.Close(); Tables.Open(Kits.Molecules); }); break;
                case StationType.SolutionStation: Toy(toys.transform, Kits.Solutions.Title, "გახსენი, გაათბე, გამოზარდე კრისტალები", LabIcon.Flask, tint, () => { hub.Close(); Tables.Open(Kits.Solutions); }); break;
                case StationType.AcidBaseStation: Toy(toys.transform, Kits.Potions.Title, "ექვსი ფერის ნაყენი წვეთებით", LabIcon.Flask, tint, () => { hub.Close(); Tables.Open(Kits.Potions); }); break;
                case StationType.ElementStation: Toy(toys.transform, "პერიოდული ცხრილი", "იპოვე ყველა ელემენტი", LabIcon.Atom, tint, () => { hub.Close(); game.Periodic.Open(); }); break;
            }
            Toy(toys.transform, "სასწავლო გზა", "ყველა გაკვეთილი ერთ რუკაზე", LabIcon.Book, LabUI.Navy, () => { hub.Close(); OpenMap(); });

            Ui.Line(hub.Body, "გაკვეთილები არჩევითია: შეეხე, თუ გაინტერესებს, რატომ ხდება ასე.", 17, LabUI.Muted, 26);
            var scroll = LabUI.Scroll(hub.Body);
            foreach (var (chapter, number) in chapters)
            {
                Color color = ChapterColors[number - 1];
                var banner = LabUI.Panel(scroll, chapter.Title, color);
                LabUI.Size(banner.gameObject, 44);
                var title = LabUI.Text(banner.transform, "<b>თავი " + number + " • " + chapter.Title + "</b>" + (Curriculum.ChapterDone(number) ? "  •  დასრულებულია" : ""), 19, Color.white);
                LabUI.Fill(title.rectTransform, 10);
                var grid = LabUI.Object(scroll, "გაკვეთილები", typeof(GridLayoutGroup)).GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(270, 96); grid.spacing = new Vector2(10, 10);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4;
                foreach (var lesson in Curriculum.Of(number)) LessonTile(grid.transform, lesson, color);
            }
        }

        /// <summary>A big coloured tile with an icon, a title and one short line, like the bench's buttons.</summary>
        private static void Toy(Transform parent, string title, string line, LabIcon icon, Color color, Action pressed)
        {
            var tile = LabUI.Button(parent, "", pressed, 112);
            tile.GetComponent<Image>().color = color;
            LabUI.Size(tile.gameObject, 112, -1, 1);
            var badge = LabUI.Panel(tile.transform, "ნიშანი", new Color(1, 1, 1, 0.9f));
            Place(badge.rectTransform, new Vector2(0, 0.5f), new Vector2(54, 4), new Vector2(72, 72));
            var art = LabUI.Icon(badge.transform, icon, Color.white);
            LabUI.Fill(art.rectTransform, 10);
            var label = tile.GetComponentInChildren<TMP_Text>();
            label.text = "<size=24>" + title + "</size>\n<size=16><color=#FFFFFFCC>" + line + "</color></size>";
            label.alignment = TextAlignmentOptions.Left; label.fontSizeMax = 24;
            label.rectTransform.offsetMin = new Vector2(104, 12);
        }

        /// <summary>A lesson tile: coloured with a check when done, light and pulsing-ready when next, grey with a lock otherwise.</summary>
        private void LessonTile(Transform parent, Lesson lesson, Color color)
        {
            bool done = Curriculum.Done(lesson.ID), open = Curriculum.Available(lesson);
            var tile = LabUI.Button(parent, "", () =>
            {
                if (!open) { game.HUD.Notify(Curriculum.Requirement(lesson)); return; }
                hub.Close();
                if (done && !lesson.IsReview) OpenWorkbook(lesson, true); else Begin(lesson);
            }, 96);
            tile.GetComponent<Image>().color = done ? color : open ? LabUI.Paper : new Color32(205, 205, 196, 255);
            var icon = LabUI.Icon(tile.transform, lesson.IsReview ? LabIcon.Trophy : done ? LabIcon.Check : open ? LabIcon.Flask : LabIcon.Lock, Color.white);
            Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(34, 4), new Vector2(40, 40));
            var label = tile.GetComponentInChildren<TMP_Text>();
            label.text = Curriculum.Label(lesson) + "\n<size=13>" + (done ? "ვარჯიში" : open ? "დაწყება" : "დაკეტილია") + "</size>";
            label.color = done ? Color.white : LabUI.Ink;
            label.alignment = TextAlignmentOptions.Left; label.fontSizeMin = 14; label.fontSizeMax = 19;
            label.rectTransform.offsetMin = new Vector2(64, 12);
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
        }
    }
}
