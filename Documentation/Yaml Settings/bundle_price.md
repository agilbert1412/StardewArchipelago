# Stardew Valley Archipelago Bundle Price

## Bundle Price

The Bundle Price setting allows you to customize the difficulty of the bundles found in your community center, abandoned jojamart and raccoon requests.

This setting has a massive impact on the difficulty and duration of a slot with the [Community Center](./goal.md#community-center) goal.

Expensive settings are very difficult, and you should only play Maximum Prices if you are truly ready for a crazy challenge.

### How Bundles work

Bundles in Archipelago are composed of multiple different "number of items", which all play a different role. For the sake of this explanation, I will use the [Thematic](./bundle_randomization.md#thematic) Spring Foraging Bundle.

The first is the item pool. This is the list of all items that **can** show up in this bundle. For the Spring Foraging Bundle, this list contains 7 items:
- Wild Horseradish x1
- Daffodil x1
- Leek x1
- Dandelion x1
- Salmonberry x1
- Spring Onion x1
- Morel x1

Each of these items can have an amount, a quality, and a flavor if applicable. In this example, all the items are a stack of 1, basic quality, no flavor.

The 2nd important value is the number of requested ingredients. This is the number of items that will get picked from the item pool, to be shown as ingredients to a given player. In the Spring Foraging Bundle, this number is 4.

The 3rd value is the number of item slots to fill. If this number is smaller than the number of ingredients, the player gets to pick which ingredients they fulfill, leading to more freedom and player choice.

![image](https://i.imgur.com/d8Awymz.png)

If there are more item slots than requested ingredients, additional items will be pulled from the pool to add ingredients.
If all items have been pulled, the bundle will now generate duplicates.
This is generally undesirable, so the default prices of bundles will respect the rule `[number of items] >= [requested ingredients] >= [item slots]`.

For Simplicity, I would usually refer to the Spring Foraging Bundle as a `7/4/4` bundle, which means 7 potential items, 4 ingredients picked, and 4 slots to fill.

When talking about how difficult a given bundle is, the size of the item pool is generally not relevant, so in this case I might refer to the Spring Foraging Bundle as a `4/4` bundle, omitting the first number entirely.

### Bundle Price Impact on item slots

The Bundle Price setting has a direct effect on the number of item slots. It can also have an indirect effect on the requested ingredients, if it increases the number of slots higher than the current number, as described in the previous section.

The available values are:
- Minimum (Always 1)
- Very Cheap (-2)
- Cheap (-1)
- Normal (0)
- Expensive (1)
- Very Expensive (+2)
- Maximum (Always 8)

Minimum and Maximum are absolute. They change the number of slots to 1 and 8 respectively, without regards for what the original value was.
All other options are relative. They place a modifier on the original value, to reduce or increase it. Again with the Spring Foraging Bundle, here are the final values that would occur under each setting

The number of item slots can never go below 1, and can never go over 8.

- Minimum (7/4/1)
- Very Cheap (7/4/2)
- Cheap (7/4/3)
- Normal (7/4/4)
- Expensive (7/5/5) -> We start pulling additional ingredients from the pool
- Very Expensive (7/6/6)
- Maximum (7/8/8) -> There will be one duplicate ingredient

### Bundle Price Impact on item amounts

The Bundle Price setting also has an impact on the amount of each ingredient. This number is always **rounded down** to the nearest integer, which means that for most items, that have an amount of "1", the setting makes no difference.

The price multipliers are:
- Minimum (0.2)
- Very Cheap (0.6)
- Cheap (0.8)
- Normal (0)
- Expensive (1.1)
- Very Expensive (1.2)
- Maximum (1.4)

In the case of the Spring Foraging Bundle, since all items have an amount of 1, this would change nothing. But in the case of a bundle like the Sticky Bundle, that requests one slot of 500 sap, here are the final prices on each option.

- Minimum (1/1/1 x100)
- Very Cheap (1/1/1 x300)
- Cheap (1/1/1 x400)
- Normal (1/1/1 x500)
- Expensive (1/2/2 x550)
- Very Expensive (1/3/3 x600)
- Maximum (1/8/8 x700)

### Bundle Price Impact on currency bundles

In the case of currency bundles, there are essentially no item slots. Just a requested amount on a given currency.
As a result, the item slots algorithm does not apply here. For balance reasons, we have decided to make the item amount multipliers bigger to compensate.

For currency bundles, here are the multipliers:
- Minimum (0.1)
- Very Cheap (0.2)
- Cheap (0.6)
- Normal (0)
- Expensive (1.4)
- Very Expensive (1.8)
- Maximum (4)

In the case of the Carnival Bundle, which requests a default of 2500 Star Tokens, here are the final amounts:
- Minimum (250)
- Very Cheap (500)
- Cheap (1500)
- Normal (2500)
- Expensive (3500)
- Very Expensive (4500)
- Maximum (10000)

## [Return to Index](./index.md)
