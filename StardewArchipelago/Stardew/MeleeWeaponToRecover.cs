using StardewValley.Tools;

namespace StardewArchipelago.Stardew
{
    public class MeleeWeaponToRecover : MeleeWeapon
    {
        public MeleeWeaponToRecover() : base()
        {

        }

        public MeleeWeaponToRecover(int weaponId) : base(weaponId)
        {

        }

        public override int salePrice()
        {
            return base.salePrice() * 4;
        }
    }
}
