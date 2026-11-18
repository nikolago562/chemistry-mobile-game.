using UnityEngine;

namespace ChemistryGame
{
    public enum CosmeticCategory { SafetyGoggles, LabCoat, HairOrHat, PersonalDeskDecor }
    [CreateAssetMenu(menuName = "Chemistry/Cosmetic")]
    public sealed class CosmeticItemSO : ScriptableObject
    {
        public string itemID;
        public string displayName;
        [TextArea] public string description;
        public CosmeticCategory category;
        public int price;
        public Color color = Color.white;
        public bool weeklyExclusive;
    }
}
