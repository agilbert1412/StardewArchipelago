using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Items.Traps.Shuffle;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Traps
{
    public class TrapExecutor
    {
        public readonly TrapDifficultyBalancer _difficultyBalancer;
        public readonly BombSpawner BombSpawner;
        public readonly TileChooser TileChooser;
        public readonly MonsterSpawner MonsterSpawner;
        public readonly BabyBirther BabyBirther;
        public readonly DebrisSpawner DebrisSpawner;
        public readonly InventoryShuffler InventoryShuffler;

        public TrapExecutor(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, IGiftHandler giftHandler)
        {
            _difficultyBalancer = new TrapDifficultyBalancer();
            BombSpawner = new BombSpawner(modHelper);
            TileChooser = new TileChooser();
            MonsterSpawner = new MonsterSpawner(TileChooser);
            BabyBirther = new BabyBirther();
            DebrisSpawner = new DebrisSpawner(logger, archipelago, _difficultyBalancer);
            InventoryShuffler = new InventoryShuffler(logger, giftHandler);
        }
    }
}
