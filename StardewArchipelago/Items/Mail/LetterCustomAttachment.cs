using System;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterCustomAttachment : LetterAttachment
    {
        public Action LetterOpenedAction { get; private set; }

        public LetterCustomAttachment(ReceivedItem apItem, Action openedAction = null) : base(apItem)
        {
            LetterOpenedAction = openedAction;
        }
    }
}
