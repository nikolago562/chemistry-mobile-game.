using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    public enum Mood { Neutral, Happy, Excited, Surprised, Thinking, Concerned }

    /// <summary>
    /// Dialogue portraits painted in code (40 x 40 px): each character has their own hair, clothes,
    /// accessory and backdrop colour, six expressions and an open-mouth talking frame. Also the small
    /// head accessories that make classmates recognisable in the world.
    /// </summary>
    public static class Portraits
    {
        private enum Hair { Messy, Long, SidePart, Bob, Spiky, Bun, Bald, GrayBun }
        private enum Extra { None, Cap, Bow, Glasses, Beret, Headband, Goggles, Spectacles, HalfMoon }
        private sealed class Look { public Color32 Skin, HairColor, Shirt, Back; public Hair Hair; public Extra Extra; public bool Coat, Tweed; }

        private static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        private static readonly Color32 Ink = AcademyArt.Ink;
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        private static Look For(string id)
        {
            var def = System.Array.Find(Classmates.All, d => d.ID == id);
            var look = new Look { Skin = C(236, 190, 150), HairColor = C(60, 40, 30), Shirt = C(120, 170, 200), Back = C(120, 170, 160) };
            if (def != null) { look.Skin = def.Skin; look.HairColor = def.Hair; look.Shirt = def.Shirt; }
            switch (id)
            {
                case "nika": look.Hair = Hair.Messy; look.Extra = Extra.Cap; look.Back = C(236, 186, 110); break;
                case "mariam": look.Hair = Hair.Long; look.Extra = Extra.Bow; look.Back = C(150, 190, 214); break;
                case "giorgi": look.Hair = Hair.SidePart; look.Extra = Extra.Glasses; look.Back = C(150, 196, 140); break;
                case "ana": look.Hair = Hair.Bob; look.Extra = Extra.Beret; look.Back = C(214, 160, 214); break;
                case "luka": look.Hair = Hair.Spiky; look.Extra = Extra.Headband; look.Back = C(130, 150, 214); break;
                case "ketevan": look.Hair = Hair.Bun; look.Extra = Extra.Goggles; look.Coat = true; look.Back = C(170, 150, 200); break;
                case "levani": look.Hair = Hair.Bald; look.Extra = Extra.Spectacles; look.Tweed = true; look.Back = C(200, 170, 120); break;
                default: // the chemistry teacher
                    look.Skin = C(236, 196, 160); look.HairColor = C(226, 226, 232); look.Shirt = C(120, 160, 150);
                    look.Hair = Hair.GrayBun; look.Extra = Extra.HalfMoon; look.Coat = true; look.Back = C(116, 170, 150); break;
            }
            return look;
        }

        /// <summary>A guess at the speaker's expression from the line itself (Georgian cue words and punctuation).</summary>
        public static Mood Guess(string text)
        {
            if (string.IsNullOrEmpty(text)) return Mood.Neutral;
            string t = text.ToLowerInvariant();
            if (Has(t, "ვაი", "უი", "ოო,", "რა?!", "მართლა?")) return Mood.Surprised;
            if (Has(t, "ვაუ", "ოჰო", "საოცარ", "გენიალურ", "წარმოუდგენ", "!!", "ჰურა")) return Mood.Excited;
            if (Has(t, "ფრთხილ", "ბოდიში", "სამწუხარ", "ეშინ", "საშიშ", "ვერ ", "არ გამო", "ცუდი", "ჯერ არა")) return Mood.Concerned;
            if (Has(t, "ჰმმ", "მაინტერესებს", "ვფიქრობ", "იქნებ")) return Mood.Thinking;
            if (t.TrimEnd().EndsWith("?")) return Mood.Thinking;
            if (t.Contains("!")) return Mood.Happy;
            return Mood.Neutral;
        }
        private static bool Has(string t, params string[] cues) { foreach (var c in cues) if (t.Contains(c)) return true; return false; }

        /// <summary>The portrait of a speaker ("teacher" or a classmate id) with an expression; <paramref name="talking"/> opens the mouth.</summary>
        public static Sprite Get(string id, Mood mood, bool talking)
        {
            string key = id + ":" + mood + ":" + talking;
            if (cache.TryGetValue(key, out var s) && s != null) return s;
            var look = For(id);
            var p = new Px(40, 40);
            Paint(p, look, mood, talking);
            return cache[key] = p.Build("portrait:" + key, new Vector2(0.5f, 0.5f), 16);
        }

        private static Color32 Shade(Color32 c, int by) => C((byte)Mathf.Clamp(c.r + by, 0, 255), (byte)Mathf.Clamp(c.g + by, 0, 255), (byte)Mathf.Clamp(c.b + by, 0, 255));

        private static void Paint(Px p, Look k, Mood mood, bool talking)
        {
            // backdrop: soft vertical gradient with a lighter halo behind the head
            for (int y = 0; y < 40; y++) p.Fill(0, y, 39, y, Shade(k.Back, (y - 20) / 3));
            p.Disc(20, 22, 15, Shade(k.Back, 18));
            Color32 hair = k.HairColor, hairDark = Shade(hair, -34), hairLight = Shade(hair, 26), skin = k.Skin, skinDark = Shade(skin, -30);

            // long hair falls behind the shoulders
            if (k.Hair == Hair.Long) { p.Fill(9, 6, 31, 24, Ink); p.Fill(10, 7, 30, 24, hairDark); }
            if (k.Hair == Hair.Bob) { p.Fill(9, 12, 31, 24, Ink); p.Fill(10, 13, 30, 24, hairDark); }

            // shoulders and clothes
            Color32 cloth = k.Coat ? C(244, 244, 240) : k.Tweed ? C(150, 116, 76) : k.Shirt;
            for (int y = 0; y < 11; y++) { int half = 13 + Mathf.Min(y, 3) - (y > 7 ? y - 7 : 0); p.Fill(20 - half - 1, y, 19 + half + 1, y, Ink); p.Fill(20 - half, y, 19 + half, y, cloth); }
            p.Fill(7, 0, 32, 1, Shade(cloth, -26));
            if (k.Coat)
            {   // lab coat lapels over a coloured shirt
                p.Fill(17, 0, 22, 9, k.Shirt); p.Fill(16, 2, 16, 9, Shade(cloth, -40)); p.Fill(23, 2, 23, 9, Shade(cloth, -40));
                p.Fill(10, 5, 12, 6, C(120, 170, 200)); // pocket pen
            }
            else if (k.Tweed) { p.Fill(17, 0, 22, 9, C(240, 236, 222)); p.Fill(18, 8, 21, 9, C(180, 60, 60)); p.Dot(17, 9, C(180, 60, 60)); p.Dot(22, 9, C(180, 60, 60)); for (int x = 9; x < 31; x += 3) p.Dot(x, 4, Shade(cloth, -22)); }
            else { p.Fill(16, 8, 23, 9, Shade(cloth, -30)); p.Fill(18, 7, 21, 7, Shade(cloth, 20)); }
            // neck
            p.Fill(17, 9, 22, 12, Ink); p.Fill(18, 9, 21, 12, skinDark);

            // head
            for (int y = 11; y <= 32; y++) for (int x = 9; x <= 30; x++)
            {
                float d = Mathf.Pow((x - 19.5f) / 10f, 2) + Mathf.Pow((y - 21.5f) / 10.6f, 2);
                if (d <= 1) p.Dot(x, y, d > 0.86f ? Ink : skin);
            }
            p.Fill(8, 19, 9, 22, Ink); p.Fill(30, 19, 31, 22, Ink); p.Dot(9, 20, skin); p.Dot(30, 20, skin); // ears
            p.Dot(19, 18, skinDark); p.Dot(20, 18, skinDark); // nose
            if (mood == Mood.Happy || mood == Mood.Excited) { p.Fill(12, 16, 14, 17, C(240, 150, 140)); p.Fill(25, 16, 27, 17, C(240, 150, 140)); }

            Eyes(p, mood, hair);
            Mouth(p, mood, talking);
            HairOn(p, k, hair, hairDark, hairLight);
            ExtraOn(p, k, mood);
        }

        private static void Eyes(Px p, Mood mood, Color32 hair)
        {
            Color32 brow = Shade(hair, -40);
            foreach (int ex in new[] { 15, 24 })
            {
                switch (mood)
                {
                    case Mood.Happy:
                        p.Dot(ex - 1, 21, Ink); p.Dot(ex, 22, Ink); p.Dot(ex + 1, 21, Ink); break;
                    case Mood.Excited:
                        p.Fill(ex - 1, 20, ex + 1, 23, Ink); p.Dot(ex, 22, Color.white); p.Dot(ex - 1, 23, Color.white); break;
                    case Mood.Surprised:
                        p.Fill(ex - 1, 20, ex + 1, 23, Ink); p.Fill(ex, 21, ex, 22, Color.white); break;
                    case Mood.Thinking:
                        p.Fill(ex + 1, 21, ex + 1, 23, Ink); p.Dot(ex + 1, 23, C(90, 80, 90)); break;
                    default:
                        p.Fill(ex, 20, ex, 22, Ink); p.Dot(ex, 22, C(90, 80, 90)); break;
                }
            }
            switch (mood)
            {
                case Mood.Surprised: case Mood.Excited:
                    p.Fill(13, 26, 16, 26, brow); p.Fill(23, 26, 26, 26, brow); break;
                case Mood.Concerned:
                    p.Fill(13, 24, 14, 24, brow); p.Fill(15, 25, 16, 25, brow); p.Fill(23, 25, 24, 25, brow); p.Fill(25, 24, 26, 24, brow); break;
                case Mood.Thinking:
                    p.Fill(13, 24, 16, 24, brow); p.Fill(23, 26, 26, 26, brow); break;
                default:
                    p.Fill(13, 25, 16, 25, brow); p.Fill(23, 25, 26, 25, brow); break;
            }
        }

        private static void Mouth(Px p, Mood mood, bool talking)
        {
            Color32 lip = C(150, 70, 70), inside = C(120, 46, 56), tongue = C(230, 120, 120);
            if (talking || mood == Mood.Excited)
            {
                int w = mood == Mood.Excited ? 3 : 2;
                p.Fill(20 - w, 14, 19 + w, 16, Ink); p.Fill(21 - w, 14, 18 + w, 15, inside); p.Fill(19, 14, 20, 14, tongue);
                if (mood == Mood.Happy || mood == Mood.Excited) p.Fill(21 - w, 16, 18 + w, 16, Color.white);
                return;
            }
            switch (mood)
            {
                case Mood.Happy: p.Dot(16, 16, Ink); p.Fill(17, 15, 22, 15, Ink); p.Dot(23, 16, Ink); break;
                case Mood.Surprised: p.Fill(19, 14, 20, 16, Ink); p.Dot(19, 15, inside); p.Dot(20, 15, inside); break;
                case Mood.Thinking: p.Fill(19, 15, 23, 15, lip); p.Dot(24, 16, lip); break;
                case Mood.Concerned: p.Dot(17, 15, lip); p.Fill(18, 16, 21, 16, lip); p.Dot(22, 15, lip); break;
                default: p.Fill(18, 15, 21, 15, lip); p.Dot(17, 16, lip); p.Dot(22, 16, lip); break;
            }
        }

        private static void HairOn(Px p, Look k, Color32 hair, Color32 dark, Color32 light)
        {
            void Cap(int top, int lowEdge)
            {   // hair covering the top of the head down to lowEdge, following the head outline
                for (int y = lowEdge; y <= top; y++) for (int x = 8; x <= 31; x++)
                {
                    float d = Mathf.Pow((x - 19.5f) / 11f, 2) + Mathf.Pow((y - 22f) / 11.6f, 2);
                    if (d <= 1) p.Dot(x, y, d > 0.84f ? Ink : hair);
                }
            }
            switch (k.Hair)
            {
                case Hair.Messy:
                    Cap(33, 26);
                    for (int x = 10; x < 30; x += 3) { p.Dot(x, 34, Ink); p.Dot(x + 1, 33, hair); }
                    p.Fill(11, 24, 13, 26, hair); p.Fill(26, 24, 28, 26, hair); p.Fill(17, 25, 19, 26, hair);
                    p.Fill(13, 30, 18, 30, light); break;
                case Hair.Long:
                    Cap(33, 25);
                    p.Fill(9, 12, 11, 25, hair); p.Fill(28, 12, 30, 25, hair); p.Fill(8, 12, 8, 25, Ink); p.Fill(31, 12, 31, 25, Ink);
                    p.Fill(20, 25, 27, 27, hair); p.Fill(12, 30, 17, 30, light); break;
                case Hair.SidePart:
                    Cap(32, 26); p.Fill(12, 25, 22, 27, hair); p.Fill(23, 28, 23, 31, dark); p.Fill(14, 29, 21, 29, light); break;
                case Hair.Bob:
                    Cap(33, 24); p.Fill(9, 15, 11, 24, hair); p.Fill(28, 15, 30, 24, hair); p.Fill(8, 15, 8, 24, Ink); p.Fill(31, 15, 31, 24, Ink);
                    p.Fill(12, 24, 27, 25, hair); p.Fill(9, 14, 11, 14, Ink); p.Fill(28, 14, 30, 14, Ink); p.Fill(13, 30, 19, 30, light); break;
                case Hair.Spiky:
                    Cap(32, 27);
                    foreach (int x in new[] { 11, 15, 19, 23, 27 }) { p.Fill(x, 33, x + 1, 35, hair); p.Dot(x, 36, Ink); p.Dot(x + 1, 36, Ink); p.Dot(x - 1, 34, Ink); }
                    p.Fill(14, 30, 20, 30, light); break;
                case Hair.Bun:
                    p.Disc(19.5f, 34.5f, 4.4f, Ink); p.Disc(19.5f, 34.5f, 3.5f, hair); p.Dot(18, 36, light);
                    Cap(32, 26); p.Fill(11, 25, 14, 27, hair); p.Fill(25, 25, 28, 27, hair); break;
                case Hair.GrayBun:
                    p.Disc(19.5f, 34.5f, 4.4f, Ink); p.Disc(19.5f, 34.5f, 3.5f, hair); p.Dot(18, 36, Color.white);
                    Cap(32, 26); p.Fill(10, 22, 12, 27, hair); p.Fill(27, 22, 29, 27, hair); p.Fill(14, 30, 20, 30, Color.white); break;
                case Hair.Bald:
                    p.Fill(9, 19, 11, 25, hair); p.Fill(28, 19, 30, 25, hair); p.Fill(8, 19, 8, 25, Ink); p.Fill(31, 19, 31, 25, Ink);
                    p.Fill(14, 29, 17, 30, Shade(k.Skin, 24)); // shine
                    // a neat grey beard
                    for (int y = 11; y <= 17; y++) for (int x = 11; x <= 28; x++)
                    {
                        float d = Mathf.Pow((x - 19.5f) / 8.8f, 2) + Mathf.Pow((y - 17f) / 6.4f, 2);
                        if (d <= 1 && !(y >= 14 && y <= 16 && x >= 17 && x <= 22)) p.Dot(x, y, d > 0.8f ? dark : hair);
                    }
                    p.Fill(15, 17, 24, 18, hair); break;
            }
        }

        private static void ExtraOn(Px p, Look k, Mood mood)
        {
            switch (k.Extra)
            {
                case Extra.Cap:
                    p.Fill(10, 29, 29, 34, Ink); p.Fill(11, 30, 28, 33, C(210, 80, 70)); p.Fill(11, 33, 28, 33, C(236, 120, 100));
                    p.Fill(24, 28, 33, 30, Ink); p.Fill(25, 29, 32, 29, C(180, 60, 56)); p.Fill(17, 31, 19, 32, C(250, 230, 120)); break;
                case Extra.Bow:
                    p.Fill(25, 31, 32, 36, Ink); p.Fill(26, 32, 28, 35, C(240, 120, 140)); p.Fill(29, 32, 31, 35, C(240, 120, 140)); p.Fill(28, 33, 29, 34, C(200, 80, 100)); break;
                case Extra.Glasses:
                case Extra.Spectacles:
                    Color32 rim = k.Extra == Extra.Glasses ? C(60, 50, 60) : C(200, 160, 70);
                    foreach (int ex in new[] { 15, 24 }) { p.Fill(ex - 3, 19, ex + 3, 19, rim); p.Fill(ex - 3, 24, ex + 3, 24, rim); p.Fill(ex - 3, 19, ex - 3, 24, rim); p.Fill(ex + 3, 19, ex + 3, 24, rim); p.Dot(ex - 2, 23, Color.white); }
                    p.Fill(18, 22, 21, 22, rim); break;
                case Extra.HalfMoon:
                    foreach (int ex in new[] { 15, 24 }) { p.Fill(ex - 3, 19, ex + 3, 19, C(200, 160, 70)); p.Dot(ex - 3, 20, C(200, 160, 70)); p.Dot(ex + 3, 20, C(200, 160, 70)); }
                    p.Fill(18, 20, 21, 20, C(200, 160, 70)); break;
                case Extra.Beret:
                    p.Disc(18, 33, 9, Ink); p.Disc(18, 33, 8, C(150, 110, 200)); p.Fill(9, 27, 30, 29, Ink); p.Fill(10, 28, 29, 28, C(124, 86, 170));
                    p.Fill(17, 41, 18, 42, Ink); p.Dot(23, 35, C(180, 150, 230)); p.Dot(12, 24, C(240, 200, 90)); p.Dot(13, 23, C(90, 170, 200)); break; // and a paint fleck
                case Extra.Headband:
                    p.Fill(9, 26, 30, 29, Ink); p.Fill(10, 27, 29, 28, C(220, 70, 70)); p.Fill(10, 27, 29, 27, Color.white); break;
                case Extra.Goggles:
                    p.Fill(9, 27, 30, 28, Ink);
                    foreach (int ex in new[] { 15, 24 }) { p.Disc(ex, 29.5f, 3.6f, Ink); p.Disc(ex, 29.5f, 2.6f, C(130, 210, 230)); p.Dot(ex - 1, 30, Color.white); }
                    break;
            }
        }

        // ---------- world accessories ----------

        /// <summary>A small head accessory so classmates read apart in the world; null when the character has none.</summary>
        public static Sprite Accessory(string id, out Vector2 offset, out bool faceOnly)
        {
            offset = new Vector2(0, 1.84f); faceOnly = false;
            switch (id)
            {
                case "nika": offset = new Vector2(0, 1.74f); return DecorArt.Paint("acc:cap", 18, 7, p => { p.Fill(1, 0, 13, 6, Ink); p.Fill(2, 1, 12, 5, C(210, 80, 70)); p.Fill(2, 5, 12, 5, C(236, 120, 100)); p.Fill(11, 0, 17, 2, Ink); p.Fill(12, 1, 16, 1, C(180, 60, 56)); }, 16, new Vector2(0.45f, 0.5f));
                case "mariam": offset = new Vector2(0.38f, 1.78f); return DecorArt.Paint("acc:bow", 9, 6, p => { p.Fill(0, 0, 8, 5, Ink); p.Fill(1, 1, 3, 4, C(240, 120, 140)); p.Fill(5, 1, 7, 4, C(240, 120, 140)); p.Fill(4, 2, 4, 3, C(200, 80, 100)); }, 16, new Vector2(0.5f, 0.5f));
                case "giorgi": case "levani":
                    offset = new Vector2(0, 1.02f); faceOnly = true;
                    Color32 rim = id == "giorgi" ? C(50, 44, 52) : C(200, 160, 70);
                    return DecorArt.Paint("acc:glasses" + id, 14, 5, p => { p.Fill(0, 0, 5, 4, rim); p.Fill(8, 0, 13, 4, rim); p.Fill(1, 1, 4, 3, C(220, 240, 250, 150)); p.Fill(9, 1, 12, 3, C(220, 240, 250, 150)); p.Fill(6, 2, 7, 2, rim); }, 16, new Vector2(0.5f, 0.5f));
                case "ana": offset = new Vector2(-0.05f, 1.86f); return DecorArt.Paint("acc:beret", 16, 6, p => { p.Fill(1, 0, 14, 4, Ink); p.Fill(2, 1, 13, 3, C(150, 110, 200)); p.Fill(0, 1, 2, 2, Ink); p.Fill(7, 5, 8, 5, Ink); }, 16, new Vector2(0.5f, 0.5f));
                case "luka": offset = new Vector2(0, 1.5f); return DecorArt.Paint("acc:band", 16, 3, p => { p.Fill(0, 0, 15, 2, Ink); p.Fill(1, 1, 14, 1, C(220, 70, 70)); }, 16, new Vector2(0.5f, 0.5f));
                case "ketevan": offset = new Vector2(0, 1.56f); return DecorArt.Paint("acc:goggles", 16, 5, p => { p.Fill(0, 1, 15, 3, Ink); p.Fill(1, 0, 6, 4, Ink); p.Fill(9, 0, 14, 4, Ink); p.Fill(2, 1, 5, 3, C(130, 210, 230)); p.Fill(10, 1, 13, 3, C(130, 210, 230)); }, 16, new Vector2(0.5f, 0.5f));
                default: return null;
            }
        }
    }

    /// <summary>Keeps a classmate's head accessory on their head, hiding face-only items when they look away.</summary>
    public sealed class HeadAccessory : MonoBehaviour
    {
        private Animator animator;
        private SpriteRenderer art;
        private bool faceOnly;
        private static readonly int MoveX = Animator.StringToHash("MoveX"), MoveY = Animator.StringToHash("MoveY");

        public static void Attach(Transform root, Animator animator, string id)
        {
            var sprite = Portraits.Accessory(id, out var offset, out bool faceOnly);
            if (sprite == null) return;
            var art = LaboratoryArt.Draw(root, "აქსესუარი", offset, sprite, 1);
            var accessory = art.gameObject.AddComponent<HeadAccessory>();
            accessory.animator = animator; accessory.art = art; accessory.faceOnly = faceOnly;
        }

        private void LateUpdate()
        {
            if (!faceOnly || animator == null) return;
            float x = animator.GetFloat(MoveX), y = animator.GetFloat(MoveY);
            art.enabled = y <= 0.4f;
            transform.localScale = new Vector3(Mathf.Abs(x) > 0.5f ? 0.5f : 1, 1, 1);
        }
    }
}
