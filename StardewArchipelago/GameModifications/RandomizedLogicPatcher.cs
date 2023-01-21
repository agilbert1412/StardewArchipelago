using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewArchipelago.GameModifications
{
    public class RandomizedLogicPatcher
    {
        private readonly Harmony _harmony;

        public RandomizedLogicPatcher(IMonitor monitor, Harmony harmony, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _harmony = harmony;
            MineshaftLogicInjections.Initialize(monitor);
            CommunityCenterLogicInjections.Initialize(monitor, locationChecker);
            FarmInjections.Initialize(monitor, archipelago);
        }

        public void PatchAllGameLogic()
        {
            PatchMineMaxFloorReached();
            PatchDefinitionOfCommunityCenterComplete();
            PatchGrandpaNote();
            PatchDebris();
        }

        private void PatchMineMaxFloorReached()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NetWorldState), typeof(NetWorldState).GetProperty(nameof(NetWorldState.LowestMineLevel)).GetSetMethod().Name),
                prefix: new HarmonyMethod(typeof(MineshaftLogicInjections), nameof(MineshaftLogicInjections.SetLowestMineLevel_SkipToSkullCavern_Prefix))
            );
        }

        private void PatchDefinitionOfCommunityCenterComplete()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.hasCompletedCommunityCenter)),
                prefix: new HarmonyMethod(typeof(CommunityCenterLogicInjections), nameof(CommunityCenterLogicInjections.HasCompletedCommunityCenter_CheckGameStateInsteadOfLetters_Prefix))
            );

            var townEvents = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Town");
            var communityCenterCeremonyEventKey = "";
            var communityCenterCeremonyEventValue = "";

            foreach (var (key, value) in townEvents)
            {
                if (!key.StartsWith("191393"))
                {
                    continue;
                }

                communityCenterCeremonyEventKey = key;
                communityCenterCeremonyEventValue = value;
            }
            townEvents.Remove(communityCenterCeremonyEventKey);
            communityCenterCeremonyEventKey = communityCenterCeremonyEventKey.Replace(" cc", " apcc");
            townEvents.Add(communityCenterCeremonyEventKey, communityCenterCeremonyEventValue);

            SendMissedAPCCMails();
        }

        private static void SendMissedAPCCMails()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (!communityCenter.areAllAreasComplete())
            {
                return;
            }
            string[] apccMails =
                { "apccPantry", "apccCraftsRoom", "apccFishTank", "apccBoilerRoom", "apccVault", "apccBulletin" };
            foreach (var apccMail in apccMails)
            {
                if (!Game1.player.mailReceived.Contains(apccMail))
                {
                    Game1.player.mailForTomorrow.Add(apccMail + "%&NL&%");
                }
            }
        }

        private void PatchGrandpaNote()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.checkAction)),
                prefix: new HarmonyMethod(typeof(FarmInjections), nameof(FarmInjections.CheckAction_GrandpaNote_PreFix))
            );
        }

        private void PatchDebris()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnWeedsAndStones)),
                prefix: new HarmonyMethod(typeof(FarmInjections), nameof(FarmInjections.SpawnWeedsAndStones_ConsiderUserPreference_PreFix))
            );
        }
    }
}
