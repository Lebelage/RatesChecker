using RatesChecker.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.NbrbApi
{
    internal interface INbrbApiService
    {
        Task<bool> CheckApiConnectionAsync();
        Task RequestAvailableCurrenciesAsync();
        Task RequestDynamicsByPeriodAsync(NbrbCurrency currencyPrimaryData, PeriodType period);
    }
}
