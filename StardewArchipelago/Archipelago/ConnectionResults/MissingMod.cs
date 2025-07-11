namespace StardewArchipelago.Archipelago.ConnectionResults
{
    public class MissingMod : ProblemMod
    {
        public string ModExpectedVersion { get; set; }
        public string ModCurrentVersion { get; set; }

        public MissingMod(string modName, string modExpectedVersion, string modCurrentVersion) : base(modName)
        {
            ModExpectedVersion = modExpectedVersion;
            ModCurrentVersion = modCurrentVersion;
        }
    }
}
