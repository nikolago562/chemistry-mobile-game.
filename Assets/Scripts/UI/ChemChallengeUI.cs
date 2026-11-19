using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// The advanced challenge floor: random questions in four tiers, combos, timed answers and a
    /// mixed exam. It opens only after the beginner curriculum, and each tier needs the one before.
    /// </summary>
    public sealed class ChemChallengeUI : MonoBehaviour
    {
        public const int ExamLength = 10, SpeedSeconds = 25;
        private LabGame game;
        private LabWindow window;
        private TMP_Text feedback, rewardLabel, timerLabel;
        private Button next, hintButton;
        private readonly List<Button> options = new List<Button>();
        private ChemistryQuestionSO payout;
        private bool answered, usedHint, hadError;
        private int sessionSolved, examIndex, examScore;
        private float deadline;
        public int Tier { get; private set; }
        public StudyQuestion Current { get; private set; }
        public bool IsOpen => window != null;
        public void Initialize(LabGame owner)
        {
            game = owner;
            payout = ScriptableObject.CreateInstance<ChemistryQuestionSO>();
        }
        public void Open()
        {
            if (!Curriculum.Complete)
            {
                game.HUD.Notify(Curriculum.LockedText + " რთული გამოწვევები თეორიის ყველა თავის შემდეგ გაიხსნება.");
                return;
            }
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("რთული გამოწვევების მაგიდა • ოსტატობის საფეხურები", 1080, 660);
            window.Closed = () => { window = null; Current = null; };
            Home();
        }
        private void Clear()
        {
            foreach (Transform child in window.Body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            options.Clear();
            timerLabel = null;
        }
        private TMP_Text Copy(Transform parent, string text, float height, float size = 19, Color? color = null)
        {
            var label = LabUI.Text(parent, text, size, color);
            LabUI.Size(label.gameObject, height);
            label.enableAutoSizing = true; label.fontSizeMin = 11; label.fontSizeMax = size;
            return label;
        }
        private void Home()
        {
            Clear(); Current = null;
            Copy(window.Body, "საფუძვლები ისწავლე — ახლა შეამოწმე ოსტატობა! ყოველი საფეხური წინას " + Curriculum.TierGoal + " სწორი პასუხით იხსნება.", 34, 19, LabUI.Green);
            var row = LabUI.Row(window.Body, "საფეხურები", 12);
            LabUI.Size(row.gameObject, 330);
            TierCard(row.transform, 1, "საფეხური 1 • საფუძვლები", "ელემენტების ამოცნობა, ატომების დათვლა, ფორმულის წაკითხვა, რეაგენტები და პროდუქტები.", LabUI.Mint);
            TierCard(row.transform, 2, "საფეხური 2 • აწყობა", "მოლეკულის ფორმულა, რეაქციის ტიპი, მარტივი დაბალანსება და კონცენტრაცია.", LabUI.Gold);
            TierCard(row.transform, 3, "საფეხური 3 • ოსტატობა", "რთული დაბალანსება, სტექიომეტრია, pH-ის გამოთვლა და ექსპერტის კითხვები. სწრაფ პასუხს ბონუსი აქვს.", LabUI.Coral);
            TierCard(row.transform, 4, "საფეხური 4 • გამოცდა", ExamLength + " შემთხვევითი კითხვა ყველა საფეხურიდან. 8 ქულა ან მეტი — ექსპერტის თასი. იხსნება სამეცნიერო ბაზრობაც.", new Color32(170, 150, 230, 255));
            Copy(window.Body, "ზედიზედ სწორი პასუხები კომბოს ზრდის (×1.5, ×2, ×3). მინიშნების ან შეცდომის შემდეგ საბაზო ჯილდოს 60%-ს მიიღებ. სწორი პასუხები კვირის რეიტინგში ითვლება.", 46, 16, LabUI.Muted);
            var note = LabUI.Row(window.Body, "გამეორება", 10);
            LabUI.Size(LabUI.Text(note.transform, "რამე დაგავიწყდა? საფუძვლები ყოველთვის შეგიძლია გაიმეორო.", 16, LabUI.Ink).gameObject, 40, -1, 1);
            LabUI.Size(LabUI.Secondary(note.transform, "სასწავლო რუკა [M]", () => { window.Close(); game.Lessons.OpenMap(); }, 40).gameObject, 40, 220, 0);
        }
        private void TierCard(Transform parent, int tier, string title, string description, Color accent)
        {
            bool open = Curriculum.TierOpen(tier);
            var card = LabUI.Card(parent, title, open ? LabUI.Navy : (Color)new Color32(78, 86, 80, 255), 14);
            LabUI.Size(card.gameObject, 330, -1, 1);
            var top = LabUI.Row(card.transform, "ნიშანი", 10);
            var image = LabUI.Icon(top.transform, open ? (tier == 4 ? LabIcon.Trophy : LabIcon.Flask) : LabIcon.Lock, Color.white); LabUI.Size(image.gameObject, 40, 40);
            var label = Copy(top.transform, title, 52, 19, accent); label.fontStyle = FontStyles.Bold; LabUI.Size(label.gameObject, 52, -1, 1);
            Copy(card.transform, description, 110, 16, LabUI.Paper);
            string progress = tier >= 4 ? "საუკეთესო შედეგი: " + LabProgress.Stat("exam_best") + " / " + ExamLength
                : "ამოხსნილი: " + Curriculum.TierSolved(tier) + (Curriculum.TierSolved(tier) < Curriculum.TierGoal ? " / " + Curriculum.TierGoal : "  • შემდეგი გახსნილია");
            Copy(card.transform, open ? progress : "დაკეტილია: ჯერ წინა საფეხურზე " + Curriculum.TierGoal + " კითხვა ამოხსენი.", 44, 15, open ? accent : LabUI.Paper);
            var button = LabUI.Button(card.transform, open ? "დაწყება →" : "დაკეტილია", () => StartTier(tier), 48);
            button.interactable = open;
        }

        /// <summary>Starts (or continues) a round on a tier; tier 4 is the mixed exam.</summary>
        public void StartTier(int tier)
        {
            if (!Curriculum.TierOpen(tier)) { game.HUD.Notify(Curriculum.LockedText + " ჯერ წინა საფეხური დაასრულე."); return; }
            if (window == null) Open();
            if (window == null) return;
            if (tier == 4 && (Tier != 4 || examIndex >= ExamLength || Current == null)) { examIndex = 0; examScore = 0; }
            Tier = tier;
            answered = usedHint = hadError = false;
            Current = ChemistryChallenges.Tier(tier, game);
            deadline = Time.unscaledTime + SpeedSeconds;
            Clear();
            var tags = LabUI.Row(window.Body, "სასწავლო ბარათი", 10);
            LabUI.Size(tags.gameObject, 32);
            LabUI.Pill(tags.transform, tier == 4 ? "გამოცდა • " + (examIndex + 1) + " / " + ExamLength : "საფეხური " + tier, LabUI.Green);
            rewardLabel = LabUI.Pill(tags.transform, "ჯილდო: +" + Current.Reward + " ქიმქოინი", new Color32(170, 112, 34, 255));
            LabUI.Pill(tags.transform, "სწორი პასუხები: " + sessionSolved, LabUI.Green);
            if (tier >= 3) timerLabel = LabUI.Pill(tags.transform, "", new Color32(151, 62, 45, 255));
            var panel = LabUI.Panel(window.Body, "შეკითხვა", LabUI.Navy); LabUI.Size(panel.gameObject, 118);
            var prompt = LabUI.Text(panel.transform, Current.Prompt, 26, LabUI.Paper);
            LabUI.Fill(prompt.rectTransform, 16); prompt.alignment = TextAlignmentOptions.Center;
            prompt.enableAutoSizing = true; prompt.fontSizeMin = 16; prompt.fontSizeMax = 28;
            var order = new List<int>(); for (int i = 0; i < Current.Choices.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--) { int j = UnityEngine.Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
            for (int r = 0; r < 2; r++)
            {
                var row = LabUI.Row(window.Body, "პასუხები", 14); LabUI.Size(row.gameObject, 64);
                for (int c = 0; c < 2 && r * 2 + c < order.Count; c++)
                {
                    int index = order[r * 2 + c];
                    var button = LabUI.Button(row.transform, (char)('A' + r * 2 + c) + "  " + Current.Choices[index], () => Choose(index), 64);
                    button.GetComponent<Image>().color = new Color32(55, 91, 77, 255);
                    options.Add(button);
                }
            }
            var explanation = LabUI.Card(window.Body, "ახსნა და მინიშნება", LabUI.Soft, 10); LabUI.Size(explanation.gameObject, 106);
            var scroll = LabUI.Scroll(explanation.transform);
            feedback = LabUI.Text(scroll, "აირჩიე პასუხი. აქ გაიგებ, რატომ არის ის სწორი ან არასწორი.", 18, LabUI.Ink);
            var footer = LabUI.Row(window.Body, "ქმედებები", 12);
            hintButton = LabUI.Secondary(footer.transform, "მინიშნება", UseHint, 44);
            LabUI.Secondary(footer.transform, "← საფეხურები", Home, 44);
            next = LabUI.Button(footer.transform, tier == 4 && examIndex + 1 >= ExamLength ? "შედეგები →" : "შემდეგი →", NextQuestion, 44); next.interactable = false;
        }
        private void Update()
        {
            if (timerLabel == null || answered) return;
            float left = deadline - Time.unscaledTime;
            timerLabel.text = left > 0 ? "სისწრაფის ბონუსი: " + Mathf.CeilToInt(left) + " წმ" : "დრო ამოიწურა — ბონუსის გარეშე";
        }
        private void NextQuestion()
        {
            if (!answered) return;
            if (Tier == 4)
            {
                examIndex++;
                if (examIndex >= ExamLength) { ExamResult(); return; }
            }
            StartTier(Tier);
        }
        private void ExamResult()
        {
            Clear(); Current = null;
            bool passed = examScore >= 8;
            var data = GameSaveManager.Instance.Data;
            LabProgress.Add(data.stats, "exam_best", Math.Max(0, examScore - LabProgress.Stat("exam_best")));
            if (passed) { data.chemCoins += 60; LabProgress.Collect("award:exam"); }
            GameSaveManager.Instance.Commit();
            var title = Copy(window.Body, "გამოცდის შედეგი: " + examScore + " / " + ExamLength, 56, 34, passed ? LabUI.Green : LabUI.Coral); title.fontStyle = FontStyles.Bold;
            Copy(window.Body, passed ? "ბრწყინვალეა! ექსპერტის თასი და +60 ქიმქოინი შენია. სამეცნიერო ბაზრობაზე გამოფენის დარბაზში შეგიძლია მონაწილეობა."
                : "კარგი ცდაა! 8 ქულისთვის გაიმეორე სუსტი თემები სასწავლო რუკაზე და სცადე თავიდან. შეცდომა პროგრესს არ აკარგვინებს.", 90, 20, LabUI.Ink);
            var footer = LabUI.Row(window.Body, "ქმედებები", 12);
            LabUI.Button(footer.transform, "ახალი გამოცდა", () => StartTier(4), 48);
            LabUI.Secondary(footer.transform, "← საფეხურები", Home, 48);
        }
        public void UseHint()
        {
            if (Current == null || answered) return;
            usedHint = true; feedback.text = "მინიშნება: " + Current.Hint;
            rewardLabel.text = "ჯილდო: +" + Mathf.Max(1, Mathf.RoundToInt(Current.Reward * 0.6f)) + " ქიმქოინი";
        }
        public void Choose(int index)
        {
            if (Current == null || answered || index < 0 || index >= Current.Choices.Length) return;
            if (index != Current.Correct)
            {
                hadError = true; GameSaveManager.Instance.RecordWrong(); AudioManager.Instance?.Wrong();
                feedback.color = new Color32(151, 62, 45, 255);
                feedback.text = "არასწორია. " + Current.Feedback[index] + "\nაირჩიე სხვა პასუხი ან გამოიყენე მინიშნება.";
                rewardLabel.text = "ჯილდო: +" + Mathf.Max(1, Mathf.RoundToInt(Current.Reward * 0.6f)) + " ქიმქოინი";
                return;
            }
            answered = true;
            bool fast = Tier >= 3 && Time.unscaledTime <= deadline && !usedHint && !hadError;
            payout.questionID = "study_" + Current.ID;
            payout.coinReward = usedHint || hadError ? Mathf.Max(1, Mathf.RoundToInt(Current.Reward * 0.6f)) : fast ? Mathf.RoundToInt(Current.Reward * 1.5f) : Current.Reward;
            payout.involvedElements = Current.Elements;
            int coins = GameSaveManager.Instance.RecordCorrect(payout, DateTime.UtcNow.Date);
            if (Tier == 4 && !hadError && !usedHint) examScore++;
            string unlocked = Curriculum.CountTier(Tier);
            GameSaveManager.Instance.Commit();
            game.Rankings.SubmitScore(); sessionSolved++;
            feedback.color = LabUI.Green;
            feedback.text = "სწორია! +" + coins + " ქიმქოინი" + (fast ? " (სისწრაფის ბონუსი!)" : "") + "\n" + Current.Explanation;
            if (unlocked != null) { game.HUD.Notify(unlocked); AudioManager.Instance?.Milestone(); feedback.text += "\n" + unlocked; }
            foreach (var button in options) button.interactable = false;
            hintButton.interactable = false; next.interactable = true;
            if (timerLabel != null) timerLabel.text = fast ? "სისწრაფის ბონუსი მიღებულია!" : "ბონუსის გარეშე";
            AudioManager.Instance?.Correct(); AudioManager.Instance?.Coin(); game.Visuals.React(true);
        }
        public void Close() { if (window != null) window.Close(); }
        private void OnDestroy() { if (payout != null) Destroy(payout); }
    }
}
