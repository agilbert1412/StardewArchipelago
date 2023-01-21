using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class FarmInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }

        public static bool CheckAction_GrandpaNote_PreFix(Farm __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var rect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
                if (!__instance.objects.ContainsKey(new Vector2((float)tileLocation.X, (float)tileLocation.Y)) && __instance.CheckPetAnimal(rect, who))
                    return true; // run original logic
                var grandpaShrinePosition = __instance.GetGrandpaShrinePosition();
                if (tileLocation.X < grandpaShrinePosition.X - 1 || tileLocation.X > grandpaShrinePosition.X + 1 ||
                    tileLocation.Y != grandpaShrinePosition.Y)
                {
                    return true; // run original logic
                }

                if (__instance.hasSeenGrandpaNote)
                {
                    return true; // run original logic
                }

                Game1.addMail("hasSeenGrandpaNote", true);
                __instance.hasSeenGrandpaNote = true;
                Game1.activeClickableMenu = new LetterViewerMenu($"{_archipelago.SlotData.SlotName}^^I may be gone, but I am still watching over you^^-Grandpa");
                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_GrandpaNote_PreFix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool SpawnWeedsAndStones_ConsiderUserPreference_PreFix(GameLocation __instance, ref int numDebris, bool weedsOnly, bool spawnFromOldWeeds)
        {
            try
            {
                switch (_archipelago.SlotData.DebrisMultiplier)
                {
                    case DebrisMultiplier.Vanilla:
                        break;
                    case DebrisMultiplier.HalfDebris:
                        numDebris /= 2;
                        break;
                    case DebrisMultiplier.QuarterDebris:
                        numDebris /= 4;
                        break;
                    case DebrisMultiplier.NoDebris:
                        numDebris = 0;
                        break;
                    case DebrisMultiplier.StartClear:
                        if (Game1.Date.TotalDays == 0)
                        {
                            numDebris = 0;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SpawnWeedsAndStones_ConsiderUserPreference_PreFix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
