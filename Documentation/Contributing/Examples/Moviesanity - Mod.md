# Implementing Moviesanity - Mod Side

## Introduction

This document intends to do a step by step explanation of the implementation of a new feature in the mod. I'm not very good at tutorials, so we'll hope for the best.

In the context of this document, the apworld side of the feature is already entirely built and assumed to be functional. This focuses exclusively on the mod.

## Moviesanity

The moviesanity feature is a 7.x.x feature introducing location checks for watching movies and buying snacks for NPCs.

## Tutorial

### 1 - Slot Data

First, we need to read the moviesanity fields from slot data. Later on, we will need to use this data to decide on behavior of the mod.

#### Add the key to `StardewArchipelago.Archipelago.SlotData.SlotDataKeys.cs` - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/47a6d6e054283d7083a84fa1b91b86831097be4d)

This is the name of the field, as it will come from the Apworld

![image](https://i.imgur.com/3mf1zcp.png)

#### Create the enum for the Moviesanity values - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/d62d99e223ce6b9b4e557e29d9195d03b043cdb2)

These values need to match the values used in the Apworld.

![image](https://i.imgur.com/LQPV6IV.png)

![image](https://i.imgur.com/yBe0yHv.png)

#### Add Moviesanity to the SlotData Class - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/92daeeca15dbbc755b96ccc63e6422808e9004f6)

This is where the value will be stored for the duration of the play session

![image](https://i.imgur.com/iqScwFE.png)

#### Read Moviesanity from the slot data fields and assign it to the SlotData [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/037796179f623fd94e2f916e4b23cb076c67cda5)

We usually pick a default value of "None" here, so that if the Apworld does not provide the setting, it's probably an older version that also doesn't want to trigger subsequent code for it.

![image](https://i.imgur.com/vxDLv6D.png)

### 2 - Patching the locations

#### Finding the code to patch

For this part, you will need to use your decompiler (usually provided with the IDE), and learn to navigate decompiled sources. I can't really teach you all that, but I can show you my thought process.

I want to patch two things. I want to send checks when a movie is watched, and also send a check when a snack is bought for an NPC. These two patches will need some conditions, like is it loved or whatever, but the general idea is two patches.
Maybe I'll even get away with a single patch, if I can check these things at the same moment.

My first instinct is to look for some movie code, ideally near the ending of the movie. I'll first head over to MovieTheater.cs, and see what is there.

![image](https://i.imgur.com/MgwQfM5.png)

The first thing that comes to mind is this method `RequestEndMovie()`. A quick look through the call stack, and this appears to be precisely the code that runs when the movie finishes, so probably exactly what we want.

![image](https://i.imgur.com/pbHZp60.png)

If it doesn't work, we'll try something else.

#### Creating the CodeInjections file - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/16494ac54e401cf38286266e76001418b7a0d7d9)

To do this, we'll create a file in StardewArchipelago.Locations.CodeInjections.Vanilla. The stuff in there could use some cleaning up. Basically, there is one file per "topic", often per setting, containing one or many patches. Once a file grows too big, it gets split, and often moved to a subfolder.

I do this by copying one of the existing Injections files, and removing most of it except the useful stuff.

![image](https://i.imgur.com/2rB7QDW.png)

#### Preparing the patch - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/9fd59d99a6a013737979a0b4b54598ef22866eb3)

Generally, I copy the definition of the method I'm patching, as a comment over a patch, just for reference.

![image](https://i.imgur.com/vofjqqx.png)

We also need to initialize the CodeInjections class. This will allow it to use structures like the ArchipelagoClient (send checks), and the logger (log errors and warnings), within the patches.

This is done in `StardewArchipelago.Locations.CodeInjections.Initializers.VanillaCodeInjectionInitializer.cs`

![image](https://i.imgur.com/kOJehne.png)

#### Writing the patch code - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/44b87e50e0f0f2a629c431d43130f9b8c6d5cd8e)

Here, we want to send locations for the movie IF conditions are met, and for the snack IF conditions are met.

This one is the part that varies between features, so the specific code I wrote is exclusive to movies and snacks, and will not be useful for other features.

It is about 150 lines, so I recommend checking the commit details [here](https://github.com/agilbert1412/StardewArchipelago/commit/44b87e50e0f0f2a629c431d43130f9b8c6d5cd8e).

The gist of it is, in the prefix patch, if we are currently about to finish the movie (state 1, about to be state 2), we check both the movie and snack locations.

For the movie, we either check "Watch a movie", or the specific movie. We verify if someone was invited who loves the movie, if that setting was picked. Same for the snack condition.

For the snack, we check the snack location. If the player chose the setting for requiring the snack to be loved, we verify this first.

![image](https://i.imgur.com/Z1HLhSc.png)

#### Applying the patch - [Commit](https://github.com/agilbert1412/StardewArchipelago/commit/e029690eb4394676cedc9554a306f1d0f8c1b186)

Lastly, we need to apply the patch to the method. This is done in `StardewArchipelago.Locations.Patcher.VanillaLocationPatcher.cs`

This uses harmony to set up the patch as a prefix on the original method.

![image](https://i.imgur.com/toVNgvP.png)

### 3 - Patching the items

TODO

### 4 - Testing

Now, you just generate a game, host it (locally, ideally), and test it! Do the checks, force send the items, make sure everything works without any errors.

### 5 - Profit!

