using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    public enum GlassShape { Erlenmeyer, Round, Tube }

    /// <summary>
    /// Pastel pixel glassware: a black outline, a white rim just inside the glass, a pale tint of the
    /// liquid above the fill line and the liquid itself shaded darker toward the bottom.
    /// </summary>
    public static class Glassware
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        private static readonly Color32 Outline = new Color32(24, 20, 28, 255);

        /// <summary>A shape for a jar id, so the shelf mixes flasks, round flasks and tubes.</summary>
        public static GlassShape ShapeFor(string id) => (GlassShape)(Math.Abs(id.GetHashCode()) % 3);

        public static Sprite Get(GlassShape shape, Color liquid, float fill = 0.45f, float ppu = 16)
        {
            string key = shape + ":" + ColorUtility.ToHtmlStringRGB(liquid) + ":" + fill + ":" + ppu;
            if (cache.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            int w = shape == GlassShape.Tube ? 10 : 22, h = 30;
            Func<int, int, bool> inside = shape switch
            {
                GlassShape.Erlenmeyer => (x, y) => Neck(x, y, w) || y >= 1 && y <= 19 && Mathf.Abs(x - (w - 1) / 2f) <= 2 + (19 - y) * 0.45f,
                GlassShape.Round => (x, y) => Neck(x, y, w) || Vector2.Distance(new Vector2(x, y), new Vector2((w - 1) / 2f, 9)) <= 8.6f,
                _ => (x, y) => x >= 2 && x <= w - 3 && y >= 2 && y <= 27 || y == 1 && x >= 3 && x <= w - 4 || y >= 26 && y <= 28 && x >= 1 && x <= w - 2,
            };
            var tint = Color.Lerp(liquid, Color.white, 0.75f); tint.a = 0.9f;
            var dark = Color.Lerp(liquid, Color.black, 0.18f); dark.a = 1;
            var light = Color.Lerp(liquid, Color.white, 0.35f); light.a = 1;
            var body = liquid; body.a = 1;
            // The fill line is measured on the body, below the neck.
            int level = Mathf.RoundToInt(1 + (shape == GlassShape.Tube ? 24 : 17) * fill);
            var px = new Px(w, h);
            bool In(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && inside(x, y);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!In(x, y))
                {
                    if (In(x - 1, y) || In(x + 1, y) || In(x, y - 1) || In(x, y + 1)) px.Dot(x, y, Outline);
                    continue;
                }
                bool edge = !In(x - 1, y) || !In(x + 1, y) || !In(x, y + 1) || !In(x, y - 1);
                bool lip = y >= h - 4;
                Color c = edge || lip ? new Color(1, 1, 1, 1) : y > level ? tint : y == level ? light : y < 4 ? dark : body;
                // A glint down the left side of the glass.
                if (!edge && !lip && In(x - 2, y) && !In(x - 3, y) && y > 4 && y < level) c = light;
                px.Dot(x, y, c);
            }
            sprite = px.Build("glass:" + key, new Vector2(0.5f, 0), ppu);
            cache[key] = sprite;
            return sprite;
        }

        private static bool Neck(int x, int y, int w)
        {
            float mid = (w - 1) / 2f;
            return y >= 16 && y <= 25 && Mathf.Abs(x - mid) <= 2 || y >= 26 && y <= 28 && Mathf.Abs(x - mid) <= 3.5f;
        }
    }
}
