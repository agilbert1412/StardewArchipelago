# Stardew Valley Archipelago - Arcade Machine Locations

## Arcade Machine Locations

This setting allows you to randomize both arcade machines available at the Stardrop Saloon: `Journey of the Prairie King` and `Junimo Kart`

### Option Values

Here are the 4 possible values
- `disabled`
- `victories`
- `victories_easy`
- `full_shuffling` **This is the default value**

#### Disabled

The Arcade Machines are entirely excluded from the Archipelago slot. You can still play them if you want, but they behave normally and give no rewards.

#### Victories

The gameplay of the arcade machines is entirely unchanged, but both machines have one location check placed at their final level completion.

You will potentially need to beat both to complete your slot, with no buffs whatsoever compared to their vanilla versions.

This is the **hardest** arcade machine setting.

#### Early Progressive

This setting behaves the same as `Victories`, but both arcade machines are made significantly easier.

Journey of the Prairie King has permanent double drop rate for all powerups, one starting upgrade for boots, gun and ammo (3 starting upgrades total), and 2 extra starting lives

Junimo Kart has 11 regenerating lives per level, instead of the usual 3 (8 permanent bonus lives)

Still one location check at the end of each game.

#### Full Shuffling

This setting randomizes the content of both arcade machines. This feels as if they were standalone, independent Archipelago games that are played side by side with Stardew in a multiworld.

Journey of the Prairie King:
- Locations (12 Total):
  - 1 location for beating each boss (3 Bosses)
  - 1 location sold by the vendor for each upgrade in vanilla (2 Boots, 3 Guns, 3 Ammo and 1 Super Gun, for a total of 9).
- Items:
  - 1 item per upgrade (2 Boots, 4 Guns, 3 Ammo). All obtained permanently for all runs.
  - 1 Permanent x2 to drop rate of powerups
  - 2 Permanent Extra Lives
  
Junimo Kart:
- Locations (10 Total):
  - One for each unique level beaten
- Items:
  - Permanent Extra Lives (10 Total, up to permanent 13 lives)
  
Both Arcade Machines can be released using the commands `!!arcade_release jotpk` and `!!arcade_release jk`, once their final level is beaten, to release any remaining checks in them.

## Interaction With Other Settings

### ~~[Special Order Locations](./special_order_locations.md)~~

If ~~[Special Order Locations](./special_order_locations.md)~~ are enabled up to the Qi Board, but the Arcade Machines are disabled, the Special Order `Let's Play A Game`, which requires the player to get a certain score in Endless Junimo Kart, will be excluded from the Qi Board.

## [Return to Index](./index.md)
