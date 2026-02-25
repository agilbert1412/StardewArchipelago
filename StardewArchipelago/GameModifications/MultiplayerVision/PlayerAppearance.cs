using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.GameModifications.MultiplayerVision
{
    public class PlayerAppearance
    {
        public bool IsMale { get; set; }

        public int Skin { get; set; }

        public int Hair { get; set; }
        public int HairColorRed { get; set; }
        public int HairColorGreen { get; set; }
        public int HairColorBlue { get; set; }

        public int EyeColorRed { get; set; }
        public int EyeColorGreen { get; set; }
        public int EyeColorBlue { get; set; }

        public int PantsColorRed { get; set; }
        public int PantsColorGreen { get; set; }
        public int PantsColorBlue { get; set; }

        public int Accessory { get; set; }

        public string HatId { get; set; }
        public string ShirtId { get; set; }
        public string PantsId { get; set; }
        public string ShoesId { get; set; }
    }
}
