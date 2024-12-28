﻿using System.Collections.Generic;

namespace StardewArchipelago.Constants.Modded
{
    public class ModVersions
    {
        public const string WILDCARD = "x";

        public static readonly Dictionary<string, string> Versions = new()
        {
            { ModNames.ALEC, $"2.2.{WILDCARD}" },
            { ModNames.ALECTO, $"1.1.{WILDCARD}" },
            { ModNames.ARCHAEOLOGY, $"2.12.1" },
            { ModNames.AYEISHA, $"0.7.{WILDCARD}" },
            { ModNames.BIGGER_BACKPACK, $"7.3.{WILDCARD}" },
            { ModNames.BINNING, $"2.1.{WILDCARD}" },
            //{ ModNames.COOKING, "1.4.5" },
            //{ ModNames.DEEP_WOODS, "3.1.0-beta" },
            //{ ModNames.DELORES, "1.1.2" },
            //{ ModNames.EUGENE, "1.3.1" },
            { ModNames.JASPER, $"1.8.3" },
            { ModNames.JUNA, $"2.1.{WILDCARD}" },
            { ModNames.LUCK, $"1.3.{WILDCARD}" },
            //{ ModNames.MAGIC, "0.8.2" },
            { ModNames.MISTER_GINGER, $"1.6.{WILDCARD}" },
            //{ ModNames.RILEY, "1.2.2" },
            //{ ModNames.SHIKO, "1.1.0" }, // The mod is 1.2.0 but they didn't update the manifest.json
            { ModNames.SKULL_CAVERN_ELEVATOR, $"1.6.{WILDCARD}" },
            { ModNames.SOCIALIZING, $"2.1.{WILDCARD}" },
            { ModNames.TRACTOR, $"4.20.{WILDCARD}" },
            //{ ModNames.WELLWICK, "1.0.0" },
            //{ ModNames.YOBA, "1.0.0" },
            { ModNames.SVE, $"1.14.{WILDCARD}" },
            { ModNames.DISTANT_LANDS, $"2.1.{WILDCARD}" },
            { ModNames.LACEY, $"1.3.{WILDCARD}" },
            //{ ModNames.BOARDING_HOUSE, "4.0.16" },
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
            { ModNames.SVE, new ContentPatcherRequirement(ModNames.AP_SVE, "2.1.x") },
        };
    }
}
