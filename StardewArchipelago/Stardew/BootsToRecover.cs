using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class BootsToRecover : Boots
    {
        public BootsToRecover(int bootsId) : base(bootsId)
        {

        }

        public override int salePrice()
        {
            return base.salePrice() * 4;
        }
    }
}
