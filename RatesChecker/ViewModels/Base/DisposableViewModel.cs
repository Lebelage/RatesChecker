using System;

namespace RatesChecker.ViewModels.Base
{
    internal class DisposableViewModel : ViewModel, IDisposable
    {
        private bool _Disposed = false;
        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _Disposed) return;
            _Disposed = true;
        }
    }
}
