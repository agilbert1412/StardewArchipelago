using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley;
using System;
using System.Collections.Generic;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class TownInjections
    {
        private const string ABANDONED_JOJA_MART = "AbandonedJojaMart";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        private static bool _hasSeenCcCeremonyCutscene;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public override void MakeMapModifications(bool force = false)
        public static bool MakeMapModifications_JojamartAndTheater_Prefix(Town __instance, bool force)
        {
            try
            {
                if (Game1.player.hasOrWillReceiveMail(JojaConstants.MEMBERSHIP_MAIL))
                {
                    return MakeMapModificationsJojaRoutePrefix(__instance);
                }

                return MakeMapModificationCCRoutePrefix(__instance);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_JojamartAndTheater_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool MakeMapModificationsJojaRoutePrefix(Town town)
        {
            if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 2)
            {
                // protected string loadedMapPath;
                var loadedMapPathField = _modHelper.Reflection.GetField<HashSet<string>>(town, "loadedMapPath");
                var loadedMapPath = loadedMapPathField.GetValue();

                var rectangle = new Rectangle(46, 11, 15, 17);
                var IsHalloween = Game1.IsFall && Game1.dayOfMonth == 27 && Game1.year % 2 == 0 && loadedMapPath.Contains("Halloween");
                if (IsHalloween)
                {
                    // protected HashSet<string> _appliedMapOverrides;
                    var _appliedMapOverridesField = _modHelper.Reflection.GetField<HashSet<string>>(town, "_appliedMapOverrides");
                    var appliedMapOverrides = _appliedMapOverridesField.GetValue();
                    appliedMapOverrides.Remove("Town-TheaterCC");
                }
                town.ApplyMapOverride("Town-TheaterCC" + (IsHalloween ? "-Halloween2" : ""), new Rectangle?(rectangle), new Rectangle?(rectangle));
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 1)
            {
                // private void refurbishCommunityCenter()
                var refurbishCommunityCenterMethod = _modHelper.Reflection.GetMethod(town, "refurbishCommunityCenter");
                refurbishCommunityCenterMethod.Invoke();
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            _hasSeenCcCeremonyCutscene = Utility.HasAnyPlayerSeenEvent(EventIds.COMMUNITY_CENTER_COMPLETE);
            if (_hasSeenCcCeremonyCutscene)
            {
                Game1.player.eventsSeen.Remove(EventIds.COMMUNITY_CENTER_COMPLETE);
            }

            return MethodPrefix.RUN_ORIGINAL_METHOD;
        }

        private static bool MakeMapModificationCCRoutePrefix(Town town)
        {
            if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 2)
            {
                var rectangle = new Rectangle(84, 41, 27, 15);
                town.ApplyMapOverride("Town-Theater", rectangle, rectangle);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 1)
            {
                // private void showDestroyedJoja()
                var showDestroyedJojaMethod = _modHelper.Reflection.GetMethod(town, "showDestroyedJoja");
                showDestroyedJojaMethod.Invoke();
                if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible"))
                {
                    town.crackOpenAbandonedJojaMartDoor();
                }
                if (!Game1.player.mailReceived.Contains(string.Join("ap", ABANDONED_JOJA_MART)))
                {
                    Game1.player.mailReceived.Add("apAbandonedJojaMart");
                }
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            _hasSeenCcCeremonyCutscene = Utility.HasAnyPlayerSeenEvent(EventIds.COMMUNITY_CENTER_COMPLETE);
            if (_hasSeenCcCeremonyCutscene)
            {
                Game1.player.eventsSeen.Remove(EventIds.COMMUNITY_CENTER_COMPLETE);
            }

            return MethodPrefix.RUN_ORIGINAL_METHOD;
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_JojamartAndTheaterAndShortcuts_Postfix(Town __instance, bool force)
        {
            try
            {
                if (_hasSeenCcCeremonyCutscene && !Game1.player­.eventsSeen.Contains(EventIds.COMMUNITY_CENTER_COMPLETE))
                {
                    Game1.player.eventsSeen.Add(EventIds.COMMUNITY_CENTER_COMPLETE);
                }

                ShortcutInjections.OpenTownShortcuts(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_JojamartAndTheaterAndShortcuts_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
