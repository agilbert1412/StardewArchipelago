namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration.Commands
{
    internal class CommandReader
    {
        public bool IsCommandValid(string messageText, out string arg1, out int arg2)
        {
            var numberParts = 3;
            var lineparts = messageText.Split(" ", numberParts);

            arg1 = "";
            arg2 = 0;

            if (lineparts.Length < numberParts)
            {
                return false;
            }
            
            arg1 = lineparts[1];

            if (!int.TryParse(lineparts[2], out arg2))
            {
                return false;
            }

            return true;
        }

        public bool IsCommandValid(string messageText, out ulong arg1, out int arg2)
        {
            var numberParts = 3;
            var lineparts = messageText.Split(" ", numberParts);

            arg1 = 0;
            arg2 = 0;

            if (lineparts.Length < numberParts)
            {
                return false;
            }

            if (!ulong.TryParse(lineparts[1], out arg1))
            {
                return false;
            }

            if (!int.TryParse(lineparts[2], out arg2))
            {
                return false;
            }

            return true;
        }

        public bool IsCommandValid(string messageText, out ulong arg1)
        {
            var numberParts = 2;
            var lineparts = messageText.Split(" ", numberParts);

            arg1 = 0;

            if (lineparts.Length < numberParts)
            {
                return false;
            }

            if (!ulong.TryParse(lineparts[1], out arg1))
            {
                return false;
            }

            return true;
        }

        public bool IsCommandValid(string messageText, out string arg1)
        {
            var numberParts = 2;
            var lineparts = messageText.Split(" ", numberParts);

            arg1 = "";

            if (lineparts.Length < numberParts)
            {
                return false;
            }

            arg1 = lineparts[1];

            return true;
        }

        public bool IsCommandValid(string messageText, out double arg1)
        {
            var numberParts = 2;
            var lineparts = messageText.Split(" ", numberParts);

            arg1 = 0;

            if (lineparts.Length < numberParts)
            {
                return false;
            }

            if (!double.TryParse(lineparts[1], out arg1))
            {
                return false;
            }

            return true;
        }
    }
}
