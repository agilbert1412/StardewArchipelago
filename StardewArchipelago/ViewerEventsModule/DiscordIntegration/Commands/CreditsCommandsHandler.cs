using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration.Commands
{
    internal class CreditsCommandsHandler
    {
        private readonly IBotCommunicator _communications;
        private readonly CommandReader _commandReader;

        public CreditsCommandsHandler(IBotCommunicator discord, CommandReader commandReader)
        {
            _communications = discord;
            _commandReader = commandReader;
        }

        public void HandleCreditsAdminCommands(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            creditAccounts.CreateBackup(5);

            HandleAddCredits(message, messageText, creditAccounts);
            HandleRemoveCredits(message, messageText, creditAccounts);
            HandleSetCredits(message, messageText, creditAccounts);
            HandleResetCredits(message, messageText, creditAccounts);
            HandleResetAllCredits(message, messageText, creditAccounts);
            HandleReadCreditsOfSomeone(message, messageText, creditAccounts);

            // Add Credits to everyone
        }

        public async Task HandleCreditsUserCommands(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            HandleReadCredits(message, messageText, creditAccounts);
            await HandleTransferCredits(message, messageText, creditAccounts);
            await HandleShareCredits(message, messageText, creditAccounts);
        }

        private void HandleAddCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!addcredits "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out ulong discordId, out var creditsAmount))
            {
                _communications.ReplyTo(message, "Usage: !addcredits [discordId] [amount]");
                return;
            }

            AddCredits(message, creditAccounts, discordId, creditsAmount);
        }

        private void HandleRemoveCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!removecredits "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out ulong discordId, out var creditsAmount))
            {
                _communications.ReplyTo(message, "Usage: !removecredits [discordId] [amount]");
                return;
            }

            RemoveCredits(message, creditAccounts, discordId, creditsAmount);
        }

        private void HandleSetCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!setcredits "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out ulong discordId, out var creditsAmount))
            {
                _communications.ReplyTo(message, "Usage: !setcredits [discordId] [amount]");
                return;
            }

            SetCredits(message, creditAccounts, discordId, creditsAmount);
        }

        private void HandleResetCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!resetcredits "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out ulong discordId))
            {
                _communications.ReplyTo(message, "Usage: !resetcredits [discordId]");
                return;
            }

            ResetCredits(message, creditAccounts, discordId);
        }

        private void HandleResetAllCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.Equals("!resetallcredits"))
            {
                return;
            }

            ResetAllCredits(message, creditAccounts);
        }

        private void HandleReadCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.Equals("!credits"))
            {
                return;
            }

            TellUserHisCreditAmount(message, creditAccounts);
        }

        private async Task HandleTransferCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!transfercredits "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(message.Content, out string discordName))
            {
                _communications.ReplyTo(message, $"Usage:{Environment.NewLine}!transfercredits [Username#Discriminator]{Environment.NewLine}!transfercredits random");
                return;
            }

            await TransferCreditsToSomeone(message, discordName, creditAccounts);
        }

        private async Task HandleShareCredits(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!sharecredits"))
            {
                return;
            }

            if (_commandReader.IsCommandValid(message.Content, out ulong minutes))
            {
                DistributeCreditsAmongstEveryActivePlayer(message, creditAccounts, minutes);
                return;
            }

            DistributeCreditsAmongstEveryActivePlayer(message, creditAccounts, 20);
        }

        private void HandleReadCreditsOfSomeone(SocketUserMessage message, string messageText, CreditAccounts creditAccounts)
        {
            if (!messageText.StartsWith("!credits "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out ulong discordId))
            {
                _communications.ReplyTo(message, "Usage: !credits [discordId]");
                return;
            }

            TellAdminCreditAmountOfSomeone(message, creditAccounts, discordId);
        }

        private void AddCredits(SocketUserMessage message, CreditAccounts creditAccounts, ulong discordId, int creditsAmount)
        {
            var account = creditAccounts[discordId];
            account.AddCredits(creditsAmount);
            _communications.ReplyTo(message, $"Added {creditsAmount} credits to {account.discordName}. New Balance: {account.GetCredits()}");
        }

        private void RemoveCredits(SocketUserMessage message, CreditAccounts creditAccounts, ulong discordId, int creditsAmount)
        {
            var account = creditAccounts[discordId];
            account.RemoveCredits(creditsAmount);
            _communications.ReplyTo(message, $"Removed {creditsAmount} credits from {account.discordName}. New Balance: {account.GetCredits()}");
        }

        private void ResetCredits(SocketUserMessage message, CreditAccounts creditAccounts, ulong discordId)
        {
            var account = creditAccounts[discordId];
            account.Reset();
            _communications.ReplyTo(message, $"Reset credits for {account.discordName}. New Balance: {account.GetCredits()}");
        }

        private void ResetAllCredits(SocketUserMessage message, CreditAccounts creditAccounts)
        {
            creditAccounts.ResetAll();
            _communications.ReplyTo(message, $"Reset credits for everyone!");
        }

        private void SetCredits(SocketUserMessage message, CreditAccounts creditAccounts, ulong discordId, int creditsAmount)
        {
            var account = creditAccounts[discordId];
            account.SetCredits(creditsAmount);
            _communications.ReplyTo(message, $"Set credits for {account.discordName} to {account.GetCredits()}");
        }

        private void TellUserHisCreditAmount(SocketUserMessage message, CreditAccounts creditAccounts)
        {
            var userAccount = creditAccounts[message.Author.Id];
            var creditAmount = userAccount.GetCredits();
            _communications.ReplyTo(message, $@"You currently have {creditAmount} credits.");
        }

        private void TellAdminCreditAmountOfSomeone(SocketUserMessage message, CreditAccounts creditAccounts, ulong discordId)
        {
            var userAccount = creditAccounts[discordId];
            var userName = userAccount.discordName;
            var creditAmount = userAccount.GetCredits();
            _communications.ReplyTo(message, $@"{userName} currently has {creditAmount} credits.");
        }

        private async Task TransferCreditsToSomeone(SocketUserMessage message, string targetUsername, CreditAccounts creditAccounts)
        {
            var userAccount = creditAccounts[message.Author.Id];
            var creditAmount = userAccount.GetCredits();

            CreditAccount targetAccount = null;
            if (targetUsername.ToLower() == "random")
            {
                targetAccount = creditAccounts.GetRandomAccount();
            }
            else
            {
                var userId = _communications.GetUserId(targetUsername);
                if (userId == 0)
                {
                    _communications.ReplyTo(message, $"Cannot find user {targetUsername}. The user needs to be online and be in this server.");
                    return;
                }

                targetAccount = creditAccounts[userId];
            }

            userAccount.RemoveCredits(creditAmount);
            targetAccount.AddCredits(creditAmount);
            _communications.ReplyTo(message, $@"You have transferred your entire balance of {creditAmount} credits to {targetAccount.discordName}! Their new balance: {targetAccount.GetCredits()}");
        }

        private async Task DistributeCreditsAmongstEveryActivePlayer(SocketUserMessage message, CreditAccounts creditAccounts, ulong maxMinutesSinceLastActivity)
        {
            var userAccount = creditAccounts[message.Author.Id];
            var creditAmount = userAccount.GetCredits();

            if (creditAmount <= 0)
            {
                _communications.ReplyTo(message, $"You do not have any credits to distribute");
                return;
            }

            var accountsInOrder = creditAccounts.GetAccountsActiveInThePastMinutes(maxMinutesSinceLastActivity, message.Author.Id);

            if (!accountsInOrder.Any())
            {
                _communications.ReplyTo(message, $"There are no accounts that were active in the past {maxMinutesSinceLastActivity} minutes available to distribute credits to");
                return;
            }


            var distributedCredits = new Dictionary<ulong, int>();

            for (var i = 0; i < creditAmount; i++)
            {
                var accountIndex = i % accountsInOrder.Length;
                var account = accountsInOrder[accountIndex];
                if (!distributedCredits.ContainsKey(account.discordId))
                {
                    distributedCredits.Add(account.discordId, 0);
                }

                distributedCredits[account.discordId]++;
            }


            foreach (var (discordId, creditsToTransfer) in distributedCredits)
            {
                var targetAccount = creditAccounts[discordId];
                targetAccount.AddCredits(creditsToTransfer);
                userAccount.RemoveCredits(creditsToTransfer);
            }

            var replyText =
                $@"You have distributed your entire balance of {creditAmount} among the following {distributedCredits.Count} people:";
            foreach (var (discordId, creditsToTransfer) in distributedCredits)
            {
                var targetAccount = creditAccounts[discordId];
                replyText += Environment.NewLine + "\t" + " - " + $"{targetAccount.discordName} ({creditsToTransfer} credits)";
            }

            _communications.ReplyTo(message, replyText);
        }
    }
}