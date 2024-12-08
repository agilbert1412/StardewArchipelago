using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Locations
{
    public class JojapocalypseManager
    {
        public HashSet<string> JojaCheckedLocations { get; set; }

        public JojapocalypseManager()
        {

        }

        public void AddCheckedLocations(List<string> locations)
        {
            JojaCheckedLocations.UnionWith(locations);
        }
    }
}
