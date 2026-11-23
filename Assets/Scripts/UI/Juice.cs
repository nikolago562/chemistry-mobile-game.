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
