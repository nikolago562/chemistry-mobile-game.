using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class CharacterCreationUI : MonoBehaviour
    {
        public TMP_InputField NameInput { get; private set; }
        public int Skin { get; private set; }
        public int Hair { get; private set; }
        public int Shirt { get; private set; }
        public bool IsOpen => window != null;
        private LabGame game;
        private LabWindow window;
        private Action ready;
        private Material previewMaterial;
        private Image portrait;
        private TMP_Text previewName, error;
        private readonly List<Sprite> previewFrames = new List<Sprite>();
        private readonly List<Action> refreshChoices = new List<Action>();
        private int direction;

        public void Initialize(LabGame lab) => game = lab;
        public void Begin(Action onReady)
        {
            if (GameSaveManager.Instance.Data.profileCompleted) { onReady?.Invoke(); return; }
            if (window != null) return;
            ready = onReady;
            var data = GameSaveManager.Instance.Data;
            Skin = Mathf.Clamp(data.skinTone, 0, 3);
            Hair = Mathf.Clamp(data.hairColor, 0, 3);
            Shirt = Mathf.Clamp(data.shirtColor, 0, 3);
            window = LabUI.Window("აკადემიაში რეგისტრაცია", 1020, 650, false);
            var intro = LabUI.Text(window.Body, "კეთილი იყოს შენი მობრძანება ქიმიის აკადემიაში! შეავსე მოსწავლის ბარათი.", 20, LabUI.Muted);
            LabUI.Size(intro.gameObject, 30);
            var split = LabUI.Row(window.Body, "პერსონაჟის შექმნა", 26);
            LabUI.FlexibleHeight(split.gameObject);
            // The student card: a green strip with the academy's name, a photo and the name on it.
            var card = LabUI.Card(split.transform, "მოსწავლის ბარათი", LabUI.Paper, 0);
            LabUI.Size(card.gameObject, 372, 330, 0);
            card.GetComponent<VerticalLayoutGroup>().spacing = 8;
            card.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 14);
            LabUI.Shadow(card, 8);
            var strip = LabUI.Panel(card.transform, "ზოლი", LabUI.Green); LabUI.Size(strip.gameObject, 46);
            var stripText = LabUI.Text(strip.transform, "<b>ქიმიის აკადემია</b>  <size=14>• მოსწავლის ბარათი</size>", 18, Color.white);
            stripText.alignment = TextAlignmentOptions.Center; LabUI.Fill(stripText.rectTransform, 6);
            var photo = LabUI.Panel(card.transform, "ფოტო", LabUI.Navy); LabUI.Size(photo.gameObject, 196);
            var light = UIArt.Picture(photo.transform, "შუქი", DecorArt.Soft(), new Vector2(300, 300), new Color(0.55f, 0.85f, 0.75f, 0.35f));
            portrait = LabUI.Object(photo.transform, "პიქსელური პერსონაჟი", typeof(Image)).GetComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            var pr = portrait.rectTransform; pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f); pr.sizeDelta = new Vector2(176, 176);
            var source = game.player.transform.Find("Sprite").GetComponent<SpriteRenderer>();
            previewMaterial = new Material(source.sharedMaterial);
            previewMaterial.SetFloat("_HasCoat", 0);
            portrait.material = previewMaterial;
            var texture = source.sprite.texture;
            for (int row = 0; row < 8; row++)
                previewFrames.Add(Sprite.Create(texture, new Rect(8, texture.height - row * 32 - 24, 16, 16), Vector2.one * 0.5f, 8));
            var turnLeft = LabUI.Secondary(photo.transform, "←", () => Turn(-1)); Corner(turnLeft, 0);
            var turnRight = LabUI.Secondary(photo.transform, "→", () => Turn(1)); Corner(turnRight, 1);
            previewName = LabUI.Text(card.transform, "შენი ქიმიკოსი", 26, LabUI.Ink);
            previewName.richText = false; previewName.fontStyle = FontStyles.Bold;
            previewName.alignment = TextAlignmentOptions.Center;
            LabUI.Size(previewName.gameObject, 34);
            var note = LabUI.Text(card.transform, "კლასი: დამწყები ქიმიკოსი  •  № " + (1000 + UnityEngine.Random.Range(1, 9000)) + "\nხალათს პირველ დღეს კარადაში მიიღებ", 14, LabUI.Muted);
            note.alignment = TextAlignmentOptions.Center;
            LabUI.Size(note.gameObject, 40);

            var settings = LabUI.Column(split.transform, "შენი არჩევანი", 10);
            LabUI.Size(settings.gameObject, 350, -1, 1);
            LabUI.Size(LabUI.Text(settings.transform, "რა გქვია?", 23).gameObject, 30);
            NameInput = LabUI.Input(settings.transform, "შენი სახელი…");
            NameInput.characterLimit = 18;
            NameInput.textComponent.richText = false;
            NameInput.onValidateInput = (text, index, c) => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '\'' ? c : '\0';
            NameInput.onValueChanged.AddListener(value =>
            {
                previewName.text = string.IsNullOrWhiteSpace(value) ? "შენი ქიმიკოსი" : value.Trim();
                if (error != null)
                {
                    error.text = "სახელი და გარეგნობა შენს პროგრესთან ერთად შეინახება.";
                    error.color = LabUI.Muted;
                }
            });
            Choices(settings.transform, "კანის ტონი", PlayerAppearance.Skin, () => Skin, value => Skin = value);
            Choices(settings.transform, "თმის ფერი", PlayerAppearance.Hair, () => Hair, value => Hair = value);
            Choices(settings.transform, "მაისურის ფერი", PlayerAppearance.Shirt, () => Shirt, value => Shirt = value);
            error = LabUI.Text(window.Body, "სახელი და გარეგნობა შენს პროგრესთან ერთად შეინახება.", 16, LabUI.Muted);
            LabUI.Size(error.gameObject, 30);
            var footer = LabUI.Row(window.Body, "დაწყება");
            var tip = LabUI.Text(footer.transform, "შენი არჩევანი • შენი თავგადასავალი", 17, LabUI.Muted);
            LabUI.Size(tip.gameObject, 48, -1, 1);
            LabUI.Size(LabUI.Button(footer.transform, "ლაბორატორიაში შესვლა", () => Confirm(), 50).gameObject, 50, 300, 0);
            NameInput.onSubmit.AddListener(_ => Confirm());
            RefreshPreview();
        }

        private void Choices(Transform parent, string label, Color[] colors, Func<int> selected, Action<int> set)
        {
            var row = LabUI.Row(parent, label, 10);
            LabUI.Size(row.gameObject, 60);
            var name = LabUI.Text(row.transform, label, 19, LabUI.Ink); name.fontStyle = FontStyles.Bold; name.alignment = TextAlignmentOptions.MidlineLeft;
            LabUI.Size(name.gameObject, 52, -1, 1);
            for (int i = 0; i < colors.Length; i++)
            {
                int choice = i;
                var button = LabUI.Button(row.transform, "", () => { set(choice); RefreshPreview(); Juice.Bounce(row.transform.GetChild(choice + 1)); }, 52);
                LabUI.Size(button.gameObject, 52, 52, 0);
                var image = button.GetComponent<Image>();
                image.sprite = UIArt.Swatch; image.type = Image.Type.Simple; image.color = colors[i]; image.preserveAspect = true;
                var mark = UIArt.Picture(button.transform, "არჩეული", UIArt.SelectRing, new Vector2(64, 64), LabUI.Gold);
                button.GetComponentInChildren<TMP_Text>().text = "";
                refreshChoices.Add(() => mark.enabled = selected() == choice);
            }
        }
        /// <summary>The turn buttons sit in the photo's lower corners.</summary>
        private static void Corner(Button button, int side)
        {
            button.GetComponent<LayoutElement>().ignoreLayout = true;
            var r = (RectTransform)button.transform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(side, 0);
            r.anchoredPosition = new Vector2(side == 0 ? 8 : -8, 8); r.sizeDelta = new Vector2(48, 40);
        }
        public void SelectAppearance(int skin, int hair, int shirt)
        {
            Skin = Mathf.Clamp(skin, 0, 3); Hair = Mathf.Clamp(hair, 0, 3); Shirt = Mathf.Clamp(shirt, 0, 3);
            RefreshPreview();
        }
        private void Turn(int amount) { direction = (direction + amount + 8) % 8; RefreshPreview(); }
        private void RefreshPreview()
        {
            if (previewMaterial == null) return;
            PlayerAppearance.Apply(previewMaterial, Skin, Hair, Shirt);
            portrait.sprite = previewFrames[direction];
            foreach (var refresh in refreshChoices) refresh();
        }
        public bool Confirm()
        {
            if (window == null) return false;
            string name = string.Join(" ", NameInput.text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            if (name.Length == 0)
            {
                error.text = "ჯერ შენი სახელი ჩაწერე.";
                error.color = LabUI.Coral;
                NameInput.ActivateInputField();
                return false;
            }
            var save = GameSaveManager.Instance;
            save.Data.playerName = name;
            save.Data.skinTone = Skin; save.Data.hairColor = Hair; save.Data.shirtColor = Shirt;
            save.Data.profileCompleted = true;
            save.Commit();
            window.CanClose = true;
            window.Close();
            window = null;
            Cleanup();
            ready?.Invoke();
            ready = null;
            return true;
        }
        private void Cleanup()
        {
            if (previewMaterial != null) Destroy(previewMaterial);
            foreach (var frame in previewFrames) Destroy(frame);
            previewFrames.Clear(); refreshChoices.Clear();
        }
        private void OnDestroy() => Cleanup();
    }
}

