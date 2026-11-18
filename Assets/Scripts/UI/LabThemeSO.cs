using UnityEngine;

namespace ChemistryGame
{
    [CreateAssetMenu(menuName = "Chemistry/Visual Theme")]
    public sealed class LabThemeSO : ScriptableObject
    {
        public Sprite panel;
        public Sprite button;
        public Sprite characterPortrait;
        public Sprite teacherPortrait;
        public Material teacherPortraitMaterial;
        public Sprite storageCupboard;
    }
}
