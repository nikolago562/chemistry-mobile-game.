using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>A trigger zone with an "[E] …" prompt that runs an action. Used for most new props.</summary>
    public sealed class WorldInteractable : MonoBehaviour
    {
        public Func<string> PromptText;
        public Func<bool> Available;
        public Action Use;
        private bool near;
        private GameObject prompt;
        private TMPro.TMP_Text label;

        public static WorldInteractable Create(Transform parent, Vector2 local, Vector2 size, float promptHeight, Func<string> text, Action use, Func<bool> available = null)
        {
            var go = new GameObject("ურთიერთქმედება", typeof(BoxCollider2D), typeof(WorldInteractable));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            var zone = go.GetComponent<BoxCollider2D>(); zone.isTrigger = true; zone.size = size;
            var item = go.GetComponent<WorldInteractable>();
            item.PromptText = text; item.Use = use; item.Available = available;
            item.prompt = new GameObject("მინიშნება");
            item.prompt.transform.SetParent(go.transform, false);
            PersonalLab.Label(item.prompt.transform, text(), new Vector2(0, promptHeight), Color.white);
            item.label = item.prompt.GetComponentInChildren<TMPro.TMP_Text>();
            item.prompt.SetActive(false);
            return item;
        }

        private void Update()
        {
            bool allowed = near && !LabUI.AnyOpen && (Available == null || Available()) &&
                GameSaveManager.Instance.Data.tutorialCompleted;
            if (prompt.activeSelf != allowed) { prompt.SetActive(allowed); if (allowed) label.text = PromptText(); }
            if (allowed && Input.GetKeyDown(KeyCode.E)) { Use?.Invoke(); if (prompt.activeSelf) label.text = PromptText(); }
        }
        private void OnTriggerEnter2D(Collider2D other) { if (other.GetComponentInParent<PlayerController>() != null) near = true; }
        private void OnTriggerExit2D(Collider2D other) { if (other.GetComponentInParent<PlayerController>() != null) near = false; }
    }

    /// <summary>Door trigger between school areas.</summary>
    public sealed class AreaDoor : MonoBehaviour
    {
        public string To;
        public SchoolAreas Areas;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) Areas.Travel(To, gameObject);
        }
    }

    public sealed class AreaDef
    {
        public string ID, Name, Purpose;
        public Vector2 Origin;
        public int HalfWidth;
        public Color Floor, Wall;
        public int Reputation;
    }

    /// <summary>
    /// The small school around the classroom: hallway hub, storage room, reading corner,
    /// greenhouse and science-fair exhibition room. Each area has its own reason to visit.
    /// </summary>
    public sealed class SchoolAreas : MonoBehaviour
    {
        public const float BackWall = 5.5f;
        public static readonly AreaDef[] Defs =
        {
            new AreaDef { ID = "hallway", Name = "დერეფანი", Purpose = "განცხადებების დაფა, ჯილდოების ვიტრინა და კლასელები", Origin = new Vector2(200, 0), HalfWidth = 15,
                Floor = new Color(0.93f, 0.9f, 0.84f), Wall = new Color(0.86f, 0.92f, 1f) },
            new AreaDef { ID = "storage", Name = "სათავსო", Purpose = "ყოველდღიური მასალები და ლაბორანტი ქეთევანი", Origin = new Vector2(260, 0), HalfWidth = 8,
                Floor = new Color(0.86f, 0.82f, 0.76f), Wall = new Color(0.82f, 0.82f, 0.8f) },
            new AreaDef { ID = "reading", Name = "კითხვის კუთხე", Purpose = "ქიმიური ფაქტები და წყნარი დასვენება", Origin = new Vector2(300, 0), HalfWidth = 8,
                Floor = new Color(1f, 0.88f, 0.74f), Wall = new Color(1f, 0.9f, 0.78f) },
            new AreaDef { ID = "greenhouse", Name = "სათბური", Purpose = "მცენარეების მორწყვა, მოსავალი და წვიმის წყალი", Origin = new Vector2(340, 0), HalfWidth = 9,
                Floor = new Color(0.8f, 0.92f, 0.78f), Wall = new Color(0.78f, 0.95f, 0.9f) },
            new AreaDef { ID = "exhibition", Name = "გამოფენის დარბაზი", Purpose = "სამეცნიერო ბაზრობა და მოგზაური მკვლევარი", Origin = new Vector2(390, 0), HalfWidth = 11,
                Floor = new Color(0.9f, 0.86f, 0.95f), Wall = new Color(0.9f, 0.85f, 1f), Reputation = 1 },
        };
        // Doors on the hallway's back wall, left to right.
        private static readonly (string to, float x)[] HallwayDoors =
            { ("classroom", -12), ("storage", -6), ("reading", 0), ("greenhouse", 6), ("exhibition", 12) };

        public string Current { get; private set; } = "classroom";
        public bool InArea => Current != "classroom";
        public static Vector2 ClassroomDoor = new Vector2(46.5f, -23.2f);
        private LabGame game;
        private float travelAfter;
        private readonly Dictionary<string, Transform> roots = new Dictionary<string, Transform>();

        public static AreaDef Def(string id) => Defs.FirstOrDefault(d => d.ID == id);
        public static string NameOf(string id) => id == "classroom" ? "ქიმიის კლასი" : id == "personal" ? "პირადი ლაბორატორია" : Def(id)?.Name ?? id;
        public static bool Unlocked(string id) => (Def(id)?.Reputation ?? 0) <= LabProgress.ReputationLevel;

        public Vector2 World(string area, Vector2 local) =>
            area == "classroom" ? (Vector2)game.classroom.TransformPoint(local) : Def(area).Origin + local;

        public void Initialize(LabGame lab)
        {
            game = lab;
            foreach (var def in Defs) roots[def.ID] = BuildRoom(def);
            BuildHallway(roots["hallway"]);
            BuildStorage(roots["storage"]);
            BuildReading(roots["reading"]);
            BuildGreenhouse(roots["greenhouse"]);
            BuildExhibition(roots["exhibition"]);
            BuildClassroomExtras();
        }

        // ---------------- travel ----------------

        public bool Travel(string to, GameObject door = null)
        {
            if (Time.unscaledTime < travelAfter || LabUI.AnyOpen || !GameSaveManager.Instance.Data.profileCompleted) return false;
            if (!GameSaveManager.Instance.Data.tutorialCompleted)
            { game.HUD.Notify("ჯერ მასწავლებელთან პირველი ცდა დაასრულე. შემდეგ სკოლის დათვალიერებაც შეგეძლება!"); travelAfter = Time.unscaledTime + 1.5f; return false; }
            if (!Unlocked(to))
            {
                game.HUD.Notify(NameOf(to) + " ჯერ დაკეტილია. საჭიროა რეპუტაციის დონე: " + LabProgress.ReputationTitles[Def(to).Reputation] + ".");
                travelAfter = Time.unscaledTime + 1.5f; return false;
            }
            string from = Current;
            Vector2 arrival;
            if (to == "classroom") arrival = game.classroom.TransformPoint(ClassroomDoor + new Vector2(0, 1.8f));
            else if (to == "hallway")
            {
                float x = HallwayDoors.First(d => d.to == from).x;
                arrival = Def("hallway").Origin + new Vector2(x, BackWall - 2.4f);
            }
            else arrival = Def(to).Origin + new Vector2(0, BackWall - 2.4f);

            var body = game.player.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.position = arrival;
            game.player.SetMovement(to == "classroom" ? Vector2.up : Vector2.down);
            Physics2D.SyncTransforms();
            Current = to;
            travelAfter = Time.unscaledTime + 1f;
            var camera = Camera.main.GetComponent<ClassroomCamera>();
            if (to == "classroom") camera.SetRoom(Vector2.zero, Vector2.zero, false);
            else
            {
                var def = Def(to);
                camera.SetRoom(def.Origin + new Vector2(-def.HalfWidth - 1, -6), def.Origin + new Vector2(def.HalfWidth + 1, BackWall + 4.6f), true);
            }
            AmbientAudio.Instance?.Door();
            game.HUD.Notify(NameOf(to) + (to == "classroom" ? "" : " • " + Def(to).Purpose));
            game.HUD.Refresh();
            return true;
        }

        // ---------------- room kit ----------------

        private Transform BuildRoom(AreaDef def)
        {
            var room = new GameObject(def.Name).transform;
            room.position = def.Origin;
            int w = def.HalfWidth;
            for (int x = -w; x < w; x++)
            for (int y = -5; y <= 5; y++)
            {
                var tile = LaboratoryArt.Draw(room, "ფილა", new Vector2(x + 0.5f, y), LaboratoryArt.Floor, -300);
                tile.color = def.Floor;
            }
            for (int x = -w; x < w; x++) WallColumn(room, x + 0.5f, def.Wall);
            Solid(room, new Vector2(0, BackWall + 0.15f), new Vector2(w * 2 + 2, 0.3f));
            Solid(room, new Vector2(0, -5.7f), new Vector2(w * 2 + 2, 0.5f), true);
            Solid(room, new Vector2(-w - 0.25f, 0), new Vector2(0.5f, 14), true);
            Solid(room, new Vector2(w + 0.25f, 0), new Vector2(0.5f, 14), true);
            if (def.ID != "hallway") Door(room, 0, "hallway", "დერეფანი");
            return room;
        }

        private static void WallColumn(Transform room, float x, Color tint)
        {
            void Piece(float h, Sprite sprite, float scaleY = 1, int order = -290)
            {
                var art = LaboratoryArt.Draw(room, "კედელი", new Vector2(x, BackWall + h), sprite, order);
                art.transform.localScale = new Vector3(1, scaleY, 1);
                art.color = tint;
            }
            Piece(0, LaboratoryArt.WallBase);
            Piece(0.25f, LaboratoryArt.WallLower);
            Piece(1.25f, LaboratoryArt.WallPaint);
            Piece(2.25f, LaboratoryArt.WallPaint);
            Piece(3.25f, LaboratoryArt.WallPaint, 0.5f);
            Piece(3.75f, LaboratoryArt.WallCap, 1, -289);
        }

        private static void Solid(Transform room, Vector2 point, Vector2 size, bool cap = false)
        {
            var go = new GameObject("კედლის საზღვარი", typeof(BoxCollider2D));
            go.transform.SetParent(room, false); go.transform.localPosition = point;
            go.GetComponent<BoxCollider2D>().size = size;
            if (!cap) return;
            var edge = LaboratoryArt.Draw(go.transform, "კედლის კიდე", Vector2.zero, LaboratoryArt.WallCap, -289);
            edge.transform.localScale = new Vector3(size.x, size.y * 4, 1);
        }

        private void Door(Transform room, float x, string to, string sign)
        {
            LaboratoryArt.Draw(room, "კარის ჩარჩო", new Vector2(x, BackWall - 0.05f),
                PixelArt.Make(36, 66, (px, py) => new Color32(47, 53, 55, 255), new Vector2(0.5f, 0)), -289);
            LaboratoryArt.Draw(room, "კარი", new Vector2(x, BackWall), to == "classroom" ? LaboratoryArt.Door : LaboratoryArt.LabDoor, -288);
            PersonalLab.DoorSign(room, sign, new Vector2(x, BackWall + 3.3f), -287, 2.1f);
            var go = new GameObject("კარი: " + sign, typeof(BoxCollider2D), typeof(AreaDoor));
            go.transform.SetParent(room, false); go.transform.localPosition = new Vector2(x, BackWall - 0.6f);
            var zone = go.GetComponent<BoxCollider2D>(); zone.isTrigger = true; zone.size = new Vector2(1.6f, 0.9f);
            var door = go.GetComponent<AreaDoor>(); door.To = to; door.Areas = this;
            if (!Unlocked(to))
            {
                var lockSign = LaboratoryArt.Draw(room, "საკეტი", new Vector2(x + 0.55f, BackWall + 1.6f), PixelArt.Make(6, 7, (px, py) =>
                    py < 4 ? new Color32(214, 170, 70, 255) : (px == 0 || px == 5 || py == 6) ? new Color32(150, 150, 160, 255) : Color.clear), -287);
                lockSign.gameObject.AddComponent<UnlockWatcher>().Initialize(to);
            }
        }

        private SpriteRenderer Prop(Transform room, string name, Vector2 local, Sprite sprite, Vector2 footprint, int order = 6)
        {
            var art = LaboratoryArt.Draw(room, name, local, sprite, order);
            if (footprint != Vector2.zero)
            {
                var shape = art.gameObject.AddComponent<BoxCollider2D>();
                shape.size = footprint; shape.offset = new Vector2(0, footprint.y * 0.5f);
                DepthOccluders.Add(art.transform.position, footprint.x * 0.5f + 0.35f, footprint.y * 0.6f, footprint.y + 2.6f);
            }
            return art;
        }

        // ---------------- hallway ----------------

        private void BuildHallway(Transform room)
        {
            foreach (var door in HallwayDoors) Door(room, door.x, door.to, NameOf(door.to));
            LabAtmosphere.Instance?.AddWindow(room, new Vector2(-9, BackWall + 2.2f), new Vector2(1.6f, 1.4f));
            LabAtmosphere.Instance?.AddWindow(room, new Vector2(3, BackWall + 2.2f), new Vector2(1.6f, 1.4f));
            LabAtmosphere.Instance?.AddWindow(room, new Vector2(9, BackWall + 2.2f), new Vector2(1.6f, 1.4f));
            var board = LaboratoryArt.Draw(room, "განცხადებების დაფა", new Vector2(-3, BackWall + 1.4f), DecorArt.Noticeboard(), -287);
            WorldInteractable.Create(room, new Vector2(-3, BackWall - 1.2f), new Vector2(2.4f, 1.6f), 2.2f, () => "[E] განცხადებების დაფა", () => game.Journal.Open());
            Prop(room, "ჯილდოების ვიტრინა", new Vector2(14, BackWall - 1.6f), DecorArt.TrophyCase(), new Vector2(1.6f, 0.8f));
            WorldInteractable.Create(room, new Vector2(14, BackWall - 2.6f), new Vector2(2, 1.4f), 2.6f, () => "[E] სკოლის ჯილდოები", () => game.Collection.Open(CollectionKind.Award));
            Prop(room, "სკამი", new Vector2(-14, -2), DecorArt.Bench(), new Vector2(1.6f, 0.5f));
            Prop(room, "სკამი", new Vector2(14, -2), DecorArt.Bench(), new Vector2(1.6f, 0.5f));
            Prop(room, "ქოთნის მცენარე", new Vector2(-14.2f, 4.2f), DecorArt.Plant(1), new Vector2(0.7f, 0.4f));
            Prop(room, "ქოთნის მცენარე", new Vector2(4.4f, 4.4f), DecorArt.Plant(0), new Vector2(0.7f, 0.4f));
            var fan = LaboratoryArt.Draw(room, "ვენტილაცია", new Vector2(-1.6f, BackWall + 3f), DecorArt.VentGrille(), -287);
            LabAtmosphere.Instance?.AddFan(room, new Vector2(-1.6f, BackWall + 3f));
            LabAtmosphere.Instance?.AddSafetyLight(room, new Vector2(-12, BackWall + 4.3f));
            LabAtmosphere.Instance?.AddDust(room, new Vector2(0, 1), new Vector2(28, 8));
            Seasonal(room, new Vector2(0, BackWall + 3.5f), 28);
        }

        // ---------------- storage room ----------------

        private void BuildStorage(Transform room)
        {
            for (int i = 0; i < 3; i++) Prop(room, "თარო", new Vector2(-6 + i * 2.1f, BackWall - 1.4f), DecorArt.Shelf(i), new Vector2(1.8f, 0.6f));
            Prop(room, "ყუთები", new Vector2(5.5f, 2.5f), DecorArt.Crates(), new Vector2(2.2f, 0.8f));
            Prop(room, "ყუთები", new Vector2(-6f, -3f), DecorArt.Crates(), new Vector2(2.2f, 0.8f));
            LabAtmosphere.Instance?.AddSafetyLight(room, new Vector2(4, BackWall + 3.6f));
            LabAtmosphere.Instance?.AddFan(room, new Vector2(-3, BackWall + 3f));
            LaboratoryArt.Draw(room, "ვენტილაცია", new Vector2(-3, BackWall + 3f), DecorArt.VentGrille(), -287);
            WorldInteractable.Create(room, new Vector2(-4, BackWall - 2.6f), new Vector2(6, 1.4f), 2.6f,
                () => LabProgress.Data.storagePickupDay == LabProgress.Today ? "[E] მასალების ყიდვა" : "[E] დღის მასალების აღება",
                () => game.Collection.OpenStorage());
            WorldInteractable.Create(room, new Vector2(5.5f, 1.3f), new Vector2(2.6f, 1.4f), 2.6f, () => "[E] ყუთების დათვალიერება", () =>
            {
                string key = "crates:" + LabProgress.Today;
                if (LabProgress.Data.claimedEvents.Contains(key)) { game.HUD.Notify("ყუთებში დღეს მეტი აღარაფერია. ხვალ ახალი მიწოდებაა."); return; }
                LabProgress.Data.claimedEvents.Add(key);
                var random = new System.Random(key.GetHashCode());
                if (random.NextDouble() < 0.35)
                {
                    var minerals = LabProgress.Items(CollectionKind.Mineral).Where(m => !LabProgress.Owns(m.ID)).ToList();
                    if (minerals.Count > 0) { LabProgress.Collect(minerals[random.Next(minerals.Count)].ID); GameSaveManager.Instance.Commit(); return; }
                }
                LabProgress.Give("wire", 1);
                GameSaveManager.Instance.Commit();
                game.HUD.Notify("ყუთში სადენის ხვია იპოვე! +1 სადენი");
            });
        }

        // ---------------- reading corner ----------------

        private void BuildReading(Transform room)
        {
            for (int i = 0; i < 3; i++) Prop(room, "წიგნების თარო", new Vector2(-5 + i * 2.2f, BackWall - 1.5f), DecorArt.Bookshelf(i), new Vector2(1.9f, 0.6f));
            Prop(room, "სავარძელი", new Vector2(4.5f, 1), DecorArt.Sofa(), new Vector2(2.2f, 0.6f));
            Prop(room, "ხალიჩა", new Vector2(4.5f, -1.4f), DecorArt.Rug(), Vector2.zero, -295);
            Prop(room, "ლამპა", new Vector2(6.8f, 1.3f), DecorArt.Lamp(), new Vector2(0.5f, 0.3f));
            LabAtmosphere.Instance?.AddGlow(room, new Vector2(6.8f, 3.1f), new Color(1f, 0.8f, 0.45f), 2.6f, true);
            LabAtmosphere.Instance?.AddWindow(room, new Vector2(5, BackWall + 2.2f), new Vector2(1.6f, 1.4f));
            LabAtmosphere.Instance?.AddDust(room, new Vector2(4, 1), new Vector2(8, 6));
            WorldInteractable.Create(room, new Vector2(-3, BackWall - 2.6f), new Vector2(6.6f, 1.4f), 2.6f,
                () => LabProgress.Data.readingDay == LabProgress.Today ? "[E] ფაქტების კოლექცია" : "[E] წიგნის წაკითხვა", ReadBook);
        }

        private void ReadBook()
        {
            var data = LabProgress.Data;
            if (data.readingDay == LabProgress.Today) { game.Collection.Open(CollectionKind.Fact); return; }
            data.readingDay = LabProgress.Today;
            var unread = LabProgress.Items(CollectionKind.Fact).Where(f => !LabProgress.Owns(f.ID)).ToList();
            if (unread.Count == 0) { LabProgress.AddReputation(1); GameSaveManager.Instance.Commit(); game.HUD.Notify("ყველა წიგნი უკვე წაკითხული გაქვს! ძველი ფავორიტი ხელახლა გადაიკითხე. +1 რეპუტაცია"); return; }
            var fact = unread[new System.Random(LabProgress.Today.GetHashCode()).Next(unread.Count)];
            LabProgress.Collect(fact.ID);
            LabProgress.CountStat("read");
            GameSaveManager.Instance.Commit();
            game.Collection.ShowFact(fact);
        }

        // ---------------- greenhouse ----------------

        private void BuildGreenhouse(Transform room)
        {
            foreach (float x in new[] { -6.8f, -3.4f, 3.4f, 6.8f })
                LabAtmosphere.Instance?.AddWindow(room, new Vector2(x, BackWall + 2.2f), new Vector2(2.2f, 2.2f));
            string[] crops = { "cabbage", "lemon", "flowers", "citrus" };
            for (int i = 0; i < crops.Length; i++)
            {
                string crop = crops[i];
                var bed = Prop(room, "საწოლი", new Vector2(-6 + i * 4, 0.4f), DecorArt.PlantBed(i), new Vector2(2.6f, 0.9f));
                bed.gameObject.AddComponent<PlantGrowth>();
            }
            WorldInteractable.Create(room, new Vector2(0, -1.4f), new Vector2(16, 1.6f), 3.2f, () =>
                LabProgress.Data.greenhouseWateredDay != LabProgress.Today ? "[E] მცენარეების მორწყვა"
                : LabProgress.Data.greenhouseHarvestDay != LabProgress.Today ? "[E] მოსავლის აღება" : "მცენარეები ხვალამდე ისვენებენ",
                WaterOrHarvest);
            Prop(room, "წვიმის კასრი", new Vector2(7.6f, 3.6f), DecorArt.RainBarrel(), new Vector2(1f, 0.6f));
            WorldInteractable.Create(room, new Vector2(7.6f, 2.5f), new Vector2(1.8f, 1.4f), 2.4f,
                () => SchoolCalendar.Raining ? "[E] წვიმის წყლის აღება" : "კასრი ცარიელია • წვიმას დაელოდე", () =>
                {
                    string key = "rain:" + LabProgress.Today;
                    if (!SchoolCalendar.Raining) { game.HUD.Notify("დღეს არ წვიმს. წვიმის წყალი მხოლოდ წვიმიან დღეს გროვდება."); return; }
                    if (LabProgress.Data.claimedEvents.Contains(key)) { game.HUD.Notify("დღევანდელი წვიმის წყალი უკვე აიღე."); return; }
                    LabProgress.Data.claimedEvents.Add(key);
                    LabProgress.Give("rainwater", 2);
                    GameSaveManager.Instance.Commit();
                    game.HUD.Notify("+2 წვიმის წყალი. ახლა წყლის ხარისხს შეისწავლი!");
                });
            LabAtmosphere.Instance?.AddDust(room, new Vector2(0, 1), new Vector2(16, 7));
        }

        private void WaterOrHarvest()
        {
            var data = LabProgress.Data;
            string today = LabProgress.Today;
            if (data.greenhouseWateredDay != today)
            {
                data.greenhouseWateredDay = today;
                LabProgress.CountStat("water");
                LabProgress.CountStat("water_days");
                if (LabProgress.Stat("water_days") >= 7) LabProgress.Collect("badge:gardener");
                GameSaveManager.Instance.Commit();
                AmbientAudio.Instance?.Splash();
                game.HUD.Notify("მცენარეები მორწყულია! ახლა მოსავლის აღებაც შეგიძლია.");
                return;
            }
            if (data.greenhouseHarvestDay == today) { game.HUD.Notify("დღევანდელი მოსავალი უკვე აღებულია. ხვალ ისევ მოდი!"); return; }
            data.greenhouseHarvestDay = today;
            int amount = SchoolCalendar.IsWeekend ? 2 : 1;
            foreach (var crop in new[] { "cabbage", "lemon", "flowers", "citrus" }) LabProgress.Give(crop, amount);
            GameSaveManager.Instance.Commit();
            AudioManager.Instance?.Coin();
            game.HUD.Notify("მოსავალი: +" + amount + " კომბოსტო, ლიმონი, ყვავილები და ციტრუსის ქერქი" + (amount > 1 ? " (შაბათ-კვირის ბონუსი!)" : "") + ".");
        }

        // ---------------- exhibition room ----------------

        private void BuildExhibition(Transform room)
        {
            for (int i = 0; i < 4; i++)
                Prop(room, "საგამოფენო სტენდი", new Vector2(-7.5f + i * 5, 0.6f), DecorArt.Pedestal(i), new Vector2(1.4f, 0.6f));
            Prop(room, "ბაზრობის სტენდი", new Vector2(-6, BackWall - 1.5f), DecorArt.FairBooth(), new Vector2(3f, 0.6f));
            WorldInteractable.Create(room, new Vector2(-6, BackWall - 2.7f), new Vector2(3.6f, 1.4f), 2.8f, () => "[E] სამეცნიერო ბაზრობა", () => game.Journal.OpenFair());
            LaboratoryArt.Draw(room, "ბანერი", new Vector2(4, BackWall + 2.6f), DecorArt.Banner(), -287);
            LabAtmosphere.Instance?.AddGlow(room, new Vector2(-7.5f, 2.1f), new Color(0.7f, 0.6f, 1f), 1.6f, false);
            LabAtmosphere.Instance?.AddGlow(room, new Vector2(7.5f, 2.1f), new Color(1f, 0.85f, 0.5f), 1.6f, false);
            Seasonal(room, new Vector2(0, BackWall + 3.5f), 20);
        }

        // ---------------- classroom extras ----------------

        private void BuildClassroomExtras()
        {
            var classroom = game.classroom;
            // Exit to the hallway on the front (bottom) edge of the classroom.
            LaboratoryArt.Draw(classroom, "დერეფნის ფეხსაწმენდი", ClassroomDoor + new Vector2(0, -0.2f), PixelArt.Make(40, 8,
                (x, y) => x < 2 || x > 37 || y == 0 || y == 7 ? new Color32(105, 81, 53, 255) : (x / 4 + y / 2) % 2 == 0 ? new Color32(182, 155, 105, 255) : new Color32(160, 132, 88, 255)), 3);
            PersonalLab.Label(classroom, "↓ დერეფანი", ClassroomDoor + new Vector2(0, 1.0f), LabUI.Paper);
            var go = new GameObject("კარი: დერეფანი", typeof(BoxCollider2D), typeof(AreaDoor));
            go.transform.SetParent(classroom, false); go.transform.localPosition = ClassroomDoor + new Vector2(0, -0.2f);
            var zone = go.GetComponent<BoxCollider2D>(); zone.isTrigger = true; zone.size = new Vector2(2.4f, 0.8f);
            var door = go.GetComponent<AreaDoor>(); door.To = "hallway"; door.Areas = this;

            // The old decorative book stack is replaced by a working collection cabinet and the noticeboards are live.
            RemoveDecorTiles(new Rect(32.2f, 8.9f, 1.6f, 1.8f));
            var cabinet = Prop(classroom, "კოლექციის კარადა", new Vector2(28.4f, 9.3f), DecorArt.CollectionCabinet(), new Vector2(1.8f, 0.8f));
            cabinet.gameObject.AddComponent<CollectionShelfView>();
            WorldInteractable.Create(classroom, new Vector2(28.4f, 8.2f), new Vector2(2.2f, 1.4f), 3.6f, () => "[E] კოლექციის კარადა", () => game.Collection.Open(CollectionKind.Mineral));
            WorldInteractable.Create(classroom, new Vector2(42.9f, 10.4f), new Vector2(2.2f, 1.4f), 2.4f, () => "[E] განცხადებების დაფა", () => game.Journal.Open());
            Seasonal(classroom, new Vector2(46.5f, 14.95f), 40);
        }

        private static void RemoveDecorTiles(Rect localArea)
        {
            foreach (var map in FindObjectsByType<UnityEngine.Tilemaps.Tilemap>())
            {
                if (map.name != "Decor") continue;
                var bounds = map.cellBounds;
                var classroom = LabGame.Instance.classroom;
                foreach (var cell in bounds.allPositionsWithin)
                {
                    if (!map.HasTile(cell)) continue;
                    Vector2 local = classroom.InverseTransformPoint(map.GetCellCenterWorld(cell));
                    if (localArea.Contains(local)) map.SetTile(cell, null);
                }
            }
        }

        private void Seasonal(Transform parent, Vector2 local, float width)
        {
            var season = SchoolCalendar.Season;
            int count = Mathf.RoundToInt(width / 1.2f);
            for (int i = 0; i < count; i++)
            {
                float x = local.x - width / 2 + (i + 0.5f) * width / count;
                LaboratoryArt.Draw(parent, "სეზონური მორთულობა", new Vector2(x, local.y - Mathf.Abs(Mathf.Sin(i * 0.9f)) * 0.15f),
                    DecorArt.SeasonPiece(season, i), parent == game.classroom ? 4 : -286);
            }
        }
    }

    /// <summary>Hides a lock badge once the area opens.</summary>
    public sealed class UnlockWatcher : MonoBehaviour
    {
        private string area;
        public void Initialize(string id) => area = id;
        private void Update() { if (SchoolAreas.Unlocked(area)) gameObject.SetActive(false); }
    }

    /// <summary>Greenhouse beds look lush after watering and sprout fruit once harvest is ready.</summary>
    public sealed class PlantGrowth : MonoBehaviour
    {
        private SpriteRenderer art;
        private void Awake() => art = GetComponent<SpriteRenderer>();
        private void Update()
        {
            var data = GameSaveManager.Instance.Data;
            bool watered = data.greenhouseWateredDay == LabProgress.Today;
            float sway = 1 + Mathf.Sin(Time.time * 1.4f + transform.position.x) * 0.015f;
            transform.localScale = new Vector3(1, sway * (watered ? 1.05f : 0.95f), 1);
            art.color = watered ? Color.white : new Color(0.86f, 0.84f, 0.7f);
        }
    }

    /// <summary>The classroom collection cabinet shows one jar per collected mineral or sample.</summary>
    public sealed class CollectionShelfView : MonoBehaviour
    {
        private int shown = -1;
        private readonly List<GameObject> jars = new List<GameObject>();
        private void Update()
        {
            var items = LabProgress.Catalog.Where(c => (c.Kind == CollectionKind.Mineral || c.Kind == CollectionKind.Sample) && LabProgress.Owns(c.ID)).Take(12).ToList();
            if (items.Count == shown) return;
            shown = items.Count;
            foreach (var jar in jars) Destroy(jar);
            jars.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                var piece = LaboratoryArt.Draw(transform, "ნიმუში", new Vector2(-0.65f + (i % 4) * 0.43f, 0.95f + (i / 4) * 0.62f), DecorArt.Jar(), 7);
                piece.color = items[i].Color;
                jars.Add(piece.gameObject);
            }
        }
    }
}
