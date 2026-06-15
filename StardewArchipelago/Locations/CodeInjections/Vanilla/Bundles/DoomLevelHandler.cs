using System;
using KaitoKid.Utilities.Interfaces;
using ManagedDoom;
using ManagedDoom.Silk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class DoomLevelHandler
    {
        private readonly ILogger _logger;
        private readonly IModHelper _helper;

        public const int IMAGE_SIZE = 256;
        private const int OFFSET_X = 936 - (IMAGE_SIZE / 2);
        private const int OFFSET_Y = 88;
        private Rectangle _gameRect;

        public DoomLevelHandler(ILogger logger, IModHelper modHelper)
        {
            _logger = logger;
            _helper = modHelper;
            _gameRect = new Rectangle(0, 40, 120, 82);

            LaunchDoom();
        }

        private void LaunchDoom()
        {
            try
            {
                var args = new string[0];
                var cmdArgs = new CommandLineArgs(args);
                var doom = new SilkDoom(cmdArgs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed at launching DOOM: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        public void DrawDoom(SpriteBatch spriteBatch, int xPositionOnScreen, int yPositionOnScreen)
        {
        }

        public bool ReceiveLeftClick(int x, int y)
        {

            return true;
        }
    }
}
