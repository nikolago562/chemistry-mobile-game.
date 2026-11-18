using UnityEngine;

namespace ChemistryGame
{
    public enum ReactionCategory { Synthesis, Decomposition, Stoichiometry, Combustion, AcidBase, ElementDiscovery }
    [CreateAssetMenu(menuName = "Chemistry/Question")]
    public sealed class ChemistryQuestionSO : ScriptableObject
    {
        public string questionID;
        public string equationPrompt;
        public string correctAnswer;
        [TextArea(2, 5)] public string hintText;
        public ReactionCategory reactionCategory;
        [Range(1, 3)] public int difficultyTier = 1;
        [Min(1)] public int coinReward = 10;
        public string[] involvedElements;
        public bool isVisualExperiment;
        public string CategoryName => new[] { "შეერთება", "დაშლა", "სტექიომეტრია", "წვა", "მჟავა და ფუძე", "ელემენტის აღმოჩენა" }[(int)reactionCategory];
    }
}

