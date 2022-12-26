using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago
{
    public class StardewItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int SellPrice { get; private set; }
        public int Edibility { get; private set; }
        public string Type { get; private set; }
        public string Category { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string Misc1 { get; private set; }
        public string Misc2 { get; private set; }
        public string BuffDuration { get; private set; }

        public StardewItem(int id, string name, int sellPrice, int edibility, string type, string category, string displayName, string description, string misc1 = "", string misc2 = "", string buffDuration = "")
        {
            Id = id;
            Name = name;
            SellPrice = sellPrice;
            Edibility = edibility;
            Type = type;
            Category = category;
            DisplayName = displayName;
            Description = description;
            Misc1 = misc1;
            Misc2 = misc2;
            BuffDuration = buffDuration;
        }
    }
}
