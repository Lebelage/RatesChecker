using RatesChecker.Services.InternalStorage;
using RatesChecker.Services.NbrbApiService;
using RatesChecker.ViewModels.Base;
using RatesChecker.Views.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Variables
        private string _WindowTitle = "Rates Checker";
        public string WindowTitle
        {
            get => _WindowTitle;
            set => Set(ref _WindowTitle, value);
        }
        #endregion
        #region Constructor
        public MainWindowViewModel() { }
        #endregion
    }
}
