using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Jojapocalypse
{
    public class JojaLocation
    {
        private string LocationName { get; set; }

        private Dictionary<string, int> PreRequisites { get; set; }

        public JojaLocation(string name)
        : this(name, new Dictionary<string, int>())
        {
        }

        public JojaLocation(string name, Dictionary<string, int> preRequisites)
        {
            LocationName = name;
            PreRequisites = new Dictionary<string, int>(preRequisites);
        }
    }
}
