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
using StardewValley.Tools;

namespace StardewArchipelago.Items
{
    public class SpecialItemManager
    {
        private Dictionary<string, Action<int>> _playerUnlocks;
        private Dictionary<string, Func<Item>> _specialItems;

        public SpecialItemManager()
        {
            _playerUnlocks = new Dictionary<string, Action<int>>();
            _specialItems = new Dictionary<string, Func<Item>>();
            RegisterPlayerImprovement();
            RegisterUniqueItems();
        }

        public bool IsUnlock(string unlockName)
        {
            return _playerUnlocks.ContainsKey(unlockName);
        }

        public void PerformUnlock(string unlockName, int numberNewReceived)
        {
            _playerUnlocks[unlockName](numberNewReceived);
        }

        public bool IsItem(string itemName)
        {
            return _specialItems.ContainsKey(itemName);
        }

        public Item GetSpecialItem(string itemName)
        {
            return _specialItems[itemName]();
        }

        private void RegisterPlayerImprovement()
        {
            _playerUnlocks.Add("Stardrop", (_) => ReceiveStardrop());
            _playerUnlocks.Add("Dwarvish Translation Guide", (_) => ReceiveDwarvishTranslationGuide());
            _playerUnlocks.Add("Skull Key", (_) => ReceiveSkullKey());
            _playerUnlocks.Add("Rusty Key", (_) => ReceiveRustyKey());
        }

        private void RegisterUniqueItems()
        {
            _specialItems.Add("Golden Scythe", ReceiveGoldenScythe);
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

        private Item ReceiveGoldenScythe()
        {
            Game1.playSound("parry");
            Game1.player.mailReceived.Add("gotGoldenScythe");
            return new MeleeWeapon(53);
        }
    }
}
