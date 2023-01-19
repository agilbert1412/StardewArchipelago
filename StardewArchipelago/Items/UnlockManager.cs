using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations.CodeInjections;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class UnlockManager
    {
        public const string PROGRESSIVE_MINE_ELEVATOR_AP_NAME = "Progressive Mine Elevator";
        public const string PROGRESSIVE_FISHING_ROD_AP_NAME = "Progressive Fishing Rod";
        public const string GOLDEN_SCYTHE_AP_NAME = "Golden Scythe";
        public const string BEACH_BRIDGE_AP_NAME = "Beach Bridge Repair";
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public UnlockManager()
        {
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            RegisterCommunityCenterRepairs();
            RegisterPlayerSkills();
            RegisterPlayerImprovement();
            RegisterProgressiveTools();
            RegisterMineElevators();
            RegisterUniqueItems();
            RegisterIsolatedEventsItems();
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

        private void RegisterPlayerSkills()
        {
            _unlockables.Add($"Progressive {Skill.Farming} Level", SendProgressiveFarmingLevel);
            _unlockables.Add($"Progressive {Skill.Fishing} Level", SendProgressiveFishingLevel);
            _unlockables.Add($"Progressive {Skill.Foraging} Level", SendProgressiveForagingLevel);
            _unlockables.Add($"Progressive {Skill.Mining} Level", SendProgressiveMiningLevel);
            _unlockables.Add($"Progressive {Skill.Combat} Level", SendProgressiveCombatLevel);
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

        private void RegisterIsolatedEventsItems()
        {
            _unlockables.Add(BEACH_BRIDGE_AP_NAME, SendBeachBridgeLetter);
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

        private LetterActionAttachment SendBeachBridgeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.BeachBridge);
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

        private LetterAttachment SendProgressiveFarmingLevel(ReceivedItem receivedItem)
        {
            const int whichSkill = (int)Skill.Farming;
            foreach (var farmer in Game1.getAllFarmers())
            {
                GiveExperienceToNextLevel(farmer, whichSkill);
                farmer.FarmingLevel = farmer.farmingLevel.Value + 1;
                farmer.newLevels.Add(new Point(whichSkill, farmer.farmingLevel.Value));
            }
            return new LetterAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveFishingLevel(ReceivedItem receivedItem)
        {
            const int whichSkill = (int)Skill.Fishing;
            foreach (var farmer in Game1.getAllFarmers())
            {
                GiveExperienceToNextLevel(farmer, whichSkill);
                farmer.FishingLevel = farmer.fishingLevel.Value + 1;
                farmer.newLevels.Add(new Point(whichSkill, farmer.fishingLevel.Value));
            }
            return new LetterAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveForagingLevel(ReceivedItem receivedItem)
        {
            const int whichSkill = (int)Skill.Foraging;
            foreach (var farmer in Game1.getAllFarmers())
            {
                GiveExperienceToNextLevel(farmer, whichSkill);
                farmer.ForagingLevel = farmer.foragingLevel.Value + 1;
                farmer.newLevels.Add(new Point(whichSkill, farmer.foragingLevel.Value));
            }
            return new LetterAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveMiningLevel(ReceivedItem receivedItem)
        {
            const int whichSkill = (int)Skill.Mining;
            foreach (var farmer in Game1.getAllFarmers())
            {
                GiveExperienceToNextLevel(farmer, whichSkill);
                farmer.MiningLevel = farmer.miningLevel.Value + 1;
                farmer.newLevels.Add(new Point(whichSkill, farmer.miningLevel.Value));
            }
            return new LetterAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveCombatLevel(ReceivedItem receivedItem)
        {
            const int whichSkill = (int)Skill.Combat;
            foreach (var farmer in Game1.getAllFarmers())
            {
                GiveExperienceToNextLevel(farmer, whichSkill);
                farmer.CombatLevel = farmer.combatLevel.Value + 1;
                farmer.newLevels.Add(new Point(whichSkill, farmer.combatLevel.Value));
            }
            return new LetterAttachment(receivedItem);
        }

        private void GiveExperienceToNextLevel(Farmer farmer, int whichSkill)
        {
            var experienceForLevelUp = GetExperienceToNextLevel(farmer, whichSkill);
            farmer.experiencePoints[whichSkill] += experienceForLevelUp;
        }

        private int GetExperienceToNextLevel(Farmer farmer, int skill)
        {
            switch (farmer.experiencePoints[skill])
            {
                case < 100:
                    return 100 - farmer.experiencePoints[skill];
                case < 380:
                    return 380 - farmer.experiencePoints[skill];
                case < 770:
                    return 770 - farmer.experiencePoints[skill];
                case < 1300:
                    return 1300 - farmer.experiencePoints[skill];
                case < 2150:
                    return 2150 - farmer.experiencePoints[skill];
                case < 3300:
                    return 3300 - farmer.experiencePoints[skill];
                case < 4800:
                    return 4800 - farmer.experiencePoints[skill];
                case < 6900:
                    return 6900 - farmer.experiencePoints[skill];
                case < 10000:
                    return 10000 - farmer.experiencePoints[skill];
                case < 15000:
                    return 15000 - farmer.experiencePoints[skill];
            }

            return 0;
        }
    }
}
