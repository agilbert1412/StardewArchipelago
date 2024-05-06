using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class BootsToRecover : Boots
    {
        private string _bootsId;

        public BootsToRecover() : base()
        {

        }

        public BootsToRecover(string bootsId) : base(bootsId)
        {
            _bootsId = bootsId;
        }

        public override int salePrice(bool ignoreProfitMargins = false)
        {
            return base.salePrice(ignoreProfitMargins) * 4;
        }

        public override bool actionWhenPurchased(string shopId)
        {
            var realBoots = new Boots(_bootsId);
            // Game1.player.itemsLostLastDeath.Clear(); No need to clear it
            isLostItem = false;
            Game1.player.recoveredItem = realBoots;
            Game1.player.mailReceived.Remove("MarlonRecovery");
            Game1.addMailForTomorrow("MarlonRecovery");
            Game1.playSound("newArtifact");
            Game1.exitActiveMenu();
            var flag = Stack > 1;
            Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), Game1.content.LoadString(flag ? "Strings\\StringsFromCSFiles:ItemRecovery_Engaged_Stack" : "Strings\\StringsFromCSFiles:ItemRecovery_Engaged", (object)Lexicon.makePlural(DisplayName, !flag)));
            return true;
        }
    }
}
