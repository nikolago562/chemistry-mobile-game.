using UnityEngine;

namespace ChemistryGame
{
    public static class PlayerAppearance
    {
        public static readonly Color[] Skin = { new Color32(255, 193, 121, 255), new Color32(220, 158, 105, 255),
            new Color32(164, 106, 73, 255), new Color32(106, 67, 53, 255) };
        public static readonly Color[] Hair = { new Color32(74, 49, 47, 255), new Color32(139, 78, 48, 255),
            new Color32(222, 180, 91, 255), new Color32(95, 65, 143, 255) };
        public static readonly Color[] Shirt = { new Color32(88, 120, 163, 255), new Color32(67, 157, 139, 255),
            new Color32(191, 97, 107, 255), new Color32(130, 95, 183, 255) };

        public static void Apply(Material material, int skin, int hair, int shirt)
        {
            material.SetColor("_SkinColor", Skin[Mathf.Clamp(skin, 0, Skin.Length - 1)]);
            material.SetColor("_HairColor", Hair[Mathf.Clamp(hair, 0, Hair.Length - 1)]);
            material.SetColor("_ShirtColor", Shirt[Mathf.Clamp(shirt, 0, Shirt.Length - 1)]);
        }
    }
}
