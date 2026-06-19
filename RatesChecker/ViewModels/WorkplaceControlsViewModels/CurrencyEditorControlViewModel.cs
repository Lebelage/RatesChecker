using Microsoft.Win32;
using RatesChecker.Infrastructure.Commands;
using RatesChecker.Models.Enums;
using RatesChecker.Services.Events;
using RatesChecker.Services.FileDialog;
using RatesChecker.Services.InternalStorage;
using RatesChecker.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;


namespace RatesChecker.ViewModels.WorkplaceControlsViewModels
{
    internal class CurrencyEditorControlViewModel : DisposableViewModel
    {
        #region Variables
        #region LoadedCurrencyDynamicList : ObservableCollection<CurrencyDisplayModel> - Currency dynamic list
        /// <summary>Currency dynamic list</summary>
        private ObservableCollection<CurrencyDisplayModel> _LoadedCurrencyDynamicList = new ObservableCollection<CurrencyDisplayModel>();
        public ObservableCollection<CurrencyDisplayModel> LoadedCurrencyDynamicList 
        { 
            get => _LoadedCurrencyDynamicList;
            set => Set(ref _LoadedCurrencyDynamicList, value); 
        }
        #endregion

        private bool _IsFileLoaded;
        public bool IsFileLoaded { get => _IsFileLoaded; set => Set(ref _IsFileLoaded, value); }

        private string _SelectedFilePath;
        public string SelectedFilePath { get => _SelectedFilePath; set => Set(ref _SelectedFilePath, value); }
        #endregion

        #region Commands
        #region (Command) : OpenCurrencyFileCommand
        public ICommand OpenCurrencyFileCommand { get; }
        private bool CanOpenCurrencyFileCommandExecute(object parameter) => true;
        private void OnOpenCurrencyFileCommandExecuted(object parameter) => OpenCurrencyFile();
        #endregion
        #endregion

        #region Methods
        async void OpenCurrencyFile()
        {
            var filePath = FileDialogService.Instance.OpenFile("JSON files (*.json)|*.json", ".json");

            if (string.IsNullOrEmpty(filePath))
                return;

            IsFileLoaded = await LocalStorageService.Instance.OpenCurrencyDynamicDataJsonAsync(filePath);
            SelectedFilePath = LocalStorageService.Instance.SelectedCurrencyFilePath;

        }

        void Restore() 
        {
            var storage = LocalStorageService.Instance;

            if (storage.GetCurrencyDynamicDataFromFile()?.Count != 0)
            {
                LoadedCurrencyDynamicList.Clear();

                foreach (var currency in storage.GetCurrencyDynamicDataFromFile())
                    LoadedCurrencyDynamicList.Add(currency);

                IsFileLoaded = true;
                SelectedFilePath = LocalStorageService.Instance.SelectedCurrencyFilePath;

            }
        }
        #endregion

        #region Handlers
        private void OnCurrencyDynamicDataFromFileUpdated(object sender, List<CurrencyDisplayModel> e)
        {
            LoadedCurrencyDynamicList.Clear();

            foreach (var currency in e)
                LoadedCurrencyDynamicList.Add(currency);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }

            
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OfficialRate" || e.PropertyName == "Abbreviation")
            {
                if (sender is CurrencyDisplayModel editedRow)
                {
                    LocalStorageService.Instance.UpdateSingleRecordInFile(editedRow);
                }
            }
        }
        #endregion

        #region Constructor/Dispose
        public CurrencyEditorControlViewModel()
        {
            OpenCurrencyFileCommand = new LambdaCommand(OnOpenCurrencyFileCommandExecuted, CanOpenCurrencyFileCommandExecute);

            EventDispatcher.Instance.CurrencyDynamicDataFromFileUpdated += OnCurrencyDynamicDataFromFileUpdated;
            LoadedCurrencyDynamicList.CollectionChanged += OnCollectionChanged;

            Restore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EventDispatcher.Instance.CurrencyDynamicDataFromFileUpdated -= OnCurrencyDynamicDataFromFileUpdated;
                LoadedCurrencyDynamicList.CollectionChanged -= OnCollectionChanged;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
