using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// Behaviour of personal-lab decorations: aquariums swim, plasma globes crackle, lamps and
    /// glowing jars warm the evening, display furniture shows what the player has achieved, the
    /// radio changes music, posters teach facts and plants need watering.
    /// </summary>
    public sealed class PersonalDecor : MonoBehaviour
    {
        /// <summary>Calm score of the personal lab (plants, water, soft furniture, music).</summary>
        public static int Calm
        {
            get
            {
                var data = GameSaveManager.Instance.Data;
                int score = 0;
                foreach (var p in data.labPlacements)
                    score += p.itemID == "plant" || p.itemID == "ficus" ? (data.labPlantsWateredDay == LabProgress.Today ? 2 : 1)
                        : p.itemID == "aquarium" ? 3 : p.itemID == "sofa" || p.itemID == "rug" || p.itemID == "lamp" || p.itemID == "season" || p.itemID == "crystal_lamp" ? 1
                        : p.itemID == "radio" && data.radioOn ? 1 : 0;
                return score;
            }
        }
        public static string CalmName => Calm >= 9 ? "ძალიან მყუდრო" : Calm >= 5 ? "მყუდრო" : Calm >= 2 ? "სასიამოვნო" : "ცარიელი";

        private string id;
        private SpriteRenderer art;
        private readonly List<Transform> fish = new List<Transform>();
        private readonly List<SpriteRenderer> pieces = new List<SpriteRenderer>();
        private SpriteRenderer glow;
        private int shownCount = -1;
        private float nextPuff;

        public void Initialize(string itemID, SpriteRenderer renderer)
        {
            id = itemID; art = renderer;
            if (id == "aquarium")
                for (int i = 0; i < 3; i++)
                {
                    var f = LaboratoryArt.Draw(transform, "თევზი", new Vector2(0, 1.4f + i * 0.3f), DecorArt.Fish(i), 1);
                    fish.Add(f.transform);
                }
            Color? light = id == "lamp" ? new Color(1f, 0.8f, 0.45f) : id == "plasma" ? new Color(0.75f, 0.45f, 1f) : id == "crystal_lamp" ? new Color(0.7f, 0.85f, 1f)
                : id == "glow_jar" ? new Color(0.45f, 0.95f, 1f) : id == "aquarium" ? new Color(0.45f, 0.8f, 1f) : (Color?)null;
            if (light.HasValue && LabAtmosphere.Instance != null)
                glow = LabAtmosphere.Instance.AddGlowRenderer(transform, new Vector2(0, id == "lamp" ? 1.7f : 1.2f), light.Value, id == "lamp" ? 3.4f : 2.2f, id != "plasma");
        }

        private void Update()
        {
            var data = GameSaveManager.Instance.Data;
            float t = Time.time;
            switch (id)
            {
                case "aquarium":
                    for (int i = 0; i < fish.Count; i++)
                    {
                        float x = Mathf.Sin(t * (0.4f + i * 0.17f) + i * 2) * 0.85f;
                        fish[i].localPosition = new Vector2(x, 1.25f + i * 0.28f + Mathf.Sin(t * 1.3f + i) * 0.06f);
                        fish[i].localScale = new Vector3(Mathf.Cos(t * (0.4f + i * 0.17f) + i * 2) >= 0 ? 1 : -1, 1, 1);
                    }
                    if (t > nextPuff) { nextPuff = t + Random.Range(0.6f, 1.5f); LabAtmosphere.Instance?.Puff(transform, new Vector2(Random.Range(-0.8f, 0.8f), 1.1f), new Color(0.9f, 1, 1, 0.8f), new Vector2(0, 0.35f), 1.6f, 2); }
                    break;
                case "plasma":
                    art.color = Color.Lerp(Color.white, new Color(1f, 0.8f, 1f), 0.5f + 0.5f * Mathf.Sin(t * 7));
                    if (t > nextPuff) { nextPuff = t + 0.12f; LabAtmosphere.Instance?.Puff(transform, new Vector2(0, 1.06f), new Color(0.85f, 0.6f, 1f), Random.insideUnitCircle.normalized * 0.9f, 0.4f, 2, DecorArt.Square(), 1.5f); }
                    break;
                case "radio":
                    if (data.radioOn && t > nextPuff) { nextPuff = t + 0.9f; LabAtmosphere.Instance?.Puff(transform, new Vector2(0.3f, 1.5f), LabUI.Gold, new Vector2(0.2f, 0.6f), 1.6f, 2, DecorArt.Bubble(), 1.4f); }
                    break;
                case "lamp":
                    if (glow != null) glow.enabled = data.lampOn;
                    break;
                case "plant":
                case "ficus":
                    bool watered = data.labPlantsWateredDay == LabProgress.Today;
                    art.color = watered ? Color.white : new Color(0.85f, 0.85f, 0.72f);
                    transform.localScale = new Vector3(1, (watered ? 1 : 0.96f) + Mathf.Sin(t * 1.2f + transform.position.x) * 0.012f, 1);
                    break;
                case "display": Shelf(data.projects.Where(p => p.timesCompleted > 0).Select(p => (Color)ResearchProjects.Get(p.id).Color).ToList(), DecorArt.Jar(), 3, new Vector2(-0.6f, 0.45f), new Vector2(0.55f, 0.56f)); break;
                case "samples": Shelf(LabProgress.Catalog.Where(c => (c.Kind == CollectionKind.Sample || c.Kind == CollectionKind.Mineral) && LabProgress.Owns(c.ID)).Select(c => c.Color).ToList(), DecorArt.Jar(), 4, new Vector2(-0.65f, 0.25f), new Vector2(0.42f, 0.62f)); break;
                case "trophy": Shelf(LabProgress.Catalog.Where(c => (c.Kind == CollectionKind.Award || c.Kind == CollectionKind.Badge) && LabProgress.Owns(c.ID)).Select(c => c.Color).ToList(), DecorArt.Medal(new Color32(239, 194, 102, 255)), 4, new Vector2(-0.55f, 0.9f), new Vector2(0.36f, 0.56f)); break;
                case "photos": Shelf(LabProgress.Items(CollectionKind.Photo).Where(c => LabProgress.Owns(c.ID)).Select(c => c.Color).ToList(), DecorArt.Photo(new Color32(255, 255, 255, 255)), 4, new Vector2(-0.68f, 0.7f), new Vector2(0.45f, 0.45f)); break;
                case "research": Shelf(ResearchProjects.Active.Select(p => (Color)ResearchProjects.Get(p.id).Color).ToList(), DecorArt.Note(new Color32(255, 255, 255, 255)), 4, new Vector2(-0.6f, 0.95f), new Vector2(0.42f, 0.42f)); break;
            }
        }

        private void Shelf(List<Color> colors, Sprite sprite, int perRow, Vector2 start, Vector2 step)
        {
            if (colors.Count == shownCount) return;
            shownCount = colors.Count;
            foreach (var piece in pieces) if (piece != null) Destroy(piece.gameObject);
            pieces.Clear();
            for (int i = 0; i < Mathf.Min(colors.Count, perRow * 3); i++)
            {
                var piece = LaboratoryArt.Draw(transform, "ჩვენება", start + new Vector2((i % perRow) * step.x, (i / perRow) * step.y), sprite, 1);
                piece.color = colors[i];
                pieces.Add(piece);
            }
        }

        /// <summary>What happens when the player presses E next to the decoration.</summary>
        public static void Use(LabGame game, string id)
        {
            var data = GameSaveManager.Instance.Data;
            switch (id)
            {
                case "research": game.Research.Open(); break;
                case "display": game.Collection.Open(CollectionKind.Photo); break;
                case "photos": game.Collection.Open(CollectionKind.Photo); break;
                case "samples": game.Collection.Open(CollectionKind.Sample); break;
                case "mineral_case": game.Collection.Open(CollectionKind.Mineral); break;
                case "trophy": case "fair_trophy": game.Collection.Open(CollectionKind.Award); break;
                case "steam_wheel":
                    AmbientAudio.Instance?.Sparkle();
                    game.HUD.Notify("ბორბალი ისევ ტრიალებს. ორთქლს მოძრაობის ძალა აქვს!");
                    break;
                case "storage": game.Collection.OpenTab("materials"); break;
                case "radio":
                    if (!data.radioOn) { data.radioOn = true; data.radioStation = 0; }
                    else if (data.radioStation >= AmbientAudio.Stations.Length - 1) data.radioOn = false;
                    else data.radioStation++;
                    GameSaveManager.Instance.Commit();
                    game.HUD.Notify(data.radioOn ? "რადიო: " + AmbientAudio.Stations[data.radioStation] : "რადიო გამორთულია. სიჩუმეც სასიამოვნოა.");
                    break;
                case "lamp":
                    data.lampOn = !data.lampOn; GameSaveManager.Instance.Commit();
                    game.HUD.Notify(data.lampOn ? "ლამპა ჩაირთო. საღამო მყუდრო გახდა." : "ლამპა გამოირთო.");
                    break;
                case "plant":
                case "ficus":
                    if (data.labPlantsWateredDay == LabProgress.Today) { game.HUD.Notify("მცენარე დღეს უკვე მორწყულია. მოთმინება, ის იზრდება!"); break; }
                    data.labPlantsWateredDay = LabProgress.Today; LabProgress.CountStat("water"); GameSaveManager.Instance.Commit();
                    AmbientAudio.Instance?.Splash();
                    game.HUD.Notify("მცენარე მორწყულია. ოთახის სიმშვიდე: " + CalmName + ".");
                    break;
                case "poster_table": Fact(game, "fact:table"); break;
                case "poster_safety": Fact(game, "fact:oxygen"); break;
                case "aquarium":
                    game.HUD.Notify("თევზებმა საკვები სიხარულით დაიჭირეს. წყალში ჟანგბადი იხსნება, ამიტომაც სუნთქავენ ლაყუჩებით.");
                    LabProgress.Collect("fact:plants"); GameSaveManager.Instance.Commit();
                    break;
                case "plasma":
                    AmbientAudio.Instance?.Sparkle();
                    game.HUD.Notify("ზზზ! პლაზმა იონიზებული აირია. შეხებისას ელვა შენი ხელისკენ მოდის.");
                    break;
                case "sofa":
                    game.HUD.Notify("ჩამოჯექი და ისვენებ. ოთახის სიმშვიდე: " + CalmName + " (" + Calm + ").");
                    break;
                case "glow_jar": game.HUD.Notify("ქილაში ფერი ნელა იცვლება. ეს შენი ფერცვალებადი ხსნარის სუვენირია."); break;
                case "crystal_lamp": game.HUD.Notify("შენ მიერ გაზრდილი კრისტალი შუქს ცისარტყელად შლის."); break;
                case "season": game.HUD.Notify("სეზონური მორთულობა: " + SchoolCalendar.SeasonName + ". სკოლაც ასეა მორთული!"); break;
                default: game.HUD.Notify("ლამაზია!"); break;
            }
        }

        private static void Fact(LabGame game, string id)
        {
            var fact = LabProgress.Catalog.First(c => c.ID == id);
            LabProgress.Collect(id);
            GameSaveManager.Instance.Commit();
            game.Collection.ShowFact(fact);
        }
    }
}
