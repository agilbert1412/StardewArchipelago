using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    public class QuestLocations
    {
        private readonly int _value;
        public bool StoryQuestsEnabled => _value >= 0;
        public int HelpWantedNumber => Math.Max(0, _value);

        internal QuestLocations(int value)
        {
            _value = value;
        }
    }
}