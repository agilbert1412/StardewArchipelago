using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace StardewArchipelago.Items.Traps
{
    public class BombSpawner
    {
        private readonly IModHelper _helper;

        public BombSpawner(IModHelper helper)
        {
            _helper = helper;
        }

        public void SpawnBomb(int explosionRadius, int delay = 0)
        {
            var location = Game1.player.currentLocation;
            var tile = Game1.player.Tile;
            var x = tile.X * 64;
            var y = tile.Y * 64;
            // protected internal static Multiplayer multiplayer = new Multiplayer();
            var multiplayerField = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
            var multiplayer = multiplayerField.GetValue();
            var parentSheetIndex = 287;
            if (explosionRadius < 5)
            {
                parentSheetIndex = 286;
            }
            if (explosionRadius > 5)
            {
                parentSheetIndex = 288;
            }

            var randomId = Game1.random.Next();
            location.playSound("thudStep");
            var bombSprite = new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, tile * 64f, true, false, location, Game1.player)
            {
                shakeIntensity = 0.5f,
                shakeIntensityChange = 1f / 500f,
                extraInfoForEndBehavior = randomId,
                endFunction = location.removeTemporarySpritesWithID,
                bombRadius = explosionRadius,
                delayBeforeAnimationStart = delay,
            };
            multiplayer.broadcastSprites(location, bombSprite);
            multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, tile * 64f + new Vector2(5f, 3f) * 4f, true, false, (y + 7) / 10000f, 0.0f, Color.Yellow, 4f, 0.0f, 0.0f, 0.0f)
            {
                id = randomId,
                delayBeforeAnimationStart = delay,
            });
            multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, tile * 64f + new Vector2(5f, 3f) * 4f, true, true, (y + 7) / 10000f, 0.0f, Color.Orange, 4f, 0.0f, 0.0f, 0.0f)
            {
                delayBeforeAnimationStart = 50 + delay,
                id = randomId,
            });
            multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, tile * 64f + new Vector2(5f, 3f) * 4f, true, false, (y + 7) / 10000f, 0.0f, Color.White, 3f, 0.0f, 0.0f, 0.0f)
            {
                delayBeforeAnimationStart = 100 + delay,
                id = randomId,
            });
            location.netAudio.StartPlaying("fuse");
        }
    }
}
