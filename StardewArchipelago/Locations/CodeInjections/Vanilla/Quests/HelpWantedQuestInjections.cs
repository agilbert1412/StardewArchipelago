using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Quests
{
    public static class HelpWantedQuestInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;
        private static ContentManager _englishContentManager;
        private static uint _rerollCount = 0;

        private static QuestLocations QuestLocations => _archipelago.SlotData.QuestLocations;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
            _englishContentManager = new ContentManager(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);
            _rerollCount = Game1.stats.DaysPlayed;
        }

        public static bool TryHandleQuestComplete(Quest quest, out bool runOriginal)
        {
            if (!quest.dailyQuest.Value)
            {
                runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                return false;
            }

            if (QuestLocations.HelpWantedNumber <= 0)
            {
                runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                return true;
            }

            // Item Delivery: __instance.dailyQuest == true and questType == 3 [Chance: 40 / 65]
            // Copper Ores: Daily True, Type 1
            // Slay Monsters: Daily True, Type 4
            // Catch fish: Daily True, Type 7
            var questLocationName = GetQuestLocationName(quest);
            var isArchipelago = !string.IsNullOrWhiteSpace(questLocationName);

            if (!isArchipelago)
            {
                runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                return true;
            }
            else
            {
                _locationChecker.AddCheckedLocation(questLocationName);
            }

            ++Game1.stats.QuestsCompleted;
            QuestInjections.OriginalQuestCompleteCode(quest);
            runOriginal = MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            return true;
        }

        public static string GetQuestLocationName(Quest quest)
        {
            switch (quest)
            {
                case FishingQuest fishingQuest:
                    var artOrPopulation = fishingQuest.target.Value == NPCNames.DEMETRIUS ? DailyQuest.POPULATION : DailyQuest.ART;
                    return $"{DailyQuest.HELP_WANTED_PREFIX}{DailyQuest.FISHING} {artOrPopulation} {Game1.season.ToString()}";
                case ItemDeliveryQuest itemDeliveryQuest:
                    return $"{DailyQuest.HELP_WANTED_PREFIX}{DailyQuest.ITEM_DELIVERY} {itemDeliveryQuest.target.Value}";
                case ResourceCollectionQuest resourceCollectionQuest:
                    var item = _itemManager.GetObjectById(resourceCollectionQuest.ItemId.Value);
                    return $"{DailyQuest.HELP_WANTED_PREFIX}{DailyQuest.GATHERING} {item.Name}";
                case SlayMonsterQuest slayMonsterQuest:
                    var monster = slayMonsterQuest.monsterName.Value;
                    monster = monster.Replace("Dust Spirit", "Dust Sprite");
                    return $"{DailyQuest.HELP_WANTED_PREFIX}{DailyQuest.SLAY_MONSTERS} {monster}";
                case SocializeQuest socializeQuest:
                    return $"{DailyQuest.HELP_WANTED_PREFIX}{DailyQuest.HELLO} 'Hello'";
            }

            return string.Empty;
        }

        // public static Quest getQuestOfTheDay()
        public static bool GetQuestOfTheDay_BalanceQuests_Prefix(ref Quest __result)
        {
            try
            {
                if (Game1.stats.DaysPlayed <= 1U)
                {
                    __result = null;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var todayRandom = CreateRerolledRandom();
                var weightedLocations = CreateWeightedMissingLocations();
                if (!weightedLocations.Any())
                {
                    __result = null;
                    var remainingHelpWanteds = QuestLocations.GetRemainingHelpWanteds(_locationChecker);
                    return remainingHelpWanteds.Any() ? MethodPrefix.DONT_RUN_ORIGINAL_METHOD : MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var chosenIndex = todayRandom.Next(0, weightedLocations.Count);
                var chosenLocation = weightedLocations[chosenIndex];
                var questType = QuestLocations.GetQuestType(chosenLocation);
                var extraInfo = QuestLocations.GetExtraInfo(questType, chosenLocation);
                switch (questType)
                {
                    case QuestType.ItemDelivery:
                        __result = CreateItemDeliveryQuest(extraInfo);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case QuestType.Fishing:
                        __result = CreateFishingQuest(extraInfo);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case QuestType.ResourceCollection:
                        __result = CreateResourceCollectionQuest(extraInfo);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case QuestType.Monster:
                        __result = CreateSlayMonsterQuest(extraInfo);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case QuestType.Socialize:
                        __result = CreateSocializeQuest(extraInfo);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    default:
                        throw new Exception($"Location {chosenLocation} generates an unknown quest type: {questType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetQuestOfTheDay_BalanceQuests_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static Quest CreateItemDeliveryQuest((string npcName, string itemName) extraInfo)
        {
            var deliveryQuest = new ItemDeliveryQuest();


            var initializationRandom = CreateRerolledRandom();

            var npcName = extraInfo.npcName;
            var item = _itemManager.GetObjectByName(extraInfo.itemName);
            var itemId = item.GetQualifiedId();
            var npc = Game1.getCharacterFromName(npcName);

            deliveryQuest.target.Value = npcName;
            deliveryQuest.questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ItemDeliveryQuestTitle", NPC.GetDisplayName(npcName));
            deliveryQuest.ItemId.Value = itemId;
            var itemObject = ItemRegistry.Create(itemId);
            deliveryQuest.moneyReward.Value = deliveryQuest.GetGoldRewardPerItem(itemObject);

            if (Game1.season != Season.Winter && initializationRandom.NextDouble() < 0.15)
            {
                switch (npcName)
                {
                    case "Demetrius":
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13311", "13314"), itemObject));
                        break;
                    case "Marnie":
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13317", "13320"), itemObject));
                        break;
                    case "Sebastian":
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13324", "13327"), itemObject));
                        break;
                    default:
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13299", "13300", "13301"));
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13302", "13303", "13304"), itemObject));
                        deliveryQuest.parts.Add(initializationRandom.Choose<string>("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13306", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13307", "",
                            "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13308"));
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc));
                        break;
                }
            }
            else
            {
                var descriptionElementArray1 = (DescriptionElement[])null;
                var descriptionElementArray2 = (DescriptionElement[])null;
                var descriptionElementArray3 = (DescriptionElement[])null;
                if ((itemObject is Object object3 ? object3.Type : null) == "Cooking" && npcName != "Wizard")
                {
                    if (initializationRandom.NextDouble() < 0.33)
                    {
                        var descriptionElementArray4 = new DescriptionElement[]
                        {
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341"),
                            null,
                            null,
                            null,
                            null,
                            null,
                            null
                        };
                        DescriptionElement descriptionElement1;
                        if (!(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156")))
                        {
                            descriptionElement1 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156"));
                        }
                        else if (!(Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")))
                        {
                            descriptionElement1 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346");
                        }
                        else
                        {
                            descriptionElement1 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157"));
                        }
                        descriptionElementArray4[6] = descriptionElement1;
                        descriptionElementArray4[7] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13349");
                        descriptionElementArray4[8] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13350");
                        descriptionElementArray4[9] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13351");
                        DescriptionElement descriptionElement2;
                        switch (Game1.season)
                        {
                            case Season.Summer:
                                descriptionElement2 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13355");
                                break;
                            case Season.Winter:
                                descriptionElement2 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13353");
                                break;
                            default:
                                descriptionElement2 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13356");
                                break;
                        }
                        descriptionElementArray4[10] = descriptionElement2;
                        descriptionElementArray4[11] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357");
                        var options = descriptionElementArray4;
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13333", "13334"), itemObject,
                            initializationRandom.ChooseFrom<DescriptionElement>(options)));
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc));
                    }
                    else
                    {
                        DescriptionElement descriptionElement;
                        switch (Game1.dayOfMonth % 7)
                        {
                            case 0:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3042");
                                break;
                            case 1:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3043");
                                break;
                            case 2:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3044");
                                break;
                            case 3:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3045");
                                break;
                            case 4:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3046");
                                break;
                            case 5:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3047");
                                break;
                            default:
                                descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3048");
                                break;
                        }
                        descriptionElementArray1 = new DescriptionElement[5]
                        {
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13360", itemObject),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13364", itemObject),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13367", itemObject),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13370", itemObject),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13373", descriptionElement, itemObject, npc)
                        };
                        descriptionElementArray2 = new DescriptionElement[5]
                        {
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                            new("")
                        };
                        descriptionElementArray3 = new DescriptionElement[5]
                        {
                            new(""),
                            new(""),
                            new(""),
                            new(""),
                            new("")
                        };
                    }
                    deliveryQuest.parts.Clear();
                    var index = initializationRandom.Next(descriptionElementArray1.Length);
                    deliveryQuest.parts.Add(descriptionElementArray1[index]);
                    deliveryQuest.parts.Add(descriptionElementArray2[index]);
                    deliveryQuest.parts.Add(descriptionElementArray3[index]);
                    if (npcName.Equals("Sebastian"))
                    {
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13378", "13381"), itemObject));
                    }
                }
                else if (initializationRandom.NextBool() && (itemObject is Object object2 ? (object2.Edibility > 0 ? 1 : 0) : 0) != 0)
                {
                    var descriptionElementArray5 = new DescriptionElement[1]
                    {
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13383", itemObject,
                            new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." +
                                                   initializationRandom.Choose<string>("13385", "13386", "13387", "13388", "13389", "13390", "13391", "13392", "13393", "13394", "13395", "13396")),
                            new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13400", itemObject))
                    };
                    var descriptionElementArray6 = new DescriptionElement[2]
                    {
                        new(initializationRandom.Choose<string>("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13398")),
                        new(initializationRandom.Choose<string>("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13402"))
                    };
                    var descriptionElementArray7 = new DescriptionElement[2]
                    {
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc)
                    };
                    if (initializationRandom.NextDouble() < 0.33)
                    {
                        var descriptionElementArray8 = new DescriptionElement[12]
                        {
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340"),
                            new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341"),
                            null,
                            null,
                            null,
                            null,
                            null,
                            null
                        };
                        DescriptionElement descriptionElement3;
                        if (!(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156")))
                        {
                            descriptionElement3 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156"));
                        }
                        else if (!(Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")))
                        {
                            descriptionElement3 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346");
                        }
                        else
                        {
                            descriptionElement3 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157"));
                        }
                        descriptionElementArray8[6] = descriptionElement3;
                        descriptionElementArray8[7] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13420");
                        descriptionElementArray8[8] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13421");
                        descriptionElementArray8[9] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13422");
                        DescriptionElement descriptionElement4;
                        switch (Game1.season)
                        {
                            case Season.Summer:
                                descriptionElement4 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13426");
                                break;
                            case Season.Winter:
                                descriptionElement4 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13424");
                                break;
                            default:
                                descriptionElement4 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13427");
                                break;
                        }
                        descriptionElementArray8[10] = descriptionElement4;
                        descriptionElementArray8[11] = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357");
                        var options = descriptionElementArray8;
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13333", "13334"), itemObject,
                            initializationRandom.ChooseFrom<DescriptionElement>(options)));
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc));
                    }
                    else
                    {
                        deliveryQuest.parts.Clear();
                        var index = initializationRandom.Next(descriptionElementArray5.Length);
                        deliveryQuest.parts.Add(descriptionElementArray5[index]);
                        deliveryQuest.parts.Add(descriptionElementArray6[index]);
                        deliveryQuest.parts.Add(descriptionElementArray7[index]);
                    }
                    switch (npcName)
                    {
                        case "Demetrius":
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13311", "13314"), itemObject));
                            break;
                        case "Marnie":
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13317", "13320"), itemObject));
                            break;
                        case "Harvey":
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13446", itemObject,
                                new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." +
                                                       initializationRandom.Choose<string>("13448", "13449", "13450", "13451", "13452", "13453", "13454", "13455", "13456", "13457", "13458", "13459"))));
                            break;
                        case "Gus":
                            if (initializationRandom.NextDouble() < 0.6)
                            {
                                deliveryQuest.parts.Clear();
                                deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13462", itemObject));
                                break;
                            }
                            break;
                    }
                }
                else if (initializationRandom.NextBool() && (itemObject is Object object1 ? (object1.Edibility >= 0 ? 1 : 0) : 0) == 0)
                {
                    deliveryQuest.parts.Clear();
                    deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13464", itemObject,
                        new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13465", "13466", "13467", "13468", "13469"))));
                    deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc));
                    if (npcName.Equals("Emily"))
                    {
                        deliveryQuest.parts.Clear();
                        deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13473", "13476"), itemObject));
                    }
                }
                else
                {
                    var descriptionElementArray9 = new DescriptionElement[9]
                    {
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13480", npc, itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13481", itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13485", itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13491", "13492"), itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13497", itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13500", itemObject,
                            new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." +
                                                   initializationRandom.Choose<string>("13502", "13503", "13504", "13505", "13506", "13507", "13508", "13509", "13510", "13511", "13512", "13513"))),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13518", npc, itemObject),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13520", "13523"), itemObject)
                    };
                    var descriptionElementArray10 = new DescriptionElement[9]
                    {
                        new(""),
                        new(initializationRandom.Choose<string>("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13482", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13483")),
                        new(initializationRandom.Choose<string>("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13487", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13488", "",
                            "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13489")),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13514", "13516")),
                        new(""),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc)
                    };
                    var descriptionElementArray11 = new DescriptionElement[9]
                    {
                        new(""),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new(""),
                        new(""),
                        new(""),
                        new("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", npc),
                        new(""),
                        new("")
                    };
                    deliveryQuest.parts.Clear();
                    var index = initializationRandom.Next(descriptionElementArray9.Length);
                    deliveryQuest.parts.Add(descriptionElementArray9[index]);
                    deliveryQuest.parts.Add(descriptionElementArray10[index]);
                    deliveryQuest.parts.Add(descriptionElementArray11[index]);
                }
            }
            deliveryQuest.dialogueparts.Clear();
            deliveryQuest.dialogueparts.Add(initializationRandom.NextBool(0.3) || npcName == "Evelyn"
                ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13526")
                : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13527", "13528")));
            var dialogueparts = deliveryQuest.dialogueparts;
            DescriptionElement descriptionElement5;
            if (!initializationRandom.NextBool(0.3))
            {
                descriptionElement5 = initializationRandom.NextBool()
                    ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13532")
                    : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13534", "13535", "13536"));
            }
            else
            {
                descriptionElement5 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13530", itemObject);
            }
            dialogueparts.Add(descriptionElement5);
            deliveryQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13538", "13539", "13540"));
            deliveryQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13542", "13543", "13544"));
            var str = npcName;
            if (str != null)
            {
                switch (str.Length)
                {
                    case 3:
                        if (str == "Sam")
                        {
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13568", "13571"), itemObject));
                            deliveryQuest.dialogueparts.Clear();
                            deliveryQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13577"));
                            break;
                        }
                        break;
                    case 4:
                        if (str == "Maru")
                        {
                            var flag = initializationRandom.NextBool();
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (flag ? "13580" : "13583"), itemObject));
                            deliveryQuest.dialogueparts.Clear();
                            deliveryQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (flag ? "13585" : "13587")));
                            break;
                        }
                        break;
                    case 5:
                        if (str == "Haley")
                        {
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13557", "13560"), itemObject));
                            deliveryQuest.dialogueparts.Clear();
                            deliveryQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13566");
                            break;
                        }
                        break;
                    case 6:
                        if (str == "Wizard")
                        {
                            deliveryQuest.parts.Clear();
                            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13546", "13548", "13551", "13553"), itemObject));
                            deliveryQuest.dialogueparts.Clear();
                            deliveryQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13555");
                            break;
                        }
                        break;
                    case 7:
                        switch (str[0])
                        {
                            case 'A':
                                if (str == "Abigail")
                                {
                                    var flag = initializationRandom.NextBool();
                                    deliveryQuest.parts.Clear();
                                    deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (flag ? "13590" : "13593"), itemObject));
                                    deliveryQuest.dialogueparts.Clear();
                                    deliveryQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (flag ? "13597" : "13599")));
                                    break;
                                }
                                break;
                            case 'E':
                                if (str == "Elliott")
                                {
                                    deliveryQuest.dialogueparts.Clear();
                                    deliveryQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13604", itemObject));
                                    break;
                                }
                                break;
                        }
                        break;
                    case 9:
                        if (str == "Sebastian")
                        {
                            deliveryQuest.dialogueparts.Clear();
                            deliveryQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13602");
                            break;
                        }
                        break;
                }
            }
            var descriptionElement6 = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + initializationRandom.Choose<string>("13608", "13610", "13612"), npc);
            deliveryQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", deliveryQuest.moneyReward.Value));
            deliveryQuest.parts.Add(descriptionElement6);
            deliveryQuest.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13614", npc, itemObject);


            return deliveryQuest;
        }

        private static Quest CreateFishingQuest((string nameInfo, string fishName) extraInfo)
        {
            var fishingQuest = new FishingQuest();

            var fish = _itemManager.GetObjectByName(extraInfo.fishName);
            var fishQualifiedId = fish.GetQualifiedId();
            var npcName = extraInfo.nameInfo.Contains(DailyQuest.ART) ? NPCNames.WILLY : NPCNames.DEMETRIUS;

            var initializationRandom = CreateRerolledRandom();
            fishingQuest.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");

            fishingQuest.ItemId.Value = fishQualifiedId;
            fishingQuest.target.Value = npcName;

            var fishObject = ItemRegistry.Create(fishQualifiedId);
            fishingQuest.numberToFish.Value = (int)Math.Ceiling(90.0 / Math.Max(1, GetGoldRewardPerItem(fishObject))) + Game1.player.FishingLevel / 5;
            fishingQuest.reward.Value = fishingQuest.numberToFish.Value * GetGoldRewardPerItem(fishObject);
            fishingQuest.parts.Clear();

            if (npcName == NPCNames.DEMETRIUS)
            {
                var isOctopus = fishQualifiedId == "(O)149";

                fishingQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13228", fishObject, fishingQuest.numberToFish.Value));
                fishingQuest.dialogueparts.Clear();
                fishingQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13231", fishObject, initializationRandom.Choose<DescriptionElement>(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13233"),
                    new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13234"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13235"),
                    new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13236", fishObject))));
                var objective = fishingQuest.objective;
                DescriptionElement descriptionElement;
                if (!isOctopus)
                {
                    descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, fishingQuest.numberToFish.Value, fishObject);
                }
                else
                {
                    descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", 0, fishingQuest.numberToFish.Value);
                }
                objective.Value = descriptionElement;
            }
            else
            {
                var isSquid = fishQualifiedId == "(O)151";

                var parts = fishingQuest.parts;
                DescriptionElement descriptionElement1;
                if (!isSquid)
                {
                    descriptionElement1 = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", fishingQuest.reward.Value, fishingQuest.numberToFish.Value, fishObject);
                }
                else
                {
                    descriptionElement1 = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", fishingQuest.reward.Value, fishingQuest.numberToFish.Value, new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13253"));
                }
                parts.Add(descriptionElement1);
                fishingQuest.dialogueparts.Clear();
                fishingQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13256", fishObject));
                fishingQuest.dialogueparts.Add(initializationRandom.Choose<DescriptionElement>(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13258"),
                    new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13259"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13260", new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs." + initializationRandom.Choose<string>("13261", "13262", "13263", "13264", "13265", "13266"))), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13267")));
                fishingQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13268"));
                var objective = fishingQuest.objective;
                DescriptionElement descriptionElement2;
                if (!isSquid)
                {
                    descriptionElement2 = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, fishingQuest.numberToFish.Value, fishObject);
                }
                else
                {
                    descriptionElement2 = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", 0, fishingQuest.numberToFish.Value);
                }
                objective.Value = descriptionElement2;
            }
            fishingQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", fishingQuest.reward.Value));
            fishingQuest.parts.Add("Strings\\StringsFromCSFiles:FishingQuest.cs.13275");


            return fishingQuest;
        }

        private static int GetGoldRewardPerItem(Item item)
        {
            return item is Object @object ? @object.Price : (int)(item.salePrice() * 1.5);
        }

        private static Quest CreateResourceCollectionQuest((string itemInfo, string npcName) extraInfo)
        {
            var resourceQuest = new ResourceCollectionQuest();


            var initializationRandom = CreateRerolledRandom();
            resourceQuest.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");

            var item = _itemManager.GetObjectByName(extraInfo.itemInfo);
            var itemId = item.Id;
            var qualifiedItemId = item.GetQualifiedId();

            resourceQuest.ItemId.Value = qualifiedItemId;
            resourceQuest.target.Value = qualifiedItemId == QualifiedItemIds.WOOD || qualifiedItemId == QualifiedItemIds.STONE ? NPCNames.ROBIN : NPCNames.CLINT;

            var num = initializationRandom.Next(6) * 2;
            for (var index = 0; index < initializationRandom.Next(1, 100); ++index)
            {
                initializationRandom.Next();
            }
            var miningLevel = Game1.getAllFarmers().Select(allFarmer => allFarmer.MiningLevel).Prepend(0).Max();
            var foragingLevel = Game1.getAllFarmers().Select(allFarmer => allFarmer.ForagingLevel).Prepend(0).Max();
            switch (itemId)
            {
                case ObjectIds.COPPER_ORE:
                    resourceQuest.number.Value = 20 + miningLevel * 2 + initializationRandom.Next(-2, 4) * 2;
                    resourceQuest.reward.Value = resourceQuest.number.Value * 10;
                    resourceQuest.number.Value -= resourceQuest.number.Value % 5;
                    break;
                case ObjectIds.IRON_ORE:
                    resourceQuest.number.Value = 15 + miningLevel + initializationRandom.Next(-1, 3) * 2;
                    resourceQuest.reward.Value = resourceQuest.number.Value * 15;
                    resourceQuest.number.Value = (int)(resourceQuest.number.Value * 0.75);
                    resourceQuest.number.Value -= resourceQuest.number.Value % 5;
                    break;
                case ObjectIds.COAL:
                    resourceQuest.number.Value = 10 + miningLevel + initializationRandom.Next(-1, 3) * 2;
                    resourceQuest.reward.Value = resourceQuest.number.Value * 25;
                    resourceQuest.number.Value = (int)(resourceQuest.number.Value * 0.75);
                    resourceQuest.number.Value -= resourceQuest.number.Value % 5;
                    break;
                case ObjectIds.GOLD_ORE:
                    resourceQuest.number.Value = 8 + miningLevel / 2 + initializationRandom.Next(-1, 1) * 2;
                    resourceQuest.reward.Value = resourceQuest.number.Value * 30;
                    resourceQuest.number.Value = (int)(resourceQuest.number.Value * 0.75);
                    resourceQuest.number.Value -= resourceQuest.number.Value % 2;
                    break;
                case ObjectIds.WOOD:
                    resourceQuest.number.Value = 25 + foragingLevel + initializationRandom.Next(-3, 3) * 2;
                    resourceQuest.number.Value -= resourceQuest.number.Value % 5;
                    resourceQuest.reward.Value = resourceQuest.number.Value * 8;
                    break;
                case ObjectIds.STONE:
                    resourceQuest.number.Value = 25 + miningLevel + initializationRandom.Next(-3, 3) * 2;
                    resourceQuest.number.Value -= resourceQuest.number.Value % 5;
                    resourceQuest.reward.Value = resourceQuest.number.Value * 8;
                    break;
            }

            var obj = ItemRegistry.Create(qualifiedItemId);
            if (qualifiedItemId != QualifiedItemIds.WOOD && qualifiedItemId != QualifiedItemIds.STONE)
            {
                resourceQuest.parts.Clear();
                var index = initializationRandom.Next(4);
                resourceQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13647", resourceQuest.number.Value, obj, new DescriptionElement(
                    "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + new[]
                    {
                        "13649",
                        "13650",
                        "13651",
                        "13652",
                    }[index])));
                resourceQuest.dialogueparts.Clear();
                if (index == 3)
                {
                    resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13655");
                    resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + initializationRandom.Choose("13656", "13657", "13658"));
                    resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13659");
                }
                else
                {
                    resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13662");
                    resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + initializationRandom.Choose("13656", "13657", "13658"));
                    var dialogueParts = resourceQuest.dialogueparts;
                    DescriptionElement descriptionElement;
                    if (!initializationRandom.NextBool())
                    {
                        descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13672");
                    }
                    else
                    {
                        descriptionElement = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13667", new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + initializationRandom.Choose("13668", "13669", "13670")));
                    }
                    dialogueParts.Add(descriptionElement);
                    resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13673");
                }
            }
            else
            {
                resourceQuest.parts.Clear();
                resourceQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13674", resourceQuest.number.Value, obj));
                resourceQuest.dialogueparts.Clear();
                resourceQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13677", new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + (resourceQuest.ItemId.Value == "(O)388"
                    ? "13678": "13679"))));
                resourceQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + initializationRandom.Choose("13681", "13682", "13683"));
            }
            resourceQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", resourceQuest.reward.Value));
            resourceQuest.parts.Add(resourceQuest.target.Value.Equals("Clint") ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13688" : "");
            resourceQuest.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", "0", resourceQuest.number.Value, obj);


            return resourceQuest;
        }

        private static Quest CreateSlayMonsterQuest((string monsterName, string npcName) extraInfo)
        {
            var slayMonsterQuest = new SlayMonsterQuest();

            slayMonsterQuest.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");

            var initializationRandom = CreateRerolledRandom();
            for (var index = 0; index < initializationRandom.Next(1, 100); ++index)
            {
                initializationRandom.Next();
            }

            var monsterName = extraInfo.monsterName.Replace("Dust Sprite", "Dust Spirit");
            var npcName = extraInfo.npcName;

            slayMonsterQuest.monsterName.Value = monsterName;
            slayMonsterQuest.target.Value = npcName;

            if (monsterName == "Frost Jelly" || monsterName == "Sludge")
            {
                slayMonsterQuest.monster.Value = new Monster("Green Slime", Vector2.Zero)
                {
                    Name = monsterName,
                };
            }
            else
            {
                slayMonsterQuest.monster.Value = new Monster(monsterName, Vector2.Zero);
            }

            if (monsterName == "Duggy")
            {
                slayMonsterQuest.parts.Clear();
                slayMonsterQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13711", slayMonsterQuest.numberToKill.Value));
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(2, 4);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 150;
            }
            else if (monsterName == "Ghost")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(2, 4);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 250;
            }
            else if (monsterName == "Sludge")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(4, 11);
                slayMonsterQuest.numberToKill.Value -= slayMonsterQuest.numberToKill.Value % 2;
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 125;
            }
            else if (monsterName == "Skeleton")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(6, 12);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 100;
            }
            else if (monsterName == "Lava Crab")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(2, 6);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 180;
            }
            else if (monsterName == "Rock Crab")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(2, 6);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 75;
            }
            else if (monsterName == "Squid Kid")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(1, 3);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 350;
            }
            else if (monsterName == "Dust Spirit")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(10, 21);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 60;
            }
            else if (monsterName == "Frost Jelly")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(4, 11);
                slayMonsterQuest.numberToKill.Value -= slayMonsterQuest.numberToKill.Value % 2;
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 85;
            }
            else if (monsterName == "Green Slime")
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(4, 11);
                slayMonsterQuest.numberToKill.Value -= slayMonsterQuest.numberToKill.Value % 2;
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 60;
            }
            else
            {
                slayMonsterQuest.numberToKill.Value = initializationRandom.Next(3, 7);
                slayMonsterQuest.reward.Value = slayMonsterQuest.numberToKill.Value * 120;
            }
            switch (monsterName)
            {
                case "Green Slime":
                case "Frost Jelly":
                case "Sludge":
                    slayMonsterQuest.parts.Clear();
                    slayMonsterQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13723", slayMonsterQuest.numberToKill.Value, slayMonsterQuest.monsterName.Value.Equals("Frost Jelly") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13725") : (slayMonsterQuest.monsterName.Value.Equals("Sludge") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13727") : (object) new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13728"))));
                    slayMonsterQuest.dialogueparts.Clear();
                    slayMonsterQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13730");
                    if (initializationRandom.NextBool())
                    {
                        slayMonsterQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13731");
                        slayMonsterQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + initializationRandom.Choose("13732", "13733"));
                        slayMonsterQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13734", new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + initializationRandom.Choose("13735", "13736")), new DescriptionElement("Strings\\StringsFromCSFiles:Dialogue.cs." + initializationRandom.Choose("795", "796", "797", "798", "799", "800", "801", "802", "803", "804", "805", "806", "807", "808", "809", "810")), new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + initializationRandom.Choose("13740", "13741", "13742"))));
                        break;
                    }
                    slayMonsterQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13744");
                    break;
                case "Rock Crab":
                case "Lava Crab":
                    slayMonsterQuest.parts.Clear();
                    slayMonsterQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13747", slayMonsterQuest.numberToKill.Value));
                    slayMonsterQuest.dialogueparts.Clear();
                    slayMonsterQuest.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13750", slayMonsterQuest.monster.Value));
                    break;
                default:
                    slayMonsterQuest.parts.Clear();
                    slayMonsterQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13752", slayMonsterQuest.monster.Value, slayMonsterQuest.numberToKill.Value, new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + initializationRandom.Choose("13755", "13756", "13757"))));
                    slayMonsterQuest.dialogueparts.Clear();
                    slayMonsterQuest.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13760");
                    break;
            }
            slayMonsterQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", slayMonsterQuest.reward.Value));
            slayMonsterQuest.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", "0", slayMonsterQuest.numberToKill.Value, slayMonsterQuest.monster.Value);


            return slayMonsterQuest;
        }

        private static Quest CreateSocializeQuest((string, string) extraInfo)
        {
            var helloQuest = new SocializeQuest();
            helloQuest.loadQuestInfo();
            return helloQuest;
        }

        private static List<string> CreateWeightedMissingLocations()
        {
            var hints = _archipelago.GetMyActiveDesiredHints();
            var weightedHelpWanteds = new List<string>();
            var allHelpWantedQuests = QuestLocations.HelpWantedQuests.Keys;
            var remainingHelpWanteds = QuestLocations.GetRemainingHelpWanteds(_locationChecker);
            var remainingHelpWantedNames = remainingHelpWanteds.Keys.ToHashSet();
            var currentForage = GetCurrentForage();
            foreach (var helpWantedLocation in allHelpWantedQuests)
            {
                var count = 1;
                if (remainingHelpWantedNames.Contains(helpWantedLocation))
                {
                    count *= 10;
                    if (hints.Any(hint => _archipelago.GetLocationName(hint) == helpWantedLocation))
                    {
                        count *= 10;
                    }
                }

                var questType = QuestLocations.GetQuestType(helpWantedLocation);
                var extraInfo = QuestLocations.GetExtraInfo(questType, helpWantedLocation);
                switch (questType)
                {
                    case QuestType.ItemDelivery:
                        count *= GetItemDeliveryWeight(extraInfo, currentForage);
                        break;
                    case QuestType.Monster:
                        count *= GetSlayingWeight(extraInfo);
                        break;
                    case QuestType.Socialize:
                        count *= GetSocializeWeight(extraInfo);
                        break;
                    case QuestType.Fishing:
                        count *= GetFishingWeight(extraInfo);
                        break;
                    case QuestType.ResourceCollection:
                        count *= GetGatheringWeight(extraInfo);
                        break;
                    default:
                        break;
                }

                for (var i = 0; i < count; i++)
                {
                    weightedHelpWanteds.Add(helpWantedLocation);
                }
            }

            return weightedHelpWanteds;
        }

        private static HashSet<string> GetCurrentForage()
        {
            var forageIds = new List<SpawnForageData>();
            foreach (var gameLocation in Game1.locations)
            {
                var season = gameLocation.GetSeason();
                foreach (var spawnForageData in gameLocation.GetData().Forage)
                {
                    if (spawnForageData.Condition != null && !GameStateQuery.CheckConditions(spawnForageData.Condition, gameLocation, random: Game1.random))
                    {
                        continue;
                    }

                    var forageSeason = spawnForageData.Season;
                    if (forageSeason.HasValue)
                    {
                        forageSeason = spawnForageData.Season;
                        if (!(forageSeason.GetValueOrDefault() == season & forageSeason.HasValue))
                        {
                            continue;
                        }
                    }
                    forageIds.Add(spawnForageData);
                }
            }

            return forageIds.Select(x => _itemManager.GetObjectById(x.ItemId).Name).ToHashSet();
        }

        private static int GetItemDeliveryWeight((string name, string itemName) extraInfo, HashSet<string> availableForage)
        {
            if (!Game1.player.friendshipData.ContainsKey(extraInfo.name))
            {
                return 0;
            }

            var item = _itemManager.GetObjectByName(extraInfo.itemName);
            if (item.Category == Category.FORAGE && availableForage.Contains(item.Name))
            {
                return 4;
            }

            var cropsDataBySeed = DataLoader.Crops(Game1.content);
            var cropsDataByCrops = cropsDataBySeed.ToDictionary(x => x.Value.HarvestItemId, x => x.Value);
            if (cropsDataByCrops.ContainsKey(item.Id))
            {
                var cropData = cropsDataByCrops[item.Id];
                if (cropData.Seasons.Contains(Game1.season))
                {
                    return 4;
                }
            }

            var allFishData = DataLoader.Fish(Game1.content);
            if (allFishData.ContainsKey(item.Id))
            {
                var fishData = allFishData[item.Id];
                var fishFields = fishData.Split("/");
                if (fishFields[1].Equals("trap", StringComparison.InvariantCultureIgnoreCase) || fishFields[6].Contains(Game1.season.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    return 4;
                }
            }

            return 1;
        }

        private static int GetSlayingWeight((string monsterName, string npcName) extraInfo)
        {
            if (!Game1.player.friendshipData.ContainsKey(extraInfo.npcName))
            {
                return 0;
            }

            var maxElevator = _archipelago.SlotData.ElevatorProgression == ElevatorProgression.Vanilla ? MineShaft.lowestLevelReached : MineshaftInjections.ElevatorFloorUnlocked();

            var canReachMonster = extraInfo.monsterName switch
            {
                MonsterName.GREEN_SLIME or MonsterCategory.SLIMES => maxElevator >= 5,
                MonsterName.ROCK_CRAB or MonsterCategory.ROCK_CRABS => maxElevator >= 15,
                MonsterName.DUGGY or MonsterCategory.DUGGIES => maxElevator >= 15,
                MonsterName.FROST_JELLY => maxElevator >= 40,
                MonsterName.SKELETON or MonsterCategory.SKELETONS => maxElevator >= 60,
                MonsterName.DUST_SPRITE or "Dust Sprite" or MonsterCategory.DUST_SPRITES => maxElevator >= 40,
                MonsterName.SLUDGE => maxElevator >= 80,
                MonsterName.LAVA_CRAB or "Squid Kid" => maxElevator >= 100,
                "Ghost" => maxElevator >= 50,
                _ => false,
            };

            if (canReachMonster)
            {
                return 4;
            }

            return 1;
        }

        private static int GetSocializeWeight((string _, string npcName) extraInfo)
        {
            if (!Game1.player.friendshipData.ContainsKey(extraInfo.npcName))
            {
                return 0;
            }

            return (int)Math.Ceiling(Game1.player.friendshipData.Length / 10d);
        }

        private static int GetFishingWeight((string type, string fishName) extraInfo)
        {
            var npc = extraInfo.type.Contains("Population") ? NPCNames.DEMETRIUS : NPCNames.WILLY;
            if (!Game1.player.friendshipData.ContainsKey(npc))
            {
                return 0;
            }

            if (!ToolUnlockManager.HasAnyFishingRod(_archipelago))
            {
                return 0;
            }

            var season = extraInfo.type.Split(" ").Last();
            if (!season.Equals(Game1.season.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return 0;
            }

            var fish = _itemManager.GetObjectByName(extraInfo.fishName);
            var fishId = fish.Id;
            if (Game1.player.fishCaught.ContainsKey(fishId))
            {
                return 4;
            }

            return 1;

        }

        private static int GetGatheringWeight((string itemName, string npcName) extraInfo)
        {
            if (!Game1.player.friendshipData.ContainsKey(extraInfo.npcName))
            {
                return 0;
            }

            if (extraInfo.itemName.Equals("Wood", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!ToolUnlockManager.HasAnyAxe(_archipelago))
                {
                    return 0;
                }
            }
            else
            {
                if (!ToolUnlockManager.HasAnyPickaxe(_archipelago))
                {
                    return 0;
                }
            }

            return 1;
        }

        // public static string getRandomItemFromSeason(Season season, int randomSeedAddition, bool forQuest, bool changeDaily = true)
        public static bool GetRandomItemFromSeason_ConsiderRerolls_Prefix(Season season, ref int randomSeedAddition, bool forQuest, bool changeDaily, ref string __result)
        {
            try
            {
                randomSeedAddition += (int)_rerollCount;
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRandomItemFromSeason_ConsiderRerolls_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static string getRandomItemFromSeason(Season season, bool forQuest, Random random)
        public static void GetRandomItemFromSeason_RemoveFishIfCantCatchThem_Postfix(Season season, bool forQuest, Random random, ref string __result)
        {
            try
            {
                if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive) ||
                    _archipelago.HasReceivedItem(ToolUnlockManager.PROGRESSIVE_FISHING_ROD))
                {
                    return;
                }

                var chosenItem = ItemRegistry.Create<Object>(__result);
                if (chosenItem.Category == Category.FISH)
                {
                    __result = Utility.getRandomItemFromSeason(season, forQuest, random);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRandomItemFromSeason_RemoveFishIfCantCatchThem_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void IncrementRerollCount()
        {
            _rerollCount++;
        }

        // protected Random CreateInitializationRandom()
        public static bool CreateInitializationRandom_ConsiderRerolls_Prefix(Quest __instance, ref Random __result)
        {
            try
            {
                __result = CreateRerolledRandom();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CreateInitializationRandom_ConsiderRerolls_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static Random CreateRerolledRandom()
        {
            return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 1.3, _rerollCount);
        }
    }
}
