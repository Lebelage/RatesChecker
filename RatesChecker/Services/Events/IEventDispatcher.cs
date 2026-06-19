using RatesChecker.Models.Enums;
using System;
using System.Collections.Generic;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.Events
{
    internal interface IEventDispatcher
    {
        event EventHandler<WorkplaceTabsType> WorkplaceTabChanged;

        event EventHandler<List<NbrbCurrency>> PrimaryCurrencyDataUpdated;
        event EventHandler<List<CurrencyDisplayModel>> CurrencyDynamicDataUpdated;
        event EventHandler<List<CurrencyDisplayModel>> CurrencyDynamicDataFromFileUpdated;

        event EventHandler<List<NbrbCurrency>> PrimaryCurrencyDataReceived;
        event EventHandler<List<CurrencyDisplayModel>> CurrencyDynamicDataReceived;

        void Invoke(string name, object sender, params object[] args);
    }
}
