using System.Collections.Generic;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.Objects;
using Color = Microsoft.Xna.Framework.Color;

namespace StardewArchipelago.Items.Traps
{
    public class OutfitChanger
    {
        private ILogger _logger;
        private IModHelper _modHelper;

        public OutfitChanger(ILogger logger, IModHelper modHelper)
        {
            _logger = logger;
            _modHelper = modHelper;
        }

        public bool TryGetMakeoverOutfit(out MakeoverOutfit chosenMakeoverOutfit)
        {
            chosenMakeoverOutfit = null;
            var random = Game1.random;
            var makeoverOutfits = DataLoader.MakeoverOutfits(Game1.content).ToList();
            if (makeoverOutfits == null)
            {
                return false;
            }

            for (var index = 0; index < makeoverOutfits.Count; ++index)
            {
                var makeoverOutfit = makeoverOutfits[index];
                if (makeoverOutfit.Gender.HasValue && makeoverOutfit.Gender.Value != Game1.player.Gender)
                {
                    makeoverOutfits.RemoveAt(index);
                    --index;
                    continue;
                }

                foreach (var outfitPart in makeoverOutfit.OutfitParts)
                {
                    if (!outfitPart.MatchesGender(Game1.player.Gender))
                    {
                        continue;
                    }

                    var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(outfitPart.ItemId);
                    var alreadyWearingThisPart =
                        Game1.player.hat.Value?.QualifiedItemId == dataOrErrorItem.QualifiedItemId ||
                        Game1.player.shirtItem.Value?.QualifiedItemId == dataOrErrorItem.QualifiedItemId ||
                        Game1.player.pantsItem.Value?.QualifiedItemId == dataOrErrorItem.QualifiedItemId;
                    if (alreadyWearingThisPart)
                    {
                        makeoverOutfits.RemoveAt(index);
                        --index;
                        break;
                    }
                }
            }

            chosenMakeoverOutfit = random.ChooseFrom(makeoverOutfits);
            if (random.NextDouble() < 0.03)
            {
                chosenMakeoverOutfit = new MakeoverOutfit()
                {
                    OutfitParts = new List<MakeoverItem>()
                    {
                        new() { ItemId = "(H)LaurelWreathCrown" },
                        new()
                        {
                            ItemId = "(P)3",
                            Color = "247 245 205"
                        },
                        new() { ItemId = "(S)1199" }
                    }
                };
            }

            if (chosenMakeoverOutfit?.OutfitParts == null)
            {
                return false;
            }

            return true;
        }

        public void EquipMakeoverOutfit(MakeoverOutfit makeoverOutfit, bool includeHat = true, bool includeShirt = true, bool includePants = true)
        {
            var player = Game1.player;
            player.Equip(null, player.shirtItem);
            player.Equip(null, player.pantsItem);
            player.Equip(null, player.hat);

            var hasEquippedHat = false;
            var hasEquippedShirt = false;
            var hasEquippedPants = false;
            foreach (var outfitPart in makeoverOutfit.OutfitParts)
            {
                if (!outfitPart.MatchesGender(Game1.player.Gender))
                {
                    continue;
                }
                var obj = ItemRegistry.Create(outfitPart.ItemId);
                if (includeHat && obj is Hat newHat && !hasEquippedHat)
                {
                    Game1.player.Equip(newHat, Game1.player.hat);
                    hasEquippedHat = true;
                    continue;
                }

                if (obj is Clothing newItem1)
                {
                    switch (newItem1.clothesType.Value)
                    {
                        case Clothing.ClothesType.SHIRT:
                            if (includeShirt && !hasEquippedShirt)
                            {
                                ColorClothing(outfitPart, newItem1);
                                Game1.player.Equip(newItem1, Game1.player.shirtItem);
                                hasEquippedShirt = true;
                                continue;
                            }

                            continue;
                        case Clothing.ClothesType.PANTS:
                            if (includePants && !hasEquippedPants)
                            {
                                ColorClothing(outfitPart, newItem1);
                                Game1.player.Equip(newItem1, Game1.player.pantsItem);
                                hasEquippedPants = true;
                                continue;
                            }

                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        private static void ColorClothing(MakeoverItem outfitPart, Clothing newItem1)
        {

            var color = Utility.StringToColor(outfitPart.Color);
            if (color.HasValue)
            {
                newItem1.clothesColor.Value = color.Value;
            }
        }

        public void RandomizeFullOutfit()
        {
            RandomizeHair();
            RandomizePants();
            RandomizeShirt();
        }

        public void RandomizePants()
        {
            var pants = DataLoader.Pants(Game1.content);
            var pantKeys = pants.Keys.ToArray();
            var chosenPantKey = pantKeys[Game1.random.Next(pantKeys.Length)];
            var pantsId = $"(P){chosenPantKey}";
            _logger.LogDebug($"Equipping Pants: {pantsId}");
            var chosenPant = ItemRegistry.Create<Clothing>(pantsId);
            Game1.player.Equip(chosenPant, Game1.player.pantsItem);
        }

        public void RandomizeShirt()
        {
            var shirts = DataLoader.Shirts(Game1.content);
            var shirtKeys = shirts.Keys.ToArray();
            var chosenShirtKey = shirtKeys[Game1.random.Next(shirtKeys.Length)];
            var shirtId = $"(S){chosenShirtKey}";
            _logger.LogDebug($"Equipping Shirt: {shirtId}");
            var chosenShirt = ItemRegistry.Create<Clothing>(shirtId);
            Game1.player.Equip(chosenShirt, Game1.player.shirtItem);
        }

        public void RandomizeHair()
        {
            var hairs = DataLoader.HairData(Game1.content);
            var hairKeys = hairs.Keys.ToArray();
            var chosenHairKey = hairKeys[Game1.random.Next(hairKeys.Length)];
            _logger.LogDebug($"Equipping Hair: {chosenHairKey}");
            Game1.player.changeHairStyle(chosenHairKey);
            Game1.player.changeHairColor(new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256)));
        }

        public void RandomizeEyes()
        {
            Game1.player.changeEyeColor(new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256)));
        }

        public void RandomizeGender()
        {
            Game1.player.changeGender(Game1.random.NextBool());
        }

        public void RandomizeHat()
        {
            var hats = DataLoader.Hats(Game1.content);
            var hatKeys = hats.Keys.ToArray();
            var chosenHatKey = hatKeys[Game1.random.Next(hatKeys.Length)];
            var hatId = $"(H){chosenHatKey}";
            _logger.LogDebug($"Equipping Hat: {hatId}");
            var chosenHat = ItemRegistry.Create<Hat>(hatId);
            Game1.player.Equip(chosenHat, Game1.player.hat);
        }
    }
}
