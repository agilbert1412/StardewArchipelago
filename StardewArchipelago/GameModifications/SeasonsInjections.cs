using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications
{
    public static class SeasonsInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, ArchipelagoStateDto state)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _state = state;
        }

        public static bool NewDayAfterFade_SeasonsRandomizer_Prefix(ref IEnumerator<int> __result)
        {
            try
            {

                return false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewDayAfterFade_SeasonsRandomizer_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // This will break in multiplayer
        public static void NewDayAfterFade()
        {
            Game1.newDaySync.start();
            Game1.flushLocationLookup();
            FixProblems();
            UnpauseAllFarmers();

            Game1.whereIsTodaysFest = null;
            StopWind();
            ResetSkullCavernChests();
            Game1.player.currentEyes = 0;
            var seed = PrepareNewRNGSeed();
            SetRNGBasedOnDayOfMonth(seed);

            Game1.player.team.endOfNightStatus.UpdateState("sleep");
            Barrier("sleep");
            Game1.gameTimeInterval = 0;
            Game1.game1.wasAskedLeoMemory = false;
            UpdatePlayerForNight();
            Game1.changeMusicTrack("none");
            PrepareDishOfTheDay();
            ResetAllCharacterDialogues();

            PassTimeForObjectsEverywhere();
            SetLightLevels();

            Game1.tmpTimeOfDay = Game1.timeOfDay;
            PrepareMailForTomorrow();
            ClearMineLevelsAndEnchantments();

            Game1.player.dayupdate();
            SetNewDailyLuck();

            IncrementDay();
            Game1.startedJukeboxMusic = false;
            PrepareDateForSaveGame(); // Randomizer needs to handle this
            Game1.player.showToolUpgradeAvailability();
            PrepareWeddings();
            MoveItemsFromMiniShippingBinsToMainShippingBin();
            SellShippedItemsBySelf();
            TransferPlacedObjectsToModifiedLocations();
            SellShippedItemsByFarmHands();
            
            PerformDivorce();
            SynchroniseWalletType();
            ClearShipping();
            SendAndSaveFarmhands();
            ReduceY1GaranteeCounterOnNightMarket();

            SetYear1(); // This seems deeply unnecessary
            DoSeasonChange(); // Randomizer needs to handle this
            UpdateAvailableSpecialOrders();

            UpdateNetWorldState();
            Barrier("date");

            RemoveExpiredSpecialOrders();
            Barrier("processOrders");

            RemoveItemsAndMailOvernight();
            SendMailHardcodedForToday(); // Randomizer needs to handle this
            SetMineDifficulties();
            Game1.RefreshQuestOfTheDay();
            Game1.weatherForTomorrow = Game1.getWeatherModificationsForDate(Game1.Date, Game1.weatherForTomorrow);
            SetWeddingWeather();
            SetWeatherFlags();
            ChooseSongBasedOnWeather();
            SetOtherLocationsWeatherForTomorrow();
            StopMusicBasedOnWeather();
            PopulateDebrisWeather();
            ChooseWeatherForTomorrow();
            ForceSunnyOnFestivalDay();
            ForceRainOnYear1Spring3(); // Randomizer needs to handle this
            SetWeatherForLocations();
            RemoveNPCMailReceived();
            FarmAnimal.reservedGrass.Clear();
            PerformDayUpdateOnLocations();
            UpdateIslandGemBirds();
            UpdateLocationsBasedOnRain();
            UpdateCurrentUpgradeValues();
            Barrier("buildingUpgrades");
            Game1.stats.AverageBedtime = (uint)Game1.timeOfDay;
            Game1.timeOfDay = 600;
            Game1.newDay = false;
            UpdateNetWorldState();
            PreparePlayerWakeUp();
            SetPricesBasedOnHouseUpgradeLevel();
            Game1.updateWeatherIcon();
            Game1.freezeControls = false;
            PickNextFarmEvent();
            RemoveDeliveredMailIfNeeded();
            ShowLostAndFoundMessage();

            Barrier("mail");
            SetNoNewLostAndFoundItems();

            SetCanHostForBuiltCabin();
            UpdateTeamPerfection();

            Barrier("checkcompletion");
            Game1.UpdateFarmPerfection();
            HandleEndOfNightEvents();
            UpdateLobbyData();
        }

        private static void FixProblems()
        {
            try
            {
                Game1.fixProblems();
            }
            catch (Exception ex)
            {
            }
        }

        private static void UnpauseAllFarmers()
        {
            foreach (var allFarmer in Game1.getAllFarmers())
            {
                allFarmer.FarmerSprite.PauseForSingleAnimation = false;
            }
        }

        private static void StopWind()
        {
            if (Game1.wind != null)
            {
                Game1.wind.Stop(AudioStopOptions.Immediate);
                Game1.wind = null;
            }
        }

        private static void ResetSkullCavernChests()
        {
            foreach (var key in new List<int>(Game1.player.chestConsumedMineLevels.Keys))
            {
                if (key > 120)
                {
                    Game1.player.chestConsumedMineLevels.Remove(key);
                }
            }
        }

        private static int PrepareNewRNGSeed()
        {
            int Seed;
            if (Game1.IsMasterGame)
            {
                Game1.player.team.announcedSleepingFarmers.Clear();
                Seed = (int)Game1.uniqueIDForThisGame / 100 + (int)Game1.stats.DaysPlayed * 10 + 1 +
                       (int)Game1.stats.StepsTaken;
                Game1.newDaySync.sendVar<NetInt, int>("seed", Seed);
            }
            else
            {
                WaitForVariable("seed");
                Seed = Game1.newDaySync.waitForVar<NetInt, int>("seed");
            }

            return Seed;
        }

        private static void SetRNGBasedOnDayOfMonth(int Seed)
        {
            Game1.random = new Random(Seed);
            for (var index = 0; index < Game1.dayOfMonth; ++index)
            {
                Game1.random.Next();
            }
        }

        private static void UpdatePlayerForNight()
        {
            Game1.player.team.Update();
            Game1.player.team.NewDay();
            Game1.player.passedOut = false;
            Game1.player.CanMove = true;
            Game1.player.FarmerSprite.PauseForSingleAnimation = false;
            Game1.player.FarmerSprite.StopAnimation();
            Game1.player.completelyStopAnimatingOrDoingAction();
        }

        private static void PrepareDishOfTheDay()
        {
            var parentSheetIndex = Game1.random.Next(194, 240);
            while (Utility.getForbiddenDishesOfTheDay().Contains(parentSheetIndex))
                parentSheetIndex = Game1.random.Next(194, 240);
            var initialStack = Game1.random.Next(1, 4 + (Game1.random.NextDouble() < 0.08 ? 10 : 0));
            if (Game1.IsMasterGame)
            {
                Game1.dishOfTheDay = new Object(Vector2.Zero, parentSheetIndex, initialStack);
            }

            Barrier("dishOfTheDay");
        }

        private static void ResetAllCharacterDialogues()
        {
            Game1.npcDialogues = null;
            foreach (var allCharacter in Utility.getAllCharacters())
            {
                allCharacter.updatedDialogueYet = false;
            }
        }

        private static void PassTimeForObjectsEverywhere()
        {
            var minutesUntilMorning = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
            foreach (var location in Game1.locations)
            {
                location.currentEvent = null;
                if (Game1.IsMasterGame)
                {
                    location.passTimeForObjects(minutesUntilMorning);
                }
            }

            if (Game1.IsMasterGame)
            {
                foreach (var building in Game1.getFarm().buildings)
                {
                    if (building.indoors.Value != null)
                    {
                        building.indoors.Value.passTimeForObjects(minutesUntilMorning);
                    }
                }
            }
        }

        private static void SetLightLevels()
        {
            Game1.globalOutdoorLighting = 0.0f;
            Game1.outdoorLight = Color.White;
            Game1.ambientLight = Color.White;
            if (Game1.isLightning && Game1.IsMasterGame)
            {
                Utility.overnightLightning();
            }
        }

        private static void PrepareMailForTomorrow()
        {
            if (Game1.MasterPlayer.hasOrWillReceiveMail("ccBulletinThankYou") &&
                !Game1.player.hasOrWillReceiveMail("ccBulletinThankYou"))
            {
                Game1.addMailForTomorrow("ccBulletinThankYou");
            }

            Game1.ReceiveMailForTomorrow();
            if (Game1.player.friendshipData.Count() > 0)
            {
                var key = Game1.player.friendshipData.Keys.ElementAt(
                    Game1.random.Next(Game1.player.friendshipData.Keys.Count()));
                if (Game1.random.NextDouble() < Game1.player.friendshipData[key].Points / 250 * 0.1 &&
                    (Game1.player.spouse == null || !Game1.player.spouse.Equals(key)) &&
                    Game1.content.Load<Dictionary<string, string>>("Data\\mail").ContainsKey(key))
                {
                    Game1.mailbox.Add(key);
                }
            }
        }

        private static void ClearMineLevelsAndEnchantments()
        {
            MineShaft.clearActiveMines();
            VolcanoDungeon.ClearAllLevels();
            for (var index = Game1.player.enchantments.Count - 1; index >= 0; --index)
            {
                Game1.player.enchantments[index].OnUnequip(Game1.player);
            }
        }

        private static void SetNewDailyLuck()
        {
            if (Game1.IsMasterGame)
            {
                Game1.player.team.sharedDailyLuck.Value = Math.Min(0.10000000149011612, Game1.random.Next(-100, 101) / 1000.0);
            }
        }

        private static void IncrementDay()
        {
            ++Game1.dayOfMonth;
            ++Game1.stats.DaysPlayed;
        }

        private static void PrepareDateForSaveGame()
        {
            GetVanillaValues(out var totalDays, out var year, out var seasonNumber, out _);
            Game1.year = year + 1;
            Game1.player.dayOfMonthForSaveGame = Game1.dayOfMonth;
            Game1.player.seasonForSaveGame = seasonNumber;
            Game1.player.yearForSaveGame = Game1.year;
        }

        private static void PrepareWeddings()
        {
            if (Game1.IsMasterGame)
            {
                Game1.queueWeddingsForToday();
                Game1.newDaySync.sendVar<NetRef<NetLongList>, NetLongList>("weddingsToday",
                    new NetLongList(Game1.weddingsToday));
            }
            else
            {
                WaitForVariable("weddingsToday");
                Game1.weddingsToday =
                    new List<long>(Game1.newDaySync.waitForVar<NetRef<NetLongList>, NetLongList>("weddingsToday"));
            }

            Game1.weddingToday = false;
            foreach (var id in Game1.weddingsToday)
            {
                var farmer = Game1.getFarmer(id);
                if (farmer != null && !farmer.hasCurrentOrPendingRoommate())
                {
                    Game1.weddingToday = true;
                    break;
                }
            }

            if (Game1.player.spouse != null && Game1.player.isEngaged() &&
                Game1.weddingsToday.Contains(Game1.player.UniqueMultiplayerID))
            {
                var friendship = Game1.player.friendshipData[Game1.player.spouse];
                if (friendship.CountdownToWedding <= 1)
                {
                    friendship.Status = FriendshipStatus.Married;
                    friendship.WeddingDate = new WorldDate(Game1.Date);
                    Game1.prepareSpouseForWedding(Game1.player);
                }
            }
        }

        private static void MoveItemsFromMiniShippingBinsToMainShippingBin()
        {
            var additional_shipped_items =
                new NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>();
            if (Game1.IsMasterGame)
            {
                Utility.ForAllLocations(location =>
                {
                    foreach (var @object in location.objects.Values)
                    {
                        if (@object is Chest && @object is Chest chest2 &&
                            chest2.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
                        {
                            if ((bool)Game1.player.team.useSeparateWallets)
                            {
                                foreach (var key in chest2.separateWalletItems.Keys)
                                {
                                    if (!additional_shipped_items.ContainsKey(key))
                                    {
                                        additional_shipped_items[key] = new NetList<Item, NetRef<Item>>();
                                    }

                                    var objList = new List<Item>(chest2.separateWalletItems[key]);
                                    chest2.separateWalletItems[key].Clear();
                                    foreach (var obj in objList)
                                    {
                                        additional_shipped_items[key].Add(obj);
                                    }
                                }
                            }
                            else
                            {
                                var shippingBin = Game1.getFarm().getShippingBin(Game1.player);
                                var objList = new List<Item>(chest2.items);
                                chest2.items.Clear();
                                foreach (var obj in objList)
                                {
                                    shippingBin.Add(obj);
                                }
                            }

                            chest2.items.Clear();
                            chest2.separateWalletItems.Clear();
                        }
                    }
                });
            }

            if (Game1.IsMasterGame)
            {
                Game1.newDaySync
                    .sendVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>,
                        NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>(
                        "additional_shipped_items", additional_shipped_items);
            }
            else
            {
                WaitForVariable("additional_shipped_items");
                additional_shipped_items =
                    Game1.newDaySync
                        .waitForVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>,
                            NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>(
                            "additional_shipped_items");
            }

            if (Game1.player.team.useSeparateWallets.Value)
            {
                var shippingBin = Game1.getFarm().getShippingBin(Game1.player);
                if (additional_shipped_items.ContainsKey(Game1.player.UniqueMultiplayerID))
                {
                    foreach (var obj in additional_shipped_items[Game1.player.UniqueMultiplayerID])
                    {
                        shippingBin.Add(obj);
                    }
                }
            }
            Barrier("handleMiniShippingBins");
        }

        private static void SellShippedItemsBySelf()
        {
            var shippingBin1 = Game1.getFarm().getShippingBin(Game1.player);
            foreach (var obj in shippingBin1)
            {
                Game1.player.displayedShippedItems.Add(obj);
            }

            if (Game1.player.useSeparateWallets || !Game1.player.useSeparateWallets && Game1.player.IsMainPlayer)
            {
                var num1 = 0;
                foreach (var obj in shippingBin1)
                {
                    var num2 = 0;
                    if (obj is Object)
                    {
                        num2 = (obj as Object).sellToStorePrice() * obj.Stack;
                        num1 += num2;
                    }

                    if (Game1.player.team.specialOrders != null)
                    {
                        foreach (var specialOrder in Game1.player.team.specialOrders)
                        {
                            if (specialOrder.onItemShipped != null)
                            {
                                specialOrder.onItemShipped(Game1.player, obj, num2);
                            }
                        }
                    }
                }

                Game1.player.Money += num1;
            }
        }

        private static void TransferPlacedObjectsToModifiedLocations()
        {
            if (Game1.IsMasterGame)
            {
                if (Game1.currentSeason.Equals("winter") && Game1.dayOfMonth == 18)
                {
                    var locationFromName1 = Game1.getLocationFromName("Submarine");
                    if (locationFromName1.objects.Count() >= 0)
                    {
                        Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName1, null, new Vector2(20f, 20f),
                            Game1.getLocationFromName("Beach"));
                    }

                    var locationFromName2 = Game1.getLocationFromName("MermaidHouse");
                    if (locationFromName2.objects.Count() >= 0)
                    {
                        Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName2, null, new Vector2(21f, 20f),
                            Game1.getLocationFromName("Beach"));
                    }
                }

                if (Game1.player.hasOrWillReceiveMail("pamHouseUpgrade") &&
                    !Game1.player.hasOrWillReceiveMail("transferredObjectsPamHouse"))
                {
                    Game1.addMailForTomorrow("transferredObjectsPamHouse", true);
                    var locationFromName3 = Game1.getLocationFromName("Trailer");
                    var locationFromName4 = Game1.getLocationFromName("Trailer_Big");
                    if (locationFromName3.objects.Count() >= 0)
                    {
                        Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName3, locationFromName4,
                            new Vector2(14f, 23f));
                    }
                }

                if (Utility.HasAnyPlayerSeenEvent(191393) && !Game1.player.hasOrWillReceiveMail("transferredObjectsJojaMart"))
                {
                    Game1.addMailForTomorrow("transferredObjectsJojaMart", true);
                    var locationFromName = Game1.getLocationFromName("JojaMart");
                    if (locationFromName.objects.Count() >= 0)
                    {
                        Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName, null, new Vector2(89f, 51f),
                            Game1.getLocationFromName("Town"));
                    }
                }
            }
        }

        private static void SellShippedItemsByFarmHands()
        {
            if (Game1.player.useSeparateWallets && Game1.player.IsMainPlayer)
            {
                foreach (var allFarmhand in Game1.getAllFarmhands())
                {
                    if (!allFarmhand.isActive() && !allFarmhand.isUnclaimedFarmhand)
                    {
                        var num3 = 0;
                        foreach (var obj in Game1.getFarm().getShippingBin(allFarmhand))
                        {
                            var num4 = 0;
                            if (obj is Object)
                            {
                                num4 = (obj as Object).sellToStorePrice(allFarmhand.UniqueMultiplayerID) * obj.Stack;
                                num3 += num4;
                            }

                            if (Game1.player.team.specialOrders != null)
                            {
                                foreach (var specialOrder in Game1.player.team.specialOrders)
                                {
                                    if (specialOrder.onItemShipped != null)
                                    {
                                        specialOrder.onItemShipped(Game1.player, obj, num4);
                                    }
                                }
                            }
                        }

                        Game1.player.team.AddIndividualMoney(allFarmhand, num3);
                        Game1.getFarm().getShippingBin(allFarmhand).Clear();
                    }
                }
            }
        }

        private static void PerformDivorce()
        {
            var divorceNPCs = new List<NPC>();
            if (Game1.IsMasterGame)
            {
                foreach (var allFarmer in Game1.getAllFarmers())
                {
                    if (allFarmer.isActive() && (bool)allFarmer.divorceTonight && allFarmer.getSpouse() != null)
                    {
                        divorceNPCs.Add(allFarmer.getSpouse());
                    }
                }
            }

            Barrier("player.dayupdate");
            if ((bool)Game1.player.divorceTonight)
            {
                Game1.player.doDivorce();
            }

            Barrier("player.divorce");
            if (Game1.IsMasterGame)
            {
                foreach (var npc in divorceNPCs)
                {
                    if (npc.getSpouse() == null)
                    {
                        npc.PerformDivorce();
                    }
                }
            }

            Barrier("player.finishDivorce");
        }

        private static void SynchroniseWalletType()
        {
            if (Game1.IsMasterGame && Game1.player.changeWalletTypeTonight.Value)
            {
                if (Game1.player.useSeparateWallets)
                {
                    ManorHouse.MergeWallets();
                }
                else
                {
                    ManorHouse.SeparateWallets();
                }
            }

            Barrier("player.wallets");
        }

        private static void ClearShipping()
        {
            Game1.getFarm().lastItemShipped = null;
            Game1.getFarm().getShippingBin(Game1.player).Clear();
            Barrier("clearShipping");
        }

        private static void ReduceY1GaranteeCounterOnNightMarket()
        {
            if (Game1.IsMasterGame && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17 &&
                Game1.currentSeason.Equals("winter") && Game1.IsMasterGame &&
                Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0)
            {
                --Game1.netWorldState.Value.VisitsUntilY1Guarantee;
            }
        }

        private static void SendAndSaveFarmhands()
        {
            var multiplayerField = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
            if (Game1.IsClient)
            {
                multiplayerField.GetValue().sendFarmhand();
                Game1.newDaySync.processMessages();
            }

            Barrier("sendFarmhands");

            if (Game1.IsMasterGame)
            {
                multiplayerField.GetValue().saveFarmhands();
            }

            Barrier("saveFarmhands");
        }

        private static void SetYear1()
        {
            // This seems deeply unnecessary
            if (Game1.dayOfMonth == 27 && Game1.currentSeason.Equals("spring"))
            {
                var year1 = Game1.year;
            }
        }

        private static void DoSeasonChange()
        {
            if (Game1.dayOfMonth == 29)
            {
                // Game1.NewSeason();
                if (!Game1.currentSeason.Equals("winter"))
                {
                    Game1.cropsOfTheWeek = Utility.cropsOfTheWeek();
                }
                
                if (Game1.stats.DaysPlayed % 112 == 0)
                {
                    ++Game1.year;
                    if (Game1.year == 2)
                    {
                        Game1.addKentIfNecessary();
                    }
                }
            }
        }

        private static void UpdateAvailableSpecialOrders()
        {
            if (Game1.IsMasterGame && (Game1.dayOfMonth == 1 || Game1.dayOfMonth == 8 || Game1.dayOfMonth == 15 ||
                                       Game1.dayOfMonth == 22))
            {
                SpecialOrder.UpdateAvailableSpecialOrders(true);
            }
        }

        private static void UpdateNetWorldState()
        {
            if (Game1.IsMasterGame)
            {
                Game1.netWorldState.Value.UpdateFromGame1();
            }
        }

        private static void RemoveExpiredSpecialOrders()
        {
            if (Game1.IsMasterGame)
            {
                for (var index = 0; index < Game1.player.team.specialOrders.Count; ++index)
                {
                    var specialOrder = Game1.player.team.specialOrders[index];
                    if ((SpecialOrder.QuestState)specialOrder.questState != SpecialOrder.QuestState.Complete &&
                        specialOrder.GetDaysLeft() <= 0)
                    {
                        specialOrder.OnFail();
                        Game1.player.team.specialOrders.RemoveAt(index);
                        --index;
                    }
                }
            }
        }

        private static void RemoveItemsAndMailOvernight()
        {
            var stringList = new List<string>(Game1.player.team.mailToRemoveOvernight);
            var intList = new List<int>(Game1.player.team.itemsToRemoveOvernight);
            if (Game1.IsMasterGame)
            {
                foreach (var rule in Game1.player.team.specialRulesRemovedToday)
                {
                    SpecialOrder.RemoveSpecialRuleAtEndOfDay(rule);
                }
            }

            Game1.player.team.specialRulesRemovedToday.Clear();
            foreach (var parent_sheet_index in intList)
            {
                var performRemoveNormalItemFromFarmerOvernightMethod =
                    _helper.Reflection.GetMethod(Game1.game1, "_PerformRemoveNormalItemFromFarmerOvernight");
                if (Game1.IsMasterGame)
                {
                    Game1.game1._PerformRemoveNormalItemFromWorldOvernight(parent_sheet_index);
                    foreach (var allFarmer in Game1.getAllFarmers())
                    {
                        performRemoveNormalItemFromFarmerOvernightMethod.Invoke(allFarmer, parent_sheet_index);
                    }
                }
                else
                {
                    performRemoveNormalItemFromFarmerOvernightMethod.Invoke(Game1.player, parent_sheet_index);
                }
            }

            foreach (var mail_key in stringList)
            {
                if (Game1.IsMasterGame)
                {
                    foreach (var allFarmer in Game1.getAllFarmers())
                    {
                        allFarmer.RemoveMail(mail_key, allFarmer == Game1.MasterPlayer);
                    }
                }
                else
                {
                    Game1.player.RemoveMail(mail_key);
                }
            }

            Barrier("removeItemsFromWorld");
            if (Game1.IsMasterGame)
            {
                Game1.player.team.itemsToRemoveOvernight.Clear();
                Game1.player.team.mailToRemoveOvernight.Clear();
            }
        }

        public static void SendMailHardcodedForToday()
        {
            GetVanillaValues(out var totalDays, out var year, out var seasonNumber, out var seasonName);
            if (Game1.content.Load<Dictionary<string, string>>("Data\\mail")
                .ContainsKey(seasonName + "_" + Game1.dayOfMonth + "_" + year))
            {
                Game1.mailbox.Add(seasonName + "_" + Game1.dayOfMonth + "_" + year);
            }
            else if (Game1.content.Load<Dictionary<string, string>>("Data\\mail")
                     .ContainsKey(seasonName + "_" + Game1.dayOfMonth))
            {
                Game1.mailbox.Add(seasonName + "_" + Game1.dayOfMonth);
            }
        }

        private static void SetMineDifficulties()
        {
            if (Game1.IsMasterGame && Game1.player.team.toggleMineShrineOvernight.Value)
            {
                Game1.player.team.toggleMineShrineOvernight.Value = false;
                Game1.player.team.mineShrineActivated.Value = !Game1.player.team.mineShrineActivated.Value;
                if (Game1.player.team.mineShrineActivated.Value)
                {
                    ++Game1.netWorldState.Value.MinesDifficulty;
                }
                else
                {
                    --Game1.netWorldState.Value.MinesDifficulty;
                }
            }

            if (Game1.IsMasterGame)
            {
                if (!Game1.player.team.SpecialOrderRuleActive("MINE_HARD") && Game1.netWorldState.Value.MinesDifficulty > 1)
                {
                    Game1.netWorldState.Value.MinesDifficulty = 1;
                }

                if (!Game1.player.team.SpecialOrderRuleActive("SC_HARD") && Game1.netWorldState.Value.SkullCavesDifficulty > 0)
                {
                    Game1.netWorldState.Value.SkullCavesDifficulty = 0;
                }
            }
        }

        private static void SetWeddingWeather()
        {
            if (Game1.weddingToday)
            {
                Game1.weatherForTomorrow = 6;
            }
        }

        private static void SetWeatherFlags()
        {
            Game1.wasRainingYesterday = Game1.isRaining || Game1.isLightning;
            if (Game1.weatherForTomorrow == 1 || Game1.weatherForTomorrow == 3)
            {
                Game1.isRaining = true;
            }

            if (Game1.weatherForTomorrow == 3)
            {
                Game1.isLightning = true;
            }

            if (Game1.weatherForTomorrow == 0 || Game1.weatherForTomorrow == 2 || Game1.weatherForTomorrow == 4 ||
                Game1.weatherForTomorrow == 5 || Game1.weatherForTomorrow == 6)
            {
                Game1.isRaining = false;
                Game1.isLightning = false;
                Game1.isSnowing = false;
                if (Game1.weatherForTomorrow == 5)
                {
                    Game1.isSnowing = true;
                }
            }
        }

        private static void ChooseSongBasedOnWeather()
        {
            if (!Game1.isRaining && !Game1.isLightning)
            {
                ++Game1.currentSongIndex;
                if (Game1.currentSongIndex > 3 || Game1.dayOfMonth == 1)
                {
                    Game1.currentSongIndex = 1;
                }
            }
        }

        private static void SetOtherLocationsWeatherForTomorrow()
        {
            if (Game1.IsMasterGame)
            {
                Game1.game1.SetOtherLocationWeatherForTomorrow(Game1.random);
            }
        }

        private static void StopMusicBasedOnWeather()
        {
            if ((Game1.isRaining || Game1.isSnowing || Game1.isLightning) &&
                Game1.currentLocation.GetLocationContext() == GameLocation.LocationContext.Default)
            {
                Game1.changeMusicTrack("none");
            }
            else if (Game1.weatherForTomorrow == 4 && Game1.weatherForTomorrow == 6)
            {
                Game1.changeMusicTrack("none");
            }
        }

        private static void PopulateDebrisWeather()
        {
            Game1.debrisWeather.Clear();
            Game1.isDebrisWeather = false;
            if (Game1.weatherForTomorrow == 2)
            {
                Game1.populateDebrisWeatherArray();
            }
        }

        private static void ChooseWeatherForTomorrow()
        {
            Game1.chanceToRainTomorrow = !Game1.currentSeason.Equals("summer")
                ? (!Game1.currentSeason.Equals("winter") ? 0.183 : 0.63)
                : (Game1.dayOfMonth > 1 ? 0.12 + Game1.dayOfMonth * (3.0 / 1000.0) : 0.0);
            if (Game1.random.NextDouble() < Game1.chanceToRainTomorrow)
            {
                Game1.weatherForTomorrow = 1;
                if (Game1.currentSeason.Equals("summer") && Game1.random.NextDouble() < 0.85 ||
                    !Game1.currentSeason.Equals("winter") && Game1.random.NextDouble() < 0.25 && Game1.dayOfMonth > 2 &&
                    Game1.stats.DaysPlayed > 27U)
                {
                    Game1.weatherForTomorrow = 3;
                }

                if (Game1.currentSeason.Equals("winter"))
                {
                    Game1.weatherForTomorrow = 5;
                }
            }
            else
            {
                Game1.weatherForTomorrow = Game1.stats.DaysPlayed <= 2U ||
                                           (!Game1.currentSeason.Equals("spring") || Game1.random.NextDouble() >= 0.2) &&
                                           (!Game1.currentSeason.Equals("fall") || Game1.random.NextDouble() >= 0.6) ||
                                           Game1.weddingToday
                    ? 0
                    : 2;
            }
        }

        private static void ForceSunnyOnFestivalDay()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.weatherForTomorrow = 4;
            }
        }

        private static void ForceRainOnYear1Spring3()
        {
            if (Game1.stats.DaysPlayed == 2U)
            {
                Game1.weatherForTomorrow = 1;
            }
        }

        private static void SetWeatherForLocations()
        {
            if (Game1.IsMasterGame)
            {
                Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).weatherForTomorrow.Value =
                    Game1.weatherForTomorrow;
                Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isRaining.Value =
                    Game1.isRaining;
                Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isSnowing.Value =
                    Game1.isSnowing;
                Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isLightning.Value =
                    Game1.isLightning;
                Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isDebrisWeather.Value =
                    Game1.isDebrisWeather;
            }
        }

        private static void RemoveNPCMailReceived()
        {
            foreach (var allCharacter in Utility.getAllCharacters())
            {
                Game1.player.mailReceived.Remove(allCharacter.Name);
                Game1.player.mailReceived.Remove(allCharacter.Name + "Cooking");
                allCharacter.drawOffset.Value = Vector2.Zero;
            }
        }

        private static void PerformDayUpdateOnLocations()
        {
            if (Game1.IsMasterGame)
            {
                int num;
                NPC.hasSomeoneRepairedTheFences = (num = 0) != 0;
                NPC.hasSomeoneFedTheAnimals = num != 0;
                NPC.hasSomeoneFedThePet = num != 0;
                NPC.hasSomeoneWateredCrops = num != 0;
                foreach (var location in Game1.locations)
                {
                    location.ResetCharacterDialogues();
                    location.DayUpdate(Game1.dayOfMonth);
                }

                Game1.UpdateHorseOwnership();
                foreach (var allCharacter in Utility.getAllCharacters())
                {
                    allCharacter.islandScheduleName.Value = null;
                    allCharacter.currentScheduleDelay = 0.0f;
                }

                foreach (var allCharacter in Utility.getAllCharacters())
                {
                    allCharacter.dayUpdate(Game1.dayOfMonth);
                }

                IslandSouth.SetupIslandSchedules();
                var purchased_item_npcs = new HashSet<NPC>();
                Game1.UpdateShopPlayerItemInventory("SeedShop", purchased_item_npcs);
                Game1.UpdateShopPlayerItemInventory("FishShop", purchased_item_npcs);
            }
        }

        private static void UpdateIslandGemBirds()
        {
            if (Game1.IsMasterGame && Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island)
                    .isRaining.Value)
            {
                var tile_position = new Vector2(0.0f, 0.0f);
                IslandLocation islandLocation = null;
                var list = new List<int>();
                for (var index = 0; index < 4; ++index)
                {
                    list.Add(index);
                }

                Utility.Shuffle(new Random((int)Game1.uniqueIDForThisGame), list);
                switch (list[Game1.currentGemBirdIndex])
                {
                    case 0:
                        islandLocation = Game1.getLocationFromName("IslandSouth") as IslandLocation;
                        tile_position = new Vector2(10f, 30f);
                        break;
                    case 1:
                        islandLocation = Game1.getLocationFromName("IslandNorth") as IslandLocation;
                        tile_position = new Vector2(56f, 56f);
                        break;
                    case 2:
                        islandLocation = Game1.getLocationFromName("Islandwest") as IslandLocation;
                        tile_position = new Vector2(53f, 51f);
                        break;
                    case 3:
                        islandLocation = Game1.getLocationFromName("IslandEast") as IslandLocation;
                        tile_position = new Vector2(21f, 35f);
                        break;
                }

                Game1.currentGemBirdIndex = (Game1.currentGemBirdIndex + 1) % 4;
                if (islandLocation != null)
                {
                    islandLocation.locationGemBird.Value = new IslandGemBird(tile_position,
                        IslandGemBird.GetBirdTypeForLocation(islandLocation.Name));
                }
            }
        }

        private static void UpdateLocationsBasedOnRain()
        {
            if (Game1.IsMasterGame)
            {
                foreach (var location in Game1.locations)
                {
                    if (Game1.IsRainingHere(location) && location.IsOutdoors)
                    {
                        foreach (var pair in location.terrainFeatures.Pairs)
                        {
                            if (pair.Value is HoeDirt && (int)((HoeDirt)pair.Value).state != 2)
                            {
                                ((HoeDirt)pair.Value).state.Value = 1;
                            }
                        }
                    }
                }

                var locationFromName = Game1.getLocationFromName("Farm");
                if (Game1.IsRainingHere(locationFromName))
                {
                    (locationFromName as Farm).petBowlWatered.Value = true;
                }
            }
        }

        private static void UpdateCurrentUpgradeValues()
        {
            if (Game1.player.currentUpgrade != null)
            {
                --Game1.player.currentUpgrade.daysLeftTillUpgradeDone;
                if (Game1.getLocationFromName("Farm").objects.ContainsKey(new Vector2(
                        Game1.player.currentUpgrade.positionOfCarpenter.X / 64f,
                        Game1.player.currentUpgrade.positionOfCarpenter.Y / 64f)))
                {
                    Game1.getLocationFromName("Farm").objects.Remove(new Vector2(
                        Game1.player.currentUpgrade.positionOfCarpenter.X / 64f,
                        Game1.player.currentUpgrade.positionOfCarpenter.Y / 64f));
                }

                if (Game1.player.currentUpgrade.daysLeftTillUpgradeDone == 0)
                {
                    var whichBuilding = Game1.player.currentUpgrade.whichBuilding;
                    if (!(whichBuilding == "House"))
                    {
                        if (!(whichBuilding == "Coop"))
                        {
                            if (!(whichBuilding == "Barn"))
                            {
                                if (whichBuilding == "Greenhouse")
                                {
                                    Game1.player.hasGreenhouse = true;
                                    Game1.greenhouseTexture = Game1.content.Load<Texture2D>("BuildingUpgrades\\Greenhouse");
                                }
                            }
                            else
                            {
                                ++Game1.player.BarnUpgradeLevel;
                                Game1.currentBarnTexture =
                                    Game1.content.Load<Texture2D>("BuildingUpgrades\\Barn" + Game1.player.BarnUpgradeLevel);
                            }
                        }
                        else
                        {
                            ++Game1.player.CoopUpgradeLevel;
                            Game1.currentCoopTexture =
                                Game1.content.Load<Texture2D>("BuildingUpgrades\\Coop" + Game1.player.CoopUpgradeLevel);
                        }
                    }
                    else
                    {
                        ++Game1.player.HouseUpgradeLevel;
                        Game1.currentHouseTexture =
                            Game1.content.Load<Texture2D>("Buildings\\House" + Game1.player.HouseUpgradeLevel);
                    }

                    Game1.stats.checkForBuildingUpgradeAchievements();
                    Game1.removeFrontLayerForFarmBuildings();
                    Game1.addNewFarmBuildingMaps();
                    Game1.player.currentUpgrade = null;
                    Game1.changeInvisibility("Robin", false);
                }
                else if (Game1.player.currentUpgrade.daysLeftTillUpgradeDone == 3)
                {
                    Game1.changeInvisibility("Robin", true);
                }
            }
        }

        private static void PreparePlayerWakeUp()
        {
            if (Game1.player.currentLocation != null)
            {
                Game1.player.currentLocation.resetForPlayerEntry();
                BedFurniture.ApplyWakeUpPosition(Game1.player);
                Game1.forceSnapOnNextViewportUpdate = true;
                Game1.UpdateViewPort(false, new Point(Game1.player.getStandingX(), Game1.player.getStandingY()));
                Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
            }
            Game1.player.sleptInTemporaryBed.Value = false;
        }

        private static void SetPricesBasedOnHouseUpgradeLevel()
        {
            var currentWallpaper = Game1.currentWallpaper;
            Game1.wallpaperPrice = Game1.random.Next(75, 500) + Game1.player.HouseUpgradeLevel * 100;
            Game1.wallpaperPrice -= Game1.wallpaperPrice % 5;
            var currentFloor = Game1.currentFloor;
            Game1.floorPrice = Game1.random.Next(75, 500) + Game1.player.HouseUpgradeLevel * 100;
            Game1.floorPrice -= Game1.floorPrice % 5;
        }

        private static void PickNextFarmEvent()
        {
            if (Game1.stats.DaysPlayed > 1U || !Game1.IsMasterGame)
            {
                Game1.farmEvent = null;
                if (Game1.IsMasterGame)
                {
                    Game1.farmEvent = Utility.pickFarmEvent();
                    Game1.newDaySync.sendVar<NetRef<FarmEvent>, FarmEvent>("farmEvent", Game1.farmEvent);
                }
                else
                {
                    WaitForVariable("farmEvent");
                    Game1.farmEvent = Game1.newDaySync.waitForVar<NetRef<FarmEvent>, FarmEvent>("farmEvent");
                }

                if (Game1.farmEvent == null)
                {
                    Game1.farmEvent = Utility.pickPersonalFarmEvent();
                }

                if (Game1.farmEvent != null && Game1.farmEvent.setUp())
                {
                    Game1.farmEvent = null;
                }
            }
        }

        private static void RemoveDeliveredMailIfNeeded()
        {
            if (Game1.farmEvent == null)
            {
                Game1.RemoveDeliveredMailForTomorrow();
            }
        }

        private static void ShowLostAndFoundMessage()
        {
            if (Game1.player.team.newLostAndFoundItems.Value)
            {
                Game1.morningQueue.Enqueue(() =>
                    Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems")));
            }
        }

        private static void SetNoNewLostAndFoundItems()
        {
            if (Game1.IsMasterGame)
            {
                Game1.player.team.newLostAndFoundItems.Value = false;
            }
        }

        private static void SetCanHostForBuiltCabin()
        {
            foreach (var building in Game1.getFarm().buildings)
            {
                if (building.daysOfConstructionLeft.Value <= 0 && building.indoors.Value is Cabin)
                {
                    Game1.player.slotCanHost = true;
                    break;
                }
            }
        }

        private static void UpdateTeamPerfection()
        {
            if (Utility.percentGameComplete() >= 1.0)
            {
                Game1.player.team.farmPerfect.Value = true;
            }
        }

        private static void HandleEndOfNightEvents()
        {
            if (Game1.farmEvent == null)
            {
                var handlePostFarmEventActionsMethod =
                    _helper.Reflection.GetMethod(typeof(Game1), "handlePostFarmEventActions");
                handlePostFarmEventActionsMethod.Invoke();
                Game1.showEndOfNightStuff();
            }
        }

        private static void UpdateLobbyData()
        {
            if (Game1.server != null)
            {
                Game1.server.updateLobbyData();
            }
        }

        private static void Barrier(string barrierName)
        {
            Game1.newDaySync.barrier(barrierName);
            while (!Game1.newDaySync.isBarrierReady(barrierName))
            {

            }
        }

        private static void WaitForVariable(string variableName)
        {
            while (!Game1.newDaySync.isVarReady(variableName))
            {
                
            }
        }

        private static void GetVanillaValues(out int totalDays, out int year, out int seasonNumber, out string seasonName)
        {
            totalDays = (int)Game1.stats.DaysPlayed;
            year = totalDays / 112;
            var daysThisYear = totalDays - (year * 112);
            seasonNumber = daysThisYear / 28;
            seasonName = SeasonsRandomizer.ValidSeasons[seasonNumber];
        }
    }
}
