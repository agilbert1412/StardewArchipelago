using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class JunimoNoteMenuRemake : IClickableMenu
    {
        public const int region_ingredientSlotModifier = 250;
        public const int region_ingredientListModifier = 1000;
        public const int region_bundleModifier = 5000;
        public const int region_areaNextButton = 101;
        public const int region_areaBackButton = 102;
        public const int region_backButton = 103;
        public const int region_purchaseButton = 104;
        public const int region_presentButton = 105;
        public const string noteTextureName = "LooseSprites\\JunimoNote";
        public Texture2D noteTexture;
        public bool specificBundlePage;
        public const int baseWidth = 320;
        public const int baseHeight = 180;
        public InventoryMenu inventory;
        public Item partialDonationItem;
        public List<Item> partialDonationComponents = new List<Item>();
        public BundleIngredientDescription? currentPartialIngredientDescription;
        public int currentPartialIngredientDescriptionIndex = -1;
        public Item heldItem;
        public Item hoveredItem;
        public static bool canClick = true;
        public int whichArea;
        public int gameMenuTabToReturnTo = -1;
        public IClickableMenu menuToReturnTo;
        public bool bundlesChanged;
        public static ScreenSwipe screenSwipe;
        public static string hoverText = "";
        public List<BundleRemake> bundles = new List<BundleRemake>();
        public static TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();
        public List<ClickableTextureComponent> ingredientSlots = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> ingredientList = new List<ClickableTextureComponent>();
        public bool fromGameMenu;
        public bool fromThisMenu;
        public bool scrambledText;
        private bool singleBundleMenu;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent purchaseButton;
        public ClickableTextureComponent areaNextButton;
        public ClickableTextureComponent areaBackButton;
        public ClickableAnimatedComponent presentButton;
        public Action<int> onIngredientDeposit;
        public Action<JunimoNoteMenuRemake> onBundleComplete;
        public Action<JunimoNoteMenuRemake> onScreenSwipeFinished;
        public BundleRemake CurrentPageBundle;
        private int oldTriggerSpot;

        public JunimoNoteMenuRemake(bool fromGameMenu, int area = 1, bool fromThisMenu = false)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            if (fromGameMenu && !fromThisMenu)
            {
                for (var index = 0; index < communityCenter.areasComplete.Count; ++index)
                {
                    if (communityCenter.shouldNoteAppearInArea(index) && !communityCenter.areasComplete[index])
                    {
                        area = index;
                        whichArea = area;
                        break;
                    }
                }
                if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
                {
                    area = 6;
                }
            }
            setUpMenu(area, communityCenter.bundlesDict());
            Game1.player.forceCanMove();
            var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
            textureComponent1.visible = false;
            textureComponent1.myID = 101;
            textureComponent1.leftNeighborID = 102;
            textureComponent1.leftNeighborImmutable = true;
            textureComponent1.downNeighborID = -99998;
            areaNextButton = textureComponent1;
            var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
            textureComponent2.visible = false;
            textureComponent2.myID = 102;
            textureComponent2.rightNeighborID = 101;
            textureComponent2.rightNeighborImmutable = true;
            textureComponent2.downNeighborID = -99998;
            areaBackButton = textureComponent2;
            var num = 6;
            for (var area1 = 0; area1 < num; ++area1)
            {
                if (area1 != area && communityCenter.shouldNoteAppearInArea(area1))
                {
                    areaNextButton.visible = true;
                    areaBackButton.visible = true;
                    break;
                }
            }
            this.fromGameMenu = fromGameMenu;
            this.fromThisMenu = fromThisMenu;
            foreach (var bundle in bundles)
            {
                bundle.DepositsAllowed = false;
            }
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public JunimoNoteMenuRemake(int whichArea, Dictionary<int, bool[]> bundlesComplete)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            setUpMenu(whichArea, bundlesComplete);
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public JunimoNoteMenuRemake(BundleRemake b, string noteTexturePath)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            singleBundleMenu = true;
            whichArea = -1;
            noteTexture = Game1.temporaryContent.Load<Texture2D>(noteTexturePath);
            tempSprites.Clear();
            inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: 36, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false)
            {
                capacity = 36
            };
            for (var index = 0; index < inventory.inventory.Count; ++index)
            {
                if (index >= inventory.actualInventory.Count)
                {
                    inventory.inventory[index].visible = false;
                }
            }
            foreach (var clickableComponent in inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
            {
                clickableComponent.downNeighborID = -99998;
            }
            foreach (var clickableComponent in inventory.GetBorder(InventoryMenu.BorderSide.Right))
            {
                clickableComponent.rightNeighborID = -99998;
            }
            inventory.dropItemInvisibleButton.visible = false;
            canClick = true;
            setUpBundleSpecificPage(b);
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            if (specificBundlePage)
            {
                currentlySnappedComponent = getComponentWithID(0);
            }
            else
            {
                currentlySnappedComponent = getComponentWithID(5000);
            }
            snapCursorToCurrentSnappedComponent();
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements() => !specificBundlePage;

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || oldID - 5000 < 0 || oldID - 5000 >= 10 || currentlySnappedComponent == null)
            {
                return;
            }
            var num1 = -1;
            var num2 = 999999;
            var center1 = currentlySnappedComponent.bounds.Center;
            for (var index = 0; index < bundles.Count; ++index)
            {
                if (bundles[index].myID != oldID)
                {
                    var num3 = 999999;
                    var center2 = bundles[index].bounds.Center;
                    switch (direction)
                    {
                        case 0:
                            if (center2.Y < center1.Y)
                            {
                                num3 = center1.Y - center2.Y + Math.Abs(center1.X - center2.X) * 3;
                                break;
                            }
                            break;
                        case 1:
                            if (center2.X > center1.X)
                            {
                                num3 = center2.X - center1.X + Math.Abs(center1.Y - center2.Y) * 3;
                                break;
                            }
                            break;
                        case 2:
                            if (center2.Y > center1.Y)
                            {
                                num3 = center2.Y - center1.Y + Math.Abs(center1.X - center2.X) * 3;
                                break;
                            }
                            break;
                        case 3:
                            if (center2.X < center1.X)
                            {
                                num3 = center1.X - center2.X + Math.Abs(center1.Y - center2.Y) * 3;
                                break;
                            }
                            break;
                    }
                    if (num3 < 10000 && num3 < num2)
                    {
                        num2 = num3;
                        num1 = index;
                    }
                }
            }
            if (num1 != -1)
            {
                currentlySnappedComponent = getComponentWithID(num1 + 5000);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (areaNextButton == null || !areaNextButton.visible)
                        {
                            break;
                        }
                        currentlySnappedComponent = areaNextButton;
                        snapCursorToCurrentSnappedComponent();
                        areaNextButton.leftNeighborID = oldID;
                        break;
                    case 2:
                        if (presentButton == null)
                        {
                            break;
                        }
                        currentlySnappedComponent = presentButton;
                        snapCursorToCurrentSnappedComponent();
                        presentButton.upNeighborID = oldID;
                        break;
                    case 3:
                        if (areaBackButton == null || !areaBackButton.visible)
                        {
                            break;
                        }
                        currentlySnappedComponent = areaBackButton;
                        snapCursorToCurrentSnappedComponent();
                        areaBackButton.rightNeighborID = oldID;
                        break;
                }
            }
        }

        public virtual void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            noteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
            if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
            {
                Game1.player.removeQuest("26");
                Game1.player.mailReceived.Add("seenJunimoNote");
            }
            if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
            {
                Game1.addMailForTomorrow("wizardJunimoNote");
            }
            if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
            {
                Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
            }
            scrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
            tempSprites.Clear();
            this.whichArea = whichArea;
            inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: 36, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false)
            {
                capacity = 36
            };
            for (var index = 0; index < inventory.inventory.Count; ++index)
            {
                if (index >= inventory.actualInventory.Count)
                {
                    inventory.inventory[index].visible = false;
                }
            }
            foreach (var clickableComponent in inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
            {
                clickableComponent.downNeighborID = -99998;
            }
            foreach (var clickableComponent in inventory.GetBorder(InventoryMenu.BorderSide.Right))
            {
                clickableComponent.rightNeighborID = -99998;
            }
            inventory.dropItemInvisibleButton.visible = false;
            var bundleData = Game1.netWorldState.Value.BundleData;
            var areaNameFromNumber = CommunityCenter.getAreaNameFromNumber(whichArea);
            var whichBundle = 0;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(areaNameFromNumber))
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    var bundles = this.bundles;
                    var bundle = new BundleRemake(int32, bundleData[key], bundlesComplete[int32], getBundleLocationFromNumber(whichBundle), "LooseSprites\\JunimoNote", this);
                    bundle.myID = whichBundle + 5000;
                    bundle.rightNeighborID = -7777;
                    bundle.leftNeighborID = -7777;
                    bundle.upNeighborID = -7777;
                    bundle.downNeighborID = -7777;
                    bundle.fullyImmutable = true;
                    bundles.Add(bundle);
                    ++whichBundle;
                }
            }
            var textureComponent = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
            textureComponent.myID = 103;
            backButton = textureComponent;
            CheckForRewards();
            canClick = true;
            Game1.playSound("shwip");
            var flag = false;
            foreach (var bundle in bundles)
            {
                if (!bundle.Complete && !bundle.Equals(CurrentPageBundle))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                return;
            }
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            communityCenter.markAreaAsComplete(whichArea);
            exitFunction = restoreAreaOnExit;
            communityCenter.areaCompleteReward(whichArea);
        }

        public virtual bool HighlightObjects(Item item)
        {
            if (CurrentPageBundle != null)
            {
                if (partialDonationItem != null && currentPartialIngredientDescriptionIndex >= 0)
                {
                    return CurrentPageBundle.IsValidItemForThisIngredientDescription(item, CurrentPageBundle.Ingredients[currentPartialIngredientDescriptionIndex]);
                }
                foreach (var ingredient in CurrentPageBundle.Ingredients)
                {
                    if (CurrentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool readyToClose()
        {
            return (!specificBundlePage || singleBundleMenu) && IsReadyToCloseMenuOrBundle();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!canClick)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            if (scrambledText)
            {
                return;
            }
            if (specificBundlePage)
            {
                if (!CurrentPageBundle.Complete && CurrentPageBundle.CompletionTimer <= 0)
                {
                    heldItem = inventory.leftClick(x, y, heldItem);
                }
                if (backButton != null && backButton.containsPoint(x, y) && heldItem == null)
                {
                    closeBundlePage();
                }
                if (partialDonationItem != null)
                {
                    if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                    {
                        for (var index = 0; index < ingredientSlots.Count; ++index)
                        {
                            if (ingredientSlots[index].item == partialDonationItem)
                            {
                                HandlePartialDonation(heldItem, ingredientSlots[index]);
                            }
                        }
                    }
                    else
                    {
                        for (var index = 0; index < ingredientSlots.Count; ++index)
                        {
                            if (ingredientSlots[index].containsPoint(x, y) && ingredientSlots[index].item == partialDonationItem)
                            {
                                if (heldItem != null)
                                {
                                    HandlePartialDonation(heldItem, ingredientSlots[index]);
                                    return;
                                }
                                ReturnPartialDonations(!Game1.oldKBState.IsKeyDown(Keys.LeftShift));
                                return;
                            }
                        }
                    }
                }
                else if (heldItem != null)
                {
                    if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                    {
                        for (var index = 0; index < ingredientSlots.Count; ++index)
                        {
                            if (CurrentPageBundle.CanAcceptThisItem(heldItem, ingredientSlots[index]))
                            {
                                if (ingredientSlots[index].item == null)
                                {
                                    heldItem = CurrentPageBundle.TryToDepositThisItem(heldItem, ingredientSlots[index], "LooseSprites\\JunimoNote", this);
                                    checkIfBundleIsComplete();
                                    return;
                                }
                            }
                            else if (ingredientSlots[index].item == null)
                            {
                                HandlePartialDonation(heldItem, ingredientSlots[index]);
                            }
                        }
                    }
                    for (var index = 0; index < ingredientSlots.Count; ++index)
                    {
                        if (ingredientSlots[index].containsPoint(x, y))
                        {
                            if (CurrentPageBundle.CanAcceptThisItem(heldItem, ingredientSlots[index]))
                            {
                                heldItem = CurrentPageBundle.TryToDepositThisItem(heldItem, ingredientSlots[index], "LooseSprites\\JunimoNote", this);
                                checkIfBundleIsComplete();
                            }
                            else if (ingredientSlots[index].item == null)
                            {
                                HandlePartialDonation(heldItem, ingredientSlots[index]);
                            }
                        }
                    }
                }
                if (purchaseButton != null && purchaseButton.containsPoint(x, y))
                {
                    var stack = CurrentPageBundle.Ingredients.Last().stack;
                    if (Game1.player.Money >= stack)
                    {
                        Game1.player.Money -= stack;
                        Game1.playSound("select");
                        CurrentPageBundle.CompletionAnimation(this);
                        if (purchaseButton != null)
                        {
                            purchaseButton.scale = purchaseButton.baseScale * 0.75f;
                        }
                        var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
                        communityCenter.bundleRewards[CurrentPageBundle.BundleIndex] = true;
                        communityCenter.bundles.FieldDict[CurrentPageBundle.BundleIndex][0] = true;
                        CheckForRewards();
                        var flag = false;
                        foreach (var bundle in bundles)
                        {
                            if (!bundle.Complete && !bundle.Equals(CurrentPageBundle))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            communityCenter.markAreaAsComplete(whichArea);
                            exitFunction = restoreAreaOnExit;
                            communityCenter.areaCompleteReward(whichArea);
                        }
                        else
                        {
                            communityCenter.getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(CurrentPageBundle.BundleColor), Game1.RequireLocation("CommunityCenter"));
                        }
                        Game1.Multiplayer.globalChatInfoMessage("Bundle");
                    }
                    else
                    {
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                    }
                }
                if (upperRightCloseButton != null && IsReadyToCloseMenuOrBundle() && upperRightCloseButton.containsPoint(x, y))
                {
                    closeBundlePage();
                    return;
                }
            }
            else
            {
                foreach (var bundle in bundles)
                {
                    if (bundle.CanBeClicked() && bundle.containsPoint(x, y))
                    {
                        setUpBundleSpecificPage(bundle);
                        Game1.playSound("shwip");
                        return;
                    }
                }
                if (presentButton != null && presentButton.containsPoint(x, y) && !fromGameMenu && !fromThisMenu)
                {
                    openRewardsMenu();
                }
                if (fromGameMenu)
                {
                    if (areaNextButton.containsPoint(x, y))
                    {
                        SwapPage(1);
                    }
                    else if (areaBackButton.containsPoint(x, y))
                    {
                        SwapPage(-1);
                    }
                }
            }
            if (heldItem == null || isWithinBounds(x, y) || !heldItem.canBeTrashed())
            {
                return;
            }
            Game1.playSound("throwDownITem");
            Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
            heldItem = null;
        }

        public virtual void ReturnPartialDonation(Item item, bool play_sound = true)
        {
            var affected_items_list = new List<Item>();
            var inventory = Game1.player.addItemToInventory(item, affected_items_list);
            foreach (var obj in affected_items_list)
            {
                this.inventory.ShakeItem(obj);
            }
            if (inventory != null)
            {
                Utility.CollectOrDrop(inventory);
                this.inventory.ShakeItem(inventory);
            }
            if (!play_sound)
            {
                return;
            }
            Game1.playSound("coin");
        }

        public virtual void ReturnPartialDonations(bool to_hand = true)
        {
            if (partialDonationComponents.Count > 0)
            {
                var play_sound = true;
                foreach (var donationComponent in partialDonationComponents)
                {
                    if (heldItem == null & to_hand)
                    {
                        Game1.playSound("dwop");
                        heldItem = donationComponent;
                    }
                    else
                    {
                        ReturnPartialDonation(donationComponent, play_sound);
                        play_sound = false;
                    }
                }
            }
            ResetPartialDonation();
        }

        public virtual void ResetPartialDonation()
        {
            partialDonationComponents.Clear();
            currentPartialIngredientDescription = new BundleIngredientDescription?();
            currentPartialIngredientDescriptionIndex = -1;
            foreach (var ingredientSlot in ingredientSlots)
            {
                if (ingredientSlot.item == partialDonationItem)
                {
                    ingredientSlot.item = null;
                }
            }
            partialDonationItem = null;
        }

        public virtual bool CanBePartiallyOrFullyDonated(Item item)
        {
            if (CurrentPageBundle == null)
            {
                return false;
            }
            var descriptionIndexForItem = CurrentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
            if (descriptionIndexForItem < 0)
            {
                return false;
            }
            var ingredient = CurrentPageBundle.Ingredients[descriptionIndexForItem];
            var num = 0;
            if (CurrentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
            {
                num += item.Stack;
            }
            foreach (var obj in Game1.player.Items)
            {
                if (CurrentPageBundle.IsValidItemForThisIngredientDescription(obj, ingredient))
                {
                    num += obj.Stack;
                }
            }
            if (descriptionIndexForItem == currentPartialIngredientDescriptionIndex && partialDonationItem != null)
            {
                num += partialDonationItem.Stack;
            }
            return num >= ingredient.stack;
        }

        public virtual void HandlePartialDonation(Item item, ClickableTextureComponent slot)
        {
            if (CurrentPageBundle != null && !CurrentPageBundle.DepositsAllowed || partialDonationItem != null && slot.item != partialDonationItem || !CanBePartiallyOrFullyDonated(item))
            {
                return;
            }
            if (!currentPartialIngredientDescription.HasValue)
            {
                currentPartialIngredientDescriptionIndex = CurrentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
                if (currentPartialIngredientDescriptionIndex != -1)
                {
                    currentPartialIngredientDescription = CurrentPageBundle.Ingredients[currentPartialIngredientDescriptionIndex];
                }
            }
            if (!currentPartialIngredientDescription.HasValue || !CurrentPageBundle.IsValidItemForThisIngredientDescription(item, currentPartialIngredientDescription.Value))
            {
                return;
            }
            var flag1 = true;
            var flag2 = item == heldItem;
            int amount;
            if (slot.item == null)
            {
                Game1.playSound("sell");
                flag1 = false;
                partialDonationItem = item.getOne();
                amount = Math.Min(currentPartialIngredientDescription.Value.stack, item.Stack);
                partialDonationItem.Stack = amount;
                item = item.ConsumeStack(amount);
                partialDonationItem.Quality = currentPartialIngredientDescription.Value.quality;
                slot.item = partialDonationItem;
                slot.sourceRect.X = 512;
                slot.sourceRect.Y = 244;
            }
            else
            {
                amount = Math.Min(currentPartialIngredientDescription.Value.stack - partialDonationItem.Stack, item.Stack);
                partialDonationItem.Stack += amount;
                item = item.ConsumeStack(amount);
            }
            if (amount > 0)
            {
                var one = heldItem.getOne();
                one.Stack = amount;
                foreach (var donationComponent in partialDonationComponents)
                {
                    if (donationComponent.canStackWith(heldItem))
                    {
                        one.Stack = donationComponent.addToStack(one);
                    }
                }
                if (one.Stack > 0)
                {
                    partialDonationComponents.Add(one);
                }
                partialDonationComponents.Sort((a, b) => b.Stack.CompareTo(a.Stack));
            }
            if (flag2 && item == null)
            {
                heldItem = null;
            }
            if (partialDonationItem.Stack >= currentPartialIngredientDescription.Value.stack)
            {
                slot.item = null;
                this.partialDonationItem = CurrentPageBundle.TryToDepositThisItem(this.partialDonationItem, slot, "LooseSprites\\JunimoNote", this);
                var partialDonationItem = this.partialDonationItem;
                if ((partialDonationItem != null ? partialDonationItem.Stack > 0 ? 1 : 0 : 0) != 0)
                {
                    ReturnPartialDonation(this.partialDonationItem);
                }
                this.partialDonationItem = null;
                ResetPartialDonation();
                checkIfBundleIsComplete();
            }
            else
            {
                if (amount <= 0 || !flag1)
                {
                    return;
                }
                Game1.playSound("sell");
            }
        }

        public bool IsReadyToCloseMenuOrBundle()
        {
            if (specificBundlePage)
            {
                var currentPageBundle = this.CurrentPageBundle;
                if ((currentPageBundle != null ? currentPageBundle.CompletionTimer > 0 ? 1 : 0 : 0) != 0)
                {
                    return false;
                }
            }
            return heldItem == null;
        }

        /// <inheritdoc />
        public override void receiveGamePadButton(Buttons button)
        {
            base.receiveGamePadButton(button);
            if (specificBundlePage)
            {
                switch (button)
                {
                    case Buttons.RightTrigger:
                        var snappedComponent1 = currentlySnappedComponent;
                        if ((snappedComponent1 != null ? snappedComponent1.myID < 50 ? 1 : 0 : 0) == 0)
                        {
                            break;
                        }
                        oldTriggerSpot = currentlySnappedComponent.myID;
                        var id = 250;
                        foreach (var ingredientSlot in ingredientSlots)
                        {
                            if (ingredientSlot.item == null)
                            {
                                id = ingredientSlot.myID;
                                break;
                            }
                        }
                        setCurrentlySnappedComponentTo(id);
                        snapCursorToCurrentSnappedComponent();
                        break;
                    case Buttons.LeftTrigger:
                        var snappedComponent2 = currentlySnappedComponent;
                        if ((snappedComponent2 != null ? snappedComponent2.myID >= 250 ? 1 : 0 : 0) == 0)
                        {
                            break;
                        }
                        setCurrentlySnappedComponentTo(oldTriggerSpot);
                        snapCursorToCurrentSnappedComponent();
                        break;
                }
            }
            else
            {
                if (!fromGameMenu)
                {
                    return;
                }
                if (button != Buttons.RightTrigger)
                {
                    if (button != Buttons.LeftTrigger)
                    {
                        return;
                    }
                    SwapPage(-1);
                }
                else
                {
                    SwapPage(1);
                }
            }
        }

        public void SwapPage(int direction)
        {
            if (direction > 0 && !areaNextButton.visible || direction < 0 && !areaBackButton.visible)
            {
                return;
            }
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            var whichArea = this.whichArea;
            var num1 = 6;
            for (var index = 0; index < num1; ++index)
            {
                whichArea += direction;
                if (whichArea < 0)
                {
                    whichArea += num1;
                }
                if (whichArea >= num1)
                {
                    whichArea -= num1;
                }
                if (communityCenter.shouldNoteAppearInArea(whichArea))
                {
                    var num2 = -1;
                    if (currentlySnappedComponent != null && (currentlySnappedComponent.myID >= 5000 || currentlySnappedComponent.myID == 101 || currentlySnappedComponent.myID == 102))
                    {
                        num2 = currentlySnappedComponent.myID;
                    }
                    var junimoNoteMenu = new JunimoNoteMenuRemake(true, whichArea, true)
                    {
                        gameMenuTabToReturnTo = gameMenuTabToReturnTo
                    };
                    Game1.activeClickableMenu = junimoNoteMenu;
                    if (num2 >= 0)
                    {
                        junimoNoteMenu.currentlySnappedComponent = junimoNoteMenu.getComponentWithID(currentlySnappedComponent.myID);
                        junimoNoteMenu.snapCursorToCurrentSnappedComponent();
                    }
                    if (junimoNoteMenu.getComponentWithID(areaNextButton.leftNeighborID) != null)
                    {
                        junimoNoteMenu.areaNextButton.leftNeighborID = areaNextButton.leftNeighborID;
                    }
                    else
                    {
                        junimoNoteMenu.areaNextButton.leftNeighborID = junimoNoteMenu.areaBackButton.myID;
                    }
                    junimoNoteMenu.areaNextButton.rightNeighborID = areaNextButton.rightNeighborID;
                    junimoNoteMenu.areaNextButton.upNeighborID = areaNextButton.upNeighborID;
                    junimoNoteMenu.areaNextButton.downNeighborID = areaNextButton.downNeighborID;
                    if (junimoNoteMenu.getComponentWithID(areaBackButton.rightNeighborID) != null)
                    {
                        junimoNoteMenu.areaBackButton.leftNeighborID = areaBackButton.leftNeighborID;
                    }
                    else
                    {
                        junimoNoteMenu.areaBackButton.leftNeighborID = junimoNoteMenu.areaNextButton.myID;
                    }
                    junimoNoteMenu.areaBackButton.rightNeighborID = areaBackButton.rightNeighborID;
                    junimoNoteMenu.areaBackButton.upNeighborID = areaBackButton.upNeighborID;
                    junimoNoteMenu.areaBackButton.downNeighborID = areaBackButton.downNeighborID;
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            if (gameMenuTabToReturnTo != -1)
            {
                closeSound = "shwip";
            }
            base.receiveKeyPress(key);
            if (key == Keys.Delete && heldItem != null && heldItem.canBeTrashed())
            {
                Utility.trashItem(heldItem);
                heldItem = null;
            }
            if (!Game1.options.doesInputListContain(Game1.options.menuButton, key) || !IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            if (singleBundleMenu)
            {
                exitThisMenu(gameMenuTabToReturnTo == -1);
            }
            closeBundlePage();
        }

        /// <inheritdoc />
        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
            if (gameMenuTabToReturnTo != -1)
            {
                Game1.activeClickableMenu = new GameMenu(gameMenuTabToReturnTo, playOpeningSound: false);
            }
            else
            {
                if (menuToReturnTo == null)
                {
                    return;
                }
                Game1.activeClickableMenu = menuToReturnTo;
            }
        }

        private void closeBundlePage()
        {
            if (partialDonationItem != null)
            {
                ReturnPartialDonations(false);
            }
            else
            {
                if (!specificBundlePage)
                {
                    return;
                }
                hoveredItem = null;
                inventory.descriptionText = "";
                if (heldItem == null)
                {
                    takeDownBundleSpecificPage();
                    Game1.playSound("shwip");
                }
                else
                {
                    heldItem = inventory.tryToAddItem(heldItem);
                }
            }
        }

        protected virtual void reOpenThisMenu()
        {
            var num = specificBundlePage ? 1 : 0;
            var junimoNoteMenu = CreateJunimoNoteMenu();
            if (num != 0)
            {
                foreach (var bundle in junimoNoteMenu.bundles)
                {
                    if (bundle.BundleIndex == CurrentPageBundle.BundleIndex)
                    {
                        junimoNoteMenu.setUpBundleSpecificPage(bundle);
                        break;
                    }
                }
            }
            Game1.activeClickableMenu = junimoNoteMenu;
        }

        protected virtual JunimoNoteMenuRemake CreateJunimoNoteMenu()
        {
            if (fromGameMenu || fromThisMenu)
            {
                return new JunimoNoteMenuRemake(fromGameMenu, whichArea, fromThisMenu)
                {
                    gameMenuTabToReturnTo = gameMenuTabToReturnTo,
                    menuToReturnTo = menuToReturnTo,
                };
            }
            else
            {
                return new JunimoNoteMenuRemake(whichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict())
                {
                    gameMenuTabToReturnTo = gameMenuTabToReturnTo,
                    menuToReturnTo = menuToReturnTo,
                };
            }
        }

        private void updateIngredientSlots()
        {
            var index = 0;
            foreach (var ingredient in CurrentPageBundle.Ingredients)
            {
                if (ingredient.completed && index < ingredientSlots.Count)
                {
                    var representativeItemId = GetRepresentativeItemId(ingredient);
                    if (ingredient.preservesId != null)
                    {
                        ingredientSlots[index].item = Utility.CreateFlavoredItem(representativeItemId, ingredient.preservesId, ingredient.quality, ingredient.stack);
                    }
                    else
                    {
                        ingredientSlots[index].item = ItemRegistry.Create(representativeItemId, ingredient.stack, ingredient.quality);
                    }
                    CurrentPageBundle.IngredientDepositAnimation(ingredientSlots[index], "LooseSprites\\JunimoNote", true);
                    ++index;
                }
            }
        }

        /// <summary>Get the qualified item ID to draw in the bundle UI for an ingredient.</summary>
        /// <param name="ingredient">The ingredient to represent.</param>
        public static string GetRepresentativeItemId(BundleIngredientDescription ingredient)
        {
            if (!ingredient.category.HasValue)
            {
                return ingredient.id;
            }
            foreach (var parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                var category1 = parsedItemData.Category;
                var category2 = ingredient.category;
                var valueOrDefault = category2.GetValueOrDefault();
                if (category1 == valueOrDefault & category2.HasValue)
                {
                    return parsedItemData.QualifiedItemId;
                }
            }
            return "0";
        }

        public static void GetBundleRewards(int area, List<Item> rewards)
        {
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(CommunityCenter.getAreaNameFromNumber(area)))
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    if (communityCenter.bundleRewards[int32])
                    {
                        var standardTextDescription = Utility.getItemFromStandardTextDescription(bundleData[key].Split('/')[1], Game1.player);
                        standardTextDescription.SpecialVariable = int32;
                        rewards.Add(standardTextDescription);
                    }
                }
            }
        }

        private void openRewardsMenu()
        {
            Game1.playSound("smallSelect");
            var objList = new List<Item>();
            GetBundleRewards(whichArea, objList);
            Game1.activeClickableMenu = new ItemGrabMenu(objList, false, true, null, null, null, rewardGrabbed, canBeExitedWithKey: true, context: this);
            Game1.activeClickableMenu.exitFunction = exitFunction != null ? exitFunction : reOpenThisMenu;
        }

        private void rewardGrabbed(Item item, Farmer who)
        {
            Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[item.SpecialVariable] = false;
        }

        private void checkIfBundleIsComplete()
        {
            ReturnPartialDonations();
            if (!specificBundlePage || CurrentPageBundle == null)
            {
                return;
            }
            var num = 0;
            foreach (var ingredientSlot in ingredientSlots)
            {
                if (ingredientSlot.item != null && ingredientSlot.item != partialDonationItem)
                {
                    ++num;
                }
            }
            if (num < CurrentPageBundle.NumberOfIngredientSlots)
            {
                return;
            }
            if (heldItem != null)
            {
                Game1.player.addItemToInventory(heldItem);
                heldItem = null;
            }
            if (!singleBundleMenu)
            {
                var location = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
                for (var index = 0; index < location.bundles[CurrentPageBundle.BundleIndex].Length; ++index)
                    location.bundles.FieldDict[CurrentPageBundle.BundleIndex][index] = true;
                location.checkForNewJunimoNotes();
                screenSwipe = new ScreenSwipe(0, w: width, h: height);
                CurrentPageBundle.CompletionAnimation(this, delay: 400);
                canClick = false;
                location.bundleRewards[CurrentPageBundle.BundleIndex] = true;
                Game1.Multiplayer.globalChatInfoMessage("Bundle");
                var flag = false;
                foreach (var bundle in bundles)
                {
                    if (!bundle.Complete && !bundle.Equals(CurrentPageBundle))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    if (whichArea == 6)
                    {
                        exitFunction = restoreaAreaOnExit_AbandonedJojaMart;
                    }
                    else
                    {
                        location.markAreaAsComplete(whichArea);
                        exitFunction = restoreAreaOnExit;
                        location.areaCompleteReward(whichArea);
                    }
                }
                else
                {
                    location.getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(CurrentPageBundle.BundleColor), location);
                }
                CheckForRewards();
            }
            else
            {
                if (onBundleComplete == null)
                {
                    return;
                }
                onBundleComplete(this);
            }
        }

        protected void restoreaAreaOnExit_AbandonedJojaMart()
        {
            Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart").restoreAreaCutscene();
        }

        protected void restoreAreaOnExit()
        {
            if (fromGameMenu)
            {
                return;
            }
            Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(whichArea);
        }

        public virtual void CheckForRewards()
        {
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(CommunityCenter.getAreaNameFromNumber(whichArea)) && bundleData[key].Split('/')[1].Length > 1)
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    if (Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[int32])
                    {
                        presentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f, true));
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!canClick)
            {
                return;
            }
            if (specificBundlePage)
            {
                heldItem = inventory.rightClick(x, y, heldItem);
                if (partialDonationItem != null)
                {
                    for (var index = 0; index < ingredientSlots.Count; ++index)
                    {
                        if (ingredientSlots[index].containsPoint(x, y) && ingredientSlots[index].item == partialDonationItem)
                        {
                            if (partialDonationComponents.Count > 0)
                            {
                                var one = partialDonationComponents[0].getOne();
                                var flag = false;
                                if (heldItem == null)
                                {
                                    heldItem = one;
                                    Game1.playSound("dwop");
                                    flag = true;
                                }
                                else if (heldItem.canStackWith(one))
                                {
                                    heldItem.addToStack(one);
                                    Game1.playSound("dwop");
                                    flag = true;
                                }
                                if (flag)
                                {
                                    if (partialDonationComponents[0].ConsumeStack(1) == null)
                                    {
                                        partialDonationComponents.RemoveAt(0);
                                    }
                                    if (partialDonationItem != null)
                                    {
                                        var num = 0;
                                        foreach (var donationComponent in partialDonationComponents)
                                        {
                                            num += donationComponent.Stack;
                                        }
                                        partialDonationItem.Stack = num;
                                    }
                                    if (partialDonationComponents.Count == 0)
                                    {
                                        ResetPartialDonation();
                                        break;
                                    }
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                    }
                }
            }
            if (specificBundlePage || !IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            exitThisMenu(gameMenuTabToReturnTo == -1);
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            if (specificBundlePage && CurrentPageBundle != null && CurrentPageBundle.CompletionTimer <= 0 && IsReadyToCloseMenuOrBundle() && CurrentPageBundle.Complete)
            {
                takeDownBundleSpecificPage();
            }
            foreach (var bundle in bundles)
            {
                bundle.Update(time);
            }
            tempSprites.RemoveWhere(sprite => sprite.update(time));
            presentButton?.update(time);
            if (screenSwipe != null)
            {
                canClick = false;
                if (screenSwipe.update(time))
                {
                    screenSwipe = null;
                    canClick = true;
                    var screenSwipeFinished = onScreenSwipeFinished;
                    if (screenSwipeFinished != null)
                    {
                        screenSwipeFinished(this);
                    }
                }
            }
            if (!bundlesChanged || !fromGameMenu)
            {
                return;
            }
            reOpenThisMenu();
        }

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (scrambledText)
            {
                return;
            }
            hoverText = "";
            if (specificBundlePage)
            {
                backButton?.tryHover(x, y);
                hoveredItem = CurrentPageBundle.Complete || CurrentPageBundle.CompletionTimer > 0 ? null : inventory.hover(x, y, heldItem);
                foreach (var ingredient in ingredientList)
                {
                    if (ingredient.bounds.Contains(x, y))
                    {
                        hoverText = ingredient.hoverText;
                        break;
                    }
                }
                if (heldItem != null)
                {
                    foreach (var ingredientSlot in ingredientSlots)
                    {
                        if (ingredientSlot.bounds.Contains(x, y) && CanBePartiallyOrFullyDonated(heldItem) && (partialDonationItem == null || ingredientSlot.item == partialDonationItem))
                        {
                            ingredientSlot.sourceRect.X = 530;
                            ingredientSlot.sourceRect.Y = 262;
                        }
                        else
                        {
                            ingredientSlot.sourceRect.X = 512;
                            ingredientSlot.sourceRect.Y = 244;
                        }
                    }
                }
                purchaseButton?.tryHover(x, y);
            }
            else
            {
                if (presentButton != null)
                {
                    hoverText = presentButton.tryHover(x, y);
                }
                foreach (var bundle in bundles)
                {
                    bundle.TryHoverAction(x, y);
                }
                if (!fromGameMenu)
                {
                    return;
                }
                areaNextButton.tryHover(x, y);
                areaBackButton.tryHover(x, y);
            }
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            if (Game1.options.showMenuBackground)
            {
                drawBackground(b);
            }
            else if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            }
            if (!specificBundlePage)
            {
                b.Draw(noteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                SpriteText.drawStringHorizontallyCenteredAt(b, scrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(whichArea) : CommunityCenter.getAreaDisplayNameFromNumber(whichArea), xPositionOnScreen + width / 2 + 16, yPositionOnScreen + 12, height: 99999, alpha: 0.88f, junimoText: scrambledText);
                if (scrambledText)
                {
                    SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), xPositionOnScreen + 96, yPositionOnScreen + 96, width: width - 192, height: 99999, alpha: 0.88f, junimoText: true);
                    base.draw(b);
                    if (Game1.options.SnappyMenus || !canClick)
                    {
                        return;
                    }
                    drawMouse(b);
                    return;
                }
                foreach (var bundle in bundles)
                {
                    bundle.Draw(b);
                }
                presentButton?.draw(b);
                foreach (var tempSprite in tempSprites)
                {
                    tempSprite.draw(b, true);
                }
                if (fromGameMenu)
                {
                    if (areaNextButton.visible)
                    {
                        areaNextButton.draw(b);
                    }
                    if (areaBackButton.visible)
                    {
                        areaBackButton.draw(b);
                    }
                }
            }
            else
            {
                b.Draw(noteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(320, 0, 320, 180), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                if (CurrentPageBundle != null)
                {
                    var num1 = CurrentPageBundle.BundleIndex;
                    var texture = noteTexture;
                    var num2 = 180;
                    if (CurrentPageBundle.BundleTextureIndexOverride >= 0)
                    {
                        num1 = CurrentPageBundle.BundleTextureIndexOverride;
                    }
                    if (CurrentPageBundle.BundleTextureOverride != null)
                    {
                        texture = CurrentPageBundle.BundleTextureOverride;
                        num2 = 0;
                    }
                    b.Draw(texture, new Vector2(xPositionOnScreen + 872, yPositionOnScreen + 88), new Rectangle(num1 * 16 * 2 % texture.Width, num2 + 32 * (num1 * 16 * 2 / texture.Width), 32, 32), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
                    if (CurrentPageBundle.label != null)
                    {
                        var x = Game1.dialogueFont.MeasureString(!Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label)).X;
                        b.Draw(noteTexture, new Vector2(xPositionOnScreen + 936 - (int)x / 2 - 16, yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                        b.Draw(noteTexture, new Rectangle(xPositionOnScreen + 936 - (int)x / 2, yPositionOnScreen + 228, (int)x, 68), new Rectangle(520, 266, 1, 17), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                        b.Draw(noteTexture, new Vector2(xPositionOnScreen + 936 + (int)x / 2, yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                        b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(2f, 2f), Game1.textShadowColor);
                        b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(0.0f, 2f), Game1.textShadowColor);
                        b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(2f, 0.0f), Game1.textShadowColor);
                        b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236), Game1.textColor * 0.9f);
                    }
                }
                if (backButton != null)
                {
                    backButton.draw(b);
                }
                if (purchaseButton != null)
                {
                    purchaseButton.draw(b);
                    Game1.dayTimeMoneyBox.drawMoneyBox(b);
                }
                var extraAlpha = 1f;
                if (partialDonationItem != null)
                {
                    extraAlpha = 0.25f;
                }
                foreach (var tempSprite in tempSprites)
                {
                    tempSprite.draw(b, true, extraAlpha: extraAlpha);
                }
                foreach (var ingredientSlot in ingredientSlots)
                {
                    var alpha = 1f;
                    if (partialDonationItem != null && ingredientSlot.item != partialDonationItem)
                    {
                        alpha = 0.25f;
                    }
                    if (ingredientSlot.item == null || partialDonationItem != null && ingredientSlot.item == partialDonationItem)
                    {
                        ingredientSlot.draw(b, (fromGameMenu ? Color.LightGray * 0.5f : Color.White) * alpha, 0.89f);
                    }
                    ingredientSlot.drawItem(b, 4, 4, alpha);
                }
                for (var index = 0; index < ingredientList.Count; ++index)
                {
                    var num3 = 1f;
                    if (currentPartialIngredientDescriptionIndex >= 0 && currentPartialIngredientDescriptionIndex != index)
                    {
                        num3 = 0.25f;
                    }
                    var ingredient = ingredientList[index];
                    var flag = false;
                    var num4 = index;
                    var count = CurrentPageBundle?.Ingredients?.Count;
                    var valueOrDefault = count.GetValueOrDefault();
                    if (num4 < valueOrDefault & count.HasValue && CurrentPageBundle.Ingredients[index].completed)
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        b.Draw(Game1.shadowTexture, new Vector2(ingredient.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, ingredient.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White * num3, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                    }
                    if (ingredient.item != null && ingredient.visible)
                    {
                        ingredient.item.drawInMenu(b, new Vector2(ingredient.bounds.X, ingredient.bounds.Y), ingredient.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (flag ? 0.25f : num3), false);
                    }
                }
                inventory.draw(b);
            }
            if (getRewardNameForArea(whichArea) != "")
            {
                SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(whichArea), xPositionOnScreen + width / 2, Math.Min(yPositionOnScreen + height + 20, Game1.uiViewport.Height - 64 - 8));
            }
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;
            if (canClick)
            {
                drawMouse(b);
            }
            heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
            if (inventory.descriptionText.Length > 0)
            {
                if (hoveredItem != null)
                {
                    drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
                }
            }
            else
            {
                drawHoverText(b, singleBundleMenu || Game1.player.hasOrWillReceiveMail("canReadJunimoText") || hoverText.Length <= 0 ? hoverText : "???", Game1.dialogueFont);
            }
            screenSwipe?.draw(b);
        }

        public virtual string getRewardNameForArea(int whichArea)
        {
            switch (whichArea)
            {
                case -1:
                    return "";
                case 0:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry");
                case 1:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts");
                case 2:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank");
                case 3:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler");
                case 4:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault");
                case 5:
                    return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin");
                default:
                    return "???";
            }
        }

        /// <inheritdoc />
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            tempSprites.Clear();
            xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - 360;
            backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
            if (fromGameMenu)
            {
                var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                textureComponent1.visible = false;
                areaNextButton = textureComponent1;
                var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
                textureComponent2.visible = false;
                areaBackButton = textureComponent2;
            }
            inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: Game1.player.maxItems.Value, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false);
            for (var index = 0; index < inventory.inventory.Count; ++index)
            {
                if (index >= inventory.actualInventory.Count)
                {
                    inventory.inventory[index].visible = false;
                }
            }
            for (var index = 0; index < bundles.Count; ++index)
            {
                var locationFromNumber = getBundleLocationFromNumber(index);
                bundles[index].bounds.X = locationFromNumber.X;
                bundles[index].bounds.Y = locationFromNumber.Y;
                bundles[index].Sprite.position = new Vector2(locationFromNumber.X, locationFromNumber.Y);
            }
            if (!specificBundlePage)
            {
                return;
            }
            var ofIngredientSlots = CurrentPageBundle.NumberOfIngredientSlots;
            var toAddTo1 = new List<Rectangle>();
            addRectangleRowsToList(toAddTo1, ofIngredientSlots, 932, 540);
            ingredientSlots.Clear();
            for (var index = 0; index < toAddTo1.Count; ++index)
                ingredientSlots.Add(new ClickableTextureComponent(toAddTo1[index], noteTexture, new Rectangle(512, 244, 18, 18), 4f));
            var toAddTo2 = new List<Rectangle>();
            ingredientList.Clear();
            addRectangleRowsToList(toAddTo2, CurrentPageBundle.Ingredients.Count, 932, 364);
            for (var index = 0; index < toAddTo2.Count; ++index)
            {
                var ingredient = CurrentPageBundle.Ingredients[index];
                var metadata = ItemRegistry.GetMetadata(ingredient.id);
                if (metadata?.TypeIdentifier == "(O)")
                {
                    var parsedOrErrorData = metadata.GetParsedOrErrorData();
                    var texture = parsedOrErrorData.GetTexture();
                    var sourceRect = parsedOrErrorData.GetSourceRect();
                    var obj = ingredient.preservesId != null ? Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack) : ItemRegistry.Create(ingredient.id, ingredient.stack, ingredient.quality);
                    var ingredientList = this.ingredientList;
                    var textureComponent = new ClickableTextureComponent("", toAddTo2[index], "", obj.DisplayName, texture, sourceRect, 4f);
                    textureComponent.myID = index + 1000;
                    textureComponent.item = obj;
                    textureComponent.upNeighborID = -99998;
                    textureComponent.rightNeighborID = -99998;
                    textureComponent.leftNeighborID = -99998;
                    textureComponent.downNeighborID = -99998;
                    ingredientList.Add(textureComponent);
                }
            }
            updateIngredientSlots();
        }

        private void setUpBundleSpecificPage(BundleRemake b)
        {
            tempSprites.Clear();
            CurrentPageBundle = b;
            specificBundlePage = true;
            if (whichArea == 4)
            {
                if (fromGameMenu)
                {
                    return;
                }
                var textureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 504, 260, 72), noteTexture, new Rectangle(517, 286, 65, 20), 4f);
                textureComponent.myID = 797;
                textureComponent.leftNeighborID = 103;
                purchaseButton = textureComponent;
                if (!Game1.options.SnappyMenus)
                {
                    return;
                }
                currentlySnappedComponent = purchaseButton;
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                var ofIngredientSlots = b.NumberOfIngredientSlots;
                var toAddTo1 = new List<Rectangle>();
                addRectangleRowsToList(toAddTo1, ofIngredientSlots, 932, 540);
                for (var index = 0; index < toAddTo1.Count; ++index)
                {
                    var ingredientSlots = this.ingredientSlots;
                    var textureComponent = new ClickableTextureComponent(toAddTo1[index], noteTexture, new Rectangle(512, 244, 18, 18), 4f);
                    textureComponent.myID = index + 250;
                    textureComponent.upNeighborID = -99998;
                    textureComponent.rightNeighborID = -99998;
                    textureComponent.leftNeighborID = -99998;
                    textureComponent.downNeighborID = -99998;
                    ingredientSlots.Add(textureComponent);
                }
                var toAddTo2 = new List<Rectangle>();
                addRectangleRowsToList(toAddTo2, b.Ingredients.Count, 932, 364);
                for (var index = 0; index < toAddTo2.Count; ++index)
                {
                    var ingredient = b.Ingredients[index];
                    var representativeItemId = GetRepresentativeItemId(ingredient);
                    var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(representativeItemId);
                    if (dataOrErrorItem.HasTypeObject())
                    {
                        var category = ingredient.category;
                        string hoverText;
                        if (category.HasValue)
                        {
                            switch (category.GetValueOrDefault())
                            {
                                case -75:
                                    hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
                                    goto label_18;
                                case -6:
                                    hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
                                    goto label_18;
                                case -5:
                                    hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
                                    goto label_18;
                                case -4:
                                    hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
                                    goto label_18;
                                case -2:
                                    hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
                                    goto label_18;
                            }
                        }
                        hoverText = dataOrErrorItem.DisplayName;
                    label_18:
                        Item flavoredItem;
                        if (ingredient.preservesId != null)
                        {
                            flavoredItem = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
                            hoverText = flavoredItem.DisplayName;
                        }
                        else
                        {
                            flavoredItem = ItemRegistry.Create(representativeItemId, ingredient.stack, ingredient.quality);
                        }
                        var texture = dataOrErrorItem.GetTexture();
                        var sourceRect = dataOrErrorItem.GetSourceRect();
                        var ingredientList = this.ingredientList;
                        var textureComponent = new ClickableTextureComponent("ingredient_list_slot", toAddTo2[index], "", hoverText, texture, sourceRect, 4f);
                        textureComponent.myID = index + 1000;
                        textureComponent.item = flavoredItem;
                        textureComponent.upNeighborID = -99998;
                        textureComponent.rightNeighborID = -99998;
                        textureComponent.leftNeighborID = -99998;
                        textureComponent.downNeighborID = -99998;
                        ingredientList.Add(textureComponent);
                    }
                }
                updateIngredientSlots();
                if (!Game1.options.SnappyMenus)
                {
                    return;
                }
                populateClickableComponentList();
                if (inventory?.inventory != null)
                {
                    for (var index = 0; index < inventory.inventory.Count; ++index)
                    {
                        if (inventory.inventory[index] != null)
                        {
                            if (inventory.inventory[index].downNeighborID == 101)
                            {
                                inventory.inventory[index].downNeighborID = -1;
                            }
                            if (inventory.inventory[index].leftNeighborID == -1)
                            {
                                inventory.inventory[index].leftNeighborID = 103;
                            }
                            if (inventory.inventory[index].upNeighborID >= 1000)
                            {
                                inventory.inventory[index].upNeighborID = 103;
                            }
                        }
                    }
                }
                currentlySnappedComponent = getComponentWithID(0);
                snapCursorToCurrentSnappedComponent();
            }
        }

        public override bool IsAutomaticSnapValid(
          int direction,
          ClickableComponent a,
          ClickableComponent b)
        {
            return (currentPartialIngredientDescriptionIndex < 0 || (!ingredientSlots.Contains(b) || b.item == partialDonationItem) && (!ingredientList.Contains(b) || ingredientList.IndexOf(b as ClickableTextureComponent) == currentPartialIngredientDescriptionIndex)) && (a.myID >= 5000 || a.myID == 101 ? 1 : a.myID == 102 ? 1 : 0) == (b.myID >= 5000 || b.myID == 101 ? 1 : b.myID == 102 ? 1 : 0);
        }

        private void addRectangleRowsToList(
          List<Rectangle> toAddTo,
          int numberOfItems,
          int centerX,
          int centerY)
        {
            switch (numberOfItems)
            {
                case 1:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
                    break;
                case 2:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
                    break;
                case 3:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
                    break;
                case 4:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
                    break;
                case 5:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
                    break;
                case 6:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 7:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 8:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 9:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 10:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 11:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 12:
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
                    break;
            }
        }

        private List<Rectangle> createRowOfBoxesCenteredAt(
          int xStart,
          int yStart,
          int numBoxes,
          int boxWidth,
          int boxHeight,
          int horizontalGap)
        {
            var ofBoxesCenteredAt = new List<Rectangle>();
            var num = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
            var y = yStart - boxHeight / 2;
            for (var index = 0; index < numBoxes; ++index)
                ofBoxesCenteredAt.Add(new Rectangle(num + index * (boxWidth + horizontalGap), y, boxWidth, boxHeight));
            return ofBoxesCenteredAt;
        }

        public void takeDownBundleSpecificPage()
        {
            if (!IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            ReturnPartialDonations(false);
            hoveredItem = null;
            if (!specificBundlePage)
            {
                return;
            }
            specificBundlePage = false;
            ingredientSlots.Clear();
            ingredientList.Clear();
            tempSprites.Clear();
            purchaseButton = null;
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            if (CurrentPageBundle != null)
            {
                currentlySnappedComponent = CurrentPageBundle;
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                snapToDefaultClickableComponent();
            }
        }

        private Point getBundleLocationFromNumber(int whichBundle)
        {
            var locationFromNumber = new Point(xPositionOnScreen, yPositionOnScreen);
            switch (whichBundle)
            {
                case 0:
                    locationFromNumber.X += 592;
                    locationFromNumber.Y += 136;
                    break;
                case 1:
                    locationFromNumber.X += 392;
                    locationFromNumber.Y += 384;
                    break;
                case 2:
                    locationFromNumber.X += 784;
                    locationFromNumber.Y += 388;
                    break;
                case 3:
                    locationFromNumber.X += 304;
                    locationFromNumber.Y += 252;
                    break;
                case 4:
                    locationFromNumber.X += 892;
                    locationFromNumber.Y += 252;
                    break;
                case 5:
                    locationFromNumber.X += 588;
                    locationFromNumber.Y += 276;
                    break;
                case 6:
                    locationFromNumber.X += 588;
                    locationFromNumber.Y += 380;
                    break;
                case 7:
                    locationFromNumber.X += 440;
                    locationFromNumber.Y += 164;
                    break;
                case 8:
                    locationFromNumber.X += 776;
                    locationFromNumber.Y += 164;
                    break;
            }
            return locationFromNumber;
        }
    }
}
