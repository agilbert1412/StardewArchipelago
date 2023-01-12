using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class UnlockManager
    {
        public const string PROGRESSIVE_MINE_ELEVATOR_AP_NAME = "Progressive Mine Elevator";
        public const string PROGRESSIVE_FISHING_ROD_AP_NAME = "Progressive Fishing Rod";
        public const string GOLDEN_SCYTHE_AP_NAME = "Golden Scythe";
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public UnlockManager()
        {
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            RegisterCommunityCenterRepairs();
            RegisterPlayerImprovement();
            RegisterProgressiveTools();
            RegisterMineElevators();
            RegisterUniqueItems();
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

        private LetterActionAttachment SendProgressiveBackpackLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.Backpack);
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

        private LetterActionAttachment SendDwarvishTranslationGuideLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.DwarvishTranslationGuide);
        }

        private LetterActionAttachment SendSkullKeyLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.SkullKey);
        }

        private LetterActionAttachment SendRustyKeyLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.RustyKey);
        }

        private LetterActionAttachment SendGoldenScytheLetter(ReceivedItem receivedItem)
        {
            Game1.player.mailReceived.Add("gotGoldenScythe");
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GoldenScythe);
        }

        private LetterActionAttachment SendProgressiveAxeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Axe");
        }

        private LetterActionAttachment SendProgressivePickaxeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Pickaxe");
        }

        private LetterActionAttachment SendProgressiveHoeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Hoe");
        }

        private LetterActionAttachment SendProgressiveWateringCanLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Watering Can");
        }

        private LetterActionAttachment SendProgressiveTrashCanLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveTool, "Trash Can");
        }

        private LetterActionAttachment SendProgressiveFishingRodLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.FishingRod);
        }
    }
}
