using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class BundleFactory
    {
        private LogHandler _logger;
        private IModHelper _modHelper;
        private StardewArchipelagoClient _archipelago;
        private ArchipelagoStateDto _state;
        private LocationChecker _locationChecker;

        public BundleFactory(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
        }

        public JunimoNoteMenuRemake CreateJunimoNoteMenu(ArchipelagoJunimoNoteMenu junimoNoteMenu, int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            switch (junimoNoteMenu)
            {
                case IngredientsJunimoNoteMenu ingredientMenu:
                    return new IngredientsJunimoNoteMenu(whichArea, bundlesComplete);

                case CurrencyJunimoNoteMenu currencyMenu:
                    return new CurrencyJunimoNoteMenu(whichArea, bundlesComplete);

                default:
                    throw new NotImplementedException($"{nameof(CreateJunimoNoteMenu)}(ArchipelagoJunimoNoteMenu junimoNoteMenu, int whichArea, Dictionary<int, bool[]> bundlesComplete) not implemented for type {junimoNoteMenu.GetType()}");
            }
        }

        public JunimoNoteMenuRemake CreateJunimoNoteMenu(ArchipelagoJunimoNoteMenu junimoNoteMenu, bool fromGameMenu, int whichArea = 1, bool fromThisMenu = false)
        {
            switch (junimoNoteMenu)
            {
                case IngredientsJunimoNoteMenu ingredientMenu:
                    return new IngredientsJunimoNoteMenu(true, whichArea, true);

                case CurrencyJunimoNoteMenu currencyMenu:
                    return new CurrencyJunimoNoteMenu(true, whichArea, true);

                default:
                    throw new NotImplementedException($"{nameof(CreateJunimoNoteMenu)}(ArchipelagoJunimoNoteMenu junimoNoteMenu, int whichArea) not implemented for type {junimoNoteMenu.GetType()}");
            }
        }

        public JunimoNoteMenuRemake CreateJunimoNoteMenu(int whichArea = 1, bool fromThisMenu = false)
        {

            switch (junimoNoteMenu)
            {
                case IngredientsJunimoNoteMenu ingredientMenu:
                    return new IngredientsJunimoNoteMenu(true, whichArea, true);

                case CurrencyJunimoNoteMenu currencyMenu:
                    return new CurrencyJunimoNoteMenu(true, whichArea, true);

                default:
                    throw new NotImplementedException($"{nameof(CreateJunimoNoteMenu)}(ArchipelagoJunimoNoteMenu junimoNoteMenu, int whichArea) not implemented for type {junimoNoteMenu.GetType()}");
            }
        }
    }
}
