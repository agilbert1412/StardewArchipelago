using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Characters;

namespace StardewArchipelago.Items.Traps
{
    internal class TemporaryBaby : Child
    {

        public TemporaryBaby(string name, bool isMale, bool isDarkSkinned, Farmer parent, int age) : base(name, isMale, isDarkSkinned, parent)
        {
            Age = age;
            speed = age + 1;
        }

        public override void dayUpdate(int dayOfMonth)
        {
            currentLocation.characters.Remove(this);
        }
    }
}
