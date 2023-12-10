using StardewArchipelago.Stardew;

namespace StardewArchipelago.Locations.Modded.SVE
{
    internal class FriendshipReleaser
    {
        private LocationChecker _locationChecker;
        private BundleReader _bundleReader;

        public FriendshipReleaser(LocationChecker locationChecker, BundleReader bundleReader)
        {
            _locationChecker = locationChecker;
            _bundleReader = bundleReader;
        }

        public void ReleaseMorrisHeartsIfNeeded()
        {
            if (!_bundleReader.IsCommunityCenterComplete())
            {
                return;
            }

            ReleaseAllMorrisHearts();
        }

        private void ReleaseAllMorrisHearts()
        {
            const string morrisFriendsanityLocation = "Friendsanity: Morris {0} <3";
            for (var i = 1; i <= 10; i++)
            {
                _locationChecker.AddCheckedLocation(string.Format(morrisFriendsanityLocation, i));
            }
        }
    }
}
