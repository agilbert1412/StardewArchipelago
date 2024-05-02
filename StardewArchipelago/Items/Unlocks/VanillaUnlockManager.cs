﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Locations;

namespace StardewArchipelago.Items.Unlocks
{
    public class VanillaUnlockManager : IUnlockManager
    {
        public const string PROGRESSIVE_TOOL_AP_PREFIX = "Progressive ";
        public const string PROGRESSIVE_MINE_ELEVATOR = "Progressive Mine Elevator";
        public const string PROGRESSIVE_FISHING_ROD = "Progressive Fishing Rod";
        public const string RETURN_SCEPTER = "Return Scepter";
        public const string GOLDEN_SCYTHE = "Golden Scythe";
        public const string PROGRESSIVE_SCYTHE = "Progressive Scythe";
        public const string BEACH_BRIDGE = "Beach Bridge";
        public const string FRUIT_BATS = "Fruit Bats";
        public const string MUSHROOM_BOXES = "Mushroom Boxes";
        public const string SPECIAL_ORDER_BOARD_AP_NAME = "Special Order Board";
        public const string QI_WALNUT_ROOM = "Qi Walnut Room";
        public const string PIERRE_STOCKLIST = "Pierre's Missing Stocklist";
        public const string ISLAND_FARMHOUSE = "Island Farmhouse";
        public const string ISLAND_MAILBOX = "Island Mailbox";
        public const string TREEHOUSE = "Treehouse";
        public const string PROGRESSIVE_WEAPON = "Progressive Weapon";
        public const string PROGRESSIVE_SWORD = "Progressive Sword";
        public const string PROGRESSIVE_CLUB = "Progressive Club";
        public const string PROGRESSIVE_DAGGER = "Progressive Dagger";
        public const string PROGRESSIVE_BOOTS = "Progressive Footwear";
        public const string PROGRESSIVE_SLINGSHOT = "Progressive Slingshot";

        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public VanillaUnlockManager(ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            RegisterCommunityCenterRepairs();
            RegisterRaccoons();
            RegisterPlayerSkills();
            RegisterPlayerImprovement();
            RegisterProgressiveTools();
            RegisterMineElevators();
            RegisterUniqueItems();
            RegisterIsolatedEventsItems();
            RegisterGingerIslandRepairs();
            RegisterSpecialItems();
            RegisterEquipment();
        }

        public bool IsUnlock(string unlockName)
        {
            return _unlockables.ContainsKey(unlockName);
        }

        public LetterAttachment PerformUnlockAsLetter(ReceivedItem unlock)
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
        }

        private void RegisterRaccoons()
        {
            _unlockables.Add(APItem.PROGRESSIVE_RACCOON, SendProgressiveRaccoon);
        }

        private void RegisterPlayerImprovement()
        {
            _unlockables.Add("Progressive Backpack", SendProgressiveBackpackLetter);
            _unlockables.Add("Stardrop", SendStardropLetter);
            _unlockables.Add("Dwarvish Translation Guide", SendDwarvishTranslationGuideLetter);
            _unlockables.Add("Skull Key", SendSkullKeyLetter);
            _unlockables.Add("Rusty Key", SendRustyKeyLetter);

            _unlockables.Add("Club Card", SendClubCardLetter);
            _unlockables.Add("Magnifying Glass", SendMagnifyingGlassLetter);
            _unlockables.Add("Iridium Snake Milk", SendIridiumSnakeMilkLetter);
            _unlockables.Add("Dark Talisman", SendDarkTalismanLetter);
            _unlockables.Add("Key To The Town", SendKeyToTheTownLetter);
        }

        private void RegisterPlayerSkills()
        {
            _unlockables.Add($"{Skill.Farming} Level", SendProgressiveFarmingLevel);
            _unlockables.Add($"{Skill.Fishing} Level", SendProgressiveFishingLevel);
            _unlockables.Add($"{Skill.Foraging} Level", SendProgressiveForagingLevel);
            _unlockables.Add($"{Skill.Mining} Level", SendProgressiveMiningLevel);
            _unlockables.Add($"{Skill.Combat} Level", SendProgressiveCombatLevel);
            RegisterPlayerMasteries();
        }

        private void RegisterPlayerMasteries()
        {
            _unlockables.Add($"{Skill.Farming} Mastery", SendFarmingMastery);
            _unlockables.Add($"{Skill.Fishing} Mastery", SendFishingMastery);
            _unlockables.Add($"{Skill.Foraging} Mastery", SendForagingMastery);
            _unlockables.Add($"{Skill.Mining} Mastery", SendMiningMastery);
            _unlockables.Add($"{Skill.Combat} Mastery", SendCombatMastery);
        }

        private void RegisterProgressiveTools()
        {
            _unlockables.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Axe", SendProgressiveAxeLetter);
            _unlockables.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Pickaxe", SendProgressivePickaxeLetter);
            _unlockables.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Hoe", SendProgressiveHoeLetter);
            _unlockables.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Watering Can", SendProgressiveWateringCanLetter);
            _unlockables.Add($"{PROGRESSIVE_TOOL_AP_PREFIX}Trash Can", SendProgressiveTrashCanLetter);
            _unlockables.Add(PROGRESSIVE_FISHING_ROD, SendProgressiveFishingRodLetter);
            _unlockables.Add(RETURN_SCEPTER, SendReturnScepterLetter);
        }

        private void RegisterUniqueItems()
        {
            _unlockables.Add(GOLDEN_SCYTHE, SendGoldenScytheLetter); // Deprecated, but kept in case of start inventory
            _unlockables.Add(PROGRESSIVE_SCYTHE, SendProgressiveScytheLetter);
            _unlockables.Add(PIERRE_STOCKLIST, SendPierreStocklistLetter);
        }

        private void RegisterIsolatedEventsItems()
        {
            _unlockables.Add(BEACH_BRIDGE, SendBeachBridgeLetter);
            _unlockables.Add(FRUIT_BATS, SendFruitBatsLetter);
            _unlockables.Add(MUSHROOM_BOXES, SendMushroomBoxesLetter);
        }

        private void RegisterGingerIslandRepairs()
        {
            _unlockables.Add("Boat Repair", RepairBoat);
            _unlockables.Add("Island North Turtle", GetLeoTrustAndRemoveNorthernTurtle);
            _unlockables.Add("Island West Turtle", RemoveWesternTurtle);
            _unlockables.Add("Dig Site Bridge", RepairDigSiteBridge);
            _unlockables.Add("Island Trader", RestoreIslandTrader);
            _unlockables.Add("Island Resort", RepairResort);
            _unlockables.Add("Farm Obelisk", CreateFarmObelisk);
            _unlockables.Add(ISLAND_MAILBOX, RepairIslandMailbox);
            _unlockables.Add(ISLAND_FARMHOUSE, RepairIslandFarmhouse);
            _unlockables.Add("Parrot Express", RepairParrotExpress);
            _unlockables.Add("Volcano Bridge", ConstructVolcanoBridge);
            _unlockables.Add("Volcano Exit Shortcut", OpenVolcanoExitShortcut);
            _unlockables.Add("Open Professor Snail Cave", OpenProfessorSnailCave);
            _unlockables.Add(TREEHOUSE, ConstructTreeHouse);
        }

        private void RegisterSpecialItems()
        {
            _unlockables.Add("Ugly Baby", GetNewBabyLetter);
            _unlockables.Add("Cute Baby", GetNewBabyLetter);
        }

        private void RegisterMineElevators()
        {
            _unlockables.Add(PROGRESSIVE_MINE_ELEVATOR, SendProgressiveMineElevatorLetter);
        }

        private void RegisterEquipment()
        {
            _unlockables.Add(PROGRESSIVE_WEAPON, SendProgressiveWeaponLetter);
            _unlockables.Add(PROGRESSIVE_SWORD, SendProgressiveSwordLetter);
            _unlockables.Add(PROGRESSIVE_CLUB, SendProgressiveClubLetter);
            _unlockables.Add(PROGRESSIVE_DAGGER, SendProgressiveDaggerLetter);
            _unlockables.Add(PROGRESSIVE_BOOTS, SendProgressiveBootsLetter);
            _unlockables.Add(PROGRESSIVE_SLINGSHOT, SendProgressiveSlingshotLetter);
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

        private LetterAttachment SendProgressiveRaccoon(ReceivedItem receivedItem)
        {
            if (!Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
            {
                Game1.MasterPlayer.mailReceived.Add("raccoonTreeFallen");
                return new LetterInformationAttachment(receivedItem);
            }

            if (_archipelago.SlotData.QuestLocations.StoryQuestsEnabled && !Game1.MasterPlayer.mailReceived.Contains("raccoonMovedIn"))
            {
                Game1.MasterPlayer.mailReceived.Add("raccoonMovedIn");
                var forest = Game1.getLocationFromName("Forest");
                Forest.fixStump(forest);
                if (_locationChecker.IsLocationMissing("The Giant Stump") && !Game1.player.hasQuest("134"))
                {
                    Game1.player.addQuest("134");
                    Game1.player.mailReceived.Add("checkedRaccoonStump");
                }
                return new LetterInformationAttachment(receivedItem);
            }

            ++Game1.netWorldState.Value.TimesFedRaccoons;
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterVanillaAttachment RepairBoat(ReceivedItem receivedItem)
        {
            var vanillaMails = new[] { "willyBoatFixed", "willyBackRoomInvitation" };
            return new LetterVanillaAttachment(receivedItem, vanillaMails, true);
        }

        private LetterActionAttachment GetLeoTrustAndRemoveNorthernTurtle(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "Hut");
        }

        private LetterActionAttachment RemoveWesternTurtle(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "Turtle");
        }

        private LetterActionAttachment RepairResort(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "Resort");
        }

        private LetterActionAttachment RepairDigSiteBridge(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "Bridge");
        }

        private LetterActionAttachment RestoreIslandTrader(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "Trader");
        }

        private LetterActionAttachment CreateFarmObelisk(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "Obelisk");
        }

        private LetterActionAttachment RepairIslandMailbox(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "House_Mailbox");
        }

        private LetterActionAttachment RepairIslandFarmhouse(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "House");
        }

        private LetterActionAttachment RepairParrotExpress(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "ParrotPlatforms");
        }

        private LetterActionAttachment ConstructVolcanoBridge(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "VolcanoBridge");
        }

        private LetterActionAttachment OpenVolcanoExitShortcut(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "VolcanoShortcutOut");
        }

        private LetterActionAttachment OpenProfessorSnailCave(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, "ProfessorSnailCave");
        }

        private LetterActionAttachment ConstructTreeHouse(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IslandUnlock, TREEHOUSE);
        }

        private LetterActionAttachment GetNewBabyLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.SpawnBaby);
        }

        private LetterActionAttachment SendProgressiveBackpackLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.Backpack);
        }

        private LetterAttachment SendProgressiveMineElevatorLetter(ReceivedItem receivedItem)
        {
            return new LetterInformationAttachment(receivedItem);
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

        private LetterActionAttachment SendClubCardLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ClubCard);
        }

        private LetterActionAttachment SendMagnifyingGlassLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.MagnifyingGlass);
        }

        private LetterActionAttachment SendIridiumSnakeMilkLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.IridiumSnakeMilk);
        }

        private LetterActionAttachment SendDarkTalismanLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.DarkTalisman);
        }

        private LetterActionAttachment SendKeyToTheTownLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.KeyToTheTown);
        }

        private LetterActionAttachment SendGoldenScytheLetter(ReceivedItem receivedItem)
        {
            if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
            {
                Game1.player.mailReceived.Add("gotGoldenScythe");
            }
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GoldenScythe);
        }

        private LetterActionAttachment SendProgressiveScytheLetter(ReceivedItem receivedItem)
        {
            if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
            {
                Game1.player.mailReceived.Add("gotGoldenScythe");
            }
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ProgressiveScythe);
        }

        private LetterActionAttachment SendPierreStocklistLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.PierreStocklist);
        }

        private LetterActionAttachment SendBeachBridgeLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.BeachBridge);
        }

        private LetterActionAttachment SendFruitBatsLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.FruitBats);
        }

        private LetterActionAttachment SendMushroomBoxesLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.MushroomBoxes);
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

        private LetterActionAttachment SendReturnScepterLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.ReturnScepter);
        }

        private LetterAttachment SendProgressiveFarmingLevel(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Farming;
            var apItem = $"{skill} Level";
            var receivedLevels = _archipelago.GetReceivedItemCount(apItem);
            foreach (var farmer in Game1.getAllFarmers())
            {
                var currentLevel = farmer.farmingLevel.Value;
                var newLevel = currentLevel + 1;
                newLevel = Math.Max(0, Math.Min(receivedLevels, newLevel));
                if (newLevel <= currentLevel)
                {
                    continue;
                }

                GiveExperienceToNextLevel(farmer, skill);
                farmer.farmingLevel.Value = newLevel;
                farmer.newLevels.Add(new Point((int)skill, newLevel));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveFishingLevel(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Fishing;
            var apItem = $"{skill} Level";
            var receivedLevels = _archipelago.GetReceivedItemCount(apItem);
            foreach (var farmer in Game1.getAllFarmers())
            {
                var currentLevel = farmer.fishingLevel.Value;
                var newLevel = currentLevel + 1;
                newLevel = Math.Max(0, Math.Min(receivedLevels, newLevel));
                if (newLevel <= currentLevel)
                {
                    continue;
                }

                GiveExperienceToNextLevel(farmer, skill);
                farmer.fishingLevel.Value = newLevel;
                farmer.newLevels.Add(new Point((int)skill, newLevel));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveForagingLevel(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Foraging;
            var apItem = $"{skill} Level";
            var receivedLevels = _archipelago.GetReceivedItemCount(apItem);
            foreach (var farmer in Game1.getAllFarmers())
            {
                var currentLevel = farmer.foragingLevel.Value;
                var newLevel = currentLevel + 1;
                newLevel = Math.Max(0, Math.Min(receivedLevels, newLevel));
                if (newLevel <= currentLevel)
                {
                    continue;
                }

                GiveExperienceToNextLevel(farmer, skill);
                farmer.foragingLevel.Value = newLevel;
                farmer.newLevels.Add(new Point((int)skill, newLevel));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveMiningLevel(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Mining;
            var apItem = $"{skill} Level";
            var receivedLevels = _archipelago.GetReceivedItemCount(apItem);
            foreach (var farmer in Game1.getAllFarmers())
            {
                var currentLevel = farmer.miningLevel.Value;
                var newLevel = currentLevel + 1;
                newLevel = Math.Max(0, Math.Min(receivedLevels, newLevel));
                if (newLevel <= currentLevel)
                {
                    continue;
                }

                GiveExperienceToNextLevel(farmer, skill);
                farmer.miningLevel.Value = newLevel;
                farmer.newLevels.Add(new Point((int)skill, newLevel));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveCombatLevel(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Combat;
            var apItem = $"{skill} Level";
            var receivedLevels = _archipelago.GetReceivedItemCount(apItem);
            foreach (var farmer in Game1.getAllFarmers())
            {
                var currentLevel = farmer.combatLevel.Value;
                var newLevel = currentLevel + 1;
                newLevel = Math.Max(0, Math.Min(receivedLevels, newLevel));
                if (newLevel <= currentLevel)
                {
                    continue;
                }

                GiveExperienceToNextLevel(farmer, skill);
                farmer.combatLevel.Value = newLevel;
                farmer.newLevels.Add(new Point((int)skill, newLevel));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendFarmingMastery(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Farming;
            foreach (var farmer in Game1.getAllFarmers())
            {
                AddCraftingRecipes(farmer, new[] { "Statue Of Blessings" });
                GiveItemsToFarmer(farmer, "(W)66");
                farmer.stats.Increment(StatKeys.Mastery((int)skill), 1);
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendForagingMastery(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Foraging;
            foreach (var farmer in Game1.getAllFarmers())
            {
                AddCraftingRecipes(farmer, new[] { "Mystic Tree Seed", "Treasure Totem" });
                farmer.stats.Increment(StatKeys.Mastery((int)skill), 1);
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendFishingMastery(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Fishing;
            foreach (var farmer in Game1.getAllFarmers())
            {
                AddCraftingRecipes(farmer, new[] { "Challenge Bait" });
                GiveItemsToFarmer(farmer, "(T)AdvancedIridiumRod");
                farmer.stats.Increment(StatKeys.Mastery((int)skill), 1);
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendMiningMastery(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Mining;
            foreach (var farmer in Game1.getAllFarmers())
            {
                AddCraftingRecipes(farmer, new[] { "Statue Of The Dwarf King", "Heavy Furnace" });
                farmer.stats.Increment(StatKeys.Mastery((int)skill), 1);
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendCombatMastery(ReceivedItem receivedItem)
        {
            const Skill skill = Skill.Combat;
            foreach (var farmer in Game1.getAllFarmers())
            {
                AddCraftingRecipes(farmer, new[] { "Anvil", "Mini-Forge" });
                Game1.player.stats.Set("trinketSlots", 1);
                farmer.stats.Increment(StatKeys.Mastery((int)skill), 1);
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private void AddCraftingRecipes(Farmer farmer, IEnumerable<string> recipes)
        {
            foreach (var recipe in recipes)
            {
                farmer.craftingRecipes.Add(recipe, 0);
            }
        }

        private void GiveItemsToFarmer(Farmer farmer, string itemId)
        {
            if (_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            var item = ItemRegistry.Create(itemId);
            if (!farmer.addItemToInventoryBool(item))
            {
                Game1.createItemDebris(item, Game1.player.getStandingPosition(), 2);
            }
        }

        public void GiveExperienceToNextLevel(Farmer farmer, Skill skill)
        {
            var experienceForLevelUp = farmer.GetExperienceToNextLevel(skill);
            farmer.AddExperience(skill, experienceForLevelUp);
        }

        private LetterActionAttachment SendProgressiveWeaponLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveWeapon);
        }

        private LetterActionAttachment SendProgressiveSwordLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveSword);
        }

        private LetterActionAttachment SendProgressiveClubLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveClub);
        }

        private LetterActionAttachment SendProgressiveDaggerLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveDagger);
        }

        private LetterActionAttachment SendProgressiveBootsLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveProgressiveBoots);
        }

        private LetterActionAttachment SendProgressiveSlingshotLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveProgressiveSlingshot);
        }
    }
}
