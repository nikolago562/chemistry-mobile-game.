using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// The first-minute tutorial: a pulsing ring, a bouncing arrow and one caption that point at
    /// exactly one thing at a time (joystick → closet → use button → vinegar → soda → guess → mix).
    /// The bench only accepts vinegar and soda until that first mix, so the path can't be skipped.
    /// Removes itself once the first discovery is made.
    /// </summary>
    public sealed class Coach : MonoBehaviour
    {
        private LabGame game;
        private RectTransform root, ring, arrow, bubble;
        private TMP_Text caption;
        private Vector3? start;
        private bool moved, taught;
        private float goalUntil;
        private static Sprite ringSprite, arrowSprite;
        private readonly Vector3[] corners = new Vector3[4];

        public string Caption => root != null && root.gameObject.activeSelf ? caption.text : "";

        public static Coach Create(LabGame lab)
        {
            var go = LabUI.Object(LabUI.Root, "სწავლება");
            LabUI.Fill((RectTransform)go.transform);
            var coach = go.AddComponent<Coach>();
            coach.game = lab;
            // Visuals live on a child so hiding them never stops this script.
            coach.Build((RectTransform)LabUI.Object(go.transform, "ხედი").transform);
            return coach;
        }

        private void Build(RectTransform self)
        {
            root = self;
            LabUI.Fill(root);
            ring = Image("რგოლი", RingSprite, LabUI.Gold);
            var frame = ring.GetComponent<Image>(); frame.type = UnityEngine.UI.Image.Type.Sliced; frame.pixelsPerUnitMultiplier = 1 / LabUI.PixelScale;
            arrow = Image("ისარი", ArrowSprite, Color.white);
            arrow.sizeDelta = new Vector2(44, 48);
            var panel = LabUI.Panel(root, "მინიშნება", LabUI.Navy);
            panel.raycastTarget = false;
            bubble = panel.rectTransform;
            bubble.sizeDelta = new Vector2(360, 64);
            caption = LabUI.Text(bubble, "", 19, Color.white);
            caption.alignment = TextAlignmentOptions.Center; caption.fontStyle = FontStyles.Bold;
            caption.enableAutoSizing = true; caption.fontSizeMin = 12; caption.fontSizeMax = 19;
            LabUI.Fill(caption.rectTransform, 10);
        }

        private RectTransform Image(string name, Sprite sprite, Color color)
        {
            var image = LabUI.Object(root, name, typeof(Image)).GetComponent<Image>();
            image.sprite = sprite; image.color = color; image.raycastTarget = false;
            return image.rectTransform;
        }

        /// <summary>What to point at now and what to say, or (null, null) to stay hidden.</summary>
        private (RectTransform target, string text) Step()
        {
            var data = GameSaveManager.Instance.Data;
            if (!data.profileCompleted || game.Dialogue.IsOpen) return (null, null);
            var touch = TouchControls.Instance;
            if (!data.starterCoatCollected)
            {
                if (LabUI.AnyOpen) return (null, null);
                if (!moved) return (touch != null ? touch.Pad : null, "გადაათრიე ცერი აქ, რომ იარო");
                return (null, "მიჰყევი ყვითელ ისრებს — კარადაში ხალათი გელოდება");
            }
            var bench = game.Lessons.Bench;
            if (!MixBenchUI.Teaching)
            {
                if (LabUI.AnyOpen) return (null, null);
                if (goalUntil == 0) goalUntil = Time.unscaledTime + 5;
                if (Time.unscaledTime < goalUntil) return (game.HUD.Tracker, "აქ ყოველთვის ჩანს, რა გააკეთო შემდეგ");
                if (Time.unscaledTime < goalUntil + 20) return (null, "მასწავლებელს დაელაპარაკე — ის სხვა მაგიდებსაც გაგაცნობს");
                Destroy(gameObject);
                return (null, null);
            }
            if (!bench.IsOpen)
            {
                if (LabUI.AnyOpen) return (null, null);
                bool atBench = Focus.Current != null && Focus.Current.IsFocused && Focus.Current.GetComponent<InteractableStation>()?.stationType == StationType.ReactionTable;
                return atBench ? (touch != null ? touch.Action : null, "შეეხე „გამოყენებას“") : (null, "მიდი მბზინავ რეაქციების მაგიდასთან");
            }
            if (!bench.Picked.Contains("vinegar")) return (bench.Spot("vinegar"), "შეეხე ძმარს");
            if (!bench.Picked.Contains("soda")) return (bench.Spot("soda"), "ახლა — სოდას");
            if (bench.Guessed == null) return (bench.Spot("guess"), "რას ფიქრობ, რა მოხდება? აირჩიე ერთი");
            return (bench.Spot("mix"), "შეურიე!");
        }

        private void LateUpdate()
        {
            var data = GameSaveManager.Instance.Data;
            if (MixBenchUI.Teaching) taught = true;
            // Once the first mix is done: show where the goal lives, then send the player to the teacher.
            else if (data.starterCoatCollected && (!taught || game.Dialogue.IsOpen)) { Destroy(gameObject); return; }
            if (start == null && game.player != null) start = game.player.transform.position;
            if (!moved && start is Vector3 from && Vector3.Distance(from, game.player.transform.position) > 1.2f) moved = true;
            var (target, text) = Step();
            bool visible = text != null;
            if (root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
            if (!visible) return;
            if (transform.GetSiblingIndex() != transform.parent.childCount - 1) transform.SetAsLastSibling();
            caption.text = text;
            bool pointing = target != null && target.gameObject.activeInHierarchy;
            ring.gameObject.SetActive(pointing); arrow.gameObject.SetActive(pointing);
            float t = Time.unscaledTime;
            if (!pointing)
            {
                // No single thing to point at: the caption sits low in the middle of the screen.
                bubble.anchorMin = bubble.anchorMax = new Vector2(0.5f, 0); bubble.pivot = new Vector2(0.5f, 0.5f);
                bubble.anchoredPosition = new Vector2(0, 120);
                return;
            }
            target.GetWorldCorners(corners);
            Vector3 centre = (corners[0] + corners[2]) / 2;
            Vector2 size = (corners[2] - corners[0]) / root.lossyScale.x;
            ring.position = centre;
            ring.sizeDelta = (size + Vector2.one * 22) * (1 + 0.04f * Mathf.Sin(t * 6));
            // The arrow and caption go above the target, or below it when it is near the top.
            Vector2 local = root.InverseTransformPoint(centre);
            bool below = local.y > root.rect.height * 0.25f;
            float edge = (below ? -1 : 1) * (ring.sizeDelta.y / 2 + 30 + 8 * Mathf.Abs(Mathf.Sin(t * 5)));
            arrow.position = centre;
            arrow.anchoredPosition += new Vector2(0, edge);
            arrow.localRotation = Quaternion.Euler(0, 0, below ? 180 : 0);
            bubble.anchorMin = bubble.anchorMax = new Vector2(0.5f, 0.5f);
            bubble.position = centre;
            var pos = bubble.anchoredPosition + new Vector2(0, edge + (below ? -62 : 62));
            float halfW = root.rect.width / 2 - bubble.sizeDelta.x / 2 - 12, halfH = root.rect.height / 2 - bubble.sizeDelta.y / 2 - 12;
            bubble.anchoredPosition = new Vector2(Mathf.Clamp(pos.x, -halfW, halfW), Mathf.Clamp(pos.y, -halfH, halfH));
        }

        /// <summary>A hollow pixel frame (9-sliced) that hugs any target, round button or wide card.</summary>
        private static Sprite RingSprite
        {
            get
            {
                if (ringSprite != null) return ringSprite;
                var texture = new Texture2D(12, 12, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    bool corner = (x < 2 || x > 9) && (y < 2 || y > 9) && !((x == 1 || x == 10) && (y == 1 || y == 10));
                    bool band = x < 3 || x > 8 || y < 3 || y > 8;
                    texture.SetPixel(x, y, band && !corner ? Color.white : Color.clear);
                }
                texture.Apply();
                return ringSprite = Sprite.Create(texture, new Rect(0, 0, 12, 12), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, Vector4.one * 4);
            }
        }

        /// <summary>A chunky gold arrow pointing down, with a dark rim.</summary>
        private static Sprite ArrowSprite => arrowSprite ??= PixelArt.Make(11, 12, (x, y) =>
        {
            bool head = y <= 5 && Mathf.Abs(x - 5) <= y;
            bool shaft = y > 5 && x >= 3 && x <= 7;
            bool rimHead = y <= 6 && Mathf.Abs(x - 5) <= y + 1;
            bool rimShaft = y > 4 && x >= 2 && x <= 8;
            if ((head || shaft) && !(y == 11 && shaft)) return new Color32(255, 206, 70, 255);
            return rimHead || rimShaft ? new Color32(110, 70, 20, 255) : Color.clear;
        });
    }
}
