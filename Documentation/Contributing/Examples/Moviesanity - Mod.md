# Implementing Moviesanity - Mod Side

## Introduction

This document intends to do a step by step explanation of the implementation of a new feature in the mod. I'm not very good at tutorials, so we'll hope for the best.

In the context of this document, the apworld side of the feature is already entirely built and assumed to be functional. This focuses exclusively on the mod.

## Moviesanity

The moviesanity feature is a 7.x.x feature introducing location checks for watching movies and buying snacks for NPCs.

## Tutorial

### 1 - Slot Data

First, we need to read the moviesanity fields from slot data. Later on, we will need to use this data to decide on behavior of the mod.

#### Add the key to `StardewArchipelago.Archipelago.SlotData.SlotDataKeys.cs`

![image](https://i.imgur.com/49K3ZnI.png)
[Commit](https://github.com/agilbert1412/StardewArchipelago/commit/47a6d6e054283d7083a84fa1b91b86831097be4d)