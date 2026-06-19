using Newtonsoft.Json;
using RatesChecker.Models.Enums;
using RatesChecker.Services.Events;
using RatesChecker.Services.InternalStorage;
using RatesChecker.Services.NbrbApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.NbrbApiService
{
    internal class NbrbApiService : INbrbApiService
    {

        #region Singleton - обход DI
        private static readonly Lazy<INbrbApiService> _instance =
        new Lazy<INbrbApiService>(() => new NbrbApiService());

        public static INbrbApiService Instance => _instance.Value;
        #endregion

        #region Variables
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.nbrb.by/exrates/";

        private IEventDispatcher eventDispatcher = EventDispatcher.Instance;
        #endregion


        #region Methods
        public async Task<bool> CheckApiConnectionAsync()
        {
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    var response = await _httpClient.GetAsync(
                        $"{BaseUrl}currencies",
                        HttpCompletionOption.ResponseHeadersRead,
                        cts.Token);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task RequestAvailableCurrenciesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}currencies");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var allCurrencies = JsonConvert.DeserializeObject<List<NbrbCurrency>>(json);

                eventDispatcher.Invoke(nameof(IEventDispatcher.PrimaryCurrencyDataReceived), this, allCurrencies.Where(c => c.Cur_DateEnd >= DateTime.Today).ToList());
            }
            catch (Exception ex) { }
        }

        public async Task RequestDynamicsByPeriodAsync(NbrbCurrency currencyPrimaryData, PeriodType period)
        {
            try
            {
                DateTime endDate = DateTime.Today;
                DateTime startDate;

                switch (period)
                {
                    case PeriodType.Day:
                        startDate = endDate.AddDays(-1);
                        break;
                    case PeriodType.Week:
                        startDate = endDate.AddDays(-7);
                        break;
                    case PeriodType.Month:
                        startDate = endDate.AddMonths(-1);
                        break;
                    case PeriodType.Year:
                        startDate = endDate.AddYears(-1);
                        break;
                    default:
                        startDate = endDate;
                        break;
                }

                string start = startDate.ToString("yyyy-MM-dd");
                string end = endDate.ToString("yyyy-MM-dd");

                var response = await _httpClient.GetAsync($"{BaseUrl}rates/dynamics/{currencyPrimaryData.Cur_ID}?startdate={start}&enddate={end}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dynamics = JsonConvert.DeserializeObject<List<NbrbRateDynamic>>(json);

                var result = new List<CurrencyDisplayModel>();
                foreach (var item in dynamics)
                {
                    result.Add(new CurrencyDisplayModel
                    {
                        Date = item.Date,
                        Abbreviation = currencyPrimaryData.Cur_Abbreviation,
                        Name = currencyPrimaryData.Cur_Name,
                        OfficialRate = item.Cur_OfficialRate
                    });
                }

                eventDispatcher.Invoke(nameof(IEventDispatcher.CurrencyDynamicDataReceived), this, result);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        private NbrbApiService()
        {
            _httpClient = new HttpClient();
        }
    }
}
