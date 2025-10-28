using Newtonsoft.Json.Linq;
using StardewArchipelago.ViewerEventsModule.EventsExecution;

namespace StardewArchipelago.ViewerEventsModule.Events
{
    public class QueuedEvent
    {
        private ViewerEvent _baseEvent;
        public string baseEventName;
        public int queueCount; //how many of these should be called
        public string username; //who triggered it

        public QueuedEvent(ViewerEvent e)
        {
            _baseEvent = e;
            baseEventName = e.name;
            queueCount = 0;
        }

        public QueuedEvent(JObject data, EventCollection allEvents)
        {
            baseEventName = data["baseEventName"].ToString();
            queueCount = int.Parse(data["queueCount"].ToString());
            username = data["username"].ToString();
            _baseEvent = allEvents.GetEvent(baseEventName);
        }

        public ViewerEvent BaseEvent => _baseEvent;

        public ExecutableEvent GetExecutableEvent()
        {
            return BaseEvent.GetExecutableEvent(this);
        }
    }
}
