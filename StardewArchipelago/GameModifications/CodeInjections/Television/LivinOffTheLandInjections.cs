﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Extensions;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.GameModifications.CodeInjections.Television
{
    internal class LivinOffTheLandInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static Hint[] _myActiveHints;
        private static List<string> _orderedTips;

        private static uint _lastDayCheckedLotL;
        private static int _currentEpisodeNumber;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _myActiveHints = _archipelago.GetMyActiveDesiredHints();
            var random = new Random((int)Game1.uniqueIDForThisGame);
            var validTips = ArchipelagoTips.Keys.Skip(1).Where(x => ArchipelagoTips[x]()).ToList();
            var firstTip = ArchipelagoTips.First().Key;
            _orderedTips = validTips.Shuffle(random);
            _orderedTips.Insert(0, firstTip);
            _lastDayCheckedLotL = 0;
            _currentEpisodeNumber = 0;
        }

        // protected virtual string getTodaysTip()
        public static bool GetTodaysTip_CustomLivinOffTheLand_Prefix(TV __instance, ref string __result)
        {
            try
            {
                var dayOfWeek = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                if (ModEntry.Instance.State.NumberOfLOTLEpisodesWatched > 0 && (dayOfWeek.Equals("Mon") || dayOfWeek.Equals("Thu")))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _myActiveHints = _archipelago.GetMyActiveDesiredHints();


                if (_lastDayCheckedLotL != Game1.stats.DaysPlayed)
                {
                    _lastDayCheckedLotL = Game1.stats.DaysPlayed;
                    _currentEpisodeNumber = ModEntry.Instance.State.NumberOfLOTLEpisodesWatched;
                    ModEntry.Instance.State.NumberOfLOTLEpisodesWatched += 1;
                }

                var episode = _orderedTips[_currentEpisodeNumber % _orderedTips.Count];

                while (episode.Contains("{0}") && !HasActiveHints())
                {
                    _currentEpisodeNumber = ModEntry.Instance.State.NumberOfLOTLEpisodesWatched;
                    episode = _orderedTips[_currentEpisodeNumber % _orderedTips.Count];
                    ModEntry.Instance.State.NumberOfLOTLEpisodesWatched += 1;
                }

                if (HasActiveHints() && episode.Contains("{0}"))
                {
                    var seed = Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed; //  + (ulong)(Game1.ticks % 100);
                    var random = new Random((int)seed);
                    var hintIndex = random.Next(_myActiveHints.Length);
                    var hint = _myActiveHints[hintIndex];
                    var formattedTip = string.Format(episode,
                        _archipelago.GetLocationName(hint),
                        _archipelago.GetPlayerName(hint.ReceivingPlayer),
                        _archipelago.GetPlayerGame(hint.ReceivingPlayer));
                    __result = formattedTip;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __result = episode;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetTodaysTip_CustomLivinOffTheLand_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // 0: Hint Location
        // 1: Hint requester
        // 2: Hint requester game
        private static readonly Dictionary<string, Func<bool>> ArchipelagoTips = new()
        {
            { "You may be used to this show airing on Mondays and Thursdays, but we are now airing on Tuesdays and Fridays as well! Tune in for brand new tips!", Always },
            { "Did you know that we're not the only station with an extended schedule? Our friends at The Queen of Sauce are now airing an extra episode every Saturday!", Always },
            { "Today we have a special guest in our studio! Please welcome... 'Rasmodius, Explorer of the Arcane'..? Sure thing buddy, the floor is yours!^^A very precious item was taken from me. It looks like ordinary ink, but it has power beyond your imagination. If you were to stumble upon it, please deliver it back to me. You will be generously rewarded. Make sure you speak to me directly!^^You heard the man folks, keep an eye out and you might earn something good!", Always },
            { "A friend of mine once told me of a story. Late in fall, they turned on their TV, and it seemed to be... haunted? I'm not sure I believe such a fairytale, but I thought it was interesting. There's something sinister about the signals you can receive on there. But the guy is kind of a deadbeat, so he was probably just pulling my leg.", Always },
            { "Some farmers enjoy going to the mines in their free time. That's fine, it's a good way to get ores and other valuable trinkets! Make sure you bring a weapon with you though, there are some nasty creatures down there. If you lost your weapon, check out your local adventure guild, I'm sure they'll be happy to help. You can even phone them! They do deliveries!", Always },
            { "Animal or Human, everyone enjoys a good cuddle at night!", Always },
            { "If you steal from your friends, they might resent you. But if you're close enough, usually they won't mind sharing some of their favorite items with you!", Always },
            { "I heard a rumor going around that some magically-attuned people have invented a magical clock that can skip time ahead. That sounds crazy, but I'm a big fan of folklore!", Always },
            { "There are legends among veteran fisherman, talking of fish that are stronger and rarer than all others. They claim there is only one of each, but that's just folklore. They are species like any other, don't overfish them!", Always },
            { "We recently got a call-in from a viewer in a discordant land. Apparently, over there, they have a lot of support for budding farmers. If you can pin down where they are, you'll even find guides and other resources!", Always},
            { "Sometimes, you might be a bit lost, and not know what is your purpose in life. Most cultures recommend looking to your ancestors for guidance. That's a bit old-fashioned, but sometimes an elder will have left you something that will help you figuring out your journey. Maybe a letter or something?", Always},
            { "Ever wanna take a long nap and wake up to a bunch of days gone by? We've all been there. Just remember, while you're snoozin', your friends and pets might feel a mite neglected. Balance, that's the name of the game!", HasMultiSleep },
            { "A backpack is a farmer's best friend. Out there somewhere, there's one for you too, just waitin' to be found. No fuss, no muss, just keep yours wits about ya, and maybe you'll stumble upon it!", HasEarlyBackpack },
            { "The climate can be a bit unpredictable these days. Sometimes it can be hot for months in a row. Don't hesitate to make the most of it with regrowing crops!", HasSeasonRandomizer },
            { "With the climate anomalies happening recently, even I tend to get confused about what season we're in. Please forgive me if I get something wrong. These are unprecedented times after all!", HasSeasonRandomizer },
            { "Let's talk about crops. They don't all have the same value, but money is not everything, trust me. Even if you have access to more valuable crops, it's usually a good idea to plant a little bit of everything. You never know what you might find comes harvest day!", HasCropsanity },
            { "We all gotta make a living somehow. Some people opt to gather interesting stuff, and move from town to town to sell it. They can provide out of season seeds, crops and fish. I hear some even use metal detectors to find and sell minerals and artifacts. How crazy is that?", HasMuseumsanity },
            { "Being friendly is generally a good thing, but you don't want to be taken advantage of either. Don't bother being too much more friendly to someone who isn't reciprocating yet. Give them time, I'm sure you'll be best buddies in a jiffy!", HasFriendsanity },
            { "Most bars have old-timey video games available in them. These old games often contain cheat codes, but can only be used once you beat the game once. So you don't need to go back to do everything you missed, a simple command will do the trick!", HasArcadeMachinesShuffled },
            { "Don't forget to never judge a book by its cover. Sometimes, even the most unassuming of doors can hide an extremely valuable interior. It's always worth a knock!", HasEntranceRandomizer },
            { "I heard a new show started airing recently. Tune in on Mondays and Fridays for... 'The Gateway Gazette'? I wonder what that's about...", HasNonChaosEntranceRandomizer },
            { "Life offers many doors, but some days, you might walk into one, and it's just a closet. Don't sweat it! Self care sometimes means going back to bed and calling it a day.", HasChaosEntranceRandomizer },
            { "Farmers don't always have it easy, so we need to stick together! Sending a gift to a farmer, even one far away, can do wonders to build community and make their day better! Just remember there's a fee. Worth it for the smiles it brings!", HasGifting },
            { "If you have a friend who needs to do a lot of running around, you can give them a coffee. I'm sure they'll appreciate the speed boost!", HasGifting },
            { "Opening your mail is usually pleasant, but sometimes, life throws you a curveball. Those unexpected surprises ain't nothin' to lose sleep over. They're just a little hiccup, no big deal. Keep smilin'!", HasEasyTraps },
            { "We have had reports of mail letters across the countryside being delivered with nasty surprises in them. Don't let 'em ruffle your feathers! Stay sharp and find a way to turn the tables. Remember, adversity's the best teacher on the rocky road of life!", HasHardTraps },
            { "I've heard a good story the other day. Apparently, there's a white-haired woman hiding in Cindersap forest. She only comes out at night, and it seems she's quite the troublemaker. But I'm sure that's just a myth to scare the kiddos into going to bed!", HasJuna },
            { "Brave adventurers are always talking about myths and legends. Rumor has it that, if you go deep into the woods, and you're lucky enough, you might encounter magical beings. The experience of petting a unicorn is unrivaled! But also, some people just dump their trash there. Try your luck!", HasDeepWoods },
            { "We got a letter from a folk all the way back in {2}. They recommend completing a... '{0}'?. I don't know what that is, but apparently, it's great! You should really get on that!", Always },
            { "...and that folks is how an ol' goblin changed my friend's life around.  Who knew a crayfish dish would be the thing to do it!  I say pay it forward.  Who knows, even goblins might teach ya a thing or two!", HasDistantLands },
            { "Now here's an odd rumor from an ol' miss up in Grampleton.  Mystics capable of turning the weave so thoroughly you can even hear their whispers over the radio!  Might help in a pinch I say!", HasMagic },
        };

        private static bool Always()
        {
            return true;
        }

        private static bool HasMultiSleep()
        {
            return _archipelago.SlotData.EnableMultiSleep;
        }

        private static bool HasEarlyBackpack()
        {
            return _archipelago.SlotData.BackpackProgression == BackpackProgression.ProgressiveEarlyBackpack;
        }

        private static bool HasSeasonRandomizer()
        {
            return _archipelago.SlotData.SeasonRandomization != SeasonRandomization.Disabled;
        }

        private static bool HasCropsanity()
        {
            return _archipelago.SlotData.Cropsanity == Cropsanity.Shuffled;
        }

        private static bool HasMuseumsanity()
        {
            return _archipelago.SlotData.Museumsanity != Museumsanity.None;
        }

        private static bool HasFriendsanity()
        {
            return _archipelago.SlotData.Friendsanity != Friendsanity.None;
        }

        private static bool HasArcadeMachinesShuffled()
        {
            return _archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.FullShuffling;
        }

        private static bool HasEntranceRandomizer()
        {
            return _archipelago.SlotData.EntranceRandomization != EntranceRandomization.Disabled;
        }

        private static bool HasNonChaosEntranceRandomizer()
        {
            return HasEntranceRandomizer() && _archipelago.SlotData.EntranceRandomization != EntranceRandomization.Chaos;
        }

        private static bool HasChaosEntranceRandomizer()
        {
            return _archipelago.SlotData.EntranceRandomization == EntranceRandomization.Chaos;
        }

        private static bool HasGifting()
        {
            return _archipelago.SlotData.Gifting;
        }

        private static bool HasTraps()
        {
            return _archipelago.SlotData.TrapItemsDifficulty != TrapItemsDifficulty.NoTraps;
        }

        private static bool HasEasyTraps()
        {
            return _archipelago.SlotData.TrapItemsDifficulty is TrapItemsDifficulty.Easy or TrapItemsDifficulty.Medium;
        }

        private static bool HasHardTraps()
        {
            return _archipelago.SlotData.TrapItemsDifficulty is TrapItemsDifficulty.Hard or TrapItemsDifficulty.Hell or TrapItemsDifficulty.Nightmare;
        }

        private static bool HasJuna()
        {
            return _archipelago.SlotData.Mods.HasMod(ModNames.JUNA);
        }

        private static bool HasDeepWoods()
        {
            return _archipelago.SlotData.Mods.HasMod(ModNames.DEEP_WOODS);
        }

        private static bool HasActiveHints()
        {
            return _myActiveHints.Any();
        }

        private static bool HasDistantLands()
        {
            return _archipelago.SlotData.Mods.HasMod(ModNames.DISTANT_LANDS);
        }

        private static bool HasMagic()
        {
            return _archipelago.SlotData.Mods.HasMod(ModNames.MAGIC);
        }
    }
}
