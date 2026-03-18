using System;
using StardewValley.Extensions;

namespace StardewArchipelago.GameModifications.MultiplayerVision.FoolVision
{
    public class ActiveFoolPlayer
    {
        private Random _random;
        public FoolPlayerPath CurrentPath { get; set; }
        public int CurrentPathIndex { get; set; }
        public PlayerAppearance Appearance {get; set; }

        public ActiveFoolPlayer(FoolPlayerPath path, Random random)
        {
            CurrentPath = path;
            CurrentPathIndex = 0;
            _random = random;
            Appearance = GenerateRandomAppearance();
        }

        private PlayerAppearance GenerateRandomAppearance()
        {
            var accessory = bouncePacketData.TryGetValue("accessory", out var accessoryValue) ? accessoryValue.ToObject<int>() : 0;

            var hair = bouncePacketData.TryGetValue("hair", out var hairValue) ? hairValue.ToObject<int>() : 0;
            var hairColorRed = bouncePacketData.TryGetValue("hairColorRed", out var hairColorRedValue) ? hairColorRedValue.ToObject<int>() : 0;
            var hairColorGreen = bouncePacketData.TryGetValue("hairColorGreen", out var hairColorGreenValue) ? hairColorGreenValue.ToObject<int>() : 0;
            var hairColorBlue = bouncePacketData.TryGetValue("hairColorBlue", out var hairColorBlueValue) ? hairColorBlueValue.ToObject<int>() : 0;

            var eyeColorRed = bouncePacketData.TryGetValue("eyeColorRed", out var eyeColorRedValue) ? eyeColorRedValue.ToObject<int>() : 0;
            var eyeColorGreen = bouncePacketData.TryGetValue("eyeColorGreen", out var eyeColorGreenValue) ? eyeColorGreenValue.ToObject<int>() : 0;
            var eyeColorBlue = bouncePacketData.TryGetValue("eyeColorBlue", out var eyeColorBlueValue) ? eyeColorBlueValue.ToObject<int>() : 0;

            var hatId = bouncePacketData.TryGetValue("hatId", out var hatIdValue) ? hatIdValue.ToObject<string>() : "";
            var shirtId = bouncePacketData.TryGetValue("shirtId", out var shirtIdValue) ? shirtIdValue.ToObject<string>() : "";
            var pantsId = bouncePacketData.TryGetValue("pantsId", out var pantsIdValue) ? pantsIdValue.ToObject<string>() : "";
            var shoesId = bouncePacketData.TryGetValue("shoesId", out var shoesIdValue) ? shoesIdValue.ToObject<string>() : "";

            var pantsColorRed = bouncePacketData.TryGetValue("pantsColorRed", out var pantsColorRedValue) ? pantsColorRedValue.ToObject<int>() : 0;
            var pantsColorGreen = bouncePacketData.TryGetValue("pantsColorGreen", out var pantsColorGreenValue) ? pantsColorGreenValue.ToObject<int>() : 0;
            var pantsColorBlue = bouncePacketData.TryGetValue("pantsColorBlue", out var pantsColorBlueValue) ? pantsColorBlueValue.ToObject<int>() : 0;

            var appearance = new PlayerAppearance()
            {
                IsMale = _random.NextBool(),

                Skin = _random.Next(0, 24),
                Accessory = _random.Next(-1, 30),

                Hair = hair,
                HairColorRed = hairColorRed,
                HairColorGreen = hairColorGreen,
                HairColorBlue = hairColorBlue,

                EyeColorRed = eyeColorRed,
                EyeColorGreen = eyeColorGreen,
                EyeColorBlue = eyeColorBlue,

                PantsColorRed = pantsColorRed,
                PantsColorGreen = pantsColorGreen,
                PantsColorBlue = pantsColorBlue,

                HatId = hatId,
                ShirtId = shirtId,
                PantsId = pantsId,
                ShoesId = shoesId,
            };

            return appearance;
        }
    }
}
