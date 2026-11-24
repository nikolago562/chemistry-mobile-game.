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
            MakeBench(furniture.transform, new Vector2(36, 9.2f), false);
            // The lesson table stands right of the teacher's own desk along the back wall, so
            // talking to her and starting a lesson are two separate spots.
            MakeBench(furniture.transform, new Vector2(40.3f, 9.2f), false);
            Glassware(furniture.transform, new Vector2(39.3f, 10.65f), false);
            MakeBench(furniture.transform, new Vector2(55, 9.2f), true);
            foreach (float y in new[] { 2f, -6f, -14f })
            foreach (float x in new[] { 37.5f, 52f })
                MakeBench(furniture.transform, new Vector2(x, y), false);
            // The advanced challenge desk sits apart, near the front, as a later goal.
            MakeBench(furniture.transform, new Vector2(37.5f, -21f), false);
            LaboratoryArt.Draw(furniture.transform, "გამოწვევების თასი", new Vector2(36.4f, -19.45f), LaboratoryArt.Device, 7);

            Glassware(furniture.transform, new Vector2(35, 10.65f), true);
            // A runner down the middle aisle, then each table's own mat, sign and signature prop.
            Mat(furniture.transform, new Vector2(44.75f, -7), new Vector2(3, 32), new Color32(176, 110, 84, 255), 1);
            foreach (var (at, color, name, prop) in Tables)
            {
                Mat(furniture.transform, at + new Vector2(0, -0.7f), new Vector2(6.5f, 4.6f), color, 1);
                Sign(furniture.transform, at + new Vector2(0, 2.75f), name, color);
                LaboratoryArt.Draw(furniture.transform, name + " • ნიშანი", at + new Vector2(0, 1.5f), Prop(prop), 8);
            }
            Cabinet(furniture.transform, new Vector2(63, -9), "ლაბორატორიის მარაგი");
            LaboratoryArt.Draw(furniture.transform, "საზომი ხელსაწყო", new Vector2(54.5f, 10.7f), LaboratoryArt.Device, 7);
            AcademyArt.Dress(game, furniture.transform);
        }

        /// <summary>Each table's bench position, colour (its chapter's), sign text and signature prop.</summary>
        private static readonly (Vector2 at, Color32 color, string name, int prop)[] Tables =
        {
            (new Vector2(37.5f, 2), new Color32(70, 140, 210, 255), "ატომის სამჭედლო", 0),
            (new Vector2(52, -14), new Color32(150, 100, 200, 255), "ელემენტები", 1),
            (new Vector2(37.5f, -6), new Color32(220, 90, 90, 255), "შერევის მაგიდა", 2),
            (new Vector2(52, -6), new Color32(40, 160, 160, 255), "მოლეკულები", 3),
            (new Vector2(37.5f, -14), new Color32(60, 170, 220, 255), "ხსნარები", 4),
            (new Vector2(52, 2), new Color32(210, 90, 160, 255), "pH ნაყენები", 5),
        };
        private static readonly Sprite[] props = new Sprite[6];
        private static Sprite signSprite;

        /// <summary>A woven rug under a table, its colour softened toward cream so the room stays warm.</summary>
        private static void Mat(Transform parent, Vector2 centre, Vector2 size, Color color, int order)
        {
            var art = LaboratoryArt.Draw(parent, "ხალიჩა", centre, AcademyArt.Rug(), order);
            art.color = Color.Lerp(color, new Color(0.96f, 0.9f, 0.8f), 0.35f);
            art.drawMode = SpriteDrawMode.Tiled;
            art.size = size;
        }

        /// <summary>A hanging name plate in the table's colour with an outlined label.</summary>
        private static void Sign(Transform parent, Vector2 point, string text, Color color)
        {
            signSprite ??= Sliced(16, 8, (x, y) =>
            {
                if ((x == 0 || x == 15) && (y == 0 || y == 7)) return Color.clear;
                if (x == 0 || x == 15 || y == 0 || y == 7) return new Color(0.25f, 0.25f, 0.28f);
                return y == 1 ? new Color(0.75f, 0.75f, 0.75f) : Color.white;
            });
            var plate = LaboratoryArt.Draw(parent, text + " • დაფა", point, signSprite, 9);
            plate.color = color;
            plate.drawMode = SpriteDrawMode.Sliced;
            plate.size = new Vector2(3.6f, 0.8f);
            foreach (float side in new[] { -1.4f, 1.4f })
                LaboratoryArt.Draw(plate.transform, "ჯაჭვი", new Vector2(side, 0.4f), Chain, 9);
            PersonalLab.Label(plate.transform, text, new Vector2(0, 0.02f), Color.white);
        }
        private static Sprite Sliced(int w, int h, System.Func<int, int, Color> draw)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) texture.SetPixel(x, y, draw(x, y));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16, 0, SpriteMeshType.FullRect, Vector4.one * 3);
        }
        private static Sprite chain;
        private static Sprite Chain => chain ??= PixelArt.Make(1, 12, (x, y) => y % 3 == 0 ? Color.clear : new Color(0.3f, 0.3f, 0.32f), new Vector2(0.5f, 0), 16);

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
