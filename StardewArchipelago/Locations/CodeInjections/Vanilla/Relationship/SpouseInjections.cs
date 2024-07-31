using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship
{

    public class SpouseInjections
    {
        private const string SPOUSE_STARDROP = "Spouse Stardrop";

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

        // public virtual bool checkAction(Farmer who, GameLocation l)
        public static bool CheckAction_SpouseStardrop_Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                var npcName = __instance.Name;
                if (__instance.IsInvisible || __instance.isSleeping.Value || !who.CanMove || npcName.Equals("Henchman") || l.Name.Equals("WitchSwamp"))
                {
                    return true; // run original logic
                }

                if (!npcName.Equals(who.spouse) || !who.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                if (__instance.Sprite.CurrentAnimation == null)
                {
                    __instance.faceDirection(-3);
                }

                if (!who.friendshipData.ContainsKey(npcName))
                {
                    return true; // run original logic
                }

                var friendshipData = who.friendshipData[npcName];

                if (__instance.Sprite.CurrentAnimation != null || friendshipData.Points < 3125 || !_locationChecker.IsLocationMissing(SPOUSE_STARDROP))
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation(SPOUSE_STARDROP);
                __instance.CurrentDialogue.Push(__instance.TryGetDialogue("SpouseStardrop") ?? new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs.4001"));
                __instance.shouldSayMarriageDialogue.Value = false;
                __instance.currentMarriageDialogue.Clear();
                who.mailReceived.Add("CF_Spouse");
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_SpouseStardrop_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
