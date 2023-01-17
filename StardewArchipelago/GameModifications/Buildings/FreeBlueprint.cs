using StardewValley;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class FreeBlueprint : BluePrint
    {
        public FreeBlueprint(string name, string sendingPlayerName) : base(name)
        {
            itemsRequired.Clear();
            moneyRequired = 0;
            displayName = $"Free {displayName}";
            description = $"A gift from {sendingPlayerName}. {description}";
        }
    }
}
