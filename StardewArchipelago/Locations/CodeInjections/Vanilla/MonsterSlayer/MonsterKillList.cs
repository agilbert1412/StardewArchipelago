using System.Text;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer
{
    public class MonsterKillList
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public MonsterKillList(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public string GetKillListLetterContent()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Header").Replace('\n', '^') + "^");
            int killCount1 = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge") + Game1.stats.getMonstersKilled("Tiger Slime");
            int killCount2 = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute") + Game1.stats.getMonstersKilled("Shadow Sniper");
            int killCount3 = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
            int killCount4 = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
            int killCount5 = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
            int killCount6 = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat") + Game1.stats.getMonstersKilled("Iridium Bat");
            int killCount7 = Game1.stats.getMonstersKilled("Duggy") + Game1.stats.getMonstersKilled("Magma Duggy");
            int monstersKilled1 = Game1.stats.getMonstersKilled("Dust Spirit");
            int monstersKilled2 = Game1.stats.getMonstersKilled("Mummy");
            int monstersKilled3 = Game1.stats.getMonstersKilled("Pepper Rex");
            int killCount8 = Game1.stats.getMonstersKilled("Serpent") + Game1.stats.getMonstersKilled("Royal Serpent");
            int killCount9 = Game1.stats.getMonstersKilled("Magma Sprite") + Game1.stats.getMonstersKilled("Magma Sparker");
            stringBuilder.Append(GetKillListLine("Slimes", killCount1, 1000));
            stringBuilder.Append(GetKillListLine("VoidSpirits", killCount2, 150));
            stringBuilder.Append(GetKillListLine("Bats", killCount6, 200));
            stringBuilder.Append(GetKillListLine("Skeletons", killCount3, 50));
            stringBuilder.Append(GetKillListLine("CaveInsects", killCount5, 125));
            stringBuilder.Append(GetKillListLine("Duggies", killCount7, 30));
            stringBuilder.Append(GetKillListLine("DustSprites", monstersKilled1, 500));
            stringBuilder.Append(GetKillListLine("RockCrabs", killCount4, 60));
            stringBuilder.Append(GetKillListLine("Mummies", monstersKilled2, 100));
            stringBuilder.Append(GetKillListLine("PepperRex", monstersKilled3, 50));
            stringBuilder.Append(GetKillListLine("Serpent", killCount8, 250));
            stringBuilder.Append(GetKillListLine("MagmaSprite", killCount9, 150));
            stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Footer").Replace('\n', '^'));
            return stringBuilder.ToString();
        }

        private string GetKillListLine(string monsterType, int killCount, int target)
        {
            const string monsterFormat = "Strings\\Locations:AdventureGuild_KillList_{0}";
            var monsterTypeDisplayText = Game1.content.LoadString(string.Format(monsterFormat, monsterType));
            var lineFormat = Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat");
            if (killCount <= 0)
            {
                lineFormat = Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_None");
            }

            if (killCount >= target)
            {
                lineFormat = Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_OverTarget");
            }
            
            var line = string.Format(lineFormat, killCount, target, monsterTypeDisplayText) + "^";
            return line;
        }
    }
}
