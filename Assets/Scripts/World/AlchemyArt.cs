using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// The lived-in alchemy layer of the classroom: apparatus racks with coiled glass and gauges, floor reagent
    /// shelves, pendant lamps, bench-top clutter (notebooks, mortars, burners, specimen jars, little flasks) and
    /// faint stains. Wall and bench-top things have no colliders; floor pieces stand in the side lanes with
    /// the same small footprint as other furniture, so the aisles stay smooth.
    /// </summary>
    public static class AlchemyArt
    {
        private static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        private static readonly Color32 Ink = C(30, 24, 30), Brass = C(196, 150, 72), BrassDark = C(140, 100, 46),
            Iron = C(88, 92, 100), IronLight = C(132, 138, 148), Glass = C(214, 236, 240), GlassEdge = C(150, 190, 200);
        private static Sprite Paint(string key, int w, int h, System.Action<Px> draw) => DecorArt.Paint("alchemy:" + key, w, h, draw);

        /// <summary>An iron apparatus rack: a coiled glass condenser, two brass gauges and a row of coloured flasks.</summary>
        public static Sprite Rack => Paint("rack", 24, 40, p =>
        {
            p.Fill(1, 0, 2, 39, Ink); p.Fill(21, 0, 22, 39, Ink);
            p.Fill(2, 0, 2, 38, Iron); p.Fill(21, 0, 21, 38, IronLight);
            foreach (int y in new[] { 1, 13, 26 }) { p.Fill(1, y, 22, y + 1, Ink); p.Fill(2, y + 1, 21, y + 1, IronLight); }
            // coiled condenser between the middle and top shelves
            for (int y = 15; y <= 24; y++)
            {
                int x = 6 + Mathf.RoundToInt(Mathf.Sin(y * 1.2f) * 3);
                p.Dot(x + 5, y, GlassEdge); p.Dot(x + 6, y, Glass); p.Dot(x + 7, y, C(120, 210, 170));
            }
            p.Fill(11, 25, 12, 26, GlassEdge);
            // two gauges
            foreach (var (cx, needle) in new[] { (6, 1), (17, -1) })
            {
                p.Disc(cx, 31.5f, 3.6f, Ink); p.Disc(cx, 31.5f, 2.7f, C(244, 236, 214));
                p.Dot(cx, 32, Ink); p.Dot(cx + needle, 33, C(200, 60, 50)); p.Dot(cx + needle * 2, 34, C(200, 60, 50));
                p.Fill(cx - 1, 27, cx + 1, 27, Brass);
            }
            // flasks on the bottom shelf
            Color32[] liquids = { C(240, 110, 150), C(120, 200, 240), C(250, 210, 90), C(140, 220, 130) };
            for (int i = 0; i < 4; i++)
            {
                int x0 = 3 + i * 5;
                p.Fill(x0, 3, x0 + 3, 7, Ink); p.Fill(x0 + 1, 3, x0 + 2, 6, liquids[i]); p.Fill(x0 + 1, 8, x0 + 2, 10, GlassEdge);
            }
            // a brass burner and a stand on the middle shelf
            p.Fill(4, 15, 6, 18, BrassDark); p.Fill(4, 19, 6, 19, Brass); p.Dot(5, 20, C(120, 180, 255)); p.Dot(5, 21, C(255, 200, 90));
            p.Fill(16, 15, 19, 15, Ink); p.Fill(17, 16, 18, 20, C(180, 120, 220)); p.Fill(16, 21, 19, 21, Ink);
        });

        /// <summary>A warm pendant lamp on a chain (pivot at the ceiling hook).</summary>
        public static Sprite Pendant => DecorArt.Paint("alchemy:pendant", 10, 22, p =>
        {
            for (int y = 9; y < 22; y++) if (y % 2 == 0) p.Dot(5, y, Iron);
            p.Fill(1, 4, 8, 8, Ink); p.Fill(2, 5, 7, 8, Brass); p.Fill(2, 8, 7, 8, BrassDark);
            p.Fill(3, 1, 6, 4, C(255, 214, 140)); p.Fill(4, 0, 5, 1, C(255, 240, 200)); p.Dot(2, 4, Ink); p.Dot(7, 4, Ink);
        }, 16, new Vector2(0.5f, 1));

        public static Sprite Mortar => Paint("mortar", 10, 8, p =>
        {
            p.Fill(1, 0, 8, 4, Ink); p.Fill(2, 1, 7, 4, C(208, 200, 188)); p.Fill(2, 4, 7, 4, C(236, 230, 220));
            p.Fill(0, 4, 9, 4, Ink); p.Fill(3, 5, 6, 5, C(170, 200, 120));
            p.Fill(6, 5, 7, 7, C(120, 96, 70)); p.Dot(8, 7, C(120, 96, 70));
        });

        public static Sprite Burner => Paint("burner", 7, 11, p =>
        {
            p.Fill(0, 0, 6, 1, Ink); p.Fill(1, 1, 5, 1, BrassDark); p.Fill(2, 2, 4, 6, Ink); p.Fill(3, 2, 3, 6, Brass);
            p.Fill(2, 7, 4, 8, C(120, 180, 255)); p.Dot(3, 9, C(255, 220, 120)); p.Dot(3, 10, C(255, 160, 80));
        });

        /// <summary>A specimen jar with something curled up inside.</summary>
        public static Sprite Specimen(int kind) => Paint("specimen" + kind, 8, 11, p =>
        {
            Color32 fluid = kind == 0 ? C(190, 220, 140) : kind == 1 ? C(240, 200, 150) : C(170, 200, 240);
            p.Fill(0, 0, 7, 8, Ink); p.Fill(1, 1, 6, 7, fluid); p.Fill(1, 6, 6, 7, Glass);
            p.Fill(1, 9, 6, 10, Ink); p.Fill(2, 9, 5, 9, kind == 2 ? Brass : C(150, 96, 60));
            if (kind == 0) { p.Dot(3, 3, C(80, 130, 70)); p.Dot(4, 4, C(80, 130, 70)); p.Dot(3, 5, C(80, 130, 70)); }
            else if (kind == 1) { p.Fill(2, 2, 4, 3, C(200, 110, 90)); p.Dot(5, 4, C(200, 110, 90)); }
            else { p.Disc(3.5f, 3.5f, 1.4f, C(250, 250, 255)); p.Dot(3, 3, Ink); }
            p.Dot(1, 2, C(255, 255, 255, 160));
        });

        /// <summary>Adds the alchemy layer to the classroom (all positions from <see cref="SchoolLayout"/>).</summary>
        public static void Dress(Transform root, AcademyAmbience ambience, System.Func<Transform, string, Vector2, Sprite, float, SpriteRenderer> furniture)
        {
            var atmosphere = LabAtmosphere.Instance;
            SpriteRenderer Draw(string name, Vector2 at, Sprite sprite, int order) => LaboratoryArt.Draw(root, name, at, sprite, order);

            // Floor pieces along the side walls (solid feet, depth zones, shadows).
            furniture(root, "აპარატურის თარო", new Vector2(SchoolLayout.LeftWall + 0.1f, -1.0f), Rack, 1.4f);
            furniture(root, "რეაქტივების თარო", new Vector2(SchoolLayout.LeftWall, -6.7f), DecorArt.Shelf(0), 1.8f);
            furniture(root, "აპარატურის თარო", new Vector2(SchoolLayout.RightWall + 0.1f, -11.0f), Rack, 1.4f);
            furniture(root, "რეაქტივების თარო", SchoolLayout.FrontShelf, DecorArt.Shelf(2), 1.8f);

            // Pendant lamps hang in front of the back wall between its pictures; they glow warmly all day.
            foreach (float x in SchoolLayout.Pendants)
            {
                ambience.Sway(Draw("ჩამოკიდებული ლამპა", new Vector2(x, 15.2f), Pendant, 5).transform);
                atmosphere?.AddGlow(root, new Vector2(x, 13.95f), new Color(1f, 0.82f, 0.52f), 1.5f, false);
            }

            // Bench-top clutter: every working table gets a notebook stack on one end and glass or tools on the other.
            Sprite[] tools = { AcademyArt.Microscope, Mortar, Burner, Specimen(0), Specimen(1), Specimen(2) };
            Color[] liquids = { new Color(0.95f, 0.45f, 0.6f), new Color(0.45f, 0.75f, 0.95f), new Color(0.98f, 0.82f, 0.35f), new Color(0.55f, 0.88f, 0.5f) };
            var benches = new[] { SchoolLayout.AtomBench, SchoolLayout.PotionBench, SchoolLayout.MixBench, SchoolLayout.MoleculeBench,
                SchoolLayout.SolutionBench, SchoolLayout.ElementBench, SchoolLayout.QuizBench, SchoolLayout.LessonBench };
            for (int i = 0; i < benches.Length; i++)
            {
                var top = benches[i] + new Vector2(0, 1.42f);
                bool flip = i % 2 == 1;
                Draw("სქელი რვეულები", top + new Vector2(flip ? 1.35f : -1.35f, 0), RoomArt.BookStack, 7);
                Draw("ხელსაწყო", top + new Vector2(flip ? -1.45f : 1.45f, 0), tools[i % tools.Length], 7);
                Draw("კოლბა", top + new Vector2(flip ? -0.85f : 0.85f, 0), Glassware.Get((GlassShape)(i % 3), liquids[i % liquids.Length], 0.5f, 26), 7);
            }
            // The side tables and the sink get their own odds and ends.
            Draw("ნიმუშის ქილა", SchoolLayout.SinkBench + new Vector2(1.5f, 1.42f), Specimen(2), 7);
            Draw("ნიმუშის ქილა", SchoolLayout.TeacherBench + new Vector2(0.2f, 1.42f), Specimen(0), 7);

            // Faint stains where experiments happen: a pink splash, a blue drip and a dusting of white powder.
            foreach (var (at, color) in new[]
            {
                (SchoolLayout.MixBench + new Vector2(2.1f, -1.9f), new Color(0.9f, 0.45f, 0.55f, 0.28f)),
                (SchoolLayout.SolutionBench + new Vector2(-2.0f, -1.6f), new Color(0.4f, 0.6f, 0.9f, 0.25f)),
                (SchoolLayout.AtomBench + new Vector2(2.2f, -2.1f), new Color(1f, 1f, 0.95f, 0.3f)),
                (SchoolLayout.ElementBench + new Vector2(-2.2f, -1.8f), new Color(0.65f, 0.5f, 0.85f, 0.22f)),
            })
                Draw("ლაქა", at, DecorArt.Puddle(Mathf.Abs((int)(at.x * 7)) % 2), 2).color = color;
        }
    }
}
