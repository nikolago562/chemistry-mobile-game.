using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// Each school room's own identity, painted in code at 16 px per unit in the academy palette:
    /// hallway (cream and sage checker stone, oak and parchment walls), storage (worn concrete, brick,
    /// metal and crates, a little darker), reading corner (basket-weave parquet, green damask, lamps),
    /// greenhouse (mossy flagstones, glass wall, plants and sun) and exhibition hall (lilac marble,
    /// plum walls, gold trim and spotlit cases). Floors and walls are one tiled sprite each.
    /// </summary>
    public static class RoomArt
    {
        private static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        private static readonly Color32 Ink = AcademyArt.Ink, Brass = AcademyArt.Brass, Cream = AcademyArt.Cream;
        private static readonly Dictionary<string, Sprite> tiled = new Dictionary<string, Sprite>();

        /// <summary>A sprite that repeats cleanly in SpriteDrawMode.Tiled (full-rect mesh).</summary>
        private static Sprite Tiled(string key, int w, int h, System.Action<Px> draw, Vector2 pivot)
        {
            if (tiled.TryGetValue(key, out var s) && s != null) return s;
            var px = new Px(w, h); draw(px);
            return tiled[key] = px.Build("room:" + key, pivot, 16, SpriteMeshType.FullRect);
        }
        private static Sprite Paint(string key, int w, int h, System.Action<Px> draw, Vector2? pivot = null) => DecorArt.Paint("room:" + key, w, h, draw, 16, pivot);
        private static int Hash(int x, int y) { unchecked { int h = x * 374761393 + y * 668265263; h = (h ^ (h >> 13)) * 1274126177; return (h ^ (h >> 16)) & 0x7fffffff; } }
        private static Color32 Shade(Color32 c, int by) => C((byte)Mathf.Clamp(c.r + by, 0, 255), (byte)Mathf.Clamp(c.g + by, 0, 255), (byte)Mathf.Clamp(c.b + by, 0, 255), c.a);

        // ---------- floors ----------

        public static Sprite Floor(string room) => room switch
        {
            "hallway" => Tiled("floor-hall", 32, 32, p =>
            {
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    bool light = (x / 16 + y / 16) % 2 == 0;
                    Color32 c = light ? C(234, 222, 196) : C(176, 186, 160);
                    int lx = x % 16, ly = y % 16;
                    if (lx == 0 || ly == 0) c = C(150, 136, 112);
                    else if (lx == 1 || ly == 15) c = Shade(c, 10);
                    else if (lx == 15 || ly == 1) c = Shade(c, -12);
                    else if (Hash(x, y) % 23 == 0) c = Shade(c, -10);
                    p.Dot(x, y, c);
                }
            }, new Vector2(0.5f, 0.5f)),
            "storage" => Tiled("floor-storage", 32, 32, p =>
            {
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    Color32 c = C(150, 142, 128);
                    int n = Hash(x / 3, y / 3) % 7;
                    c = Shade(c, n - 3);
                    if (x % 32 == 0 || y % 16 == 0) c = C(116, 108, 98);
                    else if (Hash(x, y) % 31 == 0) c = Shade(c, -14);
                    p.Dot(x, y, c);
                }
                foreach (var (x, y) in new[] { (7, 22), (8, 23), (9, 23), (10, 24), (22, 6), (23, 7), (23, 8) }) p.Dot(x, y, C(122, 114, 102));
                p.Disc(24, 23, 2.5f, C(138, 130, 116));
            }, new Vector2(0.5f, 0.5f)),
            "reading" => Tiled("floor-reading", 32, 32, p =>
            {
                Color32[] tones = { C(158, 100, 62), C(172, 112, 70), C(148, 94, 58), C(166, 108, 66) };
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    int bx = x / 16, by = y / 16, lx = x % 16, ly = y % 16;
                    bool across = (bx + by) % 2 == 0;
                    int along = across ? ly : lx, plank = along / 8;
                    var tone = tones[(bx * 2 + by + plank) % 4];
                    Color32 c = tone;
                    if (along % 8 == 0) c = C(104, 64, 40);
                    else if (along % 8 == 7) c = Shade(tone, 12);
                    else if (Hash(x, y) % 17 == 0) c = Shade(tone, -10);
                    if (lx == 0 || ly == 0) c = C(96, 58, 36);
                    p.Dot(x, y, c);
                }
            }, new Vector2(0.5f, 0.5f)),
            "greenhouse" => Tiled("floor-green", 48, 48, p =>
            {
                Color32 soil = C(96, 84, 62), moss = C(110, 140, 84);
                p.Fill(0, 0, 47, 47, soil);
                // irregular flagstones laid on a rough grid
                var stones = new[] { (1, 1, 20, 14), (23, 1, 46, 10), (1, 17, 13, 32), (16, 17, 34, 28), (37, 13, 46, 30), (1, 35, 22, 46), (25, 31, 46, 46), (16, 31, 22, 33) };
                Color32[] tone = { C(160, 164, 144), C(150, 156, 136), C(168, 170, 150), C(144, 152, 134) };
                for (int i = 0; i < stones.Length; i++)
                {
                    var (x0, y0, x1, y1) = stones[i];
                    p.Fill(x0, y0, x1, y1, tone[i % 4]);
                    p.Fill(x0, y1, x1, y1, Shade(tone[i % 4], 12)); p.Fill(x0, y0, x1, y0, Shade(tone[i % 4], -16));
                    for (int k = 0; k < 4; k++) p.Dot(x0 + 2 + Hash(i, k) % Mathf.Max(1, x1 - x0 - 3), y0 + 2 + Hash(k, i) % Mathf.Max(1, y1 - y0 - 3), Shade(tone[i % 4], -10));
                }
                for (int y = 0; y < 48; y++) for (int x = 0; x < 48; x++)
                    if (Hash(x, y) % 5 == 0 && IsSoil(stones, x, y)) p.Dot(x, y, moss);
            }, new Vector2(0.5f, 0.5f)),
            _ => Tiled("floor-exhibit", 32, 32, p =>
            {
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    Color32 c = C(204, 196, 214);
                    float vein = Mathf.Sin((x + y * 0.6f) * 0.35f + Mathf.Sin(y * 0.3f) * 2);
                    if (vein > 0.93f) c = C(180, 170, 194);
                    if (Hash(x, y) % 19 == 0) c = Shade(c, 8);
                    int lx = x % 16, ly = y % 16;
                    if (lx == 0 || ly == 0) c = C(150, 138, 164);
                    p.Dot(x, y, c);
                }
                foreach (int cx in new[] { 0, 16, 32 }) foreach (int cy in new[] { 0, 16, 32 })
                    for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++)
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) <= 2) p.Dot(cx + dx, cy + dy, Mathf.Abs(dx) + Mathf.Abs(dy) == 0 ? Brass : C(96, 70, 112));
            }, new Vector2(0.5f, 0.5f)),
        };
        private static bool IsSoil((int, int, int, int)[] stones, int x, int y)
        {
            foreach (var (x0, y0, x1, y1) in stones) if (x >= x0 && x <= x1 && y >= y0 && y <= y1) return false;
            return true;
        }

        // ---------- walls (32 x 64 px, 2 x 4 units, tiled sideways) ----------

        public static Sprite Wall(string room) => room switch
        {
            "hallway" => Tiled("wall-hall", 32, 64, p =>
            {
                Wainscot(p, C(176, 124, 78), C(140, 92, 56), C(204, 152, 98));
                Paper(p, C(238, 226, 200), C(228, 214, 186), 8);
                Crown(p, C(176, 124, 78), C(204, 152, 98));
            }, new Vector2(0.5f, 0)),
            "storage" => Tiled("wall-storage", 32, 64, p =>
            {
                p.Fill(0, 0, 31, 0, Ink); p.Fill(0, 1, 31, 20, C(112, 132, 120)); p.Fill(0, 20, 31, 21, C(90, 106, 96));
                p.Fill(0, 1, 31, 2, C(80, 92, 86));
                for (int y = 22; y < 62; y++) for (int x = 0; x < 32; x++)
                {
                    int row = (y - 22) / 5, off = row % 2 * 4, lx = (x + off) % 8, ly = (y - 22) % 5;
                    Color32 brick = Shade(C(150, 100, 82), Hash((x + off) / 8, row) % 13 - 6);
                    p.Dot(x, y, lx == 0 || ly == 0 ? C(170, 156, 136) : ly == 4 ? Shade(brick, 8) : brick);
                }
                p.Fill(0, 62, 31, 63, C(96, 90, 84));
            }, new Vector2(0.5f, 0)),
            "reading" => Tiled("wall-reading", 32, 64, p =>
            {
                Wainscot(p, AcademyArt.Walnut, AcademyArt.WalnutDark, AcademyArt.WalnutLight);
                p.Fill(0, 24, 31, 57, C(64, 96, 72));
                for (int y = 28; y < 56; y += 10) for (int x = (y / 10) % 2 * 8 + 4; x < 32; x += 16)
                    foreach (var (dx, dy) in new[] { (0, 0), (-1, -1), (1, -1), (0, -2), (-2, -2), (2, -2), (0, -3), (-1, -4), (1, -4), (0, -5) }) p.Dot(x + dx, y + dy, C(82, 118, 88));
                p.Fill(0, 24, 31, 24, C(50, 78, 58)); p.Fill(0, 55, 31, 55, Brass);
                Crown(p, AcademyArt.Walnut, AcademyArt.WalnutLight);
            }, new Vector2(0.5f, 0)),
            "greenhouse" => Tiled("wall-green", 32, 64, p =>
            {
                p.Fill(0, 0, 31, 0, Ink);
                for (int y = 1; y < 14; y++) for (int x = 0; x < 32; x++)
                {
                    int off = (y - 1) / 4 % 2 * 4;
                    p.Dot(x, y, (x + off) % 8 == 0 || (y - 1) % 4 == 0 ? C(176, 140, 110) : Shade(C(186, 104, 76), Hash((x + off) / 8, (y - 1) / 4) % 11 - 5));
                }
                p.Fill(0, 14, 31, 15, C(230, 236, 226));
                for (int y = 16; y < 64; y++) for (int x = 0; x < 32; x++)
                {
                    bool frame = x % 16 == 0 || x % 16 == 15 || y == 16 || y == 40 || y == 63;
                    float shine = (x + y) % 23 < 3 ? 14 : 0;
                    p.Dot(x, y, frame ? C(230, 236, 226) : Shade(C(186, 220, 214), (int)shine + (y - 16) / 6));
                }
                for (int y = 17; y < 63; y += 6) { p.Dot(0, y, C(170, 180, 170)); p.Dot(16, y, C(170, 180, 170)); }
            }, new Vector2(0.5f, 0)),
            _ => Tiled("wall-exhibit", 32, 64, p =>
            {
                Wainscot(p, C(78, 48, 40), C(56, 34, 30), C(108, 70, 56));
                p.Fill(0, 24, 31, 57, C(92, 64, 110));
                for (int x = 0; x < 32; x += 8) p.Fill(x, 24, x, 57, C(104, 74, 122));
                p.Fill(0, 23, 31, 23, Brass); p.Fill(0, 57, 31, 57, Brass);
                Crown(p, C(78, 48, 40), C(118, 82, 64));
                p.Fill(0, 61, 31, 61, Brass);
            }, new Vector2(0.5f, 0)),
        };
        private static void Wainscot(Px p, Color32 wood, Color32 dark, Color32 light)
        {
            p.Fill(0, 0, 31, 0, Ink); p.Fill(0, 1, 31, 2, dark); p.Fill(0, 3, 31, 3, light);
            p.Fill(0, 4, 31, 19, wood);
            foreach (int x0 in new[] { 2, 18 }) { p.Box(x0, 6, x0 + 11, 17, Shade(wood, 10), dark); p.Fill(x0 + 1, 16, x0 + 10, 16, light); }
            p.Fill(0, 20, 31, 20, dark); p.Fill(0, 21, 31, 22, Shade(light, -20)); p.Fill(0, 23, 31, 23, light);
        }
        private static void Paper(Px p, Color32 baseColor, Color32 stripe, int period)
        {
            p.Fill(0, 24, 31, 57, baseColor);
            for (int x = 0; x < 32; x += period) p.Fill(x, 24, x, 57, stripe);
            p.Fill(0, 24, 31, 24, Shade(baseColor, -18)); p.Fill(0, 56, 31, 57, Shade(baseColor, -14));
        }
        private static void Crown(Px p, Color32 wood, Color32 light)
        {
            p.Fill(0, 58, 31, 58, Shade(wood, -30)); p.Fill(0, 59, 31, 61, wood); p.Fill(0, 60, 31, 60, light); p.Fill(0, 62, 31, 63, Shade(wood, 16));
        }

        // ---------- props ----------

        public static Sprite Sconce => Paint("sconce", 8, 12, p =>
        {
            p.Fill(2, 0, 5, 2, Ink); p.Fill(3, 1, 4, 1, Brass); p.Fill(3, 3, 4, 5, Brass);
            p.Fill(1, 6, 6, 11, Ink); p.Fill(2, 7, 5, 10, C(255, 226, 160)); p.Fill(2, 10, 5, 10, C(255, 244, 214));
        }, new Vector2(0.5f, 0.5f));

        public static Sprite Barrel => Paint("barrel", 16, 20, p =>
        {
            for (int y = 0; y < 20; y++) { int bulge = y > 3 && y < 16 ? 1 : 0; p.Fill(1 - bulge, y, 14 + bulge, y, Ink); }
            for (int y = 1; y < 19; y++) { int bulge = y > 3 && y < 16 ? 1 : 0; for (int x = 2 - bulge; x <= 13 + bulge; x++) p.Dot(x, y, x % 4 == 0 ? AcademyArt.WalnutDark : AcademyArt.Walnut); }
            p.Fill(1, 4, 14, 5, C(110, 116, 124)); p.Fill(1, 14, 14, 15, C(110, 116, 124));
            p.Fill(3, 18, 12, 19, AcademyArt.WalnutLight);
        });

        public static Sprite Sacks => Paint("sacks", 24, 16, p =>
        {
            foreach (var (cx, cy, r) in new[] { (7f, 6f, 6f), (17f, 6f, 6f), (12f, 11f, 5f) })
            { p.Disc(cx, cy, r, Ink); p.Disc(cx, cy, r - 1, C(208, 186, 140)); p.Disc(cx - 1.5f, cy + 1.5f, r - 3, C(224, 206, 160)); }
            p.Fill(10, 9, 13, 10, C(150, 120, 80)); p.Fill(5, 4, 8, 4, C(170, 100, 80));
        });

        public static Sprite MetalCabinet => Paint("metalcab", 26, 40, p =>
        {
            p.Fill(0, 0, 25, 39, Ink); p.Fill(1, 1, 24, 38, C(120, 140, 138)); p.Fill(1, 37, 24, 38, C(150, 168, 164));
            p.Fill(12, 2, 13, 36, C(90, 106, 104));
            for (int y = 26; y < 34; y += 2) { p.Fill(3, y, 10, y, C(96, 112, 110)); p.Fill(15, y, 22, y, C(96, 112, 110)); }
            p.Fill(10, 16, 10, 19, Brass); p.Fill(15, 16, 15, 19, Brass);
            p.Fill(3, 7, 9, 11, Cream); p.Fill(4, 9, 8, 9, C(170, 90, 80)); p.Fill(16, 7, 22, 11, Cream); p.Fill(17, 9, 21, 9, C(80, 130, 170));
        });

        public static Sprite Ladder => Paint("ladder", 12, 40, p =>
        {
            p.Fill(1, 0, 2, 39, Ink); p.Fill(9, 0, 10, 39, Ink); p.Fill(1, 0, 1, 39, C(190, 140, 90)); p.Fill(9, 0, 9, 39, C(190, 140, 90));
            for (int y = 4; y < 40; y += 6) { p.Fill(2, y, 9, y + 1, Ink); p.Fill(2, y, 9, y, C(204, 152, 98)); }
        });

        /// <summary>A small wall sign: yellow hazard triangle, flask label or a bin label, by kind.</summary>
        public static Sprite Sign(int kind) => Paint("sign" + kind, 14, 12, p =>
        {
            p.Fill(0, 0, 13, 11, Ink); p.Fill(1, 1, 12, 10, kind == 0 ? C(250, 214, 90) : Cream);
            if (kind == 0) { for (int y = 2; y < 9; y++) { int half = (9 - y) / 2; p.Dot(6 - half, y, Ink); p.Dot(7 + half, y, Ink); } p.Fill(4, 2, 9, 2, Ink); p.Fill(6, 4, 7, 6, Ink); }
            else if (kind == 1) { p.Fill(5, 6, 8, 8, C(110, 170, 200)); p.Fill(4, 3, 9, 5, C(110, 170, 200)); p.Fill(6, 9, 7, 9, Ink); }
            else { p.Fill(3, 7, 10, 7, Ink); p.Fill(3, 4, 8, 4, Ink); }
        }, new Vector2(0.5f, 0.5f));

        public static Sprite ReadingDesk => Paint("readdesk", 40, 28, p =>
        {
            p.Fill(3, 0, 5, 12, Ink); p.Fill(34, 0, 36, 12, Ink); p.Fill(4, 1, 4, 11, AcademyArt.WalnutDark); p.Fill(35, 1, 35, 11, AcademyArt.WalnutDark);
            p.Fill(0, 11, 39, 20, Ink); p.Fill(1, 12, 38, 15, AcademyArt.Walnut); p.Fill(1, 16, 38, 19, AcademyArt.Oak); p.Fill(1, 19, 38, 19, AcademyArt.OakLight);
            p.Box(14, 12, 25, 15, AcademyArt.Walnut, AcademyArt.WalnutDark); p.Dot(19, 13, Brass); p.Dot(20, 13, Brass);
            // open book
            p.Fill(8, 18, 23, 22, Ink); p.Fill(9, 19, 15, 21, Cream); p.Fill(16, 19, 22, 21, Cream); p.Fill(15, 19, 16, 21, C(200, 186, 160));
            for (int x = 10; x < 15; x += 2) p.Dot(x, 20, C(150, 140, 130)); for (int x = 17; x < 22; x += 2) p.Dot(x, 20, C(150, 140, 130));
            // brass desk lamp with a green shade
            p.Fill(29, 18, 33, 19, Ink); p.Fill(31, 20, 31, 24, Brass); p.Fill(27, 24, 35, 27, Ink); p.Fill(28, 25, 34, 26, C(70, 120, 86)); p.Fill(29, 24, 33, 24, C(255, 232, 170));
        });

        public static Sprite SideTable => Paint("sidetable", 16, 18, p =>
        {
            p.Fill(3, 0, 4, 8, Ink); p.Fill(11, 0, 12, 8, Ink);
            p.Fill(0, 7, 15, 13, Ink); p.Fill(1, 8, 14, 11, AcademyArt.Oak); p.Fill(1, 12, 14, 12, AcademyArt.OakLight); p.Fill(1, 8, 14, 8, AcademyArt.WalnutDark);
            p.Fill(5, 13, 9, 16, Ink); p.Fill(6, 13, 8, 15, Cream); p.Fill(6, 15, 8, 15, C(160, 100, 60)); p.Dot(10, 14, Cream); p.Dot(10, 15, Ink);
        });

        public static Sprite BookStack => Paint("bookstack", 14, 12, p =>
        {
            Color32[] c = { C(200, 96, 84), C(92, 150, 120), C(226, 186, 96), C(96, 130, 196) };
            for (int i = 0; i < 4; i++) { int x0 = 1 + i % 2, w = 11 - i % 3; p.Fill(x0, i * 3, x0 + w, i * 3 + 2, Ink); p.Fill(x0 + 1, i * 3, x0 + w - 1, i * 3 + 1, c[i]); p.Fill(x0 + w - 2, i * 3, x0 + w - 1, i * 3 + 1, Cream); }
        });

        public static Sprite PotStack => Paint("potstack", 24, 18, p =>
        {
            foreach (var (x0, y0, w) in new[] { (1, 0, 10), (13, 0, 10), (6, 8, 11) })
            {
                p.Fill(x0, y0, x0 + w, y0 + 7, Ink); p.Fill(x0 + 1, y0 + 1, x0 + w - 1, y0 + 5, DecorArt.Pot); p.Fill(x0, y0 + 6, x0 + w, y0 + 7, C(150, 76, 54));
            }
            foreach (var (x, y) in new[] { (11, 16), (12, 17), (10, 17), (13, 16) }) p.Dot(x, y, DecorArt.Leaf);
        });

        public static Sprite WateringCan => Paint("can", 16, 12, p =>
        {
            p.Fill(3, 0, 11, 8, Ink); p.Fill(4, 1, 10, 7, C(110, 160, 150)); p.Fill(4, 6, 10, 6, C(150, 200, 190));
            for (int i = 0; i < 4; i++) p.Dot(11 + i, 4 + i, Ink); p.Fill(14, 8, 15, 9, Ink);
            p.Fill(5, 9, 9, 9, Ink); p.Dot(5, 10, Ink); p.Dot(9, 10, Ink); p.Fill(5, 11, 9, 11, Ink);
        });

        /// <summary>A trailing vine for the greenhouse glass, hanging from the frame.</summary>
        public static Sprite Vine => Paint("vine", 12, 40, p =>
        {
            for (int y = 0; y < 40; y++)
            {
                int x = 5 + Mathf.RoundToInt(Mathf.Sin(y * 0.35f) * 2);
                p.Dot(x, y, DecorArt.LeafDark);
                if (y % 4 == 0) { p.Dot(x - 1, y, DecorArt.Leaf); p.Dot(x - 2, y + 1, DecorArt.Leaf); p.Dot(x - 1, y + 1, DecorArt.Leaf); }
                if (y % 4 == 2) { p.Dot(x + 1, y, DecorArt.Leaf); p.Dot(x + 2, y + 1, DecorArt.Leaf); p.Dot(x + 1, y + 1, DecorArt.Leaf); }
                if (y % 11 == 5) p.Dot(x + 2, y, C(240, 170, 190));
            }
        }, new Vector2(0.5f, 1));

        /// <summary>A glass display case on a walnut plinth with a specimen and a brass plaque.</summary>
        public static Sprite DisplayCase(int kind) => Paint("case" + kind, 22, 32, p =>
        {
            Color32[] gems = { C(170, 120, 230), C(90, 200, 180), C(240, 130, 110), C(240, 200, 90) };
            p.Fill(1, 0, 20, 14, Ink); p.Fill(2, 1, 19, 13, AcademyArt.Walnut); p.Fill(2, 12, 19, 13, AcademyArt.WalnutLight); p.Fill(7, 5, 14, 8, Brass); p.Fill(8, 6, 13, 6, C(150, 110, 50));
            p.Fill(2, 14, 19, 31, Ink); p.Fill(3, 15, 18, 30, C(206, 232, 240)); p.Fill(3, 30, 18, 30, C(236, 248, 252));
            for (int i = 0; i < 5; i++) p.Dot(5 + i, 28 - i, Color.white);
            var g = gems[kind % 4];
            if (kind % 2 == 0) { p.Fill(8, 15, 13, 16, AcademyArt.WalnutDark); for (int y = 17; y < 25; y++) { int half = y < 21 ? y - 16 : 25 - y; p.Fill(10 - half / 2, y, 11 + half / 2, y, g); } p.Dot(10, 22, Color.white); }
            else { p.Disc(10.5f, 20, 3.6f, Ink); p.Disc(10.5f, 20, 2.8f, g); p.Dot(9, 21, Color.white); p.Fill(9, 15, 12, 16, AcademyArt.WalnutDark); }
        });

        /// <summary>A soft cone of light falling from the ceiling (white; tinted at runtime).</summary>
        public static Sprite Spot => Paint("spot", 40, 64, p =>
        {
            for (int y = 0; y < 64; y++)
            {
                float t = y / 63f, half = 6 + (1 - t) * 13;
                for (int x = 0; x < 40; x++)
                {
                    float u = Mathf.Abs(x - 19.5f);
                    if (u > half) continue;
                    float a = Mathf.Clamp01((half - u) / 4f) * (0.35f + 0.65f * t);
                    p.Dot(x, y, new Color(1, 1, 1, a));
                }
            }
        }, new Vector2(0.5f, 1));

        // ---------- dressing ----------

        public delegate SpriteRenderer PropMaker(Transform room, string name, Vector2 local, Sprite sprite, Vector2 footprint, int order = 6);

        /// <summary>Adds the room's floor and wall art (replacing the old tinted tiles) and its furnishings.</summary>
        public static void Build(string id, Transform room, int halfWidth, float backWall)
        {
            var floor = LaboratoryArt.Draw(room, "იატაკი", new Vector2(0, 0.5f), Floor(id), -300);
            floor.drawMode = SpriteDrawMode.Tiled; floor.size = new Vector2(halfWidth * 2, 11);
            // A soft band of shade along the foot of the back wall.
            var occlusion = LaboratoryArt.Draw(room, "კედლის ჩრდილი", new Vector2(0, backWall - 0.35f), AcademyArt.Shadow, -299);
            occlusion.transform.localScale = new Vector3(halfWidth, 1.4f, 1);
            var wall = LaboratoryArt.Draw(room, "კედელი", new Vector2(0, backWall), Wall(id), -290);
            wall.drawMode = SpriteDrawMode.Tiled; wall.size = new Vector2(halfWidth * 2, 4);
        }

        public static void Dress(string id, Transform room, float backWall, PropMaker prop)
        {
            var atmosphere = LabAtmosphere.Instance;
            var ambience = room.gameObject.AddComponent<AcademyAmbience>();
            SpriteRenderer Wall(string name, Vector2 local, Sprite sprite) => LaboratoryArt.Draw(room, name, local, sprite, -288);
            SpriteRenderer Thing(string name, Vector2 local, Sprite sprite, float width)
            {
                var art = prop(room, name, local, sprite, new Vector2(width, 0.6f));
                AcademyArt.ContactShadow(room, local, width + 0.4f);
                return art;
            }
            SpriteRenderer Loose(string name, Vector2 local, Sprite sprite) { AcademyArt.ContactShadow(room, local, sprite.bounds.size.x + 0.2f); return LaboratoryArt.Draw(room, name, local, sprite, 6); }
            void Lamp(Vector2 local, float strength)
            {
                Wall("ლამპარი", local, Sconce);
                atmosphere?.AddGlow(room, local, new Color(1f, 0.8f, 0.5f), 2.2f * strength, false);
            }
            void Runner(Vector2 centre, Vector2 size, Color color)
            {
                var rug = LaboratoryArt.Draw(room, "ხალიჩა", centre, AcademyArt.Rug(), -295);
                rug.color = color; rug.drawMode = SpriteDrawMode.Tiled; rug.size = size;
            }
            switch (id)
            {
                case "hallway":
                    Runner(new Vector2(0, 0.2f), new Vector2(27, 2.2f), new Color(0.62f, 0.36f, 0.32f));
                    foreach (float x in new[] { -13.6f, -7.6f, 1.6f, 7.6f, 13.6f }) Lamp(new Vector2(x, backWall + 2.3f), 1);
                    ambience.Sway(Thing("დიდი მცენარე", new Vector2(-9, 4.25f), AcademyArt.BigPlant, 0.9f).transform);
                    ambience.Sway(Thing("დიდი მცენარე", new Vector2(9, 4.25f), AcademyArt.BigPlant, 0.9f).transform);
                    Loose("წიგნები", new Vector2(-12.6f, -4.3f), BookStack);
                    break;
                case "storage":
                    Wall("ნიშანი", new Vector2(-6, backWall + 3.0f), Sign(0));
                    Wall("ნიშანი", new Vector2(-3.9f, backWall + 3.0f), Sign(1));
                    Wall("ნიშანი", new Vector2(-1.8f, backWall + 3.0f), Sign(2));
                    LaboratoryArt.Draw(room, "კიბე", new Vector2(4.6f, backWall - 0.1f), Ladder, -287);
                    Thing("ლითონის კარადა", new Vector2(2.6f, 4.0f), MetalCabinet, 1.5f);
                    Thing("კასრი", new Vector2(7.2f, 4.1f), Barrel, 0.9f);
                    Thing("კასრი", new Vector2(7.2f, -3.6f), Barrel, 0.9f);
                    Thing("ტომრები", new Vector2(-7.0f, -0.6f), Sacks, 1.4f);
                    atmosphere?.AddGlow(room, new Vector2(0, 2), new Color(1f, 0.85f, 0.55f), 5, false);
                    // The supply room sits in a little shade of its own.
                    var dim = LaboratoryArt.Draw(room, "ჩრდილი ოთახში", new Vector2(0, 1.5f), DecorArt.Square(), 9);
                    dim.transform.localScale = new Vector3(16 * 18, 16 * 15, 1); dim.color = new Color(0.08f, 0.06f, 0.1f, 0.14f);
                    break;
                case "reading":
                    Wall("სურათი", new Vector2(2.4f, backWall + 2.0f), AcademyArt.Picture(1));
                    Wall("სურათი", new Vector2(7.2f, backWall + 2.0f), AcademyArt.Picture(0));
                    Lamp(new Vector2(-7.3f, backWall + 2.2f), 1.2f);
                    Thing("საკითხავი მაგიდა", new Vector2(-5.2f, -2.6f), ReadingDesk, 2.4f);
                    atmosphere?.AddGlow(room, new Vector2(-4.5f, -1.0f), new Color(1f, 0.85f, 0.5f), 2.4f, false);
                    Thing("პატარა მაგიდა", new Vector2(2.2f, 1.0f), SideTable, 0.9f);
                    ambience.Sway(Thing("დიდი მცენარე", new Vector2(-7.3f, 3.9f), AcademyArt.BigPlant, 0.9f).transform);
                    Loose("წიგნები", new Vector2(7.2f, -3.4f), BookStack);
                    Loose("წიგნები", new Vector2(-2.3f, -3.7f), BookStack);
                    break;
                case "greenhouse":
                    foreach (float x in new[] { -8.5f, -5.1f, -1.7f, 1.7f, 5.1f, 8.5f })
                        ambience.Sway(LaboratoryArt.Draw(room, "ვაზი", new Vector2(x, backWall + 4f), Vine, -287).transform);
                    foreach (float x in new[] { -6.8f, 3.4f })
                    {
                        var beam = LaboratoryArt.Draw(room, "მზის სხივი", new Vector2(x - 1.0f, backWall + 1.2f), AcademyArt.Beam, 2);
                        ambience.Beams.Add((beam, 0.3f));
                    }
                    foreach (float x in new[] { -7.6f, -4.4f, 4.4f, 7.6f })
                        ambience.Sway(Thing("ქოთნის მცენარე", new Vector2(x, -4.3f), x < 0 ? AcademyArt.BigPlant : DecorArt.Plant(x > 6 ? 1 : 0), 0.9f).transform);
                    Thing("ქოთნები", new Vector2(-7.8f, 4.0f), PotStack, 1.4f);
                    Loose("სარწყავი", new Vector2(6.0f, 3.9f), WateringCan);
                    break;
                case "exhibition":
                    Runner(new Vector2(0, -0.6f), new Vector2(2.4f, 10.6f), new Color(0.7f, 0.26f, 0.3f));
                    foreach (float x in new[] { -7.5f, -2.5f, 2.5f, 7.5f })
                    {
                        var cone = LaboratoryArt.Draw(room, "პროჟექტორის შუქი", new Vector2(x, 5.0f), Spot, 2);
                        cone.color = new Color(1f, 0.92f, 0.7f, 0.22f);
                    }
                    Thing("ვიტრინა", new Vector2(-10.0f, 4.0f), DisplayCase(0), 1.3f);
                    Thing("ვიტრინა", new Vector2(9.9f, 4.0f), DisplayCase(1), 1.3f);
                    Thing("ჯილდოების კარადა", new Vector2(7.7f, 4.0f), DecorArt.TrophyCase(), 1.6f);
                    Wall("სურათი", new Vector2(-2.6f, backWall + 2.0f), AcademyArt.Picture(0));
                    Wall("სურათი", new Vector2(7.6f, backWall + 2.6f), AcademyArt.Picture(1));
                    foreach (float x in new[] { -9.6f, -1.4f, 1.4f, 9.6f }) Lamp(new Vector2(x, backWall + 3.0f), 1);
                    Thing("დიდი მცენარე", new Vector2(-10.3f, -4.2f), AcademyArt.BigPlant, 0.9f);
                    Thing("დიდი მცენარე", new Vector2(10.3f, -4.2f), AcademyArt.BigPlant, 0.9f);
                    break;
            }
        }
    }
}
