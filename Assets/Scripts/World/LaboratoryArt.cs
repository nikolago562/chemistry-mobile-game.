using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    // Clean pieces from ACTG's CC0 laboratory sheet. Coordinates are measured from the top left.
    public static class LaboratoryArt
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        public static Sprite Piece(string sheet, int x, int y, int width, int height, float ppu = 16)
        {
            string key = sheet + x + ":" + y + ":" + width + ":" + height + ":" + ppu;
            if (cache.TryGetValue(key, out var result) && result != null) return result;
            var texture = Resources.Load<Texture2D>("Laboratory/" + sheet);
            texture.filterMode = FilterMode.Point;
            result = Sprite.Create(texture, new Rect(x, texture.height - y - height, width, height), new Vector2(0.5f, 0), ppu);
            result.name = "ACTG_" + key;
            cache[key] = result;
            return result;
        }
        public static Sprite Floor => Piece("construction", 96, 32, 16, 16);
        public static Sprite Bench(bool small = false) => Piece("furniture_static", 160, 304, 32, 48, small ? 32 : 24);
        public static void CompleteBench(SpriteRenderer centre, bool small = false)
        {
            float ppu = small ? 32 : 24;
            Draw(centre.transform, "მაგიდის მარცხენა ნაწილი", new Vector2(-32 / ppu, 0), Piece("furniture_static", 96, 304, 32, 48, ppu), centre.sortingOrder);
            Draw(centre.transform, "მაგიდის მარჯვენა ნაწილი", new Vector2(32 / ppu, 0), Piece("furniture_static", 224, 304, 32, 48, ppu), centre.sortingOrder);
        }
        public static Sprite Door => Piece("doors", 160, 32, 32, 64);
        public static Sprite LabDoor => Piece("doors", 32, 32, 32, 64);
        public static Sprite WallPaint => Piece("construction", 488, 44, 16, 16);
        public static Sprite WallLower => Piece("construction", 488, 66, 16, 16);
        public static Sprite WallCap => Piece("construction", 488, 32, 16, 4);
        public static Sprite WallBase => Piece("construction", 488, 32, 16, 4);
        public static Sprite Locker => Piece("furniture_static", 160, 384, 32, 64);
        public static Sprite Glassware => Piece("decals", 96, 160, 32, 32);
        public static Sprite Flask => Piece("decals", 32, 160, 32, 32);
        public static Sprite Device => Piece("furniture_static", 224, 32, 32, 32, 24);
        public static SpriteRenderer Draw(Transform parent, string name, Vector2 position, Sprite sprite, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var art = go.GetComponent<SpriteRenderer>();
            art.sprite = sprite; art.sortingOrder = order;
            return art;
        }
    }
}
