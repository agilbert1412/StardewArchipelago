using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewValley;

namespace StardewArchipelago.GameModifications.EntranceRandomizer
{
    public class OneWayEntrance
    {
        private const int MAX_DISTANCE = 2;

        private readonly EquivalentWarps _equivalentWarps;

        public string OriginName { get; }
        public string DestinationName { get; }
        public Point OriginPosition { get; set; }
        public Point DestinationPosition { get; set; }
        public FacingDirection FacingDirectionAfterWarp { get; }

        private OneWayEntrance _replacement;

        public OneWayEntrance(EquivalentWarps equivalentWarps, string originName, string destinationName, Point originPosition, Point destinationPosition, FacingDirection facingDirectionAfterWarp)
        {
            _equivalentWarps = equivalentWarps;
            OriginName = originName;
            DestinationName = destinationName;
            OriginPosition = originPosition;
            DestinationPosition = destinationPosition;
            FacingDirectionAfterWarp = facingDirectionAfterWarp;
        }

        public void ReplaceWith(OneWayEntrance replacementEntrance)
        {
            _replacement = replacementEntrance;
        }

        public bool GetModifiedWarp(WarpRequest originalWarp, out WarpRequest newWarp)
        {
            newWarp = originalWarp;
            if (_replacement == null)
            {
                return false;
            }

            var currentLocation = Game1.currentLocation.Name;
            if (string.Equals(currentLocation, OriginName, StringComparison.CurrentCultureIgnoreCase) && string.Equals(originalWarp.LocationRequest.Name, DestinationName, StringComparison.CurrentCultureIgnoreCase))
            {
                return ReplaceWarpRequest(originalWarp, out newWarp);
            }

            return false;
        }

        private bool ReplaceWarpRequest(WarpRequest originalWarp, out WarpRequest newWarp)
        {
            newWarp = originalWarp;
            var currentTile = new Point(Game1.player.getTileX(), Game1.player.getTileY());
            if (!OriginPosition.IsCloseEnough(currentTile, MAX_DISTANCE))
            {
                return false;
            }

            var correctReplacement = _equivalentWarps.GetCorrectEquivalentWarp(_replacement);

            var locationRequest = originalWarp.LocationRequest;
            locationRequest.Name = correctReplacement.DestinationName;
            locationRequest.Location = Game1.getLocationFromName(correctReplacement.DestinationName, locationRequest.IsStructure);
            newWarp = new WarpRequest(locationRequest, correctReplacement.DestinationPosition.X, correctReplacement.DestinationPosition.Y, correctReplacement.FacingDirectionAfterWarp);
            return true;
        }
    }
}
