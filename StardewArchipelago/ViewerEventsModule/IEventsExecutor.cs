using StardewArchipelago.ViewerEventsModule.Events;

namespace StardewArchipelago.ViewerEventsModule
{
    public interface IEventsExecutor
    {
        public bool IsEventValid(string eventName, params string[] args);
        public void DequeueEvent();
        public void QueueEvent(QueuedEvent eventToQueue, int amount = 1);
        public void TriggerEvent(QueuedEvent eventToQueue, int amount = 1);
    }
}
