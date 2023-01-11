using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Items
{
    public class UnlockManager
    {
        public const string PROGRESSIVE_MINE_ELEVATOR_AP_NAME = "Progressive Mine Elevator";
        public const string PROGRESSIVE_FISHING_ROD_AP_NAME = "Progressive Fishing Rod";
        public const string GOLDEN_SCYTHE_AP_NAME = "Golden Scythe";

        private Mailman _mail;
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public UnlockManager(Mailman mailman)
        {
            _mail = mailman;
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            RegisterCommunityCenterRepairs();
            RegisterPlayerImprovement();
            RegisterProgressiveTools();
            RegisterMineElevators();
        }

        public bool IsUnlock(string unlockName)
        {
            return _unlockables.ContainsKey(unlockName);
        }

        public LetterAttachment PerformUnlock(ReceivedItem unlock)
        {
            return _unlockables[unlock.ItemName](unlock);
        }

        private void RegisterCommunityCenterRepairs()
        {
            _unlockables.Add("Bridge Repair", RepairBridge);
            _unlockables.Add("Greenhouse", RepairGreenHouse);
            _unlockables.Add("Glittering Boulder Removed", RemoveGlitteringBoulder);
            _unlockables.Add("Minecarts Repair", RepairMinecarts);
            _unlockables.Add("Bus Repair", RepairBus);
            // _unlockables.Add("Movie Theater", BuildMovieTheater);
        }

        private void RegisterPlayerImprovement()
        {
            _unlockables.Add("Progressive Backpack", SendProgressiveBackpackLetter);
            _unlockables.Add("Stardrop", SendStardropLetter);
            _unlockables.Add("Dwarvish Translation Guide", SendDwarvishTranslationGuideLetter);
            _unlockables.Add("Skull Key", SendSkullKeyLetter);
            _unlockables.Add("Rusty Key", SendRustyKeyLetter);
        }

        private void RegisterProgressiveTools()
        {
            _unlockables.Add("Progressive Axe", SendProgressiveAxeLetter);
            _unlockables.Add("Progressive Pickaxe", SendProgressivePickaxeLetter);
            _unlockables.Add("Progressive Hoe", SendProgressiveHoeLetter);
            _unlockables.Add("Progressive Watering Can", SendProgressiveWateringCanLetter);
            _unlockables.Add("Progressive Trash Can", SendProgressiveTrashCanLetter);
            _unlockables.Add(PROGRESSIVE_FISHING_ROD_AP_NAME, SendProgressiveFishingRodLetter);
        }

        private void RegisterUniqueItems()
        {
            _unlockables.Add(GOLDEN_SCYTHE_AP_NAME, SendGoldenScytheLetter);
        }

        private void RegisterMineElevators()
        {
            _unlockables.Add(PROGRESSIVE_MINE_ELEVATOR_AP_NAME, SendProgressiveMineElevatorLetter);
        }

        private LetterVanillaAttachment RepairBridge(ReceivedItem receivedItem)
        {
            return new LetterVanillaAttachment(receivedItem, "ccCraftsRoom", true);
        }

        private LetterVanillaAttachment RepairGreenHouse(ReceivedItem receivedItem)
        {
            return new LetterVanillaAttachment(receivedItem, "ccPantry", true);
        }

        private LetterVanillaAttachment RemoveGlitteringBoulder(ReceivedItem receivedItem)
        {
            return new LetterVanillaAttachment(receivedItem, "ccFishTank", true);
        }

        private LetterVanillaAttachment RepairMinecarts(ReceivedItem receivedItem)
        {
            return new LetterVanillaAttachment(receivedItem, "ccBoilerRoom", true);
        }

        private LetterVanillaAttachment RepairBus(ReceivedItem receivedItem)
        {
            return new LetterVanillaAttachment(receivedItem, "ccVault", true);
        }

        private LetterCustomAttachment SendProgressiveBackpackLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, IncreaseBackpackLevel);
        }

        private void IncreaseBackpackLevel()
        {
            var previousMaxItems = Game1.player.MaxItems;
            var backpackName = "";
            switch (Game1.player.MaxItems)
            {
                case < 12:
                    Game1.player.MaxItems = 12;
                    break;
                case < 24:
                    Game1.player.MaxItems = 24;
                    backpackName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
                    break;
                case >= 24:
                    Game1.player.MaxItems = 36;
                    backpackName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
                    break;
            }

            if (previousMaxItems >= Game1.player.MaxItems)
            {
                return;
            }

            while (Game1.player.Items.Count < Game1.player.MaxItems)
            {
                Game1.player.Items.Add(null);
            }
            Game1.player.holdUpItemThenMessage(new SpecialItem(99, backpackName));
        }

        private LetterAttachment SendProgressiveMineElevatorLetter(ReceivedItem receivedItem)
        {
            return new LetterAttachment(receivedItem);
        }

        public void ReceiveStardropIfDeserved(int expectedNumber)
        {
            var expectedEnergy = 270 + (expectedNumber * 34);
            if (expectedEnergy > Game1.player.MaxStamina)
            {
                var stardrop = new StardewValley.Object(434, 1);
                Game1.player.eatObject(stardrop, true);
            }
        }

        private LetterItemIdAttachment SendStardropLetter(ReceivedItem receivedItem)
        {
            return new LetterItemIdAttachment(receivedItem, 434);
        }

        private LetterItemIdAttachment SendDwarvishTranslationGuideLetter(ReceivedItem receivedItem)
        {
            return new LetterItemIdAttachment(receivedItem, 326);
            /*Game1.player.canUnderstandDwarves = true;
            Game1.playSound("fireball");*/
        }

        private LetterCustomAttachment SendSkullKeyLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, ReceiveSkullKey);
        }

        private void ReceiveSkullKey()
        {
            Game1.player.hasSkullKey = true;
            Game1.player.addQuest(19);
        }

        private LetterCustomAttachment SendRustyKeyLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, ReceiveRustyKey);
        }

        private void ReceiveRustyKey()
        {
            Game1.player.hasRustyKey = true;
        }

        private LetterCustomAttachment SendGoldenScytheLetter(ReceivedItem receivedItem)
        {
            Game1.player.mailReceived.Add("gotGoldenScythe");
            return new LetterCustomAttachment(receivedItem, SendGoldenScytheLetter);
        }

        private void SendGoldenScytheLetter()
        {
            Game1.playSound("parry");
            var goldenScythe = new MeleeWeapon(53);
            Game1.player.holdUpItemThenMessage(goldenScythe);
            Game1.player.addItemByMenuIfNecessary(goldenScythe);
        }

        private LetterCustomAttachment SendProgressiveAxeLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, () => ReceiveProgressiveTool("Axe"));
        }

        private LetterCustomAttachment SendProgressivePickaxeLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, () => ReceiveProgressiveTool("Pickaxe"));
        }

        private LetterCustomAttachment SendProgressiveHoeLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, () => ReceiveProgressiveTool("Hoe"));
        }

        private LetterCustomAttachment SendProgressiveWateringCanLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, () => ReceiveProgressiveTool("Watering Can"));
        }

        private LetterCustomAttachment SendProgressiveTrashCanLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, () => ReceiveProgressiveTool("Trash Can"));
        }

        private LetterCustomAttachment SendProgressiveFishingRodLetter(ReceivedItem receivedItem)
        {
            return new LetterCustomAttachment(receivedItem, () => GetFishingRodOfLevel());
        }

        private void GetFishingRodOfLevel()
        {
            var numberOfPreviousFishingRodLetters = _mail.OpenedMailsStartingWithKey(PROGRESSIVE_FISHING_ROD_AP_NAME);
            var itemToAdd = new FishingRod(numberOfPreviousFishingRodLetters);

            Game1.player.holdUpItemThenMessage(itemToAdd);
            Game1.player.addItemByMenuIfNecessary(itemToAdd);
        }

        private void ReceiveProgressiveTool(string toolGenericName)
        {
            if (toolGenericName.Contains("Trash Can"))
            {
                ReceiveTrashCanUpgrade();
                return;
            }
            
            var upgradedTool = UpgradeToolInEntireWorld(toolGenericName);

            if (upgradedTool == null)
            {
                throw new Exception($"Could not find a tool of type {toolGenericName} in this entire world");
            }

            Game1.player.holdUpItemThenMessage(upgradedTool);
        }

        private static Tool UpgradeToolInEntireWorld(string toolGenericName)
        {
            var player = Game1.player;
            foreach (var playerItem in player.Items)
            {
                if (playerItem is not Tool toolToUpgrade || !toolToUpgrade.Name.Contains(toolGenericName))
                {
                    continue;
                }

                toolToUpgrade.UpgradeLevel++;
                return toolToUpgrade;
            }

            foreach (var gameLocation in Game1.locations)
            {
                foreach (var (tile, gameObject) in gameLocation.Objects.Pairs)
                {
                    if (gameObject is not Chest chest)
                    {
                        continue;
                    }

                    foreach (var chestItem in chest.items)
                    {
                        if (chestItem is not Tool toolToUpgrade || !toolToUpgrade.Name.Contains(toolGenericName))
                        {
                            continue;
                        }

                        toolToUpgrade.UpgradeLevel++;
                        return toolToUpgrade;
                    }
                }
            }

            return null;
        }

        private static void ReceiveTrashCanUpgrade()
        {
            Game1.player.trashCanLevel++;
            var trashCanToHoldUp = new GenericTool("Trash Can",
                Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description",
                    ((Game1.player.trashCanLevel * 15).ToString() ?? "")), Game1.player.trashCanLevel,
                12 + Game1.player.trashCanLevel, 12 + Game1.player.trashCanLevel);
            trashCanToHoldUp.upgradeLevel.Value = Game1.player.trashCanLevel;
            Game1.player.holdUpItemThenMessage(trashCanToHoldUp);
        }
    }
}
