using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewArchipelago.ViewerEventsModule.Events
{
    public class EventCollection
    {
        public Dictionary<string, Event> _events;

        public double CurrentMultiplier { get; set; }

        public EventCollection()
        {
            _events = new Dictionary<string, Event>();
            CurrentMultiplier = 0.1;
        }

        public int Count => _events.Count;

        public void ImportFrom(string eventsFile)
        {
            var lines = File.ReadAllText(eventsFile, Encoding.UTF8);
            dynamic jsonData = JsonConvert.DeserializeObject(lines);
            foreach (JObject ttgEventString in jsonData)
            {
                var ttgEvent = new Event(ttgEventString);
                this.Add(ttgEvent.name, ttgEvent);
            }
        }

        public void ExportTo(string eventsFile)
        {
            var json = JsonConvert.SerializeObject(this.ToList(), Formatting.Indented);
            File.WriteAllText(eventsFile, json);
        }

        public List<Event> ToList()
        {
            return _events.Values.ToList();
        }

        public Event GetEvent(string eventName)
        {
            foreach (var eventKey in _events.Keys)
            {
                if (eventKey.Equals(eventName, StringComparison.OrdinalIgnoreCase))
                {
                    return _events[eventKey];
                }
            }

            foreach (var eventKey in _events.Keys)
            {
                if (levenshtein(eventKey.ToLower(), eventName.ToLower()) < 3)
                {
                    return _events[eventKey];
                }
            }

            Console.WriteLine($"Found no event by the name {eventName}");
            return null;
        }

        private void Add(string eventKey, Event eventToAdd)
        {
            _events.Add(eventKey, eventToAdd);
        }

        public void ClearAllBanks()
        {
            foreach (var e in ToList())
            {
                e.SetBank(0);
            }
        }

        private static int levenshtein(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b) ? 0 : b.Length;
            }

            if (string.IsNullOrEmpty(b))
            {
                return string.IsNullOrEmpty(a) ? 0 : a.Length;
            }

            int cost;
            var d = new int[a.Length + 1, b.Length + 1];
            int min1;
            int min2;
            int min3;

            for (var i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (var i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (var i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (var j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = (a[i - 1] != b[j - 1]) ? 1 : 0;

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }
    }
}
