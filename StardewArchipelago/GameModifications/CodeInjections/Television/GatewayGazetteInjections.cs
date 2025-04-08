using System;
using System.IO;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.GameModifications.CodeInjections.Television
{
    internal class GatewayGazetteInjections
    {
        private const int GAZETTE_CHANNEL = 1492;
        private const string GAZETTE_INTRO =
            "Welcome back to the Gateway Gazette! The bi-weekly show where brave adventurers explore the strange topology of the world around us!";
        private const string GAZETTE_EPISODE =
            "On today's episode, our agent {0} has traversed from {1} and discovered... {2}! They came back safe and sound to share this wonderful knowledge with us!";
        private const string GAZETTE_CHAOS_EPISODE =
            "On today's episode, our agent {0} was sent to explore... but we haven't heard back from them. Let's send them thoughts and prayers! Don't walk outside unprepared, kids!";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static EntranceManager _entranceManager;
        private static ArchipelagoStateDto _state;

        private static Texture2D _gazetteTexture;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, EntranceManager entranceManager, ArchipelagoStateDto state)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _entranceManager = entranceManager;
            _state = state;

            _gazetteTexture = TexturesLoader.GetTexture(logger, _modHelper, Path.Combine("Gazette", "gazette_all.png"));
        }

        private static IReflectedField<int> GetCurrentChannelField(TV tv)
        {
            // private int currentChannel;
            return _modHelper.Reflection.GetField<int>(tv, "currentChannel");
        }

        // public virtual void selectChannel(Farmer who, string answer)
        public static void SelectChannel_SelectGatewayGazetteChannel_Postfix(TV __instance, Farmer who, string answer)
        {
            try
            {
                var channel = answer.Split(' ')[0];
                if (channel != TVInjections.GATEWAY_GAZETTE_KEY)
                {
                    return;
                }

                var currentChannelField = GetCurrentChannelField(__instance);
                currentChannelField.SetValue(GAZETTE_CHANNEL);

                SetGazetteIntroScreen(__instance);

                Game1.drawObjectDialogue(Game1.parseText(GAZETTE_INTRO));
                Game1.afterDialogues = __instance.proceedToNextScene;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SelectChannel_SelectGatewayGazetteChannel_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual void proceedToNextScene()
        public static void ProceedToNextScene_GatewayGazette_Postfix(TV __instance)
        {
            try
            {
                var currentChannelField = GetCurrentChannelField(__instance);
                if (currentChannelField.GetValue() != GAZETTE_CHANNEL)
                {
                    return;
                }

                // private TemporaryAnimatedSprite screenOverlay;
                var screenOverlayField = _modHelper.Reflection.GetField<TemporaryAnimatedSprite>(__instance, "screenOverlay");
                if (screenOverlayField.GetValue() == null)
                {
                    PlayGazetteEpisode(__instance);
                    screenOverlayField.SetValue(new TemporaryAnimatedSprite { alpha = 1E-07f });
                }
                else
                {
                    __instance.turnOffTV();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ProceedToNextScene_GatewayGazette_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void PlayGazetteEpisode(TV tv)
        {
            SetGazetteEpisodeScreen(tv);

            var random = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
            if (_archipelago.SlotData.EntranceRandomization == EntranceRandomization.Chaos)
            {
                PlayChaosGazetteEpisode(tv, random);
                return;
            }

            var agentName = Community.AllNames[random.Next(Community.AllNames.Length)];
            var entrancesNotChecked = _entranceManager.ModifiedEntrances.Keys.Where(x => !_state.EntrancesTraversed.Any(y => y.Equals(x, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            if (!entrancesNotChecked.Any())
            {
                entrancesNotChecked = _entranceManager.ModifiedEntrances.Keys.ToArray();
            }
            var entranceToReveal = entrancesNotChecked[random.Next(entrancesNotChecked.Length)];
            var friendlyEntranceName = GetFriendlyMapName(entranceToReveal);
            var destinationInternalName = _entranceManager.ModifiedEntrances[entranceToReveal];
            var destinationFriendlyName = GetFriendlyDestinationName(destinationInternalName);
            Game1.drawObjectDialogue(Game1.parseText(string.Format(GAZETTE_EPISODE, agentName, friendlyEntranceName, destinationFriendlyName)));
            Game1.afterDialogues = tv.proceedToNextScene;
        }

        private static void PlayChaosGazetteEpisode(TV tv, Random random)
        {
            var agentName = Community.AllNames[random.Next(Community.AllNames.Length)];
            Game1.drawObjectDialogue(Game1.parseText(string.Format(GAZETTE_CHAOS_EPISODE, agentName)));
            Game1.afterDialogues = tv.proceedToNextScene;
        }

        private static string GetFriendlyDestinationName(string destinationInternalName)
        {
            var friendlyDestinationName = destinationInternalName.Split(EntranceManager.TRANSITIONAL_STRING).Last().Split(" ").Last().Split("|").First();
            return GetFriendlyMapName(friendlyDestinationName);
        }

        private static string GetFriendlyMapName(string mapName)
        {
            var friendlyMapName = mapName.Replace("Custom_", "");
            for (var i = friendlyMapName.Length - 1; i > 0; i--)
            {
                if (char.IsUpper(friendlyMapName[i]) && char.IsLetter(friendlyMapName[i]) && !char.IsWhiteSpace(friendlyMapName[i - 1]))
                {
                    friendlyMapName = friendlyMapName.Insert(i, " ");
                }
                if (friendlyMapName[i] == '|')
                {
                    do
                    {
                        friendlyMapName = friendlyMapName.Remove(i, 1);
                    } while (i < friendlyMapName.Length && char.IsDigit(friendlyMapName[i]));
                }
            }
            return friendlyMapName;
        }

        private static void SetGazetteIntroScreen(TV __instance)
        {
            // private TemporaryAnimatedSprite screen;
            var screenField = _modHelper.Reflection.GetField<TemporaryAnimatedSprite>(__instance, "screen");
            var tvSprite = CreateGatewayGazetteTvSprite(__instance, true);
            screenField.SetValue(tvSprite);
        }

        private static void SetGazetteEpisodeScreen(TV __instance)
        {
            // private TemporaryAnimatedSprite screen;
            var screenField = _modHelper.Reflection.GetField<TemporaryAnimatedSprite>(__instance, "screen");
            var tvSprite = CreateGatewayGazetteTvSprite(__instance, false);
            screenField.SetValue(tvSprite);
        }

        private static TemporaryAnimatedSprite CreateGatewayGazetteTvSprite(TV tv, bool intro)
        {
            var screenRectangle = new Rectangle(intro ? 0 : 42, 0, 42, 28);
            var interval = 2500f;
            var length = intro ? 1 : 2;
            var loops = 999999;
            var screenPosition = tv.getScreenPosition();
            var flicker = false;
            var flipped = false;
            var layerDepth = (float)((tv.boundingBox.Bottom - 1) / 10000.0 + 9.9999997473787516E-06);
            var fade = 0.0f;
            var scale = tv.getScreenSizeModifier();
            var tvSprite = new TemporaryAnimatedSprite(null, screenRectangle, interval, length, loops, screenPosition, flicker, flipped, layerDepth, fade,
                Color.White, scale, 0.0f, 0.0f, 0.0f);
            tvSprite.texture = _gazetteTexture;
            return tvSprite;
        }
    }
}
