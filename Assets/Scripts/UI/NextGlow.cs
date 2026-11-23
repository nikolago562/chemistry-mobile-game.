using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// A pulsing gold ring over whatever the player should touch next, so a bench explains itself
    /// without text. It lives on the window (not on the target) because benches rebuild their tiles.
    /// </summary>
    public sealed class NextGlow : MonoBehaviour
    {
        private static readonly Vector3[] Corners = new Vector3[4];
        private RectTransform rect, target;
        private Image ring;

        /// <summary>What to point at; null hides the ring.</summary>
        public RectTransform Target { set => target = value; }

        public static NextGlow On(LabWindow window)
        {
            var image = LabUI.Object(window.Card, "შემდეგი", typeof(Image)).GetComponent<Image>();
            image.sprite = LabUI.Ring(); image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1 / LabUI.PixelScale;
            image.raycastTarget = false; image.enabled = false;
            image.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var glow = image.gameObject.AddComponent<NextGlow>();
            glow.ring = image; glow.rect = image.rectTransform;
            return glow;
        }

        private void LateUpdate()
        {
            bool show = target != null && target.gameObject.activeInHierarchy;
            ring.enabled = show;
            if (!show) return;
            var parent = (RectTransform)rect.parent;
            target.GetWorldCorners(Corners);
            Vector2 min = parent.InverseTransformPoint(Corners[0]), max = parent.InverseTransformPoint(Corners[2]);
            float s = Mathf.Sin(Time.unscaledTime * 5);
            rect.anchorMin = rect.anchorMax = parent.pivot;
            rect.anchoredPosition = (min + max) / 2;
            rect.sizeDelta = max - min + Vector2.one * (18 + 6 * s);
            ring.color = new Color(1f, 0.6f, 0.12f, 0.8f + 0.2f * s);
            if (rect.GetSiblingIndex() != parent.childCount - 1) rect.SetAsLastSibling();
        }

        /// <summary>Turns a book button into a progress bar: "წიგნი 2/32" over a gold fill.</summary>
        public static void Progress(Button book, string label, int found, int total)
        {
            var fill = book.transform.Find("სავსე") as RectTransform;
            if (fill == null)
            {
                fill = LabUI.Panel(book.transform, "სავსე", LabUI.Gold).rectTransform;
                fill.GetComponent<Image>().raycastTarget = false;
                fill.SetSiblingIndex(0);
            }
            fill.gameObject.SetActive(found > 0);
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(total == 0 ? 0 : (float)found / total, 1);
            fill.offsetMin = new Vector2(4, 12); fill.offsetMax = new Vector2(-4, -4);
            book.GetComponentInChildren<TMP_Text>().text = label;
        }
    }
}
