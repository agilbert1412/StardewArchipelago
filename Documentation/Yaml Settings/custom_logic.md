# Stardew Valley Archipelago - Custom Logic

## Custom Logic

This setting allows you to alter the logic used for your slot. This changes what checks Archipelago expects you to be willing and able to complete based on the items that you've received. It does not affect your ability to do anything in-game in any way.

For example, under the default settings you won't be expected to catch a Lava Eel until you reach level 8 Fishing. There's nothing stopping you from catching one at level 7 Fishing, but doing so would be considered out-of-logic.

All of the following settings are disabled by default. 

### Chair Skips

A chair skip is when you bypass an obstacle by placing a chair somewhere you cannot normally get to and sitting in it. By default, you are never expected to do this. When your custom logic includes Chair Skips, you are expected to be able to reach the following locations using a chair skip:
- The entrance to the [Secret Woods](https://stardewvalleywiki.com/Secret_Woods) from Cindersap Forest. This normally requires a steel or better axe, ~~or the "Blink" ability from the [Magic Mod](<./Supported Mods.md#what-mods-are-supported>)~~.
- The [Dig Site](https://stardewvalleywiki.com/Ginger_Island#Dig_Site) in Ginger Island North. This normally requires receiving the "Dig Site Bridge" item.

These locations can be reached once you can buy a chair from Robin for 350g. If you've enabled "Critical Free Samples" (see below), you can also logically reach them using the chair in the Standard, Riverland, Forest, or Beach Farm. 

### Fishing Logic

Every fish in the game has a difficulty level, which can be seen at [the wiki](https://stardewvalleywiki.com/Fish#Fish_Types). By default, you aren't expected to catch a fish until your fishing level is at least $(\text{difficulty}/10)-1$, rounded down. For example, a Catfish has a difficulty of 75, so you aren't expected to catch one until you reach Fishing 6. 

The following settings allow you to adjust what fishing level you are expected to be at before you can catch any fish. These settings do not stack; only the hardest difficulty chosen will take effect.
- Easy Fishing: +2 (so you're not expected to catch a Catfish until Fishing 8). This doesn't affect Carp, which can always be caught at any level. This also caps at Fishing 10, at which point even the hardest fish are in logic. 
- Hard Fishing: -2 (so you're expected to catch a Catfish at Fishing 4)
- Extreme Fishing: -4 (so you're expected to catch a Catfish at Fishing 2)

Note that these settings don't affect other logical requirements for fish, including the in-game minimum levels for legendary fish (both this minimum and the above level requirement take effect). They also don't affect what fishing rods are required (Bamboo Pole for difficulty 50+ fish, Iridium Pole for difficulty 80+ fish).

### Mining Logic

The mining difficulty options affect the mining level and pickaxe you are expected to have for exploring the different floors of the Mines and Skull Cavern. By default, the following logic is used:

| Floors                | Pickaxe Needed | Mining Level Needed |
|-----------------------|----------------|---------------------|
| Mines 1-40            | Basic          | 0                   |
| Mines 41-80           | Copper         | 2                   |
| Mines 81-120          | Steel          | 4                   |
| Skull Cavern 1-50     | Copper         | 6                   |
| Skull Cavern 51-100   | Steel          | 8                   |
| Skull Cavern 101-150  | Gold           | 10                  |
| Skull Cavern 151-200  | Iridium        | 10                  |

This logic for which floors can be explored is changed using the following settings. These settings do not stack; only the hardest difficulty chosen will take effect.
- Easy Mining: Each floor needs at least a pickaxe that is one tier better, and +2 mining level (to a maximum of 10). Note that this setting only applies if you have [progressive tools](./tool_progression.md) on (otherwise you couldn't get the resources for your required pickaxe).
- Hard Mining: Each floor needs at least a pickaxe that is one tier worse (to a minimum of Basic), and -2 mining level (to a minimum of 0 in the Mines, 6 in the Skull Cavern).
- Extreme Mining: Each floor needs at least a pickaxe that is two tiers worse (to a minimum of Basic), and -4 mining level (to a minimum of 0 in the Mines, 6 in the Skull Cavern).

Note that these settings only affect which levels you are expected to progress through and collect resources from. They do not affect how far you're expected to descend from your lowest elevator (see Mining Depth below).

### Combat Logic

The combat difficulty options affect the combat level and weapon tier you are expected to have for exploring the different floors of the Mines and Skull Cavern. By default, the following logic is used:

| Floors                | Weapon Tier Needed | Combat Level Needed |
|-----------------------|--------------------|---------------------|
| Mines 1-40            | Basic              | 0                   |
| Mines 41-80           | Decent             | 2                   |
| Mines 81-120          | Good               | 4                   |
| Skull Cavern 1-50     | Great              | 6                   |
| Skull Cavern 51-100   | Great              | 8                   |
| Skull Cavern 101-150  | Great              | 10                  |
| Skull Cavern 151-200  | Great              | 10                  |

The weapon tiers are determined using the Progressive Sword/Dagger/Club items, with the tiers being Basic, Decent, Good, Great, and Galaxy (and someday Ascended). For example, your second Progressive Sword will give you a Decent Sword. 

~~If you are playing with the [Magic Mod](<./Supported Mods.md#what-mods-are-supported>), offensive spells also count as weapons for this setting. As long as you can use the Altar, you have Basic Spells. If you have Magic 2 and at least one damaging spell (fireball, frostbite, shockwave, spirit, or meteor), you have Decent Spells. For each higher tier, you need an extra damaging spell and 2 magic levels, and you need to have at least one support spell (descend, heal, or tendrils). For example, Great Spells require 3 damaging spells, one support spell, and Magic 6. Note that these magic level requirements don't replace the combat level requirements for each floor.~~

This logic for which floors can be explored is changed using the following settings. These settings do not stack; only the hardest difficulty chosen will take effect.
- Easy Combat: Each floor needs at least a weapon that is one tier better, and +2 combat level (to a maximum of 10). 
- Hard Combat: Each floor needs at least a weapon that is one tier worse (to a minimum of Basic), and -2 combat level (to a minimum of 0 in the Mines, 6 in the Skull Cavern).
- Extreme Combat: Each floor needs at least a weapon that is two tiers worse (to a minimum of Basic), and -4 combat level (to a minimum of 0 in the Mines, 6 in the Skull Cavern).

Note that these settings only affect which levels you are expected to progress through and collect resources from. They do not affect how far you're expected to descend from your lowest elevator (see Mining Depth below).

### Mining Depth Logic

These settings change how far away from your latest elevator you're expected to travel in the Mines. By default, you are assumed to only be able to descend 10 floors below your lowest elevator in the Mines. If you are playing with the [Skull Cavern Elevator Mod](<./Supported Mods.md#what-mods-are-supported>), you're also only expected to descend 25 floors beyond your latest Skull Cavern Elevator (if you don't have the mod, you are expected to dive arbitrarily deep, assuming you have good enough tools and skills).

This logic for which floors can be reached is changed using the following settings. These settings do not stack; only the hardest difficulty chosen will take effect.
- Deep Mining: You're expected to descend 20 floors past your latest elevator in the Mines (50 floors in Skull Cavern, if using the mod).
- Very Deep Mining: You're expected to descend 40 floors past your latest elevator in the Mines (100 floors in Skull Cavern, if using the mod).

### Ignore Birthdays

By default, you are not expected to earn any hearts with villagers until you unlock their birthday. If a villager doesn't have a birthday (such as ~~Apples in [SVE](<./Supported Mods.md#what-mods-are-supported>)~~), you instead need to unlock every season or reach year 3, whichever comes first.

The "Ignore Birthdays" setting disables this behavior. This means that as soon as you can meet a villager, you can logically become maximum hearts with them. This can greatly increase the number of checks available at the start of a run if you are using ~~[Friendsanity](./friendsanity)~~ or ~~[Chefsanity](./chefsanity)~~ (with friendship recipes randomized). 

### Money Logic

In order to This is relevant for checks that require reaching certain amounts of total earnings (such as Grandpa's Evaluation, or unlocking the Farm Cave). You are also assumed to be able to spend 1/5th of your total earnings on any given purchases. For example, a purchase of 1,000g won't be in logic until you're expected to earn 5,000g. 

By default, Archipelago assumes that your total money earned can reach the following amounts of money under the subsequent conditions:
- 1,000g: Always reachable
- 2,000g: Can obtain and sell fish, forageables, copper ore, or hardwood, or can use the shipping bin
- 3,000g: Can obtain and sell fish or forageables, or can use the shipping bin
- 5,000g: Can obtain and sell each of fish/foreagables/copper ore/hardwood, or can grow and sell crops, or can use the shipping bin
- 10,000g: Can grow and ship crops
- 40,000g: Can buy seeds from Pierre, and grow and ship crops
- Larger amounts: If you can buy seeds from Pierre and grow and ship crops, then you are assumed to be able to earn 20,000g multiplied by the percentage of progression items that your slot has received. For example, after receiving 2.5% of your progression items, you should be able to earn a total of 50,000g. Once you reach 90% of progression items, you are assumed to be able to make as much money as you need.

The following settings multiply the total earnings you're expected to make at any given time. For example, if using "Easy Money", Archipelago assumes that you can only earn 2,500g if you can grow and ship crops, but can't buy seeds from Pierre. If you can also buy seeds from Pierre and you've received 2.5% of your progression items, you're assumed to be able to earn 12,500g. These settings do not stack; only the hardest difficulty chosen will take effect.
- Easy Money: x0.25 
- Hard Money: x2
- Extreme Money: x8
- Nightmare Money: x20

### Bomb Hoeing

By default, you are assumed to need a Hoe in order to grow crops. The "Bomb Hoeing" setting assumes you can use bombs to till the ground instead, as long as you can logically get them. This setting only changes the seed's logic if you [start without tools](./start_without.md). 

### Rain Watering

By default, you are assumed to need a Watering Can in order to grow crops. The "Rain Watering" setting assumes you can use naturally-occuring rain to water your crops instead, as long as you can plant them on the farm in the right season (or plant them on Ginger Island). This setting only changes the seed's logic if you [start without tools](./start_without.md). 

~~Note that if you're using the [Magic Mod](<./Supported Mods.md#what-mods-are-supported>), you could be expected to water your plants using the Water spell instead of a Watering Can, even without "Rain Watering".~~

### Critical Free Samples

This setting puts checks into logic even if you only have access to a single copy of a relevant item. This affects two things:
- If using [Cropsanity](./cropsanity.md), the free seed you get for each crop will be enough to put that crop into logic, even if you don't have a way to get more seeds.
- If using "Chair Skip" (see above), the chair that starts in most farms will be enough to perform chair skips, even if you can't buy more from Robin.

Using this setting can lead to gameplay issues if you lose your free item (or need more than one of a crop) and you don't have a way to get more of it. This could be due to crows, or passing out in the Mines, or accidentally shipping/gifting it, or having the Quality Crops Bundle, or any other reason. In order to recover, more random and grindy gameplay could be needed than it ordinarily assumes (for example, using wild seeds to get more of a crop, or the Trash Catalogue to get a chair). In especially restrictive seeds, an additional copy might not be obtainable without using [Gifting](./gifting.md) or cheats (such as the /send or !getitem commands).
