# Stardew Valley Archipelago - Quest Locations

## Quest Locations

This setting allows you to shuffle the [Story Quests](https://stardewvalleywiki.com/Quests#List_of_Story_Quests), and their rewards, and also optionally a certain number of [Help Wanted Quests](https://stardewvalleywiki.com/Quests#Help_Wanted_Quests)

### How does the shuffling work?

For the Story Quests, this means that every story quest now gives a location check. Most of them did not have a vanilla reward (other than money), but for the few that do, these rewards are then shuffled in the item pool.

If you don't randomize Story Quests, you might need to complete some of them for their vanilla reward. For example, `The Mysterious Qi` for the `Club Card`.

For the Help Wanted Quests, they are usually an infinite number of them, so when shuffled, you pick a certain number of them to be location checks. Once you are past the shuffled quests, the subsequent ones revert to their vanilla behavior.

Since there are 4 types of Help Wanted Quests (This setting was created before the introduction of a 5th type), you can enable them in groups of 7, with a ratio of 4 Deliveries to 1 Gathering, 1 Fishing and 1 Slaying. This is very close to the ratio in vanilla, so it made sense to mimic it.

As a consequence, you can only enable Help Wanted Quests in multiples of 7.

### Option Values

This option is a `Range`, which can go from -1 to 56, but the valid values are [-1, 0, 7, 14, 21, 28, 35, 42, 49, 56] due to the limitation on help wanteds being a multiple of 7

If you pick -1, it disables all story quests and help wanted quests. They are entirely excluded from the randomization.

If you pick a value between 0 and 56, it enables story quests, and the value you picked is the number of help wanted quests that will be included as location checks.

**The default value is 7, which means including all Story Quests and 7 Help Wanted Quests**

## Mod Changes

### Story Quests

Many of the vanilla story quests are linked to specific in-game dates.

In Archipelago, most of them have been changed to use a specific season and day of month, and a few have been changed to use total days elapsed regardless of date, if the quest itself seemed too unrelated to the season and would be valid in any season.

Furthermore, most quests locked to Year2 have been moved to Year1 for the specific purpose of reducing the amount of sleeping players need to do, to reach a check that might be in-logic.

Here are all the changed quest starting conditions:

| Story Quest                    | Vanilla Condition | Archipelago Condition                 |
|--------------------------------|-------------------|---------------------------------------|
| To The Beach                   | Year 1, Spring 2  | Year 1, Day 2 (2nd day of 1st month)  |
| Fresh Fruit                    | Year 2, Spring 6  | Year 1, Spring 6                      |
| Granny's Gift                  | Year 2, Spring 15 | Year 1, Spring 15                     |
| Pierre's Notice                | Year 2, Spring 21 | Year 1, Spring 21                     |
| Mayor's "Shorts"               | Year 1, Summer 3  | Year 1, Day 31 (3rd day of 2nd month) |
| Aquatic Research               | Year 2, Summer 6  | Year 1, Summer 6                      |
| Mayor's Need                   | Year 2, Summer 21 | Year 1, Summer 21                     |
| Wanted: Lobster                | Year 2, Fall 6    | Year 1, Fall 6                        |
| Pam Needs Juice                | Year 2, Fall 19   | Year 1, Fall 19                       |
| Staff of Power                 | Year 2, Winter 5  | Year 1, Winter 5                      |
| Catch a Lingcod                | Year 2, Winter 13 | Year 1, Winter 13                     |
| Exotic Spirits                 | Year 2, Winter 19 | Year 1, Winter 19                     |

### Help Wanted Quests

Fishing Quests cannot appear on the board until you have access to a Fishing rod

Slaying Quests cannot appear on the board until you have slain 10 monsters, demonstrating the ability to reach monsters to slay them

As long as you have Shuffled Help Wanted quests remaining, all remaining valid quests that could show up are placed into a big pool, then one is drawn.

This means that, the more quests of one type are remaining, the higher the chances that the next quest is of this type, but it is only guaranteed if you are done with every other type.

Whenever you complete a quest, the next number of this type is the location that gets checked. So there is no strict "This is the 3rd item delivery quest" or anything like that. Whichever Item delivery you accept and complete, will send the 3rd check, if the first two checks are already sent.

This behavior is affected by remote location checking, such as `!collect`

## The Future

### Saying 'Hello'

It would be good to implement the new (from 1.6) `Saying 'Hello'` type of Help Wanted Quests.

## [Return to Index](./index.md)
