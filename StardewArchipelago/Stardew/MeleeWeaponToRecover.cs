using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Stardew
{
    public class MeleeWeaponToRecover : MeleeWeapon
    {
        public MeleeWeaponToRecover(int weaponId) : base(weaponId)
        {

        }

        public override int salePrice()
        {
            return base.salePrice() * 4;
        }
    }
}
