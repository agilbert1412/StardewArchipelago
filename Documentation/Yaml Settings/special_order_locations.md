# Stardew Valley Archipelago - Special Order Locations

## Special Order Locations

This setting allows you to shuffle the [Special Orders](https://stardewvalleywiki.com/Quests#List_of_Special_Orders) and their rewards on both the Town board and the Qi Board

### How does the shuffling work?

If enabled, all special orders will have one associated location check.

All unique rewards usually received from special orders (Solar Panel Recipe, coffee maker, mini-fridge, etc) will be instead be randomized as items in your item pool.

Important Note: Even if special orders are **not** shuffled, some of their rewards might turn out to be necessary for different tasks or goals. For example, if you enable ~~[Craftsanity](./craftsanity.md)~~, you will need to craft every recipe, including the ones from special orders.

This means that you might need to complete some, or many, vanilla special orders to complete these tasks.

As a result, turning off this setting does **not** exclude Special Orders from your slot. It simply makes them behave like vanilla. Whether that it desirable or not depends on your other settings and preferences.

The Board Special Orders always give [Prize Tickets](https://stardewvalleywiki.com/Prize_Ticket) in addition to their usualy reward, shuffled or not.

### Option Values

This option works in two parts.

`vanilla`, `board` and `board_qi` decide which special orders are shuffled

`short` and `very_short` decide if the special orders should be made shorter in general, regardless of if they are shuffled or not.

Here are the 9 possible values
- `vanilla`
- `board`
- `board_qi`
- `vanilla_short`
- `board_short` **This is the default value**
- `board_qi_short`
- `vanilla_very_short`
- `board_very_short`
- `board_qi_very_short`

#### Vanilla

The special orders are not shuffled at all. You might still need or want to complete some for their vanilla rewards.

#### Board

The [Special Orders from the board in town](https://stardewvalleywiki.com/Quests#List_of_Special_Orders) will be shuffled and their normal rewards added to your item pool. Completing them will still produce [Prize Tickets](https://stardewvalleywiki.com/Prize_Ticket).

The Board itself is added to your item pool and needs to be found, to start completing orders.

Some special orders are repeatable. Completing it a second time will give the gold reward and prize ticket, but no location check and no special reward.

Full List:

- A Curious Substance
- Aquatic Overpopulation
- Biome Balance
- Cave Patrol
- Community Cleanup
- Crop Order
- Fragments of the past (Known bug: Mistakenly excluded with [Exclude Ginger Island](./exclude_ginger_island.md))
- Gifts for George
- Gus' Famous Omelet
- Island Ingredients (Requires Ginger Island)
- Juicy Bugs Wanted!
- Pierre's Prime Produce
- Prismatic Jelly
- Robin's Project
- Robin's Resource Rush
- Rock Rejuvenation
- The Strong Stuff
- Tropical Fish (Requires Ginger Island)

#### Qi

The [Qi Walnut Room Special Orders](https://stardewvalleywiki.com/Quests#List_of_Mr._Qi.27s_Special_Orders) will be shuffled, and a large sample of packs of Qi Gems will be added to your item pool.

Some Qi Special Orders are repeatable. Completing it a second time will give the usual Qi Gem rewards, and is the only way for Qi Gems to be renewable.

Some of these special orders also enable, permanently or temporarily, access to game content that is otherwise unobtainable. For example, you cannot get Qi Beans and Qi Fruit without starting `Qi's Crop`. As a result, if the Qi Special Orders are disabled, said content is also removed from other tasks, such as Cropsanity, Fishsanity or Shipsanity, to avoid forcing the player to do them anyway.

Full List:

- Danger In The Deep (allows access to dangerous monsters and radioactive ore)
- Extended Family (allows access to 5 extra fish)
- Four Precious Stones
- Let's Play A Game (Excluded if [Arcade Machine Locations](./arcade_machine_locations.md) are excluded)
- Qi's Crop (allows access to Qi Beans and Qi Fruit)
- Qi's Cuisine
- Qi's Hungry Challenge
- Qi's Kindness
- Qi's Primsatic Grange
- Skull Cavern Invasion (allows access to dangerous monsters and radioactive ore)

#### Short

If enabled, all special orders that contain a numeric requirement, have said requirement reduced by 40%.

The `Short` and `Very Short` modifiers are very critical to customizing the duration of your slot, as Special Orders are often very time-consuming and grindy tasks.

Examples (not a full list):

- Aquatic Overpopulation requires 6 fish instead of 10
- Community Cleanup requires 12 trash instead of 20
- Pierre's Prime Produce requires 15 Gold Vegetables instead of 25
- Robin's Resource Rush requires 600 resources instead of 1000
- Qi's Crop requires 300 Qi Fruit instead of 500
- Qi's Kindness requires 30 Loved Gifts instead of 50.

#### Very Short

If enabled, all special orders that contain a numeric requirement, have said requirement reduced by 80%.

The `Short` and `Very Short` modifiers are very critical to customizing the duration of your slot, as Special Orders are often very time-consuming and grindy tasks.

Examples (not a full list):

- Aquatic Overpopulation requires 2 fish instead of 10
- Community Cleanup requires 4 trash instead of 20
- Pierre's Prime Produce requires 5 Gold Vegetables instead of 25
- Robin's Resource Rush requires 200 resources instead of 1000
- Qi's Crop requires 100 Qi Fruit instead of 500
- Qi's Kindness requires 10 Loved Gifts instead of 50.

## Interaction With Other Settings

### [Arcade Machine Locations](./arcade_machine_locations.md)

If Arcade Machines are disabled, the `Let's Play A Game` Qi Order is also disabled.

### [Exclude Ginger Island](./exclude_ginger_island.md)

If Ginger Island is excluded, Qi orders will be forced to disabled, overwriting the setting if the player had enabled it.

### Exclusions from Qi Orders

If Qi Orders are not included, the following settings will also exclude some content, to avoid forcing the player to do Qi orders anyway

- ~~[Fishsanity](./fishsanity.md)~~
  - The 5 Extended Family Legendary Fish
- ~~[Monstersanity](./monstersanity.md)~~
  - Shadow Sniper
  - Skeleton Mage
  - Magma Duggy
  - Royal Serpent
- ~~[Shipsanity](./shipsanity.md)~~
  - Hyper Speed-Gro
  - Magic Bait
  - Qi Bean and Qi Fruit
  - The 5 Extended Family Legendary Fish
  - Qi Seasoning
  - Radioactive Ore and Bar
  - Enricher
  - Pressure Nozzle
  - Galaxy Soul
  - Mushroom Tree Seed
  - Blue Grass Starter
- ~~[Craftsanity](./craftsanity.md)~~
  - Hyper Speed-Gro
  - Magic Bait
  - Heavy Tapper
  - Hopper
  - Blue Grass Starter
- ~~[Eatsanity](./eatsanity.md)~~
  - Qi Fruit
  - The 5 Extended Family Legendary Fish
- ~~[Secretsanity](./secretsanity.md)~~
  - Obtain my precious fruit whenever you like
- ~~[Hatsanity](./hatsanity.md)~~
  - Gnome's Cap
  - Radioactive Goggles
- ~~[Include Endgame Locations](./include_endgame_locations.md)~~
  - Horse Flute
  - Pierre's Missing Stocklist
  - Key To The Town
  - Mini-Shipping Bin
  - Exotic Double Bed
  - Golden Egg
- ~~[Mods](./mods.md)~~
  - (Craftsanity) Radioactive Slot Machine

## [Return to Index](./index.md)
