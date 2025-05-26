# Stardew Valley Archipelago Bundle Randomization

## Bundle Randomization

The Bundle Randomization Setting allows you to customize which bundles will be present in your Community Center. To a lesser degree, it also allows you to customize the Missing Bundle and the Raccoon Requests.

### Vanilla

This setting will use all the vanilla bundles from the vanilla game. These are listed [here](https://stardewvalleywiki.com/Bundles)

The content of the bundles is static, it has no variance. You know exactly what you get.

The Bundles are still affected by [Bundle Price](./bundle_price.md). If you increase the cost of bundles, some of the items will duplicate.

### Thematic

This setting will use all the vanilla bundles, but create a wider range of potential items to fit in them, following their theme.

For example, the `Spring Foraging Bundle`, that usually requires 4 items (Wild Horseradish, Daffodil, Leek and Dandelion), now requires a random selection of 4 items from the 7 forageables that can be found in spring. This includes the original 4, plus Salmonberry, Spring Onion and Morel.

The content of the bundles is written by hand, and might sometimes be slightly subjective. But you should get a good idea of what kind of items might be required, based on the name.

When increasing the [Bundle Price](./bundle_price.md), extra items will get pulled from the pool, until they are all included, and only then will duplicates get rolled.

This also affects the missing bundle and the raccoon requests, even if the latter already had some randomness. Additional items can be part of specific request types.

### Remixed

This setting uses all of the thematic bundles, but also introduces over 50 new bundles. This includes the 17 [Remixed Bundles](https://stardewvalleywiki.com/Remixed_Bundles) present in the vanilla game, and a large number of custom ones

Each Community Center room picks the same number of bundles as it did before, from the larger pool of potential bundles for this room.

Every custom bundle is still assigned a thematically relevant room and can only appear there.

The Missing Bundle and Raccoon Requests are the same in Thematic and Remixed.

### Remixed Anywhere

This setting uses all the bundles from Remixed, but it removes their room assignment. Every bundle can appear anywhere in the community center.

This setting gives the most flexibility to ~~[Bundle Plando](./bundle-plando.md)~~, because it removes a big limitation on how many bundles you can request without hitting the cap on a given room.

This does not affect the Missing Bundle and Raccoon Requests.

### Shuffled

This setting generates a pool containing every potential item from every remixed bundle, and then places them completely randomly in the community center.

A set of remixed bundles is first picked, and then these bundle items are replaced with completely random ones. The bundle prices are maintained, so for example the sticky bundle will still only request one item.

This setting is difficult to keep track of, due to the complete lack of cohesion and theming, so should only be picked by experienced players. Even then, it won't necessarily be fun, and is not the recommended pick for anyone.

The Vault is excluded, and is remixed instead, due to technical limitations.

### Meme

#### This setting is not released yet, and is only present in the 7.x.x development branch.

This setting completely replaces the entirety of the community center with a brand new set of over 90 entirely-custom bundles.

All of these bundles are some sort of joke, troll, or reference to a different media. They are not balanced around a specific difficulty, but instead written to maximize humor (in my opinion).

This means that many of these are trivially easy, and many are very difficult. The only consistent factor is that they are intended to be funny.

Many of these bundles are *Fixed Price* bundles, because the exact prices and amounts are part of the joke, and changing them would ruin it. This means that the [Bundle Price](./bundle_price.md) setting does not affect them.

Still, there is a significant number that are affected by [Bundle Price](./bundle_price.md), and it should be used sparingly.

Some of these bundles, if played on Maximum Prices, will be insanely difficult. I **highly discourage** playing meme bundles on maximum prices. If you do so, it is at your own risk.

## [Return to Index](./index.md)
