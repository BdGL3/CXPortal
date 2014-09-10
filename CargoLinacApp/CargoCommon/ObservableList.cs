using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;

namespace L3.Cargo.Common
{
    public interface IObservableList<T> : IList<T>, INotifyCollectionChanged { }

    public class ObservableList<T> : List<T>, IObservableList<T>, INotifyPropertyChanged
    {
        #region Constructors

        public ObservableList()
        {

            IsNotifying = true;



             //As a gimmick, I wanted to bind to the Count property, so I

             //use the OnPropertyChanged event from the INotifyPropertyChanged

             //interface to notify about Count changes.

            CollectionChanged += new NotifyCollectionChangedEventHandler(

                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {

                    OnPropertyChanged("Count");

                }

                );

        }

        #endregion



        #region Properties

        public bool IsNotifying { get; set; }

        #endregion



        #region Adding and removing items

        public new void Add(T item)
        {

            base.Add(item);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);

            OnCollectionChanged(e);

        }



        public new void AddRange(IEnumerable<T> collection)
        {

            base.AddRange(collection);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(collection));

            OnCollectionChanged(e);

        }



        public new void Clear()
        {

            base.Clear();

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            OnCollectionChanged(e);

        }



        public new void Insert(int i, T item)
        {

            base.Insert(i, item);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);

            OnCollectionChanged(e);

        }



        public new void InsertRange(int i, IEnumerable<T> collection)
        {

            base.InsertRange(i, collection);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection);

            OnCollectionChanged(e);

        }



        public new void Remove(T item)
        {

            base.Remove(item);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);

            OnCollectionChanged(e);

        }



        public new void RemoveAll(Predicate<T> match)
        {

            List<T> backup = FindAll(match);

            base.RemoveAll(match);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, backup);

            OnCollectionChanged(e);

        }



        public new void RemoveAt(int i)
        {

            T backup = this[i];

            base.RemoveAt(i);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, backup);

            OnCollectionChanged(e);

        }



        public new void RemoveRange(int index, int count)
        {

            List<T> backup = GetRange(index, count);

            base.RemoveRange(index, count);

            NotifyCollectionChangedEventArgs e =

                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, backup);

            OnCollectionChanged(e);

        }



        public new T this[int index]
        {

            get { return base[index]; }

            set
            {

                T oldValue = base[index];

                NotifyCollectionChangedEventArgs e =

                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue);

                OnCollectionChanged(e);

            }

        }

        #endregion



        #region INotifyCollectionChanged Members



        public event NotifyCollectionChangedEventHandler CollectionChanged;



        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {

            if (IsNotifying && CollectionChanged != null)

                try
                {

                    CollectionChanged(this, e);

                }

                catch (System.NotSupportedException)
                {

                    NotifyCollectionChangedEventArgs alternativeEventArgs =

                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

                    OnCollectionChanged(alternativeEventArgs);

                }

        }

        #endregion



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;



        protected void OnPropertyChanged(string propertyName)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            }

        }

        #endregion
    }
}
