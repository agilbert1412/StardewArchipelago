using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;

namespace StardewArchipelago.Items.Unlocks
{
    public interface IUnlockManager
    {
        bool IsUnlock(string unlockName);
        LetterAttachment PerformUnlockAsLetter(ReceivedItem unlock);
    }
}
