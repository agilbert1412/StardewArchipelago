using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class BootsToRecover : Boots
    {
        private int _bootsId;

        public BootsToRecover() : base()
        {

        }

        public BootsToRecover(int bootsId) : base(bootsId)
        {
            _bootsId = bootsId;
        }

        public override int salePrice()
        {
            return base.salePrice() * 4;
        }

        public override bool actionWhenPurchased()
        {
            var realBoots = new Boots(_bootsId);
            // Game1.player.itemsLostLastDeath.Clear(); No need to clear it
            this.isLostItem = false;
            Game1.player.recoveredItem = realBoots;
            Game1.player.mailReceived.Remove("MarlonRecovery");
            Game1.addMailForTomorrow("MarlonRecovery");
            Game1.playSound("newArtifact");
            Game1.exitActiveMenu();
            bool flag = this.Stack > 1;
            Game1.drawDialogue(Game1.getCharacterFromName("Marlon"), Game1.content.LoadString(flag ? "Strings\\StringsFromCSFiles:ItemRecovery_Engaged_Stack" : "Strings\\StringsFromCSFiles:ItemRecovery_Engaged", (object)Lexicon.makePlural(this.DisplayName, !flag)));
            return true;
        }
    }
}
