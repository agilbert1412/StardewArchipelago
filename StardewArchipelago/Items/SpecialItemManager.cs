using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Items
{
    public class SpecialItemManager
    {
        private Dictionary<string, Action<int>> _specialItems;

        public SpecialItemManager()
        {
            _specialItems = new Dictionary<string, Action<int>>();
            RegisterPlayerImprovement();
        }

        public bool IsUnlock(string unlockName)
        {
            return _specialItems.ContainsKey(unlockName);
        }

        public void PerformUnlock(string unlockName, int numberNewReceived)
        {
            _specialItems[unlockName](numberNewReceived);
        }

        private void RegisterPlayerImprovement()
        {
            _specialItems.Add("Stardrop", (_) => ReceiveStardrop());
            _specialItems.Add("Dwarvish Translation Guide", (_) => ReceiveDwarvishTranslationGuide());
            _specialItems.Add("Skull Key", (_) => ReceiveSkullKey());
            _specialItems.Add("Rusty Key", (_) => ReceiveRustyKey());
        }

        public void ReceiveStardropIfDeserved(int expectedNumber)
        {
            var expectedEnergy = 270 + (expectedNumber * 34);
            if (expectedEnergy > Game1.player.MaxStamina)
            {
                ReceiveStardrop();
            }
        }

        private void ReceiveStardrop()
        {
            var stardrop = new StardewValley.Object(434, 1);
            Game1.player.eatObject(stardrop, true);
        }

        private void ReceiveDwarvishTranslationGuide()
        {
            Game1.player.canUnderstandDwarves = true;
            Game1.playSound("fireball");
        }

        private void ReceiveSkullKey()
        {
            Game1.player.hasSkullKey = true;
            Game1.player.addQuest(19);
        }

        private void ReceiveRustyKey()
        {
            Game1.player.hasRustyKey = true;
        }
    }
}
