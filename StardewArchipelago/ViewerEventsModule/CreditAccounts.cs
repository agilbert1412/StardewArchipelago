using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KaitoKid.DiscordWrapper.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewArchipelago.ViewerEventsModule
{
    internal class CreditAccounts
    {
        private static readonly Random _random = new Random();
        private readonly IBotCommunicator _communications;
        private Dictionary<ulong, CreditAccount> _accounts;

        private DateTime _lastBackupTime;

        public CreditAccounts(IBotCommunicator discord)
        {
            _communications = discord;
            _accounts = new Dictionary<ulong, CreditAccount>();
        }

        public CreditAccount this[ulong discordId]
        {
            get { return GetAccount(discordId); }
        }

        public CreditAccount GetRandomAccount()
        {
            var values = Enumerable.ToList(_accounts.Values);
            var size = _accounts.Count;
            return values[_random.Next(size)];
        }

        public void ResetAll()
        {
            CreateBackup();
            _accounts = new Dictionary<ulong, CreditAccount>();
        }

        public void CreateBackup(int minMinutesSinceLastBackup = 0)
        {
            var backupTime = DateTime.Now;

            if ((backupTime - _lastBackupTime).TotalMinutes < minMinutesSinceLastBackup)
            {
                return;
            }

            var backupShortDateFormat = backupTime.ToShortDateString();
            ExportTo(@$"Backups\Credits\Credits - Backup {backupShortDateFormat}-{backupTime.Hour}-{backupTime.Minute}-{backupTime.Second}.json");

            _lastBackupTime = backupTime;
        }

        public void ImportFrom(string creditsFile)
        {
            var lines = File.ReadAllText(creditsFile, Encoding.UTF8);
            dynamic jsonData = JsonConvert.DeserializeObject(lines);
            foreach (JObject creditAccountString in jsonData)
            {
                var creditAccount = new CreditAccount(creditAccountString);
                this.Add(creditAccount.discordId, creditAccount);
            }
        }

        public void ExportTo(string creditsFile)
        {
            var json = JsonConvert.SerializeObject(this.ToList(), Formatting.Indented);
            File.WriteAllText(creditsFile, json);
        }

        private void Add(ulong discordId, CreditAccount creditAccount)
        {
            _accounts.Add(discordId, creditAccount);
        }

        private List<CreditAccount> ToList()
        {
            return _accounts.Values.ToList();
        }

        private CreditAccount GetAccount(ulong discordId)
        {
            if (!_accounts.ContainsKey(discordId))
            {
                Add(discordId, new CreditAccount
                {
                    discordId = discordId,
                    discordName = _communications.GetDisplayName(discordId),
                });
            }

            return _accounts[discordId];
        }

        public CreditAccount[] GetAccountsActiveInThePastMinutes(ulong maxMinutesSinceLastActivity, ulong exceptId)
        {
            var activeAccounts = _accounts.Where(kvp => kvp.Key != exceptId)
                .Where(kvp => (DateTime.Now - kvp.Value.lastActivityTime).TotalMinutes <= maxMinutesSinceLastActivity)
                .OrderByDescending(kvp => kvp.Value.lastActivityTime);
            return activeAccounts.Select(x => x.Value).ToArray();
        }
    }
}
