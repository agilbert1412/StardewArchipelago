# Stardew Valley Archipelago - Backpack Progression

## Backpack Progression

This setting allows you to randomize the backpack upgrades that are usually purchased at Pierre's.

### Option Values

#### Vanilla

The backpacks are not randomized at all. You start with 12 slots, and can purchase 2 more 12-slot upgrades in Pierre's shop.

Excluding Entrance Randomizer shenanigans, it means you essentially have instantly access to your full backpack. As a result, setting backpacks to vanilla is the easiest variation of this setting.

#### Progressive

You will start with 12 backpack slots, and 2 12-slot upgrades are hidden as items in the multiworld, called `Progressive Backpack`.

In Pierre's shop, you can purchase `Large Pack` and `Deluxe Pack`. They use the same prices as vanilla, and each one sends one check. You can only purchase the Deluxe Pack after receiving at least one Progressive Backpack.

#### Early Progressive

This setting behaves the same as `Progressive`, but during the randomizer generation, it will attempt to place at least one `Progressive Backpack` early.

In the context of Archipelago, an Early Location means something that you can do, using only your start inventory and nothing else. In other words, it is something that **can** be done early, but if there are a lot of games or the local game is very big, it is not garanteed that the player will find it early.

Note that this does **not** garantee that the early backpack is local. It can be in anyone's game, just on an early location.

The definition of "Early" is handled by Archipelago core, and out of our hands.

**This is the default value, and it is the recommended experience.**

## 7.x.x Content

### Backpack Size

Through the setting ~~[Backpack Size](./backpack_size.md)~~, the player can customize the size of each backpack upgrade. The default value is 12, like in vanilla.

This means that, instead of having 2 backpack upgrades of 12 slots each, you can have 6 upgrades of 4 slots each, or at the most, 24 upgrades of 1 slot each.

This applies to both items and locations. If the backpack is split into smaller variations, the total price of every `Large Pack` upgrade will total 2000g, but split between multiple upgrades, with a growth curve so that every upgrade costs more than the next one.

You can still only purchase upgrade `N`, after receiving backpack `N-1`

Note that in the game, there is a keybind to swap your hotbar with the next row of inventory.
This feature is extremely wonky, when used with backpacks of a size that is not a multiple of 12, because it offsets your inventory by 12 slots (1 row), and it will therefore lead to items suddenly switching columns from one cycle to the next.
This is generally considered undesirable, so if you rely on this keybind a lot, smaller backpack sizes might be annoying for you.

### Start Without Tools

Within the setting ~~[Tool Progression](./tool_progression.md)~~, is an option to "Start without tools". This includes the backpack.

For logic reasons, the player will still start with at least 4 backpack slots, which is the bare minimum to be able to complete every location check in the game without having to write convoluted inventory logic.
It is possible that, in the future, we eventually write the convoluted inventory logic, and allow the player to start with a true zero-slot backpack.

When starting with fewer backpack slots, more upgrades are added to the pool, so that the maximum total is still 36. The backpack upgrades from slot 1 to 12 are called `Small Pack`, and use a custom price of 500g for the total of all of them.

## [Return to Index](./index.md)
