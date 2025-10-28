using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using StardewArchipelago.ViewerEventsModule.EventsExecution;

namespace StardewArchipelago.ViewerEventsModule.Events
{
    public class ViewerEvent
    {
        public string name;
        public int cost; // The current cost
        public int bank; // The currently donated credits contributing to the next activation
        public bool stackable;
        public string alignment; // "positive", "negative" or "neutral"
        public string description;
        public string descriptionAnsi; // the description with coloring!

        public ViewerEvent()
        {

        }

        public ViewerEvent(JObject data)
        {
            name = data["name"].ToString();
            cost = int.Parse(data["cost"].ToString());
            bank = int.Parse(data["bank"].ToString());
            stackable = bool.Parse(data["stackable"].ToString());
            alignment = data["alignment"].ToString();
            description = data["description"].ToString();

            descriptionAnsi = data.ContainsKey("descriptionAnsi") && !string.IsNullOrWhiteSpace(data["descriptionAnsi"].ToString()) ? data["descriptionAnsi"].ToString() : description;
        }

        public int CheckCost()
        {
            return bank / cost;
        }

        public void CallEvent(double multiplier)
        {
            bank -= GetMultiplierCost(multiplier);
        }

        public int GetCostToNextActivation(double multiplier)
        {
            return GetMultiplierCost(multiplier) - GetBank();
        }

        public int GetBank()
        {
            return bank;
        }

        public bool IsStackable()
        {
            return stackable;
        }

        public void AddToBank(int amountToAdd)
        {
            bank += amountToAdd;
        }

        public void SetBank(int newBank)
        {
            bank = newBank;
        }

        public int GetMultiplierCost(double multiplier)
        {
            return (int)Math.Ceiling(cost * multiplier);
        }

        public void SetCost(int newCost)
        {
            cost = newCost;
        }

        public void SetCostWithMultiplier(int newCost, double multiplier)
        {
            cost = (int)Math.Round(newCost / multiplier);
        }

        public ExecutableEvent GetExecutableEvent(QueuedEvent queuedEvent)
        {
            return new ExecutableEvent(queuedEvent);
        }
    }
}
