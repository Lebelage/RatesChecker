using OxyPlot;
using RatesChecker.Models.Enums;
using System.Collections.Generic;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.CurrencyPlotService
{
    internal interface ICurrencyPlotService
    {
        void Initialize(PlotModel plotModel);

        void ShowPlot(IEnumerable<CurrencyDisplayModel> currencyDynamics,string selectedCurrency);
    }
}
