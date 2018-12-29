using System;
using System.Diagnostics;

namespace GreenDonut
{
    internal class DispatchingObserver
        : IObserver<DiagnosticListener>
    {
        private readonly DispatchingListener _listener;

        public DispatchingObserver(DispatchingListener listener)
        {
            _listener = listener ??
                throw new ArgumentNullException(nameof(listener));
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "GreenDonut.Dispatching")
            {
                value.SubscribeWithAdapter(_listener);
            }
        }
    }
}
