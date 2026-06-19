using RatesChecker.Services.InternalStorage;
using RatesChecker.Services.NbrbApi;
using RatesChecker.Services.NbrbApiService;
using System.Windows;

namespace RatesChecker
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private INbrbApiService _NbrbApiService;
        private ILocalStorage _LocalStorage;


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _NbrbApiService = NbrbApiService.Instance;
            _LocalStorage = LocalStorageService.Instance;

            bool isApiAlive = await _NbrbApiService.CheckApiConnectionAsync();

            if (!isApiAlive)
                return;

            await _NbrbApiService.RequestAvailableCurrenciesAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
