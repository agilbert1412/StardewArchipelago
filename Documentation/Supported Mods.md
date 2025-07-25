# Stardew Valley Archipelago Supported Mods Setup Guide

## Supported Mods

Supported mods are a set of mods that have been integrated into the Stardew Valley Archipelago implementation. This means that these mods are considered and included in relevant Archipelago features.
For example, mods that add regions will be included in the Entrance Randomizer, mods that add NPCs will be included in friendsanity, mods that add skills will be considered when shuffling skill levels.
Some of these mods also include completely unique checks and items based on the mod's content.

When a mod is supported, only a very specific version of that mod has been evaluated and tested for compatibility. We do not have control over future changes to mods, so players must use that specific mod version to play their Archipelago slot.
A reasonable effort will be made to stay up to date, but it has to be understood that this does not garantee a given timeline for supporting a new version of a mod, especially when content is added.

## What mods are supported?

The following mods are currently supported:

General: 
- [Stardew Valley Expanded](#stardew-valley-expanded-specific-instructions), by FlashShifter, Version 1.14.46 **Requires additional installation steps and patch files**
- [Skull Cavern Elevator](https://www.nexusmods.com/stardewvalley/mods/963), by Lestoph, Version 1.6.x
- [Bigger Backpack](https://www.nexusmods.com/stardewvalley/mods/1845), by spacechase0, Version 7.3.x
- [Tractor Mod](https://www.nexusmods.com/stardewvalley/mods/1401), by Pathoschild, Version 4.23.x
- [Distant Lands - Witch Swamp Overhaul](https://www.nexusmods.com/stardewvalley/mods/18109), by Aimon111, Version 2.2.x

Skills:
- [Spacecore Luck Skill](https://www.nexusmods.com/stardewvalley/mods/28103), by spacechase0, Version 1.3.x - Note: This mod was ported from the now deprecated "Luck Skill", and has added some new content that isn't fully randomized. But it should work like before
- [Socializing Skill](https://www.nexusmods.com/stardewvalley/mods/14142), by drbirbdev, Version 2.1.x
- [Archaeology](https://www.nexusmods.com/stardewvalley/mods/22199), by MoonSlime, Version 2.12.1
- [Binning Skill](https://www.nexusmods.com/stardewvalley/mods/14073), by drbirbdev, Version 2.1.x

Custom NPCs:
- [Juna - Roommate NPC](https://www.nexusmods.com/stardewvalley/mods/8606), by elhrvy 2.2.x
- [Professor Jasper Thomas](https://www.nexusmods.com/stardewvalley/mods/5599), by Lemurkat 1.8.3 (including additional [patch](https://github.com/Witchybun/SDV-Randomizer-Content-Patcher/releases))
- [Alec Revisited](https://www.nexusmods.com/stardewvalley/mods/10697), by HopeWasHere and SoftPandaBoi, Version 2.2.x
- [Mister Ginger (cat npc)](https://www.nexusmods.com/stardewvalley/mods/5295), by Lemurkat, Version 1.6.x
- [Ayeisha - The Postal Worker (Custom NPC)](https://www.nexusmods.com/stardewvalley/mods/6427), by TheLimeyDragon, Version 0.7.x
- [Alecto the Witch](https://www.nexusmods.com/stardewvalley/mods/10671), by zoedoll, Version 1.1.x (including additional [patch](https://github.com/Witchybun/SDV-Randomizer-Content-Patcher/releases))
- [Hat Mouse Lacey](https://github.com/ichortower/HatMouseLacey) by khortower, Version 1.4.x

In addition, a [Content Patcher mod](https://github.com/Witchybun/SDV-Randomizer-Content-Patcher/releases) may be required for the mod to properly function alongside your game.  Include these alongside StardewArchipelago.
"x" is used to denote a wildcard digit, that is flexibleand allows for variations. We assume that supported mods will not break on patch updates, and if they do, we may have to be more strict in the future.

Here is what you should see in SMAPI, when installing a mod that requires an additional patch. This example is Stardew Valley Expanded, which has two additional patches that are needed to be loaded properly.
![image](https://i.imgur.com/y3lBIkw.png)

## How to add supported mods to my Archipelago slot?

To include supported mods in your multiworld slot, you need to include a section in your yaml settings called "mods".
This section must be an array with the **exact** names of every mod you wish to include. Any improperly typed mod name will be ignored.
![image](https://i.imgur.com/uOHtXmU.png)

These mods will then be included in the multiworld generation, and considered in logic. For example, the Magic mod includes a spell that allow a player to teleport, and, if included, teleporting can be required to reach checks.

As previously mentioned, the generator and the StardewArchipelago client are designed and tested for a very specific version of any supported mod. When installing them, you must choose the correct version, or you will not be able to play.
The StardewArchipelago mod will inform you if some of your supported mods have the wrong version, and tell you what version you need.

Most mods also have dependencies to some library/utility mods. You will need to install these as well, but dependency versions are less strict.

If you can load the supported mod on the correct version, the exact version of a dependency is not important.

## Having trouble with the YAML syntax?

Alternatively, the [Advanced Options Page](https://archipelago.gg/games/Stardew%20Valley/weighted-options) is another option available to add mods to your yaml file.

You can pick **all** of your settings from this page, if you wish, or you can follow the instructions below to get a pre-generated code block for just your mods, to add to another yaml file.

- Click the link. Next, scroll down to the secton titled **Mods**. There, you will find a series of checkboxes.
- Check the checkboxes beside the name of the mods that you'd like to include in your yaml file.
- Click the **export settings** button at the bottom of the page, which will download the yaml file to your computer.
- Open that yaml file in your preferred text editor, and look for the section titled **Mods**.
- Select that entire section, and copy it to your clipboard.
- If you haven't already done so, open your yaml file that you'd like to include the mods in, and paste what you've just copied over into it.
- Make sure that you select over the pre-existing mods category that is included by default by the website.
- This is the most important step. Ensure you **save** the file to wherever you'd like to save it to. 
- For good measure, open the saved file again to make sure that it saved properly. You've now added mods to your yaml file! 

## Stardew Valley Expanded Specific Instructions

1) Download [Stardew Valley Expanded](https://www.nexusmods.com/stardewvalley/mods/3753) Version **1.14.46** and all dependencies.
2) Extract them all into your mods folder.
3) Download these [two additional patches](https://github.com/Witchybun/SDV-Randomizer-Content-Patcher/releases) `StardewAP - SVE Patch` and `StardewValleyExpanded 1.14.x-archipelago`.
     - The first patch fixes some Content patcher things to be compatible with Archipelago, and the 2nd patches allows the SVE 1.14.46 code to be loaded into Stardew Valley 1.6.9+). Once patched, the version number in SMAPI will display as `Stardew Valley Expanded 1.14.31-archipelago by FlashShifter, Esca, super-aardvark, and kittycatcasey. Updated for Stardew 1.6.9 by Kaito Kid`. This is because I had to base my patch off of the 1.14.31 code, since the 1.14.46 code was not available. The Content pack for it will maintain its 1.14.46 displayed version number
4) Extract the patch file folders directly into your mods folder, so that the Stardew Valley Expanded folder is getting partially overwritten.
      **NOTE**: if your operating system did not prompt you to overwrite some files, you have likely installed SVE or the patch files incorrectly. Common errors include: not extracting the mods from their zip files, and installing the mods in the wrong folder.
6) You should now have the Stardew Valley Expanded and StardewAP - SVE Patch folders in your Mods folder. If your yaml settings were also correctly set up in the above sections, you should be ready to play with SVE.

### What your Mods folder would look like at minimum if correctly installed
![Minimum mods needed to run SVE with SDV](/Documentation/Images/SVE%20Min%20Mods.png?raw=true)

## Weren't there more mods supported?

The addition of 1.6 caused a bit of strain for every mod developer, and so not every mod was updated to be compatible with 1.6.

The following mods have technical mod support, but are temporarily deprecated as we wait for them to update.  Do not use them for now:
- [Boarding House and Bus Stop Extension](https://www.nexusmods.com/stardewvalley/mods/4120) by TrentXV, Version 4.0.16
- [DeepWoods](https://www.nexusmods.com/stardewvalley/mods/2571), by Max Vollmer, Version 3.1.0-beta
- [Magic](https://www.nexusmods.com/stardewvalley/mods/2007), by spacechase0, Version 0.8.2
- [Cooking Skill](https://www.nexusmods.com/stardewvalley/mods/522), by spacechase0, Version 1.4.5
- [Custom NPC - Riley](https://www.nexusmods.com/stardewvalley/mods/5811), by SG, Version 1.2.2
- [Custom NPC - Yoba](https://www.nexusmods.com/stardewvalley/mods/14871), by Jerem, Version 1.0.0
- [Custom NPC Eugene](https://www.nexusmods.com/stardewvalley/mods/9222), by Leroy and translated by Sapiescent, Version 1.3.1
- ['Prophet' Wellwick](https://www.nexusmods.com/stardewvalley/mods/6462), by Jyangnam, Version 1.0.0
- [Shiko - New Custom NPC](https://www.nexusmods.com/stardewvalley/mods/3732), by Papaya, Version 1.1.0
- [Delores - Custom NPC](https://www.nexusmods.com/stardewvalley/mods/5510), by blaaze6 and Obamoose, Version 1.1.2

## Bug Reports

Before reporting a bug, make sure to follow these steps
- Uninstall all unecessary mods from your save file, and verify that your bug still happens. If it doesn't, then it might be a conflict with unsupported mods
- Verify if there is a new version of StardewArchipelago available on the [releases page](https://github.com/agilbert1412/StardewArchipelago/releases). Your bug might already be fixed
- Check your SMAPI console when the bug happens. Keep a eye out for anything red, and take screenshots.

To report the bug, you can use the github bug [report page](https://github.com/agilbert1412/StardewArchipelago/issues/new), or head over to the [Archipelago Discord server](https://discord.gg/8Z65BR2) to use the bug reports channel.
You can also simply head to the Stardew Valley channel in the Archipelago Discord to talk about the issue with people and find a workaround.

## I think a certain mod would make a great addition to AP, who should I talk to?

First, make sure you join the [Archipelago Discord server](https://discord.gg/8Z65BR2)

Currently Witchybun works on mod support, but such mod support is always open to the public to be discussed in 
the Discord server to be implemented and is not particularly gatekept by them in particular.  However for the record, the 
following mods are in the works to be supported:
- Ridgeside Village
- East Scarp

Other mods are also being considered which are smaller in scope, but are too numerous to be listed here.  These can be suggested either in #stardew-valley or its thread "Mod Support".
