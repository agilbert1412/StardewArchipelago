namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration
{
    internal class ChannelSet
    {
        public ulong DebugChannel { get; set; }
        public ulong DonationsChannel { get; set; }
        public ulong EventsChannel { get; set; }
        public ulong ChatChannel { get; set; }
        public ulong HelpCommandsChannel { get; set; }
        public ulong HelpGenericEventsChannel { get; set; }
        public ulong HelpCurrentMissionEventsChannel { get; set; }
        public ulong HelpUnitSwapEventsChannel { get; set; }
        public ulong HelpSpearOfAdunEventsChannel { get; set; }
        public ulong AdminHelpChannel { get; set; }
        public ulong AdminChannel { get; set; }
        public string AdminPing { get; set; }

        public static ChannelSet TestMyBotsChannels = new ChannelSet()
        {
            DebugChannel = 1031671840277536849, // https://discord.com/channels/348202850486190081/1031671840277536849
            DonationsChannel = 1032011978539028612, // https://discord.com/channels/348202850486190081/1032011978539028612
            EventsChannel = 1032011997912518727, // https://discord.com/channels/348202850486190081/1032011997912518727
            ChatChannel = 1032012012382847126, // https://discord.com/channels/348202850486190081/1032012012382847126
            HelpCommandsChannel = 1032012033627013191, // https://discord.com/channels/348202850486190081/1032012033627013191
            HelpGenericEventsChannel = 1036676421613011054, // https://discord.com/channels/348202850486190081/1036676421613011054
            HelpCurrentMissionEventsChannel = 1036676459747627008, // https://discord.com/channels/348202850486190081/1036676459747627008
            HelpUnitSwapEventsChannel = 1300490389685403690, // https://discord.com/channels/348202850486190081/1300490389685403690
            HelpSpearOfAdunEventsChannel = 1300490442478981202, // https://discord.com/channels/348202850486190081/1300490442478981202
            AdminHelpChannel = 1034292418708783214, // https://discord.com/channels/348202850486190081/1034292418708783214
            AdminChannel = 1032015061100802148, // https://discord.com/channels/348202850486190081/1032015061100802148
            AdminPing = "<@&1036758233156681799>", // @Starcraft Bot Admin
        };

        public static ChannelSet PlayTestChannels = new ChannelSet()
        {
            DebugChannel = 1035272422192070656, // https://discord.com/channels/1035271276475985970/1035272422192070656
            DonationsChannel = 1035272460196646952, // https://discord.com/channels/1035271276475985970/1035272460196646952
            EventsChannel = 1035272488621445191, // https://discord.com/channels/1035271276475985970/1035272488621445191
            ChatChannel = 1035271276475985973, // https://discord.com/channels/1035271276475985970/1035271276475985973
            HelpCommandsChannel = 1300483548855140473, // https://discord.com/channels/1035271276475985970/1300483548855140473
            HelpGenericEventsChannel = 1300484568436179005, // https://discord.com/channels/1035271276475985970/1300484568436179005
            HelpCurrentMissionEventsChannel = 1300485195308208162, // https://discord.com/channels/1035271276475985970/1300485195308208162
            HelpUnitSwapEventsChannel = 1300485274614366290, // https://discord.com/channels/1035271276475985970/1300485274614366290
            HelpSpearOfAdunEventsChannel = 1300485341400268882, // https://discord.com/channels/1035271276475985970/1300485341400268882
            AdminHelpChannel = 1300485690177355798, // https://discord.com/channels/1035271276475985970/1300485690177355798
            AdminChannel = 1300485933015240795, // https://discord.com/channels/1035271276475985970/1300485933015240795
            AdminPing = "<@&1035271726239596607>", // @Starcraft Bot Administration
        };

        public static ChannelSet ExtraLifeChannels = new ChannelSet()
        {
            DebugChannel = 1035272422192070656, // https://discord.com/channels/1035271276475985970/1035272422192070656
            DonationsChannel = 1036698537431404684, // https://discord.com/channels/1035271276475985970/1036698537431404684
            EventsChannel = 1037871076451041310, // https://discord.com/channels/1035271276475985970/1037871076451041310
            ChatChannel = 1037394140356411444, // https://discord.com/channels/1035271276475985970/1037394140356411444
            HelpCommandsChannel = 1035272563280064573, // https://discord.com/channels/1035271276475985970/1035272563280064573
            HelpGenericEventsChannel = 1036668648833687703, // https://discord.com/channels/1035271276475985970/1036668648833687703
            HelpCurrentMissionEventsChannel = 1036668879931461712, // https://discord.com/channels/1035271276475985970/1036668879931461712
            HelpUnitSwapEventsChannel = 1300489925237669929, // https://discord.com/channels/1035271276475985970/1300489925237669929
            HelpSpearOfAdunEventsChannel = 1300490036567085136, // https://discord.com/channels/1035271276475985970/1300490036567085136
            AdminHelpChannel = 1035272593940422676, // https://discord.com/channels/1035271276475985970/1035272593940422676
            AdminChannel = 1035272536893698079, // https://discord.com/channels/1035271276475985970/1035272536893698079
            AdminPing = "<@&1035271726239596607>", // @Starcraft Bot Administration
        };
    }
}
