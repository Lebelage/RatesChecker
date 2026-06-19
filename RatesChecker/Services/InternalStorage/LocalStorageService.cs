
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RatesChecker.Services.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.InternalStorage
{
    internal class LocalStorageService : ILocalStorage
    {
        #region Singleton - обход DI
        private static readonly Lazy<ILocalStorage> _instance =
        new Lazy<ILocalStorage>(() => new LocalStorageService());

        public static ILocalStorage Instance => _instance.Value;

        #endregion

        #region Variables
        public string SelectedCurrencyFilePath { get; private set; }

        private List<CurrencyDisplayModel> _CurrencyDynamicData = new List<CurrencyDisplayModel>();
        private List<CurrencyDisplayModel> _CurrencyDynamicDataFromFile = new List<CurrencyDisplayModel>();
        private List<NbrbCurrency> _PrimaryCurrencyData = new List<NbrbCurrency>();

        object locker = new object();
        #endregion

        #region Methods

        private void FillPrimaryCurrencyData(IEnumerable<NbrbCurrency> data)
        {
            lock (locker)
            {
                _PrimaryCurrencyData = data.ToList();

                EventDispatcher.Instance.Invoke(nameof(EventDispatcher.PrimaryCurrencyDataUpdated), this, _PrimaryCurrencyData);
            }

        }
        private void FillCurrencyDynamicData(IEnumerable<CurrencyDisplayModel> data)
        {
            lock (locker)
            {
                _CurrencyDynamicData = data.ToList();

                EventDispatcher.Instance.Invoke(nameof(EventDispatcher.CurrencyDynamicDataUpdated), this, _CurrencyDynamicData);
            }
        }
        private void FillCurrencyDynamicDataFromFile(IEnumerable<CurrencyDisplayModel> data)
        {
            lock (locker)
            {
                _CurrencyDynamicDataFromFile = data.ToList();

                EventDispatcher.Instance.Invoke(nameof(EventDispatcher.CurrencyDynamicDataFromFileUpdated), this, _CurrencyDynamicDataFromFile);
            }
        }

        public int GetCurrencyIdByAbbriviation(string abbriviation)
        {
            return _PrimaryCurrencyData.FirstOrDefault(c => c.Cur_Abbreviation == abbriviation)?.Cur_ID ?? -1;
        }
        public NbrbCurrency GetCurrencyPrimaryDataByAbbriviation(string abbriviation) { return _PrimaryCurrencyData.FirstOrDefault(c => string.Equals(c.Cur_Abbreviation, abbriviation, StringComparison.OrdinalIgnoreCase)); }
        public List<NbrbCurrency> GetCurrencyPrimaryData() { return _PrimaryCurrencyData; }
        public List<CurrencyDisplayModel> GetCurrencyDynamicData() { return _CurrencyDynamicData; }
        public List<CurrencyDisplayModel> GetCurrencyDynamicDataFromFile() { return _CurrencyDynamicDataFromFile; }


        public async Task<bool> SaveCurrencyDynamicDataToJsonAsync(string filePath)
        {
            try
            {
                string jsonContent = await Task.Run(() => JsonConvert.SerializeObject(_CurrencyDynamicData, Formatting.Indented));

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    await writer.WriteAsync(jsonContent);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public async Task<bool> OpenCurrencyDynamicDataJsonAsync(string filePath)
        {
            try
            {
                IEnumerable<CurrencyDisplayModel> data = await Task.Run(() =>
                {
                    if (!File.Exists(filePath))
                        return Enumerable.Empty<CurrencyDisplayModel>();

                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<List<CurrencyDisplayModel>>(jsonContent) ?? Enumerable.Empty<CurrencyDisplayModel>();
                });

                FillCurrencyDynamicDataFromFile(data);
                SelectedCurrencyFilePath = filePath;

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;

        }

        public void UpdateSingleRecordInFile(CurrencyDisplayModel editedItem)
        {
            if (string.IsNullOrEmpty(SelectedCurrencyFilePath) || !File.Exists(SelectedCurrencyFilePath))
                return;

            try
            {
                string jsonContent = File.ReadAllText(SelectedCurrencyFilePath);
                JArray jsonArray = JArray.Parse(jsonContent);

                var targetNode = jsonArray.Children<JObject>()
                    .FirstOrDefault(node =>
                        node["Date"]?.ToObject<DateTime>().Date == editedItem.Date.Date &&
                        node["Name"]?.ToString() == editedItem.Name);

                if (targetNode != null)
                {
                    targetNode["Abbreviation"] = editedItem.Abbreviation;
                    targetNode["OfficialRate"] = editedItem.OfficialRate;

                    File.WriteAllText(SelectedCurrencyFilePath, jsonArray.ToString(Formatting.Indented));
                }
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Handlers
        private void OnCurrencyDynamicDataReceived(object sender, List<CurrencyDisplayModel> e)
        {
            if (e is null)
                return;

            FillCurrencyDynamicData(e);
        }
        private void OnPrimaryCurrencyDataReceived(object sender, List<NbrbCurrency> e)
        {
            if (e is null)
                return;

            FillPrimaryCurrencyData(e);
        }





        #endregion

        #region Constructor
        private LocalStorageService()
        {
            var eventDispatcher = EventDispatcher.Instance;
            eventDispatcher.PrimaryCurrencyDataReceived += OnPrimaryCurrencyDataReceived;
            eventDispatcher.CurrencyDynamicDataReceived += OnCurrencyDynamicDataReceived;
        }


        #endregion
    }
}
