using UnityEngine;

namespace ChemistryGame
{
    public static class PixelArt
    {
        private static Sprite closet, flask, goggles, hat, crown, globe, poster, flame;
        public static Sprite Flame() => flame ??= Make(14, 18, (x, y) =>
        {
            float span = y < 8 ? 5 : (18 - y) * 0.5f;
            if (Mathf.Abs(x - 6.5f) > span) return Color.clear;
            return y < 8 && Mathf.Abs(x - 6.5f) < 2 ? new Color32(255, 218, 96, 255) : new Color32(241, 129, 56, 255);
        });
        public static Sprite Closet() => closet ??= Make(32, 48, (x, y) =>
        {
            if (x < 1 || x > 30 || y < 1 || y > 46) return Color.clear;
            if (y < 4) return new Color32(35, 48, 72, 255);
            if (x < 3 || x > 28 || y > 44) return new Color32(45, 66, 95, 255);
            if (x == 15 || x == 16) return new Color32(53, 77, 105, 255);
            if ((x == 12 || x == 19) && y > 17 && y < 23) return new Color32(255, 209, 104, 255);
            if (y == 38 || y == 35 || y == 32)
                return x > 5 && x < 12 || x > 20 && x < 27 ? new Color32(74, 112, 145, 255) : new Color32(119, 173, 188, 255);
            if (y > 7 && y < 26 && x > 4 && x < 27)
                return new Color32(128, 190, 199, 255);
            return x < 15 ? new Color32(138, 203, 207, 255) : new Color32(105, 169, 187, 255);
        }, new Vector2(0.5f, 0));
        public static Sprite Flask() => flask ??= Make(16, 20, (x, y) =>
        {
            bool neck = y > 11 && y < 19 && x > 5 && x < 10;
            bool body = y > 1 && y <= 12 && Mathf.Abs(x - 7.5f) < (15 - y) * 0.5f;
            if (!neck && !body) return Color.clear;
            return y < 9 ? new Color32(68, 219, 172, 255) : new Color32(186, 227, 220, 230);
        });
        public static Sprite Goggles() => goggles ??= Make(20, 7, (x, y) =>
        {
            if (y < 1 || y > 5) return Color.clear;
            if (x == 0 || x == 19 || x == 9 || x == 10 || y == 1 || y == 5) return new Color32(37, 59, 66, 255);
            return new Color32(81, 208, 240, 220);
        });
        public static Sprite Hat(bool royal) => royal ? crown ??= Make(20, 12, (x, y) =>
        {
            bool peak = (x % 6 < 3 && y < 10) || y < 5;
            return x > 0 && x < 19 && peak ? new Color32(255, 201, 61, 255) : Color.clear;
        }) : hat ??= Make(24, 12, (x, y) =>
        {
            bool shape = y < 3 || x > 5 && x < 19 && y < 10;
            return shape ? new Color32(95, 64, 147, 255) : Color.clear;
        });
        public static Sprite Globe() => globe ??= Make(24, 26, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(11.5f, 15));
            if (d < 8) return x % 5 == y % 5 ? new Color32(245, 158, 255, 255) : new Color32(100, 63, 177, 230);
            return y < 6 && x > 5 && x < 19 ? new Color32(54, 70, 84, 255) : Color.clear;
        });
        public static Sprite Poster() => poster ??= Make(25, 19, (x, y) =>
            x % 4 < 3 && y % 4 < 3 ? Color.Lerp(new Color(0.2f, 0.55f, 0.46f), new Color(0.9f, 0.7f, 0.3f), x / 25f) : Color.clear);
        public static Sprite Make(int width, int height, System.Func<int, int, Color> draw, Vector2? pivot = null, float pixelsPerUnit = 16)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) texture.SetPixel(x, y, draw(x, y));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot ?? Vector2.one * 0.5f, pixelsPerUnit);
        }
    }
}



