using System;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows;
using System.ComponentModel;

namespace L3.Cargo.Common
{
    public class ExtendedCollectionViewSource : CollectionViewSource
    {
        private BindingListAdapter mAdapter;

        static ExtendedCollectionViewSource()
        {
            CollectionViewSource.SourceProperty.OverrideMetadata(
                typeof(ExtendedCollectionViewSource),
                new FrameworkPropertyMetadata(null, CoerceSource));
           
        }

        private static object CoerceSource(DependencyObject d, object baseValue)
        {
            ExtendedCollectionViewSource cvs = (ExtendedCollectionViewSource)d;
            if (cvs.mAdapter != null)
            {
                cvs.mAdapter.Dispose();
                cvs.mAdapter = null;
            }
            IBindingList bindingList = baseValue as IBindingList;
            if (bindingList != null)
            {
                cvs.mAdapter = new BindingListAdapter(bindingList);
                return cvs.mAdapter;
            }
            return baseValue;
        }
    }

    class BindingListAdapter : ObservableCollectionEx<object>, IDisposable
    {
        private readonly IBindingList mBindingList;
        private bool mIsDisposed;

        public BindingListAdapter(IBindingList bindingList)
        {
            if (bindingList == null)
            {
                throw new ArgumentNullException("bindingList");
            }

            mBindingList = bindingList;

            foreach (object item in mBindingList)
            {

                Items.Add(item);
            }

            mBindingList.ListChanged += BindingList_ListChanged;
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                InsertItem(e.NewIndex, mBindingList[e.NewIndex]);
            }
            else if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                SetItem(e.NewIndex, mBindingList[e.NewIndex]);
            }
            else if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                RemoveItem(e.NewIndex);
            }
            else if (e.ListChangedType == ListChangedType.ItemMoved)
            {
                MoveItem(e.OldIndex, e.NewIndex);
            }
            else if (e.ListChangedType == ListChangedType.Reset)
            {
                Items.Clear();

                foreach (object item in mBindingList)
                {
                    Items.Add(item);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                if (disposing)
                {
                    mBindingList.ListChanged -= BindingList_ListChanged;
                }
            }
            mIsDisposed = true;
        }

        #endregion
    }
}
