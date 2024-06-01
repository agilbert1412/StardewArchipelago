using System.Collections.Generic;

namespace StardewArchipelago.Constants.Modded
{
    public class ModVersions
    {
        public static readonly Dictionary<string, string> Versions = new()
        {
            { ModNames.ALEC, "2.2.1" },
            { ModNames.ALECTO, "1.1.10" },
            { ModNames.ARCHAEOLOGY, "2.10.3" },
            { ModNames.AYEISHA, "0.7.3-alpha" },
            { ModNames.BIGGER_BACKPACK, "7.1.0" },
            { ModNames.BINNING, "2.0.5" },
            { ModNames.BOARDING_HOUSE, "4.0.16" },
            { ModNames.COOKING, "1.4.5" },
            { ModNames.DEEP_WOODS, "3.1.0-beta" },
            { ModNames.DELORES, "1.1.2" },
            { ModNames.DISTANT_LANDS, "1.0.8" },
            { ModNames.EUGENE, "1.3.1" },
            { ModNames.JASPER, "1.8.2" },
            { ModNames.JUNA, "2.1.5" },
            { ModNames.LACEY, "1.1.3" },
            { ModNames.LUCK, "1.2.6" },
            { ModNames.MAGIC, "0.8.2" },
            { ModNames.MISTER_GINGER, "1.6.2" },
            { ModNames.RILEY, "1.2.2" },
            { ModNames.SHIKO, "1.1.0" }, // The mod is 1.2.0 but they didn't update the manifest.json
            { ModNames.SKULL_CAVERN_ELEVATOR, "1.6.1" },
            { ModNames.SOCIALIZING, "2.0.6" },
            { ModNames.SVE, "1.14.24" },
            { ModNames.TRACTOR, "4.19.0" },
            { ModNames.WELLWICK, "1.0.0" },
            { ModNames.YOBA, "1.0.0" },
        };

        public class ContentPatcherRequirement
        {
            public string ContentPatcherMod { get; private set; }
            public string ContentPatcherVersion { get; private set; }

            public ContentPatcherRequirement(string patchName, string patchVersion)
            {
                ContentPatcherMod = patchName;
                ContentPatcherVersion = patchVersion;
            }
        }

        public static readonly Dictionary<string, ContentPatcherRequirement> CPVersions = new()
        {
            { ModNames.SVE, new ContentPatcherRequirement(ModNames.AP_SVE, "1.1.0") },
            { ModNames.JASPER, new ContentPatcherRequirement(ModNames.AP_JASPER, "1.0.0") },
        };
    }
}
