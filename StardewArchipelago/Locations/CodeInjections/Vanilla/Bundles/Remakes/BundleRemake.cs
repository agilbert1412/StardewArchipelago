#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
// ReSharper disable ParameterHidesMember

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class BundleRemake : ClickableComponent
    {
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the internal name.</summary>
        public const int NAME_INDEX = 0;
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the reward data.</summary>
        public const int REWARD_INDEX = 1;
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the items needed to complete the bundle.</summary>
        public const int INGREDIENTS_INDEX = 2;
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the bundle color.</summary>
        public const int COLOR_INDEX = 3;
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the optional number of slots to fill.</summary>
        public const int NUMBER_OF_SLOTS_INDEX = 4;
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the optional override texture name and sprite index.</summary>
        public const int SPRITE_INDEX = 5;
        /// <summary>The index in the raw <c>Data/Bundles</c> data for the display name.</summary>
        public const int DISPLAY_NAME_INDEX = 6;
        /// <summary>The number of slash-delimited fields in the raw <c>Data/Bundles</c> data.</summary>
        public const int FIELD_COUNT = 7;
        public const float SHAKE_RATE = 0.0157079641f;
        public const float SHAKE_DECAY_RATE = 0.00306796166f;
        public const int COLOR_GREEN = 0;
        public const int COLOR_PURPLE = 1;
        public const int COLOR_ORANGE = 2;
        public const int COLOR_YELLOW = 3;
        public const int COLOR_RED = 4;
        public const int COLOR_BLUE = 5;
        public const int COLOR_TEAL = 6;
        public const float DEFAULT_SHAKE_FORCE = 0.07363108f;
        public string RewardDescription;
        public List<BundleIngredientDescription> Ingredients;
        public int BundleColor;
        public int NumberOfIngredientSlots;
        public int BundleIndex;
        public int CompletionTimer;
        public bool Complete;
        public bool DepositsAllowed = true;
        public Texture2D BundleTextureOverride;
        public int BundleTextureIndexOverride = -1;
        public TemporaryAnimatedSprite Sprite;
        private float _maxShake;
        private bool _shakeLeft;

        public BundleRemake(
            string name,
            string displayName,
            List<BundleIngredientDescription> ingredients,
            bool[] completedIngredientsList,
            string rewardListString = "")
            : base(new Rectangle(0, 0, 64, 64), "")
        {
            this.name = name;
            this.label = displayName;
            this.RewardDescription = rewardListString;
            this.NumberOfIngredientSlots = ingredients.Count;
            this.Ingredients = ingredients;
        }

        public BundleRemake(
            int bundleIndex,
            string rawBundleInfo,
            bool[] completedIngredientsList,
            Point position,
            string textureName,
            JunimoNoteMenuRemake menu)
            : base(new Rectangle(position.X, position.Y, 64, 64), "")
        {
            if (menu != null && menu.FromGameMenu)
            {
                this.DepositsAllowed = false;
            }
            this.BundleIndex = bundleIndex;
            var array = rawBundleInfo.Split('/');
            this.name = array[0];
            this.label = array[6];
            this.RewardDescription = array[1];
            if (!string.IsNullOrWhiteSpace(array[5]))
            {
                try
                {
                    var strArray = array[5].Split(':', 2);
                    if (strArray.Length == 2)
                    {
                        this.BundleTextureOverride = Game1.content.Load<Texture2D>(strArray[0]);
                        this.BundleTextureIndexOverride = int.Parse(strArray[1]);
                    }
                    else
                    {
                        this.BundleTextureIndexOverride = int.Parse(array[5]);
                    }
                }
                catch
                {
                    this.BundleTextureOverride = null;
                    this.BundleTextureIndexOverride = -1;
                }
            }
            var strArray1 = ArgUtility.SplitBySpace(array[2]);
            this.Complete = true;
            this.Ingredients = new List<BundleIngredientDescription>();
            var num = 0;
            for (var index = 0; index < strArray1.Length; index += 3)
            {
                this.Ingredients.Add(new BundleIngredientDescription(strArray1[index], Convert.ToInt32(strArray1[index + 1]), Convert.ToInt32(strArray1[index + 2]), completedIngredientsList[index / 3]));
                if (!completedIngredientsList[index / 3])
                {
                    this.Complete = false;
                }
                else
                {
                    ++num;
                }
            }
            this.BundleColor = Convert.ToInt32(array[3]);
            this.NumberOfIngredientSlots = ArgUtility.GetInt(array, 4, this.Ingredients.Count);
            if (num >= this.NumberOfIngredientSlots)
            {
                this.Complete = true;
            }
            this.Sprite = new TemporaryAnimatedSprite(textureName, new Rectangle(this.BundleColor * 256 % 512, 244 + this.BundleColor * 256 / 512 * 16, 16, 16), 70f, 3, 99999, new Vector2(this.bounds.X, this.bounds.Y), false,
                false, 0.8f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f)
            {
                pingPong = true,
            };
            this.Sprite.paused = true;
            this.Sprite.sourceRect.X += this.Sprite.sourceRect.Width;
            if (this.name.ContainsIgnoreCase(Game1.currentSeason) && !this.Complete)
            {
                this.Shake();
            }
            if (!this.Complete)
            {
                return;
            }
            this.CompletionAnimation(menu, false);
        }

        public Item GetReward()
        {
            return Utility.getItemFromStandardTextDescription(this.RewardDescription, Game1.player);
        }

        public void Shake(float force = 0.07363108f)
        {
            if (!this.Sprite.paused)
            {
                return;
            }
            this._maxShake = force;
        }

        public void Shake(int extraInfo)
        {
            this._maxShake = 3f * (float)Math.PI / 128f;
            if (extraInfo != 1)
            {
                return;
            }
            Game1.playSound("leafrustle");
            var temporaryAnimatedSprite1 = new TemporaryAnimatedSprite(50, this.Sprite.position, BundleRemake.GetColorFromColorIndex(this.BundleColor))
            {
                motion = new Vector2(-1f, 0.5f),
                acceleration = new Vector2(0.0f, 0.02f),
            };
            ++temporaryAnimatedSprite1.sourceRect.Y;
            --temporaryAnimatedSprite1.sourceRect.Height;
            JunimoNoteMenuRemake.TempSprites.Add(temporaryAnimatedSprite1);
            var temporaryAnimatedSprite2 = new TemporaryAnimatedSprite(50, this.Sprite.position, BundleRemake.GetColorFromColorIndex(this.BundleColor))
            {
                motion = new Vector2(1f, 0.5f),
                acceleration = new Vector2(0.0f, 0.02f),
                flipped = true,
                delayBeforeAnimationStart = 50,
            };
            ++temporaryAnimatedSprite2.sourceRect.Y;
            --temporaryAnimatedSprite2.sourceRect.Height;
            JunimoNoteMenuRemake.TempSprites.Add(temporaryAnimatedSprite2);
        }

        public void TryHoverAction(int x, int y)
        {
            if (this.bounds.Contains(x, y) && !this.Complete)
            {
                this.Sprite.paused = false;
                JunimoNoteMenuRemake.HoverText = Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.label);
            }
            else
            {
                if (this.Complete)
                {
                    return;
                }
                this.Sprite.reset();
                this.Sprite.sourceRect.X += this.Sprite.sourceRect.Width;
                this.Sprite.paused = true;
            }
        }

        public bool IsValidItemForThisIngredientDescription(
            Item item,
            BundleIngredientDescription ingredient)
        {
            if (item == null || ingredient.completed || ingredient.quality > item.Quality)
            {
                return false;
            }
            if (ingredient.preservesId != null)
            {
                var context = new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random, "query 'FLAVORED_ITEM'");
                return ItemQueryResolver.TryResolve("FLAVORED_ITEM " + ingredient.id + " " + ingredient.preservesId, context).FirstOrDefault<ItemQueryResult>()?.Item is StardewValley.Object object1 &&
                       item is StardewValley.Object object2 && object2.preservedParentSheetIndex.Value != null && item.QualifiedItemId == object1.QualifiedItemId && object2.preservedParentSheetIndex.Value.Contains(ingredient.preservesId);
            }
            if (!ingredient.category.HasValue)
            {
                return ItemRegistry.HasItemId(item, ingredient.id);
            }
            if (item.QualifiedItemId == "(O)107" && ingredient.category.GetValueOrDefault() == -5)
            {
                return true;
            }
            var category1 = item.Category;
            var category2 = ingredient.category;
            var valueOrDefault = category2.GetValueOrDefault();
            return category1 == valueOrDefault & category2.HasValue;
        }

        public int GetBundleIngredientDescriptionIndexForItem(Item item)
        {
            for (var index = 0; index < this.Ingredients.Count; ++index)
            {
                if (this.IsValidItemForThisIngredientDescription(item, this.Ingredients[index]))
                {
                    return index;
                }
            }
            return -1;
        }

        public bool CanAcceptThisItem(Item item, ClickableTextureComponent slot)
        {
            return this.CanAcceptThisItem(item, slot, false);
        }

        public bool CanAcceptThisItem(
            Item item,
            ClickableTextureComponent slot,
            bool ignoreStackCount)
        {
            if (!this.DepositsAllowed)
            {
                return false;
            }
            for (var index = 0; index < this.Ingredients.Count; ++index)
            {
                if (this.IsValidItemForThisIngredientDescription(item, this.Ingredients[index]) && (ignoreStackCount || this.Ingredients[index].stack <= item.Stack) && slot?.item == null)
                {
                    return true;
                }
            }
            return false;
        }

        public Item TryToDepositThisItem(
            Item item,
            ClickableTextureComponent slot,
            string noteTextureName,
            JunimoNoteMenuRemake parentMenu)
        {
            if (!this.DepositsAllowed)
            {
                if (Game1.player.hasCompletedCommunityCenter())
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtAJM"));
                }
                else
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtCC"));
                }
                return item;
            }
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            for (var index1 = 0; index1 < this.Ingredients.Count; ++index1)
            {
                var ingredientDescription1 = this.Ingredients[index1];
                if (this.IsValidItemForThisIngredientDescription(item, ingredientDescription1) && slot.item == null)
                {
                    item = item.ConsumeStack(ingredientDescription1.stack);
                    var ingredients = this.Ingredients;
                    var index2 = index1;
                    ingredientDescription1 = new BundleIngredientDescription(ingredientDescription1, true);
                    var ingredientDescription2 = ingredientDescription1;
                    ingredients[index2] = ingredientDescription2;
                    this.IngredientDepositAnimation(slot, noteTextureName);
                    var representativeItemId = JunimoNoteMenuRemake.GetRepresentativeItemId(ingredientDescription1);
                    if (ingredientDescription1.preservesId != null)
                    {
                        slot.item = Utility.CreateFlavoredItem(ingredientDescription1.id, ingredientDescription1.preservesId, ingredientDescription1.quality, ingredientDescription1.stack);
                    }
                    else
                    {
                        slot.item = ItemRegistry.Create(representativeItemId, ingredientDescription1.stack, ingredientDescription1.quality);
                    }
                    Game1.playSound("newArtifact");
                    slot.sourceRect.X = 512;
                    slot.sourceRect.Y = 244;
                    if (parentMenu.OnIngredientDeposit != null)
                    {
                        parentMenu.OnIngredientDeposit(index1);
                        break;
                    }
                    communityCenter.bundles.FieldDict[this.BundleIndex][index1] = true;
                    Game1.Multiplayer.globalChatInfoMessage("BundleDonate", Game1.player.displayName, TokenStringBuilder.ItemNameFor(slot.item));
                    break;
                }
            }
            return item;
        }

        public void IngredientDepositAnimation(
            ClickableTextureComponent slot,
            string noteTextureName,
            bool skipAnimation = false)
        {
            var temporaryAnimatedSprite = new TemporaryAnimatedSprite(noteTextureName, new Rectangle(530, 244, 18, 18), 50f, 6, 1, new Vector2(slot.bounds.X, slot.bounds.Y), false, false, 0.88f, 0.0f, Color.White, 4f, 0.0f,
                0.0f, 0.0f, true)
            {
                holdLastFrame = true,
                endSound = "cowboy_monsterhit",
            };
            if (skipAnimation)
            {
                temporaryAnimatedSprite.sourceRect.Offset(temporaryAnimatedSprite.sourceRect.Width * 5, 0);
                temporaryAnimatedSprite.sourceRectStartingPos = new Vector2(temporaryAnimatedSprite.sourceRect.X, temporaryAnimatedSprite.sourceRect.Y);
                temporaryAnimatedSprite.animationLength = 1;
            }
            JunimoNoteMenuRemake.TempSprites.Add(temporaryAnimatedSprite);
        }

        public bool CanBeClicked() => !this.Complete;

        public void CompletionAnimation(JunimoNoteMenuRemake menu, bool playSound = true, int delay = 0)
        {
            if (delay <= 0)
            {
                this.CompletionAnimation(playSound);
            }
            else
            {
                this.CompletionTimer = delay;
            }
        }

        private void CompletionAnimation(bool playSound = true)
        {
            if (Game1.activeClickableMenu is JunimoNoteMenuRemake activeClickableMenu)
            {
                activeClickableMenu.TakeDownBundleSpecificPage();
            }
            this.Sprite.pingPong = false;
            this.Sprite.paused = false;
            this.Sprite.sourceRect.X = (int)this.Sprite.sourceRectStartingPos.X;
            this.Sprite.sourceRect.X += this.Sprite.sourceRect.Width;
            this.Sprite.animationLength = 15;
            this.Sprite.interval = 50f;
            this.Sprite.totalNumberOfLoops = 0;
            this.Sprite.holdLastFrame = true;
            this.Sprite.endFunction = this.Shake;
            this.Sprite.extraInfoForEndBehavior = 1;
            if (this.Complete)
            {
                this.Sprite.sourceRect.X += this.Sprite.sourceRect.Width * 14;
                this.Sprite.sourceRectStartingPos = new Vector2(this.Sprite.sourceRect.X, this.Sprite.sourceRect.Y);
                this.Sprite.currentParentTileIndex = 14;
                this.Sprite.interval = 0.0f;
                this.Sprite.animationLength = 1;
                this.Sprite.extraInfoForEndBehavior = 0;
            }
            else
            {
                if (playSound)
                {
                    Game1.playSound("dwop");
                }
                this.bounds.Inflate(64, 64);
                JunimoNoteMenuRemake.TempSprites.AddRange(Utility.sparkleWithinArea(this.bounds, 8, BundleRemake.GetColorFromColorIndex(this.BundleColor) * 0.5f));
                this.bounds.Inflate(-64, -64);
            }
            this.Complete = true;
        }

        public void Update(GameTime time)
        {
            this.Sprite.update(time);
            if (this.CompletionTimer > 0 && JunimoNoteMenuRemake.ScreenSwipe == null)
            {
                this.CompletionTimer -= time.ElapsedGameTime.Milliseconds;
                if (this.CompletionTimer <= 0)
                {
                    this.CompletionAnimation();
                }
            }
            if (Game1.random.NextDouble() < 0.005 && (this.Complete || this.name.ContainsIgnoreCase(Game1.currentSeason)))
            {
                this.Shake();
            }
            if (this._maxShake > 0.0)
            {
                if (this._shakeLeft)
                {
                    this.Sprite.rotation -= (float)Math.PI / 200f;
                    if (this.Sprite.rotation <= -(double)this._maxShake)
                    {
                        this._shakeLeft = false;
                    }
                }
                else
                {
                    this.Sprite.rotation += (float)Math.PI / 200f;
                    if (this.Sprite.rotation >= (double)this._maxShake)
                    {
                        this._shakeLeft = true;
                    }
                }
            }
            if (this._maxShake <= 0.0)
            {
                return;
            }
            this._maxShake = Math.Max(0.0f, this._maxShake - 0.0007669904f);
        }

        public void Draw(SpriteBatch b) => this.Sprite.draw(b, true);

        public static Color GetColorFromColorIndex(int color)
        {
            switch (color)
            {
                case 0:
                    return Color.Lime;
                case 1:
                    return Color.DeepPink;
                case 2:
                    return Color.Orange;
                case 3:
                    return Color.Orange;
                case 4:
                    return Color.Red;
                case 5:
                    return Color.LightBlue;
                case 6:
                    return Color.Cyan;
                default:
                    return Color.Lime;
            }
        }
    }
}
