namespace StardewArchipelago.Constants.Vanilla
{
    public class DailyQuest
    {
        public const string HELP_WANTED = "Help Wanted: {0}";
        public const string ITEM_DELIVERY = "Item Delivery";
        public const string SLAY_MONSTERS = "Slay Monsters";
        public const string FISHING = "Fishing";
        public const string GATHERING = "Gathering";
    }

    public enum QuestType
    {
        Basic = 1,
        Crafting = 2,
        ItemDelivery = 3,
        Monster = 4,
        SlayMonsters = 4,
        Socialize = 5,
        Location = 6,
        Fishing = 7,
        Building = 8,
        ItemHarvest = 9,
        LostItem = 9,
        SecretLostItem = 9,
        ResourceCollection = 10,
    }
}
