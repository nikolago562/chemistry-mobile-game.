using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class ChemMinigameUI : MonoBehaviour
    {
        public UnityEvent onSuccess = new UnityEvent();
        public UnityEvent onError = new UnityEvent();
        public UnityEvent onVisualSuccess = new UnityEvent();
        public UnityEvent onVisualError = new UnityEvent();
        public bool IsOpen => window != null;
        public ChemistryQuestionSO Current { get; private set; }
        public TMP_InputField Answer { get; private set; }
        public TMP_Text Result { get; private set; }
        private readonly Queue<ChemistryQuestionSO> queue = new Queue<ChemistryQuestionSO>();
        private ChemistryQuestionSO last;
        private LabWindow window;
        private LabGame game;
        private Button submit;
        private Button hintButton;
        private TMP_Text hint;
        private bool solved;
        private bool tutorial;
        private bool visualDesk;
        private Coroutine closing;
        private float shakeUntil;
        private Vector2 cardOrigin;

        public void Initialize(LabGame lab) => game = lab;
        public void Open(bool guided = false, bool visual = false, ChemistryQuestionSO selected = null)
        {
            if (IsOpen) return;
            tutorial = guided;
            visualDesk = visual;
            solved = false;
            Current = selected != null ? selected : guided ? game.questions[0] : Next();
            window = LabUI.Window(guided ? "პირველი ცდა — წყლის მიღება" : visual ? "ვიზუალური ექსპერიმენტი" : "რეაქციის გამოწვევა", 940, 630);
            window.Closed = OnClosed;
            cardOrigin = window.Card.anchoredPosition;
            var area = window.Body;
            var tags = LabUI.Row(area, "დავალების მონაცემები");
            LabUI.Pill(tags.transform, Current.CategoryName, LabUI.Green);
            LabUI.Pill(tags.transform, "სირთულე " + Current.difficultyTier, LabUI.Mint);
            LabUI.Pill(tags.transform, "+" + Current.coinReward + " ქიმქოინი", new Color32(205, 146, 38, 255));

            var reaction = LabUI.Panel(area, "რეაქციის დაფა", LabUI.Navy);
            LabUI.Size(reaction.gameObject, 126);
            var icon = LabUI.Icon(reaction.transform, LabIcon.Flask, LabUI.Mint);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0, 0.5f);
            icon.rectTransform.pivot = new Vector2(0, 0.5f);
            icon.rectTransform.anchoredPosition = new Vector2(24, 0);
            icon.rectTransform.sizeDelta = new Vector2(76, 76);
            icon.gameObject.AddComponent<FlaskMotion>();
            var prompt = LabUI.Text(reaction.transform, Current.equationPrompt, 34, Color.white);
            prompt.alignment = TextAlignmentOptions.Center;
            LabUI.Fill(prompt.rectTransform);
            prompt.rectTransform.offsetMin = new Vector2(122, 12);
            prompt.rectTransform.offsetMax = new Vector2(-24, -12);
            prompt.enableAutoSizing = true;
            prompt.fontSizeMin = 25;
            prompt.fontSizeMax = 34;

            var row = LabUI.Row(area, "პასუხის შეყვანა");
            Answer = LabUI.Input(row.transform, "ჩაწერე გამოტოვებული ნაწილი…");
            submit = LabUI.Button(row.transform, "შემოწმება", Submit, 60);
            LabUI.Size(submit.gameObject, 60, 190, 0);
            Answer.onSubmit.AddListener(_ => Submit());
            Result = LabUI.Text(area, "Enter — პასუხის შემოწმება", 19, LabUI.Muted);
            LabUI.Size(Result.gameObject, 32);
            var hintRow = LabUI.Row(area, "მინიშნება", 14);
            LabUI.FlexibleHeight(hintRow.gameObject);
            hintButton = LabUI.Secondary(hintRow.transform, "მინიშნება", ToggleHint, 42);
            LabUI.Size(hintButton.gameObject, 42, 145, 0);
            var hintCard = LabUI.Card(hintRow.transform, "დახმარება", LabUI.Soft, 14);
            LabUI.Size(hintCard.gameObject, 78, -1, 1);
            LabUI.FlexibleHeight(hintCard.gameObject);
            var hintArea = LabUI.Scroll(hintCard.transform);
            hint = LabUI.Text(hintArea, guided
                ? "შეაერთე წყალბადი და ჟანგბადი წყლის მისაღებად. არ დაგავიწყდეს კოეფიციენტი 2!"
                : Current.hintText, 18);
            hint.gameObject.SetActive(guided);
            hintButton.gameObject.SetActive(guided);
            var footer = LabUI.Row(area, "ცნობარი");
            LabUI.Secondary(footer.transform, "[N] ფორმულების რვეული", () => game.Notebook.Open(), 40);
            if (!guided) LabUI.Secondary(footer.transform, "[Tab] ელემენტების კოლექცია", () => game.Periodic.Open(), 40);
            Answer.Select();
            Answer.ActivateInputField();
        }
        private ChemistryQuestionSO Next()
        {
            if (queue.Count == 0)
            {
                var list = new List<ChemistryQuestionSO>(game.questions);
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }
                if (list.Count > 1 && list[0] == last) (list[0], list[1]) = (list[1], list[0]);
                foreach (var question in list) queue.Enqueue(question);
            }
            last = queue.Dequeue();
            return last;
        }
        public static string Normalize(string input) => input.Trim().Replace(" ", "").ToUpperInvariant();

        public void Submit()
        {
            if (!IsOpen || solved || string.IsNullOrWhiteSpace(Answer.text)) return;
            bool correct = Normalize(Answer.text) == Normalize(Current.correctAnswer);
            bool visual = visualDesk || Current.isVisualExperiment ||
                (GameSaveManager.Instance.Data.totalQuizzesSolved + 1) % 5 == 0;
            if (!correct)
            {
                Result.text = "არასწორია! სცადე თავიდან.";
                Result.color = new Color32(185, 57, 57, 255);
                GameSaveManager.Instance.RecordWrong();
                AudioManager.Instance?.Wrong();
                onError.Invoke();
                hintButton.gameObject.SetActive(true);
                if (visual) { game.Visuals.React(false); onVisualError.Invoke(); }
                shakeUntil = Time.unscaledTime + 0.25f;
                Answer.ActivateInputField();
                Answer.Select();
                return;
            }

            solved = true;
            int oldStreak = GameSaveManager.Instance.Data.currentDailyStreak;
            int reward = GameSaveManager.Instance.RecordCorrect(Current, DateTime.UtcNow.Date);
            if (GameSaveManager.Instance.Data.currentDailyStreak > oldStreak && oldStreak > 0)
                AudioManager.Instance?.Milestone();
            if (tutorial) game.Tutorial.CompleteFirstReaction();
            game.Rankings.SubmitScore();
            Result.text = "სწორია!  +" + reward + " ქიმქოინი";
            Result.color = new Color32(20, 151, 113, 255);
            submit.interactable = false;
            Answer.interactable = false;
            AudioManager.Instance?.Correct();
            AudioManager.Instance?.Coin();
            onSuccess.Invoke();
            if (visual || tutorial) { game.Visuals.React(true); onVisualSuccess.Invoke(); }
            if (GameSaveManager.Instance.Combo == 3 || GameSaveManager.Instance.Combo == 5 || GameSaveManager.Instance.Combo == 10)
                AudioManager.Instance?.Milestone();
            closing = StartCoroutine(CloseAfterSuccess());
        }
        private System.Collections.IEnumerator CloseAfterSuccess()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            Close();
        }
        private void ToggleHint()
        {
            hint.gameObject.SetActive(!hint.gameObject.activeSelf);
            hintButton.GetComponentInChildren<TMP_Text>().text = hint.gameObject.activeSelf ? "მინიშნების დამალვა" : "მინიშნება";
        }
        private void Update()
        {
            if (window == null) return;
            window.Card.anchoredPosition = cardOrigin + Vector2.right *
                (Time.unscaledTime < shakeUntil ? Mathf.Sin(Time.unscaledTime * 95) * 6 : 0);
        }
        public void Close() { if (window != null) window.Close(); }
        private void OnClosed()
        {
            if (closing != null) StopCoroutine(closing);
            window = null;
            closing = null;
        }
    }
}



