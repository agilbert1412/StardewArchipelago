using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
	public static class MailboxHelper {
		public static bool TryGetNextMail() {
			var mailAmount = Game1.mailbox.Count;
			if (mailAmount <= 0)
			{
				Game1.chatBox?.addMessage($"Mailbox is empty", Color.Gold);
				return true;
			}

			var farm = Game1.RequireLocation<Farm>("Farm");
			farm.mailbox();

			mailAmount = Game1.mailbox.Count;
			Game1.chatBox?.addMessage($"Mail Remaining: {mailAmount}", Color.Gold);
			return true;
		}
	}
}
