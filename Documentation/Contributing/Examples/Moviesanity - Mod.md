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

This is the name of the field, as it will come from the Apworld

[Commit](https://github.com/agilbert1412/StardewArchipelago/commit/47a6d6e054283d7083a84fa1b91b86831097be4d)

![image](https://i.imgur.com/3mf1zcp.png)

#### Create the enum for the Moviesanity values

These values need to match the values used in the Apworld.

[Commit](https://github.com/agilbert1412/StardewArchipelago/commit/d62d99e223ce6b9b4e557e29d9195d03b043cdb2)

![image](https://i.imgur.com/LQPV6IV.png)

![image](https://i.imgur.com/yBe0yHv.png)

#### Add Moviesanity to the SlotData Class

This is where the value will be stored for the duration of the play session

[Commit](https://github.com/agilbert1412/StardewArchipelago/commit/92daeeca15dbbc755b96ccc63e6422808e9004f6)

![image](https://i.imgur.com/iqScwFE.png)

#### Read Moviesanity from the slot data fields and assign it to the SlotData

We usually pick a default value of "None" here, so that if the Apworld does not provide the setting, it's probably an older version that also doesn't want to trigger subsequent code for it.

[Commit](https://github.com/agilbert1412/StardewArchipelago/commit/037796179f623fd94e2f916e4b23cb076c67cda5)

![image](https://i.imgur.com/vxDLv6D.png)