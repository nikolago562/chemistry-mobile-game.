using UnityEngine;

namespace ChemistryGame
{
    /// <summary>A small set of working benches, storage and glassware with open aisles.</summary>
    public sealed class ChemistryClassroom : MonoBehaviour
    {
        public Sprite storageCupboard;
        private static Sprite bench, sinkBench, rack, smallFlask;
        private static PhysicsMaterial2D benchMaterial;
        public void Build(LabGame game)
        {
            DepthOccluders.Clear();
            var furniture = new GameObject("ქიმიის ოთახის ავეჯი");
            furniture.transform.SetParent(game.classroom, false);
            MakeBench(furniture.transform, new Vector2(36, 9.2f), false);
            MakeBench(furniture.transform, new Vector2(55, 9.2f), true);
            foreach (float y in new[] { 2f, -6f, -14f })
            foreach (float x in new[] { 37.5f, 52f })
                MakeBench(furniture.transform, new Vector2(x, y), false);
            // The advanced challenge desk sits apart, near the front, as a later goal.
            MakeBench(furniture.transform, new Vector2(37.5f, -21f), false);
            LaboratoryArt.Draw(furniture.transform, "გამოწვევების თასი", new Vector2(36.4f, -19.45f), LaboratoryArt.Device, 7);

            Glassware(furniture.transform, new Vector2(35, 10.65f), true);
            Glassware(furniture.transform, new Vector2(36.6f, -4.55f), false);
            Glassware(furniture.transform, new Vector2(50.8f, 3.45f), true);
            Glassware(furniture.transform, new Vector2(36.7f, 3.45f), false);
            Glassware(furniture.transform, new Vector2(51.3f, -4.55f), true);
            Glassware(furniture.transform, new Vector2(36.6f, -12.55f), true);
            Glassware(furniture.transform, new Vector2(53.1f, -12.55f), false);
            Cabinet(furniture.transform, new Vector2(63, -9), "ლაბორატორიის მარაგი");
            LaboratoryArt.Draw(furniture.transform, "საზომი ხელსაწყო", new Vector2(54.5f, 10.7f), LaboratoryArt.Device, 7);
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
            go.GetComponent<SpriteRenderer>().sprite = LaboratoryArt.Bench();
            go.GetComponent<SpriteRenderer>().sortingOrder = 6;
            LaboratoryArt.CompleteBench(go.GetComponent<SpriteRenderer>());
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

        private static Sprite Bench(bool sink)
        {
            if (sink && sinkBench != null) return sinkBench;
            if (!sink && bench != null) return bench;
            var sprite = PixelArt.Make(64, 28, (x, y) =>
            {
                if (x < 1 || x > 62 || y < 1) return Color.clear;
                if (y <= 3) return new Color32(44, 63, 65, 255);
                if (y < 19)
                {
                    if (x < 3 || x > 60 || x == 31 || x == 32) return new Color32(57, 87, 88, 255);
                    if (y == 14 && (x == 25 || x == 38)) return new Color32(217, 205, 164, 255);
                    return x < 32 ? new Color32(103, 144, 143, 255) : new Color32(88, 129, 131, 255);
                }
                if (sink && x >= 43 && x <= 56 && y >= 22 && y <= 25)
                    return x == 43 || x == 56 || y == 22 || y == 25
                        ? new Color32(130, 157, 162, 255) : new Color32(72, 103, 117, 255);
                if (sink && x >= 48 && x <= 50 && y >= 26) return new Color32(100, 126, 138, 255);
                if (y == 19 || y == 20 || x == 1 || x == 62) return new Color32(126, 139, 132, 255);
                return new Color32(226, 228, 210, 255);
            }, new Vector2(0.5f, 0));
            if (sink) sinkBench = sprite;
            else bench = sprite;
            return sprite;
        }

        private static Sprite Rack() => rack ??= PixelArt.Make(18, 12, (x, y) =>
        {
            if (y <= 2 && x > 0 && x < 17 || y == 5 && x > 1 && x < 16) return new Color32(115, 85, 61, 255);
            int tube = x % 5;
            if (x < 2 || x > 15 || tube < 2 || tube > 3 || y < 3) return Color.clear;
            if (y > 8) return new Color32(189, 218, 218, 255);
            return x < 6 ? new Color32(111, 178, 157, 255)
                : x < 11 ? new Color32(125, 158, 199, 255) : new Color32(210, 166, 104, 255);
        }, new Vector2(0.5f, 0));

        private static Sprite SmallFlask() => smallFlask ??= PixelArt.Make(10, 12, (x, y) =>
        {
            bool neck = x >= 4 && x <= 5 && y >= 7;
            bool body = y >= 1 && y <= 7 && Mathf.Abs(x - 4.5f) < (10 - y) * 0.5f;
            if (!neck && !body) return Color.clear;
            return y < 5 ? new Color32(99, 170, 154, 255) : new Color32(196, 222, 218, 255);
        }, new Vector2(0.5f, 0));
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
