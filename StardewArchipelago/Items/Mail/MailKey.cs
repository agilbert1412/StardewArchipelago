using System;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class MailKey
    {
        private const string AP_PREFIX = "AP";
        private const string AP_DELIMITER = "|";
        private static Random _random = new Random((int)Game1.uniqueIDForThisGame);
        public string ItemName { get; private set; }
        public string PlayerName { get; private set; }
        public string LocationName { get; private set; }
        public string LetterOpenedAction { get; private set; }
        public string ActionParameter { get; private set; }
        public string EmbedString { get; private set; }
        private string UniqueId { get; set; }
        public bool IsEmpty { get; set; }

        public MailKey(string itemName, string playerName, string locationName, string embedString, string uniqueId, bool isEmpty)
            : this(itemName, playerName, locationName, "", "", "", uniqueId, isEmpty)
        {
        }

        public MailKey(string itemName, string playerName, string locationName, string letterOpenedAction, string actionParameter, string embedString, string uniqueId, bool isEmpty)
        {
            ItemName = Sanitize(itemName);
            PlayerName = Sanitize(playerName);
            LocationName = Sanitize(locationName);
            LetterOpenedAction = Sanitize(letterOpenedAction);
            ActionParameter = Sanitize(actionParameter);
            EmbedString = Sanitize(embedString);
            UniqueId = Sanitize(uniqueId);
            IsEmpty = isEmpty;
        }

        private static string Sanitize(string input)
        {
            return input.Replace(AP_DELIMITER, "");
        }

        public override string ToString()
        {
            var key = $"{AP_PREFIX}{AP_DELIMITER}{ItemName}{AP_DELIMITER}{PlayerName}{AP_DELIMITER}{LocationName}{AP_DELIMITER}{LetterOpenedAction}{AP_DELIMITER}{ActionParameter}{AP_DELIMITER}{UniqueId}{AP_DELIMITER}{IsEmpty}";
            var trimmedKey = key.Replace(" ", "_");
            return trimmedKey;
        }

        public static bool TryParse(string key, out MailKey mailKey)
        {
            mailKey = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (!key.StartsWith(AP_PREFIX))
            {
                return false;
            }

            var splitKey = key.Split(AP_DELIMITER);
            if (splitKey.Length < 7)
            {
                return false;
            }

            var itemName = splitKey[1].Replace("_", " ");
            var playerName = splitKey[2].Replace("_", " ");
            var locationName = splitKey[3].Replace("_", " ");
            var letterOpenedAction = splitKey[4];
            var actionParameter = splitKey[5];
            var embedString = "";
            var uniqueId = splitKey[6];
            if (splitKey.Length <= 7 || !bool.TryParse(splitKey[7], out var isEmpty))
            {
                isEmpty = false;
            }

            mailKey = new MailKey(itemName, playerName, locationName, letterOpenedAction, actionParameter, embedString, uniqueId, isEmpty);
            return true;
        }

        public static string GetBeginningOfKeyForItem(string itemName)
        {
            return $"{AP_PREFIX}{AP_DELIMITER}{Sanitize(itemName)}".Replace(" ", "_");
        }
    }
}
