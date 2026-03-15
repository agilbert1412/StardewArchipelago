# Stardew Valley Archipelago - Skill Progression

## Skill Progression

This setting allows you to randomize the skill levels for all player skills

### What are the skills?

There are 5 default skills. You start with them at level 0 and can level them up all the way to level 10.

- Farming
- Foraging
- Fishing
- Mining
- Combat

Every level gives some passive bonuses, such as tool proficiency with the related tool. Most levels also give one or multiple crafting or cooking recipes. On levels 5 and 10, you get to choose a profession as well for that skill. Profession bonuses are exclusively Quality of Life upgrades that aren't considered in logic.

Furthermore, every skill has an associated [Mastery](https://stardewvalleywiki.com/Mastery_Cave). Once a skill is maxed out, you can start using this skill to earn Mastery Experience. Mastery Experience can be used to claim Masteries in the Mastery Cave.

In Archipelago, you can enter the mastery cave once you receive the `Mastery Of The Five Ways` item.

### Option Values

This option decides whether the skills are shuffled in the multiworld, and optionally, whether the masteries for each skill also are.

Here are the 3 possible values
- `vanilla`
- `progresssive` **This is the default value**
- `progressive_with_masteries`

#### Vanilla

The skills are not shuffled at all. You earn experience and level up normally.

#### Progressive

Earning Experience will send sequential checks for each threshold that would usually lead to a level up. These are at experience amounts of [100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000].

You can use the in-game command `!!experience` at any time to check you earned experience, as the in-game UI will only show received levels, not earned experience.

Your item pool will contain 10 skill level items per skill (`Farming Level`, `Foraging Level`, etc). All normal rewards from level ups will come with received levels, not earned experience.

Some settings like ~~[Chefsanity](./chefsanity.md)~~ may take out some rewards from the level ups, and shuffle these separately.

Under these settings, the masteries themselves are not randomized. Once you have **received** 10 levels in a given skill, you can use this skill to earn mastery xp, and claim the masteries normally.

#### Progressive With Masteries

Same as **Progressive**, but the masteries are shuffled as well. They are separate from the skill levels, so you can get `Fishing Mastery` while you are still `Fishing Level 0`.

While you can use any level 10 skill to earn mastery XP and go claim any mastery locations at the cave, the only mastery locations that are in-logic are the ones from the level 10 skills.

## Interaction With Other Settings

### Modded Content

Some of the [Supported Mods](./../Supported Mods.md) introduce new skills to the game.

Specifically, the following mods:
- [Spacecore Luck Skill](https://www.nexusmods.com/stardewvalley/mods/28103)
- [Socializing Skill](https://www.nexusmods.com/stardewvalley/mods/14142)
- [Archaeology](https://www.nexusmods.com/stardewvalley/mods/22199)
- [Binning Skill](https://www.nexusmods.com/stardewvalley/mods/14073)

The custom skills don't have masteries, only normal level up items and locations.


## [Return to Index](./index.md)
