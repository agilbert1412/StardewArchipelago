# Stardew Valley Archipelago - Tool Progression

## Tool Progression

This setting allows you to randomize the tool upgrades for all your tools

### What are the tools?

There are 4 standard tools. You start with them, they are unloseable, and can be upgraded at the Blacksmith in this order: 

Basic -> Copper -> Steel -> Gold -> Iridium

- Pickaxe
- Axe
- Hoe
- Watering Can

There are also 4 special tools, which are obtained and upgraded differently

- Fishing Rod: The first one is obtained in a cutscene, the next 3 purchased at Willy's shop after reaching a certain fishing level, and the last one is part of [Fishing Mastery](https://stardewvalleywiki.com/Mastery_Cave#Masteries)
- Scythe: You start with a [Basic](https://stardewvalleywiki.com/Scythe) one, obtain a [Gold](https://stardewvalleywiki.com/Golden_Scythe) one in the [Quarry Mine](https://stardewvalleywiki.com/Quarry_Mine), and obtain an [Iridium](https://stardewvalleywiki.com/Iridium_Scythe) one as part of [Farming Mastery](https://stardewvalleywiki.com/Mastery_Cave#Masteries)
- Pan: You start with none. The Copper Pan is obtained through a cutscene after the Glittering Boulder is removed, then it can be upgraded up to iridium at the Blacksmith.
- Trash Can: This tool is not in your inventory, it's in your menu and doesn't take up space. It is upgraded at the Blacksmith normally

### Option Values

This option works in two parts. First, what is shuffled, and second, what price multiplier should be added on to it.

Here are the 6 possible values
- `vanilla`
- `progresssive` **This is the default value, and it is the recommended experience.**
- `vanilla_cheap`
- `progresssive_cheap`
- `vanilla_very_cheap`
- `progresssive_very_cheap`

#### Vanilla

The tools are not shuffled at all. You obtain them all normally, then upgrade them yourself when applicable.

#### Progressive

You will get the normal starting tools (Pickaxe, Axe, Hoe, Watering Can, Scythe and Trash Can).

All tool upgrades are received as item unlocks.

A tool upgrade looks like this: `Progressive Pickaxe`. It has several copies in the item pool, and the order you receive them in doesn't matter. Receiving one always gives you a single bump to the upgrade level of that tool.

For the tools you do not start with (Fishing Rod, Pan), the very first `Progressive X` you get will spawn the tool in your inventory at the lowest level, then subsequent ones are upgrades.

For each tool item in your pool, there is an equivalent location to check in the world. Most of them are purchased tool upgrades in shops, but a few are special
- Grim Reaper Statue (for the golden scythe)
- Bamboo Pole Cutscene (for the bamboo pole)
- Copper Pan Cutscene (for the copper pan)

The two tools that come from masteries are not shuffled if you didn't shuffle Masteries as part of ~~[Skill Progression](./skill_progression.md)~~. If they are, they do not have equivalent locations.

#### Price

3 Prices are available
- Normal
- Cheap (60% discount)
- Very Cheap (80% discount)

The price multiplier affects everything involved with a tool obtention, when possible to do so.
- Money price of upgrades at the Blacksmith
- Number of metal bars required for upgrades at the Blacksmith
- Money price of fishing rods at Willy's Shop

It does not affect obtentions that aren't a purchase, like a Grim Reaper Statue or a Mastery.

If tools are shuffled, the discount applies to the Location, not the Item.

## 7.x.x Content

### Start Without Tools

Within the setting ~~[Start Without](./start_without.md)~~, is an option to "Start Without: Tools".

This will allow the player to start without the following tools:
- Pickaxe
- Axe
- Watering Can
- Hoe
- Scythe

These now behave the same as the Fishing Rod and Pan do, which means that they have an extra item in the pool that gives you your basic version of the tool.
There is no equivalent location for these starting tools.

## [Return to Index](./index.md)
