using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// The personal lab's cozy look, painted in code: a deep navy pegboard wall with a bright trim,
    /// warm cream floor tiles, honey-wood furniture, a copper boiler, books and glowing glassware.
    /// </summary>
    public static class CozyLabArt
    {
        // Palette: night-blue wall, honey wood, copper, cream tiles.
        public static readonly Color32 Ink = new Color32(26, 22, 34, 255),
            Wall = new Color32(30, 40, 96, 255), WallDeep = new Color32(22, 30, 74, 255), Peg = new Color32(48, 64, 138, 255),
            Trim = new Color32(70, 104, 214, 255), TrimLight = new Color32(132, 170, 255, 255),
            WoodTop = new Color32(222, 168, 100, 255), Wood = new Color32(184, 124, 70, 255), WoodShade = new Color32(140, 88, 50, 255), WoodDeep = new Color32(98, 58, 34, 255),
            Copper = new Color32(212, 118, 58, 255), CopperLight = new Color32(246, 178, 108, 255), CopperDark = new Color32(150, 72, 38, 255),
            Tile = new Color32(236, 214, 178, 255), TileShade = new Color32(222, 196, 158, 255), Grout = new Color32(200, 168, 130, 255), Speck = new Color32(206, 176, 136, 255);

        private static Sprite Paint(string key, int w, int h, System.Action<Px> draw, Vector2? pivot = null) =>
            DecorArt.Paint("cozy:" + key, w, h, draw, 16, pivot);

        /// <summary>One cream floor tile with soft grout and a few speckles (1 unit).</summary>
        public static Sprite Floor => Paint("floor", 16, 16, p =>
        {
            p.Fill(0, 0, 15, 15, Tile);
            p.Fill(0, 0, 15, 0, Grout); p.Fill(0, 0, 0, 15, Grout);
            p.Fill(1, 1, 15, 1, TileShade); p.Fill(15, 1, 15, 15, TileShade);
            foreach (var (x, y) in new[] { (4, 6), (11, 11), (8, 3), (13, 5), (5, 12) }) p.Dot(x, y, Speck);
        }, new Vector2(0.5f, 0));

        /// <summary>A 1x1 piece of the navy pegboard wall.</summary>
        public static Sprite WallPanel => Paint("wall", 16, 16, p =>
        {
            p.Fill(0, 0, 15, 15, Wall);
            for (int y = 2; y < 16; y += 4) for (int x = 2; x < 16; x += 4) p.Dot(x, y, Peg);
        }, new Vector2(0.5f, 0));
        /// <summary>The lower wall band behind the furniture, a shade darker.</summary>
        public static Sprite WallLow => Paint("wall-low", 16, 16, p =>
        {
            p.Fill(0, 0, 15, 15, WallDeep);
            p.Fill(0, 15, 15, 15, Trim); p.Fill(0, 14, 15, 14, WallDeep);
        }, new Vector2(0.5f, 0));
        /// <summary>The bright blue trim along the top of the wall.</summary>
        public static Sprite WallTop => Paint("wall-top", 16, 4, p =>
        {
            p.Fill(0, 0, 15, 3, Trim); p.Fill(0, 2, 15, 2, TrimLight); p.Fill(0, 0, 15, 0, WallDeep);
        }, new Vector2(0.5f, 0));
        /// <summary>The cut-away side and front walls: deep navy with a thin lit edge.</summary>
        public static Sprite SideWall => Paint("side", 16, 4, p => { p.Fill(0, 0, 15, 3, WallDeep); p.Fill(0, 3, 15, 3, Peg); }, new Vector2(0.5f, 0));
        public static Sprite Skirting => Paint("skirting", 16, 4, p => { p.Fill(0, 0, 15, 3, WoodDeep); p.Fill(0, 3, 15, 3, Wood); }, new Vector2(0.5f, 0));

        /// <summary>A 2-unit stretch of the long back counter: honey top, drawers with brass knobs.</summary>
        public static Sprite Counter => Paint("counter", 32, 24, p =>
        {
            p.Fill(0, 0, 31, 23, Ink);
            p.Fill(0, 1, 31, 17, Wood);
            p.Fill(0, 18, 31, 22, WoodTop); p.Fill(0, 22, 31, 22, new Color32(240, 196, 136, 255));
            p.Fill(0, 17, 31, 17, WoodDeep);
            p.Fill(0, 1, 31, 2, WoodShade);
            foreach (int x0 in new[] { 2, 17 })
            {
                p.Box(x0, 5, x0 + 12, 14, Wood, WoodDeep);
                p.Fill(x0 + 1, 13, x0 + 11, 13, WoodTop);
                p.Fill(x0 + 5, 9, x0 + 7, 10, DecorArt.Gold);
            }
        }, new Vector2(0.5f, 0));

        /// <summary>A wooden work table for lab equipment (3 units wide).</summary>
        public static Sprite Table => Paint("table", 48, 24, p =>
        {
            p.Fill(2, 0, 4, 10, WoodDeep); p.Fill(43, 0, 45, 10, WoodDeep);
            p.Fill(0, 10, 47, 23, Ink);
            p.Fill(1, 11, 46, 16, Wood);
            p.Fill(1, 17, 46, 22, WoodTop);
            for (int x = 8; x < 46; x += 10) p.Fill(x, 18, x, 21, new Color32(204, 148, 86, 255));
            p.Fill(1, 22, 46, 22, new Color32(240, 196, 136, 255));
            p.Fill(1, 16, 46, 16, WoodDeep);
            p.Box(14, 11, 33, 15, WoodShade, WoodDeep); p.Fill(22, 13, 25, 13, DecorArt.Gold);
        }, new Vector2(0.5f, 0));

        /// <summary>A copper boiler with a gauge panel, legs and a pipe up to a glass bulb.</summary>
        public static Sprite Boiler => Paint("boiler", 56, 48, p =>
        {
            // pipe from the tank up and over to the bulb
            p.Fill(40, 26, 43, 41, Ink); p.Fill(41, 26, 42, 40, CopperDark);
            p.Fill(30, 40, 43, 43, Ink); p.Fill(31, 41, 42, 42, Copper);
            p.Disc(26, 40, 6.5f, Ink); p.Disc(26, 40, 5.5f, new Color32(120, 190, 255, 255)); p.Disc(24.5f, 41.5f, 2, new Color32(220, 240, 255, 255));
            p.Fill(25, 31, 27, 34, Ink);
            // tank
            p.Fill(1, 6, 50, 28, Ink);
            p.Fill(2, 7, 49, 27, Copper);
            p.Fill(2, 23, 49, 26, CopperLight); p.Fill(2, 7, 49, 9, CopperDark);
            for (int x = 6; x < 49; x += 7) { p.Dot(x, 21, CopperDark); p.Dot(x, 12, CopperDark); }
            p.Box(12, 11, 30, 22, new Color32(98, 150, 160, 255), Ink);
            p.Disc(17, 17, 3, new Color32(240, 236, 220, 255)); p.Dot(17, 17, Ink); p.Dot(18, 18, Ink);
            p.Disc(25, 17, 3, new Color32(240, 236, 220, 255)); p.Dot(25, 17, Ink); p.Dot(24, 18, Ink);
            p.Fill(14, 12, 28, 12, new Color32(80, 220, 160, 255));
            // feet
            p.Fill(5, 0, 9, 6, Ink); p.Fill(6, 1, 8, 5, CopperDark);
            p.Fill(42, 0, 46, 6, Ink); p.Fill(43, 1, 45, 5, CopperDark);
        }, new Vector2(0.5f, 0));

        /// <summary>An arched wooden bookshelf full of colourful spines (2 x 3 units).</summary>
        public static Sprite Bookshelf => Paint("bookshelf", 32, 48, p =>
        {
            for (int y = 0; y < 48; y++) for (int x = 0; x < 32; x++)
            {
                float top = 38 + Mathf.Sqrt(Mathf.Max(0, 1 - Mathf.Pow((x - 15.5f) / 16f, 2))) * 9;
                if (y > top) continue;
                p.Dot(x, y, y > top - 1.2f || x == 0 || x == 31 ? Ink : WoodShade);
            }
            Color32[] spines = { new Color32(224, 92, 84, 255), new Color32(92, 170, 120, 255), new Color32(240, 196, 96, 255),
                new Color32(96, 140, 220, 255), new Color32(170, 110, 200, 255), new Color32(236, 236, 220, 255), new Color32(80, 200, 200, 255) };
            int k = 0;
            foreach (int shelf in new[] { 3, 15, 27 })
            {
                p.Fill(2, shelf - 2, 29, shelf - 1, WoodDeep);
                for (int x = 3; x < 29; )
                {
                    int w = 2 + (k * 7 % 3), h = 8 + (k * 5 % 3);
                    var c = spines[k % spines.Length];
                    p.Fill(x, shelf, x + w - 1, shelf + h, c);
                    p.Fill(x, shelf + h - 2, x + w - 1, shelf + h - 2, new Color32(255, 240, 200, 255));
                    x += w + (k % 4 == 3 ? 2 : 0); k++;
                }
            }
            p.Fill(2, 39, 29, 40, WoodDeep);
        }, new Vector2(0.5f, 0));

        public static Sprite Clock => Paint("clock", 16, 16, p =>
        {
            p.Disc(7.5f, 7.5f, 7.6f, Ink); p.Disc(7.5f, 7.5f, 6.5f, new Color32(120, 110, 230, 255)); p.Disc(7.5f, 7.5f, 5f, new Color32(230, 232, 255, 255));
            p.Fill(7, 7, 8, 11, Ink); p.Fill(7, 7, 10, 8, Ink);
        }, new Vector2(0.5f, 0.5f));

        /// <summary>A rounded lilac screen with chemistry doodles.</summary>
        public static Sprite Screen => Paint("screen", 36, 24, p =>
        {
            for (int y = 0; y < 24; y++) for (int x = 0; x < 36; x++)
            {
                bool corner = (x < 3 || x > 32) && (y < 3 || y > 20) && Vector2.Distance(new Vector2(x, y), new Vector2(x < 3 ? 3 : 32, y < 3 ? 3 : 20)) > 3;
                if (!corner) p.Dot(x, y, Ink);
            }
            p.Fill(2, 2, 33, 21, new Color32(130, 90, 220, 255));
            p.Fill(4, 4, 31, 19, new Color32(206, 196, 250, 255));
            for (int x = 6; x < 15; x += 2) p.Dot(x, 15, new Color32(90, 70, 160, 255));
            p.Fill(6, 12, 13, 12, new Color32(90, 70, 160, 255));
            for (int x = 19; x < 29; x++) p.Dot(x, 14 + (int)(Mathf.Sin(x * 0.9f) * 2), new Color32(230, 80, 110, 255));
            p.Fill(6, 8, 28, 8, new Color32(150, 130, 220, 255));
        }, new Vector2(0.5f, 0.5f));

        /// <summary>A little wall shelf of potion bottles.</summary>
        public static Sprite VialShelf => Paint("vials", 32, 20, p =>
        {
            p.Fill(0, 0, 31, 3, Ink); p.Fill(1, 1, 30, 2, Wood);
            Color32[] tints = { new Color32(140, 230, 170, 255), new Color32(250, 170, 200, 255), new Color32(150, 210, 250, 255), new Color32(250, 220, 130, 255), new Color32(200, 160, 250, 255) };
            for (int i = 0; i < 5; i++)
            {
                int x = 2 + i * 6, h = 8 + (i % 2) * 4;
                p.Fill(x, 4, x + 4, 4 + h, Ink);
                p.Fill(x + 1, 5, x + 3, 3 + h, new Color32(236, 246, 255, 255));
                p.Fill(x + 1, 5, x + 3, 4 + h / 2, tints[i]);
                p.Fill(x + 1, 5 + h, x + 3, 6 + h, new Color32(150, 100, 70, 255));
            }
        }, new Vector2(0.5f, 0));

        public static Sprite HangingPlant => Paint("plant", 20, 24, p =>
        {
            p.Fill(9, 17, 10, 23, Ink);
            p.Fill(4, 11, 15, 17, Ink); p.Fill(5, 12, 14, 16, DecorArt.Pot); p.Fill(5, 16, 14, 16, new Color32(220, 130, 96, 255));
            foreach (var (x, y, len) in new[] { (5, 11, 9), (8, 11, 6), (12, 11, 8), (15, 12, 10) })
                for (int i = 0; i < len; i++) { p.Dot(x + (i % 3 == 1 ? -1 : 0), y - i, i % 2 == 0 ? DecorArt.Leaf : DecorArt.LeafDark); p.Dot(x + 1, y - i, DecorArt.Leaf); }
        }, new Vector2(0.5f, 0));

        /// <summary>A few puffs that rise from a point and fade, looping (the boiler's steam).</summary>
        public static void Steam(Transform parent, Vector2 local, int order)
        {
            var root = new GameObject("ორთქლი"); root.transform.SetParent(parent, false); root.transform.localPosition = local;
            root.AddComponent<SteamPuffs>().Build(order);
        }

        private sealed class SteamPuffs : MonoBehaviour
        {
            private SpriteRenderer[] puffs;
            public void Build(int order)
            {
                puffs = new SpriteRenderer[4];
                for (int i = 0; i < puffs.Length; i++) puffs[i] = LaboratoryArt.Draw(transform, "ორთქლის ღრუბელი", Vector2.zero, DecorArt.Soft(), order);
            }
            private void Update()
            {
                for (int i = 0; i < puffs.Length; i++)
                {
                    float t = Mathf.Repeat(Time.time * 0.35f + i / (float)puffs.Length, 1);
                    puffs[i].transform.localPosition = new Vector3(Mathf.Sin(t * 5 + i) * 0.25f, t * 2.2f, 0);
                    puffs[i].transform.localScale = Vector3.one * (0.35f + t * 0.9f);
                    puffs[i].color = new Color(1, 1, 1, Mathf.Sin(t * Mathf.PI) * 0.45f);
                }
            }
        }
    }
}
