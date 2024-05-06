using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Extensions;

namespace StardewArchipelago.Items.Mail
{
    public class LetterEventSeenAttachment : LetterInformationAttachment
    {
        public IEnumerable<string> EventsToMarkAsSeen { get; private set; }

        protected override bool IsEmptyLetter => true;

        public LetterEventSeenAttachment(ReceivedItem apItem, string eventToMarkAsSeen) : this(apItem, new[] { eventToMarkAsSeen })
        {
        }

        public LetterEventSeenAttachment(ReceivedItem apItem, IEnumerable<string> eventsToMarkAsSeen) : base(apItem)
        {
            EventsToMarkAsSeen = eventsToMarkAsSeen;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            Game1.player.eventsSeen.AddRange(EventsToMarkAsSeen);
            base.SendToPlayer(mailman);
        }
    }
}
