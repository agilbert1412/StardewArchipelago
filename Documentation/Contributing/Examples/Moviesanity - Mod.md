# Implementing Moviesanity - Mod Side

## Introduction

This document is a step by step explanation of the implementation of the moviesanity feature in the mod.

In the context of this document, the apworld side of the feature is already entirely implemented and assumed to be functional. This focuses exclusively on the mod.

Building the Apworld first is not necessary, but usually recommended. If you make the mod first, some steps in this guide might require you to use placeholder values or data until the apworld is ready.

## Moviesanity

The moviesanity feature is a 7.x.x feature introducing location checks for watching movies and buying snacks for NPCs.

## Tutorial

### 1 - Slot Data

First, we need to read the moviesanity fields from slot data. Later on, we will need to use this data to decide on behavior of the mod.

#### Add the key to `StardewArchipelago.Archipelago.SlotData.SlotDataKeys.cs` - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/47a6d6e054283d7083a84fa1b91b86831097be4d)

This is the name of the field, as it will come from the Apworld.

![image](https://i.imgur.com/3mf1zcp.png)

#### Create the enum for the Moviesanity values - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/d62d99e223ce6b9b4e557e29d9195d03b043cdb2)

These values need to match the values used in the Apworld.

![image](https://i.imgur.com/LQPV6IV.png)

![image](https://i.imgur.com/yBe0yHv.png)

#### Add Moviesanity to the SlotData Class - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/92daeeca15dbbc755b96ccc63e6422808e9004f6)

This is where the value will be stored for the duration of the play session.

![image](https://i.imgur.com/iqScwFE.png)

#### Read Moviesanity from the slot data fields and assign it to the SlotData [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/037796179f623fd94e2f916e4b23cb076c67cda5)

We usually pick a default value of "None" here, so that if the Apworld does not provide the setting, it's probably an older version that also doesn't want to trigger subsequent code for it.

![image](https://i.imgur.com/vxDLv6D.png)

### 2 - Patching the locations

#### Finding the code to patch

For this part, you will need to use your decompiler (usually provided with the IDE), and learn to navigate decompiled sources. I can't really teach you all that, but I can show you my thought process.

I want to patch two things. I want to send checks when a movie is watched, and also send a check when a snack is bought for an NPC. These two patches will need some conditions, like is it loved or whatever, but the general idea is two patches.
Maybe I'll even get away with a single patch, if I can check these things at the same moment.

My first instinct is to look for some movie code, ideally near the ending of the movie. I'll first head over to `MovieTheater.cs`, and see what is there.

![image](https://i.imgur.com/MgwQfM5.png)

The first thing that comes to mind is this method `RequestEndMovie()`. A quick look through the call stack, and this appears to be precisely the code that runs when the movie finishes, so probably exactly what we want.

![image](https://i.imgur.com/pbHZp60.png)

If it doesn't work, we'll try something else.

#### Creating the CodeInjections file - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/16494ac54e401cf38286266e76001418b7a0d7d9)

To do this, we'll create a file in `StardewArchipelago.Locations.CodeInjections.Vanilla`. The stuff in there could use some cleaning up. Basically, there is one file per "topic", often per setting, containing one or many patches. Once a file grows too big, it gets split, and often moved to a subfolder.

I do this by copying one of the existing Injections files, and removing most of it except the useful stuff.

![image](https://i.imgur.com/2rB7QDW.png)

#### Preparing the patch - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/9fd59d99a6a013737979a0b4b54598ef22866eb3)

Generally, I copy the definition of the method I'm patching, as a comment over a patch, just for reference.

![image](https://i.imgur.com/vofjqqx.png)

We also need to initialize the CodeInjections class. This will allow it to use structures like the ArchipelagoClient (send checks), and the logger (log errors and warnings), within the patches.

This is done in `StardewArchipelago.Locations.CodeInjections.Initializers.VanillaCodeInjectionInitializer.cs`

![image](https://i.imgur.com/kOJehne.png)

#### Writing the patch code - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/44b87e50e0f0f2a629c431d43130f9b8c6d5cd8e)

Here, we want to send locations for the movie IF conditions are met, and for the snack IF conditions are met.

This one is the part that varies between features, so the specific code I wrote is exclusive to movies and snacks, and will not be useful for other features.

It is about 150 lines, so I recommend checking the commit details [here](https://github.com/agilbert1412/StardewArchipelago/commit/44b87e50e0f0f2a629c431d43130f9b8c6d5cd8e).

The gist of it is, in the prefix patch, if we are currently about to finish the movie (state 1, about to be state 2), we check both the movie and snack locations.

For the movie, we either check "Watch a movie", or the specific movie. We verify if someone was invited who loves the movie, if that setting was picked. Same for the snack condition.

For the snack, we check the snack location. If the player chose the setting for requiring the snack to be loved, we verify this first.

![image](https://i.imgur.com/Z1HLhSc.png)

#### Applying the patch - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/e029690eb4394676cedc9554a306f1d0f8c1b186)

Lastly, we need to apply the patch to the method. This is done in `StardewArchipelago.Locations.Patcher.VanillaLocationPatcher.cs`.

This uses harmony to set up the patch as a prefix on the original method.

![image](https://i.imgur.com/toVNgvP.png)

### 3 - Patching the items

The goal for the items patch is pretty simple. We have 4 items, which are 4 flavor types for the theater snacks. I simply want to only let players purchase snacks from the flavors they have unlocked.

#### Finding the code to patch

The first thing I found, is that the snacks shop is an actual shop, not a custom dialogue. That makes this easy, as we simply need to edit this shop.

![image](https://i.imgur.com/ntUkfF3.png)

I then navigate to my unpacked Stardew assets ([Here](https://stardewvalleywiki.com/Modding:Editing_XNB_files) is a tutorial to unpack the XNB data files to readable files), go to `Content/Data/Shops.json`.

You can find more info on the shop format [here](https://stardewvalleywiki.com/Modding:Shops).

![image](https://i.imgur.com/Rtm49xr.png)

But the important part is that it generates the inventory using a query with the Id `MOVIE_CONCESSIONS_FOR_GUEST`.

This query, and all vanilla queries, can be found in `ItemQueryResolver.cs`.

![image](https://i.imgur.com/ML3h9qn.png)

We can see that it simply loops through concessions for a given guest, and returns them one by one. This leads us back to `MovieTheater.cs`, to the method `GetConcessionsForGuest(string npc_name)`

![image](https://i.imgur.com/TTekXNj.png)

Now we have a decision to make. What behavior do we want, exactly?

#### Game Design

The concession stand does not offer all snacks. It only offers up to 5 snacks with interesting rules

From the wiki: "The game tries to populate the list with one loved snack, two liked snacks, and one disliked snack among the five snacks available at the counter, if they exist. If the theater is built from the Joja Warehouse, JojaCorn is always available for purchase."

So, we have multiple options here, when some snacks are locked

1: Let the game roll 5 snacks, then remove the locked ones. This can lead to an empty shop, even if the player has unlocked some snacks

2: Do our custom rolling code that attempts to emulate the vanilla code, but only from unlocked snacks.

3: Offer all unlocked snacks, instead of a random subset of them.

I personally tend towards #3, because the Movies is already an annoying game mechanic as the player can only go once a week and can miss a critical movie and have to wait very long. Maybe making the snack selection less of a RNG-fest will make this more pleasant.

So, in this case, we will replace the method with an alternate version, that does that!

#### Preparing the patch - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/89d2b76c3e969718fcac056cdf2b2047a3ed03a5)

First, the skeleton of the patch. Note that this method is different from the previous one, as it is static, it has a return value, and we intend on fully replacing it instead of simply prefixing it.

![image](https://i.imgur.com/RMZNyBh.png)

#### Writing the patch code - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/8e3c344322daf4f15e524bf21dc7e2b786f80999)

Here, I basically started by copying the code from the original entirely, then I just remove everything that is no longer needed, notably everything related to offering specific preferences. This leads to basically just returning all concessions.

I take this opportunity to simplify the code and rename variables, as decompiled code is not very readable or manageable.

![image](https://i.imgur.com/6PsT21B.png)

Then, I want to add the locking based on AP items.

First, I want to add the Concessions IDs to named constants, so they are easier to work with

![image](https://i.imgur.com/kfm1PAB.png)

Then, I make a dictionary that easily allows me to tell what flavor is required to unlock a given snack. We have a similar dictionary in the apworld for the logic of it, so I'll just copy it over, and change the Python syntax to C# syntax.

![image](https://i.imgur.com/j3Lzxl3.png)

And, lastly, I write a quick method to tell if a given snack is unlocked, and I can easily use it in a `Where()` query to filter the concessions.

![image](https://i.imgur.com/Hyn9iA7.png)

#### Applying the patch - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/a58263155c9545063e3e3928e05e051b2e852d67)

Lastly, we need to apply the patch, same as the other one. This is done in `StardewArchipelago.Locations.Patcher.VanillaLocationPatcher.cs`

This uses harmony to set up the patch as a prefix on the original method.

![image](https://i.imgur.com/PQuyWhO.png)

### 4 - Testing

Now, you just generate a game, host it (locally, ideally), and test it! Do the checks, force send the items, make sure everything works without any errors.

Error #1 [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/264bf9c190b170b677569cbed5a87827b9a1ff7d): I forgot to add the ModHelper to the Initialize call, when I added it earlier to the Injections class for easier reflection.

This caused a build error.

![image](https://i.imgur.com/ikZxUgj.png)

Other than that, things seem to work!

### 5 - Profit!

![image](https://i.imgur.com/6usMB8M.png)

### 6 - Going Above and Beyond

To try to make this setting more enjoyable, I have two ideas.

#### Remove the limit of one movie per week - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/00fd23e425852a8c3a19f9accff339f078881dd6)

It seems to me like being able to watch multiple movies in a row will make it much easier to deal with this setting. Especially since it will be easy to accidentally invite the wrong person or order the wrong snack. Or, with ER, to not get to the theater at all.

I found that the fields `Farmer.lastSeenMovieWeek` and `NPC.lastSeenMovieWeek` are the ones that track how recently movies were watched. These are just a week number, so if I just set them to a really low value, like -1, after each movie, we should be fine.

![image](https://i.imgur.com/A4jI14f.png)

This field for NPCs is assigned when the movie starts, but for the player, it is assigned at the **end** of the `RequestEndMovie` method, that we patched with a prefix earlier.

So we need to patch it with a postfix this time, which is a patch that runs **after** the original method.

![image](https://i.imgur.com/7vL0MMn.png)

![image](https://i.imgur.com/dwdf2gg.png)

#### Cycle Movies every week instead of every month - [Commit Details](https://github.com/agilbert1412/StardewArchipelago/commit/0510db672ca6c3dd90a5fe37f4d4271fcf1af855)

Again, in the spirit of reducing the pointless sleeping for the movie grind, I figured the movie schedule should change more often. Once a week seems decent, plus the fact that players can give it multiple attempts in the same week with the previous change.

After digging into how the movies are decided, I realized that the algorithm is very incompatible with the seasons randomizer. Notably, the part where it tries to check the date when you unlocked the theater, and heavily relies on seasons, plus the fact that in AP, we can't know in advance what season the player will choose next.

So I decided to rewrite the algorithm found in `MovieTheater.GetMoviesForSeason()`. My new algorithm will return 4 movies for the current, month, but they will be the two movies from that season, duplicated twice each, in a pattern 0-1-0-1.

This will lead to weekly cycling and allow players to quickly pick a season to get to the movie they want. I also added an offset when checking the upcoming movie, so they get swapped. This will not be accurate on the last week of the month, if the player does not repeat the season, but that seems minor, and I have no good way to make it better.

I ended up writing this patch

![image](https://i.imgur.com/H7KDIig.png)