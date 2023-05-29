using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Locations.Patcher
{
    public interface ILocationPatcher
    {
        void ReplaceAllLocationsRewardsWithChecks();
    }
}
