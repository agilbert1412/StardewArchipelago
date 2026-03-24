using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Quests;

namespace StardewArchipelago.GameModifications.Tooltips
{
    public class BillboardInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ModConfig _config;
        private static ArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;
        private static Friends _friends;
        private static Texture2D _bigArchipelagoIcon;
        private static Texture2D _miniArchipelagoIcon;
        private static Texture2D _travelingMerchantIcon;
        public static ClickableComponent _rerollButton;
        private const string REROLL_TEXT = "Reroll";

        public static void Initialize(LogHandler logger, IModHelper modHelper, ModConfig config, ArchipelagoClient archipelago, StardewLocationChecker locationChecker, Friends friends)
        {
            _logger = logger;
            _modHelper = modHelper;
            _config = config;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _friends = friends;

            var desiredTextureName = ArchipelagoTextures.COLOR;
            _bigArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(48, desiredTextureName);
            _miniArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(24, desiredTextureName);
            _travelingMerchantIcon = TexturesLoader.GetTexture("traveling_merchant.png");
        }

        // public Billboard(bool dailyQuest = false)
        public static void BillboardConstructor_InitializeReroll_Postfix(ref Billboard __instance, bool dailyQuest)
        {
            try
            {
                if (!dailyQuest)
                {
                    return;
                }

                var buttonWidth = (int)Game1.dialogueFont.MeasureString(REROLL_TEXT).X + 24;
                var buttonHeight = (int)Game1.dialogueFont.MeasureString(REROLL_TEXT).Y + 24;
                var buttonX = __instance.xPositionOnScreen + __instance.width * 2 / 4 - (buttonWidth / 2);
                var buttonY = __instance.yPositionOnScreen + (__instance.height / 16) - 24;
                var bounds = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
                _rerollButton = new ClickableComponent(bounds, "")
                {
                    myID = 111,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998
                };
                UpdateRerollButtonVisibility();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BillboardConstructor_InitializeReroll_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void UpdateRerollButtonVisibility()
        {
            _rerollButton.visible = Game1.CanAcceptDailyQuest();
        }

        // public override void draw(SpriteBatch b)
        public static void Draw_AddArchipelagoIndicatorsAndReroll_Postfix(Billboard __instance, SpriteBatch b)
        {
            try
            {
                // private bool dailyQuestBoard;
                var dailyQuestBoard = _modHelper.Reflection.GetField<bool>(__instance, "dailyQuestBoard").GetValue();
                if (dailyQuestBoard)
                {
                    DrawRerollButton(__instance, b);
                    DrawDailyQuestIndicator(__instance, b);
                }
                else
                {
                    DrawCalendarIndicators(__instance, b);
                }

                __instance.drawMouse(b);
                // private string hoverText = "";
                var hoverTextField = _modHelper.Reflection.GetField<string>(__instance, "hoverText");
                var hoverText = hoverTextField.GetValue();

                if (hoverText.Length > 0)
                {
                    IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddArchipelagoIndicatorsAndReroll_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawRerollButton(Billboard billboard, SpriteBatch spriteBatch)
        {
            if (_rerollButton.visible)
            {
                IClickableMenu.drawTextureBox(spriteBatch, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), _rerollButton.bounds.X, _rerollButton.bounds.Y, _rerollButton.bounds.Width, _rerollButton.bounds.Height, _rerollButton.scale > 1.0 ? Color.LightPink : Color.White, 4f * _rerollButton.scale);
                Utility.drawTextWithShadow(spriteBatch, REROLL_TEXT, Game1.dialogueFont, new Vector2(_rerollButton.bounds.X + 12, _rerollButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
            }
        }

        private static void DrawDailyQuestIndicator(Billboard billBoard, SpriteBatch spriteBatch)
        {
            var quest = Game1.questOfTheDay;
            if (quest?.currentObjective == null || quest.currentObjective.Length == 0)
            {
                return;
            }

            var dailyQuestCheckName = GetDailyQuestCheckName(quest);

            if (string.IsNullOrWhiteSpace(dailyQuestCheckName) || !_locationChecker.GetAllLocationsNotCheckedContainingWord(dailyQuestCheckName).Any())
            {
                return;
            }

            var size = 48;
            var position1 = new Vector2(billBoard.acceptQuestButton.bounds.X - size - 12, billBoard.acceptQuestButton.bounds.Y + 12);
            var position2 = new Vector2(billBoard.acceptQuestButton.bounds.X + billBoard.acceptQuestButton.bounds.Width + 12, billBoard.acceptQuestButton.bounds.Y + 12);
            var sourceRectangle = new Rectangle(0, 0, size, size);
            var color = Color.White;
            spriteBatch.Draw(_bigArchipelagoIcon, position1, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.Draw(_bigArchipelagoIcon, position2, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }

        private static string GetDailyQuestCheckName(Quest quest)
        {
            return quest.questType.Value switch
            {
                (int)QuestType.ItemDelivery => string.Format(DailyQuest.HELP_WANTED, DailyQuest.ITEM_DELIVERY),
                (int)QuestType.SlayMonsters => string.Format(DailyQuest.HELP_WANTED, DailyQuest.SLAY_MONSTERS),
                (int)QuestType.Fishing => string.Format(DailyQuest.HELP_WANTED, DailyQuest.FISHING),
                (int)QuestType.ResourceCollection => string.Format(DailyQuest.HELP_WANTED, DailyQuest.GATHERING),
                _ => "",
            };
        }

        private static void DrawCalendarIndicators(Billboard billBoard, SpriteBatch spriteBatch)
        {
            var calendarDays = billBoard.calendarDays;
            var birthdays = billBoard.GetBirthdays();
            for (var i = 0; i < calendarDays.Count; i++)
            {
                var birthdaysToday = birthdays.GetValueOrDefault(i + 1) ?? new List<NPC>();
                DrawAPIconIfNeeded(birthdaysToday, spriteBatch, calendarDays, i);
                DrawTravelingMerchantIconIfNeeded(spriteBatch, calendarDays, i);
            }
        }

        private static void DrawAPIconIfNeeded(List<NPC> birthdaysToday, SpriteBatch b, List<ClickableTextureComponent> calendarDays, int index)
        {
            if (!_config.ShowCalendarIndicators)
            {
                return;
            }

            var day = index + 1;
            var dayComponent = calendarDays[index];

            if (!GetMissingFestivalChecks(day).Any() && !GetMissingNpcChecks(birthdaysToday).Any() && !GetMissingTravelingCartChecks(day).Any() && !GetMissingSecretsChecks(day).Any())
            {
                return;
            }

            var calendarDayPosition = new Vector2(dayComponent.bounds.X, dayComponent.bounds.Y);
            var logoPosition = calendarDayPosition + new Vector2(dayComponent.bounds.Width - 24 - 4, 4f);
            var sourceRectangle = new Rectangle(0, 0, 24, 24);
            b.Draw(_miniArchipelagoIcon, logoPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }

        private static void DrawTravelingMerchantIconIfNeeded(SpriteBatch b, List<ClickableTextureComponent> calendarDays, int index)
        {
            var day = index + 1;
            var dayOfWeek = Days.GetDayOfWeekName(day);
            var merchantDayItem = string.Format(TravelingMerchantInjections.AP_MERCHANT_DAYS, dayOfWeek);
            if (!_archipelago.HasReceivedItem(merchantDayItem))
            {
                return;
            }

            var calendarDayPosition = new Vector2(calendarDays[index].bounds.X, calendarDays[index].bounds.Y);
            var logoPosition = calendarDayPosition + new Vector2(4, calendarDays[index].bounds.Height - 24 - 4);
            var sourceRectangle = new Rectangle(0, 0, 12, 12);
            b.Draw(_travelingMerchantIcon, logoPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
        }

        // public override void performHoverAction(int x, int y)
        public static void PerformHoverAction_RerollButtonAndTooltips_Postfix(Billboard __instance, int x, int y)
        {
            try
            {
                // private bool dailyQuestBoard;
                if (_modHelper.Reflection.GetField<bool>(__instance, "dailyQuestBoard").GetValue())
                {
                    PerformHoverActionRerollButton(__instance, x, y);
                }
                else
                {
                    PerformHoverActionCalendarTooltips(__instance, x, y);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformHoverAction_RerollButtonAndTooltips_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool PerformHoverActionCalendarTooltips(Billboard __instance, int x, int y)
        {
            if (!_config.ShowCalendarIndicators)
            {
                return true;
            }

            // private string hoverText = "";
            var hoverTextField = _modHelper.Reflection.GetField<string>(__instance, "hoverText");
            var hoverText = hoverTextField.GetValue();

            var birthdays = __instance.GetBirthdays();

            for (var i = 0; i < __instance.calendarDays.Count; i++)
            {
                var day = i + 1;
                if (!__instance.calendarDays[i].bounds.Contains(x, y))
                {
                    continue;
                }

                var birthdaysToday = birthdays.GetValueOrDefault(day) ?? new List<NPC>();

                var missingFestivalChecks = GetMissingFestivalChecks(day);
                var missingNpcChecks = GetMissingNpcChecks(birthdaysToday);
                var missingCartChecks = GetMissingTravelingCartChecks(day);
                var missingSecretsChecks = GetMissingSecretsChecks(day);

                foreach (var location in missingFestivalChecks)
                {
                    hoverText += $"{Environment.NewLine}- {location}";
                }

                foreach (var location in missingNpcChecks)
                {
                    hoverText += $"{Environment.NewLine}- {location.TurnHeartsIntoStardewHearts()}";
                }

                foreach (var location in missingCartChecks)
                {
                    hoverText += $"{Environment.NewLine}- {location}";
                }

                foreach (var location in missingSecretsChecks)
                {
                    hoverText += $"{Environment.NewLine}- {location}";
                }
            }

            hoverTextField.SetValue(hoverText.Trim());
            return false;
        }

        // public override void performHoverAction(int x, int y)
        public static void PerformHoverActionRerollButton(Billboard billboard, int x, int y)
        {
            if (!_rerollButton.visible)
            {
                return;
            }
            var scale1 = _rerollButton.scale;
            _rerollButton.scale = _rerollButton.bounds.Contains(x, y) ? 1.5f : 1f;
            if (_rerollButton.scale > (double)scale1)
            {
                Game1.playSound("Cowboy_gunshot");
            }
            return;
        }

        private static IEnumerable<string> GetMissingFestivalChecks(int day)
        {
            var season = Game1.currentSeason;
            var festivalIdentifier = FestivalLocationNames.FestivalIdentifier(Game1.season, day);

            if (!FestivalLocationNames.LocationsByFestival.ContainsKey(festivalIdentifier))
            {
                yield break;
            }

            var festivalLocations = FestivalLocationNames.LocationsByFestival[festivalIdentifier];
            foreach (var location in festivalLocations)
            {
                if (_locationChecker.IsLocationMissing(location))
                {
                    yield return location;
                }
            }
        }

        private static IEnumerable<string> GetMissingNpcChecks(List<NPC> birthdaysToday)
        {
            foreach (var npc in birthdaysToday)
            {
                var friend = _friends.GetFriend(npc.Name);
                if (friend == null)
                {
                    continue;
                }

                foreach (var location in _locationChecker.GetAllLocationsNotCheckedContainingWord($"Friendsanity: {friend.ArchipelagoName}"))
                {
                    yield return location;
                }
            }
        }

        private static IEnumerable<string> GetMissingTravelingCartChecks(int day)
        {
            var dayOfWeek = Days.GetDayOfWeekName(day);
            var merchantDayItem = string.Format(TravelingMerchantInjections.AP_MERCHANT_DAYS, dayOfWeek);
            if (!_archipelago.HasReceivedItem(merchantDayItem))
            {
                return Enumerable.Empty<string>();
            }

            var merchantItemPatern = $"Traveling Merchant {dayOfWeek} Item";
            return _locationChecker.GetAllLocationsNotCheckedContainingWord(merchantItemPatern);
        }

        private static IEnumerable<string> GetMissingSecretsChecks(int day)
        {
            var season = Game1.season;
            foreach (var secretDate in SecretsLocationNames.SECRET_DATES)
            {
                var secretName = secretDate.Name;
                var secretSeasons = secretDate.Seasons;
                var secretDays = secretDate.Days;

                if (!secretSeasons.Contains(season) || !secretDays.Contains(day))
                {
                    continue;
                }


                if (_locationChecker.IsLocationMissing(secretName))
                {
                    yield return secretName;
                }
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_ClickRerollButton_Postfix(Billboard __instance, int x, int y, bool playSound)
        {
            try
            {
                if (_rerollButton.visible && _rerollButton.containsPoint(x, y))
                {
                    Game1.playSound("newArtifact");
                    HelpWantedQuestInjections.IncrementRerollCount();
                    Game1.RefreshQuestOfTheDay();
                    __instance.UpdateDailyQuestButton();
                }

                UpdateRerollButtonVisibility();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_ClickRerollButton_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
