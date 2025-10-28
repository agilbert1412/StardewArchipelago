using Microsoft.Xna.Framework;
using StardewArchipelago.ViewerEventsModule.Events;
using StardewValley;

namespace StardewArchipelago.ViewerEventsModule.EventsExecution
{
    public class ExecutableEvent
    {
        private QueuedEvent QueuedEvent { get; }

        public ExecutableEvent(QueuedEvent queuedEvent)
        {
            QueuedEvent = queuedEvent;
        }

        public void Execute()
        {
            var message = $"{QueuedEvent.BaseEvent.name}";
            if (QueuedEvent.queueCount > 1)
            {
                message += $" (x{QueuedEvent.queueCount})";
            }

            message += $" from {QueuedEvent.username}";

            Game1.chatBox.addMessage(message, Color.Blue);
        }
    }
}
