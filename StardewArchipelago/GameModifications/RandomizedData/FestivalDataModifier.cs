using Force.DeepCloner;
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
using StardewValley.GameData;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class FestivalDataModifier
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;
        private static Dictionary<string, string> _datesMapping;

        public FestivalDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
            _datesMapping = new Dictionary<string, string>();
        }

        public void OnFestivalDatesDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/FestivalDates"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    _datesMapping.Clear();
                    var festivalDatesData = asset.AsDictionary<string, string>().Data;

                    foreach (var festivalDateKey in festivalDatesData.Keys.ToArray())
                    {
                        ModifyFestivalDateData(festivalDatesData, festivalDateKey);
                    }
                },
                AssetEditPriority.Late
            );
        }

        //public void OnPassiveFestivalsDataRequested(object sender, AssetRequestedEventArgs e)
        //{
        //    if (!e.NameWithoutLocale.IsEquivalentTo("Data/PassiveFestivals"))
        //    {
        //        return;
        //    }

        //    e.Edit(asset =>
        //        {
        //            var passiveFestivalsData = asset.AsDictionary<string, PassiveFestivalData>().Data;

        //            foreach (var passiveFestivalKey in passiveFestivalsData.Keys.ToArray())
        //            {
        //                ModifyPassiveFestivalData(passiveFestivalsData, passiveFestivalKey);
        //            }
        //        },
        //        AssetEditPriority.Late
        //    );
        //}

        private void ModifyFestivalDateData(IDictionary<string, string> allFestivalDatesData, string festivalDateKey)
        {
            var festivalName = allFestivalDatesData[festivalDateKey];

            if (!_dataRandomization.FestivalData.TryGetValue(festivalName, out var randomizedFestivalData))
            {
                return;
            }

            var numbers = "0123456789".ToCharArray();
            var indexSplit = festivalDateKey.IndexOfAny(numbers);
            var season = festivalDateKey.Substring(0, indexSplit);
            var day = int.Parse(festivalDateKey.Substring(indexSplit));
            if (!string.IsNullOrWhiteSpace(randomizedFestivalData.Season))
            {
                season = randomizedFestivalData.Season;
            }
            if (randomizedFestivalData.StartDay is >= 1 and <= 28)
            {
                day = randomizedFestivalData.StartDay.Value;
            }
            var newDateKey = season.ToLower() + day;
            if (newDateKey.Equals(festivalDateKey, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            allFestivalDatesData.Remove(festivalDateKey);
            allFestivalDatesData.Add(newDateKey, festivalName);
            _datesMapping.Add(newDateKey, festivalDateKey);
        }

        //private void ModifyPassiveFestivalData(IDictionary<string, PassiveFestivalData> passiveFestivalsData, string passiveFestivalKey)
        //{
        //    var festivalData = passiveFestivalsData[passiveFestivalKey];

        //    var randomizedFestivalData = _dataRandomization.FestivalData.Values.FirstOrDefault(x => x.Name.Replace(" ", "").Equals(passiveFestivalKey, StringComparison.InvariantCultureIgnoreCase));
        //    if (randomizedFestivalData == null)
        //    {
        //        return;
        //    }

        //    if (!string.IsNullOrWhiteSpace(randomizedFestivalData.Season))
        //    {
        //        var seasonNumber = Utility.getSeasonNumber(randomizedFestivalData.Season);
        //        if (seasonNumber >= 0)
        //        {
        //            festivalData.Season = (Season)seasonNumber;
        //        }
        //    }
        //    if (randomizedFestivalData.StartDay != null && randomizedFestivalData.EndDay != null)
        //    {
        //        festivalData.StartDay = randomizedFestivalData.StartDay.Value;
        //        festivalData.EndDay = randomizedFestivalData.EndDay.Value;
        //    }

        //    passiveFestivalsData[passiveFestivalKey] = festivalData;
        //}

        // public static bool tryToLoadFestivalData(string festival, out string assetName, out Dictionary<string, string> data, out string locationName, out int startTime, out int endTime)
        public static bool TryToLoadFestivalData_LoadOriginalDataForNewDate_Prefix(string festival, out string assetName, out Dictionary<string, string> data, out string locationName, out int startTime, out int endTime, ref bool __result)
        {
            try
            {
                if (_datesMapping.TryGetValue(festival, out var originalFestival))
                {
                    __result = TryLoadFestivalDataUnmodified(originalFestival, out assetName, out data, out locationName, out startTime, out endTime);
                }
                else
                {
                    __result = TryLoadFestivalDataUnmodified(festival, out assetName, out data, out locationName, out startTime, out endTime);
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryToLoadFestivalData_LoadOriginalDataForNewDate_Prefix)}:\n{ex}");
                assetName = $@"Data\Festivals\{festival}";
                data = null;
                locationName = null;
                startTime = 0;
                endTime = 0;
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool TryLoadFestivalDataUnmodified(string festival, out string assetName, out Dictionary<string, string> festivalData, out string locationName, out int startTime, out int endTime)
        {
            assetName = $@"Data\Festivals\{festival}";
            festivalData = null;
            locationName = null;
            startTime = 0;
            endTime = 0;
            if (Event.invalidFestivals.Contains(festival))
            {
                return false;
            }
            var contentLoader = Game1.content.CreateTemporary();
            try
            {
                if (!contentLoader.DoesAssetExist<Dictionary<string, string>>(assetName))
                {
                    Event.invalidFestivals.Add(festival);
                    return false;
                }
                festivalData = contentLoader.Load<Dictionary<string, string>>(assetName);
            }
            catch
            {
                Event.invalidFestivals.Add(festival);
                return false;
            }

            if (!festivalData.TryGetValue("conditions", out var festivalConditionsStr))
            {
                _logger.LogError($"Festival '{festival}' doesn't have the required 'conditions' festivalData field.");
                return false;
            }

            var festivalConditions = festivalConditionsStr.Split('/', StringSplitOptions.TrimEntries);
            if (!ArgUtility.TryGet(festivalConditions, 0, out locationName, out var error, false, nameof(locationName)) || !ArgUtility.TryGet(festivalConditions, 1, out var str2, out error, false, "string rawTimeSpan"))
            {
                var interpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 3);
                interpolatedStringHandler.AppendLiteral("Festival '");
                interpolatedStringHandler.AppendFormatted(festival);
                interpolatedStringHandler.AppendLiteral("' has preconditions '");
                interpolatedStringHandler.AppendFormatted(festivalConditionsStr);
                interpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
                interpolatedStringHandler.AppendFormatted(error);
                interpolatedStringHandler.AppendLiteral(".");
                var stringAndClear = interpolatedStringHandler.ToStringAndClear();
                _logger.LogError(stringAndClear);
                return false;
            }
            var array2 = ArgUtility.SplitBySpace(str2);
            if (ArgUtility.TryGetInt(array2, 0, out startTime, out error, nameof(startTime)) && ArgUtility.TryGetInt(array2, 1, out endTime, out error, nameof(endTime)))
            {
                return true;
            }
            var interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(79, 4);
            interpolatedStringHandler1.AppendLiteral("Festival '");
            interpolatedStringHandler1.AppendFormatted(festival);
            interpolatedStringHandler1.AppendLiteral("' has preconditions '");
            interpolatedStringHandler1.AppendFormatted(festivalConditionsStr);
            interpolatedStringHandler1.AppendLiteral("' with time range '");
            interpolatedStringHandler1.AppendFormatted(string.Join(" ", array2));
            interpolatedStringHandler1.AppendLiteral("' which couldn't be parsed: ");
            interpolatedStringHandler1.AppendFormatted(error);
            interpolatedStringHandler1.AppendLiteral(".");
            var stringAndClear1 = interpolatedStringHandler1.ToStringAndClear();
            _logger.LogError(stringAndClear1);
            return false;
        }

        // public static bool tryToLoadFestival(string festival, out Event ev)
        public static bool TryToLoadFestival_ConsiderModifiedFestivals_Prefix(string festival, out Event ev, ref bool __result)
        {
            ev = null;
            try
            {
                if (!Event.tryToLoadFestivalData(festival, out var assetName, out var data1, out var locationName, out var startTime, out var endTime) || locationName != Game1.currentLocation.Name || Game1.timeOfDay < startTime || Game1.timeOfDay >= endTime)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_datesMapping.TryGetValue(festival, out var originalFestival))
                {
                    festival = originalFestival;
                }

                ev = new Event()
                {
                    id = "festival_" + festival,
                    isFestival = true,
                };

                // private string festivalDataAssetName;
                var festivalDataAssetNameField = _helper.Reflection.GetField<string>(ev, "festivalDataAssetName");
                // private Dictionary<string, string> festivalData;
                var festivalDataField = _helper.Reflection.GetField<Dictionary<string, string>>(ev, "festivalData");
                // private Dictionary<string, Vector3> actorPositionsAfterMove;
                var actorPositionsAfterMoveField = _helper.Reflection.GetField<Dictionary<string, Vector3>>(ev, "actorPositionsAfterMove");
                // private Color previousAmbientLight;
                var previousAmbientLightField = _helper.Reflection.GetField<Color>(ev, "previousAmbientLight");

                festivalDataAssetNameField.SetValue(assetName);
                festivalDataField.SetValue(data1);
                actorPositionsAfterMoveField.SetValue(new Dictionary<string, Vector3>());
                previousAmbientLightField.SetValue(Game1.ambientLight);
                festivalDataField.GetValue()["file"] = festival;
                if (!ev.TryGetFestivalDataForYear("set-up", out var data2))
                {
                    _logger.LogError("Festival " + ev.id + " doesn't have the required 'set-up' data field.");
                }
                ev.eventCommands = Event.ParseCommands(data2, ev.farmer);
                Game1.player.festivalScore = 0;
                Game1.setRichPresence(nameof(festival), festival);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryToLoadFestival_ConsiderModifiedFestivals_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static int getStartTimeOfFestival()
        public static bool GetStartTimeOfFestival_LoadOriginalDataForNewDate_Prefix(ref int __result)
        {
            try
            {
                if (Game1.weatherIcon != 1)
                {
                    __result = -1;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var currentKey = Game1.currentSeason + Game1.dayOfMonth;
                if (_datesMapping.TryGetValue(currentKey, out var originalFestivalKey))
                {
                    currentKey = originalFestivalKey;
                }

                var festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>(@"Data\Festivals\" + currentKey);
                var conditions = festivalData["conditions"];
                var timeCondition = conditions.Split('/')[1];
                var startTimeStr = ArgUtility.SplitBySpaceAndGet(timeCondition, 0);
                var startTime = Convert.ToInt32(startTimeStr);
                __result = startTime;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetStartTimeOfFestival_LoadOriginalDataForNewDate_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // Billboard: public virtual List<Billboard.BillboardEvent> GetEventsForDay(int day, Dictionary<int, List<NPC>> birthdays)
        public static bool GetEventsForDay_ConsiderModifiedFestivals_Prefix(Billboard __instance, int day, Dictionary<int, List<NPC>> birthdays, ref List<Billboard.BillboardEvent> __result)
        {
            try
            {
                var eventsForDay = new List<Billboard.BillboardEvent>();
                if (Utility.isFestivalDay(day, Game1.season))
                {
                    var festivalKey = Game1.currentSeason + day;
                    var dataKey = festivalKey;
                    if (_datesMapping.TryGetValue(festivalKey, out var originalFestivalKey))
                    {
                        dataKey = originalFestivalKey;
                    }
                    var displayName = Game1.temporaryContent.Load<Dictionary<string, string>>(@"Data\Festivals\" + dataKey)["name"];
                    eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Festival,
                        new[]
                        {
                            festivalKey,
                        },
                        displayName));
                }

                if (Utility.TryGetPassiveFestivalDataForDay(day, Game1.season, null, out var id, out var data, true))
                {
                    var showOnCalendar = data?.ShowOnCalendar;
                    if (showOnCalendar == null || showOnCalendar == false)
                    {
                        data.DisplayName = Game1.content.LoadString($"Strings\\1_6_Strings:{id}");
                    }
                    var text = TokenParser.ParseText(data.DisplayName);
                    if (!GameStateQuery.CheckConditions(data.Condition))
                    {
                        eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.PassiveFestival, new[]
                        {
                            id,
                        }, "???")
                        {
                            locked = true,
                        });
                    }
                    else
                    {
                        eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.PassiveFestival, new string[1]
                        {
                            id,
                        }, text));
                    }
                }

                //if (Game1.IsSummer && (day == 20 || day == 21))
                //{
                //    var displayName = Game1.content.LoadString("Strings\\1_6_Strings:TroutDerby");
                //    eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.FishingDerby, Array.Empty<string>(), displayName));
                //}
                //else if (Game1.IsWinter && (day == 12 || day == 13))
                //{
                //    var displayName = Game1.content.LoadString("Strings\\1_6_Strings:SquidFest");
                //    eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.FishingDerby, Array.Empty<string>(), displayName));
                //}

                // private List<int> booksellerdays;
                var booksellerdaysField = _helper.Reflection.GetField<List<int>>(__instance, "booksellerdays");
                var booksellerdays = booksellerdaysField.GetValue();

                if (booksellerdays.Contains(day))
                {
                    var displayName = Game1.content.LoadString("Strings\\1_6_Strings:Bookseller");
                    eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Bookseller, Array.Empty<string>(), displayName));
                }
                if (birthdays.TryGetValue(day, out var npcList))
                {
                    foreach (var npc in npcList)
                    {
                        var ch = npc.displayName.Last<char>();
                        var displayName = ch == 's' || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de && (ch == 'x' || ch == 'ß' || ch == 'z')
                            ? Game1.content.LoadString("Strings\\UI:Billboard_SBirthday", npc.displayName)
                            : Game1.content.LoadString("Strings\\UI:Billboard_Birthday", npc.displayName);
                        Texture2D texture;
                        try
                        {
                            texture = Game1.content.Load<Texture2D>("Characters\\" + npc.getTextureName());
                        }
                        catch
                        {
                            texture = npc.Sprite.Texture;
                        }
                        eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Birthday, new string[1]
                        {
                            npc.Name,
                        }, displayName, texture, npc.getMugShotSourceRect()));
                    }
                }
                var farmerSet = new HashSet<Farmer>();
                var onlineFarmers = Game1.getOnlineFarmers();
                foreach (var farmer in onlineFarmers)
                {
                    if (!farmerSet.Contains(farmer) && farmer.isEngaged() && !farmer.hasCurrentOrPendingRoommate())
                    {
                        var sub2 = (string)null;
                        var worldDate = (WorldDate)null;
                        var characterFromName = Game1.getCharacterFromName(farmer.spouse);
                        if (characterFromName != null)
                        {
                            worldDate = farmer.friendshipData[farmer.spouse].WeddingDate;
                            sub2 = characterFromName.displayName;
                        }
                        else
                        {
                            var spouse = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
                            if (spouse.HasValue)
                            {
                                var player = Game1.GetPlayer(spouse.Value);
                                if (player != null && onlineFarmers.Contains(player))
                                {
                                    worldDate = farmer.team.GetFriendship(farmer.UniqueMultiplayerID, spouse.Value).WeddingDate;
                                    farmerSet.Add(player);
                                    sub2 = player.Name;
                                }
                            }
                        }
                        if (!(worldDate == null))
                        {
                            if (worldDate.TotalDays < Game1.Date.TotalDays)
                            {
                                worldDate = new WorldDate(Game1.Date);
                                ++worldDate.TotalDays;
                            }
                            var totalDays1 = worldDate?.TotalDays;
                            var totalDays2 = Game1.Date.TotalDays;
                            if (totalDays1.GetValueOrDefault() >= totalDays2 & totalDays1.HasValue && Game1.season == worldDate.Season && day == worldDate.DayOfMonth)
                            {
                                eventsForDay.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Wedding, new string[2]
                                {
                                    farmer.Name,
                                    sub2,
                                }, Game1.content.LoadString("Strings\\UI:Calendar_Wedding", farmer.Name, sub2)));
                                farmerSet.Add(farmer);
                            }
                        }
                    }
                }

                __result = eventsForDay;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetEventsForDay_ConsiderModifiedFestivals_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void warpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
        public static bool WarpFarmer_ConsiderModifiedFestivals_Prefix(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
        {
            try
            {
                if (Game1.weatherIcon != 1 || Game1.whereIsTodaysFest == null || !locationRequest.Name.Equals(Game1.whereIsTodaysFest) || Game1.warpingForForcedRemoteEvent)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var currentKey = Game1.currentSeason + Game1.dayOfMonth;
                if (_datesMapping.TryGetValue(currentKey, out var originalFestivalKey) && currentKey != originalFestivalKey)
                {
                    currentKey = originalFestivalKey;
                }
                else
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var performWarpFarmerMethod = _helper.Reflection.GetMethod(typeof(Game1), "performWarpFarmer");

                var warp_offset_x = Game1.nextFarmerWarpOffsetX;
                var warp_offset_y = Game1.nextFarmerWarpOffsetY;
                Game1.nextFarmerWarpOffsetX = 0;
                Game1.nextFarmerWarpOffsetY = 0;
                foreach (var activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
                {
                    if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) && Game1.dayOfMonth >= data.StartDay && Game1.dayOfMonth <= data.EndDay && data.Season == Game1.season && data.MapReplacements != null && data.MapReplacements.TryGetValue(locationRequest.Name, out var locationName))
                    {
                        locationRequest = Game1.getLocationRequest(locationName);
                    }
                }
                switch (locationRequest.Name)
                {
                    case "BusStop":
                        if (tileX < 10)
                        {
                            tileX = 10;
                            break;
                        }
                        break;
                    case "Farm":
                        switch (Game1.currentLocation?.NameOrUniqueName)
                        {
                            case "FarmCave":
                                if (tileX == 34 && tileY == 6)
                                {
                                    if (Game1.getFarm().TryGetMapPropertyAs("FarmCaveEntry", out Point parsed))
                                    {
                                        tileX = parsed.X;
                                        tileY = parsed.Y;
                                        break;
                                    }
                                    switch (Game1.whichFarm)
                                    {
                                        case 5:
                                            tileX = 30;
                                            tileY = 36;
                                            break;
                                        case 6:
                                            tileX = 34;
                                            tileY = 16;
                                            break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                                break;
                            case "Forest":
                                if (tileX == 41 && tileY == 64)
                                {
                                    if (Game1.getFarm().TryGetMapPropertyAs("ForestEntry", out Point parsed))
                                    {
                                        tileX = parsed.X;
                                        tileY = parsed.Y;
                                        break;
                                    }
                                    switch (Game1.whichFarm)
                                    {
                                        case 5:
                                            tileX = 40;
                                            tileY = 64;
                                            break;
                                        case 6:
                                            tileX = 82;
                                            tileY = 103;
                                            break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                                break;
                            case "BusStop":
                                if (tileX == 79 && tileY == 17 && Game1.getFarm().TryGetMapPropertyAs("BusStopEntry", out Point parsed1))
                                {
                                    tileX = parsed1.X;
                                    tileY = parsed1.Y;
                                    break;
                                }
                                break;
                            case "Backwoods":
                                if (tileX == 40 && tileY == 0 && Game1.getFarm().TryGetMapPropertyAs("BackwoodsEntry", out Point parsed2))
                                {
                                    tileX = parsed2.X;
                                    tileY = parsed2.Y;
                                    break;
                                }
                                break;
                        }
                        break;
                    case "IslandSouth":
                        if (tileX <= 15 && tileY <= 6)
                        {
                            tileX = 21;
                            tileY = 43;
                            break;
                        }
                        break;
                    case "Trailer":
                        if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
                        {
                            locationRequest = Game1.getLocationRequest("Trailer_Big");
                            tileX = 13;
                            tileY = 24;
                            break;
                        }
                        break;
                    case "Club":
                        if (!Game1.player.hasClubCard)
                        {
                            locationRequest = Game1.getLocationRequest("SandyHouse");
                            locationRequest.OnWarp += () =>
                            {
                                var characterFromName = Game1.currentLocation.getCharacterFromName("Bouncer");
                                if (characterFromName == null)
                                {
                                    return;
                                }
                                var vector2 = new Vector2(17f, 4f);
                                characterFromName.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:Club_Bouncer_TextAboveHead" + (Game1.random.Next(2) + 1).ToString()));
                                var num = Game1.random.Next();
                                Game1.currentLocation.playSound("thudStep");
                                Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(288, 100f, 1, 24, vector2 * 64f, true, false, Game1.currentLocation, Game1.player)
                                {
                                    shakeIntensity = 0.5f,
                                    shakeIntensityChange = 1f / 500f,
                                    extraInfoForEndBehavior = num,
                                    endFunction = new TemporaryAnimatedSprite.endBehavior(Game1.currentLocation.removeTemporarySpritesWithID)
                                }, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector2 * 64f + new Vector2(5f, 0.0f) * 4f, true, false, 0.0263f, 0.0f, Color.Yellow, 4f, 0.0f, 0.0f, 0.0f)
                                {
                                    id = num
                                }, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector2 * 64f + new Vector2(5f, 0.0f) * 4f, true, true, 0.0263f, 0.0f, Color.Orange, 4f, 0.0f, 0.0f, 0.0f)
                                {
                                    delayBeforeAnimationStart = 100,
                                    id = num
                                }, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector2 * 64f + new Vector2(5f, 0.0f) * 4f, true, false, 0.0263f, 0.0f, Color.White, 3f, 0.0f, 0.0f, 0.0f)
                                {
                                    delayBeforeAnimationStart = 200,
                                    id = num
                                });
                                Game1.currentLocation.netAudio.StartPlaying("fuse");
                            };
                            tileX = 17;
                            tileY = 4;
                            break;
                        }
                        break;
                }
                if (VolcanoDungeon.IsGeneratedLevel(locationRequest.Name))
                {
                    warp_offset_x = 0;
                    warp_offset_y = 0;
                }
                if (Game1.player.isRidingHorse() && Game1.currentLocation != null)
                {
                    var new_location = locationRequest.Location ?? Game1.getLocationFromName(locationRequest.Name);
                    if (Game1.game1.ShouldDismountOnWarp(Game1.player.mount, Game1.currentLocation, new_location))
                    {
                        Game1.player.mount.dismount();
                        warp_offset_x = 0;
                        warp_offset_y = 0;
                    }
                }
                if (Game1.weatherIcon == 1 && Game1.whereIsTodaysFest != null && locationRequest.Name.Equals(Game1.whereIsTodaysFest) && !Game1.warpingForForcedRemoteEvent)
                {
                    var festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>(@"Data\Festivals\" + currentKey);
                    var strArray = ArgUtility.SplitBySpace(festivalData["conditions"].Split('/')[1]);
                    if (Game1.timeOfDay <= Convert.ToInt32(strArray[1]))
                    {
                        if (Game1.timeOfDay < Convert.ToInt32(strArray[0]))
                        {
                            if (Game1.currentLocation?.Name == "Hospital")
                            {
                                locationRequest = Game1.getLocationRequest("BusStop");
                                tileX = 34;
                                tileY = 23;
                            }
                            else
                            {
                                Game1.player.Position = Game1.player.lastPosition;
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2973"));
                                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                            }
                        }
                        else
                        {
                            if (Game1.IsMultiplayer)
                            {
                                Game1.netReady.SetLocalReady("festivalStart", true);
                                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                                {
                                    Game1.exitActiveMenu();
                                    if (Game1.player.mount != null)
                                    {
                                        Game1.player.mount.dismount();
                                        warp_offset_x = 0;
                                        warp_offset_y = 0;
                                    }
                                    performWarpFarmerMethod.Invoke(locationRequest, tileX, tileY, facingDirectionAfterWarp);
                                });
                                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                            }
                            if (Game1.player.mount != null)
                            {
                                Game1.player.mount.dismount();
                                warp_offset_x = 0;
                                warp_offset_y = 0;
                            }
                        }
                    }
                }
                tileX += warp_offset_x;
                tileY += warp_offset_y;
                performWarpFarmerMethod.Invoke(locationRequest, tileX, tileY, facingDirectionAfterWarp);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(WarpFarmer_ConsiderModifiedFestivals_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static int originalWeatherIcon = -1;
        private static Dictionary<string, string> _festivalData = null;

        // public virtual void OnGame1_PerformTenMinuteClockUpdate(Action action) => action();
        public static bool OnGame1_PerformTenMinuteClockUpdate_ConsiderModifiedFestivals_Prefix(ModHooks __instance, Action action)
        {
            try
            {
                if (Game1.weatherIcon == 1)
                {
                    originalWeatherIcon = Game1.weatherIcon;
                    Game1.weatherIcon = 0;


                    var currentKey = Game1.currentSeason + Game1.dayOfMonth;
                    if (_datesMapping.TryGetValue(currentKey, out var originalFestivalKey))
                    {
                        currentKey = originalFestivalKey;
                    }

                    _festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>(@"Data\Festivals\" + currentKey);
                    var strArray = _festivalData["conditions"].Split('/');
                    if (Game1.whereIsTodaysFest == null)
                    {
                        Game1.whereIsTodaysFest = strArray[0];
                    }
                }
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnGame1_PerformTenMinuteClockUpdate_ConsiderModifiedFestivals_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
        public static void OnGame1_PerformTenMinuteClockUpdate_ConsiderModifiedFestivals_Postfix(ModHooks __instance, Action action)
        {
            try
            {
                if (originalWeatherIcon == 1)
                {
                    Game1.weatherIcon = originalWeatherIcon;

                    var strArray = _festivalData["conditions"].Split('/');
                    var int32 = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(strArray[1], 0));
                    if (Game1.timeOfDay != int32)
                    {
                        return;
                    }

                    string text;
                    if (_festivalData.TryGetValue("startedMessage", out text))
                    {
                        Game1.showGlobalMessage(TokenParser.ParseText(text));
                    }
                    else
                    {
                        string str;
                        if (!_festivalData.TryGetValue("locationDisplayName", out str))
                        {
                            var name = strArray[0];
                            switch (name)
                            {
                                case "Forest":
                                    str = Game1.IsWinter ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2634") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2635");
                                    break;
                                case "Town":
                                    str = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2637");
                                    break;
                                case "Beach":
                                    str = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2639");
                                    break;
                                default:
                                    str = TokenParser.ParseText(GameLocation.GetData(name)?.DisplayName) ?? name;
                                    break;
                            }
                        }
                        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2640", (object)_festivalData["name"]) + str);
                    }
                }

                if (originalWeatherIcon != -1)
                {
                    originalWeatherIcon = -1;
                    _festivalData = null;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnGame1_PerformTenMinuteClockUpdate_ConsiderModifiedFestivals_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
