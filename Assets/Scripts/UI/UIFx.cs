using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public enum FxKind { Foam, Burst, Smoke, Spill, Wobble }

    /// <summary>Small UI particle effects for experiments.</summary>
    public sealed class UIFx : MonoBehaviour
    {
        private Vector2 velocity; private float life, age, grow; private Image image; private RectTransform rect; private float spin;
        private static Image Piece(RectTransform stage, Vector2 point, Vector2 size, Color color, bool round)
        {
            var image = LabUI.Panel(stage, "ეფექტი", color);
            if (round) image.sprite = DecorArt.Soft();
            image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
            image.rectTransform.anchoredPosition = point; image.rectTransform.sizeDelta = size;
            return image;
        }
        private static void Spawn(RectTransform stage, Vector2 point, Vector2 size, Color color, Vector2 velocity, float life, float grow, bool round, float spin = 0)
        {
            var image = Piece(stage, point, size, color, round);
            var fx = image.gameObject.AddComponent<UIFx>();
            fx.image = image; fx.rect = image.rectTransform; fx.velocity = velocity; fx.life = life; fx.grow = grow; fx.spin = spin;
        }
        public static void Burst(RectTransform stage, Vector2 point, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dir = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(60, 180);
                Spawn(stage, point, Vector2.one * UnityEngine.Random.Range(5, 10), color, dir, 0.8f, -4, false, UnityEngine.Random.Range(-200, 200));
            }
        }
        private void Update()
        {
            age += Time.unscaledDeltaTime;
            if (age >= life) { Destroy(gameObject); return; }
            rect.anchoredPosition += velocity * Time.unscaledDeltaTime;
            rect.sizeDelta = Vector2.Max(Vector2.one * 2, rect.sizeDelta + Vector2.one * grow * Time.unscaledDeltaTime);
            rect.localRotation = Quaternion.Euler(0, 0, rect.localEulerAngles.z + spin * Time.unscaledDeltaTime);
            var c = image.color; c.a = Mathf.Min(c.a, 1 - age / life); image.color = c;
        }
    }

    public sealed class Wobble : MonoBehaviour
    {
        private float age;
        private void Update()
        {
            age += Time.unscaledDeltaTime;
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(age * 35) * 4 * (1 - age / 0.9f));
            if (age > 0.9f) { transform.localRotation = Quaternion.identity; Destroy(this); }
        }
    }

    /// <summary>
    /// Harmless leftovers of failed experiments: puddles, foam and dust that wait on the floor
    /// until the player cleans them up. They are saved, so a messy lab stays messy until tidied.
    /// </summary>
    public sealed class SafeMishaps : MonoBehaviour
    {
        private LabGame game;
        private readonly List<GameObject> spills = new List<GameObject>();
        public void Initialize(LabGame lab)
        {
            game = lab;
            int pending = Mathf.Min(GameSaveManager.Instance.Data.pendingSpills, 4);
            GameSaveManager.Instance.Data.pendingSpills = 0;
            for (int i = 0; i < pending; i++) Place(game.Personal.Room, game.Personal.SpawnPosition - game.Personal.RoomOrigin + new Vector2(-3 + i * 2, -2.6f), i % 3);
        }

        public void Spawn(FxKind kind)
        {
            if (GameSaveManager.Instance.Data.pendingSpills >= 4) return;
            var player = (Vector2)game.player.transform.position + new Vector2(UnityEngine.Random.Range(-1.2f, 1.2f), -1.1f);
            bool personal = game.Personal.Inside;
            var parent = personal ? game.Personal.Room : game.Areas.InArea ? null : game.classroom;
            if (parent == null) return;
            Vector2 local = parent.InverseTransformPoint(player);
            Place(parent, local, kind == FxKind.Foam ? 1 : kind == FxKind.Burst ? 2 : 0);
        }

        private void Place(Transform parent, Vector2 local, int look)
        {
            GameSaveManager.Instance.Data.pendingSpills++;
            GameSaveManager.Instance.Commit();
            var art = LaboratoryArt.Draw(parent, "უვნებელი ავარია", local, DecorArt.Puddle(look), parent == game.classroom ? 3 : -298);
            spills.Add(art.gameObject);
            WorldInteractable.Create(art.transform, Vector2.zero, new Vector2(1.6f, 1.2f), 1.6f, () => "[E] დასუფთავება", () => Clean(art.gameObject), null, Glow.Urgent);
        }

        private void Clean(GameObject spill)
        {
            if (spill == null) return;
            spills.Remove(spill);
            var data = GameSaveManager.Instance.Data;
            data.pendingSpills = Mathf.Max(0, data.pendingSpills - 1);
            LabProgress.CountStat("clean");
            if (LabProgress.Stat("clean") >= 10) LabProgress.Collect("badge:cleaner");
            LabProgress.AddReputation(1);
            GameSaveManager.Instance.Commit();
            for (int i = 0; i < 8; i++)
                LabAtmosphere.Instance?.Puff(spill.transform.parent, spill.transform.localPosition, new Color(1f, 1f, 0.8f), UnityEngine.Random.insideUnitCircle * 1.2f, 0.6f, 12);
            AmbientAudio.Instance?.Sparkle();
            game.HUD.Notify("დასუფთავებულია! სუფთა ლაბორატორია ბედნიერი ლაბორატორიაა. +1 რეპუტაცია");
            Destroy(spill);
        }
    }
}
