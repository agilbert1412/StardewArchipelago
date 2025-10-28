using Discord;
using Discord.WebSocket;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.ViewerEventsModule.DiscordIntegration.Commands;
using StardewArchipelago.ViewerEventsModule.EventsExecution;
using System;
using System.Threading.Tasks;

namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration
{
    internal class DiscordModule
    {
        private readonly ILogger _logger;
        private readonly IBotCommunicator _communications;
        private readonly ViewerEventsExecutor _eventsExecutor;

        private readonly ChannelSet _activeChannels = ChannelSet.ExtraLifeChannels;

        private readonly CreditsCommandsHandler _creditsCommandsHandler;
        private readonly EventsCommandsHandler _eventsCommandsHandler;
        private readonly DonationsCommandsHandler _donationsCommandsHandler;
        private readonly CommandReader _commandReader;
        private readonly HelpProvider _helpProvider;
        private readonly CreditAccounts _accounts;

        public static Random random = new Random();

        public const string EVENTS_FILE = "EventsList.json";
        public const string CREDITS_FILE = "Credits.json";

        public DiscordModule(ILogger logger, IBotCommunicator communications, ViewerEventsExecutor eventsExecutor)
        {
            _logger = logger;
            _communications = communications;
            _eventsExecutor = eventsExecutor;
            _commandReader = new CommandReader();
            _helpProvider = new HelpProvider(_communications, _activeChannels);
            _creditsCommandsHandler = new CreditsCommandsHandler(_communications, _commandReader);
            _eventsCommandsHandler = new EventsCommandsHandler(_communications, _commandReader, _helpProvider);
            _donationsCommandsHandler = new DonationsCommandsHandler(_communications, _activeChannels);
            _accounts = new CreditAccounts(_communications);

            SetupData();


            // ClearBankDEVONLY();
            //return;
        }

        public async Task InitializeAsync()
        {
        }

        public async Task ExecuteViewerCommand(SocketUserMessage message)
        {
            var messageText = message.Content.ToLower();
            var sender = message.Author;
            var senderName = sender.Username;

            _logger.LogInfo($"{senderName} said '{messageText}'");

            HandleAdminCommands(message, messageText);
            await HandleUserCommands(message, messageText, senderName);
            await HandleDonationCommands(message);
            ExportData();
        }

        private async void HandleAdminCommands(SocketUserMessage message, string messageText)
        {
            if (!IsInAdminChannel(message))
            {
                return;
            }

            if (messageText == "test")
            {
                await message.ReplyAsync("Toast");
            }

            _creditsCommandsHandler.HandleCreditsAdminCommands(message, messageText, _accounts);
            _eventsCommandsHandler.HandleEventsAdminCommands(message, messageText, _eventsExecutor);

            if (messageText.Equals("!help"))
            {
                _helpProvider.SendAllHelpMessages(_eventsExecutor.Events);
            }
        }

        private async Task HandleUserCommands(SocketUserMessage message, string messageText, string senderName)
        {
            if (!(IsInAdminChannel(message) || IsInEventsChannel(message)))
            {
                return;
            }

            await _creditsCommandsHandler.HandleCreditsUserCommands(message, messageText, _accounts);
            await _eventsCommandsHandler.HandleEventsUserCommands(message, messageText, _accounts, _eventsExecutor);
        }

        private async Task HandleDonationCommands(SocketUserMessage message)
        {
            if (!(IsInDonationsChannel(message)))
            {
                return;
            }

            await _donationsCommandsHandler.HandleEventsDonationCommands(message, _accounts);
        }

        private void ExportData()
        {
            ExportEvents();
            ExportCredits();
            // Export Queue
        }

        private void ClearBankDEVONLY()
        {
            //TODO: Plug into an admin command

            _logger.LogInfo("DEV ONLY: Reset the bank for everything!");
            _eventsExecutor.Events.ClearAllBanks();
            
            ExportData();
            Environment.Exit(0);
        }

        private void SetupData()
        {
            ImportEvents();
            ImportCredits();
            // Import queue;
        }

        private void ImportEvents()
        {
            _eventsExecutor.Events.ImportFrom(EVENTS_FILE);
        }

        private void ImportCredits()
        {
            _accounts.ImportFrom(CREDITS_FILE);
        }

        private void ExportEvents()
        {
            _eventsExecutor.Events.ExportTo(EVENTS_FILE);
        }

        private void ExportCredits()
        {
            _accounts.ExportTo(CREDITS_FILE);
        }

        private bool IsInAdminChannel(SocketMessage message)
        {
            return message.Channel.Id == _activeChannels.AdminChannel;
        }

        private bool IsInEventsChannel(SocketMessage message)
        {
            return message.Channel.Id == _activeChannels.EventsChannel;
        }

        private bool IsInDonationsChannel(SocketMessage message)
        {
            return message.Channel.Id == _activeChannels.DonationsChannel;
        }
    }
}
