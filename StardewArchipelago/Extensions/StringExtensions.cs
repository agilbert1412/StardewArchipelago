using Archipelago.MultiClient.Net.Helpers;
using System.Diagnostics;
using System;
using StardewArchipelago.Constants;

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

        public static string ToAnonymousName(this string name)
        {
            if (name == null)
            {
                Debugger.Break();
                return name;
            }

            // Frazz -> 125
            // Kimou00 -> 99
            // Cap -> 57
            // Honeywell -> 8

            var hash = name.GetHash();
            var random = new Random(hash);
            var animalIndex = random.Next(AnimalNames.AllNames.Length);
            return AnimalNames.AllNames[animalIndex];
        }

        public static string TurnHeartsIntoStardewHearts(this string messageWithHearts)
        {
            messageWithHearts = messageWithHearts.Replace("<3", "<");
            return messageWithHearts;
        }

        public static string AnonymizePlayerNames(this string messageContent, IPlayerHelper players)
        {
            if (!ModEntry.Instance.Config.AnonymizeNamesInChat)
            {
                return messageContent;
            }

            foreach (var playerInfo in players.AllPlayers)
            {
                if (messageContent.Contains(playerInfo.Name))
                {
                    messageContent = AnonymizePlayerName(messageContent, playerInfo.Name);
                }

                if (messageContent.Contains(playerInfo.Alias))
                {
                    messageContent = AnonymizePlayerName(messageContent, playerInfo.Alias);
                }
            }

            return messageContent;
        }

        private static string AnonymizePlayerName(string messageContent, string name)
        {
            var anonymizedMessage = messageContent;
            var anonymousName = name.ToAnonymousName();
            var delimitingChars = new[] { " ", ".", "?", "!", "(", ")", ":", ";", "[", "]", "{", "}", "*", "&", "¬", "%", "$", "#", "/", @"\", "|", "@", "" };
            foreach (var startChar in delimitingChars)
            {
                foreach (var endChar in delimitingChars)
                {
                    if (startChar == "" && endChar == "" && anonymizedMessage == name)
                    {
                        anonymizedMessage = anonymousName;
                    }
                    var toReplace = $"{name}{endChar}";
                    if (startChar == "" && endChar != "" && anonymizedMessage.StartsWith(toReplace))
                    {
                        anonymizedMessage = $"{anonymousName}{endChar}" + anonymizedMessage.Substring(toReplace.Length);
                    }
                    toReplace = $"{startChar}{name}";
                    if (startChar != "" && endChar == "" && anonymizedMessage.EndsWith(toReplace))
                    {
                        anonymizedMessage = anonymizedMessage.Substring(0, anonymizedMessage.Length - toReplace.Length) + $"{startChar}{anonymousName}";
                    }
                    toReplace = $"{startChar}{name}{endChar}";
                    if (startChar != "" && endChar != "" && anonymizedMessage.Contains(toReplace))
                    {
                        anonymizedMessage = anonymizedMessage.Replace(toReplace, $"{startChar}{anonymousName}{endChar}");
                    }
                }
            }

            return anonymizedMessage;
        }
    }
}
