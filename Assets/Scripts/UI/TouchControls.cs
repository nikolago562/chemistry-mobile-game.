using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Phone controls: a floating joystick in the bottom-left (8 directions, diagonals included)
    /// and a round action button in the bottom-right that appears when something can be used.
    /// Works with touch and with the mouse, so it can be tried in the editor.
    /// </summary>
    public sealed class TouchControls : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        /// <summary>Current joystick direction (length 0 or 1, snapped to 8 directions).</summary>
        public static Vector2 Move { get; private set; }
        private static int actionFrame = -10;
        /// <summary>
        /// E on a keyboard, or a tap on the action button. A tap is used up by the first object that
        /// reads it, within a frame of the tap (button clicks and object updates run in either order).
        /// </summary>
        public static bool Interact
        {
            get
            {
                if (Input.GetKeyDown(KeyCode.E)) return true;
                if (Time.frameCount - actionFrame > 1) return false;
                actionFrame = -10;
                return true;
            }
        }

        /// <summary>Acts like a tap on the action button (used by tap-to-interact).</summary>
        public static void Press() => actionFrame = Time.frameCount;

        private const float Radius = 70, Dead = 0.25f;
        private static readonly Vector2 Home = new Vector2(150, 150);
        private RectTransform zone, pad, knob, action;
        public static TouchControls Instance { get; private set; }
        /// <summary>The joystick and the action button, for the tutorial pointer.</summary>
        public RectTransform Pad => pad;
        public RectTransform Action => actionGroup != null && actionGroup.alpha > 0.5f ? action : null;
        private CanvasGroup padGroup, actionGroup;
        private int pointer = int.MinValue;

        public static TouchControls Create()
        {
            var zone = LabUI.Object(LabUI.Root, "ჯოისტიკის არე", typeof(Image));
            var controls = zone.AddComponent<TouchControls>();
            controls.Build((RectTransform)zone.transform);
            Instance = controls;
            return controls;
        }

        private void Build(RectTransform area)
        {
            zone = area;
            // The left 40% of the lower 60% of the screen: start a drag anywhere there.
            zone.anchorMin = Vector2.zero; zone.anchorMax = new Vector2(0.4f, 0.6f);
            zone.offsetMin = zone.offsetMax = Vector2.zero;
            zone.GetComponent<Image>().color = Color.clear;
            zone.SetAsFirstSibling();

            pad = Round(zone, "ჯოისტიკი", 170, new Color(0.13f, 0.22f, 0.19f, 0.45f));
            pad.anchorMin = pad.anchorMax = Vector2.zero;
            pad.anchoredPosition = Home;
            padGroup = pad.gameObject.AddComponent<CanvasGroup>();
            padGroup.blocksRaycasts = false;
            var rim = Round(pad, "რგოლი", 170, new Color(LabUI.Mint.r, LabUI.Mint.g, LabUI.Mint.b, 0.55f), true);
            rim.anchoredPosition = Vector2.zero;
            knob = Round(pad, "სახელური", 76, LabUI.Paper);
            knob.anchoredPosition = Vector2.zero;
            var dot = Round(knob, "ცენტრი", 30, LabUI.Mint);
            dot.anchoredPosition = Vector2.zero;

            action = Round(LabUI.Root, "მოქმედების ღილაკი", 124, LabUI.Green);
            action.anchorMin = action.anchorMax = new Vector2(1, 0);
            action.anchoredPosition = new Vector2(-130, 130);
            Round(action, "რგოლი", 124, new Color(1, 1, 1, 0.35f), true).anchoredPosition = Vector2.zero;
            var hand = LabUI.Text(action, "გამოყენება", 17, Color.white);
            hand.alignment = TMPro.TextAlignmentOptions.Center; hand.fontStyle = TMPro.FontStyles.Bold;
            hand.enableAutoSizing = true; hand.fontSizeMin = 11; hand.fontSizeMax = 17;
            LabUI.Fill(hand.rectTransform, 10);
            var image = action.GetComponent<Image>();
            image.raycastTarget = true;
            var button = action.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => actionFrame = Time.frameCount);
            action.gameObject.AddComponent<ButtonMotion>();
            actionGroup = action.gameObject.AddComponent<CanvasGroup>();
            action.SetAsFirstSibling();
            action.gameObject.AddComponent<SafeInset>();
            zone.gameObject.AddComponent<SafeInset>();
        }

        private static RectTransform Round(Transform parent, string name, float size, Color color, bool ring = false)
        {
            var image = LabUI.Panel(parent, name, color);
            image.sprite = ring ? RingSprite : DiscSprite;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
            rect.sizeDelta = Vector2.one * size;
            return rect;
        }
        private static Sprite disc, ring;
        private static Sprite DiscSprite => disc ??= PixelArt.Make(64, 64, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) < 31.5f ? Color.white : Color.clear);
        private static Sprite RingSprite => ring ??= PixelArt.Make(64, 64, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
            return d < 31.5f && d > 28 ? Color.white : Color.clear;
        });

        /// <summary>Snaps a stick offset to one of 8 directions, or zero inside the dead zone.</summary>
        public static Vector2 Snap(Vector2 offset)
        {
            if (offset.magnitude < Dead) return Vector2.zero;
            float angle = Mathf.Round(Mathf.Atan2(offset.y, offset.x) / (Mathf.PI / 4)) * (Mathf.PI / 4);
            var v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y)).normalized;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointer != int.MinValue) return;
            pointer = e.pointerId;
            // Floating stick: it jumps to where the thumb lands.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, e.position, e.pressEventCamera, out var local);
            pad.anchoredPosition = local - zone.rect.min;
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pad, e.position, e.pressEventCamera, out var local);
            var offset = Vector2.ClampMagnitude(local, Radius);
            knob.anchoredPosition = offset;
            Move = Snap(offset / Radius);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            pointer = int.MinValue;
            Move = Vector2.zero;
            knob.anchoredPosition = Vector2.zero;
            pad.anchoredPosition = Home;
        }

        private void Update()
        {
            // Hidden while a window or dialogue is open; the action button only shows when something can be used.
            bool free = !LabUI.AnyOpen && GameSaveManager.Instance.Data.profileCompleted && (LabGame.Instance?.Dialogue == null || !LabGame.Instance.Dialogue.IsOpen);
            if (!free && pointer != int.MinValue) OnPointerUp(new PointerEventData(EventSystem.current) { pointerId = pointer });
            padGroup.alpha = Mathf.MoveTowards(padGroup.alpha, free ? (pointer != int.MinValue ? 1 : 0.7f) : 0, Time.unscaledDeltaTime * 6);
            zone.GetComponent<Image>().raycastTarget = free;
            var focus = Focus.Current;
            bool usable = free && focus != null && focus.IsFocused;
            actionGroup.alpha = Mathf.MoveTowards(actionGroup.alpha, usable ? 1 : 0, Time.unscaledDeltaTime * 6);
            actionGroup.blocksRaycasts = actionGroup.interactable = usable;
        }
    }
}
