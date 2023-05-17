using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
