using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using System.Text;
using StardewArchipelago.Items.Traps;
using StardewValley.Characters;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class AppearanceRandomizer
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;

        private static Dictionary<Point, List<string>> _normalAppearances;
        private static Dictionary<Point, Dictionary<string, string>> _shuffledAppearances;

        public AppearanceRandomizer(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, Harmony harmony)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;

            GenerateSeededShuffledAppearances();

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.ChooseAppearance)),
                prefix: new HarmonyMethod(typeof(AppearanceRandomizer), nameof(AppearanceRandomizer.ChooseAppearance_AppearanceRandomizer_Prefix))
            );
        }

        public static void GenerateSeededShuffledAppearances()
        {
            _normalAppearances = new Dictionary<Point, List<string>>();
            _shuffledAppearances = new Dictionary<Point, Dictionary<string, string>>();
            var charData = Game1.characterData;
            foreach (var (name, data) in charData)
            {
                var size = data.Size;
                if (!_normalAppearances.ContainsKey(size))
                {
                    _normalAppearances.Add(size, new List<string>());
                }
                if (!_shuffledAppearances.ContainsKey(size))
                {
                    _shuffledAppearances.Add(size, new Dictionary<string, string>());
                }

                _normalAppearances[size].Add(name);
                AddExtraFunSprites(size, name);
            }

            var seed = int.Parse(_archipelago.SlotData.Seed);
            if (_archipelago.SlotData.AppearanceRandomization == AppearanceRandomization.Chaos)
            {
                seed += (int)Game1.stats.DaysPlayed;
            }
            var random = new Random(seed);
            foreach (var (size, characters) in _normalAppearances)
            {
                foreach (var character in characters)
                {
                    var shuffledAppearance = characters[random.Next(0, characters.Count)];
                    _shuffledAppearances[size].Add(character, shuffledAppearance);
                }
            }
        }

        private static void AddExtraFunSprites(Point size, string name)
        {
            if (name == "Krobus")
            {
                _normalAppearances[size].Add("Krobus_Trenchcoat");
            }
        }

        private static void AddExtraFunSprites()
        {
            var size = new Point(16, 32);
            _normalAppearances[size].Add("Toddler");
            _normalAppearances[size].Add("Toddler_dark");
            _normalAppearances[size].Add("Toddler_girl");
            _normalAppearances[size].Add("Toddler_girl_dark");
            _normalAppearances[size].Add("SafariGuy");
            size = new Point(32, 32);
            _normalAppearances[size].Add("TrashBear");
            _normalAppearances[size].Add("SeaMonsterKrobus");
            _normalAppearances[size].Add("raccoon");
            size = new Point(32, 42);
            _normalAppearances[size].Add("robot");
        }

        public static void RefreshAllNPCs()
        {
            foreach (var gameLocation in Game1.locations)
            {
                foreach (var gameLocationCharacter in gameLocation.characters)
                {
                    gameLocationCharacter.ChooseAppearance();
                }
            }
        }

        // public virtual void TryChooseAppearance(LocalizedContentManager content = null)
        public static bool ChooseAppearance_AppearanceRandomizer_Prefix(NPC __instance, LocalizedContentManager content)
        {
            try
            {
                if (_archipelago.SlotData.AppearanceRandomization == AppearanceRandomization.Disabled)
                {
                    return true; // run original logic
                }

                if (__instance.IsMonster)
                {
                    return true; // run original logic
                }

                if (!Game1.characterData.ContainsKey(__instance.Name))
                {
                    return true; // run original logic
                }

                var charData = Game1.characterData[__instance.Name];
                var size = charData.Size;
                if (!_shuffledAppearances.ContainsKey(size))
                {
                    return true; // run original logic
                }

                var shuffling = _shuffledAppearances[size];
                if (!shuffling.ContainsKey(__instance.Name))
                {
                    return true; // run original logic
                }

                var shuffledName = shuffling[__instance.Name];

                if (TryChooseAppearance(__instance, content, shuffledName))
                {
                    return false; // don't run original logic
                }


                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChooseAppearance_AppearanceRandomizer_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        private static bool TryChooseAppearance(NPC npc, LocalizedContentManager content, string shuffledName)
        {
            if (npc is Pet || npc is Child)
            {
                var a = 5;
            }

            npc.LastAppearanceId = null;
            if (npc.SimpleNonVillagerNPC)
            {
                return true;
            }
            content ??= Game1.content;
            var currentLocation = npc.currentLocation;
            if (currentLocation == null)
            {
                return true;
            }
            
            var textureName = NPC.getTextureNameForCharacter(shuffledName);

            var lastLocationNameForAppearanceField = _modHelper.Reflection.GetField<string>(npc, "LastLocationNameForAppearance");
            lastLocationNameForAppearanceField.SetValue(currentLocation.NameOrUniqueName);

            var successUniquePortrait = TryLoadUniquePortrait(npc, content, currentLocation, textureName);
            var successUniqueSprite = TryLoadUniqueSprite(npc, content, currentLocation, textureName);
            if (successUniquePortrait && successUniqueSprite)
            {
                return true;
            }

            var isWearingIslandAttireField = _modHelper.Reflection.GetField<bool>(npc, "isWearingIslandAttire");
            var isWearingIslandAttire = isWearingIslandAttireField.GetValue();

            var chosenAppearanceData = (CharacterAppearanceData)null;
            if (NPC.TryGetData(shuffledName, out var shuffledCharacterData))
            {
                var count = shuffledCharacterData.Appearance?.Count;
                if (count.HasValue && count > 0)
                {
                    var characterAppearanceDataList = new List<CharacterAppearanceData>();
                    var num2 = 0;
                    var daySaveRandom = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(shuffledName));
                    var season = currentLocation.GetSeason();
                    var isOutdoors = currentLocation.IsOutdoors;
                    var lowestPrecedence = int.MaxValue;
                    foreach (var possibleAppearance in shuffledCharacterData.Appearance)
                    {
                        if (possibleAppearance.Precedence <= lowestPrecedence &&
                            (possibleAppearance.IsIslandAttire != isWearingIslandAttire || possibleAppearance.Season.HasValue && possibleAppearance.Season.Value != season || (isOutdoors ? (possibleAppearance.Outdoors ? 1 : 0) : (possibleAppearance.Indoors ? 1 : 0)) == 0 ? 0 : (GameStateQuery.CheckConditions(possibleAppearance.Condition, currentLocation, random: daySaveRandom) ? 1 : 0)) != 0)
                        {
                            if (possibleAppearance.Precedence < lowestPrecedence)
                            {
                                lowestPrecedence = possibleAppearance.Precedence;
                                characterAppearanceDataList.Clear();
                                num2 = 0;
                            }
                            characterAppearanceDataList.Add(possibleAppearance);
                            num2 += possibleAppearance.Weight;
                        }
                    }
                    switch (characterAppearanceDataList.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            chosenAppearanceData = characterAppearanceDataList[0];
                            break;
                        default:
                            chosenAppearanceData = characterAppearanceDataList[characterAppearanceDataList.Count - 1];
                            var num4 = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(npc.Name)).Next(num2 + 1);
                            using (var enumerator = characterAppearanceDataList.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var current = enumerator.Current;
                                    num4 -= current.Weight;
                                    if (num4 <= 0)
                                    {
                                        chosenAppearanceData = current;
                                        break;
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            if (!successUniquePortrait)
            {
                var portraitAssetName = "Portraits/" + textureName;
                var successChosenPortrait = TryLoadChosenAppearancePortrait(npc, content, chosenAppearanceData, portraitAssetName, currentLocation);
                if (!successChosenPortrait && isWearingIslandAttire)
                {
                    var portraitBeachName = portraitAssetName + "_Beach";
                    if (content.DoesAssetExist<Texture2D>(portraitBeachName))
                    {
                        string error;
                        successChosenPortrait = npc.TryLoadPortraits(portraitBeachName, out error, content);
                        if (!successChosenPortrait)
                        {
                            var sourceName = $"' for island attire: ";
                            LogWarningMissingAsset(npc, portraitBeachName, currentLocation, error, "portraits", sourceName);
                        }
                    }
                }
                string error1;
                if (!successChosenPortrait && !npc.TryLoadPortraits(portraitAssetName, out error1, content))
                {
                    var sourceName = $"': ";
                    LogWarningMissingAsset(npc, portraitAssetName, currentLocation, error1, "portraits", sourceName);
                }
                if (successChosenPortrait)
                {
                    npc.LastAppearanceId = chosenAppearanceData?.Id;
                }
            }
            if (!successUniqueSprite)
            {
                var characterSpriteAssetName = "Characters/" + textureName;
                var successLoadChosenSprite = TryLoadChosenSprite(npc, content, chosenAppearanceData, characterSpriteAssetName, currentLocation);
                if (!successLoadChosenSprite && isWearingIslandAttire)
                {
                    var characterBeachSpriteAssetName = characterSpriteAssetName + "_Beach";
                    if (content.DoesAssetExist<Texture2D>(characterBeachSpriteAssetName))
                    {
                        string error;
                        successLoadChosenSprite = npc.TryLoadSprites(characterBeachSpriteAssetName, out error, content);
                        if (!successLoadChosenSprite)
                        {
                            var sourceName = $"' for island attire: ";
                            LogWarningMissingAsset(npc, characterBeachSpriteAssetName, currentLocation, error, "sprites", sourceName);
                        }
                    }
                }
                string error2;
                if (!successLoadChosenSprite && !npc.TryLoadSprites(characterSpriteAssetName, out error2, content))
                {
                    var sourceName = $"': ";
                    LogWarningMissingAsset(npc, characterSpriteAssetName, currentLocation, error2, "sprites", sourceName);
                }
                if (successLoadChosenSprite)
                {
                    npc.LastAppearanceId = chosenAppearanceData?.Id;
                }
            }
            if (shuffledCharacterData == null || npc.Sprite == null)
            {
                return false;
            }
            npc.Sprite.SpriteWidth = shuffledCharacterData.Size.X;
            npc.Sprite.SpriteHeight = shuffledCharacterData.Size.Y;
            npc.Sprite.ignoreSourceRectUpdates = false;

            return true;
        }

        private static bool TryLoadChosenSprite(NPC npc, LocalizedContentManager content, CharacterAppearanceData chosenAppearanceData, string assetName3, GameLocation currentLocation)
        {

            var flag4 = false;
            if (chosenAppearanceData != null && chosenAppearanceData.Sprite != null && chosenAppearanceData.Sprite != assetName3)
            {
                string error;
                flag4 = npc.TryLoadSprites(chosenAppearanceData.Sprite, out error, content);
                if (!flag4)
                {
                    var sourceName = $"' (per appearance entry '{chosenAppearanceData.Id}' in Data/Characters): ";
                    LogWarningMissingAsset(npc, chosenAppearanceData.Sprite, currentLocation, error, "sprites", sourceName);
                }
            }
            return flag4;
        }

        private static bool TryLoadChosenAppearancePortrait(NPC npc, LocalizedContentManager content, CharacterAppearanceData chosenAppearanceData, string portraitAssetName, GameLocation currentLocation)
        {

            var flag3 = false;
            if (chosenAppearanceData != null && chosenAppearanceData.Portrait != null && chosenAppearanceData.Portrait != portraitAssetName)
            {
                string error;
                flag3 = npc.TryLoadPortraits(chosenAppearanceData.Portrait, out error, content);
                if (!flag3)
                {
                    var sourceName = $"' (per appearance entry '{chosenAppearanceData.Id}' in Data/Characters): ";
                    LogWarningMissingAsset(npc, chosenAppearanceData.Portrait, currentLocation, error, "portraits", sourceName);
                }
            }
            return flag3;
        }

        private static bool TryLoadUniqueSprite(NPC npc, LocalizedContentManager content, GameLocation currentLocation, string textureName)
        {

            var flag2 = false;
            string propertyValue2;
            if (currentLocation.TryGetMapProperty("UniqueSprite", out propertyValue2) && ArgUtility.SplitBySpace(propertyValue2).Contains(textureName))
            {
                var assetName = $"Characters\\{textureName}_{currentLocation.Name}";
                string error;
                flag2 = npc.TryLoadSprites(assetName, out error, content);
                if (!flag2)
                {
                    var sourceName = $"' (per the UniqueSprite map property in '{currentLocation.NameOrUniqueName}'): ";
                    LogWarningMissingAsset(npc, assetName, currentLocation, error, "sprites", sourceName);
                }
            }
            return flag2;
        }

        private static bool TryLoadUniquePortrait(NPC npc, LocalizedContentManager content, GameLocation currentLocation, string textureName)
        {

            var flag1 = false;
            string propertyValue1;
            if (currentLocation.TryGetMapProperty("UniquePortrait", out propertyValue1) && ArgUtility.SplitBySpace(propertyValue1).Contains(textureName))
            {
                var assetName = $"Portraits\\{textureName}_{currentLocation.Name}";
                string error;
                flag1 = npc.TryLoadPortraits(assetName, out error, content);
                if (!flag1)
                {
                    var sourceName = $"' (per the UniquePortrait map property in '{currentLocation.NameOrUniqueName}'): ";
                    LogWarningMissingAsset(npc, assetName, currentLocation, error, "portraits", sourceName);
                }
            }
            return flag1;
        }

        private static void LogWarningMissingAsset(NPC npc, string assetName, GameLocation currentLocation, string error, string type, string sourceName)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("NPC ");
            stringBuilder.Append(npc.Name);
            stringBuilder.Append($" can't load {type} from '");
            stringBuilder.Append(assetName);
            stringBuilder.Append(sourceName);
            stringBuilder.Append(error);
            stringBuilder.Append($". Falling back to default {type}.");
            _logger.LogDebug(stringBuilder.ToString());
        }

        /*

                var spritePool = GetSpritePool();
                if (!spritePool.Any())
                {
                    return;
                }

                foreach (var gameLocation in Game1.locations)
                {
                    foreach (var character in gameLocation.characters)
                    {
                        var isVillager = character.IsVillager || character is Pet;

                        if (!isVillager && _archipelago.SlotData.AppearanceRandomization == AppearanceRandomization.Villagers)
                        {
                            continue;
                        }

                        var chosenSprite = GetRandomAppearance(character.Sprite, character.Name, spritePool);
                        character.Sprite = new AnimatedSprite(chosenSprite.Key, 0, chosenSprite.Value.X, chosenSprite.Value.Y);
                    }
                }*/

        /*private static Dictionary<string, Point> GetSpritePool()
        {
            var spritePool = new Dictionary<string, Point>();
            if (_archipelago.SlotData.AppearanceRandomization == AppearanceRandomization.Disabled)
            {
                return spritePool;
            }

            foreach (var (texture, point) in _characterSpritesAndSize)
            {
                spritePool.Add($"{_characterTexturePrefix}{texture}", point);
            }

            if (_archipelago.SlotData.AppearanceRandomization != AppearanceRandomization.Villagers)
            {
                // spritePool.AddRange(_monsterSprites.Select(x => $"{_monsterTexturePrefix}{x}"));
                // spritePool.AddRange(_animalSprites.Select(x => $"{_animalTexturePrefix}{x}"));
            }

            return spritePool;
        }

        private static KeyValuePair<string, Point> GetRandomAppearance(AnimatedSprite originalSprite, string characterName, List<KeyValuePair<string, Point>> spritePool)
        {
            var acceptableSprites = spritePool;
            var characterSize = new Point(originalSprite.SpriteWidth, originalSprite.SpriteHeight);
            if (_archipelago.SlotData.AppearanceRandomization != AppearanceRandomization.Chaos)
            {
                acceptableSprites = spritePool.Where(x => x.Value.Equals(characterSize)).ToList();
            }

            var random = GetSeededRandom(characterName);
            var chosenSpriteIndex = random.Next(0, acceptableSprites.Count);
            var chosenSprite = acceptableSprites[chosenSpriteIndex];
            return chosenSprite;
        }

        private static Random GetSeededRandom(string originalName)
        {
            var originalNameHashForSeed = Math.Abs(originalName.GetHash()) / 10;
            var seed = (int)Game1.uniqueIDForThisGame + originalNameHashForSeed;
            if (_archipelago.SlotData.AppearanceRandomizationDaily)
            {
                seed += (int)Game1.stats.DaysPlayed;
            }

            var random = new Random(seed);
            return random;
        }


        private const string _characterTexturePrefix = "Characters\\";
        private static readonly Dictionary<string, Point> _characterSpritesAndSize = new()
        {
            { "Junimo", new Point(16, 16) },
            { "Dwarf", new Point(16, 24) },
            { "Krobus", new Point(16, 24) },
            { "Krobus_Trenchcoat", new Point(16, 24) },
            { "TrashBear", new Point(32, 32) },
            { "Bear", new Point(32, 32) },
            { "Gourmand", new Point(32, 32) },
            { "LeahExMale", new Point(16, 32) },
            { "LeahExFemale", new Point(16, 32) },
            { "Toddler", new Point(16, 32) },
            { "Toddler_dark", new Point(16, 32) },
            { "Toddler_girl", new Point(16, 32) },
            { "Toddler_girl_dark", new Point(16, 32) },
            { "Birdie", new Point(16, 32) },
            { "ParrotBoy", new Point(16, 32) },
            { "George", new Point(16, 32) },
            { "Evelyn", new Point(16, 32) },
            { "Alex", new Point(16, 32) },
            { "Emily", new Point(16, 32) },
            { "Haley", new Point(16, 32) },
            { "Jodi", new Point(16, 32) },
            { "Sam", new Point(16, 32) },
            { "Vincent", new Point(16, 32) },
            { "Kent", new Point(16, 32) },
            { "Clint", new Point(16, 32) },
            { "Lewis", new Point(16, 32) },
            { "Caroline", new Point(16, 32) },
            { "Abigail", new Point(16, 32) },
            { "Pierre", new Point(16, 32) },
            { "Gus", new Point(16, 32) },
            { "Pam", new Point(16, 32) },
            { "Penny", new Point(16, 32) },
            { "Harvey", new Point(16, 32) },
            { "Elliott", new Point(16, 32) },
            { "Maru", new Point(16, 32) },
            { "Robin", new Point(16, 32) },
            { "Demetrius", new Point(16, 32) },
            { "Sebastian", new Point(16, 32) },
            { "Linus", new Point(16, 32) },
            { "Wizard", new Point(16, 32) },
            { "Marnie", new Point(16, 32) },
            { "Shane", new Point(16, 32) },
            { "Jas", new Point(16, 32) },
            { "Leah", new Point(16, 32) },
            { "MrQi", new Point(16, 32) },
            { "Sandy", new Point(16, 32) },
            { "Gunther", new Point(16, 32) },
            { "Marlon", new Point(16, 32) },
            { "Willy", new Point(16, 32) },
            { "Bouncer", new Point(16, 32) },
            { "Henchman", new Point(16, 32) },
            { "SafariGuy", new Point(16, 32) },
            { "Morris", new Point(16, 32) },
            { "Mariner", new Point(16, 32) },
            // { "maleRival", new Point(16, 32) },
            // { "femaleRival", new Point(16, 32) },
        };


        private static readonly string[] _villagers =
        {
            "Birdie",
            "ParrotBoy",
            "George",
            "Evelyn",
            "Alex",
            "Emily",
            "Haley",
            "Jodi",
            "Sam",
            "Vincent",
            "Kent",
            "Clint",
            "Lewis",
            "Caroline",
            "Abigail",
            "Pierre",
            "Gus",
            "Pam",
            "Penny",
            "Harvey",
            "Elliott",
            "Maru",
            "Robin",
            "Demetrius",
            "Sebastian",
            "Linus",
            "Wizard",
            "Marnie",
            "Shane",
            "Jas",
            "Leah",
            "Sandy",
            "Gunther",
            "Marlon",
            "Willy",
        };
        private const string _monsterTexturePrefix = _characterTexturePrefix + "Monsters\\";

        private static readonly string[] _monsterSprites =
        {
            "Wilderness Golem", "Skeleton", "Ghost", "Bat", "Big Slime", "Blue Squid", "Bug", "Pepper Rex", "Duggy",
            "Dust Spirit", "Dwarvish Sentry", "Fly", "Green Slime", "Grub", "Lava Crab", "Lava Lurk", "Spider",
            "Metal Head", "Mummy", "Rock Crab", "Stone Golem", "Serpent", "Shadow Brute", "Shadow Girl", "Shadow Guy",
            "Shadow Shaman", "Shadow Sniper", "Spiker", "Squid Kid",
        };

        private const string _animalTexturePrefix = "Animals\\";

        private static readonly string[] _animalSprites =
        {
            "horse", "Dog", "Dog1", "Dog2", "Dog3", "cat", "cat1", "cat2", "cat3", "White Chicken", "BabyWhite Chicken",
            "Brown Chicken", "BabyBrown Chicken", "Duck", "Rabbit", "BabyRabbit", "Cow", "BabyCow", "Sheep", "ShearedSheep", "BabySheep", "Pig", "BabyPig",
        };

        */
    }
}
