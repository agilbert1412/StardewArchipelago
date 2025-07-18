using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;

namespace StardewArchipelago.GameModifications.CodeInjections.Powers
{
    public class ArchipelagoPower
    {
        private static Texture2D CustomPowersTexture => TexturesLoader.GetTexture(Path.Combine("Powers", "Powers.png"));
        public static Texture2D DefaultTexture => ArchipelagoTextures.GetArchipelagoLogo(32, ArchipelagoTextures.COLOR);

        public string Name { get; }
        public Texture2D Texture { get; }
        public Rectangle TextureRectangle { get; }
        public float TextureScale { get; }
        public string Description { get; }
        public string Condition => GameStateConditionProvider.CreateHasReceivedItemCondition(Name);

        public Func<SlotData, bool> IsIncluded { get; }

        public ArchipelagoPower(string name, string description = null, Point? textureOrigin = null, Func<SlotData, bool> isIncluded = null)
        {
            Name = name;
            Description = string.IsNullOrWhiteSpace(description) ? Name : description;
            if (textureOrigin != null)
            {
                Texture = CustomPowersTexture;
                TextureRectangle = new Rectangle(textureOrigin.Value.X, textureOrigin.Value.Y, 16, 16) ;
                TextureScale = 4f;
            }
            else
            {
                Texture = DefaultTexture;
                TextureRectangle = new Rectangle(0, 0, 32, 32);
                TextureScale = 2f;
            }
            IsIncluded = isIncluded ?? (_ => true);
        }

        public ArchipelagoPower(string name, string description, int textureOriginX, int textureOriginY) : this(name, description, new Point(textureOriginX, textureOriginY))
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is not ArchipelagoPower otherPower)
            {
                return false;
            }

            return this.Equals(otherPower);
        }

        protected bool Equals(ArchipelagoPower otherPower)
        {
            if (otherPower == null)
            {
                return false;
            }

            return this.Name == otherPower.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
