using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class WeeklyLeaderboardManager : MonoBehaviour
    {
        public const int TopRivalScore = 64;
        public readonly struct Entry
        {
            public readonly string Name;
            public readonly int Score;
            public readonly bool IsPlayer;
            public Entry(string name, int score, bool player = false) { Name = name; Score = score; IsPlayer = player; }
        }
        // Subscribe to this event to attach an online leaderboard transport.
        public event Action<int> ScoreSubmitted;
        public Entry[] Entries()
        {
            var save = GameSaveManager.Instance;
            var all = new List<Entry>
            {
                new Entry("თამარი", TopRivalScore), new Entry("ნიკა", 42), new Entry("მარიამი", 28),
                new Entry("გიორგი", 16), new Entry("ანა", 8),
                new Entry(save.Data.profileCompleted ? save.Data.playerName : "შენ", save.Data.weeklyQuizzesSolved, true)
            };
            return all.OrderByDescending(e => e.Score).ThenBy(e => e.IsPlayer ? 1 : 0).ToArray();
        }
        public int Rank => Array.FindIndex(Entries(), e => e.IsPlayer) + 1;
        public string Title => !Curriculum.Complete ? Curriculum.Title : GameSaveManager.Instance.Data.weeklyQuizzesSolved > TopRivalScore ? "კვირის ალქიმიკოსი"
            : GameSaveManager.Instance.Data.totalQuizzesSolved >= 100 ? "ლაბორატორიის ლეგენდა"
            : GameSaveManager.Instance.Data.weeklyQuizzesSolved >= 25 ? "სტექიომეტრიის ოსტატი" : "დამწყები ქიმიკოსი";
        public void SubmitScore() => ScoreSubmitted?.Invoke(GameSaveManager.Instance.Data.weeklyQuizzesSolved);
    }
}
