using RatesChecker.Infrastructure.Commands;
using RatesChecker.Models.Enums;
using RatesChecker.Services.Events;
using RatesChecker.ViewModels.Base;
using System.Windows.Input;

namespace RatesChecker.ViewModels
{
    internal class SelectionBarControlViewModel : DisposableViewModel
    {
        #region Variables
        private bool _IsCurrencyStreamingButtonEnable = false;
        public bool IsCurrencyStreamingButtonEnable
        {
            get { return _IsCurrencyStreamingButtonEnable; }
            set
            {

                Set(ref _IsCurrencyStreamingButtonEnable, value);

                if (value == true)
                    IsCurrencyEditorButtonEnable = false;

            }
        }

        private bool _IsCurrencyEditorButtonEnable = true;
        public bool IsCurrencyEditorButtonEnable
        {
            get { return _IsCurrencyEditorButtonEnable; }
            set
            {
                Set(ref _IsCurrencyEditorButtonEnable, value);

                if (value == true)
                    IsCurrencyStreamingButtonEnable = false;
            }
        }
        #endregion

        #region Commands
        #region (Command) : OpenWorkplaceCurrencyStreamingViewCommand
        public ICommand OpenWorkplaceCurrencyStreamingViewCommand { get; }
        private bool CanOpenWorkplaceCurrencyStreamingViewCommandExecute(object parameter) => true;
        private void OnOpenWorkplaceCurrencyStreamingViewCommandExecuted(object parameter) => SetSelectedWorkplace(WorkplaceTabsType.CurrencyStreaming);
        #endregion

        #region (Command) : OpenWorkplaceCurrencyEditorViewCommand
        public ICommand OpenWorkplaceCurrencyEditorViewCommand { get; }
        private bool CanOpenWorkplaceCurrencyEditorViewCommandExecute(object parameter) => true;
        private void OnOpenWorkplaceCurrencyEditorViewCommandExecuted(object parameter) => SetSelectedWorkplace(WorkplaceTabsType.CurrencyEditor);
        #endregion

        #endregion

        #region Methods
        void SetSelectedWorkplace(WorkplaceTabsType workplaceTabType)
        {
            switch (workplaceTabType)
            {
                case WorkplaceTabsType.CurrencyStreaming:
                    IsCurrencyEditorButtonEnable = true;
                    break;
                case WorkplaceTabsType.CurrencyEditor:
                    IsCurrencyStreamingButtonEnable = true;
                    break;
            }

            EventDispatcher.Instance?.Invoke(nameof(EventDispatcher.WorkplaceTabChanged), this, workplaceTabType);
            
        }
        #endregion

        #region Constructor
        public SelectionBarControlViewModel()
        {
            OpenWorkplaceCurrencyStreamingViewCommand = new LambdaCommand(OnOpenWorkplaceCurrencyStreamingViewCommandExecuted, CanOpenWorkplaceCurrencyStreamingViewCommandExecute);
            OpenWorkplaceCurrencyEditorViewCommand = new LambdaCommand(OnOpenWorkplaceCurrencyEditorViewCommandExecuted, CanOpenWorkplaceCurrencyEditorViewCommandExecute);
        }
        #endregion
    }
}
