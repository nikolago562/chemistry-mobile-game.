using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public static class LabUI
    {
        public static readonly Color Ink = new Color32(39, 57, 48, 255);
        public static readonly Color Muted = new Color32(103, 113, 96, 255);
        public static readonly Color Green = new Color32(45, 105, 82, 255);
        public static readonly Color Mint = new Color32(105, 199, 164, 255);
        public static readonly Color Coral = new Color32(224, 133, 102, 255);
        public static readonly Color Paper = new Color32(247, 239, 215, 255);
        public static readonly Color Gold = new Color32(239, 194, 102, 255);
        public static readonly Color Navy = new Color32(33, 57, 48, 255);
        public static readonly Color Soft = new Color32(228, 223, 199, 255);
        public static TMP_FontAsset Font;
        public static LabThemeSO Theme;
        public static Transform Root;
        private static readonly List<LabWindow> windows = new List<LabWindow>();
        private static Sprite rounded;
        public static bool AnyOpen => windows.Exists(w => w != null && w.gameObject.activeInHierarchy);
        public static bool Typing => UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;

        /// <summary>
        /// The FiraGO assets are dynamic: a glyph seen for the first time costs a 50–90 ms font
        /// lookup (FontEngine GPOS/GSUB) in the middle of play. Load every glyph the game uses,
        /// in every weight, once at boot instead.
        /// </summary>
        private static void Prewarm(TMP_FontAsset font)
        {
            if (font == null) return;
            var glyphs = new System.Text.StringBuilder("აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ„“”–—•…→←↑↓★×÷±°·✓∞");
            for (char c = ' '; c <= '~'; c++) glyphs.Append(c);
            var assets = new HashSet<TMP_FontAsset> { font };
            foreach (var weight in font.fontWeightTable) { assets.Add(weight.regularTypeface); assets.Add(weight.italicTypeface); }
            if (font.fallbackFontAssetTable != null) assets.UnionWith(font.fallbackFontAssetTable);
            foreach (var asset in assets)
                if (asset != null && asset.atlasPopulationMode == AtlasPopulationMode.Dynamic) asset.TryAddCharacters(glyphs.ToString(), out _);
        }

        public static void Initialize(TMP_FontAsset font, LabThemeSO theme = null)
        {
            windows.Clear();
            Font = font;
            Theme = theme;
            Prewarm(font);
            var go = new GameObject("ინტერფეისი", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Root = go.transform;
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            go.GetComponent<Canvas>().sortingOrder = 30;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        public static GameObject Object(Transform parent, string name, params Type[] components)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            foreach (var component in components) go.AddComponent(component);
            return go;
        }
        /// <summary>Each sprite pixel covers this many UI units, so frames read as chunky pixel art.</summary>
        public const float PixelScale = 2;
        private static Sprite frame, chunky;

        /// <summary>Builds a point-filtered 9-slice sprite from a pixel function (1 = full tint, lower = shade).</summary>
        private static Sprite Pixel9(int w, int h, Vector4 border, Func<int, int, float> shade)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float v = shade(x, y);
                texture.SetPixel(x, y, v < 0 ? Color.clear : new Color(v, v, v, 1));
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, border);
        }

        /// <summary>Panels and cards: a 1px dark outline with notched corners and a darker bottom edge.</summary>
        public static Sprite Frame() => frame ??= Pixel9(10, 10, Vector4.one * 3, (x, y) =>
        {
            bool corner = (x == 0 || x == 9) && (y == 0 || y == 9);
            if (corner) return -1;
            if (x == 0 || x == 9 || y == 0 || y == 9) return 0.42f;
            return y == 1 ? 0.84f : 1;
        });

        /// <summary>Buttons: the same outline plus a thick darker lip underneath, so they look pressable.</summary>
        public static Sprite Chunky() => chunky ??= Pixel9(10, 13, new Vector4(3, 5, 3, 3), (x, y) =>
        {
            bool corner = (x == 0 || x == 9) && (y == 0 || y == 12);
            if (corner) return -1;
            if (x == 0 || x == 9 || y == 0 || y == 12) return 0.32f;
            if (y <= 3) return 0.68f;
            return y == 11 ? 1 : 0.96f;
        });

        public static Sprite Rounded()
        {
            if (rounded != null) return rounded;
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool corner = (x < 2 || x >= size - 2) && (y < 2 || y >= size - 2);
                float shade = x == 0 || y == 0 || x == size - 1 || y == size - 1 ? 0.78f : 1;
                texture.SetPixel(x, y, corner ? Color.clear : new Color(shade, shade, shade, 1));
            }
            texture.Apply();
            rounded = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100, 0,
                SpriteMeshType.FullRect, Vector4.one * 3);
            return rounded;
        }
        public static Image Panel(Transform parent, string name, Color color)
        {
            var image = Object(parent, name, typeof(Image)).GetComponent<Image>();
            image.sprite = Frame();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1 / PixelScale;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
        public static Image Card(Transform parent, string name, Color color, int padding = 18)
        {
            var image = Panel(parent, name, color);
            var stack = image.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(padding, padding, padding, padding);
            stack.spacing = 10;
            stack.childControlWidth = stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            return image;
        }
        public static void Shadow(Image image, float amount = 8)
        {
            var shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.12f, 0.17f, 0.12f, 0.32f);
            shadow.effectDistance = new Vector2(3, -Mathf.Min(amount, 4));
        }
        public static void Fill(RectTransform rect, float padding = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * padding;
            rect.offsetMax = Vector2.one * -padding;
        }
        public static TMP_Text Text(Transform parent, string text, float size = 20, Color? color = null)
        {
            var label = Object(parent, "ტექსტი", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.font = Font;
            label.fontSize = size;
            label.color = color ?? Ink;
            label.text = text;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.lineSpacing = 2;
            return label;
        }
        /// <summary>
        /// Styles in-world lettering (names, prompts) as bold white text with a solid black
        /// outline and a soft drop shadow, so it stays readable over any floor or furniture.
        /// </summary>
        public static void WorldLabel(TMP_Text label, Color? face = null, float outline = 0.3f)
        {
            label.color = face ?? Color.white;
            label.fontStyle |= FontStyles.Bold;
            // One shared material per (font material, outline): per-label instances broke batching,
            // and creating one fails on labels built inside an inactive room.
            var source = label.fontSharedMaterial;
            if (source == null) return;
            var key = (source, outline);
            if (!worldMaterials.TryGetValue(key, out var material) || material == null)
            {
                material = new Material(source) { name = source.name + " (world label " + outline + ")" };
                material.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(0, 0, 0, 255));
                material.SetFloat(ShaderUtilities.ID_OutlineWidth, outline);
                material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0, 0, 0, 0.55f));
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.35f);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.45f);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.35f);
                material.SetFloat(ShaderUtilities.ID_UnderlayDilate, outline);
                worldMaterials[key] = material;
            }
            label.fontSharedMaterial = material;
        }
        private static readonly Dictionary<(Material, float), Material> worldMaterials = new Dictionary<(Material, float), Material>();
        public static void Size(GameObject go, float height, float width = -1, float flex = -1)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            element.preferredWidth = width;
            element.flexibleWidth = flex;
            if (width >= 0) element.minWidth = width;
            else if (flex >= 0) element.minWidth = 0;
        }
        public static void FlexibleHeight(GameObject go)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            element.flexibleHeight = 1;
        }
        public static VerticalLayoutGroup Column(Transform parent, string name, float spacing = 12)
        {
            var group = Object(parent, name, typeof(VerticalLayoutGroup)).GetComponent<VerticalLayoutGroup>();
            group.spacing = spacing;
            group.childControlHeight = group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = true;
            return group;
        }
        public static HorizontalLayoutGroup Row(Transform parent, string name, float spacing = 12)
        {
            var group = Object(parent, name, typeof(HorizontalLayoutGroup)).GetComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.childControlHeight = group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = false;
            group.childAlignment = TextAnchor.MiddleLeft;
            return group;
        }
        public static Button Button(Transform parent, string text, Action pressed, float height = 46)
        {
            var image = Panel(parent, text, Green);
            image.sprite = Chunky();
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f);
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.9f);
            colors.disabledColor = new Color(0.65f, 0.65f, 0.72f, 0.7f);
            colors.fadeDuration = 0.09f;
            button.colors = colors;
            var label = Text(image.transform, text, 18, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = true;
            label.fontSizeMin = 13;
            label.fontSizeMax = 19;
            Fill(label.rectTransform, 9);
            label.rectTransform.offsetMin = new Vector2(9, 12); // the lip takes the bottom 8 units
            Size(image.gameObject, height, -1, 1);
            image.gameObject.AddComponent<ButtonMotion>();
            button.onClick.AddListener(() => { AudioManager.Instance?.Click(); pressed(); });
            return button;
        }
        public static Button Secondary(Transform parent, string text, Action pressed, float height = 42)
        {
            var button = Button(parent, text, pressed, height);
            button.GetComponent<Image>().color = Soft;
            button.GetComponentInChildren<TMP_Text>().color = Green;
            return button;
        }
        public static TMP_Text Pill(Transform parent, string text, Color color, float width = -1)
        {
            var image = Panel(parent, text, color);
            Size(image.gameObject, 32, width, width < 0 ? 1 : 0);
            var label = Text(image.transform, text, 16, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            Fill(label.rectTransform, 5);
            return label;
        }
        public static TMP_InputField Input(Transform parent, string placeholder)
        {
            var image = Panel(parent, "პასუხი", Soft);
            var outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.43f, 0.32f, 0.6f);
            outline.effectDistance = new Vector2(1, -1);
            var input = image.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = image;
            image.raycastTarget = true;
            var viewport = Object(image.transform, "შიგთავსი", typeof(RectMask2D));
            Fill((RectTransform)viewport.transform, 15);
            var text = Text(viewport.transform, "", 26);
            Fill(text.rectTransform);
            var empty = Text(viewport.transform, placeholder, 21, Muted);
            Fill(empty.rectTransform);
            input.textViewport = (RectTransform)viewport.transform;
            input.textComponent = (TextMeshProUGUI)text;
            input.placeholder = empty;
            input.fontAsset = Font;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.caretColor = Green;
            input.customCaretColor = true;
            input.characterLimit = 80;
            var colors = input.colors;
            colors.selectedColor = Color.white;
            colors.highlightedColor = Color.white;
            input.colors = colors;
            Size(image.gameObject, 60, -1, 1);
            return input;
        }
        public static LabWindow Window(string title, float width = 960, float height = 610, bool closable = true)
        {
            var backdrop = Panel(Root, title, new Color(0.08f, 0.13f, 0.10f, 0.80f));
            backdrop.raycastTarget = true;
            backdrop.sprite = null;
            Fill(backdrop.rectTransform);
            var card = Panel(backdrop.transform, "ფანჯარა", Paper);
            card.raycastTarget = true;
            card.pixelsPerUnitMultiplier = 1 / (PixelScale * 1.5f);
            card.rectTransform.sizeDelta = new Vector2(width, height);
            Shadow(card, 12);
            var stack = card.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(28, 28, 24, 24);
            stack.spacing = 18;
            stack.childControlWidth = stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            var window = backdrop.gameObject.AddComponent<LabWindow>();
            window.CanClose = closable;
            window.Card = card.rectTransform;
            window.Fade = card.gameObject.AddComponent<CanvasGroup>();
            windows.Add(window);
            var banner = Panel(card.transform, "სათაური", Green);
            Size(banner.gameObject, 56);
            var header = banner.gameObject.AddComponent<HorizontalLayoutGroup>();
            header.padding = new RectOffset(10, 8, 6, 8);
            header.spacing = 12;
            header.childAlignment = TextAnchor.MiddleLeft;
            header.childControlWidth = header.childControlHeight = true;
            header.childForceExpandWidth = header.childForceExpandHeight = false;
            var mark = Panel(header.transform, "ლაბორატორიის ნიშანი", Gold);
            Size(mark.gameObject, 40, 40);
            var atom = Icon(mark.transform, LabIcon.Atom, Color.white);
            Fill(atom.rectTransform, 6);
            var label = Text(header.transform, title, 26, Color.white);
            label.fontStyle = FontStyles.Bold;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true; label.fontSizeMin = 16; label.fontSizeMax = 26;
            Size(label.gameObject, 40, -1, 1);
            var close = Button(header.transform, "×", window.Close, 40);
            close.GetComponent<Image>().color = Coral;
            close.gameObject.SetActive(closable);
            Size(close.gameObject, 40, 44, 0);
            var body = Column(card.transform, "შიგთავსი", 14);
            FlexibleHeight(body.gameObject);
            window.Body = (RectTransform)body.transform;
            AudioManager.Instance?.Open();
            return window;
        }
        public static Image Icon(Transform parent, LabIcon kind, Color color)
        {
            var image = Object(parent, kind.ToString(), typeof(Image)).GetComponent<Image>();
            image.sprite = LabIcons.Get(kind);
            // Sprites contain their own shaded palette; retain opacity without tinting it.
            image.color = new Color(1, 1, 1, color.a);
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }
        public static RectTransform Scroll(Transform parent)
        {
            var root = Object(parent, "გადახვევა", typeof(Image), typeof(ScrollRect));
            root.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            FlexibleHeight(root);
            var viewport = Object(root.transform, "ხილული არე", typeof(RectMask2D));
            Fill((RectTransform)viewport.transform);
            ((RectTransform)viewport.transform).offsetMax = new Vector2(-14, 0);
            var content = Column(viewport.transform, "შიგთავსი", 12);
            var rect = (RectTransform)content.transform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = Vector2.zero;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var track = Panel(root.transform, "გადახვევის ზოლი", Soft);
            var trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(1, 0);
            trackRect.anchorMax = Vector2.one;
            trackRect.pivot = Vector2.one;
            trackRect.sizeDelta = new Vector2(5, 0);
            var handle = Panel(track.transform, "მაჩვენებელი", Green);
            track.raycastTarget = handle.raycastTarget = true;
            Fill(handle.rectTransform);
            var scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.targetGraphic = handle;
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            var scroll = root.GetComponent<ScrollRect>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = rect;
            scroll.horizontal = false;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30;
            return rect;
        }
        public static Image Progress(Transform parent, float value, Color color, float height = 8)
        {
            var track = Panel(parent, "პროგრესი", Soft);
            Size(track.gameObject, height);
            var fill = Panel(track.transform, "შევსება", color);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(value), 1);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            return fill;
        }
        public static void BringToFront(LabWindow window)
        {
            windows.Remove(window);
            windows.Add(window);
            window.transform.SetAsLastSibling();
        }
        public static void CloseTop()
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i] == null || !windows[i].gameObject.activeInHierarchy) continue;
                windows[i].Close();
                return;
            }
        }
        public static void Forget(LabWindow window) => windows.Remove(window);
    }

    public sealed class LabWindow : MonoBehaviour
    {
        public RectTransform Body;
        public RectTransform Card;
        public CanvasGroup Fade;
        public Action Closed;
        public bool CanClose = true;
        private bool closing;
        private float opened;
        private void Awake() => opened = Time.unscaledTime;
        public void Close()
        {
            if (closing || !CanClose) return;
            closing = true;
            AudioManager.Instance?.Close();
            LabUI.Forget(this);
            Closed?.Invoke();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        private void LateUpdate()
        {
            var parent = (RectTransform)transform;
            Vector2 size = parent.rect.size - new Vector2(34, 34);
            float fit = Mathf.Min(1, size.x / Card.sizeDelta.x, size.y / Card.sizeDelta.y);
            float t = Mathf.Clamp01((Time.unscaledTime - opened) / 0.28f);
            float spring = 1f + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
            Card.localScale = Vector3.one * (fit * Mathf.LerpUnclamped(0.9f, 1, spring));
            if (Fade != null) Fade.alpha = Mathf.Clamp01(t * 2);
        }
        private void OnDestroy() => LabUI.Forget(this);
    }
}
