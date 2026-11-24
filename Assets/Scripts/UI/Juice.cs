using System.Collections;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>Small feel-good effects shared by the UI: click sparks, confetti, pop-ins, count-ups and the reward chest.</summary>
    public static class Juice
    {
        private static readonly Color[] Party =
        {
            new Color32(239, 194, 102, 255), new Color32(105, 199, 164, 255), new Color32(224, 133, 102, 255),
            new Color32(110, 170, 230, 255), new Color32(200, 140, 220, 255),
        };

        /// <summary>A few sparks from a UI element, drawn on the top canvas so they survive the element being rebuilt.</summary>
        public static void Sparkle(Transform at, int count = 7)
        {
            if (LabUI.Root == null || at == null) return;
            var root = (RectTransform)LabUI.Root;
            Vector2 point = root.InverseTransformPoint(at.position);
            UIFx.Burst(root, point, Party[Random.Range(0, Party.Length)], count);
        }

        /// <summary>
        /// Shows a result card over a dimmed, input-blocking backdrop inside <paramref name="host"/>: the host
        /// shakes once, the card pops in, and it leaves on a tap (after a short guard) or after <paramref name="hold"/> seconds.
        /// </summary>
        public static IEnumerator Popup(RectTransform host, RectTransform card, float hold = 2.2f)
        {
            if (host == null || card == null) yield break;
            var dim = LabUI.Panel(host, "ფონი", new Color(0.05f, 0.08f, 0.07f, 0.55f));
            dim.sprite = null; dim.raycastTarget = true;
            dim.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            LabUI.Fill(dim.rectTransform);
            bool tapped = false;
            dim.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(() => tapped = true);
            card.SetParent(dim.transform, false);
            var group = dim.gameObject.AddComponent<CanvasGroup>();
            Vector2 rest = host.anchoredPosition;
            float leave = -1;
            for (float t = 0; dim != null; t += Time.unscaledDeltaTime)
            {
                float u = Mathf.Clamp01(t / 0.3f);
                card.localScale = Vector3.one * (t < 0.3f ? Mathf.LerpUnclamped(0.6f, 1, 1 + 2.7f * Mathf.Pow(u - 1, 3) + 1.7f * Mathf.Pow(u - 1, 2)) : 1);
                host.anchoredPosition = rest + (t < 0.25f ? Random.insideUnitCircle * 6 * (1 - t / 0.25f) : Vector2.zero);
                if (leave < 0 && (t > hold || tapped && t > 0.35f)) leave = t;
                if (leave >= 0) { group.alpha = 1 - (t - leave) / 0.25f; if (group.alpha <= 0) break; }
                yield return null;
            }
            if (host != null) host.anchoredPosition = rest;
            if (dim != null) Object.Destroy(dim.gameObject);
        }

        /// <summary>A quick squash-and-stretch on whatever was just tapped.</summary>
        public static void Bounce(Transform target)
        {
            if (target == null) return;
            var squash = target.GetComponent<Squash>() ?? target.gameObject.AddComponent<Squash>();
            squash.Restart();
        }

        /// <summary>
        /// A coloured drop flies in an arc from one UI element to another, splashes and plops, then
        /// calls <paramref name="landed"/>. Drawn on the top canvas so rebuilt tiles don't cut it short.
        /// </summary>
        public static void Fly(Transform from, Transform to, Color color, System.Action landed = null, float pitch = 1)
        {
            if (LabUI.Root == null || from == null || to == null) { landed?.Invoke(); return; }
            var root = (RectTransform)LabUI.Root;
            var drop = LabUI.Object(root, "წვეთი", typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
            drop.sprite = DropSprite; drop.color = color; drop.raycastTarget = false;
            drop.rectTransform.sizeDelta = Vector2.one * 30;
            drop.gameObject.AddComponent<Flyer>().Begin(root.InverseTransformPoint(from.position), to, color, landed, pitch);
        }

        private static Sprite drop;
        private static Sprite DropSprite => drop ??= PixelArt.Make(12, 12, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(5.5f, 5.5f));
            return d < 5.5f ? (d < 3 && x < 6 && y > 5 ? new Color(1, 1, 1, 1) : new Color(0.92f, 0.92f, 0.92f, 1)) : Color.clear;
        });

        private sealed class Squash : MonoBehaviour
        {
            private float t = 1;
            public void Restart() => t = 0;
            private void Update()
            {
                if (t >= 1) return;
                t = Mathf.Min(1, t + Time.unscaledDeltaTime / 0.28f);
                float wobble = Mathf.Sin(t * Mathf.PI * 3) * (1 - t) * 0.12f;
                transform.localScale = new Vector3(1 + wobble, 1 - wobble, 1);
            }
        }

        private sealed class Flyer : MonoBehaviour
        {
            private Vector2 from; private Transform to; private Color color; private System.Action landed; private float t, pitch;
            public void Begin(Vector2 start, Transform target, Color tint, System.Action done, float note) { from = start; to = target; color = tint; landed = done; pitch = note; }
            private void Update()
            {
                var root = (RectTransform)transform.parent;
                if (to == null) { Destroy(gameObject); return; }
                Vector2 end = root.InverseTransformPoint(to.position);
                t += Time.unscaledDeltaTime / 0.32f;
                float k = Mathf.Clamp01(t);
                ((RectTransform)transform).anchoredPosition = Vector2.Lerp(from, end, k) + Vector2.up * Mathf.Sin(k * Mathf.PI) * 120;
                transform.localScale = Vector3.one * (1 - 0.35f * k);
                if (t < 1) return;
                UIFx.Burst(root, end, color, 10);
                AmbientAudio.Instance?.Drop(pitch);
                landed?.Invoke();
                Destroy(gameObject);
            }
        }

        /// <summary>Multi-coloured confetti bursts across a card.</summary>
        public static void Confetti(Transform at, int bursts = 5, float spread = 380)
        {
            if (LabUI.Root == null || at == null) return;
            var root = (RectTransform)LabUI.Root;
            Vector2 centre = root.InverseTransformPoint(at.position);
            for (int i = 0; i < bursts; i++)
                UIFx.Burst(root, centre + new Vector2(Random.Range(-spread, spread), Random.Range(-60, 120)), Party[i % Party.Length], 14);
        }

        /// <summary>Scales a thing in from nothing with a little overshoot after <paramref name="delay"/> seconds.</summary>
        public static IEnumerator PopIn(Transform target, float delay)
        {
            if (target == null) yield break;
            target.localScale = Vector3.zero;
            for (float t = 0; t < delay; t += Time.unscaledDeltaTime) yield return null;
            for (float t = 0; t < 0.35f && target != null; t += Time.unscaledDeltaTime)
            {
                float x = t / 0.35f - 1;
                target.localScale = Vector3.one * (1 + 2.7f * x * x * x + 1.7f * x * x);
                yield return null;
            }
            if (target != null) target.localScale = Vector3.one;
        }

        /// <summary>Counts a number up with soft ticks, then a coin chime.</summary>
        public static IEnumerator CountUp(TMP_Text label, string prefix, int to, string suffix, float delay)
        {
            if (label == null) yield break;
            label.text = prefix + "0" + suffix;
            for (float t = 0; t < delay; t += Time.unscaledDeltaTime) yield return null;
            int shown = -1;
            for (float t = 0; t < 0.8f && label != null; t += Time.unscaledDeltaTime)
            {
                int value = Mathf.RoundToInt(to * Mathf.SmoothStep(0, 1, t / 0.8f));
                if (value != shown) { shown = value; label.text = prefix + value + suffix; AudioManager.Instance?.Hover(); }
                yield return null;
            }
            if (label == null) yield break;
            label.text = prefix + to + suffix;
            AudioManager.Instance?.Coin();
            Sparkle(label.transform, 10);
        }

        /// <summary>Rocks a thing side to side until it is destroyed or <paramref name="still"/> says stop.</summary>
        public static IEnumerator Jiggle(Transform target, System.Func<bool> still)
        {
            float t = 0;
            while (target != null && !still())
            {
                t += Time.unscaledDeltaTime;
                float burst = Mathf.Repeat(t, 1.6f) < 0.5f ? Mathf.Sin(t * 30) * 7 : 0;
                target.localRotation = Quaternion.Euler(0, 0, burst);
                yield return null;
            }
            if (target != null) target.localRotation = Quaternion.identity;
        }

        private static Sprite closed, open;
        /// <summary>A 16×14 pixel treasure chest, closed or open.</summary>
        public static Sprite Chest(bool opened)
        {
            if (opened && open != null) return open;
            if (!opened && closed != null) return closed;
            Color ink = new Color32(46, 34, 30, 255), wood = new Color32(168, 98, 52, 255), dark = new Color32(122, 68, 36, 255),
                gold = new Color32(239, 194, 102, 255), glow = new Color32(255, 240, 170, 255);
            var sprite = PixelArt.Make(16, 14, (x, y) =>
            {
                bool body = x >= 1 && x <= 14 && y <= 7, lid = opened ? x >= 1 && x <= 14 && y >= 11 && y <= 13 : x >= 1 && x <= 14 && y >= 8 && y <= 12;
                if (opened && x >= 3 && x <= 12 && y >= 8 && y <= 10) return glow;
                if (!body && !lid) return Color.clear;
                if (x == 1 || x == 14 || y == 0 || (body && y == 7) || (lid && (y == (opened ? 13 : 12) || y == (opened ? 11 : 8)))) return ink;
                if (x == 4 || x == 11) return gold;
                if (!opened && x >= 7 && x <= 8 && y >= 6 && y <= 9) return gold;
                return y % 3 == 0 ? dark : wood;
            });
            return opened ? open = sprite : closed = sprite;
        }
    }
}
