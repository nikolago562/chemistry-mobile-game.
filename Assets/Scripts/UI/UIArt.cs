using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Shared pieces for the paper-and-ink screens: ruled notebook paper with a margin line,
    /// medals (and their locked silhouettes), round colour swatches with a selection ring,
    /// icon chips and the badge-unlock pop. Same palette as the world: parchment, walnut,
    /// botanical green, gold, coral.
    /// </summary>
    public static class UIArt
    {
        public static readonly Color Walnut = new Color32(110, 70, 44, 255), Margin = new Color32(226, 132, 120, 255), Rule = new Color32(190, 210, 222, 255);
        private static Sprite paper, medal, medalShadow, swatch, ring;

        /// <summary>Ruled paper (repeats as Image.Type.Tiled): cream with a pale blue line every 14 units.</summary>
        private static Sprite PaperSprite => paper ??= Build(16, 14, (x, y) => y == 0 ? Rule : new Color32(250, 245, 228, 255), true);

        private static Sprite Build(int w, int h, System.Func<int, int, Color> draw, bool tiled = false)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = tiled ? TextureWrapMode.Repeat : TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) texture.SetPixel(x, y, draw(x, y));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        }

        /// <summary>A notebook page: ruled lines, a coral margin line and binding holes down the left.</summary>
        public static Image Paper(Transform parent, string name, bool holes = true)
        {
            var frame = LabUI.Panel(parent, name, new Color32(214, 200, 168, 255));
            var sheet = LabUI.Object(frame.transform, "ქაღალდი", typeof(Image)).GetComponent<Image>();
            sheet.sprite = PaperSprite; sheet.type = Image.Type.Tiled; sheet.pixelsPerUnitMultiplier = 1 / LabUI.PixelScale; sheet.raycastTarget = false;
            LabUI.Fill(sheet.rectTransform, 3);
            var margin = LabUI.Object(frame.transform, "არშია", typeof(Image)).GetComponent<Image>();
            margin.color = Margin; margin.raycastTarget = false;
            var m = margin.rectTransform; m.anchorMin = new Vector2(0, 0); m.anchorMax = new Vector2(0, 1); m.pivot = new Vector2(0, 0.5f);
            m.sizeDelta = new Vector2(2, -6); m.anchoredPosition = new Vector2(44, 0);
            if (holes)
                for (int i = 0; i < 6; i++)
                {
                    var hole = LabUI.Object(frame.transform, "ნახვრეტი", typeof(Image)).GetComponent<Image>();
                    hole.sprite = DecorArt.Disc(); hole.color = new Color32(206, 192, 160, 255); hole.raycastTarget = false;
                    var r = hole.rectTransform; r.anchorMin = r.anchorMax = new Vector2(0, 1 - (i + 0.5f) / 6); r.sizeDelta = Vector2.one * 14; r.anchoredPosition = new Vector2(20, 0);
                }
            foreach (Transform child in frame.transform) child.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return frame;
        }

        /// <summary>A medal on a ribbon: gold rim, coloured face and a star (white; drawn in its colours).</summary>
        public static Sprite Medal => medal ??= MedalSprite(false);
        /// <summary>The same medal as a dark silhouette with a question mark, for badges not yet earned.</summary>
        public static Sprite LockedMedal => medalShadow ??= MedalSprite(true);
        private static Sprite MedalSprite(bool locked) => DecorArt.Paint(locked ? "ui:medal-locked" : "ui:medal", 32, 40, p =>
        {
            Color32 ink = AcademyArt.Ink, shade = new Color32(70, 82, 76, 255), gold = AcademyArt.Brass, goldDark = new Color32(190, 140, 60, 255);
            // ribbon tails
            for (int y = 22; y < 40; y++) { p.Fill(7 + (39 - y) / 4, y, 14 + (39 - y) / 4, y, locked ? shade : new Color32(200, 80, 80, 255)); p.Fill(17 - (39 - y) / 4, y, 24 - (39 - y) / 4, y, locked ? shade : new Color32(80, 120, 200, 255)); }
            p.Disc(15.5f, 15, 13, ink);
            p.Disc(15.5f, 15, 12, locked ? shade : goldDark);
            p.Disc(15.5f, 15.5f, 10.5f, locked ? new Color32(90, 104, 96, 255) : gold);
            p.Disc(15.5f, 15, 8.5f, locked ? new Color32(80, 94, 86, 255) : new Color32(255, 255, 255, 255));
            if (locked)
            {   // a question mark
                Color32 q = new Color32(150, 166, 156, 255);
                p.Fill(13, 18, 18, 19, q); p.Fill(18, 15, 19, 18, q); p.Fill(15, 13, 17, 14, q); p.Fill(15, 11, 16, 12, q); p.Fill(15, 8, 16, 9, q);
                return;
            }
            // star
            for (int y = 0; y < 40; y++) for (int x = 0; x < 32; x++)
            {
                float dx = x - 15.5f, dy = y - 15f, a = Mathf.Atan2(dy, dx), r = Mathf.Sqrt(dx * dx + dy * dy);
                float edge = 3.2f + 2.6f * Mathf.Pow(Mathf.Abs(Mathf.Cos(2.5f * a + Mathf.PI / 4)), 3);
                if (r < edge) p.Dot(x, y, r < edge - 1 ? new Color32(255, 232, 150, 255) : goldDark);
            }
            p.Dot(10, 21, Color.white); p.Dot(11, 22, Color.white);
        }, 16, new Vector2(0.5f, 0.5f));

        public static Sprite Swatch => swatch ??= DecorArt.Paint("ui:swatch", 24, 24, p =>
        {
            p.Disc(11.5f, 11.5f, 11.6f, AcademyArt.Ink); p.Disc(11.5f, 11.5f, 10.4f, Color.white);
            p.Disc(8f, 15f, 2f, new Color(1, 1, 1, 1));
        }, 16, new Vector2(0.5f, 0.5f));
        public static Sprite SelectRing => ring ??= DecorArt.Paint("ui:ring", 32, 32, p =>
        {
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                if (d < 15.6f && d > 12.8f) p.Dot(x, y, Color.white);
            }
        }, 16, new Vector2(0.5f, 0.5f));

        /// <summary>A small rounded chip: an icon and a short text, e.g. "🪙 60".</summary>
        public static TMP_Text Chip(Transform parent, LabIcon icon, string text, Color color, float width = -1)
        {
            var chip = LabUI.Panel(parent, text, color);
            LabUI.Size(chip.gameObject, 34, width, width < 0 ? 0 : 0);
            var row = chip.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(6, 10, 4, 5); row.spacing = 6; row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = row.childForceExpandHeight = false;
            var image = LabUI.Icon(chip.transform, icon, Color.white); LabUI.Size(image.gameObject, 24, 24, 0);
            var label = LabUI.Text(chip.transform, text, 16, LabUI.Ink); label.fontStyle = FontStyles.Bold; label.textWrappingMode = TextWrappingModes.NoWrap;
            label.GetComponent<RectTransform>(); LabUI.Size(label.gameObject, 24, -1, 0);
            var fit = chip.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return label;
        }

        /// <summary>Places an image centred in its parent at a fixed size.</summary>
        public static Image Picture(Transform parent, string name, Sprite sprite, Vector2 size, Color? color = null)
        {
            var image = LabUI.Object(parent, name, typeof(Image)).GetComponent<Image>();
            image.sprite = sprite; image.color = color ?? Color.white; image.preserveAspect = true; image.raycastTarget = false;
            var rect = image.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.sizeDelta = size;
            return image;
        }

        // ------------------------------------------------------------ badge unlock

        /// <summary>A medal drops in at the top of the screen with the badge's name. Non-blocking.</summary>
        public static void BadgeUnlocked(string name, Color colour)
        {
            if (LabUI.Root == null || LabGame.Instance == null) return;
            LabGame.Instance.StartCoroutine(Drop(name, colour));
        }

        private static IEnumerator Drop(string name, Color colour)
        {
            var card = LabUI.Panel(LabUI.Root, "ახალი ნიშანი", LabUI.Paper);
            var group = card.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            var rect = card.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1); rect.pivot = new Vector2(0.5f, 1); rect.sizeDelta = new Vector2(420, 96);
            LabUI.Shadow(card, 10);
            var band = LabUI.Panel(card.transform, "ფერი", colour); var b = band.rectTransform;
            b.anchorMin = Vector2.zero; b.anchorMax = new Vector2(0, 1); b.pivot = new Vector2(0, 0.5f); b.sizeDelta = new Vector2(12, -10); b.anchoredPosition = new Vector2(6, 0);
            var medalImage = Picture(card.transform, "მედალი", Medal, new Vector2(56, 70));
            medalImage.rectTransform.anchorMin = medalImage.rectTransform.anchorMax = new Vector2(0, 0.5f); medalImage.rectTransform.anchoredPosition = new Vector2(58, 0);
            var text = LabUI.Text(card.transform, "<size=16><color=#B07A2C>ახალი ნიშანი!</color></size>\n<b>" + name + "</b>", 21, LabUI.Ink);
            text.enableAutoSizing = true; text.fontSizeMin = 12; text.fontSizeMax = 21;
            LabUI.Fill(text.rectTransform, 10); text.rectTransform.offsetMin = new Vector2(100, 10);
            AmbientAudio.Instance?.Sparkle();
            for (float t = 0; t < 3.2f; t += Time.unscaledDeltaTime)
            {
                if (card == null) yield break;
                float drop = Mathf.Clamp01(t / 0.35f);
                float ease = 1 + 2.7f * Mathf.Pow(drop - 1, 3) + 1.7f * Mathf.Pow(drop - 1, 2);
                rect.anchoredPosition = new Vector2(0, Mathf.LerpUnclamped(110, -112, ease));
                medalImage.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 5) * 8 * Mathf.Exp(-t));
                if (t > 0.35f && t - Time.unscaledDeltaTime <= 0.35f) Juice.Sparkle(medalImage.transform, 8);
                group.alpha = t > 2.8f ? 1 - (t - 2.8f) / 0.4f : 1;
                yield return null;
            }
            if (card != null) Object.Destroy(card.gameObject);
        }
    }
}
