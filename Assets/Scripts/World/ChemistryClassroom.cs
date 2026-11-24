using UnityEngine;

namespace ChemistryGame
{
    /// <summary>A small set of working benches, storage and glassware with open aisles.</summary>
    public sealed class ChemistryClassroom : MonoBehaviour
    {
        public Sprite storageCupboard;
        private static PhysicsMaterial2D benchMaterial;
        public void Build(LabGame game)
        {
            DepthOccluders.Clear();
            var furniture = new GameObject("ქიმიის ოთახის ავეჯი");
            furniture.transform.SetParent(game.classroom, false);
            MakeBench(furniture.transform, SchoolLayout.TeacherBench, false);
            // The lesson table stands right of the teacher's own desk along the back wall, so
            // talking to her and starting a lesson are two separate spots.
            MakeBench(furniture.transform, SchoolLayout.LessonBench, false);
            Glassware(furniture.transform, SchoolLayout.LessonBench + new Vector2(-1, 1.45f), false);
            MakeBench(furniture.transform, SchoolLayout.SinkBench, true);
            foreach (var table in Tables) MakeBench(furniture.transform, table.at, false);
            // The advanced challenge desk sits apart, near the front, as a later goal.
            MakeBench(furniture.transform, SchoolLayout.QuizBench, false);
            LaboratoryArt.Draw(furniture.transform, "გამოწვევების თასი", SchoolLayout.QuizBench + new Vector2(-1.1f, 1.55f), LaboratoryArt.Device, 7);

            Glassware(furniture.transform, SchoolLayout.TeacherBench + new Vector2(-1, 1.45f), true);
            // A runner down the middle aisle, then each table's own mat, sign and signature prop.
            Mat(furniture.transform, new Vector2(45.5f, (SchoolLayout.FloorMinY + 8.6f) / 2), new Vector2(2.6f, 8.6f - SchoolLayout.FloorMinY), new Color32(176, 110, 84, 255), 1);
            foreach (var (at, color, name, prop) in Tables)
            {
                Mat(furniture.transform, at + new Vector2(0, -0.7f), SchoolLayout.MatSize, color, 1);
                // No floating name plate: the animated prop says what the table is; the prompt shows in range.
                var art = LaboratoryArt.Draw(furniture.transform, name + " • ნიშანი", at + new Vector2(0, 1.5f), Prop(prop), 8);
                art.gameObject.AddComponent<StationIdle>().Kind = prop;
            }
            Cabinet(furniture.transform, SchoolLayout.SupplyCabinet, "ლაბორატორიის მარაგი");
            LaboratoryArt.Draw(furniture.transform, "საზომი ხელსაწყო", SchoolLayout.SinkBench + new Vector2(-0.5f, 1.5f), LaboratoryArt.Device, 7);
            AcademyArt.Dress(game, furniture.transform);
        }

        /// <summary>Each table's bench position, colour (its chapter's), sign text and signature prop.</summary>
        private static readonly (Vector2 at, Color32 color, string name, int prop)[] Tables =
        {
            (SchoolLayout.AtomBench, new Color32(70, 140, 210, 255), "ატომის სამჭედლო", 0),
            (SchoolLayout.ElementBench, new Color32(150, 100, 200, 255), "ელემენტები", 1),
            (SchoolLayout.MixBench, new Color32(220, 90, 90, 255), "შერევის მაგიდა", 2),
            (SchoolLayout.MoleculeBench, new Color32(40, 160, 160, 255), "მოლეკულები", 3),
            (SchoolLayout.SolutionBench, new Color32(60, 170, 220, 255), "ხსნარები", 4),
            (SchoolLayout.PotionBench, new Color32(210, 90, 160, 255), "pH ნაყენები", 5),
        };
        private static readonly Sprite[] props = new Sprite[6];

        /// <summary>A woven rug under a table, its colour softened toward cream so the room stays warm.</summary>
        private static void Mat(Transform parent, Vector2 centre, Vector2 size, Color color, int order)
        {
            var art = LaboratoryArt.Draw(parent, "ხალიჩა", centre, AcademyArt.Rug(), order);
            art.color = Color.Lerp(color, new Color(0.96f, 0.9f, 0.8f), 0.35f);
            art.drawMode = SpriteDrawMode.Tiled;
            art.size = size;
        }

        /// <summary>Signature props drawn in pixels: atom model, element chart, bubbling flask, ball-and-stick water, beakers, pH tubes.</summary>
        private static Color C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        private static Sprite Prop(int kind)
        {
            if (props[kind] != null) return props[kind];
            Color ink = C(40, 46, 56, 255), glass = C(200, 226, 236, 255);
            Color[] rainbow = { C(220, 50, 70, 255), C(240, 120, 150, 255), C(150, 80, 180, 255), C(80, 110, 220, 255), C(60, 170, 110, 255), C(230, 210, 60, 255) };
            System.Func<int, int, Color> draw = kind switch
            {
                0 => (x, y) =>
                {
                    float dx = x - 11.5f, dy = y - 11.5f;
                    if (dx * dx + dy * dy < 7) return (x + y) % 2 == 0 ? C(224, 96, 90, 255) : C(180, 186, 196, 255);
                    float a = (dx * dx) / 110f + (dy * dy) / 18f, b = (dx * dx) / 18f + (dy * dy) / 110f;
                    if ((x == 21 && y == 12) || (x == 11 && y == 21)) return C(90, 170, 240, 255);
                    return Mathf.Abs(a - 1) < 0.14f || Mathf.Abs(b - 1) < 0.14f ? C(140, 200, 250, 255) : Color.clear;
                },
                1 => (x, y) =>
                {
                    if (y < 3) return x >= 10 && x <= 13 ? ink : Color.clear;
                    if (x < 1 || x > 22 || y > 17) return Color.clear;
                    if (x == 1 || x == 22 || y == 3 || y == 17) return ink;
                    bool cell = (x - 2) % 3 != 2 && (y - 4) % 3 != 2;
                    bool used = y >= 13 ? x <= 4 || x >= 19 : y >= 10 ? x <= 7 || x >= 14 : true;
                    return cell && used ? rainbow[((x - 2) / 3 + (y - 4) / 3) % 6] : C(236, 232, 214, 255);
                },
                2 => (x, y) =>
                {
                    float dx = x - 8, dy = y - 7;
                    bool body = dx * dx + dy * dy < 36, neck = x >= 6 && x <= 9 && y >= 12 && y <= 17;
                    if ((x == 4 && y == 16) || (x == 11 && y == 19) || (x == 7 && y == 21)) return C(150, 240, 160, 200);
                    if (!body && !neck) return Color.clear;
                    bool edge = body ? dx * dx + dy * dy > 26 : x == 6 || x == 9;
                    if (edge) return ink;
                    return y < 9 ? C(90, 210, 120, 255) : glass;
                },
                3 => (x, y) =>
                {
                    float ox = x - 11.5f, oy = y - 6, h1x = x - 3.5f, h1y = y - 12, h2x = x - 19.5f, h2y = y - 12;
                    if (ox * ox + oy * oy < 20) return ox * ox + oy * oy > 15 ? ink : C(230, 80, 80, 255);
                    if (h1x * h1x + h1y * h1y < 11 || h2x * h2x + h2y * h2y < 11) return h1x * h1x + h1y * h1y > 7 && h2x * h2x + h2y * h2y > 7 ? ink : C(240, 240, 248, 255);
                    bool stick = Mathf.Abs((y - 6) - (11.5f - x) * 0.75f) < 0.8f && x > 5 && x < 10 || Mathf.Abs((y - 6) - (x - 11.5f) * 0.75f) < 0.8f && x > 13 && x < 18;
                    return stick ? C(190, 190, 200, 255) : Color.clear;
                },
                4 => (x, y) =>
                {
                    int cup = x / 8, local = x % 8;
                    if (local == 0 || local == 7 || y > 11) return Color.clear;
                    if (local == 1 || local == 6 || y == 0) return ink;
                    Color[] fill = { C(70, 150, 225, 255), C(240, 200, 80, 255), C(120, 210, 170, 255) };
                    return y <= 4 + cup * 2 ? fill[cup] : glass;
                },
                _ => (x, y) =>
                {
                    if (y <= 1) return C(150, 110, 80, 255);
                    if (y == 7 && x >= 1 && x <= 22) return C(150, 110, 80, 255);
                    int tube = x / 4, local = x % 4;
                    if (local == 0 || local == 3 || y > 13) return Color.clear;
                    return y <= 9 ? rainbow[tube] : glass;
                },
            };
            int w = kind == 2 ? 16 : 24, h = kind == 2 ? 22 : kind == 4 ? 12 : kind == 5 ? 14 : kind == 3 ? 16 : kind == 1 ? 18 : 24;
            return props[kind] = PixelArt.Make(w, h, draw, new Vector2(0.5f, 0), 16);
        }

        private void Cabinet(Transform parent, Vector2 point, string label)
        {
            var go = new GameObject(label, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = point;
            go.GetComponent<SpriteRenderer>().sprite = LaboratoryArt.Locker;
            go.GetComponent<SpriteRenderer>().sortingOrder = 6;
            var shape = go.GetComponent<BoxCollider2D>();
            shape.offset = new Vector2(0, 0.5f);
            shape.size = new Vector2(1.8f, 0.9f);
            DepthOccluders.Add(go.transform.position, 1.35f, 0.6f, 4.2f);
        }

        private static void MakeBench(Transform parent, Vector2 point, bool sink)
        {
            var go = new GameObject(sink ? "ნიჟარიანი დახლი" : "ლაბორატორიული მაგიდა", typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = point;
            go.GetComponent<SpriteRenderer>().sprite = AcademyArt.Desk(sink);
            go.GetComponent<SpriteRenderer>().sortingOrder = 6;
            AcademyArt.ContactShadow(parent, point, 4.4f);
            var shape = go.GetComponent<BoxCollider2D>();
            // The bench's floor footprint runs from the cabinet's front foot to the back edge of
            // the countertop, so the player can never stand on the drawn tabletop from behind.
            // Rounded, inset edges let the player's feet slide past the corners.
            shape.offset = new Vector2(0, 1.0f);
            shape.size = new Vector2(3.65f, 1.75f);
            DepthOccluders.Add(go.transform.position, 2.35f, 1.0f, 4.6f);
            shape.edgeRadius = 0.08f;
            if (benchMaterial == null)
                benchMaterial = new PhysicsMaterial2D("Lab bench") { friction = 0, bounciness = 0 };
            shape.sharedMaterial = benchMaterial;
        }

        private static void Glassware(Transform parent, Vector2 point, bool tubes)
        {
            var go = new GameObject(tubes ? "სინჯარების შტატივი" : "საზომი კოლბა", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = point;
            go.GetComponent<SpriteRenderer>().sprite = tubes ? LaboratoryArt.Glassware : LaboratoryArt.Flask;
            go.GetComponent<SpriteRenderer>().sortingOrder = 7;
        }
    }

    /// <summary>Small idle motion on a table's signature prop: orbiting electrons, rising bubbles, shifting pH colours, a bobbing molecule, a twinkle.</summary>
    public sealed class StationIdle : MonoBehaviour
    {
        public int Kind;
        private SpriteRenderer art;
        private readonly SpriteRenderer[] dots = new SpriteRenderer[3];
        private Vector3 home;
        private static Sprite dot;
        private void Start()
        {
            art = GetComponent<SpriteRenderer>();
            home = transform.localPosition;
            dot ??= PixelArt.Make(2, 2, (x, y) => Color.white, new Vector2(0.5f, 0.5f), 16);
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i] = LaboratoryArt.Draw(transform, "ნაწილაკი", Vector2.zero, dot, art.sortingOrder + 1);
                dots[i].color = Color.clear;
            }
        }
        private void Update()
        {
            float t = Time.time + home.x * 0.37f;
            float h = art.sprite.bounds.size.y;
            for (int i = 0; i < dots.Length; i++)
            {
                float u = (t * 0.7f + i / 3f) % 1;
                switch (Kind)
                {
                    case 0: // electrons on the two orbits
                        float a = t * 2.4f + i * 2.1f;
                        dots[i].transform.localPosition = i % 2 == 0 ? new Vector3(Mathf.Cos(a) * 0.62f, h / 2 + Mathf.Sin(a) * 0.24f) : new Vector3(Mathf.Cos(a) * 0.24f, h / 2 + Mathf.Sin(a) * 0.62f);
                        dots[i].color = new Color(0.55f, 0.85f, 1, i < 2 ? 1 : 0);
                        break;
                    case 2: case 4: // bubbles rising out of the glass
                        dots[i].transform.localPosition = new Vector3((i - 1) * 0.22f + Mathf.Sin(u * 9 + i) * 0.05f, h * 0.7f + u * 0.8f);
                        dots[i].color = new Color(0.75f, 1, 0.85f, Mathf.Sin(u * Mathf.PI) * 0.9f);
                        break;
                    case 1: // a twinkle over the chart
                        dots[i].transform.localPosition = new Vector3((Mathf.Repeat(i * 0.37f + Mathf.Floor(t * 0.7f + i / 3f) * 0.61f, 1) - 0.5f) * 1.2f, h * (0.35f + i * 0.2f));
                        dots[i].color = new Color(1, 0.95f, 0.7f, Mathf.Max(0, Mathf.Sin(u * Mathf.PI * 2)));
                        break;
                    default: dots[i].color = Color.clear; break;
                }
            }
            // The molecule bobs as if its bonds were springs; the pH tubes slowly shift colour.
            if (Kind == 3) transform.localPosition = home + Vector3.up * Mathf.Round(Mathf.Sin(t * 2.2f) * 1.5f) / 16f;
            if (Kind == 5) art.color = Color.HSVToRGB(Mathf.Repeat(t * 0.08f, 1), 0.18f, 1);
        }
    }

    /// <summary>
    /// Floor areas behind tall furniture. While the player's feet are inside one, the player is
    /// drawn underneath the furniture so it reads as standing behind it, not on top of it.
    /// </summary>
    public static class DepthOccluders
    {
        private struct Zone { public Vector2 Base; public float HalfWidth, From, To; }
        private static readonly System.Collections.Generic.List<Zone> zones = new System.Collections.Generic.List<Zone>();
        public static void Clear() => zones.Clear();
        public static void Add(Vector2 worldBase, float halfWidth, float from, float to) =>
            zones.Add(new Zone { Base = worldBase, HalfWidth = halfWidth, From = from, To = to });
        public static bool Behind(Vector2 feet)
        {
            foreach (var zone in zones)
                if (Mathf.Abs(feet.x - zone.Base.x) < zone.HalfWidth &&
                    feet.y > zone.Base.y + zone.From && feet.y < zone.Base.y + zone.To) return true;
            return false;
        }
    }
}
