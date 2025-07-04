using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Secrets
{
    public class DifficultSecretsInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ScoutedLocation _stackMasterScout;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _stackMasterScout = _archipelago.ScoutSingleLocation(SecretsLocationNames.SECRET_IRIDIUM_STACKMASTER_TROPHY, false);
        }

        public static void DoneEatingStardropSecret(Farmer __instance)
        {
            try
            {
                var itemToEat = __instance.itemToEat as Object;
                if (itemToEat.QualifiedItemId != QualifiedItemIds.STARDROP)
                {
                    return;
                }

                var triggerWords = new string[] { "Kaito", "ConcernedApe", "CA" };
                if (triggerWords.Any(x => __instance.favoriteThing.Value.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.THANK_THE_DEVS);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneEatingStardropSecret)}:\n{ex}");
                return;
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_AnnoyMoonMan_Postfix(ShippingMenu __instance, int x, int y, bool playSound)
        {
            try
            {
                // private int timesPokedMoon;
                var timesPokedMoonField = _modHelper.Reflection.GetField<int>(__instance, "timesPokedMoon");
                if (timesPokedMoonField.GetValue() > 10)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.ANNOY_THE_MOON_MAN);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_AnnoyMoonMan_Postfix)}:\n{ex}");
                return;
            }
        }

        // private void PlaySound(string sound)
        public static void PlaySound_StrangeSightingAndBigFoot_Postfix(TemporaryAnimatedSprite __instance, string sound)
        {
            try
            {
                var bigfootTexture = "Characters\\asldkfjsquaskutanfsldk";
                var strangeTexture = "LooseSprites\\temporary_sprites_1";

                if (__instance.textureName == strangeTexture)
                {

                    var spriteRectangle = new Rectangle(448, 546, 16, 25);
                    if (!__instance.sourceRect.Equals(spriteRectangle))
                    {
                        return;
                    }

                    _locationChecker.AddCheckedLocation(SecretsLocationNames.STRANGE_SIGHTING);
                    return;
                }

                if (__instance.textureName == bigfootTexture)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.BIGFOOT);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlaySound_StrangeSightingAndBigFoot_Postfix)}:\n{ex}");
                return;
            }
        }

        // public SeaMonsterTemporarySprite(float animationInterval, int animationLength, int numberOfLoops, Vector2 position)
        public static void SeaMonsterTemporarySpriteConstructor_SeaMonsterSighting_Postfix(SeaMonsterTemporarySprite __instance, float animationInterval, int animationLength, int numberOfLoops, Vector2 position)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.SEA_MONSTER_SIGHTING);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SeaMonsterTemporarySpriteConstructor_SeaMonsterSighting_Postfix)}:\n{ex}");
                return;
            }
        }

        // private void showQiCheatingEvent()
        public static void ShowQiCheatingEvent_MeMeMeMeMeMeMeMeMeMeMeMeMeMeMeMe_Postfix(Summit __instance)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.ME_ME_ME_ME);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShowQiCheatingEvent_MeMeMeMeMeMeMeMeMeMeMeMeMeMeMeMe_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static void numbersEasterEgg(int number)
        public static void NumbersEasterEgg_StackMasterTrophy_Postfix(int number)
        {
            try
            {
                if (_stackMasterScout == null)
                {
                    _stackMasterScout = _archipelago.ScoutSingleLocation(SecretsLocationNames.SECRET_IRIDIUM_STACKMASTER_TROPHY, false);
                }
                if (number > 250000 && !Game1.player.mailReceived.Contains("numbersEggFreeScout"))
                {
                    Game1.player.mailReceived.Add("numbersEggFreeScout");
                    Game1.chatBox.addMessage("Let me give you a little hint...", new Color(100, 50, (int)byte.MaxValue));
                    Game1.chatBox.addMessage($"I wonder if {_stackMasterScout.PlayerName} really needs their {_stackMasterScout.ItemName}?", new Color(100, 50, (int)byte.MaxValue));
                }
                if (number >= 1000000)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_IRIDIUM_STACKMASTER_TROPHY);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(NumbersEasterEgg_StackMasterTrophy_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
