using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class FreeBlueprint : BluePrint
    {
        public FreeBlueprint(string name, string sendingPlayerName) : base(name)
        {
            itemsRequired.Clear();
            moneyRequired = 0;
            displayName = $"Free {displayName}";
            description = $"A gift from {sendingPlayerName}. {description}";
        }
    }
}
