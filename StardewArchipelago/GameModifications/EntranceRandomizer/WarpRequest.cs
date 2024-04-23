using StardewValley;

namespace StardewArchipelago.GameModifications.EntranceRandomizer
{
    public class WarpRequest
    {
        public LocationRequest LocationRequest { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public FacingDirection FacingDirectionAfterWarp { get; set; }

        public WarpRequest(LocationRequest locationRequest, int tileX, int tileY, FacingDirection facingDirectionAfterWarp)
        {
            LocationRequest = locationRequest;
            TileX = tileX;
            TileY = tileY;
            FacingDirectionAfterWarp = facingDirectionAfterWarp;
        }
    }
}
