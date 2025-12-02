# Stardew Valley Archipelago - Elevator Progression

## Elevator Progression

This setting allows you to randomize the elevator floor buttons available to navigate the Mines

This setting does not have an impact on the actual treasure chests in the mines, which are always randomized.

### Option Values

#### Vanilla

The elevator floors are not shuffled. You unlock the elevator as normal. As soon as you reach a given floor (multiple of 5), the elevator to go down to that floor becomes available forever, for free.

#### Progressive

Every 5 floors you reach is a location check. Your own elevator is locked behind 24 `Progressive Mine Elevator` items. Each one received unlocks 5 more floors from the elevator.

This severely locks down how deep you can reach in the mines, since you can only go as far as you can get in a single dive, plus your current elevator level.

Logic expects you to usually mine about 5 floors deeper than your elevator, sometimes 10 if you are particularly well-equipped.

You can send a floor location, by using the elevator to go directly to this floor.

#### Progressive From Previous Floor

This setting behaves the same as `Progressive`, with the exception that you can only check an elevator location if you dig to it. You cannot use the elevator to travel directly to a given floor, and check the location this way.

You can use the elevator to go to the previous 5-floor slice, and then mine down 5 floors by hand. The only requirement is that you reach floor N, by finding a ladder on floor N-1.

This is slightly more time consuming and difficult.

## [Return to Index](./index.md)
