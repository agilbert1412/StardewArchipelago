using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CarpenterInjections
    {
        public const string BUILDING_PROGRESSIVE_HOUSE = "Progressive House";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public BlueprintEntry(int index, string id, BuildingData data, string skinId)
        public static bool BlueprintEntryConstructor_IfFreeMakeTheIdCorrect_Prefix(CarpenterMenu.BlueprintEntry __instance, int index, ref string id, BuildingData data, string skinId)
        {
            try
            {
                id = RemoveFreePrefix(id);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BlueprintEntryConstructor_IfFreeMakeTheIdCorrect_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static string RemoveFreePrefix(string id)
        {
            const string freePrefix = "Free ";
            if (id.StartsWith(freePrefix))
            {
                return id.Substring(freePrefix.Length);
            }

            return id;
        }

        // public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey)
        public static bool CreateQuestionDialogue_CarpenterDialogOptions_Prefix(GameLocation __instance, string question, Response[] answerChoices, string dialogKey)
        {
            try
            {
                if (dialogKey != "carpenter" && (dialogKey != "telephone" || answerChoices.All(x => x.responseKey != "Carpenter_BuildingCost")))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var receivedHouseUpgrades = _archipelago.GetReceivedItemCount(BUILDING_PROGRESSIVE_HOUSE);
                if (Game1.player.HouseUpgradeLevel >= receivedHouseUpgrades)
                {
                    answerChoices = answerChoices.Where(x => x.responseKey != "Upgrade").ToArray();
                }

                var canConstructAnyBuilding = false;
                foreach (var (_, buildingData) in Game1.buildingData)
                {
                    if (buildingData.Builder.Equals("Robin", StringComparison.InvariantCultureIgnoreCase) && GameStateQuery.CheckConditions(buildingData.BuildCondition))
                    {
                        canConstructAnyBuilding = true;
                        break;
                    }
                }

                if (!canConstructAnyBuilding)
                {
                    answerChoices = answerChoices.Where(x => x.responseKey != "Construct" && x.responseKey != "Carpenter_BuildingCost").ToArray();
                }

                if (_archipelago.SlotData.IncludeEndgameLocations)
                {
                    answerChoices = answerChoices.Where(x => x.responseKey != "CommunityUpgrade").ToArray();
                }

                __instance.lastQuestionKey = dialogKey;
                Game1.drawObjectQuestionDialogue(question, answerChoices);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CreateQuestionDialogue_CarpenterDialogOptions_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool HouseUpgradeOffer_OfferCheaperUpgrade_Prefix(GameLocation __instance)
        {
            try
            {
                var text = "";
                var priceMultiplier = _archipelago.SlotData.BuildingPriceMultiplier;
                if (Math.Abs(priceMultiplier - 1.0) < 0.001)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                switch (Game1.player.HouseUpgradeLevel)
                {
                    case 0:
                        text = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1"));
                        text = text.Replace("10,000", $"{(int)Math.Round(10000 * priceMultiplier)}")
                            .Replace("450", $"{Math.Max(1, (int)Math.Round(450 * priceMultiplier))}");
                        break;
                    case 1:
                        var priceGold = (int)Math.Round(50000 * priceMultiplier);
                        var priceWood = Math.Max(1, (int)Math.Round(150 * priceMultiplier));
                        text = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2", priceGold, priceWood));
                        break;
                    case 2:
                        text = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3"));
                        text = text.Replace("100,000", $"{Math.Max(1, (int)Math.Round(100000 * priceMultiplier))}");
                        break;
                }

                __instance.createQuestionDialogue(text, __instance.createYesNoResponses(), "upgrade");
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HouseUpgradeOffer_OfferCheaperUpgrade_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool HouseUpgradeOffer_OfferFreeUpgrade_Prefix(GameLocation __instance)
        {
            try
            {
                var receivedHouseUpgrades = _archipelago.GetReceivedItemCount(BUILDING_PROGRESSIVE_HOUSE);
                if (Game1.player.HouseUpgradeLevel >= receivedHouseUpgrades)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var houseUpgradeReceivedFromAP = _archipelago.GetAllReceivedItems()
                    .Where(x => x.ItemName == BUILDING_PROGRESSIVE_HOUSE).ToArray();

                var upgrade1Dialogue = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1"));
                var upgrade2Dialogue = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2"));
                var upgrade3Dialogue = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3"));

                var apUpgradeDialogue = "I see here someone has already funded an upgrade for your house.";
                var startNowQuestion = "^Do you want me to start working on that right away?";

                switch (Game1.player.HouseUpgradeLevel)
                {
                    case 0:
                        apUpgradeDialogue +=
                            $"^{houseUpgradeReceivedFromAP[0].PlayerName} paid for a Kitchen and a Master Bedroom.{startNowQuestion}";
                        __instance.createQuestionDialogue(apUpgradeDialogue, __instance.createYesNoResponses(), "upgrade");
                        break;
                    case 1:
                        var bonusDialogue = GetChildRoomDialogue();
                        apUpgradeDialogue +=
                            $"^{houseUpgradeReceivedFromAP[1].PlayerName} paid for an entire second floor, including one extra bedroom. {bonusDialogue}{startNowQuestion}";
                        __instance.createQuestionDialogue(apUpgradeDialogue, __instance.createYesNoResponses(), "upgrade");
                        break;
                    case 2:
                        apUpgradeDialogue +=
                            $"^{houseUpgradeReceivedFromAP[2].PlayerName} paid for a Cellar and 33 Casks.^I hope I'll get to taste an aged Goat Cheese!{startNowQuestion}";
                        __instance.createQuestionDialogue(apUpgradeDialogue, __instance.createYesNoResponses(), "upgrade");
                        break;
                }
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HouseUpgradeOffer_OfferFreeUpgrade_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static string GetChildRoomDialogue()
        {
            var player = Game1.player;
            if (!player.isMarriedOrRoommates() && !player.isEngaged())
            {
                var who = player.friendshipData.Keys.Where(name => player.friendshipData[name].IsDating()).MaxBy(name => player.friendshipData[name].Points);

                if (who == null)
                {
                    return "If you ever start something with one of the people in town, you'll be ready!";
                }

                return $"Are you planning on popping the question to {who} soon?";
            }

            var frienshipNature = player.friendshipData[player.spouse];
            var spouse = Game1.getCharacterFromName(player.spouse);


            if (player.isEngaged())
            {
                return $"You and {spouse.Name} will be ready to start your new lives!";
            }

            if (frienshipNature.RoommateMarriage)
            {
                return $"But you probably won't need the kid beds";
            }
            else
            {
                var spouseIsMale = spouse.Gender == 0;
                if (player.IsMale == spouseIsMale)
                {
                    return $"You and {spouse.Name} will be able to adopt a baby!";
                }
                else
                {
                    if (player.IsMale)
                    {
                        return $"Is {spouse.Name} expecting a baby?";
                    }
                    else
                    {
                        return $"Are you expecting a baby with {spouse.Name}?";
                    }
                }
            }
        }

        // private void houseUpgradeAccept()
        public static bool HouseUpgradeAccept_FreeFromAP_Prefix(GameLocation __instance)
        {
            try
            {
                var receivedHouseUpgrades = _archipelago.GetReceivedItemCount(BUILDING_PROGRESSIVE_HOUSE);
                if (Game1.player.HouseUpgradeLevel >= receivedHouseUpgrades)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                Game1.player.daysUntilHouseUpgrade.Value = 3;
                Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
                Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HouseUpgradeAccept_FreeFromAP_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool HouseUpgradeAccept_CheaperInAP_Prefix(GameLocation __instance)
        {
            try
            {
                var priceMultiplier = _archipelago.SlotData.BuildingPriceMultiplier;
                if (Math.Abs(priceMultiplier - 1.0) < 0.001)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                switch (Game1.player.HouseUpgradeLevel)
                {
                    case 0:
                        var price1 = (int)(10000 * priceMultiplier);
                        var woodAmount = Math.Max(1, (int)(450 * priceMultiplier));
                        if (Game1.player.Money >= price1 && Game1.player.Items.ContainsId("388", woodAmount))
                        {
                            Game1.player.daysUntilHouseUpgrade.Value = 3;
                            Game1.player.Money -= price1;
                            Game1.player.Items.ReduceId("388", woodAmount);
                            Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
                            Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                            break;
                        }
                        if (Game1.player.Money < price1)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                            break;
                        }
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood", (object)450));
                        break;
                    case 1:
                        var price2 = (int)(50000 * priceMultiplier);
                        var hardwoodAmount = Math.Max(1, (int)(150 * priceMultiplier));
                        if (Game1.player.Money >= price2 && Game1.player.Items.ContainsId("709", hardwoodAmount))
                        {
                            Game1.player.daysUntilHouseUpgrade.Value = 3;
                            Game1.player.Money -= price2;
                            Game1.player.Items.ReduceId("709", hardwoodAmount);
                            Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
                            Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                            break;
                        }
                        if (Game1.player.Money < price2)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                            break;
                        }
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughHardwood", (object)100));
                        break;
                    case 2:
                        var price3 = (int)(100000 * priceMultiplier);
                        if (Game1.player.Money >= price3)
                        {
                            Game1.player.daysUntilHouseUpgrade.Value = 3;
                            Game1.player.Money -= price3;
                            Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
                            Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                            break;
                        }
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                        break;
                }
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HouseUpgradeAccept_CheaperInAP_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
