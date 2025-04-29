using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Bundles;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable PossibleLossOfFraction

#nullable disable
namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public abstract class JunimoNoteMenuRemake : IClickableMenu
    {
        public const int REGION_INGREDIENT_SLOT_MODIFIER = 250;
        public const int REGION_INGREDIENT_LIST_MODIFIER = 1000;
        public const int REGION_BUNDLE_MODIFIER = 5000;
        public const int REGION_AREA_NEXT_BUTTON = 101;
        public const int REGION_AREA_BACK_BUTTON = 102;
        public const int REGION_BACK_BUTTON = 103;
        public const int REGION_PURCHASE_BUTTON = 104;
        public const int REGION_PRESENT_BUTTON = 105;
        public const int BASE_WIDTH = 320;
        public const int BASE_HEIGHT = 180;
        public const string NOTE_TEXTURE_NAME = "LooseSprites\\JunimoNote";
        public Texture2D NoteTexture;
        public bool SpecificBundlePage;
        public InventoryMenu Inventory;
        public Item HeldItem;
        public Item HoveredItem;
        public static bool CanClick = true;
        public int WhichArea;
        public int GameMenuTabToReturnTo = -1;
        public IClickableMenu MenuToReturnTo;
        public bool BundlesChanged;
        public static ScreenSwipe ScreenSwipe;
        public static string HoverText = "";
        public List<BundleRemake> Bundles = new();
        public static TemporaryAnimatedSpriteList TempSprites = new();
        public bool FromGameMenu;
        public bool FromThisMenu;
        public bool ScrambledText;
        protected readonly bool _singleBundleMenu;
        public ClickableTextureComponent BackButton;
        public ClickableTextureComponent AreaNextButton;
        public ClickableTextureComponent AreaBackButton;
        public ClickableAnimatedComponent PresentButton;
        public Action<int> OnIngredientDeposit;
        public Action<JunimoNoteMenuRemake> OnBundleComplete;
        public Action<JunimoNoteMenuRemake> OnScreenSwipeFinished;
        public BundleRemake CurrentPageBundle;
        protected int _oldTriggerSpot;

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
                        WhichArea = area;
                        break;
                    }
                }
                if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
                {
                    area = 6;
                }
            }
            SetUpMenu(area, communityCenter.bundlesDict());
            Game1.player.forceCanMove();
            var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
            textureComponent1.visible = false;
            textureComponent1.myID = REGION_AREA_NEXT_BUTTON;
            textureComponent1.leftNeighborID = REGION_AREA_BACK_BUTTON;
            textureComponent1.leftNeighborImmutable = true;
            textureComponent1.downNeighborID = -99998;
            AreaNextButton = textureComponent1;
            var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
            textureComponent2.visible = false;
            textureComponent2.myID = REGION_AREA_BACK_BUTTON;
            textureComponent2.rightNeighborID = REGION_AREA_NEXT_BUTTON;
            textureComponent2.rightNeighborImmutable = true;
            textureComponent2.downNeighborID = -99998;
            AreaBackButton = textureComponent2;
            var num = 6;
            for (var area1 = 0; area1 < num; ++area1)
            {
                if (area1 != area && communityCenter.shouldNoteAppearInArea(area1))
                {
                    AreaNextButton.visible = true;
                    AreaBackButton.visible = true;
                    break;
                }
            }
            this.FromGameMenu = fromGameMenu;
            this.FromThisMenu = fromThisMenu;
            foreach (var bundle in Bundles)
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
            SetUpMenu(whichArea, bundlesComplete);
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public JunimoNoteMenuRemake(BundleRemake bundle, string noteTexturePath)
          : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
        {
            _singleBundleMenu = true;
            WhichArea = -1;
            NoteTexture = Game1.temporaryContent.Load<Texture2D>(noteTexturePath);
            TempSprites.Clear();
            Inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: 36, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false)
            {
                capacity = 36
            };
            for (var index = 0; index < Inventory.inventory.Count; ++index)
            {
                if (index >= Inventory.actualInventory.Count)
                {
                    Inventory.inventory[index].visible = false;
                }
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
            {
                clickableComponent.downNeighborID = -99998;
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Right))
            {
                clickableComponent.rightNeighborID = -99998;
            }
            Inventory.dropItemInvisibleButton.visible = false;
            CanClick = true;
            SetUpBundleSpecificPage(bundle);
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            if (SpecificBundlePage)
            {
                currentlySnappedComponent = getComponentWithID(0);
            }
            else
            {
                currentlySnappedComponent = getComponentWithID(REGION_BUNDLE_MODIFIER);
            }
            snapCursorToCurrentSnappedComponent();
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements() => !SpecificBundlePage;

        protected override void customSnapBehavior(int direction, int oldRegion, int oldId)
        {
            if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || oldId - REGION_BUNDLE_MODIFIER < 0 || oldId - REGION_BUNDLE_MODIFIER >= 10 || currentlySnappedComponent == null)
            {
                return;
            }
            var num1 = -1;
            var num2 = 999999;
            var center1 = currentlySnappedComponent.bounds.Center;
            for (var index = 0; index < Bundles.Count; ++index)
            {
                if (Bundles[index].myID != oldId)
                {
                    var num3 = 999999;
                    var center2 = Bundles[index].bounds.Center;
                    switch (direction)
                    {
                        case 0:
                            if (center2.Y < center1.Y)
                            {
                                num3 = center1.Y - center2.Y + Math.Abs(center1.X - center2.X) * 3;
                            }
                            break;
                        case 1:
                            if (center2.X > center1.X)
                            {
                                num3 = center2.X - center1.X + Math.Abs(center1.Y - center2.Y) * 3;
                            }
                            break;
                        case 2:
                            if (center2.Y > center1.Y)
                            {
                                num3 = center2.Y - center1.Y + Math.Abs(center1.X - center2.X) * 3;
                            }
                            break;
                        case 3:
                            if (center2.X < center1.X)
                            {
                                num3 = center1.X - center2.X + Math.Abs(center1.Y - center2.Y) * 3;
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
                currentlySnappedComponent = getComponentWithID(num1 + REGION_BUNDLE_MODIFIER);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (AreaNextButton == null || !AreaNextButton.visible)
                        {
                            break;
                        }
                        currentlySnappedComponent = AreaNextButton;
                        snapCursorToCurrentSnappedComponent();
                        AreaNextButton.leftNeighborID = oldId;
                        break;
                    case 2:
                        if (PresentButton == null)
                        {
                            break;
                        }
                        currentlySnappedComponent = PresentButton;
                        snapCursorToCurrentSnappedComponent();
                        PresentButton.upNeighborID = oldId;
                        break;
                    case 3:
                        if (AreaBackButton == null || !AreaBackButton.visible)
                        {
                            break;
                        }
                        currentlySnappedComponent = AreaBackButton;
                        snapCursorToCurrentSnappedComponent();
                        AreaBackButton.rightNeighborID = oldId;
                        break;
                }
            }
        }

        public virtual void SetUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            NoteTexture = Game1.temporaryContent.Load<Texture2D>(NOTE_TEXTURE_NAME);
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
            ScrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
            TempSprites.Clear();
            this.WhichArea = whichArea;
            Inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: 36, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false)
            {
                capacity = 36
            };
            for (var index = 0; index < Inventory.inventory.Count; ++index)
            {
                if (index >= Inventory.actualInventory.Count)
                {
                    Inventory.inventory[index].visible = false;
                }
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
            {
                clickableComponent.downNeighborID = -99998;
            }
            foreach (var clickableComponent in Inventory.GetBorder(InventoryMenu.BorderSide.Right))
            {
                clickableComponent.rightNeighborID = -99998;
            }
            Inventory.dropItemInvisibleButton.visible = false;
            var bundleData = Game1.netWorldState.Value.BundleData;
            var areaNameFromNumber = CommunityCenter.getAreaNameFromNumber(whichArea);
            var whichBundle = 0;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(areaNameFromNumber))
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    var bundles = this.Bundles;
                    var bundle = new BundleRemake(int32, bundleData[key], bundlesComplete[int32], GetBundleLocationFromNumber(whichBundle), NOTE_TEXTURE_NAME, this);
                    bundle.myID = whichBundle + REGION_BUNDLE_MODIFIER;
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
            textureComponent.myID = REGION_BACK_BUTTON;
            BackButton = textureComponent;
            CheckForRewards();
            CanClick = true;
            Game1.playSound("shwip");
            var flag = false;
            foreach (var bundle in Bundles)
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

        public override bool IsAutomaticSnapValid(
            int direction,
            ClickableComponent a,
            ClickableComponent b)
        {
            var aIsInBundleRegion = a.myID is >= REGION_BUNDLE_MODIFIER or >= REGION_AREA_NEXT_BUTTON or REGION_AREA_BACK_BUTTON;
            var bIsInBundleRegion = b.myID is >= REGION_BUNDLE_MODIFIER or >= REGION_AREA_NEXT_BUTTON or REGION_AREA_BACK_BUTTON;
            var sameRegion = aIsInBundleRegion == bIsInBundleRegion;
            return sameRegion;
        }

        public virtual bool HighlightObjects(Item item)
        {
            return false;
        }

        public override bool readyToClose()
        {
            return (!SpecificBundlePage || _singleBundleMenu) && IsReadyToCloseMenuOrBundle();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!CanClick)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            if (ScrambledText)
            {
                return;
            }
            if (SpecificBundlePage)
            {
                if (TryReceiveLeftClickInSpecificBundlePage(x, y))
                {
                    return;
                }
            }
            else
            {
                if (TryReceiveLeftClickInBundlesPage(x, y))
                {
                    return;
                }
            }
            if (HeldItem == null || isWithinBounds(x, y) || !HeldItem.canBeTrashed())
            {
                return;
            }
            Game1.playSound("throwDownITem");
            Game1.createItemDebris(HeldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
            HeldItem = null;
        }

        private bool TryReceiveLeftClickInSpecificBundlePage(int x, int y)
        {
            if (!CurrentPageBundle.Complete && CurrentPageBundle.CompletionTimer <= 0)
            {
                HeldItem = Inventory.leftClick(x, y, HeldItem);
            }
            if (BackButton != null && BackButton.containsPoint(x, y) && HeldItem == null)
            {
                CloseBundlePage();
            }
            if (TryReceiveLeftClickInBundleArea(x, y))
            {
                return true;
            }
            if (upperRightCloseButton != null && IsReadyToCloseMenuOrBundle() && upperRightCloseButton.containsPoint(x, y))
            {
                CloseBundlePage();
                return true;
            }
            return false;
        }

        protected abstract bool TryReceiveLeftClickInBundleArea(int x, int y);

        private bool TryReceiveLeftClickInBundlesPage(int x, int y)
        {
            foreach (var bundle in Bundles)
            {
                if (bundle.CanBeClicked() && bundle.containsPoint(x, y))
                {
                    SetUpBundleSpecificPage(bundle);
                    Game1.playSound("shwip");
                    return true;
                }
            }
            if (PresentButton != null && PresentButton.containsPoint(x, y) && !FromGameMenu && !FromThisMenu)
            {
                OpenRewardsMenu();
            }
            if (FromGameMenu)
            {
                if (AreaNextButton.containsPoint(x, y))
                {
                    SwapPage(1);
                }
                else if (AreaBackButton.containsPoint(x, y))
                {
                    SwapPage(-1);
                }
            }
            return false;
        }

        public bool IsReadyToCloseMenuOrBundle()
        {
            if (SpecificBundlePage)
            {
                var currentPageBundle = this.CurrentPageBundle;
                if ((currentPageBundle != null ? currentPageBundle.CompletionTimer > 0 ? 1 : 0 : 0) != 0)
                {
                    return false;
                }
            }
            return HeldItem == null;
        }

        /// <inheritdoc />
        public override void receiveGamePadButton(Buttons button)
        {
            base.receiveGamePadButton(button);
            if (SpecificBundlePage)
            {
                ReceiveGamepadButtonInSpecificBundlePage(button);
            }
            else
            {
                if (!FromGameMenu)
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

        private void ReceiveGamepadButtonInSpecificBundlePage(Buttons button)
        {
            switch (button)
            {
                case Buttons.RightTrigger:
                    ReceiveRightTriggerInSpecificBundlePage();
                    break;
                case Buttons.LeftTrigger:
                    var snappedComponent2 = currentlySnappedComponent;
                    if ((snappedComponent2 != null ? snappedComponent2.myID >= REGION_INGREDIENT_SLOT_MODIFIER ? 1 : 0 : 0) == 0)
                    {
                        break;
                    }
                    setCurrentlySnappedComponentTo(_oldTriggerSpot);
                    snapCursorToCurrentSnappedComponent();
                    break;
            }
        }

        protected virtual void ReceiveRightTriggerInSpecificBundlePage()
        {
        }

        public void SwapPage(int direction)
        {
            if (direction > 0 && !AreaNextButton.visible || direction < 0 && !AreaBackButton.visible)
            {
                return;
            }
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            var whichArea = this.WhichArea;
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
                    var junimoNoteMenu = CreateNewMenu(whichArea);
                    Game1.activeClickableMenu = junimoNoteMenu;
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            if (GameMenuTabToReturnTo != -1)
            {
                closeSound = "shwip";
            }
            base.receiveKeyPress(key);
            if (key == Keys.Delete && HeldItem != null && HeldItem.canBeTrashed())
            {
                Utility.trashItem(HeldItem);
                HeldItem = null;
            }
            if (!Game1.options.doesInputListContain(Game1.options.menuButton, key) || !IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            if (_singleBundleMenu)
            {
                exitThisMenu(GameMenuTabToReturnTo == -1);
            }
            CloseBundlePage();
        }

        /// <inheritdoc />
        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
            if (GameMenuTabToReturnTo != -1)
            {
                Game1.activeClickableMenu = new GameMenu(GameMenuTabToReturnTo, playOpeningSound: false);
            }
            else
            {
                if (MenuToReturnTo == null)
                {
                    return;
                }
                Game1.activeClickableMenu = MenuToReturnTo;
            }
        }

        protected virtual void CloseBundlePage()
        {
            if (!SpecificBundlePage)
            {
                return;
            }
            HoveredItem = null;
            Inventory.descriptionText = "";
            if (HeldItem == null)
            {
                TakeDownBundleSpecificPage();
                Game1.playSound("shwip");
            }
            else
            {
                HeldItem = Inventory.tryToAddItem(HeldItem);
            }
        }

        protected virtual void ReOpenThisMenu()
        {
            var num = SpecificBundlePage ? 1 : 0;
            var junimoNoteMenu = CreateJunimoNoteMenu();
            if (num != 0)
            {
                foreach (var bundle in junimoNoteMenu.Bundles)
                {
                    if (bundle.BundleIndex == CurrentPageBundle.BundleIndex)
                    {
                        junimoNoteMenu.SetUpBundleSpecificPage(bundle);
                        break;
                    }
                }
            }
            Game1.activeClickableMenu = junimoNoteMenu;
        }

        protected abstract JunimoNoteMenuRemake CreateJunimoNoteMenu();

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

        private void OpenRewardsMenu()
        {
            Game1.playSound("smallSelect");
            var objList = new List<Item>();
            GetBundleRewards(WhichArea, objList);
            Game1.activeClickableMenu = new ItemGrabMenu(objList, false, true, null, null, null, RewardGrabbed, canBeExitedWithKey: true, context: this);
            Game1.activeClickableMenu.exitFunction = exitFunction != null ? exitFunction : ReOpenThisMenu;
        }

        private void RewardGrabbed(Item item, Farmer who)
        {
            Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[item.SpecialVariable] = false;
        }

        protected void restoreaAreaOnExit_AbandonedJojaMart()
        {
            Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart").restoreAreaCutscene();
        }

        protected void restoreAreaOnExit()
        {
            if (FromGameMenu)
            {
                return;
            }
            Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(WhichArea);
        }

        public virtual void CheckForRewards()
        {
            var bundleData = Game1.netWorldState.Value.BundleData;
            foreach (var key in bundleData.Keys)
            {
                if (key.Contains(CommunityCenter.getAreaNameFromNumber(WhichArea)) && bundleData[key].Split('/')[1].Length > 1)
                {
                    var int32 = Convert.ToInt32(key.Split('/')[1]);
                    if (Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[int32])
                    {
                        PresentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite(NOTE_TEXTURE_NAME, new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f, true));
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!CanClick)
            {
                return;
            }
            if (SpecificBundlePage)
            {
                ReceiveRightClickInSpecificBundlePage(x, y);
            }
            if (SpecificBundlePage || !IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            exitThisMenu(GameMenuTabToReturnTo == -1);
        }

        protected virtual void ReceiveRightClickInSpecificBundlePage(int x, int y)
        {
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            if (SpecificBundlePage && CurrentPageBundle != null && CurrentPageBundle.CompletionTimer <= 0 && IsReadyToCloseMenuOrBundle() && CurrentPageBundle.Complete)
            {
                TakeDownBundleSpecificPage();
            }
            foreach (var bundle in Bundles)
            {
                bundle.Update(time);
            }
            TempSprites.RemoveWhere(sprite => sprite.update(time));
            PresentButton?.update(time);
            if (ScreenSwipe != null)
            {
                CanClick = false;
                if (ScreenSwipe.update(time))
                {
                    ScreenSwipe = null;
                    CanClick = true;
                    var screenSwipeFinished = OnScreenSwipeFinished;
                    if (screenSwipeFinished != null)
                    {
                        screenSwipeFinished(this);
                    }
                }
            }
            if (!BundlesChanged || !FromGameMenu)
            {
                return;
            }
            ReOpenThisMenu();
        }

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (ScrambledText)
            {
                return;
            }
            HoverText = "";
            if (SpecificBundlePage)
            {
                PerformHoverActionInSpecificBundlePage(x, y);
            }
            else
            {
                PerformHoverActionInBundlesPage(x, y);
            }
        }

        protected virtual void PerformHoverActionInSpecificBundlePage(int x, int y)
        {
            BackButton?.tryHover(x, y);
            HoveredItem = CurrentPageBundle.Complete || CurrentPageBundle.CompletionTimer > 0 ? null : Inventory.hover(x, y, HeldItem);
        }

        private void PerformHoverActionInBundlesPage(int x, int y)
        {

            if (PresentButton != null)
            {
                HoverText = PresentButton.tryHover(x, y);
            }
            foreach (var bundle in Bundles)
            {
                bundle.TryHoverAction(x, y);
            }
            if (!FromGameMenu)
            {
                return;
            }
            AreaNextButton.tryHover(x, y);
            AreaBackButton.tryHover(x, y);
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
            if (!SpecificBundlePage)
            {
                DrawBundlesPage(b, out var stopHere);
                if (stopHere)
                {
                    return;
                }
            }
            else
            {
                DrawSpecificBundlePage(b);
            }
            if (GetRewardNameForArea(WhichArea) != "")
            {
                SpriteText.drawStringWithScrollCenteredAt(b, GetRewardNameForArea(WhichArea), xPositionOnScreen + width / 2, Math.Min(yPositionOnScreen + height + 20, Game1.uiViewport.Height - 64 - 8));
            }
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;
            if (CanClick)
            {
                drawMouse(b);
            }
            HeldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
            if (Inventory.descriptionText.Length > 0)
            {
                if (HoveredItem != null)
                {
                    drawToolTip(b, HoveredItem.getDescription(), HoveredItem.DisplayName, HoveredItem);
                }
            }
            else
            {
                drawHoverText(b, _singleBundleMenu || Game1.player.hasOrWillReceiveMail("canReadJunimoText") || HoverText.Length <= 0 ? HoverText : "???", Game1.dialogueFont);
            }
            ScreenSwipe?.draw(b);
        }

        private void DrawBundlesPage(SpriteBatch b, out bool stopHere)
        {
            b.Draw(NoteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, BASE_WIDTH, BASE_HEIGHT), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            SpriteText.drawStringHorizontallyCenteredAt(b, ScrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(WhichArea) : CommunityCenter.getAreaDisplayNameFromNumber(WhichArea), xPositionOnScreen + width / 2 + 16, yPositionOnScreen + 12, height: 99999, alpha: 0.88f, junimoText: ScrambledText);
            if (ScrambledText)
            {
                SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), xPositionOnScreen + 96, yPositionOnScreen + 96, width: width - 192, height: 99999, alpha: 0.88f, junimoText: true);
                base.draw(b);
                if (Game1.options.SnappyMenus || !CanClick)
                {
                    stopHere = true;
                    return;
                }
                drawMouse(b);
                stopHere = true;
                return;
            }
            foreach (var bundle in Bundles)
            {
                bundle.Draw(b);
            }
            PresentButton?.draw(b);
            foreach (var tempSprite in TempSprites)
            {
                tempSprite.draw(b, true);
            }
            if (FromGameMenu)
            {
                if (AreaNextButton.visible)
                {
                    AreaNextButton.draw(b);
                }
                if (AreaBackButton.visible)
                {
                    AreaBackButton.draw(b);
                }
            }
            stopHere = false;
        }

        protected virtual void DrawSpecificBundlePage(SpriteBatch b)
        {
            b.Draw(NoteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(BASE_WIDTH, 0, BASE_WIDTH, BASE_HEIGHT), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            DrawCurrentBundle(b);
            DrawBackButton(b);
            DrawBundleRequirements(b);
            Inventory.draw(b);
        }

        protected virtual void DrawBundleRequirements(SpriteBatch b)
        {
        }

        private void DrawCurrentBundle(SpriteBatch b)
        {
            if (CurrentPageBundle == null)
            {
                return;
            }

            var num1 = CurrentPageBundle.BundleIndex;
            var texture = NoteTexture;
            var num2 = BASE_HEIGHT;
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
            DrawBundleLabel(b);
        }

        private void DrawBundleLabel(SpriteBatch b)
        {
            if (CurrentPageBundle.label == null)
            {
                return;
            }

            var x = Game1.dialogueFont.MeasureString(!Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label)).X;
            b.Draw(NoteTexture, new Vector2(xPositionOnScreen + 936 - (int)x / 2 - 16, yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            b.Draw(NoteTexture, new Rectangle(xPositionOnScreen + 936 - (int)x / 2, yPositionOnScreen + 228, (int)x, 68), new Rectangle(520, 266, 1, 17), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            b.Draw(NoteTexture, new Vector2(xPositionOnScreen + 936 + (int)x / 2, yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(2f, 2f), Game1.textShadowColor);
            b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(0.0f, 2f), Game1.textShadowColor);
            b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236) + new Vector2(2f, 0.0f), Game1.textShadowColor);
            b.DrawString(Game1.dialogueFont, !Game1.player.hasOrWillReceiveMail("canReadJunimoText") ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", CurrentPageBundle.label), new Vector2(xPositionOnScreen + 936 - x / 2f, yPositionOnScreen + 236), Game1.textColor * 0.9f);
        }

        private void DrawBackButton(SpriteBatch b)
        {
            BackButton?.draw(b);
        }

        public virtual string GetRewardNameForArea(int whichArea)
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
            TempSprites.Clear();
            xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - 360;
            BackButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + borderWidth * 2 + 8, yPositionOnScreen + borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
            if (FromGameMenu)
            {
                var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                textureComponent1.visible = false;
                AreaNextButton = textureComponent1;
                var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
                textureComponent2.visible = false;
                AreaBackButton = textureComponent2;
            }
            Inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, true, highlightMethod: HighlightObjects, capacity: Game1.player.maxItems.Value, rows: 6, horizontalGap: 8, verticalGap: 8, drawSlots: false);
            for (var index = 0; index < Inventory.inventory.Count; ++index)
            {
                if (index >= Inventory.actualInventory.Count)
                {
                    Inventory.inventory[index].visible = false;
                }
            }
            for (var index = 0; index < Bundles.Count; ++index)
            {
                var locationFromNumber = GetBundleLocationFromNumber(index);
                Bundles[index].bounds.X = locationFromNumber.X;
                Bundles[index].bounds.Y = locationFromNumber.Y;
                Bundles[index].Sprite.position = new Vector2(locationFromNumber.X, locationFromNumber.Y);
            }
            GameWindowSizeChangedForSpecificBundle();
        }

        protected virtual void GameWindowSizeChangedForSpecificBundle()
        {
            if (!SpecificBundlePage)
            {
                return;
            }
        }

        protected virtual void SetUpBundleSpecificPage(BundleRemake bundle)
        {
            TempSprites.Clear();
            CurrentPageBundle = bundle;
            SpecificBundlePage = true;
        }

        public bool IsBundleCurrencyBased()
        {
            var ingredient = CurrentPageBundle.Ingredients.Last();
            var ingredientId = ingredient.id;
            return CurrencyBundle.CurrencyIds.ContainsValue(ingredientId);
        }

        protected void AddRectangleRowsToList(
          List<Rectangle> rectangles,
          int numberOfItems,
          int centerX,
          int centerY)
        {
            switch (numberOfItems)
            {
                case 1:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
                    break;
                case 2:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
                    break;
                case 3:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
                    break;
                case 4:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
                    break;
                case 5:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
                    break;
                case 6:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 7:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
                    break;
                case 8:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 9:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
                    break;
                case 10:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 11:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
                    break;
                case 12:
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
                    rectangles.AddRange(CreateRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
                    break;
            }
        }

        private List<Rectangle> CreateRowOfBoxesCenteredAt(
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
            {
                ofBoxesCenteredAt.Add(new Rectangle(num + index * (boxWidth + horizontalGap), y, boxWidth, boxHeight));
            }
            return ofBoxesCenteredAt;
        }

        public virtual void ReturnPartialDonations(bool toHand = true)
        {
        }

        public virtual void TakeDownBundleSpecificPage()
        {
            if (!IsReadyToCloseMenuOrBundle())
            {
                return;
            }
            ReturnPartialDonations(false);
            HoveredItem = null;
            if (!SpecificBundlePage)
            {
                return;
            }
            SpecificBundlePage = false;
            TempSprites.Clear();
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

        private Point GetBundleLocationFromNumber(int whichBundle)
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

        protected abstract JunimoNoteMenuRemake CreateNewMenu(int whichArea);
    }
}
