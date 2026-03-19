using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Linq;

namespace StardewArchipelago.GameModifications.MultiplayerVision.FoolVision
{
    public class ActiveFoolPlayer
    {
        public string UniqueId { get; set; }
        private Random _random;
        public FoolPlayerPath CurrentPath { get; set; }
        public int CurrentPathIndex { get; set; }
        public PlayerAppearance Appearance {get; set; }

        public ActiveFoolPlayer(FoolPlayerPath path, Random random)
        {
            UniqueId = Guid.NewGuid().ToString();
            CurrentPath = path;
            CurrentPathIndex = 0;
            _random = random;
            Appearance = GenerateRandomAppearance();
        }

        private PlayerAppearance GenerateRandomAppearance()
        {
            var allHatIds = DataLoader.Hats(Game1.content).Keys.ToArray();
            var chosenHatId = allHatIds[_random.Next(allHatIds.Length)];

            var allShirtIds = DataLoader.Shirts(Game1.content).Keys.ToArray();
            var chosenShirtId = allShirtIds[_random.Next(allShirtIds.Length)];

            var allPantIds = DataLoader.Pants(Game1.content).Keys.ToArray();
            var chosenPantId = allPantIds[_random.Next(allPantIds.Length)];

            var allShoesIds = DataLoader.Boots(Game1.content).Keys.ToArray();
            var chosenShoesId = allShoesIds[_random.Next(allShoesIds.Length)];

            var appearance = new PlayerAppearance()
            {
                IsMale = _random.NextBool(),

                Skin = _random.Next(0, 24),
                Accessory = _random.Next(-1, 30),

                Hair = _random.Next(0, Farmer.GetLastHairStyle()),
                HairColorRed = _random.Next(0, 256),
                HairColorGreen = _random.Next(0, 256),
                HairColorBlue = _random.Next(0, 256),

                EyeColorRed = _random.Next(0, 256),
                EyeColorGreen = _random.Next(0, 256),
                EyeColorBlue = _random.Next(0, 256),

                PantsColorRed = _random.Next(0, 256),
                PantsColorGreen = _random.Next(0, 256),
                PantsColorBlue = _random.Next(0, 256),

                HatId = chosenHatId,
                ShirtId = chosenShirtId,
                PantsId = chosenPantId,
                ShoesId = chosenShoesId,
            };

            return appearance;
        }

        public VisiblePlayer CreateVisiblePlayer()
        {
            var currentPathPoint = CurrentPath.DataPoints[CurrentPathIndex];
            var visiblePlayer = new VisiblePlayer()
            {
                UniqueIdentifier = UniqueId,
                MapName = CurrentPath.MapName,
                Position = currentPathPoint.Position,
                Velocity = currentPathPoint.Velocity,
                FacingDirection = currentPathPoint.FacingDirection,
                IsGlowing = false,
                XOffset = 0,
                YOffset = 0,
                IsSitting = false,
                IsRidingHorse = false,
                Rotation = 0,
            };

            visiblePlayer.Appearance = Appearance;

            return visiblePlayer;
        }
    }
}
