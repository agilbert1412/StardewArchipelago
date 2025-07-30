# Stardew Valley Archipelago - Season Randomization

## Season Randomization

This setting allows you to decide on a progression style for your in-game seasons

### General Changes

In Archipelago, at the end of every month, you will get to **pick** the next season. You can repeat the same season indefinitely, or play them in any order you prefer.

This is designed in order to make content more easily reached, once it becomes possible, by requiring the player to sleep only 1 month to go to any season, instead of up to a year like in vanilla.

### Option Values

#### Disabled

The seasons are not randomized at all. Instead, you start in spring, with all seasons immediately available. You will not need to unlock anything.

Warning: Turning Season Randomization off might *feel* like it would make the game easier, but it generally makes it **harder**. This severely inflates your early spheres, by having 4 times more locations immediately available.
If you are playing alone, this means you will have more things to do, and therefore more trouble finding the few critical progression items that would allow you to move on to the next spheres.
If you are playing in a multiworld, this means you will have a significantly heavier burden at the beginning, with 4 times as many locations that others need you to do, to get their items. It means you will be significantly more likely to hold up the multiworld.

#### Randomized

You will be assigned one random starting season, and the other 3 are random unlocks to be found in the multiworld.

This is the default value, and it is the recommended experience.

#### Randomized Not Winter

Same as "Randomized", but you are garanteed to start in Spring, Summer or Fall. This is slightly easier, because Winter starts can be difficult in SVAP, due to money being harder to earn in Winter.

#### Progressive

This option will start you in spring, and generate 3 `Progressive Season` unlocks in the multiworld. When obtained, they will unlock the 3 remaining seasons in the vanilla order (Summer, then Fall, then Winter).

This leads to a randomized experience with proper hard locks, but more predictable and easier to plan ahead for. This is probably the easiest Season Randomization setting.

### Special Behavior

If you are playing on `Randomized` or `Randomized Not Winter`, you can place one season in your ~~[Start Inventory](./start_inventory.md)~~. If the generator notices that you are already starting with at least one season, it will **not** assign you a random starting season, and instead let it be the way you chose.

This allows you to start in the season of your preference, yet still randomize the other 3.

You can also start with more than one season. The game will start in the one you wrote first in your yaml, and the others will be immediately unlocked.

## [Return to Index](./index.md)
