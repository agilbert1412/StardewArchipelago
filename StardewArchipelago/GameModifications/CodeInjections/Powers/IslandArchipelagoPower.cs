using Microsoft.Xna.Framework;

namespace StardewArchipelago.GameModifications.CodeInjections.Powers
{
    public class IslandArchipelagoPower : ArchipelagoPower
    {
        public IslandArchipelagoPower(string name, string description = null, Point? textureOrigin = null) : base(name, description, textureOrigin, (x) => !x.ExcludeGingerIsland)
        {
        }

        public IslandArchipelagoPower(string name, string description, int textureOriginX, int textureOriginY) : this(name, description, new Point(textureOriginX, textureOriginY))
        {
        }
    }
}
