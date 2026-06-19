using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class TentKitInjections
    {
        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public virtual bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        public static bool PlacementAction_AllowTentsAnywhere_Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            try
            {
                if (__instance.QualifiedItemId != QualifiedItemIds.TENT_KIT)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (location == null || !location.IsOutdoors)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (TryPlaceTentIndoors(location, x, y, who))
                {
                    __result = true;
                }
                else
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Tent_Blocked"));
                    __result = false;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlacementAction_AllowTentsAnywhere_Prefix)}\t{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool TryPlaceTentIndoors(GameLocation location, int x, int y, Farmer who)
        {
            var vector2_1 = new Vector2(x / 64, y / 64);
            if (who == null)
            {
                return false;
            }

            var area = Rectangle.Empty;
            switch (Utility.getDirectionFromChange(vector2_1, who.Tile))
            {
                case 0:
                    area = new Rectangle((int)(vector2_1.X - 1.0), (int)(vector2_1.Y - 1.0), 3, 2);
                    break;
                case 1:
                    area = new Rectangle((int)vector2_1.X, (int)(vector2_1.Y - 1.0), 3, 2);
                    break;
                case 2:
                    area = new Rectangle((int)(vector2_1.X - 1.0), (int)vector2_1.Y, 3, 2);
                    break;
                case 3:
                    area = new Rectangle((int)(vector2_1.X - 2.0), (int)(vector2_1.Y - 1.0), 3, 2);
                    break;
            }

            if (area == Rectangle.Empty || !location.isAreaClear(area))
            {
                return false;
            }

            location.largeTerrainFeatures.Add(new Tent(new Vector2(area.X + 1, area.Y + 1)));
            Game1.playSound("moss_cut");
            Game1.playSound("woodyHit");
            var rectangle = new Rectangle(area.X * 64, area.Y * 64, 192, 128);
            Utility.addDirtPuffs(location, area.X, area.Y, 3, 2, 9);
            return true;

        }
    }
}
