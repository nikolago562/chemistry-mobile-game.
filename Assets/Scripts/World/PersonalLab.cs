using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    [Serializable]
    public sealed class LabPlacement { public string itemID; public int column; public int row; }

    public sealed class LabEquipment
    {
        public readonly string ID, Name, Description;
        public readonly int Price;
        public readonly Color Color;
        /// <summary>Decorations change the room's look and mood instead of opening an activity.</summary>
        public readonly bool Decor;
        public readonly int ReputationLevel;
        /// <summary>Non-empty for items that are earned rather than bought.</summary>
        public readonly string Unlock;
        public LabEquipment(string id, string name, string description, int price, Color color, bool decor = false, int reputation = 0, string unlock = "")
        { ID = id; Name = name; Description = description; Price = price; Color = color; Decor = decor; ReputationLevel = reputation; Unlock = unlock; }
    }

    public sealed class PersonalLab : MonoBehaviour
    {
        public const int ExpansionPrice = 250;
        public static readonly LabEquipment[] Catalog = {
            new LabEquipment("molecule", "მოლეკულების აწყობა", "შეაერთე ატომები და ააწყვე წყალი, ჟანგბადი და სხვა მოლეკულები.", 40, LabUI.Mint),
            new LabEquipment("solution", "ხსნარების სადგური", "შეცვალე გახსნილი ნივთიერების რაოდენობა და ხსნარის მოცულობა. დააკვირდი კონცენტრაციის ცვლილებას.", 55, LabUI.Green),
            new LabEquipment("ph", "მჟავებისა და ფუძეების მაგიდა", "შეურიე მჟავასა და ფუძის ხსნარები და დააკვირდი ნეიტრალიზაციას.", 70, LabUI.Coral),
            new LabEquipment("atom", "ატომების სახელოსნო", "დაამატე პროტონები, ნეიტრონები და ელექტრონები. შექმენი ატომები და იონები.", 90, LabUI.Gold),
            new LabEquipment("membrane", "მემბრანული ტრანსპორტი", "გახსენი არხი და დააკვირდი დიფუზიას. ტუმბოს საშუალებით ნაწილაკები გრადიენტის საწინააღმდეგოდ გადაიტანე.", 110, LabUI.Mint),
            new LabEquipment("chromatography", "ფერების დეტექტივი", "აირჩიე გამხსნელი, გამოთქვი ვარაუდი და ქაღალდზე პიგმენტები ერთმანეთისგან განაცალკევე.", 120, LabUI.Coral),
            new LabEquipment("gas", "აირის წნევის სადგური", "შეკუმშე აირი, შეცვალე ტემპერატურა და დააკვირდი, როგორ იცვლება წნევა.", 140, LabUI.Gold),
            // New hands-on activities.
            new LabEquipment("balance", "დაბალანსების დაფა", "კოეფიციენტები შეცვალე და დააკვირდი, როგორ თანაბრდება ატომების სასწორი.", 50, LabUI.Mint),
            new LabEquipment("thermo", "თერმოსტატი", "ტემპერატურა ზუსტ ზოლში შეინარჩუნე: კრისტალების ზრდა და ორთქლით გამოხდა.", 60, LabUI.Coral),
            new LabEquipment("analysis", "ანალიზის სადგური", "ალის ფერი, pH, ხსნადობა და ძმრის ტესტი: ამოიცანი უცნობი ნიმუშები.", 65, LabUI.Gold),
            new LabEquipment("valve", "დოზირების სარქველი", "მართე სარქველი და მანომეტრი, ფერი ან წნევა მიზნამდე მიიყვანე.", 55, LabUI.Green),
            new LabEquipment("particles", "ნაწილაკების დამჭერი", "დაიჭირე ელექტრონები და ნახე, როგორ მუშაობს ბატარეა.", 75, LabUI.Mint, false, 1),
            new LabEquipment("compare", "რეაქციების შედარების მაგიდა", "იწინასწარმეტყველე, რომელი რეაქციაა სწრაფი, მერე დააკვირდი ბუშტუკებს.", 70, LabUI.Coral, false, 1),
            // Decorations.
            new LabEquipment("research", "კვლევების დაფა", "აჩვენებს დაუსრულებელ პროექტებს. [E] ხსნის კვლევების დაფას.", 0, LabUI.Green, true),
            new LabEquipment("plant", "ოთახის მცენარე", "ყოველდღე მორწყე. მცენარეები ოთახს ამშვიდებს.", 0, LabUI.Green, true),
            new LabEquipment("ficus", "ფიკუსი", "დიდი მწვანე ფოთლები და მეტი სიმშვიდე.", 25, LabUI.Green, true),
            new LabEquipment("radio", "რადიო", "სამი სადგური: ლოფაი, ფორტეპიანო და ჩიპტიუნი. [E] ცვლის მუსიკას.", 45, LabUI.Coral, true),
            new LabEquipment("photos", "ცდების ფოტოდაფა", "დასრულებული ცდების ფოტოები ავტომატურად ჩნდება.", 30, LabUI.Gold, true),
            new LabEquipment("storage", "მასალების საწყობი", "მასალების მარაგს აჩვენებს.", 40, LabUI.Paper, true),
            new LabEquipment("samples", "ნიმუშების თარო", "ყოველი შეგროვებული ნიმუში და მინერალი თაროზე ჩნდება.", 45, LabUI.Mint, true),
            new LabEquipment("poster_table", "პლაკატი: პერიოდული ცხრილი", "[E] ახალ ფაქტს გასწავლის.", 15, LabUI.Gold, true),
            new LabEquipment("poster_safety", "პლაკატი: უსაფრთხოება", "[E] ახალ ფაქტს გასწავლის.", 15, LabUI.Coral, true),
            new LabEquipment("lamp", "იატაკის ლამპა", "საღამოს თბილ შუქს აფრქვევს.", 35, LabUI.Gold, true),
            new LabEquipment("rug", "ხალიჩა", "რბილი და ფერადი. ოთახს ამშვიდებს.", 25, LabUI.Coral, true),
            new LabEquipment("sofa", "მყუდრო სავარძელი", "დასასვენებელი კუთხე.", 55, LabUI.Navy, true),
            new LabEquipment("display", "ცდების ვიტრინა", "დასრულებული პროექტების შედეგებს აჩვენებს.", 60, LabUI.Mint, true, 1),
            new LabEquipment("trophy", "ჯილდოების თარო", "მედლები და ნიშნები თაროზე.", 50, LabUI.Gold, true, 1),
            new LabEquipment("plasma", "პლაზმური სფერო", "ცოცხალი იისფერი ელვები.", 70, LabUI.Coral, true, 1),
            new LabEquipment("aquarium", "აკვარიუმი", "სამი პატარა თევზი და ბუშტუკები. დიდი სიმშვიდე.", 90, LabUI.Mint, true, 2),
            new LabEquipment("season", "სეზონური მორთულობა", "იცვლება სეზონის მიხედვით.", 0, LabUI.Paper, true, 0, "სეზონური სუვენირი განცხადებების დაფიდან"),
            new LabEquipment("crystal_lamp", "კრისტალის ნათურა", "შენ მიერ გაზრდილი კრისტალი.", 0, LabUI.Mint, true, 0, "პროექტი: კრისტალების გაზრდა"),
            new LabEquipment("glow_jar", "მანათობელი ქილა", "ფერცვალებადი ხსნარის სუვენირი.", 0, LabUI.Mint, true, 0, "პროექტი: ფერცვალებადი ხსნარი"),
            new LabEquipment("mineral_case", "მინერალების ყუთი", "იშვიათი მინერალების ვიტრინა.", 0, LabUI.Gold, true, 0, "პროფესორი ლევანი (ხუთშაბათი, პარასკევი)"),
            new LabEquipment("fair_trophy", "ბაზრობის თასი", "სამეცნიერო ბაზრობის პირველი ადგილი.", 0, LabUI.Gold, true, 0, "სამეცნიერო ბაზრობის გამარჯვება"),
            new LabEquipment("steam_wheel", "ორთქლის ბორბალი", "შენი პირველი ორთქლის მანქანა.", 0, LabUI.Mint, true, 0, "მისია: ორთქლის მანქანა"),
        };
        public static readonly int[] ExpansionPrices = { 250, 400, 600 };
        public static readonly int[] ExpansionReputation = { 0, 2, 3 };
        public static readonly int[] LevelColumns = { 4, 6, 8, 8 }, LevelRows = { 3, 4, 4, 5 };
        public int Level => Mathf.Clamp(GameSaveManager.Instance.Data.labExpansionLevel, 0, 3);
        public bool Inside { get; private set; }
        public int Columns => LevelColumns[Level];
        public int Rows => LevelRows[Level];
        public Vector2 RoomOrigin => new Vector2(120, 0);
        public float BackWallY => Rows <= 3 ? 8.5f : Rows * 4 - 4.5f;
        public Vector2 ExitPosition => RoomOrigin + new Vector2(0, BackWallY - 0.8f);
        public Vector2 SpawnPosition => RoomOrigin + new Vector2(0, BackWallY - 2.5f);
        public Vector2 RoomMinimum => RoomOrigin + new Vector2(-10, -7);
        public Vector2 RoomMaximum => RoomOrigin + new Vector2(Columns * 4 - 5, BackWallY + 5);
        public int UsableSlots => Columns * Rows - 2;
        public bool IsEntranceCell(int column, int row) => row == Rows - 1 && (column == 1 || column == 2);
        public Transform Room { get; private set; }
        public LabWorkshopUI Workshop { get; private set; }
        public LabActivitiesUI Activities { get; private set; }
        private LabGame game;
        private Vector2 returnPoint;
        private float travelAfter;

        public void Initialize(LabGame lab)
        {
            game = lab;
            Workshop = gameObject.AddComponent<LabWorkshopUI>(); Workshop.Initialize(this);
            Activities = gameObject.AddComponent<LabActivitiesUI>();
            // Repair invalid or duplicate placements from older/corrupt save files without charging the player.
            var data = GameSaveManager.Instance.Data;
            var ids = new HashSet<string>(); var cells = new HashSet<Vector2Int>();
            data.labPlacements.RemoveAll(p => p == null || !Catalog.Any(c => c.ID == p.itemID) ||
                !data.ownedLabItems.Contains(p.itemID) || p.column < 0 || p.column >= Columns || p.row < 0 || p.row >= Rows ||
                !ids.Add(p.itemID) || !cells.Add(new Vector2Int(p.column, p.row)));
            // Keep owned furniture when clearing the new entrance landing in existing saves.
            bool relocated = false;
            foreach (var placement in data.labPlacements.Where(p => IsEntranceCell(p.column, p.row)))
            {
                var free = Enumerable.Range(0, Columns * Rows)
                    .Select(i => new Vector2Int(i % Columns, i / Columns))
                    .Where(c => !IsEntranceCell(c.x, c.y) && !data.labPlacements.Any(p => p.column == c.x && p.row == c.y))
                    .OrderBy(c => Mathf.Abs(c.x - placement.column) + Mathf.Abs(c.y - placement.row))
                    .ThenBy(c => Mathf.Abs(c.y - placement.row)).First();
                placement.column = free.x; placement.row = free.y; relocated = true;
            }
            if (relocated) GameSaveManager.Instance.Commit();
            var depth = game.player.gameObject.AddComponent<PersonalLabDepth>(); depth.Initialize(this);
            BuildRoom();
            var portal = Portal(game.classroom, new Vector2(46.5f, 10.1f), false);
            var door = LaboratoryArt.Draw(game.classroom, "პირადი ლაბორატორიის კარი", new Vector2(46.5f, 11), LaboratoryArt.Door, 5);
            door.transform.localScale = new Vector3(1.5f, 1, 1);
            // Framed name plate on the door's upper panel (the door reaches the top of the wall).
            DoorSign(game.classroom, "ჩემი ლაბორატორია", new Vector2(46.5f, 14.15f), 6, 2.8f);
        }
        private GameObject Portal(Transform parent, Vector2 point, bool exit)
        {
            var go = new GameObject(exit ? "კლასში დაბრუნება" : "პირად ლაბორატორიაში შესვლა", typeof(BoxCollider2D), typeof(LabDoorway));
            go.transform.SetParent(parent, false); go.transform.localPosition = point;
            var zone = go.GetComponent<BoxCollider2D>(); zone.isTrigger = true; zone.size = new Vector2(2.4f, 1.1f);
            go.GetComponent<LabDoorway>().Initialize(this, exit);
            LaboratoryArt.Draw(go.transform, "კარის ფეხსაწმენდი", new Vector2(0, -0.1f), PixelArt.Make(40, 6,
                (x, y) => x < 2 || x > 37 || y == 0 || y == 5 ? new Color32(105, 81, 53, 255) : new Color32(182, 155, 105, 255)), exit ? -295 : 3);
            return go;
        }
        public bool Travel(bool exit)
        {
            if (Time.unscaledTime < travelAfter || LabUI.AnyOpen || !GameSaveManager.Instance.Data.profileCompleted) return false;
            if (exit != Inside) return false;
            if (!exit && !GameSaveManager.Instance.Data.tutorialCompleted)
            { game.HUD.Notify("ჯერ მასწავლებელთან პირველი ცდა დაასრულე. შემდეგ საკუთარი ლაბორატორია გელოდება!"); return false; }
            if (!exit) returnPoint = game.classroom.TransformPoint(new Vector3(46.5f, 8.3f));
            Inside = !exit;
            // The lab room (~480 sprites) is only active while the player is in it.
            Room.gameObject.SetActive(Inside);
            travelAfter = Time.unscaledTime + 1.0f;
            var body = game.player.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.position = exit ? returnPoint : SpawnPosition;
            game.player.SetMovement(Vector2.zero);
            Physics2D.SyncTransforms();
            var camera = Camera.main.GetComponent<ClassroomCamera>();
            camera.SetRoom(RoomMinimum, RoomMaximum, Inside);
            game.HUD.Refresh();
            game.HUD.Notify(Inside ? "შენს ლაბორატორიაში ხარ! გეგმის დაფასთან ხელსაწყოებს იყიდი და დადგამ." : "კლასში დაბრუნდი!");
            return true;
        }
        public bool Buy(string id)
        {
            var item = Catalog.FirstOrDefault(c => c.ID == id);
            var save = GameSaveManager.Instance;
            if (item == null || save.Data.ownedLabItems.Contains(id) || save.Data.chemCoins < item.Price) return false;
            save.Data.chemCoins -= item.Price; save.Data.ownedLabItems.Add(id); save.Commit();
            AudioManager.Instance?.Coin();
            return true;
        }
        public bool Expand()
        {
            var save = GameSaveManager.Instance;
            int level = Level;
            if (level >= ExpansionPrices.Length || save.Data.chemCoins < ExpansionPrices[level] || LabProgress.ReputationLevel < ExpansionReputation[level]) return false;
            save.Data.chemCoins -= ExpansionPrices[level]; save.Data.labExpansionLevel = level + 1; save.Data.labExpanded = true; save.Commit();
            BuildRoom();
            if (Inside) Camera.main.GetComponent<ClassroomCamera>().SetRoom(RoomMinimum, RoomMaximum, true);
            AudioManager.Instance?.Milestone(); return true;
        }
        public bool Place(string id, int column, int row)
        {
            var save = GameSaveManager.Instance;
            if (!save.Data.ownedLabItems.Contains(id) || column < 0 || column >= Columns || row < 0 || row >= Rows || IsEntranceCell(column, row)) return false;
            var occupant = save.Data.labPlacements.FirstOrDefault(p => p.column == column && p.row == row && p.itemID != id);
            var point = RoomOrigin + Cell(column, row);
            var feet = game.player.GetComponent<Rigidbody2D>().position + game.player.GetComponent<BoxCollider2D>().offset;
            if (Inside && Mathf.Abs(point.x - feet.x) < 1.9f && feet.y > point.y - 0.4f && feet.y < point.y + 1.8f)
                return false;
            // Replacing: whatever already stands there swaps into the old cell, or goes back to storage.
            var previous = save.Data.labPlacements.FirstOrDefault(p => p.itemID == id);
            if (occupant != null)
            {
                if (previous != null) { occupant.column = previous.column; occupant.row = previous.row; }
                else save.Data.labPlacements.Remove(occupant);
            }
            save.Data.labPlacements.RemoveAll(p => p.itemID == id);
            save.Data.labPlacements.Add(new LabPlacement { itemID = id, column = column, row = row });
            save.Commit(); BuildEquipment(); return true;
        }
        /// <summary>Puts an earned item in the first free cell, so the reward is already standing in the lab.</summary>
        public bool PlaceAnywhere(string id)
        {
            var data = GameSaveManager.Instance.Data;
            for (int row = 0; row < Rows; row++)
                for (int column = 0; column < Columns; column++)
                    if (!IsEntranceCell(column, row) && !data.labPlacements.Any(p => p.column == column && p.row == row) && Place(id, column, row)) return true;
            return false;
        }
        public void Store(string id)
        {
            var save = GameSaveManager.Instance;
            save.Data.labPlacements.RemoveAll(p => p.itemID == id); save.Commit(); BuildEquipment();
        }
        public static Vector2 Cell(int col, int row) => new Vector2(-6 + col * 4, -3 + row * 4);
        private Transform equipmentRoot;
        private void BuildRoom()
        {
            if (Room != null) { Room.gameObject.SetActive(false); Destroy(Room.gameObject); }
            Room = new GameObject("შენი ლაბორატორია").transform; Room.position = RoomOrigin;
            int right = Columns * 4 - 7, top = Mathf.CeilToInt(BackWallY + 0.5f);
            var floor = new GameObject("იატაკი").transform; floor.SetParent(Room, false);
            for (int x = -9; x <= right; x++)
            for (int y = -6; y <= top; y++)
                LaboratoryArt.Draw(floor, "ფილა", new Vector2(x + 0.5f, y), LaboratoryArt.Floor, -300);
            Wall(new Vector2(-9.5f, (top - 3) / 2f), new Vector2(0.5f, top + 10));
            Wall(new Vector2(right + 1.5f, (top - 3) / 2f), new Vector2(0.5f, top + 10));
            Wall(new Vector2((right - 8) / 2f, BackWallY + 0.15f), new Vector2(right + 11, 0.3f));
            Wall(new Vector2((right - 8) / 2f, -6.5f), new Vector2(right + 11, 0.5f));
            // Assemble a full-height wall from clean pieces of the same CC0 laboratory sheet.
            for (int x = -9; x <= right; x++)
                BackWallColumn(x + 0.5f);
            BackWallColumn(-9.125f, 0.25f);
            BackWallColumn(right + 1.125f, 0.25f);
            LaboratoryArt.Draw(Room, "კარის ჩარჩო", new Vector2(0, BackWallY - 0.05f),
                PixelArt.Make(40, 66, (px, py) => new Color32(47, 53, 55, 255), new Vector2(0.5f, 0)), -289);
            LaboratoryArt.Draw(Room, "გასასვლელი კარი", new Vector2(0, BackWallY), LaboratoryArt.LabDoor, -288);
            var exit = Portal(Room, ExitPosition - RoomOrigin, true);
            DoorSign(Room, "კლასი", new Vector2(0, BackWallY + 3.3f), -287, 2.2f);
            LaboratoryArt.Draw(Room, "კედლის პლაკატი", new Vector2(5, BackWallY + 1.2f), PixelArt.Poster(), -288);
            // A window to the sky outside and warm evening light, so time of day reaches the lab too.
            LabAtmosphere.Instance?.AddWindow(Room, new Vector2(-5, BackWallY + 2.2f), new Vector2(1.6f, 1.4f));
            LabAtmosphere.Instance?.AddGlow(Room, new Vector2(-2, BackWallY - 3), new Color(1f, 0.72f, 0.4f), 9, true);
            LabAtmosphere.Instance?.AddDust(Room, new Vector2(-5, BackWallY - 3), new Vector2(6, 5));
            // Optional support is furniture: the notebook on a lectern, the shop and room plan on a clipboard stand.
            Furniture(new Vector2(-8, BackWallY - 1.7f), DecorArt.Lectern(), "[E] რვეული", () => game.Notebook.Open(), Glow.Optional);
            Furniture(new Vector2(right - 0.2f, BackWallY - 1.7f), DecorArt.PlanStand(), "[E] აღჭურვილობა და მოწყობა", () => Workshop.Open(), Glow.Reward);
            BuildEquipment();
            Room.gameObject.SetActive(Inside);
        }
        private void Furniture(Vector2 local, Sprite sprite, string prompt, System.Action use, Glow glow)
        {
            var art = LaboratoryArt.Draw(Room, prompt, local, sprite, -50 - Mathf.RoundToInt(local.y * 10));
            var shape = art.gameObject.AddComponent<BoxCollider2D>(); shape.size = new Vector2(1f, 0.4f); shape.offset = new Vector2(0, 0.2f);
            WorldInteractable.Create(Room, local + new Vector2(0, -0.5f), new Vector2(1.8f, 1.4f), 2.6f, () => prompt, use, () => Inside, glow);
        }
        private void BackWallColumn(float x, float width = 1)
        {
            void Piece(string name, float height, Sprite sprite, float scaleY = 1, int order = -290)
            {
                var art = LaboratoryArt.Draw(Room, name, new Vector2(x, BackWallY + height), sprite, order);
                art.transform.localScale = new Vector3(width, scaleY, 1);
            }
            Piece("კედლის ცოკოლი", 0, LaboratoryArt.WallBase);
            Piece("კედლის ქვედა პანელი", 0.25f, LaboratoryArt.WallLower);
            Piece("კედლის ზედა პანელი", 1.25f, LaboratoryArt.WallPaint);
            Piece("კედლის ზედა პანელი", 2.25f, LaboratoryArt.WallPaint);
            Piece("კედლის ზედა პანელი", 3.25f, LaboratoryArt.WallPaint, 0.5f);
            Piece("კედლის ზედა კიდე", 3.75f, LaboratoryArt.WallCap, 1, -289);
        }
        private void Wall(Vector2 point, Vector2 size)
        {
            var go = new GameObject("ოთახის საზღვარი", typeof(BoxCollider2D));
            go.transform.SetParent(Room, false); go.transform.localPosition = point; go.GetComponent<BoxCollider2D>().size = size;
            // Back-wall collision is hidden behind the painted face; other edges are cutaway caps.
            if (point.y > BackWallY) return;
            var edge = LaboratoryArt.Draw(go.transform, "კედლის კიდე", new Vector2(0, -size.y * 0.5f), LaboratoryArt.WallCap, -289);
            edge.transform.localScale = new Vector3(size.x, size.y * 4, 1);
        }
        private void BuildEquipment()
        {
            if (equipmentRoot != null) { equipmentRoot.gameObject.SetActive(false); Destroy(equipmentRoot.gameObject); }
            equipmentRoot = new GameObject("შენი აღჭურვილობა").transform; equipmentRoot.SetParent(Room, false);
            foreach (var placement in GameSaveManager.Instance.Data.labPlacements)
            {
                var item = Catalog.First(c => c.ID == placement.itemID);
                if (item.Decor) { BuildDecor(item, placement); continue; }
                var art = LaboratoryArt.Draw(equipmentRoot, item.Name, Cell(placement.column, placement.row), LaboratoryArt.Bench(true), 4);
                LaboratoryArt.CompleteBench(art, true);
                art.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = -50 - Mathf.RoundToInt((Cell(placement.column, placement.row).y + 0.65f) * 10);
                // Cover the full footprint up to the countertop's back edge (sprite is 1.5 units tall).
                var shape = art.gameObject.AddComponent<BoxCollider2D>(); shape.size = new Vector2(2.6f, 1.3f);
                shape.offset = new Vector2(0, 0.73f); shape.edgeRadius = 0.08f;
                shape.sharedMaterial = game.player.GetComponent<BoxCollider2D>().sharedMaterial;
                var tool = LaboratoryArt.Draw(art.transform, item.Name, new Vector2(0, 1.1f),
                    item.ID == "atom" || item.ID == "membrane" || item.ID == "gas" ? LaboratoryArt.Device :
                    item.ID == "chromatography" ? PixelArt.Poster() : item.ID == "molecule" ? PixelArt.Globe() : LaboratoryArt.Glassware, 6);
                tool.color = Color.Lerp(Color.white, item.Color, 0.35f);
                LabAtmosphere.Instance?.AddGlow(art.transform, new Vector2(0, 1.6f), Color.Lerp(item.Color, Color.white, 0.4f), 1.4f, true);
                var zone = new GameObject("ცდის დაწყება", typeof(BoxCollider2D), typeof(PersonalEquipment));
                zone.transform.SetParent(art.transform, false); zone.transform.localPosition = new Vector2(0, -0.5f);
                zone.GetComponent<BoxCollider2D>().isTrigger = true; zone.GetComponent<BoxCollider2D>().size = new Vector2(3.1f, 2.5f);
                zone.GetComponent<PersonalEquipment>().Initialize(this, item);
            }
            LabProgress.CheckFurnitureSets();
            Physics2D.SyncTransforms();
        }
        private void BuildDecor(LabEquipment item, LabPlacement placement)
        {
            var point = Cell(placement.column, placement.row);
            var sprite = DecorArt.Decor(item.ID);
            bool flat = item.ID == "rug";
            var art = LaboratoryArt.Draw(equipmentRoot, item.Name, point + new Vector2(0, flat ? 0.5f : 0), sprite, flat ? -299 : 0);
            if (!flat)
            {
                art.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = -50 - Mathf.RoundToInt((point.y + 0.4f) * 10);
                var shape = art.gameObject.AddComponent<BoxCollider2D>();
                float width = Mathf.Min(2.2f, sprite.bounds.size.x * 0.9f);
                shape.size = new Vector2(width, 0.6f); shape.offset = new Vector2(0, 0.35f); shape.edgeRadius = 0.06f;
                shape.sharedMaterial = game.player.GetComponent<BoxCollider2D>().sharedMaterial;
            }
            art.gameObject.AddComponent<PersonalDecor>().Initialize(item.ID, art);
            var zone = new GameObject("დეკორაცია", typeof(BoxCollider2D), typeof(PersonalEquipment));
            zone.transform.SetParent(art.transform, false); zone.transform.localPosition = new Vector2(0, flat ? 0 : -0.3f);
            zone.GetComponent<BoxCollider2D>().isTrigger = true; zone.GetComponent<BoxCollider2D>().size = new Vector2(2.4f, 2.0f);
            zone.GetComponent<PersonalEquipment>().Initialize(this, item);
        }
        public LabGame Game => game;
        private static Sprite signPlate;
        /// <summary>A small wooden-framed plaque with lettering, like a room sign above a door.</summary>
        public static void DoorSign(Transform parent, string text, Vector2 point, int order, float width = 3.6f)
        {
            signPlate ??= PixelArt.Make(48, 14, (x, y) =>
            {
                if ((x == 0 || x == 47) && (y == 0 || y == 13)) return Color.clear;
                if (x == 0 || x == 47 || y == 0 || y == 13) return new Color32(52, 34, 22, 255);
                if (x == 1 || x == 46 || y == 1 || y == 12) return new Color32(150, 104, 60, 255);
                if (x == 2 || x == 45 || y == 2 || y == 11) return new Color32(104, 70, 41, 255);
                if ((x == 4 || x == 43) && (y == 4 || y == 9)) return new Color32(214, 190, 128, 255);
                return y >= 10 ? new Color32(48, 84, 69, 255) : new Color32(36, 66, 54, 255);
            });
            // Native plate is 3 x 0.875 units; stretch only horizontally so the frame stays crisp.
            var plate = LaboratoryArt.Draw(parent, "კარის აბრა", point, signPlate, order);
            plate.transform.localScale = new Vector3(width / 3f, 1, 1);
            var go = new GameObject(text, typeof(RectTransform), typeof(Canvas)); go.transform.SetParent(parent, false);
            go.transform.localPosition = point; go.transform.localScale = Vector3.one / 64;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 64 - 36, 40);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace; go.GetComponent<Canvas>().sortingOrder = order + 1;
            var label = LabUI.Text(go.transform, text, 24, LabUI.Paper);
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            label.enableAutoSizing = true; label.fontSizeMin = 10; label.fontSizeMax = 24;
            label.fontStyle = TMPro.FontStyles.Bold;
            LabUI.Fill(label.rectTransform);
        }
        public static void Label(Transform parent, string text, Vector2 point, Color color)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Canvas)); go.transform.SetParent(parent, false);
            go.transform.localPosition = point; go.transform.localScale = Vector3.one / 65;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 75);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace; go.GetComponent<Canvas>().sortingOrder = 12;
            var label = LabUI.Text(go.transform, text, 18, color); label.alignment = TMPro.TextAlignmentOptions.Center;
            LabUI.WorldLabel(label, color); LabUI.Fill(label.rectTransform);
        }
    }
    public sealed class LabDoorway : MonoBehaviour
    {
        private PersonalLab lab; private bool exit;
        public void Initialize(PersonalLab owner, bool isExit)
        {
            lab = owner; exit = isExit;
            var zone = GetComponent<BoxCollider2D>();
            Focus.Attach(gameObject, zone.size, zone.offset, () => Glow.Door).Selectable = false;
        }
        private void OnTriggerEnter2D(Collider2D other)
        { if (other.GetComponentInParent<PlayerController>() != null) lab.Travel(exit); }
    }
    public sealed class PersonalEquipment : MonoBehaviour
    {
        private PersonalLab lab; private LabEquipment item; private bool near; private GameObject prompt;
        public void Initialize(PersonalLab owner, LabEquipment equipment)
        {
            lab = owner; item = equipment;
            prompt = new GameObject("მინიშნება"); prompt.transform.SetParent(transform, false);
            PersonalLab.Label(prompt.transform, "[E] " + item.Name, new Vector2(0, 3.4f), item.Color); prompt.SetActive(false);
            var zone = GetComponent<BoxCollider2D>();
            // Machines a research project is waiting on call out in coral; decorations stay quiet.
            focus = Focus.Attach(gameObject, zone.size, zone.offset, () => item.Decor ? Glow.Optional : Waiting() != null ? Glow.Urgent : Glow.Equipment);
        }
        private Focus focus;
        /// <summary>A research project waiting on this kind of experiment takes priority over free play.</summary>
        private ProjectProgress Waiting() => item.Decor ? null : ResearchProjects.Active.FirstOrDefault(p =>
        {
            var stage = ResearchProjects.Stage(p);
            return stage != null && stage.Kind == StageKind.Experiment &&
                (stage.Target == item.ID || stage.Target == "water" && item.ID == "analysis" || stage.Target == "distill" && item.ID == "thermo");
        });
        private void Update()
        {
            focus.Near = near && lab.Inside;
            bool focused = focus.IsFocused && !LabUI.AnyOpen;
            prompt.SetActive(focused);
            if (!focused || !TouchControls.Interact) return;
            var game = lab.Game;
            if (item.Decor) { PersonalDecor.Use(game, item.ID); return; }
            var waiting = Waiting();
            if (waiting != null) { game.Games.Open(ResearchProjects.Stage(waiting).Target, waiting.id); return; }
            if (ExperimentGamesUI.IsGame(item.ID)) { game.Games.Open(item.ID); return; }
            LabProgress.CountStat("activity:any"); LabProgress.CountStat("activity:" + item.ID);
            lab.Activities.Open(item.ID);
        }
        private void OnTriggerEnter2D(Collider2D other) { if (other.GetComponentInParent<PlayerController>() != null) near = true; }
        private void OnTriggerExit2D(Collider2D other) { if (other.GetComponentInParent<PlayerController>() != null) near = false; }
    }
}
