using System.Linq;

namespace ChemistryGame
{
    public sealed class Order
    {
        public string Key, Npc, Text;
        public MixSign Wants;
        public string Name => Classmates.Get(Npc)?.Name ?? Npc;
    }

    /// <summary>
    /// Three classmate orders a day, judged by what the product does (fizzes, changes colour…),
    /// so any mix that shows the right sign fills them. The day's three are fixed by the date.
    /// </summary>
    public static class Orders
    {
        public const int Pay = 12, Friendship = 3, XP = 5, PerDay = 3;

        private static readonly (string npc, MixSign wants, string text)[] Wishes =
        {
            ("nika", MixSign.Gas, "რაკეტის მოდელს აირი სჭირდება — რამე, რაც ბუშტუკებს გამოყოფს!"),
            ("nika", MixSign.Heat, "ცივა! რეაქცია მინდა, რომელიც თავისით თბება."),
            ("mariam", MixSign.Solid, "კოლექციისთვის ნალექი მინდა: ორი სითხიდან მყარი!"),
            ("mariam", MixSign.Gas, "ჩემი ქვები მჟავაში შიშინებენ. მაჩვენე რამე, რაც აქაფდება."),
            ("giorgi", MixSign.Dissolves, "მშვიდი ცდა მინდა: რამე, რაც უბრალოდ იხსნება. აფეთქებების გარეშე!"),
            ("giorgi", MixSign.Nothing, "შეურიე ორი რამ ისე, რომ არაფერი მოხდეს. ასე უსაფრთხოა!"),
            ("ana", MixSign.Colour, "სპექტაკლისთვის ჯადოსნური სითხე მინდა, რომელიც ფერს იცვლის!"),
            ("ana", MixSign.Dissolves, "ხატვისთვის ფერადი ხსნარი მჭირდება — რამე, რაც წყალში იხსნება."),
            ("luka", MixSign.Heat, "ბაზრობაზე ყველას გავაოცებ: რეაქცია, რომელიც თბება!"),
            ("luka", MixSign.Colour, "შეჯიბრისთვის ფერის შეცვლის ხრიკი მინდა."),
            ("ketevan", MixSign.Solid, "თაროსთვის ნიმუში მინდა: ნალექი სინჯარაში."),
            ("ketevan", MixSign.Gas, "ვულკანის მოდელს ქაფი სჭირდება — ბევრი ბუშტუკი!"),
        };

        private static GameSaveData Data => GameSaveManager.Instance.Data;
        private static string day;
        private static Order[] today;
        private static bool wasEarly;

        /// <summary>Today's orders: three different classmates wanting three different signs.</summary>
        public static Order[] Today
        {
            get
            {
                string key = SchoolCalendar.DayKey;
                bool early = Mixing.Discovered < 4;
                if (today != null && day == key && early == wasEarly) return today;
                wasEarly = early;
                int seed = key.Aggregate(17, (h, c) => h * 31 + c) & int.MaxValue;
                var picked = new System.Collections.Generic.List<Order>();
                for (int i = 0; i < Wishes.Length && picked.Count < PerDay; i++)
                {
                    int w = (seed + i * 5) % Wishes.Length;
                    var (npc, wants, text) = Wishes[w];
                    if (picked.Any(o => o.Npc == npc || o.Wants == wants)) continue;
                    if (early && (wants == MixSign.Heat || wants == MixSign.Solid) && !Data.claimedEvents.Contains("order:" + key + ":" + w)) continue;
                    picked.Add(new Order { Key = "order:" + key + ":" + w, Npc = npc, Wants = wants, Text = text });
                }
                day = key; today = picked.ToArray();
                return today;
            }
        }

        public static bool Done(Order order) => Data.claimedEvents.Contains(order.Key);
        public static Order Next => Today.FirstOrDefault(o => !Done(o));
        public static int Left => Today.Count(o => !Done(o));

        /// <summary>Fills the first open order this sign satisfies and pays for it; null when none matched.</summary>
        public static Order Deliver(MixSign sign)
        {
            var order = Today.FirstOrDefault(o => !Done(o) && o.Wants == sign);
            if (order == null) return null;
            Data.claimedEvents.Add(order.Key);
            Data.chemCoins += Pay;
            Classmates.State(order.Npc).friendship += Friendship;
            LabProgress.AddReputation(1);
            GameSaveManager.Instance.AddXP(XP);
            return order;
        }

        /// <summary>The HUD line when no story mission is running.</summary>
        public static string Objective => Next is Order o ? o.Name + ": „" + o.Text + "“" : "დღის შეკვეთები შესრულდა! აღმოაჩინე ახალი რეაქციები.";
    }
}
