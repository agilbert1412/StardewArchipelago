using System;
using KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults;

namespace StardewArchipelago.Archipelago.ConnectionResults
{
    public class NoConnectionInformationResult : FailedConnectionResult
    {
        public NoConnectionInformationResult() : base($"The game being loaded has no connection information.{Environment.NewLine}You can use the connect_override command to input connection fields before loading it")
        {
        }
    }
}
