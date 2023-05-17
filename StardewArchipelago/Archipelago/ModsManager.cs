using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Constants;

namespace StardewArchipelago.Archipelago
{
    public class ModsManager
    {
        private Dictionary<string, string> _activeMods;

        public ModsManager(Dictionary<string, string> activeMods)
        {
            _activeMods = activeMods;
        }

        public bool IsModded => _activeMods.Any();

        public bool HasMod(string modName)
        {
            return _activeMods.ContainsKey(modName);
        }

        public string GetVersion(string modName)
        {
            return _activeMods[modName];
        }

        public bool HasModdedSkill()
        {
            return HasMod(ModNames.LUCK) || HasMod(ModNames.BINNING) || HasMod(ModNames.COOKING) ||
                   HasMod(ModNames.MAGIC) || HasMod(ModNames.SOCIALIZING) || HasMod(ModNames.ARCHAEOLOGY);
        }
    }
}
