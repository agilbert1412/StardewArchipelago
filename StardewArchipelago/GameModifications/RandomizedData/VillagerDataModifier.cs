using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using StardewArchipelago.Extensions;
using StardewValley.GameData.Characters;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class VillagerDataModifier
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;

        public VillagerDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        public void OnVillagersBirthdayDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Characters"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    try
                    {
                        var charactersData = asset.AsDictionary<string, CharacterData>().Data;
                        var festivalData = DataLoader.Festivals_FestivalDates(Game1.content);
                        var availableDates = new Dictionary<string, List<int>>();
                        foreach (var seasonName in Enum.GetNames(typeof(Season)).Select(x => x.ToLower()))
                        {
                            availableDates.Add(seasonName, new List<int>());
                            for (var i = 1; i <= 28; i++)
                            {
                                availableDates[seasonName].Add(i);
                            }
                        }
                        foreach (var (festivalDate, _) in festivalData)
                        {
                            var (season, day) = FestivalDataModifier.SplitFestivalDateKey(festivalDate);
                            availableDates[season].Remove(day);
                        }

                        var characterKeys = charactersData.Keys.OrderBy(x => x).ToArray();
                        foreach (var characterName in characterKeys)
                        {
                            RegisterUnrandomizedBirthday(charactersData, characterName, availableDates);
                        }

                        foreach (var characterName in characterKeys)
                        {
                            ModifyVillagerBirthday(charactersData, characterName, availableDates);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed at editing Villagers Data. Message: {ex.Message}{Environment.NewLine}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void RegisterUnrandomizedBirthday(IDictionary<string, CharacterData> charactersData, string characterName, Dictionary<string, List<int>> availableDates)
        {
            if (_dataRandomization.VillagersData.ContainsKey(characterName))
            {
                return;
            }

            var characterData = charactersData[characterName];
            var birthdaySeason = characterData.BirthSeason?.ToString().ToLower();
            var birthdayDay = characterData.BirthDay;
            if (birthdaySeason != null && availableDates.ContainsKey(birthdaySeason))
            {
                availableDates[birthdaySeason].Remove(birthdayDay);
            }
        }

        private void ModifyVillagerBirthday(IDictionary<string, CharacterData> charactersData, string characterName, Dictionary<string, List<int>> availableDates)
        {
            if (!_dataRandomization.VillagersData.ContainsKey(characterName))
            {
                return;
            }

            var characterData = charactersData[characterName];
            var randomizedData = _dataRandomization.VillagersData[characterName];
            var birthdaySeasonName = randomizedData.Birthday;
            if (string.IsNullOrWhiteSpace(birthdaySeasonName) || !Enum.TryParse<Season>(birthdaySeasonName, out var birthdaySeason))
            {
                return;
            }

            var key = birthdaySeason.ToString().ToLower();
            var availableDays = availableDates[key];
            var seed = int.Parse(_archipelago.SlotData.Seed) + characterName.GetHash();
            var random = new Random(seed);
            var birthdayDay = availableDays[random.Next(0, availableDays.Count)];

            characterData.BirthSeason = birthdaySeason;
            characterData.BirthDay = birthdayDay;
            availableDates[key].Remove(birthdayDay);
        }
    }
}
