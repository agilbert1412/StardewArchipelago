using System;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using xTile.Dimensions;
using Microsoft.Xna.Framework;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Network;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class PhoneInjections
    {
        public const string TRANSITIONAL_STRING = " to ";
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static WeaponsManager _weaponsManager;
        private static EntranceManager _entranceManager;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, WeaponsManager weaponsManager, EntranceManager entranceManager)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _weaponsManager = weaponsManager;
            _entranceManager = entranceManager;
        }

        // public bool TryHandleOutgoingCall(string callId)
        public static bool TryHandleOutgoingCall_EntranceRandomizer_Prefix(DefaultPhoneHandler __instance, string callId)
        {
            try
            {
                List<Response> _currentCallResponses = new List<Response>();
                switch (callId)
                {
                    case "AdventureGuild":
                        CallAdventureGuild();
                        break;
                    case "AnimalShop":
                        CallAnimalShop();
                        break;
                    case "Blacksmith":
                        CallBlacksmith();
                        break;
                    case "Carpenter":
                        CallCarpenter();
                        break;
                    case "Saloon":
                        CallSaloon();
                        break;
                    case "SeedShop":
                        CallSeedShop();
                        break;
                    default:
                        throw new Exception($"{callId} is not a valid phone number.");
                }

                Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
                {
                    if (_currentCallResponses.Count > 0)
                    {
                        Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), _currentCallResponses.ToArray(), "telephone");
                    }
                });
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(TryHandleOutgoingCall_EntranceRandomizer_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_EntranceRandomizer_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            try
            {
                switch (questionAndAnswer)
                {
                    case "telephone_AdventureGuildEntrance":
                        WriteMarlonEntranceResponse();
                        break;
                    case "telephone_AdventureGuildRecovery":
                        WriteMarlonRecoveryResponse();
                        break;
                    case "telephone_AnimalShopEntrance":
                        WriteMarnieResponse();
                        break;
                    case "telephone_BlacksmithEntrance":
                        WriteClintResponse();
                        break;
                    case "telephone_CarpenterEntrance":
                        WriteRobinResponse();
                        break;
                    case "telephone_SeedShopEntrance":
                        WritePierreResponse();
                        break;
                    default:
                        return true;
                }
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_EntranceRandomizer_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void CallAdventureGuild()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("AdventureGuild");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.playSound("bigSelect");

                WriteCharacterDialog("Marlon", "Marlon Speaking... Are you lost again? Yes, yes... Tell me what you need.");
                Game1.afterDialogues += () =>
                {
                    List<Response> _currentCallResponses = new List<Response>();
                    _currentCallResponses.Add(new Response("AdventureGuildEntrance", "Check Entrance"));
                    _currentCallResponses.Add(new Response("AdventureGuildRecovery", "Item Recovery"));
                    _currentCallResponses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), _currentCallResponses.ToArray(), "telephone");
                };
            }, 4950);
        }

        private static void CallAnimalShop()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("AnimalShop");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC characterFromName = Game1.getCharacterFromName("Marnie");
                if (GameLocation.AreStoresClosedForFestival() ||
                    characterFromName.ScheduleKey == "fall_18" ||
                    characterFromName.ScheduleKey == "winter_18" ||
                    characterFromName.ScheduleKey == "Tue" ||
                    characterFromName.ScheduleKey == "Mon")
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_ClosedDay");
                }
                else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
                {
                    Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
                }
                else
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_Closed");
                }

                Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
                {
                    List<Response> _currentCallResponses = new List<Response>();
                    _currentCallResponses.Add(new Response("AnimalShopEntrance", "Check Entrance"));
                    _currentCallResponses.Add(new Response("AnimalShop_CheckAnimalPrices", Game1.content.LoadString("Strings\\Characters:Phone_CheckAnimalPrices")));
                    _currentCallResponses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), _currentCallResponses.ToArray(), "telephone");
                });
            }, 4950);
        }

        private static void CallBlacksmith()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("Blacksmith");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC characterFromName = Game1.getCharacterFromName("Clint");
                if (GameLocation.AreStoresClosedForFestival() ||
                    characterFromName.ScheduleKey == "winter_16" ||
                    characterFromName.ScheduleKey == "Fri")
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
                }
                int value = Game1.player.daysLeftForToolUpgrade.Value;
                if (value == 1)
                {
                    Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Working_OneDay");
                }
                else if (value > 0)
                {
                    Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Working", value);
                }
                else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
                {
                    Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
                }
                else
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Closed");
                }

                Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
                {
                    List<Response> _currentCallResponses = new List<Response>();
                    _currentCallResponses.Add(new Response("BlacksmithEntrance", "Check Entrance"));
                    _currentCallResponses.Add(new Response("Blacksmith_UpgradeCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckToolCost")));
                    _currentCallResponses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), _currentCallResponses.ToArray(), "telephone");
                });
            }, 4950);
        }

        private static void CallCarpenter()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("Carpenter");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC characterFromName = Game1.getCharacterFromName("Robin");
                Town town = Game1.getLocationFromName("Town") as Town;
                if (GameLocation.AreStoresClosedForFestival() ||
                    characterFromName.ScheduleKey == "summer_18")
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Festival");
                }
                else if (characterFromName.ScheduleKey == "Tue")
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Workout");
                }
                else if (town != null && town.daysUntilCommunityUpgrade.Value > 0)
                {
                    int value = town.daysUntilCommunityUpgrade.Value;
                    DisplayRobinWorkDays(characterFromName, value);
                }
                else if (Game1.IsThereABuildingUnderConstruction())
                {
                    BuilderData builderData = Game1.netWorldState.Value.GetBuilderData("Robin");
                    int num = 0;
                    if (builderData != null)
                    {
                        num = builderData.daysUntilBuilt.Value;
                    }
                    DisplayRobinWorkDays(characterFromName, num);
                }
                else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
                {
                    Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
                }
                else
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Closed");
                }

                Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
                {
                    List<Response> _currentCallResponses = new List<Response>
                    {
                        new Response("CarpenterEntrance", "Check entrance"),
                        new Response("Carpenter_ShopStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock"))
                    };
                    if ((int)Game1.player.houseUpgradeLevel < 3)
                    {
                        _currentCallResponses.Add(new Response("Carpenter_HouseCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckHouseCost")));
                    }

                    _currentCallResponses.Add(new Response("Carpenter_BuildingCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckBuildingCost")));
                    _currentCallResponses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), _currentCallResponses.ToArray(), "telephone");
                });
            }, 4950);
        }

        private static void DisplayRobinWorkDays(NPC characterFromName, int value)
        {
            if (value == 1)
            {
                Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Working_OneDay");
            }
            else
            {
                Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Working", value);
            }
        }

        private static void CallSaloon()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("Saloon");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC character = Game1.getCharacterFromName("Gus");
                if (GameLocation.AreStoresClosedForFestival())
                {
                    Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Gus_Festival");
                }
                else if (Game1.timeOfDay >= 1200 && Game1.timeOfDay < 2400 && (character.ScheduleKey != "fall_4" || Game1.timeOfDay >= 1700))
                {
                    if (Game1.dishOfTheDay == null)
                    {
                        WriteCharacterDialog("Gus", $"Hello,? Hi @! Yes... today's special is: '{Game1.dishOfTheDay.DisplayName}'! Just stop by {GetCurrentModifiedEntranceTo("Saloon")} in {GetCurrentModifiedEntranceFrom("Saloon")}. We're open right now, so feel free to come on in for a taste.$0#$b# *click*");
                    }
                    else
                    {
                        WriteCharacterDialog("Gus", $"Hello,? Hi @! Yup, we're open right now. Just stop by {GetCurrentModifiedEntranceTo("Saloon")}  in {GetCurrentModifiedEntranceFrom("Saloon")}. Hope to see you soon!. *click*");
                    }
                }
                else if (Game1.dishOfTheDay != null && Game1.timeOfDay < 2400)
                {
                    WriteAnsweringMachineDialog("Gus", $"Hello, this is Gus from the Stardrop Saloon. We're currently closed, but please come by later if you'd like to try our daily special, '{Game1.dishOfTheDay.DisplayName}'!" +
                        $"$0#$b#We are currently located at {GetCurrentModifiedEntranceTo("Saloon")} , just come in through {GetCurrentModifiedEntranceFrom("Saloon")}. *beep*");
                }
                else
                {
                    WriteAnsweringMachineDialog("Gus", $"Hello, this is Gus from the Stardrop Saloon. We're currently closed. Please stop in between 12PM and 12AM if you would like some of our delicious food and drink." +
                        $"$0#$b#We are currently located at {GetCurrentModifiedEntranceTo("Saloon")} , just come in through {GetCurrentModifiedEntranceFrom("Saloon")}. *beep*");
                }

                Game1.currentLocation.answerDialogueAction("HangUp", Array.Empty<string>());
            }, 4950);
        }

        private static void CallSeedShop()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("SeedShop");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC characterFromName = Game1.getCharacterFromName("Pierre");
                string text = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                if (GameLocation.AreStoresClosedForFestival())
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Pierre_Festival");
                }
                else if ((Game1.isLocationAccessible("CommunityCenter") || text != "Wed") && Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
                {
                    Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Pierre_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
                }
                else
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Pierre_Closed");
                }

                Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
                {
                    List<Response> _currentCallResponses = new List<Response>();
                    _currentCallResponses.Add(new Response("SeedShopEntrance", "Check entrance"));
                    _currentCallResponses.Add(new Response("SeedShop_CheckSeedStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock")));
                    _currentCallResponses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), _currentCallResponses.ToArray(), "telephone");
                });
            }, 4950);
        }

        private static void WriteMarlonEntranceResponse()
        {
            string dialog = $"The mines are at {GetCurrentModifiedEntranceTo("Mine|18|13")}.$0#$b#Oh... Come in through {GetCurrentModifiedEntranceFrom("Mine|18|13")}. Farewell. *Click*";

            WriteCharacterDialog("Marlon", dialog);
        }

        private static void WriteMarlonRecoveryResponse()
        {
            if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
            {
                WriteCharacterDialog("Marlon", "Yep, I'm going to fetch your item tonight. You can relax. No need to keep calling! *click*");
            }
            else
            {
                WriteCharacterDialog("Marlon", "Yes, yes... I'm willing to fetch it... for a price.");
                Game1.afterDialogues += () =>
                {
                    var equipmentsToRecover = _weaponsManager.GetEquipmentsForSale(IDProvider.ARCHIPELAGO_EQUIPMENTS_RECOVERY);
                    if (equipmentsToRecover.Any())
                    {
                        Game1.player.forceCanMove();
                        Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon");
                    }
                    else
                    {
                        WriteCharacterDialog("Marlon", "What's that? You haven't lost anything in the mines? Okay. Sounds like you don't need my help, then. Take care. *click*");
                    }
                };
            }
        }

        private static void WriteMarnieResponse()
        {
            var character = Game1.getCharacterFromName("Marnie");
            if (GameLocation.AreStoresClosedForFestival() || character.ScheduleKey == "fall_18" || character.ScheduleKey == "winter_18" || character.ScheduleKey == "Tue" || character.ScheduleKey == "Mon" || Game1.timeOfDay <= 900 || Game1.timeOfDay > 1600)
            {
                WriteAnsweringMachineDialog("Marnie", $"We have currently set up shop at {GetCurrentModifiedEntranceTo("AnimalShop")} today. Just come in through {GetCurrentModifiedEntranceFrom("AnimalShop")}. Please visit us from 9 AM to 4 PM from Wednesday to Sunday. *Click*");
            }
            else
            {
                WriteCharacterDialog("Marnie", $"Oh! Yes, we are set up at {GetCurrentModifiedEntranceTo("AnimalShop")} today. Just come in through {GetCurrentModifiedEntranceFrom("AnimalShop")}. I hope you will be able to stop by! *Click*");
            }
        }

        private static void WriteClintResponse()
        {
            var character = Game1.getCharacterFromName("Clint");
            if (GameLocation.AreStoresClosedForFestival() || character.ScheduleKey == "Fri" || character.ScheduleKey == "winter_16" || Game1.timeOfDay <= 900 || Game1.timeOfDay > 1600)
            {
                WriteAnsweringMachineDialog("Clint", $"I am currently located at {GetCurrentModifiedEntranceTo("Blacksmith")} today. Come in through {GetCurrentModifiedEntranceFrom("Blacksmith")}. *Click*");
            }
            else
            {
                WriteCharacterDialog("Clint", $"Let's see... I will be working at {GetCurrentModifiedEntranceTo("Blacksmith")} in {GetCurrentModifiedEntranceFrom("Blacksmith")} today. Yup. Umm... Goodbye. *Click*");
            }
        }


        private static void WriteRobinResponse()
        {
            var character = Game1.getCharacterFromName("Robin");
            if (GameLocation.AreStoresClosedForFestival() || character.ScheduleKey == "Tue" || character.ScheduleKey == "summer_18" || Game1.timeOfDay <= 900 || Game1.timeOfDay > 1700)
            {
                WriteAnsweringMachineDialog("Robin", $"I'm currently at {GetCurrentModifiedEntranceTo("ScienceHouse|6|24")}. Come in through {GetCurrentModifiedEntranceFrom("ScienceHouse|6|24")} between 9 AM and 5PM if you need anything! *beep*");
            }
            else
            {
                WriteCharacterDialog("Robin", $"Of course! I'm currently at {GetCurrentModifiedEntranceTo("ScienceHouse|6|24")}. That's in {GetCurrentModifiedEntranceFrom("ScienceHouse|6|24")} if you're confused... again, please swing by if you need anything! *click*");
            }
        }

        private static void WritePierreResponse()
        {
            var character = Game1.getCharacterFromName("Pierre");
            if (GameLocation.AreStoresClosedForFestival() || (Game1.isLocationAccessible("CommunityCenter") || character.ScheduleKey == "Wed") || Game1.timeOfDay <= 0 || Game1.timeOfDay > 1700)
            {
                WriteAnsweringMachineDialog("Pierre", $"We've moved to {GetCurrentModifiedEntranceTo("SeedShop")}. Come in through {GetCurrentModifiedEntranceFrom("SeedShop")}. See you then! *beep*");
            }
            else
            {
                WriteCharacterDialog("Pierre", $"Oh yes... We now live in {GetCurrentModifiedEntranceTo("SeedShop")}. Come in through {GetCurrentModifiedEntranceFrom("SeedShop")}. Please be sure to check out our deals on seeds and produce! *click*");
            }
        }

        private static void WriteCharacterDialog(string characterName, string dialog)
        {
            var character = Game1.getCharacterFromName(characterName);
            Dialogue locationDialog = new Dialogue(character, "", dialog);
            Game1.DrawDialogue(locationDialog);
        }

        private static void WriteAnsweringMachineDialog(string characterName, string dialog)
        {
            var character = Game1.getCharacterFromName(characterName);
            Dialogue locationDialog = new Dialogue(character, "", dialog);
            locationDialog.overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine");
            Game1.DrawDialogue(locationDialog);
        }

        private static Dictionary<string, string> _locationAliases = new()
        {
            { "The Mayor's Manor", "ManorHouse" },
            { "Pierre's General Store", "SeedShop" },
            { "Clint's Blacksmith", "Blacksmith" },
            { "Alex's House", "JoshHouse" },
            { "The Tunnel Entrance", "Backwoods" },
            { "Marnie's Ranch", "AnimalShop" },
            { "Leah's Cottage", "LeahHouse" },
            { "The Wizard Tower", "WizardHouse" },
            { "The Sewers", "Sewer" },
            { "The Bus Tunnel", "Tunnel" },
            { "Robin's Carpenter Shop To Basement", "ScienceHouse" },
            { "Robin's Carpenter Shop", "ScienceHouse|6|24" }, // LockedDoorWarp 6 24 ScienceHouse 900 2000S–
            { "Maru's Room", "ScienceHouse|3|8" }, // LockedDoorWarp 3 8 ScienceHouse 900 2000 Maru 500N
            { "The Adventurer's Guild", "AdventureGuild" },
            { "Willy's Fish Shop", "FishShop" },
            { "The Museum", "ArchaeologyHouse" },
            { "The Wizard's Basement", "WizardHouseBasement" },
            { "The Mines", "Mine|18|13" }, // 54 4 Mine 18 13
            { "The Quarry Mine Entrance", "Mine|67|17" }, // 103 15 Mine 67 17
            { "The Ginger Island Shipwreck", "CaptainRoom" },
            { "The Ginger Island Farm", "IslandWest" },
            { "Gourmand's Cave", "IslandFarmcave" },
            { "The Crystal Cave", "IslandWestCave1" },
            { "The Boulder Cave", "IslandNorthCave1" },
            { "The Skull Cavern Entrance", "SkullCave" },
            { "The Oasis", "SandyHouse" },
            { "The Casino", "Club" },
            { "The Bathhouse Entrance", "BathHouse_Entry" },
            { "The Locker Room", "BathHouse_{0}Locker" },
            { "The Public Bath", "BathHouse_Pool" },
            { "Southeast Ginger Island", "IslandSouthEast" },
            { "The Pirate's Cove", "IslandSouthEastCave" },
            { "Leo's Hut", "IslandHut" },
            { "The Dig Site", "IslandNorth" },
            { "The Field Office", "IslandFieldOffice" },
            { "The Island Farmhouse", "IslandFarmHouse" },
            { "The Volcano Entrance", "VolcanoDungeon0|31|53" },
            { "The Volcano River", "VolcanoDungeon0|6|49" },
            { "The Secret Beach", "IslandNorth|12|31" },
            { "Professor Snail's Cave", "IslandNorthCave1" },
            { "Qi's Walnut Room", "QiNutRoom" },
            { "The Mutant Bug Lair", "BugLand" },
            { "The Witch's Hut", "WitchHut" },
            { "The Witch's Swamp", "WitchSwamp" },
            { "The Witch's Warp Cave", "WitchWarpCave" },
            { "Jodi's House", "SamHouse" },
            { "The Jungle Gem Puzzle", "IslandShrine" },
            { "The Ginger Island Jungle", "IslandEast" },
            { "Leo's Tree House", "LeoTreeHouse" },
            { "The Farm House", "FarmHouse" },
            { "The Bathhouse Entry", "BathHouse_Entry" },
            { "The Men's Locker Room", "BathHouse_MensLocker" },
            { "The Women's Locker Room", "BathHouse_WomensLocker" },
            { "Haley's House", "HaleyHouse" },
            { "Joja Mart", "JojaMart" },
            { "Harvey's Room", "HarveyRoom" },
            { "Sebastian's Room", "SebastianRoom" },
            { "The Mastery Cave", "MasteryCave" }
        };

        private static string GetCurrentModifiedEntranceTo(string currentChaosBuilding)
        {
            string currentEntrance = _entranceManager.ModifiedEntrances.FirstOrDefault(e => e.Key.Contains($"{currentChaosBuilding}{TRANSITIONAL_STRING}")).Value;
            currentEntrance = currentEntrance?.Split(TRANSITIONAL_STRING).First() ?? "";

            return _locationAliases.FirstOrDefault(k => k.Value == currentEntrance).Key ?? "The " + currentEntrance;

        }
        private static string GetCurrentModifiedEntranceFrom(string currentChaosBuilding)
        {
            string currentEntrance = _entranceManager.ModifiedEntrances.FirstOrDefault(e => e.Key.Contains($"{currentChaosBuilding}{TRANSITIONAL_STRING}")).Value;
            currentEntrance = currentEntrance?.Split(TRANSITIONAL_STRING).Last() ?? "";

            return _locationAliases.FirstOrDefault(k => k.Value == currentEntrance).Key ?? "The " + currentEntrance;

        }
    }
}
