# Stardew Valley Archipelago - Cropsanity

## Cropsanity

This setting allows you to randomize the unlocking and harvesting of crops and seeds

### Option Values

#### Disabled

The crops and seeds are not randomized at all. You start with every crop unlocked and available to grow. Growing the crops does not give anything.

Warning: Turning Cropsanity off might *feel* like it would make the game easier, but it generally makes it **harder**. This severely inflates your early spheres, by having significantly more items available to complete locations with.
If you are playing alone, this means you will have more things to do, and therefore more trouble finding the few critical progression items that would allow you to move on to the next spheres.
If you are playing in a multiworld, this means you will have a significantly heavier burden at the beginning, with many more locations that others need you to do, to get their items. It means you will be significantly more likely to hold up the multiworld.

#### Enabled

You will start with no seeds and crops unlocked.

Each seed will come as an item unlock from the multiworld. Upon receiving a seed unlock, it will come with a free sample of 1, and will unlock the ability to purchase more, at the relevant shop(s).
Growing every crop will give you one Harvest location check, which means that for each seed you receive, it should be grown at least once, and perhaps more for use in other areas of the game.

This includes
- Normal crops (Sold at Pierre and Sandy)
- Fruit trees and Tea saplings (Unlock the sapling as an item, harvest the fruit as a location)
- Special seeds sold in shops (Ex: Rare seeds, sold at the Traveling Cart)
- Special seeds found elsewhere (Ex: Ancient Seeds, Raccoon Seeds, etc)
- Foreageable items that are also a crop (Grapes)

This does not include
- Forageable items that are found in the wild (Salmonberries, Spring onions)
- Foreageable items that can be grown (Dandelion, Crocus)
- Grown items that do not produce a crop (Grass Starters, Fiber Seeds)

**This is the default value, and it is the recommended experience.**

## Related Mod Configs

### Seed Shop Overhaul

Archipelago is balanced around a Seed Shops overhaul that comes bundled with the mod by default.

This overhaul can be turned off from the mod config, at any time, if the player prefers the vanilla experience.

#### Overhaul Changes

- Pierre and Sandy now have a limited stock of seeds, which refreshes daily. The limited stock is semi-random, from 0 to 20, but any amount that rolls under 5 is instead removed for the day.
- Jojamart sells unlimited seeds, and offers a significant 20% discount to all seeds. They also sell every seed in every season.
- Jojamart only sells seeds in bulk.
  - Most seeds are sold in large packs of 50 at a time
  - Ingredients (flour, sugar, etc) are sold in stacks of 20
  - Rare seeds are sold in stacks of 10
  - Joja colas are sold in packs of 6
- Upon finding Pierre's missing stocklist, both Pierre and Sandy's stock will double permanently, in addition to Pierre selling out of season seeds.
- Upon receiving a Progressive Movie Theater, Pierre and Sandy's stock will also double permanently, twice (once for each theater).
- This means that, once all 3 of these conditions are met, the stock for their seeds will be a random number from 0 to 160, with again stocks under 5 being missing for the day.

#### Overhaul reasons

This overhaul has been done for several purposes. The main one, is to subtly nudge players, especially beginners, into a playstyle that fits Archipelago better. Vanilla tends to encourage doing a lot of farming, of just one very-profitable crop, as a source of money and experience.
In Archipelago, we instead want to encourage players to grow a little bit of everything, for the purpose of checking cropsanity locations, and also having a supply of most crops, as they will often be needed for other settings, as well as cropsanity.
Examples include ~~[Quest Locations](./quest_locations.md)~~, ~~[Shipsanity](./shipsanity.md)~~ and ~~[Cooksanity](./cooksanity.md)~~.
While making a lot of money is always helpful, it is not the best use of your time, in the early game of a Stardew Valley slot. It is generally better to do a little bit of everything, rather than focus one setting and clear it out entirely. For example, it will be better for the multiworld progression if you send the first 2 level up locations for each 5 skill, rather than maxing out farming to level 10 right away.

This overhaul makes any given type of seed limited in amount, and encourages the player to buy many types, instead of one. Even then, they will be unable to fill out large patches of farmland, like they would do in vanilla. This will also encourage them to not waste a massive amount of time watering every morning, and instead they will tend to use their energy on other tasks, again progressing in a more efficient way.

The secondary reason for this overhaul is lore. In vanilla, Joja is explicitly stated to offer better prices and services than Pierre, but then in gameplay, they are not. They offer the exact same thing, and there is really no dilemma or meaningful choice to be made.
With this overhaul, Joja offers a clearly better deal, with lower prices, better opening hours, and out-of-season purchases. The downside, which helps give Pierre a slight advantage, is being forced to mass-purchase. This also helps players who do decide to go for the maximized profits playstyle, which matches Joja (and the Joja route) much better thematically.
This also fits the fact that large corporations can afford to stock seemingly endless products, while Pierre cannot, as he cannot buy in bulk, and cannot afford to have a lot of unsold stock.
When Joja closes down, and Pierre gets a monopoly, he then starts stocking more seeds, to match the higher demand (due to reduced supply).

### Strict Logic

Strict Logic is a mod config that disables some obtention methods for some items, that would not be taken into account by logic rules.

It defaults to **On**, because it helps streamline players into their intended progression path, instead of straying far out of logic by accident, and potentially negatively impacting their experience or that of others.

#### Interaction with Cropsanity

All the following interactions do not apply, if Cropsanity is turned off, as all seeds and crops immediately qualify as "unlocked".

- Seed spots will only yield their intended seed, if the seed is unlocked. Else, they will yield mixed seeds
- The Raccoon shop will only sell the seasonal raccoon seeds, if it is unlocked. Else, it will sell mixed seeds.
- Mixed Seeds and Mixed flower seeds will turn into a random unlocked crop, instead of turning into one of very few, pre-defined crops, like they do in vanilla.
  - Mixed seeds can always turn into seasonal forage seeds. If no crops are unlocked for the current planting place/season, they will always turn into forage seeds.
  - Mixed seeds can turn into regrowing crops (Cranberries, blueberries, etc), but these crops are 1/10th as likely as normal crops to roll, so they are rare.
  - Mixed seeds can turn into Special high-value crops (Sweet Gem Berry, Ancient Fruit), but these crops are 1/100th as likely as normal crops to roll, so they are extremely rare.
  - Flower seeds can have zero candidates, and if that is the case, the game will not let the player plant them at all.
- Mystery Boxes and prize tickets cannot yield specific seeds, if the seed is not unlocked yet.

## [Return to Index](./index.md)
