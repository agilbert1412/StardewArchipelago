using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace StardewArchipelago.GameModifications.EntranceRandomizer
{
    public class EquivalentWarps
    {
        private static string[] _jojaMartLocations = new[] { "JojaMart", "AbandonedJojaMart", "MovieTheater" };
        private static string[] _trailerLocations = new[] { "Trailer", "Trailer_Big" };

        public List<string[]> EquivalentAreas = new()
        {
            _jojaMartLocations,
            _trailerLocations,
        };

        private Dictionary<string, OneWayEntrance> _allEntrances;

        public EquivalentWarps()
        {
        }

        public void SetAllEntrances(Dictionary<string, OneWayEntrance> allEntrances)
        {
            _allEntrances = allEntrances;
        }

        public OneWayEntrance GetCorrectEquivalentWarp(OneWayEntrance chosenWarp)
        {
            if (IsJojaMart(chosenWarp.DestinationName, out var jojaMartCorrectWarp))
            {
                return jojaMartCorrectWarp;
            }

            if (IsTrailer(chosenWarp.DestinationName, out var trailerCorrectWarp))
            {
                return trailerCorrectWarp;
            }

            return chosenWarp;
        }

        private bool IsJojaMart(string destinationName, out OneWayEntrance correctWarp)
        {
            correctWarp = null;
            if (!_jojaMartLocations.Contains(destinationName))
            {
                return false;
            }
            
            if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
            {
                correctWarp = _allEntrances["Town to MovieTheater"];
                return true;
            }

            const int brokenJojaDoorEventId = 191393;
            if (Utility.HasAnyPlayerSeenEvent(brokenJojaDoorEventId))
            {
                correctWarp = _allEntrances["Town to AbandonedJojaMart"];
                return true;
            }

            correctWarp = _allEntrances["Town to JojaMart"];
            return true;
        }

        private bool IsTrailer(string destinationName, out OneWayEntrance correctWarp)
        {
            correctWarp = null;
            if (!_trailerLocations.Contains(destinationName))
            {
                return false;
            }

            if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
            {
                correctWarp = _allEntrances["Town to Trailer_Big"];
                return true;
            }

            correctWarp = _allEntrances["Town to Trailer"];
            return true;
        }
    }
}
