using System;
using StardewArchipelago.Constants.Modded;

namespace StardewArchipelago.Archipelago
{
    public class VersionValidator
    {
        public bool IsVersionCorrect(string version, string expectedVersion)
        {
            if (version.Equals(expectedVersion, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            var versionDigits = version.Split('.');
            var expectedVersionDigits = expectedVersion.Split('.');
            if (versionDigits.Length != expectedVersionDigits.Length)
            {
                return false;
            }

            for (var i = 0; i < versionDigits.Length; i++)
            {
                var versionDigit = versionDigits[i];
                var expectedVersionDigit = expectedVersionDigits[i];

                if (versionDigit.Equals(expectedVersionDigit, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (expectedVersionDigit.Equals(ModVersions.WILDCARD, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
