# Stardew Valley Archipelago Contributor Guide

## Introduction

This mod is separated in two parts.

First, the actual mod, that players use in their Stardew Valley. This mod is coded in C#, and the standard Stardew Valley modding tutorials (like the ones on the wiki https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started) will be massively helpful to understand it.

Second, the backend, also called the `apworld`. This is in Python, and lives in the Archipelago Core repository, for the most part. This part handles the generator, the website, the hosting, etc. It can also be modified for purposes of compatibility with potential trackers. Other Stardew Valley mods have no such thing, so you'll find documentation and help on the [Archipelago Repository](https://github.com/ArchipelagoMW/Archipelago) and [Discord Server](https://discord.gg/8Z65BR2).

## Software

For every entry on this list, I describe what you strictly need, and then the exact program I personally use. You do not have to use the same programs as I do, but if you do, you get the bonus of being able to ask me questions and for help, and I'll be able to respond.

Everything on this list is available for free, for individuals.

### Required

- C# IDE (I use [Visual Studio Community](https://visualstudio.microsoft.com/vs/community/))
- Python IDE if you intend to modify the apworld (I use [PyCharm](https://www.jetbrains.com/pycharm/))
- Git client (most IDEs come with a built-in one, but I prefer to use [TortoiseGit](https://tortoisegit.org/))
- Stardew Valley (duh. I use the [Steam Version](https://store.steampowered.com/app/413150/Stardew_Valley/))
- [SMAPI](https://smapi.io/)
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

### Optional

- [Notepad++](https://notepad-plus-plus.org/) for lightweight text editing, useful for small tasks when the IDE is overkill
- [Archipelago](https://github.com/ArchipelagoMW/Archipelago/releases) to be able to run the apworld not from source, sometimes useful.

## First Steps

- Fork and clone the relevant git repositories, and set up multiple remotes for each so you have access to the original remotes and your own fork that you can push changes to
  * [Archipelago Core](https://github.com/ArchipelagoMW/Archipelago), [my fork of it](https://github.com/agilbert1412/Archipelago), and your fork of it
  * [StardewArchipelago](https://github.com/agilbert1412/StardewArchipelago) and your fork of it
  * OPTIONAL - [ArchipelagoUtilities](https://github.com/agilbert1412/ArchipelagoUtilities) a set of utilities for my Archipelago mod, in case you need to modify something there (unlikely)
