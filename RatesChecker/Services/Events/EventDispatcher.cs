using RatesChecker.Models.CurrencyModel;
using RatesChecker.Models.Enums;
using RatesChecker.Services.InternalStorage;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using static RatesChecker.Models.CurrencyModel.CurrencyModel;

namespace RatesChecker.Services.Events
{
    internal class EventDispatcher : IEventDispatcher
    {
        #region Singleton - обход DI
        private static readonly Lazy<IEventDispatcher> _instance =
        new Lazy<IEventDispatcher>(() => new EventDispatcher());

        public static IEventDispatcher Instance => _instance.Value;
        #endregion

        #region Variables
        public event EventHandler<List<NbrbCurrency>> PrimaryCurrencyDataReceived;
        public event EventHandler<List<CurrencyDisplayModel>> CurrencyDynamicDataReceived;

        public event EventHandler<List<NbrbCurrency>> PrimaryCurrencyDataUpdated;
        public event EventHandler<List<CurrencyDisplayModel>> CurrencyDynamicDataUpdated;
        public event EventHandler<List<CurrencyDisplayModel>> CurrencyDynamicDataFromFileUpdated;

        public event EventHandler<WorkplaceTabsType> WorkplaceTabChanged;

        private readonly Dispatcher _Dispatcher = Dispatcher.CurrentDispatcher;
        #endregion

        public void Invoke(string name, object sender, params object[] args)
        {
            if (!(GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(this) is MulticastDelegate md))
                return;

            object[] invokeArgs = new object[args.Length + 1];
            invokeArgs[0] = sender;
            args.CopyTo(invokeArgs, 1);

            foreach (Delegate d in md.GetInvocationList())
                _Dispatcher.BeginInvoke(new Action(() => d.Method.Invoke(d.Target, invokeArgs)));
        }
    }
}
