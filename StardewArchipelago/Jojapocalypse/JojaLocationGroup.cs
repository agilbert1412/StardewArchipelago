using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace StardewArchipelago.Jojapocalypse
{
    public class JojaLocationGroup
    {
        public string Name { get; set; }
        public JojaShop Shop { get; set; }
        public List<JojaLocation> Locations { get; set; }
        private List<JojaPatch> _patches;
        private bool _isPatched;

        public JojaLocationGroup(string name, JojaShop shop, IEnumerable<JojaLocation> locations)
            : this(name, shop, locations, new List<JojaPatch>())
        {
        }

        public JojaLocationGroup(string name, JojaShop shop, IEnumerable<JojaLocation> locations, List<JojaPatch> patches)
        {
            Name = name;
            Shop = shop;
            Locations = new List<JojaLocation>(locations);
            _patches = patches;
            _isPatched = false;
        }

        public void Patch(Harmony harmony)
        {
            if (_isPatched)
            {
                return;
            }

            foreach (var jojaPatch in _patches)
            {
                harmony.Patch(original: jojaPatch.Original,
                    prefix: jojaPatch.Prefix,
                    postfix: jojaPatch.Postfix);
            }

            _isPatched = true;
        }

        public void AddPatch(JojaPatch patch)
        {
            _patches.Add(patch);
        }
    }

}