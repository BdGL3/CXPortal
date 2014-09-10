using System;
using System.Collections.Generic;

namespace L3.Cargo.Communications.Common
{
    public abstract class CaseListTracker<T> : IObservable<T>
    {

        private List<IObserver<T>> observers;

        public CaseListTracker()
        {
            observers = new List<IObserver<T>>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }

        public void Notify(T obj)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(obj);
            }
        }
    }
}