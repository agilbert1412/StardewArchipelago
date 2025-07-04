using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship
{

    public class PregnancyInjections
    {
        private const string FIRST_BABY = "Have a Baby";
        private const string SECOND_BABY = "Have Another Baby";

        private const string NPC_GIVE_BIRTH_QUESTION = "Would you like me to give birth to a {0}, {1}?";
        private const string PLAYER_GIVE_BIRTH_QUESTION = "Would you like to give birth to a {0}, {1}?";
        private const string ORDER_BABY_QUESTION = "Should I order a {0}, {1}?";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public bool canGetPregnant()
        public static bool CanGetPregnant_ShuffledPregnancies_Prefix(NPC __instance, ref bool __result)
        {
            try
            {
                if (__instance is Horse)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var farmer = __instance.getSpouse();
                if (farmer == null || farmer.divorceTonight.Value)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var heartLevelForNpc = farmer.getFriendshipHeartLevelForNPC(__instance.Name);
                var spouseFriendship = farmer.GetSpouseFriendship();
                __instance.DefaultMap = farmer.homeLocation.Value;
                var homeOfFarmer = Utility.getHomeOfFarmer(farmer);
                if (homeOfFarmer.cribStyle.Value <= 0 || homeOfFarmer.upgradeLevel < 2 ||
                    spouseFriendship.DaysUntilBirthing >= 0 || heartLevelForNpc < 10 || farmer.GetDaysMarried() < 7)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationMissing(FIRST_BABY) || _locationChecker.IsLocationMissing(SECOND_BABY))
                {
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationChecked(FIRST_BABY) && _locationChecker.IsLocationChecked(SECOND_BABY) &&
                    _archipelago.GetReceivedItemCount("Cute Baby") + _archipelago.GetReceivedItemCount("Cute Baby") >= 2 && homeOfFarmer.getChildrenCount() < 2)
                {
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __result = false;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CanGetPregnant_ShuffledPregnancies_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool setUp()
        public static bool Setup_PregnancyQuestionEvent_Prefix(QuestionEvent __instance, ref bool __result)
        {
            try
            {
                var whichQuestionField = _helper.Reflection.GetField<int>(__instance, "whichQuestion");
                var whichQuestion = whichQuestionField.GetValue();
                if (whichQuestion != 1 && whichQuestion != 3)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Response[] answerChoices1 =
                {
                    new("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
                    new("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No")),
                };

                var spouse = Game1.getCharacterFromName(Game1.player.spouse);
                var question = PickBabyQuestionBasedOnGenders(spouse);

                var babyName = "Baby";
                if (_locationChecker.IsLocationMissing(FIRST_BABY))
                {
                    var scoutedItem = _archipelago.ScoutStardewLocation(FIRST_BABY);
                    if (scoutedItem != null)
                    {
                        babyName = scoutedItem.ItemName;
                    }
                }
                else if (_locationChecker.IsLocationMissing(SECOND_BABY))
                {
                    var scoutedItem = _archipelago.ScoutStardewLocation(SECOND_BABY);
                    if (scoutedItem != null)
                    {
                        babyName = scoutedItem.ItemName;
                    }
                }
                question = string.Format(question, babyName, Game1.player.Name);
                var answerPregnancyQuestionMethod = _helper.Reflection.GetMethod(__instance, "answerPregnancyQuestion");
                Game1.currentLocation.createQuestionDialogue(question, answerChoices1, (who, answer) => answerPregnancyQuestionMethod.Invoke(who, answer), spouse);
                Game1.messagePause = true;
                __result = false;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Setup_PregnancyQuestionEvent_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static string PickBabyQuestionBasedOnGenders(NPC spouse)
        {
            if (spouse.isRoommate() || spouse.Name.Equals("Krobus"))
            {
                return ORDER_BABY_QUESTION;
            }

            if (spouse.Gender == Gender.Female && Game1.player.IsMale)
            {
                return NPC_GIVE_BIRTH_QUESTION;
            }

            if (spouse.Gender == Gender.Male && !Game1.player.IsMale)
            {
                return PLAYER_GIVE_BIRTH_QUESTION;
            }


            return ORDER_BABY_QUESTION;
        }

        // private void answerPregnancyQuestion(Farmer who, string answer)
        public static bool AnswerPregnancyQuestion_CorrectDate_Prefix(QuestionEvent __instance, Farmer who, string answer)
        {
            try
            {
                if (!answer.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var worldDate = new WorldDate(Game1.Date);
                worldDate.TotalDays += 14;
                who.GetSpouseFriendship().NextBirthingDate = worldDate;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerPregnancyQuestion_CorrectDate_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool tickUpdate(GameTime time)
        public static bool TickUpdate_BirthingEvent_Prefix(BirthingEvent __instance, GameTime time, ref bool __result)
        {
            try
            {
                if (_locationChecker.IsLocationChecked(FIRST_BABY) && _locationChecker.IsLocationChecked(SECOND_BABY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var timerField = _helper.Reflection.GetField<int>(__instance, "timer");
                Game1.player.CanMove = false;
                timerField.SetValue(timerField.GetValue() + time.ElapsedGameTime.Milliseconds);
                Game1.fadeToBlackAlpha = 1f;

                if (timerField.GetValue() < 1500)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                Game1.playSound("smallSelect");
                Game1.player.getSpouse().daysAfterLastBirth = 5;
                Game1.player.GetSpouseFriendship().NextBirthingDate = (WorldDate)null;
                var dialogueOptions = new[] { "NewChild_FirstChild", "NewChild_Adoption" };
                var chosenDialogue = dialogueOptions[Game1.random.Next(0, dialogueOptions.Length)];


                var locationBeingChecked = _locationChecker.IsLocationMissing(FIRST_BABY) ? FIRST_BABY : SECOND_BABY;
                var scoutedItem = _archipelago.ScoutStardewLocation(locationBeingChecked);
                var scoutedItemName = scoutedItem.ItemName;
                var marriageDialogue = new MarriageDialogueReference("Data\\ExtraDialogue", chosenDialogue, true, scoutedItemName);
                Game1.player.getSpouse().currentMarriageDialogue.Insert(0, marriageDialogue);

                var playerName = Lexicon.capitalize(Game1.player.Name);
                var spouseName = Game1.player.spouse;
                var pronoun = "it";
                var scoutedItemClassification = $"{scoutedItem.GetClassificationString()} item";

                var multiplayerField = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
                var multiplayer = multiplayerField.GetValue();

                // "Baby" => "{0} and {1} welcomed a baby {2} to the family! They named {3} {4}."
                Game1.morningQueue.Enqueue(() => multiplayer.globalChatInfoMessage("Baby", playerName, spouseName, scoutedItemClassification, pronoun, scoutedItemName));

                // I think the following lines aren't necessary as I have prevented the keyboard dialogue from showing up in the first place. If things break, try uncommenting it.
                // if (Game1.keyboardDispatcher != null) Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)null;
                Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).GetPlayerBedSpot()) * 64f;
                Game1.globalFadeToClear();

                _locationChecker.AddCheckedLocation(locationBeingChecked);

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TickUpdate_BirthingEvent_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
