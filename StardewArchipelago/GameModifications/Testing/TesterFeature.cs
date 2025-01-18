namespace StardewArchipelago.GameModifications.Testing
{
    public class TesterFeature
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public TesterFeature(string name, int defaultValue)
        {
            Name = name;
            Value = defaultValue;
        }
    }
}
