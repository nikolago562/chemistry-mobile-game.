using System;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>One thing to do: a one-line objective, when it is done, and where in the world it happens.</summary>
    public sealed class MissionStep
    {
        public Func<string> Text;
        public Func<bool> Done;
        public Func<Vector3?> Target = () => null;
    }

    public sealed class Mission
    {
        public string ID, Title;
        public int Coins, Reputation = 1;
        /// <summary>Lab item given on completion (earned decoration), or "".</summary>
        public string Item = "";
        public MissionStep[] Steps;
    }

    /// <summary>
    /// Runs the story missions in order. The current step drives the single HUD objective,
    /// the objective glow and the waypoint arrow. World objects set flags; finishing the last
    /// step pays the reward and starts the next mission.
    /// </summary>
    public static class Missions
    {
        public static Mission[] All = new Mission[0];
        private static GameSaveData Data => GameSaveManager.Instance.Data;

        public static bool Flag(string flag) => Data.missionFlags.Contains(flag);
        public static void Set(string flag)
        {
            if (!Flag(flag)) { Data.missionFlags.Add(flag); GameSaveManager.Instance.Commit(); }
            Refresh();
        }
        public static bool IsDone(string id) => Data.missionsDone.Contains(id);

        public static Mission Current => All.FirstOrDefault(m => !IsDone(m.ID));
        public static MissionStep Step => Current?.Steps.FirstOrDefault(s => !s.Done());
        public static bool Active(Mission mission, int step) => Current == mission && Step == mission.Steps[step];
        /// <summary>True while a mission step is guiding the player (its target owns the objective glow).</summary>
        public static bool Steering => Data.profileCompleted && Step != null;
        public static Vector3? Target => Steering ? Step.Target() : null;
        public static bool IsTarget(Vector3 point) => Target is Vector3 t && Vector2.Distance(t, point) < 1.6f;
        public static int StepIndex => Current == null ? 0 : Array.IndexOf(Current.Steps, Step);

        /// <summary>Pays for every mission whose steps are all done, in order, and announces the next one.</summary>
        public static void Refresh()
        {
            var game = LabGame.Instance;
            bool finished = false;
            while (Current != null && Step == null)
            {
                var mission = Current;
                Data.missionsDone.Add(mission.ID);
                Data.chemCoins += mission.Coins;
                LabProgress.AddReputation(mission.Reputation);
                if (mission.Item != "" && !Data.ownedLabItems.Contains(mission.Item))
                {
                    Data.ownedLabItems.Add(mission.Item);
                    game?.Personal?.PlaceAnywhere(mission.Item);
                }
                GameSaveManager.Instance.Commit();
                AudioManager.Instance?.Milestone(); AudioManager.Instance?.Coin();
                game?.HUD?.Notify("მისია შესრულდა: „" + mission.Title + "“" + (mission.Coins > 0 ? " • +" + mission.Coins + " ქიმქოინი" : ""));
                finished = true;
            }
            if (finished && Current != null) game?.HUD?.Notify("ახალი მისია: „" + Current.Title + "“ • " + Step.Text());
            if (finished) game?.HUD?.Refresh();
        }
    }
}
