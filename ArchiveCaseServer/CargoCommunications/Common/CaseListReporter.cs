using System;

namespace L3.Cargo.Communications.Common
{

    public class CaseListReporter : IObserver<CaseList>
    {
        private IDisposable unsubscriber;
        private string instName;

        public CaseListReporter(string name)
        {
            this.instName = name;
        }

        public string Name
        { get { return this.instName; } }

        public virtual void Subscribe(IObservable<CaseList> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("The Caselist Tracker has completed updating data to {0}.", this.Name);
            this.Unsubscribe();
        }

        public virtual void OnError(Exception e)
        {
            Console.WriteLine("{0}: The Caselist cannot be determined.", this.Name);
        }

        public virtual void OnNext(CaseList caselist)
        {
            Console.WriteLine("caselist updated");
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
    }
}
