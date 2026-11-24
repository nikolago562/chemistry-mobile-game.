using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ChemistryGame
{
    /// <summary>
    /// The classroom's cozy academy look, painted in code at 16 px per unit: honey oak plank floor,
    /// walnut wainscot under sage wallpaper, curtained windows with a view, a framed chalkboard,
    /// wooden lab desks with sage epoxy tops, woven rugs, perimeter shelves and plants, sun shafts
    /// and soft contact shadows. The scene tilemaps keep their colliders; only their art is replaced.
    /// </summary>
    public static class AcademyArt
    {
        private static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        public static readonly Color32 Ink = C(46, 32, 30),
            Walnut = C(124, 78, 48), WalnutDark = C(92, 56, 36), WalnutLight = C(156, 104, 64),
            Oak = C(196, 142, 92), OakLight = C(222, 172, 116), Brass = C(232, 186, 96),
            Sage = C(150, 172, 132), SageLight = C(162, 184, 142), SageDark = C(132, 154, 118),
            Top = C(62, 96, 80), TopLight = C(86, 126, 104), TopEdge = C(44, 70, 58),
            Cream = C(244, 234, 208), Chalk = C(46, 78, 62), Sun = C(255, 226, 168);

        private static Sprite Paint(string key, int w, int h, System.Action<Px> draw, Vector2? pivot = null) =>
            DecorArt.Paint("academy:" + key, w, h, draw, 16, pivot);

        // ---------- floor ----------

        private static readonly Color32[] plankTones = { C(201, 148, 98), C(190, 138, 90), C(208, 156, 104), C(186, 134, 87) };
        public const int FloorW = 8, FloorH = 4;
        /// <summary>One cell of a continuous 8 x 4 cell oak floor (index = x + 8 y). Boards run across cells,
        /// so plank ends and tone changes never line up with the tile grid.</summary>
        public static Sprite Floor(int index) => Paint("floor" + index, 16, 16, p =>
        {
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                int X = index % FloorW * 16 + x, Y = index / FloorW * 16 + y, row = Y / 8, ly = Y % 8;
                if (ly == 0) { p.Dot(x, y, C(140, 94, 60)); continue; }
                // two or three board ends per 128 px row, wrapping so the pattern tiles seamlessly
                int count = 2 + row % 2, seg = 0; bool end = false, after = false;
                for (int j = 0; j < count; j++)
                {
                    int s = (row * 37 + j * (128 / count) + row * row * 11) % 128;
                    if (X > s) seg++;
                    end |= X == s; after |= X == s + 1;
                }
                if (seg == count) seg = 0;
                var tone = plankTones[(row * 3 + seg * 5 + row * seg) % 4];
                if (end) { p.Dot(x, y, C(140, 94, 60)); continue; }
                Color32 c = ly == 7 || after ? Lighter(tone, 10) : tone;
                int grain = (X / 5 * 13 + Y * 7 + seg * 3) % 19;
                if (grain == 0 && ly > 1 && ly < 6) c = Darker(tone, 10);
                if ((X * 7 + row * 29) % 211 == 5 && ly == 4) c = C(150, 100, 64);
                p.Dot(x, y, c);
            }
        }, new Vector2(0.5f, 0.5f));

        private static Color32 Lighter(Color32 c, int by) => C((byte)Mathf.Min(255, c.r + by), (byte)Mathf.Min(255, c.g + by), (byte)Mathf.Min(255, c.b + by * 3 / 4));
        private static Color32 Darker(Color32 c, int by) => C((byte)Mathf.Max(0, c.r - by), (byte)Mathf.Max(0, c.g - by), (byte)Mathf.Max(0, c.b - by));

        // ---------- back wall ----------

        /// <summary>A 2 x 4 unit stretch of wall: baseboard, raised walnut panels, chair rail, sage wallpaper, crown moulding.</summary>
        public static Sprite Wall => Paint("wall", 32, 64, p =>
        {
            p.Fill(0, 0, 31, 0, Ink); p.Fill(0, 1, 31, 2, WalnutDark); p.Fill(0, 3, 31, 3, WalnutLight);
            p.Fill(0, 4, 31, 19, Walnut);
            foreach (int x0 in new[] { 2, 18 })
            {
                p.Box(x0, 6, x0 + 11, 17, C(136, 88, 54), WalnutDark);
                p.Fill(x0 + 1, 16, x0 + 10, 16, WalnutLight); p.Fill(x0 + 10, 7, x0 + 10, 15, C(116, 72, 44));
            }
            p.Fill(0, 20, 31, 20, WalnutDark); p.Fill(0, 21, 31, 22, C(176, 124, 78)); p.Fill(0, 23, 31, 23, C(204, 152, 98));
            p.Fill(0, 24, 31, 57, Sage);
            for (int x = 0; x < 32; x += 8) p.Fill(x, 24, x + 1, 57, SageLight);
            for (int y = 29; y < 56; y += 9)
                for (int x = (y / 9) % 2 == 0 ? 4 : 12; x < 32; x += 16)
                { p.Dot(x, y, C(198, 204, 168)); p.Dot(x - 1, y - 1, C(184, 196, 156)); p.Dot(x + 1, y - 1, C(184, 196, 156)); p.Dot(x, y - 2, C(184, 196, 156)); }
            p.Fill(0, 24, 31, 24, SageDark); p.Fill(0, 56, 31, 57, SageDark);
            p.Fill(0, 58, 31, 58, WalnutDark); p.Fill(0, 59, 31, 61, C(138, 90, 56)); p.Fill(0, 60, 31, 60, C(182, 130, 82)); p.Fill(0, 62, 31, 63, WalnutLight);
        }, new Vector2(0.5f, 0));

        /// <summary>A wooden window frame with mullions, a green valance, tied curtains, a sill and a little pot plant.</summary>
        public static Sprite Window => Paint("window", 51, 43, p =>
        {
            Color32 cloth = C(232, 206, 176), fold = C(208, 174, 146);
            for (int side = 0; side < 2; side++)
                for (int y = 6; y <= 40; y++)
                {
                    int pinch = y < 21 ? (21 - y) / 4 : 0, inner = side == 0 ? 10 - pinch : 40 + pinch, outer = side == 0 ? 1 : 49;
                    for (int x = Mathf.Min(inner, outer); x <= Mathf.Max(inner, outer); x++)
                        p.Dot(x, y, x == outer || x == inner ? Darker(fold, 30) : (x % 3 == 0 ? fold : cloth));
                }
            for (int y = 7; y <= 39; y++) for (int x = 8; x <= 42; x++)
            {
                bool opening = x >= 11 && x <= 39 && y >= 10 && y <= 36;
                if (opening) continue;
                bool outline = x == 8 || x == 42 || y == 7 || y == 39;
                p.Dot(x, y, outline ? Ink : (x == 10 || y == 37) ? WalnutDark : Oak);
            }
            p.Fill(25, 10, 25, 36, Oak); p.Fill(26, 10, 26, 36, WalnutDark);
            p.Fill(11, 23, 39, 23, Oak); p.Fill(11, 22, 39, 22, WalnutDark);
            p.Fill(0, 20, 2, 21, Brass); p.Fill(48, 20, 50, 21, Brass);
            p.Fill(0, 38, 50, 42, Ink); p.Fill(1, 39, 49, 42, C(108, 140, 96)); p.Fill(1, 41, 49, 41, C(132, 164, 116));
            for (int x = 1; x < 50; x += 4) { p.Dot(x, 38, C(108, 140, 96)); p.Dot(x + 1, 38, C(108, 140, 96)); p.Dot(x, 37, Brass); }
            p.Fill(5, 4, 45, 7, Ink); p.Fill(6, 5, 44, 6, C(170, 116, 70)); p.Fill(6, 7, 44, 7, OakLight);
            p.Fill(10, 1, 40, 3, Ink); p.Fill(11, 2, 39, 3, WalnutDark);
            // a terracotta pot with a sprig on the sill
            p.Fill(13, 8, 18, 12, Ink); p.Fill(14, 9, 17, 11, DecorArt.Pot); p.Fill(14, 11, 17, 11, C(220, 130, 96));
            foreach (var (x, y) in new[] { (15, 13), (16, 14), (14, 15), (17, 16), (15, 16), (16, 13) }) p.Dot(x, y, y % 2 == 0 ? DecorArt.Leaf : DecorArt.LeafDark);
            // a small blue bottle on the other side
            p.Fill(33, 8, 36, 13, Ink); p.Fill(34, 9, 35, 12, C(120, 190, 220)); p.Fill(34, 14, 35, 15, Ink); p.Dot(34, 12, C(220, 244, 255));
        }, new Vector2(0.5f, 23.5f / 43f));

        /// <summary>The view through a window: far hills, near meadow and two round trees (sky stays the time-of-day pane).</summary>
        public static Sprite View => Paint("view", 29, 27, p =>
        {
            for (int x = 0; x < 29; x++)
            {
                int far = 9 + Mathf.RoundToInt(Mathf.Sin(x * 0.28f + 1) * 2), near = 5 + Mathf.RoundToInt(Mathf.Sin(x * 0.2f + 3) * 1.5f);
                p.Fill(x, 0, x, far, C(150, 186, 160)); p.Fill(x, 0, x, near, C(112, 160, 108));
                p.Dot(x, near, C(136, 182, 120));
            }
            p.Fill(18, 9, 19, 14, C(150, 140, 160)); p.Fill(17, 15, 20, 15, C(150, 140, 160)); p.Dot(18, 16, C(150, 140, 160)); // a far tower
            foreach (var (cx, cy, r) in new[] { (5f, 9f, 3.5f), (23f, 8f, 3f) })
            {
                p.Fill((int)cx, 3, (int)cx, (int)cy - 2, C(110, 76, 52));
                p.Disc(cx, cy, r, C(70, 124, 82)); p.Disc(cx - 0.8f, cy + 0.8f, r - 1.4f, C(96, 152, 98));
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>A framed green chalkboard with a chalk tray and corner doodles (the middle stays clear for the lettering).</summary>
        public static Sprite Chalkboard => Paint("chalkboard", 64, 40, p =>
        {
            p.Fill(0, 0, 63, 39, Ink); p.Fill(1, 1, 62, 38, Oak); p.Fill(1, 38, 62, 38, OakLight); p.Fill(1, 1, 62, 1, WalnutDark);
            p.Fill(3, 4, 60, 36, Ink); p.Fill(4, 5, 59, 35, Chalk);
            for (int i = 0; i < 40; i++) { int x = 5 + (i * 37) % 54, y = 6 + (i * 23) % 29; p.Dot(x, y, C(56, 90, 72)); p.Dot(x + 1, y, C(56, 90, 72)); }
            Color32 chalk = C(214, 232, 214);
            // benzene ring, top left
            foreach (var (x, y) in new[] { (8, 31), (9, 32), (10, 32), (11, 31), (11, 30), (10, 29), (9, 29), (8, 30) }) p.Dot(x, y, chalk);
            p.Dot(12, 32, chalk); p.Dot(13, 33, chalk);
            // flask, top right
            p.Fill(53, 29, 53, 33, chalk); p.Fill(55, 29, 55, 33, chalk); p.Dot(52, 28, chalk); p.Dot(56, 28, chalk); p.Dot(51, 27, chalk); p.Dot(57, 27, chalk);
            p.Fill(51, 26, 57, 26, chalk); p.Fill(52, 27, 56, 27, C(150, 220, 170));
            // arrow, bottom left, and a little atom, bottom right
            p.Fill(7, 8, 13, 8, chalk); p.Dot(12, 9, chalk); p.Dot(12, 7, chalk);
            foreach (var (x, y) in new[] { (52, 9), (53, 10), (55, 10), (56, 9), (55, 8), (53, 8) }) p.Dot(x, y, chalk); p.Dot(54, 9, C(240, 170, 170));
            p.Fill(2, 2, 61, 3, WalnutDark); p.Fill(2, 3, 61, 3, OakLight);
            p.Fill(12, 4, 15, 4, Cream); p.Fill(19, 4, 21, 4, C(250, 190, 200)); p.Fill(46, 4, 49, 4, C(250, 230, 150));
        }, new Vector2(0.5f, 0.5f));

        public static Sprite Clock => Paint("clock", 18, 18, p =>
        {
            p.Disc(8.5f, 8.5f, 8.6f, Ink); p.Disc(8.5f, 8.5f, 7.6f, Walnut); p.Disc(8.5f, 8.5f, 6.4f, Cream);
            p.Disc(7.5f, 9.5f, 2, C(252, 246, 228));
            foreach (var (x, y) in new[] { (8, 14), (9, 14), (8, 2), (9, 2), (2, 8), (2, 9), (14, 8), (14, 9) }) p.Dot(x, y, WalnutDark);
            p.Dot(4, 13, Brass); p.Dot(13, 13, Brass); p.Dot(4, 4, Brass); p.Dot(13, 4, Brass);
        }, new Vector2(0.5f, 0.5f));

        /// <summary>A wood-framed cream print with one of three chemistry drawings.</summary>
        public static Sprite Picture(int kind) => Paint("picture" + kind, 24, 28, p =>
        {
            p.Fill(0, 0, 23, 27, Ink); p.Fill(1, 1, 22, 26, Oak); p.Fill(1, 26, 22, 26, OakLight); p.Fill(1, 1, 22, 1, WalnutDark);
            p.Fill(3, 3, 20, 24, Darker(Cream, 30)); p.Fill(4, 4, 19, 23, Cream);
            Color32 inkLine = C(80, 70, 70);
            if (kind == 0)
            {   // Bohr atom
                for (int a = 0; a < 40; a++)
                {
                    float t = a / 40f * Mathf.PI * 2;
                    p.Dot(Mathf.RoundToInt(11.5f + Mathf.Cos(t) * 6.5f), Mathf.RoundToInt(14 + Mathf.Sin(t) * 3), C(90, 160, 180));
                    p.Dot(Mathf.RoundToInt(11.5f + Mathf.Cos(t) * 3), Mathf.RoundToInt(14 + Mathf.Sin(t) * 6.5f), C(90, 160, 180));
                }
                p.Disc(11.5f, 14, 1.6f, C(224, 104, 90)); p.Dot(11, 14, C(120, 140, 220));
                p.Dot(18, 14, C(240, 200, 90)); p.Dot(11, 20, C(240, 200, 90));
                p.Fill(7, 6, 16, 6, inkLine);
            }
            else if (kind == 1)
            {   // Erlenmeyer flask diagram
                for (int y = 7; y <= 15; y++) { int half = (16 - y) / 2 + 1; p.Dot(11 - half, y, inkLine); p.Dot(12 + half, y, inkLine); if (y < 12) p.Fill(12 - half, y, 11 + half, y, C(120, 200, 150)); }
                p.Fill(6, 6, 17, 6, inkLine); p.Fill(10, 16, 10, 20, inkLine); p.Fill(13, 16, 13, 20, inkLine); p.Fill(9, 21, 14, 21, inkLine);
                p.Dot(11, 13, C(200, 240, 210)); p.Dot(12, 9, C(200, 240, 210));
                p.Fill(16, 18, 19, 18, inkLine); p.Fill(16, 12, 18, 12, inkLine);
            }
            else
            {   // water molecule
                p.Dot(9, 14, inkLine); p.Dot(8, 13, inkLine); p.Dot(14, 14, inkLine); p.Dot(15, 13, inkLine);
                p.Disc(11.5f, 17, 3.4f, inkLine); p.Disc(11.5f, 17, 2.6f, C(224, 92, 84)); p.Disc(10.5f, 18, 0.8f, C(250, 170, 160));
                p.Disc(6.5f, 11, 2.4f, inkLine); p.Disc(6.5f, 11, 1.7f, C(236, 236, 244));
                p.Disc(16.5f, 11, 2.4f, inkLine); p.Disc(16.5f, 11, 1.7f, C(236, 236, 244));
                p.Fill(7, 6, 9, 6, inkLine); p.Fill(11, 6, 12, 6, inkLine); p.Fill(14, 6, 16, 6, inkLine);
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>A short wall shelf: books, a jar and a trailing plant.</summary>
        public static Sprite WallShelf => Paint("wallshelf", 30, 22, p =>
        {
            Color32[] spines = { C(200, 96, 84), C(92, 150, 120), C(226, 186, 96), C(96, 130, 196), C(160, 110, 180) };
            foreach (int shelf in new[] { 0, 11 })
            {
                p.Fill(0, shelf, 29, shelf + 2, Ink); p.Fill(1, shelf + 1, 28, shelf + 1, Oak); p.Fill(1, shelf + 2, 28, shelf + 2, OakLight);
            }
            for (int i = 0, x = 2; i < 5; i++) { int h = 6 + i % 2 * 2; p.Fill(x, 3, x + 2, 3 + h, Ink); p.Fill(x, 4, x + 1, 2 + h, spines[i]); x += 3; }
            p.Fill(17, 3, 18, 9, Ink); p.Fill(19, 3, 22, 6, Ink); // a leaning book and a stack
            p.Fill(20, 4, 21, 5, spines[3]);
            p.Fill(24, 3, 28, 9, Ink); p.Fill(25, 4, 27, 8, C(236, 246, 255)); p.Fill(25, 4, 27, 6, C(250, 196, 110)); p.Fill(25, 9, 27, 9, C(150, 100, 70));
            p.Fill(3, 14, 8, 19, Ink); p.Fill(4, 15, 7, 18, DecorArt.Pot);
            foreach (var (x, y) in new[] { (5, 20), (6, 21), (4, 21), (8, 20), (9, 19), (10, 18), (10, 17), (11, 16), (11, 15), (2, 19), (1, 18), (1, 17) }) p.Dot(x, y, (x + y) % 2 == 0 ? DecorArt.Leaf : DecorArt.LeafDark);
            for (int i = 0, x = 15; i < 4; i++) { p.Fill(x, 14, x + 1, 19 - i % 2, spines[(i + 2) % 5]); x += 3; }
            p.Disc(26, 16.5f, 2.6f, Ink); p.Disc(26, 16.5f, 1.8f, C(120, 200, 170)); p.Dot(25, 17, C(220, 250, 236));
        }, new Vector2(0.5f, 0.5f));

        // ---------- furniture ----------

        /// <summary>A walnut lab desk with drawers, cupboard doors and a sage epoxy top (4 x 2 units, same footprint as the old bench).</summary>
        public static Sprite Desk(bool sink) => Paint(sink ? "sinkdesk" : "desk", 64, 32, p =>
        {
            p.Fill(1, 0, 62, 19, Ink); p.Fill(2, 1, 61, 18, Walnut); p.Fill(2, 1, 61, 2, WalnutDark);
            foreach (var (x0, x1) in new[] { (4, 19), (44, 59) })
                foreach (var (y0, y1) in new[] { (4, 9), (11, 16) })
                {
                    p.Box(x0, y0, x1, y1, Walnut, WalnutDark); p.Fill(x0 + 1, y1 - 1, x1 - 1, y1 - 1, WalnutLight);
                    int k = (x0 + x1) / 2; p.Fill(k, y0 + 2, k + 1, y0 + 3, Brass);
                }
            p.Box(22, 4, 31, 16, Walnut, WalnutDark); p.Box(32, 4, 41, 16, Walnut, WalnutDark);
            p.Fill(23, 15, 30, 15, WalnutLight); p.Fill(33, 15, 40, 15, WalnutLight);
            p.Fill(29, 9, 29, 11, Brass); p.Fill(34, 9, 34, 11, Brass);
            p.Fill(0, 18, 63, 31, Ink);
            p.Fill(1, 19, 62, 19, TopEdge); p.Fill(1, 20, 62, 20, TopLight); p.Fill(1, 21, 62, 30, Top); p.Fill(1, 30, 62, 30, C(74, 110, 92));
            foreach (var (x, y) in new[] { (6, 27), (17, 24), (29, 28), (40, 23), (51, 27), (58, 22), (12, 22) }) p.Dot(x, y, TopLight);
            if (!sink) return;
            p.Fill(42, 22, 57, 28, Ink); p.Fill(43, 23, 56, 27, C(170, 184, 190)); p.Fill(44, 23, 55, 26, C(110, 140, 156)); p.Dot(46, 25, C(210, 236, 248));
            p.Fill(49, 28, 50, 31, C(190, 196, 200)); p.Fill(48, 29, 51, 29, C(150, 160, 166));
        }, new Vector2(0.5f, 0));

        /// <summary>A brass microscope.</summary>
        public static Sprite Microscope => Paint("microscope", 12, 16, p =>
        {
            p.Fill(1, 0, 10, 2, Ink); p.Fill(2, 1, 9, 1, C(90, 96, 110));
            p.Fill(7, 2, 9, 11, Ink); p.Fill(8, 3, 8, 10, Brass);
            p.Fill(2, 5, 8, 6, Ink); p.Fill(3, 5, 7, 5, C(200, 210, 220));
            p.Fill(3, 8, 6, 15, Ink); p.Fill(4, 9, 5, 14, Brass); p.Fill(4, 14, 5, 14, C(250, 224, 150));
            p.Fill(5, 7, 6, 8, Ink);
        }, new Vector2(0.5f, 0));

        /// <summary>A ball-and-stick molecule model on a little stand.</summary>
        public static Sprite MoleculeModel => Paint("model", 16, 16, p =>
        {
            p.Fill(5, 0, 10, 1, Ink); p.Fill(7, 1, 8, 5, WalnutDark);
            Color32 stick = C(200, 200, 208);
            p.Fill(4, 10, 11, 10, stick); p.Fill(7, 6, 8, 13, stick);
            p.Disc(7.5f, 10, 2.6f, Ink); p.Disc(7.5f, 10, 1.8f, C(70, 70, 80));
            foreach (var (x, y, c) in new[] { (2.5f, 10f, C(236, 236, 244)), (12.5f, 10f, C(236, 236, 244)), (7.5f, 14.2f, C(224, 92, 84)), (7.5f, 5.8f, C(96, 170, 110)) })
            { p.Disc(x, y, 1.9f, Ink); p.Disc(x, y, 1.2f, c); }
        }, new Vector2(0.5f, 0));

        /// <summary>A small distillation set: burner, round flask of violet liquid, glass condenser and a receiving flask.</summary>
        public static Sprite Still => Paint("still", 40, 26, p =>
        {
            Color32 glass = C(214, 236, 244), glassEdge = C(110, 150, 170), liquid = C(176, 130, 226);
            p.Fill(3, 0, 14, 1, Ink); p.Fill(5, 2, 12, 5, Ink); p.Fill(6, 3, 11, 4, C(90, 96, 110)); // burner
            p.Disc(8.5f, 11, 5.4f, glassEdge); p.Disc(8.5f, 11, 4.5f, glass);
            for (int y = 6; y <= 10; y++) for (int x = 4; x <= 13; x++) if ((x - 8.5f) * (x - 8.5f) + (y - 11) * (y - 11) < 20) p.Dot(x, y, liquid);
            p.Dot(7, 13, C(255, 255, 255));
            p.Fill(7, 16, 10, 20, glassEdge); p.Fill(8, 16, 9, 19, glass);
            for (int i = 0; i < 20; i++) { int x = 10 + i, y = 20 - i / 3; p.Dot(x, y, glassEdge); p.Dot(x, y - 1, glassEdge); p.Dot(x, y + 1, i % 3 == 0 ? C(150, 210, 230) : glassEdge); }
            p.Fill(14, 21, 15, 22, Ink); p.Fill(14, 22, 14, 25, Ink); // clamp stand
            p.Fill(30, 0, 38, 1, Ink);
            for (int y = 2; y <= 9; y++) { int half = (12 - y) / 2 + 1; p.Fill(34 - half, y, 34 + half, y, y < 5 ? liquid : glass); p.Dot(34 - half, y, glassEdge); p.Dot(34 + half, y, glassEdge); }
            p.Fill(33, 10, 35, 14, glassEdge); p.Dot(34, 11, glass);
        }, new Vector2(0.5f, 0));

        /// <summary>A tall leafy plant in a terracotta pot.</summary>
        public static Sprite BigPlant => Paint("bigplant", 20, 30, p =>
        {
            p.Fill(5, 0, 14, 9, Ink); p.Fill(6, 1, 13, 8, DecorArt.Pot); p.Fill(5, 8, 14, 10, Ink); p.Fill(6, 9, 13, 9, C(220, 130, 96)); p.Fill(6, 1, 7, 7, C(160, 82, 60));
            foreach (var (cx, cy, r) in new[] { (5f, 16f, 4f), (14f, 17f, 4.2f), (9.5f, 23f, 4.8f), (4f, 23f, 3f), (15.5f, 24f, 3f), (10f, 14f, 3.5f) })
            { p.Disc(cx, cy, r, DecorArt.LeafDark); p.Disc(cx - 0.6f, cy + 0.6f, r - 1.2f, DecorArt.Leaf); p.Dot(Mathf.RoundToInt(cx - 1), Mathf.RoundToInt(cy + 1), C(140, 200, 130)); }
            p.Fill(9, 10, 10, 13, C(70, 110, 60));
        }, new Vector2(0.5f, 0));

        // ---------- soft light and shade ----------

        /// <summary>A soft oval contact shadow, 2 units wide; scale it to the furniture.</summary>
        public static Sprite Shadow => Paint("shadow", 32, 8, p =>
        {
            for (int y = 0; y < 8; y++) for (int x = 0; x < 32; x++)
            {
                float d = Mathf.Pow((x - 15.5f) / 16f, 2) + Mathf.Pow((y - 3.5f) / 4f, 2);
                if (d < 1) p.Dot(x, y, new Color(0.16f, 0.09f, 0.07f, (1 - d) * 0.42f));
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>A slanted shaft of window light (white; tinted at runtime). Pivot at the top left.</summary>
        public static Sprite Beam => Paint("beam", 62, 80, p =>
        {
            for (int y = 0; y < 80; y++)
            {
                float t = y / 79f, left = (1 - t) * 32;
                for (int x = 0; x < 62; x++)
                {
                    float u = x - left;
                    if (u < 0 || u > 29) continue;
                    float edge = Mathf.Clamp01(Mathf.Min(u, 29 - u) / 4f);
                    float a = edge * (0.25f + 0.75f * t) * Mathf.Clamp01(t * 6);
                    if (a > 0.02f) p.Dot(x, y, new Color(1, 1, 1, a));
                }
            }
        }, new Vector2(0, 1));

        /// <summary>The window's light patch on the floor: four panes split by the mullion shadows.</summary>
        public static Sprite Patch => Paint("patch", 36, 16, p =>
        {
            for (int y = 0; y < 16; y++) for (int x = 0; x < 36; x++)
            {
                float u = x - (15 - y) * 0.4f;
                if (u < 1 || u > 30) continue;
                if (Mathf.Abs(u - 15.5f) < 1 || y == 8) continue;
                float edge = Mathf.Clamp01(Mathf.Min(Mathf.Min(u - 1, 30 - u), Mathf.Min(y, 15 - y)) / 3f);
                p.Dot(x, y, new Color(1, 1, 1, 0.35f + 0.65f * edge));
            }
        }, new Vector2(0.5f, 0.5f));

        // ---------- rugs ----------

        private static readonly Dictionary<int, Sprite> rugs = new Dictionary<int, Sprite>();
        /// <summary>A woven rug that tiles (draw mode Tiled): fringe, dark edge, a bright band and a diamond field. Grey-scale, tinted per table.</summary>
        public static Sprite Rug()
        {
            if (rugs.TryGetValue(0, out var cached) && cached != null) return cached;
            const int size = 24;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                int d = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
                float v;
                if (d == 0) v = (y == 0 || y == size - 1) && x % 2 == 0 && x > 0 && x < size - 1 ? 0.95f : -1;
                else if (d == 1) v = 0.42f;
                else if (d <= 3) v = 1f;
                else if (d == 4) v = 0.7f;
                else
                {
                    int u = (x - 6) % 12, w = (y - 6) % 12;
                    float m = Mathf.Abs(u - 5.5f) + Mathf.Abs(w - 5.5f);
                    v = m > 4 && m < 5.2f ? 1f : m < 1.2f ? 0.98f : 0.84f;
                }
                texture.SetPixel(x, y, v < 0 ? Color.clear : new Color(v, v, v, 1));
            }
            texture.Apply();
            return rugs[0] = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16, 0, SpriteMeshType.FullRect, Vector4.one * 6);
        }

        // ---------- dressing the classroom ----------

        /// <summary>Replaces the classroom's tile art and adds the wall dressing, perimeter furniture, sun shafts and ambience.</summary>
        public static void Dress(LabGame game, Transform root)
        {
            var room = game.classroom;
            RetileClassroom(room);
            for (float x = 26; x <= 66; x += 2) Draw(root, "კედელი", new Vector2(x, 11), Wall, 0);

            var ambience = root.gameObject.AddComponent<AcademyAmbience>();
            foreach (float x in new[] { 27f, 52f })
            {
                var view = Draw(root, "ხედი ფანჯრიდან", new Vector2(x, 13.17f), View, 2);
                ambience.Views.Add(view);
                Draw(root, "ფანჯრის ჩარჩო", new Vector2(x, 13.17f), Window, 3);
                // A shaft of sunlight from the sill down onto the floor, and the window's bright patch where it lands.
                var beam = Draw(root, "მზის სხივი", new Vector2(x - 0.95f, 12.3f), Beam, 2);
                ambience.Beams.Add((beam, 0.28f));
                var patch = Draw(root, "მზის ლაქა", new Vector2(x + 1.8f, 7.75f), Patch, 2);
                patch.transform.localScale = new Vector3(1, 1.25f, 1);
                ambience.Beams.Add((patch, 0.4f));
            }
            Draw(root, "საათი", new Vector2(30.03f, 13.95f), Clock, 4);
            Draw(root, "დაფა", new Vector2(36, 13.35f), Chalkboard, 3);
            Draw(root, "განცხადებების დაფა", new Vector2(42.9f, 13.0f), DecorArt.Noticeboard(), 3);
            Draw(root, "ბოთლების თარო", new Vector2(32.4f, 12.0f), CozyLabArt.VialShelf, 3);
            Draw(root, "სურათი: კოლბა", new Vector2(39.85f, 12.95f), Picture(1), 3);
            Draw(root, "სურათი: წყლის მოლეკულა", new Vector2(49.3f, 12.85f), Picture(2), 3);
            Draw(root, "კედლის თარო", new Vector2(54.6f, 13.1f), WallShelf, 3);
            Draw(root, "სურათი: ატომი", new Vector2(61, 13.5f), Picture(0), 3);
            ambience.Sway(Draw(root, "ჩამოკიდებული მცენარე", new Vector2(64.8f, 12.9f), CozyLabArt.HangingPlant, 3).transform);

            // Perimeter furniture, flush with the side walls and clear of the aisles at x 30 and 60.
            Furniture(root, "წიგნების კარადა", new Vector2(26.3f, 3.2f), CozyLabArt.Bookshelf, 1.8f);
            ambience.Sway(Furniture(root, "დიდი მცენარე", new Vector2(26.0f, -1.4f), BigPlant, 1.0f).transform);
            var scope = Furniture(root, "მიკროსკოპის მაგიდა", new Vector2(26.9f, -9f), CozyLabArt.Table, 2.8f);
            Draw(scope.transform, "მიკროსკოპი", new Vector2(-0.6f, 1.1f), Microscope, 7);
            Draw(scope.transform, "მოლეკულის მოდელი", new Vector2(0.7f, 1.1f), MoleculeModel, 7);
            ambience.Blink(Draw(scope.transform, "ინდიკატორი", new Vector2(-1.15f, 0.82f), DecorArt.Square(), 7), new Color(0.4f, 1f, 0.6f), 1.6f);
            Furniture(root, "წიგნების კარადა", new Vector2(65.7f, 3.0f), CozyLabArt.Bookshelf, 1.8f);
            ambience.Sway(Furniture(root, "დიდი მცენარე", new Vector2(65.9f, -3.2f), BigPlant, 1.0f).transform);
            var still = Furniture(root, "გამოხდის მაგიდა", new Vector2(65.1f, -16f), CozyLabArt.Table, 2.8f);
            Draw(still.transform, "გამოხდის აპარატი", new Vector2(0, 1.1f), Still, 7);
            ambience.Blink(Draw(still.transform, "ინდიკატორი", new Vector2(1.2f, 0.82f), DecorArt.Square(), 7), new Color(1f, 0.6f, 0.3f), 2.3f);
            ambience.Sway(Furniture(root, "დიდი მცენარე", new Vector2(26.0f, -18.5f), BigPlant, 1.0f).transform);

            // Soft contact shadows under the furniture other code builds (closet, collection and shop cabinets).
            ContactShadow(root, game.lockerPosition, 2.2f);
            ContactShadow(root, new Vector2(28.4f, 9.3f), 2.1f);
            ContactShadow(root, new Vector2(63, -9), 2.2f);
        }

        /// <summary>Oak planks with gentle per-cell colour drift and darker cells along the walls; the back wall and its
        /// old decor tiles fade out (their colliders stay), and the new painted wall takes their place.</summary>
        private static void RetileClassroom(Transform room)
        {
            var planks = new Tile[AcademyArt.FloorW * AcademyArt.FloorH];
            for (int i = 0; i < planks.Length; i++)
            {
                planks[i] = ScriptableObject.CreateInstance<Tile>();
                planks[i].sprite = Floor(i); planks[i].colliderType = Tile.ColliderType.None; planks[i].flags = TileFlags.None;
            }
            foreach (var map in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                bool floor = map.name == "Floor", hide = map.name == "Wall" || map.name == "Decor";
                if (!floor && !hide) continue;
                foreach (var cell in map.cellBounds.allPositionsWithin)
                {
                    if (!map.HasTile(cell)) continue;
                    Vector2 local = room.InverseTransformPoint(map.GetCellCenterWorld(cell));
                    if (local.x < 24.5f || local.x > 67.5f) continue;
                    if (floor)
                    {
                        int cx = Mathf.FloorToInt(local.x), cy = Mathf.FloorToInt(local.y);
                        map.SetTile(cell, planks[Mod(cx, FloorW) + FloorW * Mod(cy, FloorH)]);
                        float shade = 0.94f + Mathf.PerlinNoise(cx * 0.17f, cy * 0.17f) * 0.08f;
                        if (cy >= 10) shade *= 0.8f; else if (cy == 9) shade *= 0.92f;
                        if (cx <= 25 || cx >= 66) shade *= 0.9f;
                        if (cy <= -24) shade *= 0.93f;
                        map.SetColor(cell, new Color(shade, shade * 0.985f, shade * 0.97f));
                    }
                    else if (local.y > 10.9f)
                    {
                        map.SetTileFlags(cell, TileFlags.None);
                        map.SetColor(cell, Color.clear);
                    }
                }
            }
        }

        private static int Mod(int a, int m) => (a % m + m) % m;

        private static SpriteRenderer Draw(Transform parent, string name, Vector2 local, Sprite sprite, int order) =>
            LaboratoryArt.Draw(parent, name, local, sprite, order);

        /// <summary>Furniture standing on the floor: solid footprint, a depth zone behind it and a contact shadow.</summary>
        private static SpriteRenderer Furniture(Transform root, string name, Vector2 local, Sprite sprite, float width)
        {
            var art = Draw(root, name, local, sprite, 6);
            var shape = art.gameObject.AddComponent<BoxCollider2D>();
            shape.size = new Vector2(width, 0.7f); shape.offset = new Vector2(0, 0.35f);
            DepthOccluders.Add(art.transform.position, width * 0.5f + 0.35f, 0.45f, sprite.bounds.size.y + 1.2f);
            ContactShadow(root, local, width + 0.4f);
            return art;
        }

        public static void ContactShadow(Transform parent, Vector2 local, float width)
        {
            var shade = Draw(parent, "ჩრდილი", local + new Vector2(0, 0.02f), Shadow, 2);
            shade.transform.localScale = new Vector3(width / 2f, 1.1f, 1);
        }
    }

    /// <summary>Gentle classroom motion: breathing sun shafts that fade at evening and in rain, the window view
    /// dimming with the sky, plants nudging a pixel in a draught, and blinking equipment lights.</summary>
    public sealed class AcademyAmbience : MonoBehaviour
    {
        public readonly List<(SpriteRenderer art, float strength)> Beams = new List<(SpriteRenderer, float)>();
        public readonly List<SpriteRenderer> Views = new List<SpriteRenderer>();
        private readonly List<(Transform t, Vector3 home, float phase)> swaying = new List<(Transform, Vector3, float)>();
        private readonly List<(SpriteRenderer art, Color color, float period)> blinkers = new List<(SpriteRenderer, Color, float)>();
        private float sun = -1;

        public void Sway(Transform t) => swaying.Add((t, t.localPosition, swaying.Count * 1.7f));
        public void Blink(SpriteRenderer art, Color color, float period)
        {
            art.transform.localScale = Vector3.one; // one screen pixel
            blinkers.Add((art, color, period));
        }

        private void Update()
        {
            float t = Time.time;
            // Full sun by day, low amber shafts at dusk, none at night.
            float target = SchoolCalendar.Raining ? 0.2f : LabAtmosphere.Night ? 0 : LabAtmosphere.Evening > 0 ? 0.55f : 1;
            sun = sun < 0 ? target : Mathf.MoveTowards(sun, target, Time.deltaTime * 0.2f);
            var warm = LabAtmosphere.Evening > 0 ? new Color(1f, 0.6f, 0.38f) : new Color(1f, 0.89f, 0.66f);
            for (int i = 0; i < Beams.Count; i++)
            {
                warm.a = Beams[i].strength * sun * (0.85f + 0.15f * Mathf.Sin(t * 0.35f + i));
                Beams[i].art.color = warm;
            }
            var dusk = Color.Lerp(Color.white, new Color(0.38f, 0.4f, 0.6f), Mathf.Max(LabAtmosphere.Evening, SchoolCalendar.Raining ? 0.45f : 0));
            foreach (var view in Views) view.color = dusk;
            foreach (var (tr, home, phase) in swaying)
                tr.localPosition = home + new Vector3(Mathf.Round(Mathf.Sin(t * 0.7f + phase) * 0.8f) / 16f, 0, 0);
            foreach (var (art, color, period) in blinkers)
                art.color = Mathf.Repeat(t, period) < 0.18f ? color : color * new Color(0.35f, 0.35f, 0.35f, 1);
        }
    }
}
