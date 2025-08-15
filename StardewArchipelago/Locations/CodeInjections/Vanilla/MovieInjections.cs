using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;
using StardewValley.Locations;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;
using StardewValley.GameData.Movies;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class MovieInjections
    {
        private const string LOVED_REACTION = "love";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public static List<MovieConcession> GetConcessionsForGuest(string npc_name)
        public static bool GetConcessionsForGuest_OfferAllUnlockedSnacks_Prefix(string npc_name, ref List<MovieConcession> __result)
        {
            try
            {
                if (_archipelago.SlotData.Moviesanity <= Moviesanity.One)
                {
                    // On "None" and "One", the snacks are not shuffled at all
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var allConcessions = MovieTheater.GetConcessions().Values.Where(IsConcessionUnlocked).ToList();
                var daySaveRandom = Utility.CreateDaySaveRandom();
                Utility.Shuffle(daySaveRandom, allConcessions);

                __result = allConcessions;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetConcessionsForGuest_OfferAllUnlockedSnacks_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool IsConcessionUnlocked(MovieConcession movieConcession)
        {
            if (!_concessionFlavors.TryGetValue(movieConcession.Id, out var flavor))
            {
                return false;
            }

            return _archipelago.HasReceivedItem(flavor);
        }

        // public void RequestEndMovie(long uid)
        public static bool RequestEndMovie_SendMoviesanityLocations_Prefix(MovieTheater __instance, long uid)
        {
            try
            {
                // protected int CurrentState
                var currentState = _modHelper.Reflection.GetProperty<int>(__instance, "CurrentState").GetValue();

                if (_archipelago.SlotData.Moviesanity == Moviesanity.None || currentState != 1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckMovieLocation(__instance);
                CheckSnackLocation(__instance);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(RequestEndMovie_SendMoviesanityLocations_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void RequestEndMovie(long uid)
        public static void RequestEndMovie_SendMoviesanityLocations_Postfix(MovieTheater __instance, long uid)
        {
            try
            {
                foreach (var farmer in Game1.getAllFarmers().Where(x => x != null))
                {
                    farmer.lastSeenMovieWeek.Value = -1;
                }
                Utility.ForEachVillager((npc) =>
                {
                    npc.lastSeenMovieWeek.Value = -1;
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(RequestEndMovie_SendMoviesanityLocations_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static List<MovieData> GetMoviesForSeason(WorldDate date)
        public static bool GetMoviesForSeason_LoopEveryWeek_Prefix(WorldDate date, ref List<MovieData> __result)
        {
            try
            {
                var movieData = MovieTheater.GetMovieData();
                var moviesThisSeason = movieData.Where(x => MovieTheater.MovieSeasonMatches(x, Game1.season)).ToArray();
                __result = new List<MovieData>();
                for (var i = 0; i < 4; i++)
                {
                    if (date.TotalDays == Game1.Date.TotalDays)
                    {
                        // If we are checking today's movie, we add the current season movies, twice each, so they alternate 0-1-0-1
                        __result.Add(moviesThisSeason[i % moviesThisSeason.Length]);
                    }
                    else
                    {
                        // If we are checking the upcoming movie, we invert it, so they alternate 1-0-1-0
                        __result.Add(moviesThisSeason[(i + 1) % moviesThisSeason.Length]);
                    }
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetMoviesForSeason_LoopEveryWeek_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
        public static bool PerformTouchAction_WarnCorrectlyAboutLeaving_Prefix(MovieTheater __instance, string[] action, Vector2 playerStandingPosition)
        {
            try
            {
                if (__instance.IgnoreTouchActions())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (ArgUtility.Get(action, 0) != "Theater_Exit")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGetPoint(action, 1, out var point, out var error, "Point exitTile"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // protected int _exitX;
                // protected int _exitY;
                var exitXField = _modHelper.Reflection.GetField<int>(__instance, "_exitX");
                var exitYField = _modHelper.Reflection.GetField<int>(__instance, "_exitY");

                var theaterTileOffset = Town.GetTheaterTileOffset();
                exitXField.SetValue(point.X + theaterTileOffset.X);
                exitYField.SetValue(point.Y + theaterTileOffset.Y);

                // protected int CurrentState
                var currentStateProp = _modHelper.Reflection.GetProperty<int>(__instance, "CurrentState");
                var currentState = currentStateProp.GetValue();

                if (currentState == 0)
                {
                    Game1.player.position.Y -= (float)(((double)Game1.player.Speed + (double)Game1.player.addedSpeed) * 2.0);
                    Game1.player.Halt();
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_LeavePrompt"), Game1.currentLocation.createYesNoResponses(), "LeaveMovie");
                }
                else
                {
                    // protected void _Leave()
                    var leaveMethod = _modHelper.Reflection.GetMethod(__instance, "_Leave");
                    leaveMethod.Invoke();
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformTouchAction_WarnCorrectlyAboutLeaving_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckMovieLocation(MovieTheater movieTheater)
        {
            var moviesanitySetting = _archipelago.SlotData.Moviesanity;
            if (moviesanitySetting == Moviesanity.One)
            {
                _locationChecker.AddCheckedLocation("Watch A Movie");
                return;
            }

            var movie = MovieTheater.GetMovieToday();
            var movieName = _movieTitles[movie.Id];

            if (moviesanitySetting != Moviesanity.AllMovies)
            {
                if (!HasInvitedSomeoneWhoLovesTheMovie())
                {
                    return;
                }

                if (moviesanitySetting == Moviesanity.AllMoviesWithLovedSnack)
                {
                    if (!HasInvitedSomeoneWhoLovesTheSnack(movieTheater))
                    {
                        return;
                    }
                }
            }

            _locationChecker.AddCheckedLocation($"Watch {movieName}");
        }

        private static bool HasInvitedSomeoneWhoLovesTheMovie()
        {
            foreach (var movieInvitation in Game1.player.team.movieInvitations)
            {
                var response = MovieTheater.GetResponseForMovie(movieInvitation.invitedNPC);
                if (response == LOVED_REACTION)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckSnackLocation(MovieTheater movieTheater)
        {
            var moviesanitySetting = _archipelago.SlotData.Moviesanity;
            if (moviesanitySetting <= Moviesanity.AllMoviesWithLovedSnack)
            {
                return;
            }

            foreach (var movieInvitation in Game1.player.team.movieInvitations)
            {
                var concession = GetPurchasedConcession(movieTheater, movieInvitation);
                if (concession == null)
                {
                    continue;
                }

                if (moviesanitySetting == Moviesanity.AllMoviesAndAllLovedSnacks)
                {
                    var response = MovieTheater.GetConcessionTasteForCharacter(movieInvitation.invitedNPC, concession);
                    if (response != LOVED_REACTION)
                    {
                        continue;
                    }
                }

                _locationChecker.AddCheckedLocation($"Share {concession.Name}");
            }
        }

        private static bool HasInvitedSomeoneWhoLovesTheSnack(MovieTheater movieTheater)
        {
            foreach (var movieInvitation in Game1.player.team.movieInvitations)
            {
                var concession = GetPurchasedConcession(movieTheater, movieInvitation);
                var response = MovieTheater.GetConcessionTasteForCharacter(movieInvitation.invitedNPC, concession);
                if (response == LOVED_REACTION)
                {
                    return true;
                }
            }

            return false;
        }

        private static MovieConcession GetPurchasedConcession(MovieTheater movieTheater, MovieInvitation movieInvitation)
        {
            var concessions = movieTheater.GetConcessionsDictionary();
            return concessions.GetValueOrDefault(movieInvitation.invitedNPC);

        }

        private static readonly Dictionary<string, string> _movieTitles = new()
        {
            {"spring_movie_0", "The Brave Little Sapling"},
            {"fall_movie_0", "Mysterium"},
            {"summer_movie_0", "Journey Of The Prairie King: The Motion Picture"},
            {"summer_movie_1", "Wumbus"},
            {"winter_movie_1", "The Zuzu City Express"},
            {"winter_movie_0", "The Miracle At Coldstar Ranch"},
            {"spring_movie_1", "Natural Wonders: Exploring Our Vibrant World"},
            {"fall_movie_1", "It Howls In The Rain"},
        };

        private static readonly Dictionary<string, string> _concessionFlavors = new()
        {
            { Concessions.APPLE_SLICES, ConcessionFlavor.SWEET },
            { Concessions.BLACK_LICORICE, ConcessionFlavor.SWEET },
            { Concessions.CAPPUCCINO_MOUSSE_CAKE, ConcessionFlavor.SWEET },
            { Concessions.CHOCOLATE_POPCORN, ConcessionFlavor.SWEET },
            { Concessions.COTTON_CANDY, ConcessionFlavor.SWEET },
            { Concessions.FRIES, ConcessionFlavor.SALTY },
            { Concessions.HUMMUS_SNACK_PACK, ConcessionFlavor.SALTY },
            { Concessions.ICE_CREAM_SANDWICH, ConcessionFlavor.SWEET },
            { Concessions.JASMINE_TEA, ConcessionFlavor.DRINKS },
            { Concessions.JAWBREAKER, ConcessionFlavor.SWEET },
            { Concessions.JOJA_COLA, ConcessionFlavor.DRINKS },
            { Concessions.JOJACORN, ConcessionFlavor.SALTY },
            { Concessions.KALE_SMOOTHIE, ConcessionFlavor.DRINKS },
            { Concessions.NACHOS, ConcessionFlavor.MEALS },
            { Concessions.PANZANELLA_SALAD, ConcessionFlavor.MEALS },
            { Concessions.PERSONAL_PIZZA, ConcessionFlavor.MEALS },
            { Concessions.POPCORN, ConcessionFlavor.SALTY },
            { Concessions.ROCK_CANDY, ConcessionFlavor.SWEET },
            { Concessions.SALMON_BURGER, ConcessionFlavor.MEALS },
            { Concessions.SALTED_PEANUTS, ConcessionFlavor.SALTY },
            { Concessions.SOUR_SLIMES, ConcessionFlavor.SWEET },
            { Concessions.STAR_COOKIE, ConcessionFlavor.SWEET },
            { Concessions.STARDROP_SORBET, ConcessionFlavor.SWEET },
            { Concessions.TRUFFLE_POPCORN, ConcessionFlavor.SALTY },
        };
    }
}
