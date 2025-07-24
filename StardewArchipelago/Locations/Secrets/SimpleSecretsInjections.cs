using System;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.Locations.Secrets
{
    public class SimpleSecretsInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual void proceedToNextScene()
        public static bool ProceedToNextScene_ForsakenSouls_Prefix(TV __instance)
        {
            try
            {
                if (GetCurrentChannelField(__instance).GetValue() == 666)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.FREE_THE_FORSAKEN_SOULS);
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ProceedToNextScene_ForsakenSouls_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static IReflectedField<int> GetCurrentChannelField(TV tv)
        {
            // private int currentChannel;
            return _modHelper.Reflection.GetField<int>(tv, "currentChannel");
        }

        // public override void DayUpdate()
        public static void DayUpdate_SomethingForSanta_Postfix(Furniture __instance)
        {
            try
            {
                if (!Game1.IsMasterGame || Game1.season != Season.Winter || Game1.dayOfMonth != 25 || (__instance.furniture_type.Value != 11 && __instance.furniture_type.Value != 5) || __instance.heldObject.Value == null)
                {
                    return;
                }

                if (!Game1.player.mailReceived.Contains($"CookiePresent_year{Game1.year}") && !Game1.player.mailReceived.Contains($"MilkPresent_year{Game1.year}"))
                {
                    return;
                }

                if (__instance.heldObject.Value.QualifiedItemId != "(O)MysteryBox")
                {
                    return;
                }

                if (_locationChecker.IsLocationMissing(SecretsLocationNames.SOMETHING_FOR_SANTA))
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.SOMETHING_FOR_SANTA);
                    __instance.heldObject.Value = ItemRegistry.Create<Object>("(O)MysteryBox");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_SomethingForSanta_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_LonelyStone_Postfix(MapPage __instance, int x, int y, bool playSound)
        {
            try
            {
                foreach (var clickableComponent in __instance.points.Values)
                {
                    if (!clickableComponent.containsPoint(x, y))
                    {
                        continue;
                    }
                    var name = clickableComponent.name;
                    if (name == "Beach/LonelyStone")
                    {
                        _locationChecker.AddCheckedLocation(SecretsLocationNames.ACKNOWLEDGE_THE_LONELY_STONE);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_LonelyStone_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void draw(SpriteBatch b)
        public static void Draw_JungleJunimo_Postfix(SkillsPage __instance, SpriteBatch b)
        {
            try
            {
                if (Game1.player.hasCompletedCommunityCenter())
                {
                    return;
                }


                var allAreasRewardsReceived = HasReceivedAllCCRewards();
                if (allAreasRewardsReceived)
                {
                    DrawJunimo(__instance, b);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_JungleJunimo_Postfix)}:\n{ex}");
                return;
            }
        }
        private static bool HasReceivedAllCCRewards()
        {
            var allAreasRewardsReceived =
                Game1.player.hasOrWillReceiveMail("ccPantry") &&
                Game1.player.hasOrWillReceiveMail("ccCraftsRoom") &&
                Game1.player.hasOrWillReceiveMail("ccFishTank") &&
                Game1.player.hasOrWillReceiveMail("ccBoilerRoom") &&
                Game1.player.hasOrWillReceiveMail("ccVault") &&
                Game1.player.hasOrWillReceiveMail("ccBulletin");
            return allAreasRewardsReceived;
        }
        private static void DrawJunimo(SkillsPage skillsPage, SpriteBatch b)
        {

            var timesClickedJunimoField = _modHelper.Reflection.GetField<int>(skillsPage, "timesClickedJunimo");
            var timesClickedJunimo = timesClickedJunimoField.GetValue();
            var num9 = skillsPage.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)(skillsPage.height / 2.0) + 21;
            var num10 = skillsPage.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2;
            var x4 = num10 + 80;
            var y2 = num9 + 16;
            b.Draw(Game1.mouseCursors_1_6, new Vector2(x4 - 4 + 30f, y2 + 52 + 30f), new Rectangle(386, 299, 13, 15), Color.White, 0.0f, new Vector2(7.5f), (float)(4.0 + timesClickedJunimo * 0.20000000298023224), SpriteEffects.None, 0.7f);
            if (Game1.player.mailReceived.Contains("activatedJungleJunimo"))
            {
                b.Draw(Game1.mouseCursors_1_6, new Vector2(x4 - 80, y2 - 16), new Rectangle(311, 251, 51, 48), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static bool ReceiveLeftClick_JungleJunimo_Prefix(SkillsPage __instance, int x, int y, bool playSound)
        {
            try
            {
                // private int timesClickedJunimo;
                var timesClickedJunimoField = _modHelper.Reflection.GetField<int>(__instance, "timesClickedJunimo");

                var isInJunimoArea = x > __instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 && x < __instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 + 200 &&
                                     y > __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)(__instance.height / 2.0) + 21 && y < __instance.yPositionOnScreen + __instance.height;
                if (isInJunimoArea && HasReceivedAllCCRewards() && !Game1.MasterPlayer.hasOrWillReceiveMail("JojaMember") && !Game1.player.mailReceived.Contains("activatedJungleJunimo"))
                {
                    timesClickedJunimoField.SetValue(timesClickedJunimoField.GetValue() + 1);
                    if (timesClickedJunimoField.GetValue() > 6)
                    {
                        Game1.playSound("discoverMineral");
                        Game1.playSound("leafrustle");
                        Game1.player.mailReceived.Add("activatedJungleJunimo");
                        _locationChecker.AddCheckedLocation(SecretsLocationNames.JUNGLE_JUNIMO);
                    }
                    else
                        Game1.playSound("hammer");
                }

                foreach (ClickableComponent ccTrackerButton in __instance.ccTrackerButtons)
                {
                    if (ccTrackerButton != null && ccTrackerButton.containsPoint(x, y) && !ccTrackerButton.label.Equals("???"))
                    {
                        Game1.activeClickableMenu = new ArchipelagoJunimoNoteMenu(true, Convert.ToInt32(ccTrackerButton.name), true)
                        {
                            GameMenuTabToReturnTo = GameMenu.skillsTab
                        };
                        break;
                    }
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_JungleJunimo_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_SecretActions_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (who == null || !who.IsLocalPlayer || __instance.ShouldIgnoreAction(action, who, tileLocation))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
                if (!ArgUtility.TryGet(action, 0, out var key1, out var error, name: "string actionType") || key1 == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (key1.Equals("HMTGF"))
                {
                    CheckSecretStatueLocation(__instance, who, QualifiedItemIds.SUPER_CUCUMBER, SecretsLocationNames.HMTGF, "discoverMineral");

                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (key1.Equals("Starpoint"))
                {
                    if (!ArgUtility.TryGet(action, 1, out var which, out error, name: "string which"))
                    {
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }

                    if (which == "3")
                    {
                        CheckSecretStatueLocation(__instance, who, QualifiedItemIds.DUCK_MAYONNAISE, SecretsLocationNames.PINKY_LEMON, "discoverMineral");
                    }
                    else if (which == "4")
                    {
                        CheckSecretStatueLocation(__instance, who, QualifiedItemIds.STRANGE_BUN, SecretsLocationNames.FOROGUEMON, "croak");
                    }
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (key1.Equals("DwarfGrave") && who.canUnderstandDwarves)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.GALAXIES_WILL_HEED_YOUR_CRY);
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_DwarfGrave_Translated").Replace('\n', '^'));
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_SecretActions_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckSecretStatueLocation(GameLocation gameLocation, Farmer who, string itemId, string locationName, string sound)
        {

            if (who.ActiveObject != null && who.ActiveObject.QualifiedItemId == itemId)
            {
                _locationChecker.AddCheckedLocation(locationName);
                Game1.player.reduceActiveItemByOne();
                gameLocation.localSound(sound);
                Game1.flashAlpha = 1f;
            }
        }

        // public void addItemByMenuIfNecessary(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
        public static bool AddItemByMenuIfNecessary_FarAwayStone_Prefix(Farmer __instance, Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback, bool forceQueue)
        {
            try
            {
                if (!item.QualifiedItemId.Equals(QualifiedItemIds.FAR_AWAY_STONE))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _locationChecker.AddCheckedLocation(SecretsLocationNames.SUMMON_BONE_SERPENT);
                if (_archipelago.HasReceivedItem("Far Away Stone"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddItemByMenuIfNecessary_FarAwayStone_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_Meowmere_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (!ArgUtility.TryGet(args, 1, out var itemId, out var error, name: "string itemId"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var lower = itemId.ToLower();
                if (lower != "meowmere")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (_archipelago.GetReceivedItemCount("Progressive Weapon") >= 2 || _archipelago.GetReceivedItemCount("Progressive Sword") >= 2)
                {
                    Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create(QualifiedItemIds.MEOWMERE));
                }

                _locationChecker.AddCheckedLocation(SecretsLocationNames.MEOWMERE);

                if (Game1.activeClickableMenu == null)
                {
                    ++@event.CurrentCommand;
                }
                ++@event.CurrentCommand;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_Meowmere_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void playElliottPiano(int key)
        public static bool PlayElliottPiano_FamiliarTune_Prefix(GameLocation __instance, int key)
        {
            try
            {
                if (Game1.elliottPiano >= 7 && key == 1)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.A_FAMILIAR_TUNE);
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlayElliottPiano_FamiliarTune_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public StardewValley.Dialogue TryGetDialogue(string key)
        public static void TryGetDialogue_MonstersInHouse_Postfix(NPC __instance, string key, ref Dialogue __result)
        {
            try
            {
                if (key != "Spouse_MonstersInHouse")
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(SecretsLocationNames.FLUBBER_EXPERIMENT);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryGetDialogue_MonstersInHouse_Postfix)}:\n{ex}");
                return;
            }
        }

        // private void optionButtonClick(string name)
        public static bool OptionButtonClick_NameEasterEggs_Prefix(CharacterCustomization __instance, string name)
        {
            try
            {
                if (name is not "OK" || !__instance.canLeaveMenu() || Game1.player?.Name == null || Game1.player.Name == __instance.oldName)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var num1 = Game1.player.Name.IndexOf("[", StringComparison.InvariantCulture);
                var num2 = Game1.player.Name.IndexOf("]", StringComparison.InvariantCulture);
                if (num1 < 0 || num2 <= num1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var displayName = ItemRegistry.GetData(Game1.player.Name.Substring(num1 + 1, num2 - num1 - 1))?.DisplayName;
                if (displayName != null)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.SEEMS_FISHY);
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OptionButtonClick_NameEasterEggs_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void addNiceTryEasterEggMessage()
        public static void AddNiceTryEasterEggMessage_NiceTry_Postfix(ChatBox __instance)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.NICE_TRY);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddNiceTryEasterEggMessage_NiceTry_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static void ConcernedApe(string[] command, ChatBox chat)
        public static void ConcernedApe_EnjoyNewLifeHere_Postfix(string[] command, ChatBox chat)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.ENJOY_YOUR_NEW_LIFE_HERE);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ConcernedApe_EnjoyNewLifeHere_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static void Qi(string[] command, ChatBox chat)
        public static void Qi_WhatdYouExpect_Postfix(string[] command, ChatBox chat)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.WHAT_YOU_EXPECT);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Qi_WhatdYouExpect_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void KaitoKidChatCommand(string[] command, ChatBox chat)
        {
            var mailKeyGotEasterEgg = $"KaitoChat_{Game1.stats.DaysPlayed}";
            var mailKeyGotHint = $"KaitoChatHint_{Game1.stats.DaysPlayed}";
            var color = new Color(104, 214, byte.MaxValue);
            if (Game1.player.mailReceived.Add(mailKeyGotEasterEgg))
            {
                chat.addMessage("Are you getting used to your new life here in this randomizer? If so, try harder settings!", color);
                _locationChecker.AddCheckedLocation(SecretsLocationNames.ENJOY_YOUR_NEW_LIFE_HERE);
            }
            else if (Game1.player.mailReceived.Add(mailKeyGotHint))
            {
                var random = new Random((int)Game1.stats.DaysPlayed * 2);
                var missingLocations = _archipelago.GetAllMissingLocations().ToArray();
                var maxRetries = Math.Max(10, missingLocations.Length / 10);
                ScoutedLocation scout = null;
                while (maxRetries > 0 && (scout == null || !scout.ClassificationFlags.HasFlag(ItemFlags.Advancement)))
                {
                    var randomLocation = missingLocations[random.Next(missingLocations.Length)];
                    scout = _archipelago.ScoutSingleLocation(randomLocation);
                    maxRetries--;
                }
                if (scout != null)
                {
                    chat.addMessage($"You might be able to find {scout.ItemName} at {scout.LocationName}", color);
                }
                else
                {
                    chat.addMessage($"Kaito Kid is procrastinating...", color);
                }
            }
            else
            {
                chat.addMessage("But don't tell anyone where you got this info!", color);
            }

        }

        // public virtual StardewValley.Dialogue GetGiftReaction(Farmer giver, Object gift, int taste)
        public static void GetGiftReaction_SpecialDialogues_Postfix(NPC __instance, Farmer giver, Object gift, int taste, ref Dialogue __result)
        {
            try
            {
                if (!__instance.CanReceiveGifts() || !Game1.NPCGiftTastes.TryGetValue(__instance.Name, out _) || __instance.isBirthday())
                {
                    return;
                }

                if (__instance.Name == NPCNames.KROBUS && Game1.Date.DayOfWeek == DayOfWeek.Friday)
                {
                    return;
                }

                CheckGiftDialogueLocation(__instance, gift, SecretsLocationNames.WHAT_KIND_OF_MONSTER_IS_THIS, NPCNames.WILLY, QualifiedItemIds.MUTANT_CARP);
                CheckGiftDialogueLocation(__instance, gift, SecretsLocationNames.MOUTH_WATERING_ALREADY, NPCNames.ABIGAIL, QualifiedItemIds.MAGIC_ROCK_CANDY);
                CheckGiftDialogueLocation(__instance, gift, SecretsLocationNames.LOVELY_PERFUME, NPCNames.KROBUS, QualifiedItemIds.MONSTER_MUSK);
                CheckGiftDialogueLocation(__instance, gift, SecretsLocationNames.WHERE_DOES_THIS_JUICE_COME_FROM, NPCNames.DWARF, QualifiedItemIds.MILK, QualifiedItemIds.LARGE_MILK, QualifiedItemIds.GOAT_MILK, QualifiedItemIds.LARGE_GOAT_MILK);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetGiftReaction_SpecialDialogues_Postfix)}:\n{ex}");
                return;
            }
        }
        private static void CheckGiftDialogueLocation(NPC __instance, Object gift, string locationName, string npcName, params string[] validGifts)
        {
            if (__instance.Name == npcName && validGifts.Contains(gift.QualifiedItemId))
            {
                _locationChecker.AddCheckedLocation(locationName);
            }
        }
    }
}
