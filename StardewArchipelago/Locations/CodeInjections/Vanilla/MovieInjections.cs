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

        private static readonly Dictionary<string, string> _movieTitles = new Dictionary<string, string>()
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
    }
}
