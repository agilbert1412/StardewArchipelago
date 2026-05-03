# Stardew Valley Archipelago - Bundles Per Room

## Bundles Per Room

The Bundles Per Room setting allows you to customize the number of bundles that will generate as part of your community center.

This setting, along with [Bundle Price](./bundle_price.md), has a massive impact on the difficulty and duration of a slot with the [Community Center](./goal.md#community-center) goal.

A very low number of bundles can make a slot very short, and a very high number can make it very long.

This setting can compound with [Bundle Price](./bundle_price.md) in both directions, or they can make for an interesting balanced run if set up to counter each other, such as a large number of extremely cheap bundles.

### Number of bundles

In a default community center, there are exactly 30 bundles, distributed like this:

Pantry: 6 Bundles
Crafts Room: 6 Bundles
Fish Tank: 6 Bundles
Boiler Room: 3 Bundles
Vault: 4 Bundles
Bulletin Board: 5 Bundles

### Relative Change

The Bundles Per Room setting functions based on a relative calculation. The available values are [-2, -1, 0, +1, +2, +3, +4].

This number will be applied on every individual room. This means that a value of -2, means a reduction of 2 bundles per room, which leads to a total reduction of 12 bundles. Your final count would be 18, with only one single bundle in the boiler room.

The highest value is +4, which leads to a total of 54 bundles in the community center, several rooms would have 10 bundles.

### Capped Amounts

Some bundle rooms can sometimes not have enough bundle candidates to fulfill a high Bundles Per Room value.

For example, in [Bundle Randomization: Vanilla or Thematic](./bundle_randomization.md), there are no extra bundles available to fill the slots, so high values would not increase the numbers in the Community Center rooms.

### Bundles outside of the Community Center

#### Abandoned JojaMart

Unaffected by this setting. There is always exactly one Missing Bundle.

#### Raccoons

Affected by this setting, but only downwards. There are 8 bundles by default, it can be reduced to 6, but cannot be increased higher than 8

#### Trash Bear

While not technically bundles, the Trash Bear requests are affected by this setting, similar to how they are affected by [Bundle Price](./bundle_price.md).

By default, the Trash Bear has 2 categories of requests, Foraging and Cooking. Each category can request multiple items.

If Bundles Per Room are reduced, the Cooking category and its associated location is removed, leaving only the Foraging Category.

If Bundles Per room are increased by 1, an additional Farming Category is added, requesting specific crops.

If Bundles Per Room are increased by 2 or more, a fourth category is added, Fishing, requesting specific fish.

As a result, a Trash Bear made as small as possible through both this setting and [Bundle Price](./bundle_price.md), would have one foraging request of 1 item.

A Trash Bear made as big as possible through both this setting and [Bundle Price](./bundle_price.md), would have 4 requests of 4 items.

Requests are displayed as rows, with each item on a row part of that same request. They can be progressed and completed in any order

![Maxed out Trash Bear](https://i.imgur.com/15UsDrN.png)

![The same Trash Bear after donating some of the items](https://i.imgur.com/516qNqz.png)

## [Return to Index](./index.md)
