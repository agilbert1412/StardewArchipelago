namespace StardewArchipelago.Archipelago.ConnectionResults
{
    public class IncompatibleMod : ProblemMod
    {
        public string Justification { get; set; }

        public IncompatibleMod(string modName, string justification) : base(modName)
        {
            Justification = justification;
        }
    }
}
