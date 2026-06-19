using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RatesChecker.Infrastructure.Commands;
using RatesChecker.Models.Enums;
using RatesChecker.Services.CurrencyPlotService;
using RatesChecker.Services.Events;
using RatesChecker.Services.FileDialog;
using RatesChecker.Services.InternalStorage;
using RatesChecker.Services.NbrbApi;
using RatesChecker.Services.NbrbApiService;
using RatesChecker.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.ViewModels.WorkplaceControlsViewModels
{
    internal class CurrencyStreamingControlViewModel : DisposableViewModel
    {
        #region Variables

        #region CurrenciesAbbreviationList : ObservableCollection<string> - Currencies abbreviation list
        /// <summary>Currencies abbreviation list</summary>
        public ObservableCollection<string> CurrenciesAbbreviationList { get; }
        #endregion

        #region SelectedCurrencyDynamicList : ObservableCollection<CurrencyDisplayModel> - Currency dynamic list
        /// <summary>Currency dynamic list</summary>
        private ObservableCollection<CurrencyDisplayModel> _SelectedCurrencyDynamicList = new ObservableCollection<CurrencyDisplayModel>();
        public ObservableCollection<CurrencyDisplayModel> SelectedCurrencyDynamicList { get => _SelectedCurrencyDynamicList; set => Set(ref _SelectedCurrencyDynamicList, value); }
        #endregion

        #region CurrenciesAbbreviationList : ObservableCollection<string> - Currencies abbreviation list
        /// <summary>Currencies abbreviation list</summary>
        public ObservableCollection<PeriodType> DynamicPeriodsList { get; } = new ObservableCollection<PeriodType>() { PeriodType.Day, PeriodType.Week, PeriodType.Month, PeriodType.Year };
        #endregion

        #region SelectedCurrency : string - Selected currency abbrviation
        /// <summary>Currencies abbreviation list</summary>
        private string _SelectedCurrency;
        public string SelectedCurrency
        {
            get
            {
                return _SelectedCurrency;
            }
            set
            {
                Set(ref _SelectedCurrency, value);

                var data = LocalStorageService.Instance.GetCurrencyPrimaryDataByAbbriviation(_SelectedCurrency);

                if (data is null)
                    return;

                RequestDynamicCurrencyData(data, SelectedDynamicPeriod);

            }
        }
        #endregion

        #region SelectedDynamicPeriod : PeriodType - Selected dynamic period type
        /// <summary>Selected dynamic period type</summary>
        private PeriodType _SelectedDynamicPeriod = PeriodType.Day;
        public PeriodType SelectedDynamicPeriod
        {
            get { return _SelectedDynamicPeriod; }
            set
            {
                Set(ref _SelectedDynamicPeriod, value);

                var data = LocalStorageService.Instance.GetCurrencyPrimaryDataByAbbriviation(_SelectedCurrency);

                if (data is null)
                    return;

                RequestDynamicCurrencyData(data, value);
            }
        }
        #endregion

        #region CurrencyPlotModel : PlotModel - Dynamic plot
        private PlotModel _CurrencyPlotModel;
        public PlotModel CurrencyPlotModel { get => _CurrencyPlotModel; set => Set(ref _CurrencyPlotModel, value); }
        #endregion

        #region ShowTableOrPlotButtonContent : string - Content of switch plot or table button
        private string _ShowTableOrPlotButtonContent = "Show plot";
        public string ShowTableOrPlotButtonContent { get => _ShowTableOrPlotButtonContent; set => Set(ref _ShowTableOrPlotButtonContent, value); }
        #endregion

        #region DataGridVisibility : Visibility - Data grid visibility property
        private Visibility _DataGridVisibility = Visibility.Visible;
        public Visibility DataGridVisibility { get => _DataGridVisibility; set => Set(ref _DataGridVisibility, value); }
        #endregion

        #region PlotVisibility : Visibility - Plot visibility property
        private Visibility _PlotVisibility = Visibility.Collapsed;
        public Visibility PlotVisibility { get => _PlotVisibility; set => Set(ref _PlotVisibility, value); }
        #endregion
        #endregion

        #region Commands

        #region (Command) : SaveCurrencyDynamicDataToJsonCommand
        public ICommand SaveCurrencyDynamicDataToJsonCommand { get; }
        private bool CanSaveCurrencyDynamicDataToJsonCommandExecute(object parameter) => true;
        private void OnSaveCurrencyDynamicDataToJsonCommandExecuted(object parameter) => SaveCurrencyDynamicDataToJson();
        #endregion

        #region (Command) : ChangePlotVisibilityCommand
        public ICommand ChangePlotVisibilityCommand { get; }
        private bool CanChangePlotVisibilityCommandExecute(object parameter) => true;
        private void OnChangePlotVisibilityCommandExecuted(object parameter) 
        {
            PlotVisibility = PlotVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            DataGridVisibility = PlotVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            ShowTableOrPlotButtonContent = PlotVisibility == Visibility.Visible ? "Show table" : "Show plot";
        } 
        #endregion

        #endregion

        #region Methods

        void Restore()
        {
            var storage = LocalStorageService.Instance;

            if (storage.GetCurrencyPrimaryData()?.Count != 0)
            {
                CurrenciesAbbreviationList.Clear();

                var currencies = storage.GetCurrencyPrimaryData();

                foreach (var currency in currencies)
                    CurrenciesAbbreviationList.Add(currency.Cur_Abbreviation);
            }

            if (storage.GetCurrencyDynamicData()?.Count != 0)
            {
                SelectedCurrencyDynamicList.Clear();

                var currencies = storage.GetCurrencyDynamicData();

                foreach (var currency in currencies)
                    SelectedCurrencyDynamicList.Add(currency);

                DateTime minDate = SelectedCurrencyDynamicList.Min(x => x.Date);
                DateTime maxDate = SelectedCurrencyDynamicList.Max(x => x.Date);

                double totalDays = (maxDate - minDate).TotalDays;

                if (totalDays <= 1.5)
                {
                    _SelectedDynamicPeriod = PeriodType.Day;
                }
                else if (totalDays <= 8)
                {
                    _SelectedDynamicPeriod = PeriodType.Week;
                }
                else if (totalDays <= 32)
                {
                    _SelectedDynamicPeriod = PeriodType.Month;
                }
                else
                {
                    _SelectedDynamicPeriod = PeriodType.Year;
                }

                _SelectedCurrency = storage.GetCurrencyDynamicData().FirstOrDefault()?.Abbreviation;

                CurrencyPlotService.Instance.ShowPlot(currencies, SelectedCurrency);
            }
        }

        private void RequestDynamicCurrencyData(NbrbCurrency selectedCurrencyPrimaryData, PeriodType periodType)
        {
            NbrbApiService.Instance.RequestDynamicsByPeriodAsync(selectedCurrencyPrimaryData, periodType);
        }

        private async void SaveCurrencyDynamicDataToJson()
        {
            string filePath = FileDialogService.Instance.SaveFile("JSON files (*.json)|*.json", ".json");

            if (string.IsNullOrEmpty(filePath))
                return;

            var res = await LocalStorageService.Instance.SaveCurrencyDynamicDataToJsonAsync(filePath);
        }
        #endregion

        #region Handlers

        private void OnCurrencyDynamicDataUpdated(object sender, List<CurrencyDisplayModel> e)
        {
            SelectedCurrencyDynamicList.Clear();

            foreach (var currency in e)
                SelectedCurrencyDynamicList.Add(currency);

            CurrencyPlotService.Instance.ShowPlot(e, SelectedCurrency);
        }

        private void OnPrimaryCurrencyDataUpdated(object sender, List<NbrbCurrency> e)
        {
            CurrenciesAbbreviationList.Clear();

            foreach (var currency in e)
                CurrenciesAbbreviationList.Add(currency.Cur_Abbreviation);
        }
        #endregion

        #region Constructors/Dispose
        public CurrencyStreamingControlViewModel()
        {
            CurrenciesAbbreviationList = new ObservableCollection<string>();

            CurrencyPlotModel = new PlotModel();
            CurrencyPlotService.Instance.Initialize(CurrencyPlotModel);

            var eventDispatcher = EventDispatcher.Instance;
            eventDispatcher.PrimaryCurrencyDataUpdated += OnPrimaryCurrencyDataUpdated;
            eventDispatcher.CurrencyDynamicDataUpdated += OnCurrencyDynamicDataUpdated;

            SaveCurrencyDynamicDataToJsonCommand = new LambdaCommand(OnSaveCurrencyDynamicDataToJsonCommandExecuted, CanSaveCurrencyDynamicDataToJsonCommandExecute);
            ChangePlotVisibilityCommand = new LambdaCommand(OnChangePlotVisibilityCommandExecuted, CanChangePlotVisibilityCommandExecute);

            Restore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var eventDispatcher = EventDispatcher.Instance;
                eventDispatcher.PrimaryCurrencyDataUpdated -= OnPrimaryCurrencyDataUpdated;
                eventDispatcher.CurrencyDynamicDataUpdated -= OnCurrencyDynamicDataUpdated;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
