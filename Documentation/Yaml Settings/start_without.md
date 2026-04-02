# Stardew Valley Archipelago - Start Without

## Start Without

This setting allows you to start without certain items that you would normally have at the start or gain access to very quickly.

### Option Values

Setting any of these to "true" will mean you will start without them when you generate a seed. They all are set to "false" by default.
- `Backpack`
- `Buildings`
- `Community Center`
- `Landslide`
- `Tools`

#### Backpack

When this is disabled, you will start with 12 inventory slots, plain and simple.

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

When this is disabled, the [Starting Buildings](./building_progression.md) of the `Shipping Bin` and `Pet Bowl` will be placed on the farm in their default spots from the start.

When this is enabled, the `Shipping Bin` and `Pet Bowl` will not be on the farm until you receive the corresponding `Shipping Bin` and `Pet Bowl` items. Once received, they will try to auto-construct themselves on their default tile, if it is free. If not, you can go construct them for free like any other received building. Also like other buildings, you can build more for their usual cost.

Generation will try to place the `Shipping Bin` in an early location, which usually means Sphere 1. If you don't receive it quickly, someone likely missed a Sphere 1 location, so you should go yell at them to check it.

Marnie will still show up with your pet, even if you haven't received the `Pet Bowl`. However, gaining friendship with them will be slow (you can't water their bowl if it doesn't exist), so gaining friendship to grab their `Pet 1-5 <3` locations won't be in logic until you receive the `Pet Bowl`.

#### Community Center

When this is disabled, you can progress through the beginning Community Center quests as normal:
- Enter Town from the Bus Stop on a non-rainy day after Year 1, Day 5
- Enter the Community Center and check the bundle
- Sleep; receive the Wizard's Invitation in the mail
- Enter the Wizard's Tower

When this is enabled, three new items will be added* to the pool:
- `Community Center Key`: Unlocks the Community Center. The door will be locked until you find this item.
- `Wizard's Invitation`: Unlocks the Wizard's Tower by sending the letter that gives the Meet the Wizard quest to the mailbox. The door will be locked until you find this item.
- `Forest Magic`: Unlocks the ability to see what bundles you have, no matter if you can visit them in person or not.

Some of the requirements for the beginning Community Center quests will be changed:
- Completing the `Quest: Rat Problem` location will now require you to receive either the `Community Center Key`, OR at least one `Progressive Movie Theater` to check the Missing Bundle. Checking your bundles after receiving `Forest Magic` item through the inventory screen will not complete this quest.
- Completing the `Quest: Meet the Wizard` location will now require you to receive the `Wizard's Invitation` (the letter won't appear in the mailbox until you receive this item).

*in reality, they're always in the pool; they're just precollected items if this setting is disabled

#### Landslide

When this is disabled, you must receive the `Landslide Removed` item in order to clear the landslide in the Mountains that blocks the Mines and Adventurer's Guild. You also cannot take a Minecart to the Mines until you receive this item. However, if you receive the `Bridge Repair` and `Minecarts Repair` items before receiving the `Landslide Removed` item, you can take a Minecart to the Quarry and walk to the Mines from there.

When this is enabled, the landslide in the Mountains will be removed from the start, allowing you immediate access to the Mines and Adventurer's Guild. As soon as you receive the `Minecarts Repair` item, you can travel to the Mines via Minecart.

#### Tools

When this is disabled, the Pickaxe, Axe, Watering Can, Hoe, and Scythe will be in your inventory from the beginning, as they would at the start of the vanilla game (unless you've started without a backpack).

When this is enabled, these items will not be in your inventory from the beginning; instead, they will behave like the Fishing Rod and Pan do, which means that they have an extra item in the pool that gives you your basic version of the tool.
There is no equivalent location for these starting tools.

Note that logic doesn't account for random debris spawning on your farm. It is recommended to set the "AllowHandBreaking" setting in the config file located in `\Stardew Valley\mods\StardewArchipelago` to `true` so that you can break rocks/twigs/small trees as they block your path.

## Interaction With Other Settings

### [Entrance Randomization](./entrance_randomization.md)

The following table summarizes entrances with requirements that depend on whether you start with/without certain items:

| Entrance           | Applicable Start Without Setting | Requirement when Starting With | Requirement when Starting Without |
|--------------------|----------------------------------|--------------------------------|-----------------------------------|
| Community Center   | Community Center                 | None                           | Community Center Key              |
| Wizard's Tower     | Community Center                 | None                           | Wizard's Invitation               |
| Mines              | Landslide                        | Landslide Removed              | None                              |
| Adventurer's Guild | Landslide                        | Landslide Removed              | None                              |

Note that these only apply when [Entrance Randomization](./entrance_randomization.md) is set to `Buildings Without House` or `Buildings`.

### [Tool Progression](./tool_progression.md)

If [Tool Progression](./tool_progression.md) is set to `vanilla` while Start Without: Tools is set to "true", then the vanilla Tool Progression setting will override the Start Without Tools setting. After all, starting without tools isn't vanilla tools!

## [Return to Index](./index.md)
