using System.Linq;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class PlayerCosmetics : MonoBehaviour
    {
        private LabGame game;
        private SpriteRenderer playerSprite, accessory, desk;
        private TMP_Text title;
        private Material clothing;
        private CosmeticItemSO accessoryItem;

        public void Initialize(LabGame lab)
        {
            game = lab;
            playerSprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            clothing = playerSprite.material;
            var shadow = Renderer("ფეხების ჩრდილი", transform, new Vector3(0, 0.05f), 0);
            shadow.sprite = PixelArt.Make(18, 6, (x, y) =>
                Mathf.Pow((x - 8.5f) / 8f, 2) + Mathf.Pow((y - 2.5f) / 2.5f, 2) < 1
                    ? new Color(0.12f, 0.07f, 0.06f, 0.32f) : Color.clear);
            accessory = Renderer("სათვალე ან ქუდი", transform, Vector3.zero, 7);
            desk = Renderer("პირადი მაგიდის დეკორი", game.classroom, SchoolLayout.ElementBench + new Vector2(0, 1.5f), 7);
            var label = new GameObject("მოთამაშის წოდება", typeof(RectTransform), typeof(Canvas));
            label.transform.SetParent(transform, false);
            label.transform.localPosition = new Vector3(0, 2.55f);
            label.transform.localScale = Vector3.one / 70f;
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 76);
            label.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            label.GetComponent<Canvas>().sortingOrder = 10;
            title = LabUI.Text(label.transform, "", 20, Color.white);
            title.alignment = TextAlignmentOptions.Center;
            LabUI.WorldLabel(title);
            LabUI.Fill(title.rectTransform);
            GameSaveManager.Instance.Changed += Refresh;
            Refresh();
        }

        private static SpriteRenderer Renderer(string name, Transform parent, Vector3 position, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            return renderer;
        }

        private void Refresh()
        {
            var data = GameSaveManager.Instance.Data;
            PlayerAppearance.Apply(clothing, data.skinTone, data.hairColor, data.shirtColor);
            var coat = game.cosmetics.FirstOrDefault(c => c.itemID == data.equippedLabCoatID);
            clothing.SetFloat("_HasCoat", coat != null ? 1 : 0);
            clothing.SetColor("_CoatColor", coat != null ? coat.color : Color.white);
            accessoryItem = game.cosmetics.FirstOrDefault(c => c.itemID == data.equippedHatOrGogglesID);
            var decor = game.cosmetics.FirstOrDefault(c => c.itemID == data.equippedDeskDecorID);
            accessory.enabled = accessoryItem != null;
            if (accessoryItem != null)
            {
                bool goggles = accessoryItem.category == CosmeticCategory.SafetyGoggles;
                accessory.sprite = goggles ? PixelArt.Goggles() : PixelArt.Hat(accessoryItem.itemID == "weekly_crown");
                accessory.color = accessoryItem.color;
                accessory.transform.localPosition = new Vector3(0, goggles ? 1.0f : 1.84f);
                accessory.transform.localScale = Vector3.one * (goggles ? 0.65f : 0.8f);
            }
            desk.enabled = decor != null;
            if (decor != null)
            {
                desk.sprite = decor.itemID == "decor_globe" ? PixelArt.Globe()
                    : decor.itemID == "decor_poster" ? PixelArt.Poster() : PixelArt.Flask();
                desk.color = decor.color;
                desk.transform.localPosition = SchoolLayout.ElementBench + new Vector2(0, 1.45f - desk.sprite.bounds.min.y);
            }
            title.richText = false;
            title.text = (data.profileCompleted ? data.playerName + "\n" : "") + game.Rankings.Title;
        }

        private void LateUpdate()
        {
            if (desk.enabled)
            {
                float glow = 0.93f + Mathf.Sin(Time.time * 2f) * 0.07f;
                desk.color = new Color(glow, glow, 1);
            }
            if (accessoryItem == null || accessoryItem.category != CosmeticCategory.SafetyGoggles) return;
            var facing = game.player.Facing;
            accessory.enabled = facing.y <= 0.4f;
            accessory.transform.localScale = new Vector3(Mathf.Abs(facing.x) > 0.5f ? 0.35f : 0.65f, 0.65f, 0.65f);
        }

        private void OnDestroy()
        {
            if (GameSaveManager.Instance != null) GameSaveManager.Instance.Changed -= Refresh;
            if (clothing != null) Destroy(clothing);
        }
    }
}
