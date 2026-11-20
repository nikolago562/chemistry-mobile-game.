using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>What an interactable is for; picks its highlight colour.</summary>
    public enum Glow { Equipment, Reward, Teacher, Urgent, Door, Done, Optional, Hidden }

    /// <summary>
    /// Shared highlight for everything the player can use: a coloured floor outline that wakes up
    /// near the player, brightens and sparkles when it is the one [E] will use, and blinks softly
    /// from afar when it is the current objective. Only one interactable is focused at a time:
    /// the objective first, then the closest object, then doors, then optional decorations.
    /// </summary>
    public sealed class Focus : MonoBehaviour
    {
        public static readonly Color[] Colors =
        {
            new Color32(92, 232, 206, 255),  // Equipment: mint
            new Color32(255, 204, 92, 255),  // Reward: gold
            new Color32(178, 140, 255, 255), // Teacher: violet
            new Color32(255, 128, 104, 255), // Urgent: coral
            new Color32(108, 170, 255, 255), // Door: blue
            new Color32(120, 220, 120, 255), // Done: green
            new Color32(230, 230, 220, 255), // Optional: soft white
            Color.clear,
        };
        private const float WakeDistance = 4.5f;
        private static readonly List<Focus> all = new List<Focus>();
        private static Sprite ringSprite, sparkSprite;

        public Func<Glow> Kind = () => Glow.Equipment;
        public Func<bool> Objective;
        /// <summary>Set by the owner every frame: the player stands in its zone and may use it.</summary>
        public bool Near;
        /// <summary>Doors light up but never take the [E] prompt.</summary>
        public bool Selectable = true;
        private SpriteRenderer ring;
        private SpriteRenderer[] tint = new SpriteRenderer[0];
        private Color[] baseColors = new Color[0];
        private readonly SpriteRenderer[] sparks = new SpriteRenderer[3];
        private float level;
        private bool wasFocused;

        public bool IsObjective => Objective != null && Objective();
        public bool IsFocused => Selectable && Near && Current == this;

        public static Focus Current
        {
            get
            {
                Focus current = null;
                float best = float.MaxValue;
                foreach (var f in all)
                {
                    if (!f.Selectable || !f.Near || !f.isActiveAndEnabled) continue;
                    float rank = f.Rank();
                    if (rank < best) { best = rank; current = f; }
                }
                return current;
            }
        }

        /// <summary>Adds a highlight around a zone (local size and offset) of the given object.</summary>
        public static Focus Attach(GameObject owner, Vector2 size, Vector2 offset, Func<Glow> kind, Func<bool> objective = null, params SpriteRenderer[] tinted)
        {
            var focus = owner.AddComponent<Focus>();
            if (kind != null) focus.Kind = kind;
            focus.Objective = objective;
            focus.tint = tinted ?? new SpriteRenderer[0];
            focus.baseColors = Array.ConvertAll(focus.tint, r => r != null ? r.color : Color.white);
            var go = new GameObject("მონიშვნა", typeof(SpriteRenderer));
            go.transform.SetParent(owner.transform, false);
            go.transform.localPosition = offset;
            focus.ring = go.GetComponent<SpriteRenderer>();
            focus.ring.sprite = Ring();
            focus.ring.drawMode = SpriteDrawMode.Sliced;
            focus.ring.size = size + Vector2.one * 0.3f;
            // shortcut: rooms past x=100 draw floors at -300 and props below 0; the classroom tilemap tops out at 3.
            focus.ring.sortingOrder = owner.transform.position.x > 100 ? -299 : 4;
            focus.ring.color = Color.clear;
            for (int i = 0; i < focus.sparks.Length; i++)
            {
                var spark = new GameObject("ნაპერწკალი", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                spark.transform.SetParent(go.transform, false);
                spark.sprite = Spark();
                spark.sortingOrder = 14;
                spark.color = Color.clear;
                focus.sparks[i] = spark;
            }
            return focus;
        }

        private void OnEnable() => all.Add(this);
        private void OnDisable() { all.Remove(this); Near = false; }

        private float Distance()
        {
            var player = LabGame.Instance != null ? LabGame.Instance.player : null;
            return player == null || ring == null ? 999 : Vector2.Distance(player.transform.position, ring.transform.position);
        }

        /// <summary>Lower is better: objective, then nearest interactive, then doors, then optional or finished things.</summary>
        private float Rank()
        {
            var kind = Kind();
            float tier = IsObjective ? 0 : kind == Glow.Door ? 200 : kind == Glow.Optional || kind == Glow.Done ? 300 : 100;
            return tier + Distance();
        }

        private void Update()
        {
            if (ring == null) return;
            var kind = Kind();
            bool focused = IsFocused && !LabUI.AnyOpen;
            bool objective = kind != Glow.Hidden && IsObjective;
            if (focused && !wasFocused) AudioManager.Instance?.Hover();
            wasFocused = focused;

            float distance = Distance();
            float target = kind == Glow.Hidden || LabUI.AnyOpen ? 0
                : focused ? 1
                : objective ? 0.55f + 0.35f * Mathf.Sin(Time.time * 3.2f)
                : kind == Glow.Done || kind == Glow.Optional ? (distance < WakeDistance * 0.6f ? 0.2f : 0)
                : distance < WakeDistance ? Mathf.Lerp(0.45f, 0.12f, distance / WakeDistance) : 0;
            level = Mathf.MoveTowards(level, target, Time.deltaTime * 4);

            var color = Colors[(int)kind];
            float pulse = focused ? 0.8f + 0.2f * Mathf.Sin(Time.time * 6) : 1;
            ring.color = new Color(color.r, color.g, color.b, level * pulse);
            for (int i = 0; i < tint.Length; i++)
                if (tint[i] != null) tint[i].color = Color.Lerp(baseColors[i], color, 0.3f * level * pulse);

            // A few sparks rise from the outline while the object is focused or is the objective.
            bool sparkle = level > 0.5f && (focused || objective);
            var half = ring.size / 2;
            for (int i = 0; i < sparks.Length; i++)
            {
                float t = (Time.time * 0.6f + i / 3f) % 1;
                sparks[i].transform.localPosition = new Vector3(Mathf.Sin(i * 2.3f + 1) * half.x * 0.8f, -half.y * 0.4f + t * (half.y + 1.4f));
                sparks[i].color = sparkle ? new Color(color.r, color.g, color.b, Mathf.Sin(t * Mathf.PI) * level) : Color.clear;
            }
        }

        private static Sprite Ring()
        {
            if (ringSprite != null) return ringSprite;
            const int n = 16;
            var texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int edge = Mathf.Min(Mathf.Min(x, n - 1 - x), Mathf.Min(y, n - 1 - y));
                bool corner = (x < 2 || x > n - 3) && (y < 2 || y > n - 3);
                texture.SetPixel(x, y, corner ? Color.clear : edge < 2 ? Color.white : edge < 3 ? new Color(1, 1, 1, 0.45f) : new Color(1, 1, 1, 0.1f));
            }
            texture.Apply();
            return ringSprite = Sprite.Create(texture, new Rect(0, 0, n, n), Vector2.one * 0.5f, 16, 0, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
        }

        private static Sprite Spark() => sparkSprite ??= PixelArt.Make(3, 3, (x, y) => x == 1 || y == 1 ? Color.white : Color.clear);
    }
}
