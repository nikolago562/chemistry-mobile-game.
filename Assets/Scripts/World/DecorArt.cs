using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>A tiny pixel canvas used to paint the new props in code.</summary>
    public sealed class Px
    {
        public readonly int W, H;
        private readonly Color32[] buffer;
        public Px(int w, int h) { W = w; H = h; buffer = new Color32[w * h]; }
        public void Dot(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < W && y < H) buffer[y * W + x] = c; }
        public void Fill(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Dot(x, y, c); }
        public void Box(int x0, int y0, int x1, int y1, Color32 fill, Color32 line)
        { Fill(x0, y0, x1, y1, line); Fill(x0 + 1, y0 + 1, x1 - 1, y1 - 1, fill); }
        public void Disc(float cx, float cy, float r, Color32 c)
        { for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r) Dot(x, y, c); }
        public Sprite Build(string name, Vector2 pivot, float ppu)
        {
            var texture = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(buffer); texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, W, H), pivot, ppu);
            sprite.name = name;
            return sprite;
        }
    }

    public static class DecorArt
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        public static readonly Color32 Line = new Color32(43, 34, 31, 255), WoodDark = new Color32(98, 64, 40, 255),
            Wood = new Color32(150, 104, 60, 255), WoodLight = new Color32(192, 142, 86, 255), Metal = new Color32(128, 138, 148, 255),
            MetalDark = new Color32(82, 90, 100, 255), Glass = new Color32(176, 216, 228, 200), Leaf = new Color32(76, 150, 88, 255),
            LeafDark = new Color32(48, 108, 66, 255), Pot = new Color32(184, 98, 70, 255), Paper = new Color32(244, 236, 212, 255),
            Navy = new Color32(33, 57, 48, 255), Mint = new Color32(105, 199, 164, 255), Coral = new Color32(224, 133, 102, 255),
            Gold = new Color32(239, 194, 102, 255), Violet = new Color32(140, 100, 200, 255);

        public static Sprite Paint(string key, int w, int h, Action<Px> draw, float ppu = 16, Vector2? pivot = null)
        {
            if (cache.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            var px = new Px(w, h); draw(px);
            sprite = px.Build(key, pivot ?? new Vector2(0.5f, 0), ppu);
            cache[key] = sprite;
            return sprite;
        }

        // ---------- simple effects ----------
        public static Sprite Soft() => Paint("soft", 32, 32, p =>
        {
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 16f;
                if (d < 1) p.Dot(x, y, new Color(1, 1, 1, Mathf.Pow(1 - d, 2) * 0.9f));
            }
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Disc() => Paint("disc", 32, 32, p => p.Disc(15.5f, 15.5f, 15.6f, Color.white), 16, new Vector2(0.5f, 0.5f));
        public static Sprite Square() => Paint("square", 1, 1, p => p.Dot(0, 0, Color.white), 16, new Vector2(0.5f, 0.5f));
        public static Sprite Bubble() => Paint("bubble", 3, 3, p => { p.Dot(1, 0, Color.white); p.Dot(0, 1, Color.white); p.Dot(2, 1, Color.white); p.Dot(1, 2, Color.white); }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Jar() => Paint("jar", 6, 8, p => { p.Box(0, 0, 5, 6, new Color32(255, 255, 255, 255), new Color32(200, 210, 220, 255)); p.Fill(1, 7, 4, 7, Metal); });
        public static Sprite Puddle(int kind) => Paint("puddle" + kind, 28, 12, p =>
        {
            Color32 c = kind == 0 ? new Color32(160, 210, 230, 200) : kind == 1 ? new Color32(250, 250, 250, 230) : new Color32(200, 130, 220, 200);
            p.Disc(9, 5, 5, c); p.Disc(17, 6, 5.5f, c); p.Disc(13, 4, 4, c); p.Disc(22, 4, 3, c);
            p.Dot(10, 7, new Color32(255, 255, 255, 230)); p.Dot(18, 8, new Color32(255, 255, 255, 230));
        }, 16, new Vector2(0.5f, 0.5f));

        // ---------- hallway, storage, reading ----------
        public static Sprite Noticeboard() => Paint("notice", 48, 30, p =>
        {
            p.Box(0, 0, 47, 29, new Color32(180, 132, 84, 255), WoodDark);
            p.Fill(4, 15, 14, 25, Paper); p.Fill(17, 13, 27, 26, new Color32(200, 230, 214, 255)); p.Fill(30, 16, 43, 25, new Color32(250, 220, 170, 255));
            p.Fill(5, 4, 18, 11, new Color32(220, 200, 240, 255)); p.Fill(22, 3, 33, 10, Paper); p.Fill(36, 4, 44, 12, new Color32(240, 190, 180, 255));
            foreach (var (x, y) in new[] { (9, 25), (22, 26), (36, 25), (11, 11), (27, 10), (40, 12) }) p.Dot(x, y, Coral);
            for (int x = 6; x < 13; x += 2) p.Dot(x, 20, MetalDark);
            for (int x = 19; x < 26; x += 2) p.Dot(x, 18, MetalDark);
        }, 16, new Vector2(0.5f, 0.5f));
        /// <summary>Wall chart: the 18-column table shape in group colours, with the f-block below.</summary>
        public static Sprite PeriodicChart() => Paint("periodic", 48, 32, p =>
        {
            p.Box(0, 0, 47, 31, Paper, WoodDark);
            Color32[] group = { Coral, Gold, Violet, Mint, new Color32(120, 170, 210, 255) };
            for (int row = 0; row < 7; row++)
            for (int col = 0; col < 18; col++)
            {
                bool cell = row == 0 ? col == 0 || col == 17 : row < 3 ? col < 2 || col > 11 : true;
                if (!cell) continue;
                var c = col < 2 ? group[0] : col < 12 ? group[4] : col < 17 ? group[3] : group[2];
                p.Fill(3 + col * 2 + col / 6, 27 - row * 3, 4 + col * 2 + col / 6, 28 - row * 3, c);
            }
            for (int col = 0; col < 15; col++) p.Fill(9 + col * 2, 3, 10 + col * 2, 4, group[1]);
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Lectern() => Paint("lectern", 16, 22, p =>
        {
            p.Fill(6, 0, 9, 12, WoodDark); p.Fill(4, 0, 11, 1, Line);
            p.Box(1, 12, 14, 18, Wood, Line);
            p.Fill(2, 16, 7, 20, Paper); p.Fill(8, 16, 13, 20, new Color32(232, 222, 196, 255)); p.Fill(7, 15, 8, 20, Line);
            for (int x = 3; x < 7; x++) p.Dot(x, 18, MetalDark);
            for (int x = 9; x < 13; x++) p.Dot(x, 18, MetalDark);
        });
        public static Sprite PlanStand() => Paint("planstand", 18, 28, p =>
        {
            p.Fill(3, 0, 4, 14, MetalDark); p.Fill(13, 0, 14, 14, MetalDark);
            p.Box(1, 12, 16, 27, Paper, WoodDark);
            p.Fill(3, 22, 8, 25, Mint); p.Fill(10, 22, 14, 25, Coral); p.Fill(3, 15, 8, 20, Gold); p.Fill(10, 15, 14, 20, Violet);
            p.Fill(7, 26, 10, 27, Metal);
        });
        public static Sprite TrophyCase() => Paint("trophycase", 28, 40, p =>
        {
            p.Box(0, 0, 27, 39, Glass, WoodDark); p.Fill(1, 0, 26, 5, Wood);
            for (int s = 0; s < 3; s++) p.Fill(1, 13 + s * 9, 26, 13 + s * 9, WoodLight);
            p.Fill(5, 14, 8, 18, Gold); p.Fill(6, 19, 7, 20, Gold); p.Disc(18, 17, 3, new Color32(200, 200, 210, 255));
            p.Fill(10, 23, 14, 27, new Color32(205, 127, 50, 255)); p.Fill(18, 23, 22, 29, Gold); p.Disc(7, 34, 2.5f, Coral);
        });
        public static Sprite Bench() => Paint("bench", 28, 12, p =>
        {
            p.Fill(1, 0, 2, 6, WoodDark); p.Fill(25, 0, 26, 6, WoodDark); p.Box(0, 5, 27, 8, Wood, WoodDark); p.Fill(1, 9, 26, 11, WoodLight);
        });
        public static Sprite Plant(int variant) => Paint("plant" + variant, 14, 24, p =>
        {
            p.Box(3, 0, 10, 6, Pot, new Color32(130, 64, 46, 255)); p.Fill(2, 6, 11, 7, new Color32(150, 76, 54, 255));
            if (variant == 0) { p.Disc(7, 13, 5, Leaf); p.Disc(4, 16, 3, LeafDark); p.Disc(10, 17, 3, Leaf); p.Disc(7, 20, 3, LeafDark); }
            else { for (int i = 0; i < 5; i++) { p.Fill(3 + i * 2, 8, 3 + i * 2, 12 + (i % 3) * 4, i % 2 == 0 ? Leaf : LeafDark); p.Dot(3 + i * 2, 13 + (i % 3) * 4, Leaf); } p.Fill(6, 8, 7, 22, LeafDark); }
        });
        public static Sprite VentGrille() => Paint("vent", 20, 14, p =>
        {
            p.Box(0, 0, 19, 13, new Color32(110, 120, 130, 255), MetalDark);
            p.Disc(9.5f, 6.5f, 5.5f, new Color32(50, 56, 64, 255));
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite FanBlade() => Paint("fanblade", 11, 11, p =>
        {
            for (int i = 0; i < 11; i++) { p.Dot(5, i, Metal); p.Dot(i, 5, Metal); }
            p.Fill(4, 0, 6, 2, Metal); p.Fill(4, 8, 6, 10, Metal); p.Fill(0, 4, 2, 6, Metal); p.Fill(8, 4, 10, 6, Metal); p.Dot(5, 5, MetalDark);
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Shelf(int variant) => Paint("shelf" + variant, 30, 36, p =>
        {
            p.Box(0, 0, 29, 35, new Color32(140, 146, 150, 255), MetalDark);
            for (int s = 0; s < 3; s++)
            {
                int y = 4 + s * 11; p.Fill(1, y, 28, y, MetalDark);
                for (int i = 0; i < 4; i++)
                {
                    Color32 c = new[] { Coral, Mint, Gold, Violet, Paper }[(i + s + variant) % 5];
                    if ((i + s + variant) % 3 == 0) p.Box(3 + i * 6, y + 1, 7 + i * 6, y + 7, c, Line);
                    else { p.Fill(4 + i * 6, y + 1, 7 + i * 6, y + 5, c); p.Fill(5 + i * 6, y + 6, 6 + i * 6, y + 7, Glass); }
                }
            }
        });
        public static Sprite Crates() => Paint("crates", 36, 26, p =>
        {
            p.Box(0, 0, 17, 13, WoodLight, WoodDark); p.Box(18, 0, 35, 13, Wood, WoodDark); p.Box(8, 13, 25, 25, WoodLight, WoodDark);
            for (int i = 1; i < 17; i += 4) p.Fill(i, 1, i, 12, Wood);
            p.Fill(13, 18, 20, 20, Paper);
        });
        public static Sprite Bookshelf(int variant) => Paint("books" + variant, 32, 38, p =>
        {
            p.Box(0, 0, 31, 37, WoodDark, Line);
            for (int s = 0; s < 3; s++)
            {
                int y = 2 + s * 12; p.Fill(1, y, 30, y + 1, Wood);
                int x = 2;
                int seed = variant * 7 + s * 3;
                while (x < 29)
                {
                    int width = 2 + (seed % 2); int height = 7 + (seed % 3);
                    Color32 c = new[] { Coral, Mint, Gold, Violet, new Color32(90, 120, 180, 255), Paper }[seed % 6];
                    p.Fill(x, y + 2, Math.Min(29, x + width - 1), y + 1 + height, c);
                    x += width + (seed % 4 == 0 ? 1 : 0); seed = (seed * 5 + 3) % 97;
                }
            }
        });
        public static Sprite Sofa() => Paint("sofa", 38, 22, p =>
        {
            Color32 cloth = new Color32(120, 96, 170, 255), clothDark = new Color32(88, 66, 130, 255);
            p.Box(0, 2, 37, 20, cloth, Line); p.Fill(1, 2, 36, 8, clothDark); p.Box(0, 2, 6, 14, cloth, Line); p.Box(31, 2, 37, 14, cloth, Line);
            p.Fill(7, 9, 18, 13, new Color32(140, 116, 190, 255)); p.Fill(19, 9, 30, 13, new Color32(140, 116, 190, 255));
            p.Fill(2, 0, 3, 1, WoodDark); p.Fill(34, 0, 35, 1, WoodDark);
        });
        public static Sprite Rug() => Paint("rug", 44, 18, p =>
        {
            p.Box(0, 0, 43, 17, new Color32(200, 110, 90, 255), new Color32(150, 74, 60, 255));
            p.Box(4, 3, 39, 14, new Color32(230, 190, 120, 255), new Color32(150, 74, 60, 255));
            for (int x = 8; x < 36; x += 6) p.Fill(x, 8, x + 2, 9, new Color32(110, 160, 140, 255));
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Lamp() => Paint("lamp", 12, 30, p =>
        {
            p.Fill(3, 0, 8, 1, MetalDark); p.Fill(5, 2, 6, 21, MetalDark);
            p.Box(1, 21, 10, 29, new Color32(250, 220, 150, 255), new Color32(170, 120, 70, 255));
        });

        // ---------- greenhouse ----------
        public static Sprite PlantBed(int crop) => Paint("bed" + crop, 42, 26, p =>
        {
            p.Box(0, 0, 41, 9, new Color32(110, 76, 48, 255), WoodDark); p.Fill(2, 7, 39, 9, new Color32(84, 58, 40, 255));
            for (int i = 0; i < 5; i++)
            {
                int cx = 5 + i * 8;
                if (crop == 0) { p.Disc(cx, 13, 3.6f, new Color32(140, 80, 165, 255)); p.Disc(cx, 13, 2, new Color32(170, 110, 190, 255)); }
                else if (crop == 1) { p.Fill(cx, 9, cx, 20, LeafDark); p.Disc(cx, 19, 3.5f, Leaf); p.Disc(cx + 1, 15, 1.6f, new Color32(240, 214, 80, 255)); }
                else if (crop == 2) { p.Fill(cx, 9, cx, 16, LeafDark); p.Disc(cx, 17, 2.4f, new Color32(236, 140, 170, 255)); p.Dot(cx, 17, Gold); }
                else { p.Fill(cx, 9, cx, 21, LeafDark); p.Disc(cx, 20, 3.5f, Leaf); p.Disc(cx - 1, 16, 1.6f, new Color32(240, 150, 60, 255)); }
            }
        });
        public static Sprite RainBarrel() => Paint("barrel", 16, 20, p =>
        {
            p.Box(0, 0, 15, 17, new Color32(90, 120, 150, 255), new Color32(50, 70, 90, 255));
            p.Fill(1, 5, 14, 5, new Color32(60, 80, 100, 255)); p.Fill(1, 12, 14, 12, new Color32(60, 80, 100, 255));
            p.Fill(2, 18, 13, 19, new Color32(60, 80, 100, 255));
        });

        // ---------- exhibition ----------
        public static Sprite Pedestal(int variant) => Paint("pedestal" + variant, 20, 30, p =>
        {
            p.Box(2, 0, 17, 15, Paper, new Color32(150, 140, 160, 255));
            p.Box(3, 16, 16, 28, Glass, new Color32(150, 160, 170, 255));
            Color32 c = new[] { Violet, Mint, Coral, Gold }[variant % 4];
            if (variant % 2 == 0) { p.Fill(8, 17, 11, 22, c); p.Fill(9, 23, 10, 25, new Color32(220, 230, 240, 255)); }
            else { p.Disc(9.5f, 20, 3, c); p.Dot(9, 21, Color.white); }
        });
        public static Sprite FairBooth() => Paint("booth", 52, 34, p =>
        {
            p.Box(0, 0, 51, 15, Wood, WoodDark); p.Fill(1, 12, 50, 15, WoodLight);
            p.Fill(2, 16, 3, 33, WoodDark); p.Fill(48, 16, 49, 33, WoodDark);
            for (int x = 2; x < 50; x++) p.Dot(x, 30 + ((x / 4) % 2), (x / 4) % 2 == 0 ? Coral : Paper);
            p.Fill(4, 26, 47, 29, Coral);
            p.Box(14, 4, 24, 11, Paper, Line); p.Box(28, 4, 38, 11, new Color32(200, 230, 214, 255), Line);
        });
        public static Sprite Banner() => Paint("banner", 56, 18, p =>
        {
            p.Box(0, 2, 55, 17, new Color32(120, 90, 180, 255), new Color32(70, 50, 120, 255));
            for (int x = 1; x < 55; x += 6) { p.Dot(x + 2, 1, new Color32(120, 90, 180, 255)); p.Dot(x + 2, 0, new Color32(120, 90, 180, 255)); }
            p.Disc(9, 10, 4, Gold); p.Dot(9, 10, Coral);
            for (int x = 17; x < 50; x += 3) p.Fill(x, 9, x + 1, 11, Paper);
        }, 16, new Vector2(0.5f, 0.5f));

        // ---------- seasons ----------
        public static Sprite SeasonPiece(Season season, int index) => Paint("season" + (int)season + "_" + (index % 3), 10, 12, p =>
        {
            for (int x = 0; x < 10; x++) p.Dot(x, 11, new Color32(90, 70, 60, 255));
            switch (season)
            {
                case Season.Winter:
                    p.Fill(4, 4, 5, 10, Color.white); p.Fill(2, 6, 7, 7, Color.white); p.Dot(3, 9, Color.white); p.Dot(6, 9, Color.white); p.Dot(3, 4, Color.white); p.Dot(6, 4, Color.white);
                    break;
                case Season.Spring:
                    p.Disc(4.5f, 6, 3, index % 3 == 0 ? new Color32(240, 160, 190, 255) : index % 3 == 1 ? new Color32(250, 230, 120, 255) : new Color32(180, 160, 240, 255)); p.Dot(4, 6, Gold); p.Fill(4, 9, 5, 10, Leaf);
                    break;
                case Season.Summer:
                    for (int y = 3; y < 11; y++) for (int x = 0; x < 10; x++) if (Mathf.Abs(x - 4.5f) < (y - 2) * 0.6f) p.Dot(x, y, new[] { Coral, Gold, Mint }[index % 3]);
                    break;
                default:
                    p.Disc(4.5f, 6, 3, index % 3 == 0 ? new Color32(220, 120, 50, 255) : index % 3 == 1 ? new Color32(200, 70, 50, 255) : new Color32(230, 180, 60, 255)); p.Fill(4, 9, 4, 10, WoodDark);
                    break;
            }
        });

        // ---------- classroom ----------
        public static Sprite CollectionCabinet() => Paint("collectioncab", 30, 48, p =>
        {
            p.Box(0, 0, 29, 47, Glass, WoodDark); p.Fill(1, 0, 28, 8, Wood); p.Box(1, 1, 28, 8, Wood, WoodDark);
            for (int s = 0; s < 3; s++) p.Fill(1, 14 + s * 10, 28, 14 + s * 10, WoodLight);
            p.Fill(13, 4, 16, 5, Gold); p.Fill(0, 44, 29, 47, WoodDark);
        });

        // ---------- personal lab decorations ----------
        public static Sprite Decor(string id)
        {
            switch (id)
            {
                case "display": return Paint("d_display", 30, 40, p => { p.Box(0, 0, 29, 39, Glass, WoodDark); p.Fill(1, 0, 28, 5, Wood); for (int s = 0; s < 3; s++) p.Fill(1, 12 + s * 9, 28, 12 + s * 9, WoodLight); });
                case "research": return Paint("d_research", 34, 34, p => { p.Fill(3, 0, 4, 12, WoodDark); p.Fill(29, 0, 30, 12, WoodDark); p.Box(0, 10, 33, 33, new Color32(60, 90, 80, 255), WoodDark); p.Fill(2, 12, 31, 31, new Color32(48, 78, 66, 255)); for (int x = 5; x < 29; x += 5) p.Fill(x, 26, x + 2, 26, Paper); });
                case "samples": return Paint("d_samples", 30, 34, p => { p.Box(0, 0, 29, 33, WoodLight, WoodDark); for (int s = 0; s < 3; s++) p.Fill(1, 3 + s * 10, 28, 3 + s * 10, WoodDark); });
                case "trophy": return TrophyCase();
                case "photos": return Paint("d_photos", 32, 30, p => { p.Fill(3, 0, 4, 10, WoodDark); p.Fill(27, 0, 28, 10, WoodDark); p.Box(0, 8, 31, 29, new Color32(190, 145, 95, 255), WoodDark); });
                case "storage": return Paint("d_storage", 30, 30, p => { p.Box(0, 0, 29, 13, Wood, WoodDark); p.Box(3, 13, 26, 25, WoodLight, WoodDark); p.Fill(11, 18, 18, 20, Paper); p.Fill(6, 4, 23, 6, WoodDark); });
                case "radio": return Paint("d_radio", 26, 26, p => { p.Fill(4, 0, 5, 9, WoodDark); p.Fill(20, 0, 21, 9, WoodDark); p.Box(0, 8, 25, 23, new Color32(200, 120, 90, 255), Line); p.Disc(7, 15, 4.5f, new Color32(60, 50, 50, 255)); for (int y = 11; y < 20; y += 2) p.Fill(14, y, 22, y, new Color32(240, 220, 180, 255)); p.Fill(18, 23, 18, 25, MetalDark); });
                case "plant": return Plant(0);
                case "ficus": return Paint("d_ficus", 22, 40, p => { p.Box(6, 0, 15, 8, Pot, new Color32(130, 64, 46, 255)); p.Fill(10, 9, 11, 26, WoodDark); p.Disc(11, 30, 8, Leaf); p.Disc(6, 26, 5, LeafDark); p.Disc(16, 27, 5, Leaf); p.Disc(11, 35, 4, LeafDark); });
                case "aquarium": return Paint("d_aquarium", 40, 34, p => { p.Fill(3, 0, 5, 12, WoodDark); p.Fill(34, 0, 36, 12, WoodDark); p.Box(0, 10, 39, 13, Wood, WoodDark); p.Box(0, 14, 39, 33, new Color32(90, 170, 210, 200), new Color32(60, 80, 100, 255)); p.Fill(1, 15, 38, 17, new Color32(220, 200, 150, 255)); p.Fill(6, 18, 7, 24, Leaf); p.Fill(30, 18, 31, 27, LeafDark); p.Fill(1, 32, 38, 32, new Color32(170, 220, 240, 255)); });
                case "plasma": return Paint("d_plasma", 24, 30, p => { p.Box(5, 0, 18, 7, new Color32(40, 40, 50, 255), Line); p.Disc(11.5f, 17, 10, new Color32(60, 40, 90, 200)); p.Disc(11.5f, 17, 2, new Color32(240, 220, 255, 255)); });
                case "poster_table": return Paint("d_ptable", 28, 34, p => { p.Fill(13, 0, 14, 12, WoodDark); p.Box(0, 10, 27, 33, Paper, Line); for (int y = 13; y < 30; y += 3) for (int x = 2; x < 26; x += 3) if (!(y > 22 && x > 4 && x < 22)) p.Fill(x, y, x + 1, y + 1, new[] { Mint, Coral, Gold, Violet }[(x + y) % 4]); });
                case "poster_safety": return Paint("d_psafety", 28, 34, p => { p.Fill(13, 0, 14, 12, WoodDark); p.Box(0, 10, 27, 33, new Color32(250, 230, 140, 255), Line); for (int y = 14; y < 30; y++) for (int x = 4; x < 24; x++) if (Mathf.Abs(x - 13.5f) < (30 - y) * 0.62f && y > 15) p.Dot(x, y, Coral); p.Fill(13, 19, 14, 25, Line); p.Fill(13, 17, 14, 17, Line); });
                case "sofa": return Sofa();
                case "lamp": return Lamp();
                case "rug": return Paint("d_rug", 44, 18, p => { p.Box(0, 0, 43, 17, new Color32(110, 160, 140, 255), new Color32(60, 110, 90, 255)); p.Box(5, 3, 38, 14, new Color32(240, 220, 170, 255), new Color32(60, 110, 90, 255)); for (int x = 9; x < 35; x += 6) p.Fill(x, 8, x + 2, 9, Coral); });
                case "season": return Paint("d_season" + (int)SchoolCalendar.Season, 24, 34, p => SeasonalDecor(p));
                case "crystal_lamp": return Paint("d_crystal", 20, 30, p => { p.Box(4, 0, 15, 6, WoodDark, Line); for (int y = 7; y < 29; y++) for (int x = 0; x < 20; x++) if (Mathf.Abs(x - 9.5f) < Mathf.Min(y - 6, 30 - y) * 0.45f) p.Dot(x, y, (x + y) % 5 == 0 ? new Color32(250, 250, 255, 255) : new Color32(180, 205, 245, 230)); });
                case "glow_jar": return Paint("d_glowjar", 16, 22, p => { p.Box(2, 0, 13, 17, new Color32(120, 220, 240, 230), new Color32(80, 140, 170, 255)); p.Fill(3, 18, 12, 20, WoodLight); p.Dot(6, 8, Color.white); p.Dot(9, 12, Color.white); });
                case "mineral_case": return Paint("d_mineral", 34, 24, p => { p.Fill(3, 0, 4, 8, WoodDark); p.Fill(29, 0, 30, 8, WoodDark); p.Box(0, 7, 33, 23, Glass, WoodDark); Color32[] c = { new Color32(232, 232, 240, 255), Gold, new Color32(150, 110, 210, 255), new Color32(70, 170, 110, 255), Coral, Paper }; for (int i = 0; i < 6; i++) p.Disc(5 + (i % 3) * 11, 12 + (i / 3) * 6, 2.2f, c[i]); });
                case "fair_trophy": return Paint("d_ftrophy", 18, 30, p => { p.Box(3, 0, 14, 6, WoodDark, Line); p.Fill(7, 7, 10, 13, Gold); p.Box(2, 14, 15, 26, Gold, new Color32(180, 130, 40, 255)); p.Fill(0, 20, 2, 24, Gold); p.Fill(15, 20, 17, 24, Gold); p.Dot(8, 21, Color.white); });
                default: return null;
            }
        }

        private static void SeasonalDecor(Px p)
        {
            switch (SchoolCalendar.Season)
            {
                case Season.Winter:
                    p.Box(8, 0, 15, 5, Pot, Line);
                    for (int y = 6; y < 33; y++) for (int x = 0; x < 24; x++) if (Mathf.Abs(x - 11.5f) < (33 - y) * 0.38f) p.Dot(x, y, (x + y) % 7 == 0 ? Gold : (x * 3 + y) % 11 == 0 ? Coral : LeafDark);
                    p.Dot(11, 33, Gold); break;
                case Season.Spring:
                    p.Box(6, 0, 17, 7, new Color32(140, 170, 220, 255), Line);
                    for (int i = 0; i < 5; i++) { p.Fill(7 + i * 2, 8, 7 + i * 2, 18 + i % 3 * 4, Leaf); p.Disc(7 + i * 2, 20 + i % 3 * 4, 2.2f, new[] { new Color32(240, 160, 190, 255), Gold, new Color32(180, 160, 240, 255) }[i % 3]); }
                    break;
                case Season.Summer:
                    p.Box(4, 0, 19, 10, new Color32(240, 220, 160, 255), Line); p.Disc(11.5f, 20, 7, Gold); p.Disc(11.5f, 20, 4, new Color32(255, 230, 140, 255));
                    break;
                default:
                    p.Disc(11.5f, 8, 8, new Color32(230, 130, 50, 255)); p.Fill(11, 15, 12, 18, LeafDark); p.Disc(6, 20, 2.5f, new Color32(200, 70, 50, 255)); p.Disc(17, 22, 2.5f, Gold);
                    break;
            }
        }

        public static Sprite Fish(int variant) => Paint("fish" + variant, 6, 4, p =>
        {
            Color32 c = variant == 0 ? new Color32(250, 150, 60, 255) : variant == 1 ? new Color32(250, 230, 90, 255) : new Color32(120, 200, 250, 255);
            p.Fill(1, 1, 3, 2, c); p.Dot(0, 0, c); p.Dot(0, 3, c); p.Dot(4, 1, c); p.Dot(4, 2, c); p.Dot(3, 2, Line);
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Medal(Color32 c) => Paint("medal" + c.r + c.g, 5, 7, p => { p.Fill(1, 4, 3, 6, Coral); p.Disc(2, 2, 2, c); });
        public static Sprite Photo(Color32 c) => Paint("photo" + c.r + c.g + c.b, 7, 6, p => { p.Box(0, 0, 6, 5, c, Paper); p.Dot(2, 3, Color.white); });
        public static Sprite Note(Color32 c) => Paint("note" + c.r + c.g, 6, 6, p => { p.Fill(0, 0, 5, 5, c); p.Dot(2, 5, Coral); p.Fill(1, 2, 4, 2, MetalDark); });

        // ---------- windows & wall effects ----------
        public static Sprite WindowFrame(int w, int h) => Paint("window" + w + "x" + h, w, h, p =>
        {
            p.Box(0, 0, w - 1, h - 1, new Color32(0, 0, 0, 0), WoodDark);
            p.Fill(1, 0, w - 2, 1, Wood); p.Fill(w / 2, 1, w / 2, h - 2, WoodDark); p.Fill(1, h / 2, w - 2, h / 2, WoodDark);
            p.Fill(0, 0, w - 1, 0, WoodLight);
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite ClockHand(int length) => Paint("hand" + length, 1, length, p => p.Fill(0, 0, 0, length - 1, Line), 32, new Vector2(0.5f, 0));
        public static Sprite Light() => Paint("safetylight", 4, 4, p => { p.Fill(0, 0, 3, 3, MetalDark); p.Fill(1, 1, 2, 2, Color.white); }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite Flask() => Paint("bgflask", 8, 10, p =>
        {
            for (int y = 0; y < 6; y++) for (int x = 0; x < 8; x++) if (Mathf.Abs(x - 3.5f) < (7 - y) * 0.6f) p.Dot(x, y, Color.white);
            p.Fill(3, 6, 4, 9, new Color32(210, 230, 240, 255));
        });
    }
}
