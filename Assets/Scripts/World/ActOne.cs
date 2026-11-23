using System.Collections;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// Act 1, "First day in the lab": the four opening missions and the props they use.
    /// Safety first (coat, then fix three hazards), What is matter? (sorting at the desk),
    /// Three states (melt, boil and condense on the experiment cart) and the showpiece
    /// Steam machine (fill a kettle, heat it, a paper wheel spins and goes to your lab).
    /// Every step is done in the world; one short line names the idea after you see it.
    /// </summary>
    public sealed class ActOne : MonoBehaviour
    {
        // Classroom-local positions, in open floor so nothing blocks walking.
        public static readonly Vector2 CartPoint = new Vector2(44.6f, 5.0f), SpillPoint = new Vector2(44.6f, -3f),
            BottlePoint = new Vector2(59.5f, 3f), KettlePoint = new Vector2(56.3f, 10.55f);
        public static Mission FirstMix, Safety, States, Steam;

        private LabGame game;
        private Transform room;
        private SpriteRenderer flame, papers, puddle, bottleLabel, beaker, water, ice, plate, kettleSink, kettleCart, wheel, carry;
        private readonly SpriteRenderer[] drops = new SpriteRenderer[4];
        private bool busy, heating;
        /// <summary>True while a cart experiment is running (the activity music plays).</summary>
        public bool Busy => busy;
        private float wheelSpeed, nextRefresh;
        private static GameSaveData Data => GameSaveManager.Instance.Data;
        private static bool F(string flag) => Missions.Flag(flag);

        public void Initialize(LabGame lab)
        {
            game = lab;
            room = lab.classroom;
            BuildProps();
            DefineMissions();
            Missions.Refresh();
        }

        private Vector3 World(Vector2 local) => room.TransformPoint(local);

        // ---------------------------------------------------------------- missions

        private void DefineMissions()
        {
            int hazards() => new[] { "hz_flame", "hz_spill", "hz_bottle" }.Count(F);
            FirstMix = new Mission
            {
                ID = "first_mix", Title = "პირველი რეაქცია", Coins = 10,
                Steps = new[]
                {
                    new MissionStep { Text = () => "აიღე ხალათი მბზინავ კარადაში.", Done = () => Data.starterCoatCollected,
                        Target = () => World(game.lockerPosition + new Vector2(0, -1f)) },
                    new MissionStep { Text = () => "pH ნაყენების მაგიდაზე ფერადი ნაყენი გააკეთე.", Done = () => Kits.PotionMade,
                        Target = () => game.StationPoint(StationType.AcidBaseStation) },
                    new MissionStep { Text = () => "რეაქციების მაგიდასთან შეურიე ძმარი და სოდა.", Done = () => LabProgress.Owns(Mixing.Key("vinegar", "soda")),
                        Target = () => game.StationPoint(StationType.ReactionTable) },
                },
            };
            Safety = new Mission
            {
                ID = "safety", Title = "უსაფრთხოება პირველ ადგილზე", Coins = 10,
                Steps = new[]
                {
                    new MissionStep { Text = () => "აიღე ხალათი მბზინავ კარადაში.", Done = () => Data.starterCoatCollected,
                        Target = () => World(game.lockerPosition + new Vector2(0, -1f)) },
                    new MissionStep { Text = () => "იპოვე და გამოასწორე საფრთხეები: " + hazards() + "/3", Done = () => hazards() == 3,
                        Target = () => !F("hz_flame") ? World(CartPoint) : !F("hz_spill") ? World(SpillPoint) : World(BottlePoint) },
                },
            };
            States = new Mission
            {
                ID = "states", Title = "მატერიის სამი მდგომარეობა", Coins = 15,
                Steps = new[]
                {
                    new MissionStep { Text = () => "ცდის ურიკაზე ყინული გააცხელე.", Done = () => F("melted"), Target = () => World(CartPoint) },
                    new MissionStep { Text = () => "კიდევ გააცხელე. რა მოხდება?", Done = () => F("boiled"), Target = () => World(CartPoint) },
                    new MissionStep { Text = () => "ორთქლს ცივი თეფში დაუჭირე.", Done = () => F("condensed"), Target = () => World(CartPoint) },
                },
            };
            Steam = new Mission
            {
                ID = "steam", Title = "ორთქლის მანქანა", Coins = 25, Item = "steam_wheel",
                Steps = new[]
                {
                    new MissionStep { Text = () => "აიღე ჩაიდანი ნიჟარასთან და წყლით აავსე.", Done = () => F("kettle_full"), Target = () => World(KettlePoint + new Vector2(0, -1.9f)) },
                    new MissionStep { Text = () => "ჩაიდანი ცდის ურიკაზე დადგი.", Done = () => F("kettle_placed"), Target = () => World(CartPoint) },
                    new MissionStep { Text = () => "აანთე სანთურა და ნახე, რა მოხდება.", Done = () => F("wheel_spun"), Target = () => World(CartPoint) },
                },
            };
            Missions.All = new[] { FirstMix, Safety, States, Steam };
        }

        // ---------------------------------------------------------------- props

        private SpriteRenderer Art(Transform parent, string name, Vector2 local, Sprite sprite, int order)
        {
            var art = LaboratoryArt.Draw(parent, name, local, sprite, order);
            return art;
        }

        private void BuildProps()
        {
            // The experiment cart: no collider, so it never blocks the aisle.
            var cart = Art(room, "ცდის ურიკა", CartPoint, DecorArt.Paint("a1_cart", 36, 22, p =>
            {
                p.Fill(3, 3, 4, 16, DecorArt.MetalDark); p.Fill(31, 3, 32, 16, DecorArt.MetalDark);
                p.Box(0, 15, 35, 19, DecorArt.Metal, DecorArt.MetalDark); p.Box(1, 7, 34, 9, DecorArt.Metal, DecorArt.MetalDark);
                p.Disc(4, 2, 2.2f, DecorArt.Line); p.Disc(31, 2, 2.2f, DecorArt.Line);
            }), 6).transform;
            var burner = Art(cart, "სანთურა", new Vector2(-0.45f, 1.2f), DecorArt.Paint("a1_burner", 8, 6, p => { p.Box(0, 0, 7, 2, DecorArt.MetalDark, DecorArt.Line); p.Fill(3, 3, 4, 5, DecorArt.Metal); }), 7);
            flame = Art(burner.transform, "ალი", new Vector2(0, 0.42f), PixelArt.Flame(), 8);
            flame.transform.localScale = Vector3.one * 0.45f;
            papers = Art(cart, "ქაღალდები", new Vector2(0.35f, 1.2f), DecorArt.Paint("a1_papers", 10, 6, p =>
            { p.Fill(0, 0, 9, 1, DecorArt.Paper); p.Fill(1, 2, 9, 3, new Color32(232, 222, 196, 255)); p.Fill(0, 4, 8, 5, DecorArt.Paper); }), 7);
            beaker = Art(cart, "ჭიქა", new Vector2(-0.45f, 1.85f), DecorArt.Paint("a1_beaker", 10, 12, p =>
            { p.Box(0, 0, 9, 11, new Color32(220, 240, 250, 90), new Color32(190, 220, 235, 255)); p.Fill(1, 11, 8, 11, Color.clear); }), 9);
            water = Art(beaker.transform, "წყალი", new Vector2(0, -0.32f), DecorArt.Square(), 8);
            water.color = new Color(0.45f, 0.7f, 0.95f, 0.85f);
            ice = Art(beaker.transform, "ყინული", new Vector2(0, 0.08f), DecorArt.Paint("a1_ice", 6, 6, p => { p.Box(0, 0, 5, 5, new Color32(225, 245, 255, 255), new Color32(170, 210, 240, 255)); p.Dot(1, 4, Color.white); }), 8);
            plate = Art(cart, "ცივი თეფში", new Vector2(-0.45f, 2.75f), DecorArt.Paint("a1_plate", 14, 3, p => { p.Box(0, 0, 13, 2, new Color32(200, 215, 230, 255), DecorArt.MetalDark); }), 9);
            for (int i = 0; i < drops.Length; i++)
            {
                drops[i] = Art(plate.transform, "წვეთი", new Vector2(-0.3f + i * 0.2f, -0.12f), DecorArt.Square(), 9);
                drops[i].transform.localScale = new Vector3(1.5f, 2, 1);
                drops[i].color = new Color(0.5f, 0.75f, 1f, 0);
            }
            var kettleSprite = DecorArt.Paint("a1_kettle", 14, 12, p =>
            {
                p.Box(2, 0, 11, 8, new Color32(210, 120, 90, 255), DecorArt.Line); p.Fill(4, 9, 9, 10, DecorArt.Line);
                p.Fill(12, 4, 13, 6, DecorArt.Line); p.Fill(0, 3, 1, 7, DecorArt.Line); p.Dot(4, 6, Color.white);
            });
            kettleSink = Art(room, "ჩაიდანი", KettlePoint, kettleSprite, 8);
            kettleCart = Art(cart, "ჩაიდანი ურიკაზე", new Vector2(-0.45f, 1.55f), kettleSprite, 9);
            var stick = Art(cart, "ბორბლის ღერძი", new Vector2(0.55f, 1.2f), DecorArt.Paint("a1_stick", 1, 14, p => p.Fill(0, 0, 0, 13, DecorArt.WoodDark)), 8);
            wheel = Art(stick.transform, "ქაღალდის ბორბალი", new Vector2(0, 0.9f), DecorArt.Paint("a1_wheel", 12, 12, p =>
            {
                // Four blades, one per quadrant, each a triangle in its own colour.
                Color32[] c = { DecorArt.Coral, DecorArt.Mint, DecorArt.Gold, DecorArt.Violet };
                for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    int q = (x >= 6 ? 1 : 0) + (y >= 6 ? 2 : 0);
                    int u = x >= 6 ? x - 6 : 5 - x, v = y >= 6 ? y - 6 : 5 - y;
                    if (q == 0 || q == 3 ? u >= v : v >= u) p.Dot(x, y, c[q]);
                }
                p.Disc(5.5f, 5.5f, 1.2f, DecorArt.Line);
            }, 16, new Vector2(0.5f, 0.5f)), 9);
            wheel.transform.localScale = Vector3.one * 1.4f;
            puddle = Art(room, "დაღვრილი სითხე", SpillPoint, DecorArt.Puddle(0), 4);
            var crate = Art(room, "ყუთი", BottlePoint, DecorArt.Crates(), 6);
            crate.transform.localScale = Vector3.one * 0.55f;
            var bottle = Art(room, "უსახელო ბოთლი", BottlePoint + new Vector2(0.1f, 0.95f), DecorArt.Jar(), 7);
            bottle.transform.localScale = Vector3.one * 1.6f;
            bottleLabel = Art(bottle.transform, "ეტიკეტი", new Vector2(0, 0.2f), DecorArt.Square(), 8);
            bottleLabel.transform.localScale = new Vector3(5, 2.5f, 1);
            bottleLabel.color = DecorArt.Gold;
            carry = Art(game.player.transform, "ხელში ჩაიდანი", new Vector2(0, 2.95f), kettleSprite, 13);

            bool coat() => Data.profileCompleted && Data.starterCoatCollected;
            bool Here(Vector2 local) => Missions.IsTarget(World(local));
            WorldInteractable.Create(room, CartPoint + new Vector2(0, -0.2f), new Vector2(2.6f, 1.6f), 3.6f,
                () => CartPrompt() ?? "", UseCart, () => CartPrompt() != null, Glow.Equipment, () => Here(CartPoint));
            WorldInteractable.Create(room, SpillPoint, new Vector2(2f, 1.4f), 1.8f, () => "[E] მოწმენდა",
                () => Fix("hz_spill", "დაღვრილზე ფეხი აგიცდება. მაშინვე მოწმინდე!"), () => coat() && !F("hz_spill"), Glow.Urgent, () => Here(SpillPoint));
            WorldInteractable.Create(room, BottlePoint + new Vector2(0, -0.3f), new Vector2(2f, 1.6f), 2.9f, () => "[E] ეტიკეტის დაწერა",
                () => Fix("hz_bottle", "უცნობ ბოთლს ვერავინ გამოიყენებს. ყოველთვის დააწერე, რა არის შიგნით!"), () => coat() && !F("hz_bottle"), Glow.Urgent, () => Here(BottlePoint));
            WorldInteractable.Create(room, KettlePoint + new Vector2(0, -1.9f), new Vector2(2f, 1.3f), 3.9f, () => "[E] ჩაიდნის აღება და ავსება",
                FillKettle, () => Missions.Current == Steam && !F("kettle_full"), Glow.Equipment, () => Here(KettlePoint + new Vector2(0, -1.9f)));
        }

        private string CartPrompt()
        {
            if (busy || !Data.profileCompleted) return null;
            if (!F("hz_flame")) return Data.starterCoatCollected ? "[E] ცეცხლის ჩაქრობა" : null;
            if (Missions.Current == States)
                return !F("melted") ? "[E] ყინულის გაცხელება" : !F("boiled") ? "[E] კიდევ გაცხელება" : "[E] ცივი თეფშის მიტანა";
            if (Missions.Current == Steam && F("kettle_full"))
                return !F("kettle_placed") ? "[E] ჩაიდნის დადგმა" : "[E] სანთურის ანთება";
            return null;
        }

        public void UseCart()
        {
            if (busy || CartPrompt() == null) return;
            if (!F("hz_flame")) { Fix("hz_flame", "ღია ცეცხლი ქაღალდთან ხანძარს იწვევს. როცა არ იყენებ, ჩააქრე!"); Smoke(); return; }
            if (Missions.Current == States) StartCoroutine(!F("melted") ? Melt() : !F("boiled") ? Boil() : Condense());
            else if (!F("kettle_placed")) { AudioManager.Instance?.Click(); Missions.Set("kettle_placed"); }
            else StartCoroutine(SteamWheel());
        }

        private void Fix(string flag, string lesson)
        {
            if (F(flag)) return;
            AmbientAudio.Instance?.Sparkle();
            Missions.Set(flag);
            game.HUD.Notify(lesson);
        }

        private void FillKettle()
        {
            if (F("kettle_full")) return;
            AmbientAudio.Instance?.Splash();
            Missions.Set("kettle_full");
        }

        // ---------------------------------------------------------------- experiments

        private Vector2 BeakerLocal => (Vector2)room.InverseTransformPoint(beaker.transform.position);

        private IEnumerator Melt()
        {
            busy = heating = true;
            for (float t = 0; t < 2.6f; t += Time.deltaTime)
            {
                float k = t / 2.6f;
                ice.transform.localScale = Vector3.one * Mathf.Lerp(1, 0.05f, k);
                SetWater(Mathf.Lerp(0.05f, 0.55f, k));
                yield return null;
            }
            busy = heating = false;
            Missions.Set("melted");
            game.HUD.Notify("მყარი ყინული თხევად წყლად იქცა. ეს დნობაა!");
        }

        private IEnumerator Boil()
        {
            busy = heating = true;
            float next = 0;
            for (float t = 0; t < 3.2f; t += Time.deltaTime)
            {
                SetWater(Mathf.Lerp(0.55f, 0.25f, t / 3.2f));
                if (t > next)
                {
                    next = t + 0.12f;
                    var atmosphere = LabAtmosphere.Instance;
                    atmosphere?.Puff(room, BeakerLocal + new Vector2(Random.Range(-0.2f, 0.2f), -0.2f), new Color(0.9f, 1, 1, 0.9f), new Vector2(0, 0.6f), 0.4f, 10);
                    if (t > 0.8f) atmosphere?.Puff(room, BeakerLocal + new Vector2(Random.Range(-0.15f, 0.15f), 0.45f), new Color(1, 1, 1, 0.8f), new Vector2(Random.Range(-0.1f, 0.1f), 1.1f), 1.3f, 10, DecorArt.Soft(), 0.35f);
                }
                yield return null;
            }
            busy = heating = false;
            Missions.Set("boiled");
            game.HUD.Notify("წყალი დუღს და ორთქლად იქცევა: თხევადი → აირი!");
        }

        private IEnumerator Condense()
        {
            busy = heating = true;
            plate.enabled = true;
            float next = 0;
            for (float t = 0; t < 3f; t += Time.deltaTime)
            {
                if (t > next)
                {
                    next = t + 0.15f;
                    LabAtmosphere.Instance?.Puff(room, BeakerLocal + new Vector2(Random.Range(-0.15f, 0.15f), 0.45f), new Color(1, 1, 1, 0.7f), new Vector2(0, 0.7f), 0.7f, 10, DecorArt.Soft(), 0.3f);
                }
                for (int i = 0; i < drops.Length; i++)
                    drops[i].color = new Color(0.5f, 0.75f, 1f, Mathf.Clamp01(t * 1.3f - i * 0.6f));
                yield return null;
            }
            busy = heating = false;
            Missions.Set("condensed");
            game.HUD.Notify("ცივ თეფშზე ორთქლი ისევ წყლის წვეთებად იქცა. ეს კონდენსაციაა!");
        }

        private IEnumerator SteamWheel()
        {
            busy = heating = true;
            float next = 0;
            for (float t = 0; t < 4.5f; t += Time.deltaTime)
            {
                wheelSpeed = Mathf.Lerp(0, 900, Mathf.Clamp01((t - 0.8f) / 2.5f));
                if (t > next && t > 0.6f)
                {
                    next = t + 0.1f;
                    Vector2 spout = (Vector2)room.InverseTransformPoint(kettleCart.transform.position) + new Vector2(0.45f, 0.35f);
                    LabAtmosphere.Instance?.Puff(room, spout, new Color(1, 1, 1, 0.8f), new Vector2(1.3f, 0.5f), 0.9f, 10, DecorArt.Soft(), 0.3f);
                }
                yield return null;
            }
            busy = heating = false;
            wheelSpeed = 120;
            Missions.Set("wheel_spun");
            game.HUD.Notify("ორთქლი ბორბალს აბრუნებს! ასე მუშაობდა პირველი ორთქლის მანქანები. ბორბალი შენს ლაბორატორიაშია.");
        }

        /// <summary>Fills the beaker from its bottom (the beaker sprite's pivot) to <paramref name="level"/> of its height.</summary>
        private void SetWater(float level)
        {
            float height = Mathf.Max(0.01f, level * 0.65f);
            water.transform.localScale = new Vector3(8, height * 16, 1);
            water.transform.localPosition = new Vector3(0, 0.06f + height / 2, 0);
        }

        private void Smoke()
        {
            for (int i = 0; i < 6; i++)
                LabAtmosphere.Instance?.Puff(room, (Vector2)room.InverseTransformPoint(flame.transform.position) + new Vector2(Random.Range(-0.1f, 0.1f), 0.1f),
                    new Color(0.6f, 0.6f, 0.65f, 0.8f), new Vector2(Random.Range(-0.2f, 0.2f), 0.8f + i * 0.1f), 1.2f, 10, DecorArt.Soft(), 0.25f);
        }

        // ---------------------------------------------------------------- per frame

        private void Update()
        {
            if (Time.unscaledTime > nextRefresh) { nextRefresh = Time.unscaledTime + 0.5f; Missions.Refresh(); }
            bool hazardFlame = !F("hz_flame");
            flame.enabled = hazardFlame || heating;
            flame.transform.localScale = Vector3.one * (0.45f + Mathf.Sin(Time.time * 18) * 0.04f);
            papers.enabled = hazardFlame;
            puddle.enabled = !F("hz_spill");
            bottleLabel.enabled = F("hz_bottle");
            bool kettlePlaced = F("kettle_placed");
            bool beakerShown = !kettlePlaced && (Missions.Current == States || F("melted"));
            beaker.enabled = water.enabled = beakerShown;
            plate.enabled = beakerShown && (F("condensed") || busy && F("boiled"));
            foreach (var d in drops) d.enabled = plate.enabled;
            if (!busy)
            {
                ice.enabled = beakerShown && !F("melted");
                ice.transform.localScale = Vector3.one;
                SetWater(!F("melted") ? 0.05f : !F("boiled") ? 0.55f : 0.25f);
                if (F("condensed")) foreach (var d in drops) d.color = new Color(0.5f, 0.75f, 1f, 1);
            }
            kettleSink.enabled = !F("kettle_full");
            carry.enabled = F("kettle_full") && !kettlePlaced;
            kettleCart.enabled = wheel.enabled = kettlePlaced;
            wheel.transform.parent.GetComponent<SpriteRenderer>().enabled = kettlePlaced;
            if (F("wheel_spun") && !busy) wheelSpeed = 120;
            wheel.transform.localRotation = Quaternion.Euler(0, 0, wheel.transform.localEulerAngles.z - wheelSpeed * Time.deltaTime);
        }
    }
}
