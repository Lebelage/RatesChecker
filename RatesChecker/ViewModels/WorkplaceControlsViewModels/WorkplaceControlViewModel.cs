using RatesChecker.Models.Enums;
using RatesChecker.Services.Events;
using RatesChecker.ViewModels.Base;
using RatesChecker.Views.Controls;
using System;
using System.Windows.Controls;

namespace RatesChecker.ViewModels.WorkplaceControlsViewModels
{
    internal class WorkplaceControlViewModel : DisposableViewModel
    {
        #region Variables
        private Control _CurrentView = new Workplace_CurrencyStreamingControl();
        public Control CurrentView
        {
            get => _CurrentView;
            set => Set(ref _CurrentView, value);
        }
        #endregion

        #region Handlers
        private void WorkplaceTabChanged(object sender, WorkplaceTabsType e)
        {
            (CurrentView.DataContext as DisposableViewModel)?.Dispose();

            switch (e)
            {
                case WorkplaceTabsType.CurrencyStreaming:
                    CurrentView = new Workplace_CurrencyStreamingControl();
                    break;
                case WorkplaceTabsType.CurrencyEditor:
                    CurrentView = new Workplace_CurrencyEditorControl();
                    break;
            }
        }
        #endregion

        #region Constructors
        public WorkplaceControlViewModel()
        {
            EventDispatcher.Instance.WorkplaceTabChanged += WorkplaceTabChanged;
        }
        #endregion


    }
}
