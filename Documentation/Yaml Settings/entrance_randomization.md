# Stardew Valley Archipelago - Entrance Randomization

## Entrance Randomization

Entrance Randomization is, arguably, the most "flashy" setting in Stardew Valley Archipelago.

It randomizes every entrance in the game (doors), so that they do not lead where you expect them to lead. This forces the player to learn a brand new map layout, with some things available much earlier than usual, and some things availably much later.

It can lead to very locked down seeds, if a critical entrance (ex: Pierre) is locked behind a very late-game door (ex: Qi's Walnut Room).

This setting can be a lot of fun, or a lot of trouble. It is neither recommended, nor discouraged, for new players. You just need to know what you are signing up for.

### Disabled

No Entrance Randomization, all doors are normal.

### Pelican Town

This is baby's first ER. This shuffles only the doors that are on the main Pelican Town map, with each other. This does not include the locked doors on that map (the sewers and Community Center).

This does not result in any logic or progression changes. This only has an impact on opening hours, and the psychological impact of making the player re-learn the map.

### Non Progression

This setting randomizes every door in the world that does not start out locked. For the purpose of this setting, "lock" means a door that you require some Archipelago Items to reach, for example the sewer needs a Rusty Key. The Oasis requires a bus. Etc.

A door that starts locked, but is unlocked naturally without receiving any items, is considered unlocked. For example, Willy's shop that opens naturally on day 2.

Note that this does **not** include doors like Leah's Cottage (hearts), the Bathhouse (Boulder), or the sewers (Rusty Key).

Finally, this setting also does not include the farmhouse, even though it technically qualifies, because it is intended as an **easy** setting, and the farmhouse being randomized makes the game much more difficult.

### Buildings Without House

Same as "Buildings" (see below), but excluding the farmhouse, so you can get a good night's sleep every night

### Buildings

This setting randomizes every door in the world that roughly qualifies as "entering something". This includes building doors, bedroom doors, caves, etc. This does not include overworld transitions (for example, Farm to Bus Stop), or generally transitions that don't have a well-defined "Inside" and "Outside".

This includes locked doors, the farmhouse, and generally is considered a very difficult setting. The randomization can make your slot much harder or longer, by locking access to critical areas.

### Chaos

You should not play this setting. Seriously.

This randomizes the same doors as "Buildings", but it reshuffles them every day. This means you cannot learn the map layout, and on most days, you will be unable to go to a specific place you need to, and will end up having to sleep to try again tomorrow.

This setting is extremely unforgiving and frustrating, and your own skill will dictate very little of what you can do.

## The Future

Originally, we built ER as our own thing, as many games did. In late 2024, a new core feature has been introduced to Archipelago, called "Generic Entrance Randomizer", or GER.
This allows worlds to rely on its smart and complex algorithm to do entrance randomization for them. This massively simplifies the work for worlds, and increases the potential for features related to it.

We have transitioned to GER in May of 2025. This makes Stardew faster and more stable, and opens the door to future ER features.

Originally, because of technical limitations in our original Entrance Randomizer implementation, we stopped at "Buildings", because we needed entrance directions to do anything.

This is no longer a requirement, so we intend some changes in the future, although we can't promise when exactly.

1: `Entrance Randomizer: World` - This setting would randomize every map transition in the world, including outside, undirected transitions, like Bus Stop <-> Town.
2: `Entrance Randomizer: Everything` - This setting would randomize every map transition, and also every method of transportation that is not a conventional transition. This would include randomizing Obelisk and Warp Totem destinations, Minecarts and Parrot Express travel, etc.
3: `Entrance Randomizer: Decoupled` - This would be a flag that you could set on top of any of the existing ER settings. It would decouple all the randomized doors, so that crossing it one way would not be the same as crossing it the opposite way. So you could, for example, enter the saloon, end up in the Blacksmith, but then try to exit the blacksmith, and land at the Community Center. The non-Euclidian space would make for a brand new, mind-melting challenge!
4: Turn `Entrance Randomizer: Chaos` into a flag - Similar to how `Decoupled` was described, we could have chaos be a flag, instead of its own setting, that could be applied on top of one of the existing settings. It could lead to an actually playable version of Chaos, if you did `Pelican Town + Chaos` for example.

## Interaction With Other Settings

### [Start Without](./start_without.md)

Within the setting [Start Without](./start_without.md), are options to "Start Without: Community Center" and "Start Without: Landslide". These change the conditions necessary to enter the Community Center, Wizard's Tower, Mines, and Adventurer's Guild. For example, if you start without the Community Center, you now need the `Community Center Key` to enter the entrance; however, if you start with the Community Center, the entrance will be open immediately.

This may seem to imply that whether they count as `Non-Progression` entrances could change dynamically with this setting; however, due to the way Start Without is handled, you will be given these items in the mail on the first day when their corresponding settings are disabled. Because of that, these entrances are always considered to be locked behind an Archipelago Item, and will thus never count as `Non-Progression`.

## [Return to Index](./index.md)
