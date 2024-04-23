namespace StardewArchipelago.Extensions
{
    public static class StringExtensions
    {
        public static string ToCapitalized(this string word)
        {
            if (word.Length < 2)
            {
                return word.ToUpper();
            }

            return word[..1].ToUpper() + word[1..].ToLower();
        }

        public static string TurnHeartsIntoStardewHearts(this string messageWithHearts)
        {
            messageWithHearts = messageWithHearts.Replace("<3", "<");
            return messageWithHearts;
        }
    }
}
