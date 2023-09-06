using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Unlocks;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer
{
    public static class MonsterSlayerInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void showMonsterKillList()
        public static bool ShowMonsterKillList_CustomListFromAP_Prefix(AdventureGuild __instance, ref bool __result)
        {
            try
            {
                if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
                    Game1.player.mailReceived.Add("checkedMonsterBoard");

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
                stringBuilder.Append(this.killListLine("Slimes", killCount1, 1000));
                stringBuilder.Append(this.killListLine("VoidSpirits", killCount2, 150));
                stringBuilder.Append(this.killListLine("Bats", killCount6, 200));
                stringBuilder.Append(this.killListLine("Skeletons", killCount3, 50));
                stringBuilder.Append(this.killListLine("CaveInsects", killCount5, 125));
                stringBuilder.Append(this.killListLine("Duggies", killCount7, 30));
                stringBuilder.Append(this.killListLine("DustSprites", monstersKilled1, 500));
                stringBuilder.Append(this.killListLine("RockCrabs", killCount4, 60));
                stringBuilder.Append(this.killListLine("Mummies", monstersKilled2, 100));
                stringBuilder.Append(this.killListLine("PepperRex", monstersKilled3, 50));
                stringBuilder.Append(this.killListLine("Serpent", killCount8, 250));
                stringBuilder.Append(this.killListLine("MagmaSprite", killCount9, 150));
                stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Footer").Replace('\n', '^'));
                Game1.drawLetterMessage(stringBuilder.ToString());

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShowMonsterKillList_CustomListFromAP_Prefix)}:\n{ex}",
                    LogLevel.Error);
                throw;
            }
        }
    }
}
