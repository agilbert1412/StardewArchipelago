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

            SendMail(mailTitle + (noLetter ? "%&NL&%" : ""));
        }

        public void SendArchipelagoInvisibleMail(string mailKey, string apItemName, string findingPlayer, string locationName)
        {
            if (Game1.player.hasOrWillReceiveMail(mailKey))
            {
                return;
            }

            GenerateMail(mailKey, apItemName, findingPlayer, locationName, "");
            SendMail(mailKey + "%&NL&%");
        }

        public void SendArchipelagoMail(string mailKey, string apItemName, string findingPlayer, string locationName, string attachmentEmbedString)
        {
            if (Game1.player.hasOrWillReceiveMail(mailKey))
            {
                return;
            }

            GenerateMail(mailKey, apItemName, findingPlayer, locationName, attachmentEmbedString);

            SendMail(mailKey);
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

        private void SendMail(string mailTitle)
        {
            if (_sendForTomorrow)
            {
                Game1.player.mailForTomorrow.Add(mailTitle);
            }
            else
            {
                if (mailTitle.Contains("%&NL&%"))
                {
                    var cleanedTitle = mailTitle.Replace("%&NL&%", "");
                    if (!Game1.player.mailReceived.Contains(cleanedTitle))
                    {
                        Game1.player.mailReceived.Add(cleanedTitle);
                    }
                }
                else
                {
                    Game1.mailbox.Add(mailTitle);
                }
            }
        }

        // 0: Item
        // 1: Sender
        // 2: Location
        // 3: Embed
        private static readonly string[] ApMailStrings = {
            "Hey @, I was at {2}, minding my own business, and there I found a {0}.^I thought you would make better use of it than I ever could.^^    -{1}{3}[#]Archipelago Item",
            "I found a {0} in {2}.^Enjoy!^^    -{1}{3}[#]Archipelago Item",
            "There was a {0} in my {2}.^Do you think you can make it useful?^^    -{1}{3}[#]Archipelago Item",
            "Hey @, I was passing by {2} when I found a {0}. You gotta stop leaving your stuff lying around!^^    -{1}{3}[#]Archipelago Item",
            "This is an official notice. You have been charged and found guilty of 1 case of Littering within multiworld bounds. {1} was able to identify the {0} left near {2} as yours, so it has been returned to you. Please leave your trash in appropriate receptacles.^^    -APPD{3}[#]Archipelago Item",
            "Hello Valued Customer!^At Jojamart, we understand how hard it can be to get the things you need for a price you can pay.^Please take this free* gift, a {0}, as a token of our good faith.^We would greatly appreciate it if you consider your pre-approved credit card offer enclosed.^^*No monetary costs were incurred by Jojamart  during the procurement of this item from {2}. The mental health of the affected employee {1} is classified.^^    -Joja Customer Representative{3}[#]Archipelago Item",
            "I was thinking of you today, so when I saw this {0} at {2}, I thought it would make a perfect gift!^^    -{1}{3}[#]Archipelago Item",
            "I know, I know, your birthday was a while ago. Sorry this is so late. It took me a while to get this {0}. It was only being sold at {2}, can you believe that?!^^    -{1}{3}[#]Archipelago Item",
            "It's dangerous to go alone. Take this! ({0})^^    -{1}{3}[#]Archipelago Item",
        };
    }
}
