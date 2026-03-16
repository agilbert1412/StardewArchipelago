# Stardew Valley Archipelago - Building Progression

## Building Progression

This setting allows you to randomize the carpenter buildings.

### What are the buildings?

There are many different [Carpenter buildings](https://stardewvalleywiki.com/Carpenter%27s_Shop#Farm_Buildings). Some are progression, and some are mostly pointless. Some of them also have special traits

- Barn
- Big Barn (Requires a Barn)
- Deluxe Barn (Requires a Big Barn)
- Coop
- Big Coop (Requires a Coop)
- Deluxe Coop (Requires a Big Coop)
- Fish Pond
- Mill
- Shed
- Big Shed (Requires a Shed)
- Silo
- Slime Hutch
- Stable
- Well
- Shipping Bin (Starting Building)
- Pet Bowl (Starting Building)

Each building costs money and materials. The costs are visible in-game or on the wiki.
These buildings can be constructed at the Carpenter shop, and take 3 days to build.

You can construct multiple copies of a building if you wish.

The starting buildings automatically appear on your farm, but you can still go ask Robin to construct duplicates.

Buildings that require another buildings will not appear in the shop until the required building is constructed on your farm. This is because they are an upgrade to it, not a new building entirely.

### Option Values

This option works in two parts. First, what is shuffled, and second, what price multiplier should be added on to it.

Here are the 6 possible values
- `vanilla`
- `progresssive`
- `vanilla_cheap`
- `progresssive_cheap` **This is the default value, and it is the recommended experience.**
- `vanilla_very_cheap`
- `progresssive_very_cheap`

#### Vanilla

The buildings are not shuffled at all. They are all immediately unlocked, and you can go pay the costs to construct them whenever you want.

#### Progressive

You will get the starting buildings from the start (Shipping Bin and Pet Bowl) (Except if you decided to start without them in ~~[Start Without](./start_without.md)~~)

All other buildings need to be received as items before they can be constructed. For buildings that have multiple tiers of upgrade, the name will use the format `Progressive Coop`, and you can receive up to 3 copies.

The first copy of each building that you construct will be entirely free. Subsequent duplicates will have the normal cost.

For each building, there is an equivalent location that can be purchased in the Carpenter **shop**. This location uses the format `Stable Blueprint`, `Deluxe Barn Blueprint`, etc.

To purchase it, you must have the money and materials on you (the materials are listed in the tooltip). For the blueprint location to appear, if it is an upgraded building, you must have the lower-tier version constructed on your farm.

The Starting buildings still have a blueprint location to purchase.

All blueprint locations in the shop are scouted, so you can decide which ones to grind materials for.

#### Price

3 Prices are available
- Normal
- Cheap (50% discount)
- Very Cheap (80% discount)

The price multiplier affects both the price and the materials for every building and blueprint, regardless of shuffled or not.

## Interaction With Other Settings

### Start Without Buildings

Within the setting ~~[Start Without](./start_without.md)~~, is an option to "Start Without: Buildings".

This will allow the player to start without the following buildings on their farm:
- Shipping Bin
- Pet Bowl

Once received, both of these buildings will try to auto-construct themselves on their default tile, if it is free. If not, you can go construct them for free like any other received building.

### Modded Content

Some of the [Supported Mods](../Supported Mods.md) introduce new buildings to the game.

Specifically, the following mods:
- [Tractor Mod](https://www.nexusmods.com/stardewvalley/mods/1401) (Tractor Garage)

The custom buildings behave the same as the vanilla ones, and can be shuffled or not, and discounted or not.

## [Return to Index](./index.md)
