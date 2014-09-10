using WPF_MVVM.Helpers;
using System.Windows;
using System;

namespace L3.Cargo.Safety.Display.Common.ViewModel
{
    public class BaseViewModel : NotificationObject
    {
        public event EventHandler SelectView;

        private BaseViewModel _baseModel;
        public static readonly DependencyProperty ElementProperty =
             DependencyProperty.RegisterAttached("Element",
             typeof(object),
             typeof(BaseViewModel),
             new PropertyMetadata(null, OnElementPropertyChanged));

        private static void OnElementPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.DataContext != null)
            {
                (element.DataContext as BaseViewModel).FrameworkElementReference = element;
            }
        }

        public static void SetElement(object element, FrameworkElement value)
        {
            throw new NotSupportedException();
        }

        public static FrameworkElement GetElement(object element)
        {
            throw new NotSupportedException();
        }

        private FrameworkElement _element;

        /// <summary>
        /// Action that gets run when this module is selected
        /// </summary>
        public Action ActionWhenSelected
        {
            get;
            set;
        }

        public FrameworkElement FrameworkElementReference
        {
            get
            {
                return _element;
            }
            set
            {
                _element = value;
                RaisePropertyChanged(() => FrameworkElementReference);
            }
        }



        public BaseViewModel ViewModel
        {
            get;
            set;
        }

        protected void callSelectedHandler(object sender, EventArgs args)
        {
            SelectView(sender, args);
        }
    }
}