using System;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class LabBoardUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow rankings;
        private LabWindow quests;
        public void Initialize(LabGame lab) => game = lab;
        public void OpenRankings()
        {
            if (rankings != null) { LabUI.BringToFront(rankings); return; }
            rankings = LabUI.Window("კვირის ლაბორატორიული ლიგა", 950, 640);
            rankings.Closed = () => rankings = null;
            string end = GameSaveManager.Iso(GameSaveManager.WeekStart(DateTime.UtcNow.Date).AddDays(7));
            var banner = LabUI.Panel(rankings.Body, "კვირის ჯილდო", LabUI.Navy);
            LabUI.Size(banner.gameObject, 96);
            var trophy = LabUI.Icon(banner.transform, LabIcon.Trophy, LabUI.Gold);
            trophy.rectTransform.anchorMin = trophy.rectTransform.anchorMax = new Vector2(0, 0.5f);
            trophy.rectTransform.pivot = new Vector2(0, 0.5f);
            trophy.rectTransform.anchoredPosition = new Vector2(18, 0);
            trophy.rectTransform.sizeDelta = new Vector2(64, 64);
            var headline = LabUI.Text(banner.transform, "პირველი ადგილი — ოქროს გვირგვინი\n<size=16><color=#C1C5E2>კვირა სრულდება " + end + " • 00:00 UTC</color></size>", 22, Color.white);
            LabUI.Fill(headline.rectTransform, 16);
            headline.rectTransform.offsetMin = new Vector2(104, 16);
            var content = LabUI.Scroll(rankings.Body);
            int rank = 0;
            foreach (var entry in game.Rankings.Entries())
            {
                rank++;
                Color accent = entry.IsPlayer ? LabUI.Mint : rank == 1 ? new Color32(201, 144, 42, 255) : LabUI.Green;
                var card = LabUI.Card(content, entry.Name, entry.IsPlayer ? new Color32(220, 248, 240, 255) : LabUI.Soft, 12);
                var row = LabUI.Row(card.transform, "შედეგი");
                LabUI.Pill(row.transform, rank.ToString(), accent, 44);
                var name = LabUI.Text(row.transform, entry.Name, 21);
                LabUI.Size(name.gameObject, 32, -1, 1);
                LabUI.Size(LabUI.Text(row.transform, entry.Score + " ცდა", 20, accent).gameObject, 32, 120, 0);
                LabUI.Progress(card.transform, Mathf.Clamp01(entry.Score / 65f), accent, 5);
            }
            var note = LabUI.Text(rankings.Body, "შენი წოდება — " + game.Rankings.Title + "\nრეიტინგში სხვა მოთამაშეები ვირტუალური კლასელები არიან. თანაბარი ქულისას უპირატესობა კლასელს ენიჭება.", 16, LabUI.Muted);
            LabUI.Size(note.gameObject, 48);
        }
        public void OpenQuests()
        {
            if (quests != null) { LabUI.BringToFront(quests); return; }
            quests = LabUI.Window("დღის აღმოჩენები", 940, 610);
            quests.Closed = () => quests = null;
            var save = GameSaveManager.Instance;
            var status = LabUI.Row(quests.Body, "დღის პროგრესი");
            LabUI.Size(LabUI.Text(status.transform, "დღეს ამოხსნილია " + save.Data.dailyQuizzesSolved + " დავალება", 21).gameObject, 34, -1, 1);
            LabUI.Pill(status.transform, "განახლება • 00:00 UTC", LabUI.Mint, 240);
            var content = LabUI.Scroll(quests.Body);
            int[] goals = { 3, 5, 10 }, rewards = { 10, 20, 40 };
            for (int i = 0; i < goals.Length; i++)
            {
                int goal = goals[i], reward = rewards[i];
                var card = LabUI.Card(content, "დღის გამოწვევა", LabUI.Soft, 18);
                var row = LabUI.Row(card.transform, "დავალება");
                var icon = LabUI.Icon(row.transform, LabIcon.Quest, i == 0 ? LabUI.Mint : i == 1 ? LabUI.Green : LabUI.Coral);
                LabUI.Size(icon.gameObject, 48, 48);
                var text = LabUI.Text(row.transform, "ამოხსენი " + goal + " დავალება\n<size=16><color=#6F7796>ჯილდო • " + reward + " ქიმქოინი</color></size>", 22);
                LabUI.Size(text.gameObject, 65, -1, 1);
                bool claimed = save.Data.claimedDailyQuests.Contains(goal.ToString());
                var button = LabUI.Button(row.transform, claimed ? "მიღებულია" : "ჯილდოს მიღება", () =>
                {
                    if (!save.ClaimQuest(goal, reward)) return;
                    AudioManager.Instance?.Coin();
                    quests.Close();
                    OpenQuests();
                });
                LabUI.Size(button.gameObject, 46, 175, 0);
                button.interactable = !claimed && save.Data.dailyQuizzesSolved >= goal;
                LabUI.Progress(card.transform, save.Data.dailyQuizzesSolved / (float)goal, LabUI.Mint);
            }
        }
    }
}
