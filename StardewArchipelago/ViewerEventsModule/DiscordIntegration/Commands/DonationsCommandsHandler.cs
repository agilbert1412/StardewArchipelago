using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration.Commands
{
    internal class DonationsCommandsHandler
    {
        private readonly IBotCommunicator _communications;
        private readonly ChannelSet _channels;

        public DonationsCommandsHandler(IBotCommunicator discord, ChannelSet channels)
        {
            _communications = discord;
            _channels = channels;
        }

        public async Task HandleEventsDonationCommands(SocketUserMessage message, CreditAccounts accounts)
        {
            await HandleDonationCommand(message, accounts);
        }

        private async Task HandleDonationCommand(SocketUserMessage message, CreditAccounts accounts)
        {
            // 'St. John Johnson just donated $50.00 with the message "Happy National Child Health Day"!'
            var donationRegex = new Regex(@"(.+) just donated \$(.+) with the message ""(.*)""!", RegexOptions.IgnoreCase);
            if (!donationRegex.IsMatch(message.Content))
            {
                _communications.ReplyTo(message, $"Message detected in the donation channel, but doesn't appear to be a donation...");
                return;
            }

            var match = donationRegex.Match(message.Content);
            var groups = match.Groups;

            if (groups.Count < 4)
            {
                _communications.ReplyTo(message, $"Donation detected, but donation message was improperly formatted. {_channels.AdminPing} can you help?");
                return;
            }

            if (!double.TryParse(groups[2].Value, out var donationAmount))
            {
                _communications.ReplyTo(message, $"Donation detected, but couldn't read the dollar amount. {_channels.AdminPing} can you help?");
                return;
            }

            var creditsEarned = (int)Math.Round(donationAmount * 100);

            var usersInChannel = await _communications.GetUsersInChannel(message.Channel);
            var usersMentioned = new List<IUser>();
            foreach (var user in usersInChannel)
            {
                if (UserIsMentioned(message.Content, user, true))
                {
                    usersMentioned.Add(user);
                }
            }

            if (!usersMentioned.Any())
            {
                foreach (var user in usersInChannel)
                {
                    if (UserIsMentioned(message.Content, user, false))
                    {
                        usersMentioned.Add(user);
                    }
                }
            }

            if (!usersMentioned.Any())
            {
                _communications.ReplyTo(message, $"Donation detected, but couldn't find a username in it. {_channels.AdminPing} can you help?");
                return;
            }

            var creditPerUser = creditsEarned / usersMentioned.Count;
            foreach (var userToGiveCreditsTo in usersMentioned)
            {
                var discordId = userToGiveCreditsTo.Id;
                var account = accounts[discordId];
                account.AddCredits(creditPerUser);
                await _communications.ReplyToAsync(message, $"Donation registered for <@{account.discordId}>. Added {creditPerUser} credits to your account. New Balance: {account.GetCredits()}. Thank you for donating!");
            }
        }

        private bool UserIsMentioned(string message, IUser user, bool strict)
        {
            var delimitingChars = new string[] { " ", "\"", ".", "(", ")", "[", "]", "{", "}", "-", "_", ",", "!", "?" };
            if (!string.IsNullOrWhiteSpace(user.GlobalName))
            {
                foreach (var charBefore in delimitingChars)
                {
                    foreach (var charAfter in delimitingChars)
                    {
                        if (message.Contains($"{charBefore}{user.GlobalName}{charAfter}"))
                        {
                            return true;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                foreach (var charBefore in delimitingChars)
                {
                    foreach (var charAfter in delimitingChars)
                    {
                        if (message.Contains($"{charBefore}{user.Username}{charAfter}"))
                        {
                            return true;
                        }
                    }
                }
            }

            if (strict)
            {
                return false;
            }

            return (!string.IsNullOrWhiteSpace(user.GlobalName) && message.Contains(user.GlobalName)) ||
                   (!string.IsNullOrWhiteSpace(user.Username) && message.Contains(user.Username));
        }

        private void OldDiscordIdMatcher()
        {
            //var discordIdRegex = new Regex("([^ ]+)#([0-9]{4})");
            //if (!discordIdRegex.IsMatch(donationMessage))
            //{
            //    _communications.ReplyTo(message, $"Donation detected, but couldn't find username. {_channels.AdminPing} can you help?");
            //    return;
            //}

            //var donatorMatch = discordIdRegex.Match(donationMessage);
            //var donatorGroups = donatorMatch.Groups;

            //if (donatorGroups.Count < 3)
            //{
            //    _communications.ReplyTo(message, $"Donation detected, but username was improperly formatted. {_channels.AdminPing} can you help?");
            //    return;
            //}

            //var discordName = donatorGroups[1].Value;
            //var discordDiscriminator = donatorGroups[2].Value;
            //var discordId = _communications.GetUserId(discordName, discordDiscriminator);

            //if (discordId == 0)
            //{
            //    _communications.ReplyTo(message, $"Donation detected, but couldn't find user {discordName}#{discordDiscriminator}. {_channels.AdminPing} can you help?");
            //    return;
            //}
        }
    }
}