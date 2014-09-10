using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace L3.Cargo.Common
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        // Override the event so this class can access it
        public override event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Be nice - use BlockReentrancy like MSDN said
            using (BlockReentrancy())
            {
                System.Collections.Specialized.NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                if (eventHandler == null)
                    return;

                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (System.Collections.Specialized.NotifyCollectionChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, e);
                        dispatcherObject.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(DispatcherUnhandledException);
                        dispatcherObject.Dispatcher.UnhandledExceptionFilter += new DispatcherUnhandledExceptionFilterEventHandler(DispatcherUnhandledExceptionFilter);
                    }
                    else // Execute handler as is
                        handler(this, e);
                }
            }
        }

        private void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }
        
        private void DispatcherUnhandledExceptionFilter(object sender, DispatcherUnhandledExceptionFilterEventArgs e)
        {
            // Do nothing here.
        }
    }
}
