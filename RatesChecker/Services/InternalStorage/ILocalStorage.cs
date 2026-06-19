
using System.Collections.Generic;
using System.Threading.Tasks;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.InternalStorage
{
    internal interface ILocalStorage
    {
        string SelectedCurrencyFilePath { get; }

        NbrbCurrency GetCurrencyPrimaryDataByAbbriviation(string abbriviation);
        List<NbrbCurrency> GetCurrencyPrimaryData();
        List<CurrencyDisplayModel> GetCurrencyDynamicData();
        List<CurrencyDisplayModel> GetCurrencyDynamicDataFromFile();

        Task<bool> SaveCurrencyDynamicDataToJsonAsync(string filePath);
        Task<bool> OpenCurrencyDynamicDataJsonAsync(string filePath);

        void UpdateSingleRecordInFile(CurrencyDisplayModel editedItem);
    }
}
