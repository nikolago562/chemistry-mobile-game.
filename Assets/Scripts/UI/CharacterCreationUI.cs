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
            window = LabUI.Window("შენი პირველი დღე იწყება", 1000, 640, false);
            var intro = LabUI.Text(window.Body, "გაგვეცანი და შექმენი შენი პატარა ქიმიკოსი.", 21, LabUI.Muted);
            LabUI.Size(intro.gameObject, 32);
            var split = LabUI.Row(window.Body, "პერსონაჟის შექმნა", 26);
            LabUI.FlexibleHeight(split.gameObject);
            var card = LabUI.Card(split.transform, "შენი პერსონაჟი", LabUI.Navy, 20);
            LabUI.Size(card.gameObject, 350, 290, 0);
            previewName = LabUI.Text(card.transform, "შენი ქიმიკოსი", 24, Color.white);
            previewName.richText = false;
            previewName.alignment = TextAlignmentOptions.Center;
            LabUI.Size(previewName.gameObject, 34);
            portrait = LabUI.Object(card.transform, "პიქსელური პერსონაჟი", typeof(Image)).GetComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            LabUI.Size(portrait.gameObject, 160);
            var source = game.player.transform.Find("Sprite").GetComponent<SpriteRenderer>();
            previewMaterial = new Material(source.sharedMaterial);
            previewMaterial.SetFloat("_HasCoat", 0);
            portrait.material = previewMaterial;
            var texture = source.sprite.texture;
            for (int row = 0; row < 8; row++)
                previewFrames.Add(Sprite.Create(texture, new Rect(8, texture.height - row * 32 - 24, 16, 16), Vector2.one * 0.5f, 8));
            var rotate = LabUI.Row(card.transform, "მობრუნება", 8);
            LabUI.Size(LabUI.Secondary(rotate.transform, "←", () => Turn(-1)).gameObject, 42, 60);
            var rotateLabel = LabUI.Text(rotate.transform, "მოაბრუნე", 17, Color.white);
            rotateLabel.alignment = TextAlignmentOptions.Center;
            LabUI.Size(rotateLabel.gameObject, 42, -1, 1);
            LabUI.Size(LabUI.Secondary(rotate.transform, "→", () => Turn(1)).gameObject, 42, 60);
            var note = LabUI.Text(card.transform, "ხალათს ლაბორატორიაში მიიღებ", 16, new Color(0.8f, 0.82f, 0.94f));
            note.alignment = TextAlignmentOptions.Center;
            LabUI.Size(note.gameObject, 36);

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
            LabUI.Size(LabUI.Text(row.transform, label, 19).gameObject, 48, -1, 1);
            for (int i = 0; i < colors.Length; i++)
            {
                int choice = i;
                var button = LabUI.Button(row.transform, "", () => { set(choice); RefreshPreview(); }, 44);
                LabUI.Size(button.gameObject, 44, 44, 0);
                button.GetComponent<Image>().sprite = LabUI.Rounded();
                button.GetComponent<Image>().color = colors[i];
                var check = button.GetComponentInChildren<TMP_Text>();
                check.color = i == 0 && label == "კანის ტონი" || i == 2 && label == "თმის ფერი" ? LabUI.Ink : Color.white;
                refreshChoices.Add(() => check.text = selected() == choice ? "●" : "");
            }
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

