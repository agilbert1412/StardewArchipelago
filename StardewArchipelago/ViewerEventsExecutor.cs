using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.ViewerEventsModule;
using StardewArchipelago.ViewerEventsModule.Events;

namespace StardewArchipelago
{
    public class ViewerEventsExecutor : IEventsExecutor
    {
        private readonly ILogger _logger;
        private readonly EventCollection _events;
        private readonly EventQueue _eventQueue;

        public ViewerEventsExecutor(ILogger logger)
        {
            _logger = logger;
            _events = new EventCollection();
            _eventQueue = new EventQueue(logger);

            _logger.LogInfo(_events.Count + " is the total events count.");
        }

        public bool IsEventValid(string eventName, params string[] args)
        {
            throw new System.NotImplementedException();
        }

        public void DequeueEvent()
        {
            throw new System.NotImplementedException();
        }

        public void QueueEvent(QueuedEvent eventToQueue, int amount = 1)
        {
            throw new System.NotImplementedException();
        }

        public void TriggerEvent(QueuedEvent eventToQueue, int amount = 1)
        {
            throw new System.NotImplementedException();
        }
    }
}
