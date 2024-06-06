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

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class PhoneInjections
    {
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

        // public void CallAdventureGuild()
        public static bool CallAdventureGuild_AllowRecovery_Prefix(DefaultPhoneHandler __instance)
        {
            try
            {
                Game1.currentLocation.playShopPhoneNumberSounds("AdventureGuild");
                Game1.player.freezePause = 4950;
                DelayedAction.functionAfterDelay(() =>
                {
                    Game1.playSound("bigSelect");

                    if (_archipelago.SlotData.EntranceRandomization != EntranceRandomization.Chaos)
                    {
                        var character = Game1.getCharacterFromName("Marlon");
                        if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
                        {
                            Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_AlreadyRecovering");
                        }
                        else
                        {
                            Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_Open");
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
                                    Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_NoDeathItems");
                                }
                            };
                        }
                    }
                    else
                    {
                        var character = Game1.getCharacterFromName("Marlon");
                        Dialogue chaosDialog = new Dialogue(character, "", "Marlon Speaking... Are you lost again? Yes, yes... Tell me what you need.");
                        Game1.DrawDialogue(chaosDialog);
                        Game1.afterDialogues += () =>
                        {
                            Response[] answerChoices = new Response[3]
                            {
                                new Response("AdventureGuildEntrance", "Check Entrance"),
                                new Response("AdventureGuildRecovery", "Item Recovery"),
                                new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
                            };
                            Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), answerChoices, "telephone");
                        };
                    }
                }, 4950);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CallAdventureGuild_AllowRecovery_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AnswerDialogueAction_AdventureGuildEntrance_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer != "telephone_AdventureGuildEntrance")
            {
                return true;
            }

            var character = Game1.getCharacterFromName("Marlon");
            Dialogue locationDialog = new Dialogue(character, "", $"We are at the... *checks window*... { _entranceManager.GetCurrentModifiedEntranceTo("AdventureGuild") }.$0#$b#Oh... Come in through the {_entranceManager.GetCurrentModifiedEntranceFrom("AdventureGuild")}. Farewell. *Click*");
            Game1.DrawDialogue(locationDialog);
            return false;
        }

        public static bool AnswerDialogueAction_AdventureGuildRecovery_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer != "telephone_AdventureGuildRecovery")
            {
                return true;
            }

            var character = Game1.getCharacterFromName("Marlon");
            if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
            {
                Dialogue locationDialog = new Dialogue(character, "", $"Yep, I'm going to fetch your item tonight. You can relax. No need to keep calling! *click*");
                Game1.DrawDialogue(locationDialog);
            }
            else
            {
                Dialogue locationDialog = new Dialogue(character, "", $"Yes, yes... I'm willing to fetch it... for a price.");
                Game1.DrawDialogue(locationDialog);
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
                        Dialogue locationDialog = new Dialogue(character, "", "What's that? You haven't lost anything in the mines? Okay. Sounds like you don't need my help, then. Take care. *click*");
                        Game1.DrawDialogue(locationDialog);
                    }
                };
            }
            return false;
        }

        public static bool CallAnimalShop_CheckEntranceOption_Prefix(DefaultPhoneHandler __instance)
        {
            if (_archipelago.SlotData.EntranceRandomization != EntranceRandomization.Chaos)
            {
                return true; //Run original logic
            }

            GameLocation location = Game1.currentLocation;
            location.playShopPhoneNumberSounds("AnimalShop");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC characterFromName = Game1.getCharacterFromName("Marnie");
                if (GameLocation.AreStoresClosedForFestival())
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_ClosedDay");
                }
                else if (characterFromName.ScheduleKey == "fall_18" || characterFromName.ScheduleKey == "winter_18" || characterFromName.ScheduleKey == "Tue" || characterFromName.ScheduleKey == "Mon")
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
                    Response[] answerChoices = new Response[3]
                    {
                        new Response("AnimalShopEntrance", "Check Entrance"),
                        new Response("AnimalShop_CheckAnimalPrices", Game1.content.LoadString("Strings\\Characters:Phone_CheckAnimalPrices")),
                        new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
                    };
                    location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), answerChoices, "telephone");
                });
            }, 4950);
            return false;
        }

        public static bool AnswerDialogueAction_AnimalShopEntrance_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer != "telephone_AnimalShopEntrance")
            {
                return true;
            }

            var character = Game1.getCharacterFromName("Marnie");
            if (GameLocation.AreStoresClosedForFestival() || character.ScheduleKey == "fall_18" || character.ScheduleKey == "winter_18" || character.ScheduleKey == "Tue" || character.ScheduleKey == "Mon" || Game1.timeOfDay <= 900 || Game1.timeOfDay > 1600)
            {
                Dialogue locationDialog = new Dialogue(character, "", $"We have currently set up shop at the {_entranceManager.GetCurrentModifiedEntranceTo("AnimalShop")} today. Just come in through the {_entranceManager.GetCurrentModifiedEntranceFrom("AnimalShop")}. Please visit us from 9 AM to 4 PM from Wednesday to Sunday. *Click*");
                locationDialog.overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine");
                Game1.DrawDialogue(locationDialog);
            }
            else
            {
                Dialogue locationDialog = new Dialogue(character, "", $"Oh! Yes, we are set up at the {_entranceManager.GetCurrentModifiedEntranceTo("AnimalShop")} today. Just come in through the {_entranceManager.GetCurrentModifiedEntranceFrom("AnimalShop")}. I hope you will be able to stop by! *Click*");
                Game1.DrawDialogue(locationDialog);
            }
            return false;
        }

        public static bool CallBlacksmith_CheckEntranceOption_Prefix(DefaultPhoneHandler __instance)
        {
            if (_archipelago.SlotData.EntranceRandomization != EntranceRandomization.Chaos)
            {
                return true; //Run original logic
            }

            GameLocation location = Game1.currentLocation;
            location.playShopPhoneNumberSounds("Blacksmith");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(delegate
            {
                Game1.playSound("bigSelect");
                NPC characterFromName = Game1.getCharacterFromName("Clint");
                if (GameLocation.AreStoresClosedForFestival())
                {
                    Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
                }
                else if (Game1.player.daysLeftForToolUpgrade.Value > 0)
                {
                    int value = Game1.player.daysLeftForToolUpgrade.Value;
                    if (value == 1)
                    {
                        Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Working_OneDay");
                    }
                    else
                    {
                        Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Working", value);
                    }
                }
                else
                {
                    string scheduleKey = characterFromName.ScheduleKey;
                    if (!(scheduleKey == "winter_16"))
                    {
                        if (scheduleKey == "Fri")
                        {
                            Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
                        }
                        else if (Game1.timeOfDay >= 600 && Game1.timeOfDay < 1600)
                        {
                            Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
                        }
                        else
                        {
                            Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Closed");
                        }
                    }
                    else
                    {
                        Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
                    }
                }

                Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
                {
                    Response[] answerChoices = new Response[3]
                    {
                        new Response("BlacksmithEntrance", "Check Entrance"),
                        new Response("Blacksmith_UpgradeCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckToolCost")),
                        new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
                    };
                    location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), answerChoices, "telephone");
                });
            }, 4950);
            return false;
        }

        public static bool AnswerDialogueAction_BlacksmithEntrance_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer != "telephone_BlacksmithEntrance")
            {
                return true;
            }

            var character = Game1.getCharacterFromName("Clint");
            if (GameLocation.AreStoresClosedForFestival() || character.ScheduleKey == "Fri" || character.ScheduleKey == "winter_16" || Game1.timeOfDay <= 600 || Game1.timeOfDay > 1600)
            {
                Dialogue locationDialog = new Dialogue(character, "", $"I am currently located at the {_entranceManager.GetCurrentModifiedEntranceTo("Blacksmith")} today. Come in through the {_entranceManager.GetCurrentModifiedEntranceFrom("Blacksmith")}. *Click*");
                locationDialog.overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine");
                Game1.DrawDialogue(locationDialog);
            }
            else
            {
                Dialogue locationDialog = new Dialogue(character, "", $"Let's see... I will be working at the {_entranceManager.GetCurrentModifiedEntranceTo("Blacksmith")} in the {_entranceManager.GetCurrentModifiedEntranceFrom("Blacksmith")} today. Yup. Umm... Goodbye. *Click*");
                Game1.DrawDialogue(locationDialog);
            }
            return false;
        }
    }
}
