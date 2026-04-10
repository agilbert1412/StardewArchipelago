# Stardew Valley Archipelago - Backpack Size

## Backpack Size

This setting allows you to choose how many inventory slots each `Progressive Backpack` items gives you.

### Option Values

You can choose from a backpack size of 1, 2, 3, 4, 6, or 12, with 12 being the default value because it corresponds with the vanilla backpack size. These values were chosen because they can cleanly multiply into 36 while still allowing for the vanilla starting amount of 12 to be possible.

### Items

Depending on the backpack size you choose, additional `Progressive Backpack` items will be added to the pool, such that you end up with 36 inventory slots after they all have been obtained. Here is a summary of how many backpacks you can expect to find with each backpack size value:

| Backpack Size | Backpacks in Pool |
|---------------|-------------------|
| 1             | 24                |
| 2             | 12                |
| 3             | 8                 |
| 4             | 6                 |
| 6             | 4                 |
| 12            | 2                 |

### Locations

Additional backpacks will be added to Pierre's General Store if you use a backpack size less than 12. The price for each backpack will total to the amount the original backpack would be worth. For example, the total price of every `Large Pack` upgrade will total 2000g, but split between multiple upgrades, with a growth curve so that every upgrade costs more than the previous one.
You can still only purchase upgrade `N`, after receiving backpack `N-1`

### Disclaimer

In the game, there is a keybind to swap your hotbar with the next row of inventory.
This feature is extremely wonky when used with backpacks of a size that is not a multiple of 12 because it offsets your inventory by 12 slots (1 row), and it will therefore lead to items suddenly switching columns from one cycle to the next.
This is generally considered undesirable, so if you rely on this keybind a lot, smaller backpack sizes might be annoying for you.

## Interaction With Other Settings

### Backpack Progression

When [Backpack Progression](./backpack_progression.md) is set to `Early Progressive`, only one `Progressive Backpack` will be attempted to be placed early. This means that, with a small backpack size, you might not get as much out of this setting as you would with a larger backpack size.

### Start Without

Within the setting [Start Without](./start_without.md), is a setting to "Start Without: Backpack". When this is enabled and the backpack size is set to anything other than 12, some `Small Pack` locations will be added to Pierre's General Store to purchase at the start, whose prices will add up to 500g. Additionally, the number of `Progressive Backpack` items in the pool will be adjusted so that you will still end up with 36 inventory slots after receiving all of them. Starting without tools also affects how many inventory slots you start with, thus further adjusting the amount of backpacks in the pool.

Here is a summary of how `Start Without` interacts with each backpack size:

| Backpack Size | Backpacks in Pool (Without Starting Backpack) | Backpacks in Pool (Without Starting Backpack or Tools) |
|---------------|-----------------------------------------------|--------------------------------------------------------|
| 1             | 30                                            | 32                                                     |
| 2             | 15                                            | 16                                                     |
| 3             | 10                                            | 10                                                     |
| 4             | 7                                             | 8                                                      |
| 6             | 5                                             | 5                                                      |
| 12            | 2                                             | 2                                                      |

Note that starting without tools but with the backpack will not affect the number of `Progressive Backpack` items in the pool.

## [Return to Index](./index.md)
