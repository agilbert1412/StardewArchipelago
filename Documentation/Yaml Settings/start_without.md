# Stardew Valley Archipelago - Start Without

## Start Without

This setting allows you to start without certain items that you would normally have at the start or gain access to very quickly.

### Option Values

Enabling any of these will mean you will start without them when you generate a seed. They are all disabled by default.
- `Backpack`
- `Buildings`
- `Community Center`
- `Landslide`
- `Tools`

#### Backpack

When this is enabled, you will start without any inventory slots. You will need to check the mailbox in order to gain access to some inventory slots; the backpacks will always be in the first couple of messages. 

The number of inventory slots you receive will depend on your ~~[Backpack Size](./backpack_size.md)~~ setting - generally, though, you will be given enough backpacks to have at least 6 inventory slots (4 if you start without tools). These amounts have been deemed to be the bare minimum to be able to complete every location check in the game without having to write convoluted inventory logic.
It is possible that, in the future, we eventually write the convoluted inventory logic, and allow the player to start with a true zero-slot backpack.

Here is what you will start with given a specific backpack size:

| Backpack Size | Starting Inventory Slots (With Tools) | Starting Inventory Slots (Without Tools) |
|---------------|---------------------------------------|------------------------------------------|
| 1             | 6                                     | 4                                        |
| 2             | 6                                     | 4                                        |
| 3             | 6                                     | 6                                        |
| 4             | 8                                     | 4                                        |
| 6             | 6                                     | 6                                        |
| 12            | 12                                    | 12                                       |

This will also increase the amount of `Progressive Backpack` items in the pool so that you will end up with 36 inventory slots by the end. Additionally, some `Small Pack` locations will be added to Pierre's General Store to purchase at the start, whose prices will add up to 500g.

If you start with tools, they will be found in gift boxes similar to those that you'd get when enabling the ~~[Quick Start](./quick_start.md)~~ setting.

#### Buildings

When this is enabled, the [Starting Buildings](./building_progression.md#what-are-the-buildings) (`Shipping Bin` and `Pet Bowl`) will not be on the farm until you receive the corresponding `Shipping Bin` and `Pet Bowl` items. Once received, they will try to auto-construct themselves on their default tile, if it is free. If not, you can go construct them for free like any other received building. Also like other buildings, you can build more for their usual cost.

Generation will try to place the `Shipping Bin` in an early location, which usually means Sphere 1. If you don't receive it quickly, someone likely missed a Sphere 1 location.

Marnie will still show up with your pet, even if you haven't received the `Pet Bowl`. However, gaining friendship with them will be slow (you can't water their bowl if it doesn't exist), so gaining friendship to grab their `Pet 1-5 <3` locations won't be in logic until you receive the `Pet Bowl`.

#### Community Center

When this is enabled, three new items will be added* to the pool:
- `Community Center Key`: Unlocks the Community Center. The door will be locked until you find this item. Note that you can still complete `Quest: Rat Problem` by checking the Missing Bundle in JojaMart if you receive a `Progressive Movie Theater` before the `Community Center Key`.
- `Wizard's Invitation`: Unlocks the Wizard's Tower by sending the letter that gives the Meet the Wizard quest to the mailbox. Because of that, the `Wizard's Invitation` is now hard required to complete `Quest: Meet the Wizard`, even with entrances randomized. The door to the Wizard's Tower will be locked until you find this item.
- `Forest Magic`: Unlocks the ability to see what bundles you have, no matter if you can visit them in person or not. Viewing the bundles through the inventory will not complete `Quest: Rat Problem`.

*in reality, they're always in the pool; they're just put in Start Inventory if this setting is disabled

#### Landslide

When this is enabled, you must receive the `Landslide Removed` item in order to clear the landslide in the Mountains that blocks the Mines and Adventurer's Guild. You also cannot take a Minecart to the Mines until you receive this item. However, if you receive the `Bridge Repair` and `Minecarts Repair` items before receiving the `Landslide Removed` item, you can take a Minecart to the Quarry and walk to the Mines from there.

#### Tools

When this is enabled, the Pickaxe, Axe, Watering Can, Hoe, and Scythe items will not be in your inventory from the beginning; instead, they will behave like the Fishing Rod and Pan do, which means that they have an extra item in the pool that gives you your basic version of the tool. Once you find the first one, you will need to check the mailbox to obtain them.
There is no equivalent location for these starting tools.

Note that logic doesn't account for random debris spawning on your farm. It is recommended to set the "AllowHandBreaking" setting in the config file located in `\Stardew Valley\mods\StardewArchipelago` to `true` so that you can break rocks/twigs/small trees as they block your path.

## Interaction With Other Settings

### [Tool Progression](./tool_progression.md)

If [Tool Progression](./tool_progression.md) is set to `vanilla` while Start Without: Tools is set to "true", then the vanilla Tool Progression setting will override the Start Without Tools setting. After all, starting without tools isn't vanilla tools!

## [Return to Index](./index.md)