using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
	public static class MailboxHelper {
		public static void TryGetNextMail() 
        {
            if (Game1.eventUp || Game1.CurrentEvent != null || Game1.IsFading() || Game1.activeClickableMenu != null || Game1.player.isEating || !Game1.player.CanMove || Game1.currentMinigame != null || Game1.isWarping || Game1.killScreen ||
                Game1.player.isInBed.Value || Game1.player.FarmerSprite.isPassingOut() || Game1.player.passedOut)
            {
                return;
            }

			var mailAmount = Game1.mailbox.Count;
			if (mailAmount <= 0)
			{
				Game1.chatBox?.addMessage($"Mailbox is empty", Color.Gold);
				return;
			}

			var farm = Game1.RequireLocation<Farm>("Farm");
			farm.mailbox();

			mailAmount = Game1.mailbox.Count;
			Game1.chatBox?.addMessage($"Mail Remaining: {mailAmount}", Color.Gold);
			return;
		}
	}
}
