namespace StardewArchipelago.Stardew.NameMapping
{
    public interface INameMapper
    {
        string GetEnglishName(string internalName);
        string GetInternalName(string englishName);
    }
}
