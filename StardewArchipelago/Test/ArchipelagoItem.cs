using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Test
{
    public class ArchipelagoItem
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public ItemClassification Classification { get; set; }

        public ArchipelagoItem(string name, long id, ItemClassification classification)
        {
            Name = name;
            Id = id;
            Classification = classification;
        }
    }

    public enum ItemClassification
    {
        Progression,
        Useful,
        Filler,
    }
}
