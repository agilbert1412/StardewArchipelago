using System;
using System.Collections.Generic;
using Force.DeepCloner;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class Mailman
    {
        private static readonly Random _random = new Random();
        private bool _sendForTomorrow = true;

        private Dictionary<string, string> _lettersGenerated;

        public Mailman(Dictionary<string, string> lettersGenerated)
        {
            _lettersGenerated = lettersGenerated.DeepClone();
            foreach (var (mailKey, mailContent) in lettersGenerated)
            {
                var mailData = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
                mailData[mailKey] = mailContent;
            }
        }

        public void SendVanillaMail(string mailTitle, bool noLetter)
        {
            if (Game1.player.mailReceived.Contains(mailTitle))
            {
                return;
            }

            Game1.player.mailForTomorrow.Add(mailTitle + (noLetter ? "%&NL&%" : ""));
        }

        public void SendArchipelagoInvisibleMail(string mailKey, string apItemName, string findingPlayer, string locationName)
        {
            if (Game1.player.hasOrWillReceiveMail(mailKey))
            {
                return;
            }

            GenerateMail(mailKey, apItemName, findingPlayer, locationName, "");
            Game1.player.mailForTomorrow.Add(mailKey + "%&NL&%");
        }

        public void SendArchipelagoMail(string mailKey, string apItemName, string findingPlayer, string locationName, string attachmentEmbedString)
        {
            if (Game1.player.hasOrWillReceiveMail(mailKey))
            {
                return;
            }

            GenerateMail(mailKey, apItemName, findingPlayer, locationName, attachmentEmbedString);

            Game1.player.mailForTomorrow.Add(mailKey);
        }

        private void GenerateMail(string mailKey, string apItemName, string findingPlayer, string locationName,
            string embedString)
        {
            apItemName = apItemName.Replace("<3", "<");
            var mailData = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            var mailContent = string.Format(GetRandomApMailString(), apItemName, findingPlayer, locationName, embedString);
            mailData[mailKey] = mailContent;
            _lettersGenerated.Add(mailKey, mailContent);
        }

        public int OpenedMailsContainingKey(string apItemName)
        {
            var numberReceived = 0;
            foreach (var mail in Game1.player.mailReceived)
            {
                if (!mail.Contains($"AP|{apItemName}"))
                {
                    continue;
                }

                numberReceived++;
            }

            return numberReceived;
        }

        public Dictionary<string, string> GetAllLettersGenerated()
        {
            return _lettersGenerated.DeepClone();
        }

        public void SendToday()
        {
            _sendForTomorrow = false;
        }

        public void SendTomorrow()
        {
            _sendForTomorrow = true;
        }

        private string GetRandomApMailString()
        {
            return ApMailStrings[_random.Next(0, ApMailStrings.Length)];
        }

        private static readonly string[] ApMailStrings = {
            "Hey @, I was at {2}, minding my own business, and there I found a {0}.^I thought you would make better use of it than I ever could.^^    -{1}{3}[#]Archipelago Item",
            "I found a {0} in {2}.^Enjoy!^^    -{1}{3}[#]Archipelago Item",
            "There was a {0} in my {2}.^Do you think you can make it useful?^^    -{1}{3}[#]Archipelago Item",
        };
    }
}
