using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    public enum TeacherQuestKind { SolveReactions, DiscoverElements, ReachCombo }

    public sealed class TeacherQuest
    {
        public readonly string ID, Title, Pitch, Goal;
        public readonly TeacherQuestKind Kind;
        public readonly int Target, Reward;
        public TeacherQuest(string id, string title, string pitch, string goal, TeacherQuestKind kind, int target, int reward)
        { ID = id; Title = title; Pitch = pitch; Goal = goal; Kind = kind; Target = target; Reward = reward; }
    }

    /// <summary>
    /// Quests handed out by the teacher. Only one is active at a time. Progress is measured
    /// against save counters captured when the quest was accepted, so nothing extra has to be
    /// recorded by the experiments themselves.
    /// </summary>
    public static class TeacherQuests
    {
        public static readonly TeacherQuest[] Pool =
        {
            new TeacherQuest("warmup", "პატარა გახურება",
                "დავიწყოთ მსუბუქად. ამოხსენი სამი რეაქცია ნებისმიერ სამუშაო მაგიდასთან.",
                "ამოხსენი 3 რეაქცია", TeacherQuestKind.SolveReactions, 3, 25),
            new TeacherQuest("marathon", "რეაქციების მარათონი",
                "ნამდვილ ქიმიკოსს მოთმინება სჭირდება. ამოხსენი ექვსი რეაქცია.",
                "ამოხსენი 6 რეაქცია", TeacherQuestKind.SolveReactions, 6, 50),
            new TeacherQuest("scholar", "ბეჯითი მოსწავლე",
                "დიდი გამოწვევა მაქვს შენთვის: ათი რეაქცია. შეძლებ?",
                "ამოხსენი 10 რეაქცია", TeacherQuestKind.SolveReactions, 10, 90),
            new TeacherQuest("explorer", "ელემენტების მაძიებელი",
                "პერიოდულ ცხრილში ჯერ კიდევ ბევრი საიდუმლოა. აღმოაჩინე ორი ახალი ელემენტი.",
                "აღმოაჩინე 2 ახალი ელემენტი", TeacherQuestKind.DiscoverElements, 2, 30),
            new TeacherQuest("collector", "ცხრილის შემვსები",
                "მინდა, შენი კოლექცია გაიზარდოს. აღმოაჩინე ხუთი ახალი ელემენტი.",
                "აღმოაჩინე 5 ახალი ელემენტი", TeacherQuestKind.DiscoverElements, 5, 65),
            new TeacherQuest("steady", "ზედიზედ სამი",
                "სიზუსტე მნიშვნელოვანია. ამოხსენი სამი რეაქცია ზედიზედ, შეცდომის გარეშე.",
                "კომბო ×3 შეცდომის გარეშე", TeacherQuestKind.ReachCombo, 3, 35),
            new TeacherQuest("flawless", "უშეცდომო ხუთეული",
                "ახლა ხუთი სწორი პასუხი ზედიზედ. ერთი შეცდომა და თავიდან იწყებ!",
                "კომბო ×5 შეცდომის გარეშე", TeacherQuestKind.ReachCombo, 5, 60),
        };

        private static GameSaveData Data => GameSaveManager.Instance.Data;
        private static int lastCombo;

        public static TeacherQuest Active =>
            string.IsNullOrEmpty(Data.teacherQuestID) ? null : Pool.FirstOrDefault(q => q.ID == Data.teacherQuestID);

        public static int Progress(TeacherQuest quest)
        {
            if (quest == null) return 0;
            int value = quest.Kind switch
            {
                TeacherQuestKind.SolveReactions => Data.totalQuizzesSolved - Data.teacherQuestStart,
                TeacherQuestKind.DiscoverElements => Data.unlockedElements.Count - Data.teacherQuestStart,
                _ => Data.teacherQuestBest,
            };
            return Mathf.Clamp(value, 0, quest.Target);
        }

        public static bool Done(TeacherQuest quest) => quest != null && Progress(quest) >= quest.Target;

        /// <summary>Two or three quests to choose from; the selection changes daily and after each turn-in.</summary>
        public static List<TeacherQuest> Offers(LabGame game)
        {
            int totalElements = game.elements != null && game.elements.elements != null && game.elements.elements.Length > 0
                ? game.elements.elements.Length : 118;
            int remaining = totalElements - Data.unlockedElements.Count;
            var candidates = Pool.Where(q => q.Kind != TeacherQuestKind.DiscoverElements || remaining >= q.Target).ToList();
            int seed = GameSaveManager.Iso(DateTime.UtcNow.Date).GetHashCode() ^ (Data.teacherQuestsCompleted * 7919);
            var random = new System.Random(seed);
            var shuffled = candidates.OrderBy(_ => random.Next()).ToList();
            int count = Mathf.Min(shuffled.Count, Data.teacherQuestsCompleted == 0 ? 2 : 3);
            // The very first offer always includes the gentle warm-up.
            if (Data.teacherQuestsCompleted == 0)
            {
                var warmup = Pool[0];
                shuffled.Remove(warmup);
                shuffled.Insert(0, warmup);
            }
            return shuffled.Take(count).ToList();
        }

        public static void Accept(TeacherQuest quest)
        {
            var save = GameSaveManager.Instance;
            Data.teacherQuestID = quest.ID;
            Data.teacherQuestBest = 0;
            Data.teacherQuestStart = quest.Kind switch
            {
                TeacherQuestKind.SolveReactions => Data.totalQuizzesSolved,
                TeacherQuestKind.DiscoverElements => Data.unlockedElements.Count,
                _ => save.Combo,
            };
            lastCombo = save.Combo;
            save.Commit();
        }

        public static void Abandon()
        {
            Data.teacherQuestID = "";
            GameSaveManager.Instance.Commit();
        }

        /// <summary>Pays out the active quest if it is complete. Returns the reward, or 0.</summary>
        public static int TurnIn()
        {
            var quest = Active;
            if (!Done(quest)) return 0;
            Data.chemCoins += quest.Reward;
            Data.teacherQuestsCompleted++;
            Data.teacherQuestID = "";
            GameSaveManager.Instance.Commit();
            AudioManager.Instance?.Coin();
            return quest.Reward;
        }

        /// <summary>Follows the combo counter for streak quests. Called whenever the save changes.</summary>
        public static void Track()
        {
            var quest = Active;
            var save = GameSaveManager.Instance;
            if (quest == null || quest.Kind != TeacherQuestKind.ReachCombo) { lastCombo = save.Combo; return; }
            // A wrong answer resets the combo; after that, every streak counts from zero.
            if (save.Combo < lastCombo) Data.teacherQuestStart = 0;
            lastCombo = save.Combo;
            Data.teacherQuestBest = Mathf.Max(Data.teacherQuestBest, save.Combo - Data.teacherQuestStart);
        }
    }
}
