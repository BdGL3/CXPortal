using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using L3.Cargo.Common;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    public class AdornerBase : Adorner
    {
        #region Private Members

        private bool _IsEnabled;

        #endregion Private Members


        #region Protected Members

        protected VisualCollection visualChildren;

        protected Boolean EventsRegistered;

        protected double _Zoom;

        protected double _OffsetX;

        protected double _OffsetY;

        protected override int VisualChildrenCount
        {
            get
            {
                return visualChildren.Count;
            }
        }

        #endregion Protected Members


        #region Public Members

        public virtual bool Enabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                _IsEnabled = value;
                this.IsEnabled = value;

                if (_IsEnabled)
                {
                    RegisterEvents();
                }
                else
                {
                    UnregisterEvents();
                }
                InvalidateVisual();
            }
        }

        #endregion Public Members


        #region Constructors

        public AdornerBase (UIElement element, PanZoomPanel panZoomPanel)
            : base(element)
        {
            MenuItem mi = new MenuItem();
            var binding = new Binding("Remove");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(mi, MenuItem.HeaderProperty, binding);
            mi.Click += new RoutedEventHandler(RemoveItem_Click);

            this.ContextMenu = new ContextMenu();
            this.ContextMenu.Items.Add(mi);

            _Zoom = panZoomPanel.Zoom;
            _OffsetX = panZoomPanel.OffsetX;
            _OffsetY = panZoomPanel.OffsetY;

            panZoomPanel.ZoomChanged += new DoublePropertyEventHandler(panZoomPanel_ZoomChanged);
            panZoomPanel.OffsetXChanged += new DoublePropertyEventHandler(panZoomPanel_OffsetXChanged);
            panZoomPanel.OffsetYChanged += new DoublePropertyEventHandler(panZoomPanel_OffsetYChanged);

            IsClipEnabled = true;
            EventsRegistered = false;
            _IsEnabled = false;

            visualChildren = new VisualCollection(this);
        }

        #endregion Constructors


        #region Protected Methods

        protected virtual void RegisterEvents ()
        {
            if (!EventsRegistered)
            {
                AdornedElement.MouseMove += new MouseEventHandler(AdornedElement_MouseMove);
                AdornedElement.MouseLeftButtonDown += new MouseButtonEventHandler(AdornedElement_MouseLeftButtonDown);
                AdornedElement.MouseRightButtonUp += new MouseButtonEventHandler(AdornedElement_MouseRightButtonUp);
                MouseRightButtonUp += new MouseButtonEventHandler(OnMouseRightButtonUp);
                EventsRegistered = true;
            }
        }

        protected virtual void UnregisterEvents ()
        {
            if (EventsRegistered)
            {
                AdornedElement.MouseMove -= AdornedElement_MouseMove;
                AdornedElement.MouseLeftButtonDown -= AdornedElement_MouseLeftButtonDown;
                AdornedElement.MouseRightButtonUp -= AdornedElement_MouseRightButtonUp;
                MouseRightButtonUp -= OnMouseRightButtonUp;
                EventsRegistered = false;
            }
        }

        protected override Visual GetVisualChild (int index)
        {
            return visualChildren[index];
        }

        protected Point PointToImageCoordinates(Point windowPoint)
        {
            return new Point((windowPoint.X - _OffsetX) / _Zoom, (windowPoint.Y - _OffsetY) / _Zoom);
        }

        protected Point PointToWindowCoordinates(Point imgPoint)
        {
            return new Point(imgPoint.X * _Zoom + _OffsetX, imgPoint.Y * _Zoom + _OffsetY);
        }

        #endregion Protected Methods


        #region Layer Events

        protected virtual void AdornedElement_MouseMove (object sender, MouseEventArgs e)
        {
        }

        protected virtual void AdornedElement_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
        {
        }

        protected virtual void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //this.ContextMenu.IsOpen = true;
        }

        protected virtual void AdornedElement_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        protected virtual void panZoomPanel_ZoomChanged(object sender, DoublePropertyEventArgs e)
        {
            _Zoom = e.NewValue;
            InvalidateVisual();
        }

        protected virtual void panZoomPanel_OffsetXChanged(object sender, DoublePropertyEventArgs e)
        {
            _OffsetX = e.NewValue;
            InvalidateVisual();
        }

        protected virtual void panZoomPanel_OffsetYChanged(object sender, DoublePropertyEventArgs e)
        {
            _OffsetY = e.NewValue;
            InvalidateVisual();
        }

        #endregion Layer Events


        #region Menu Events

        protected virtual void RemoveItem_Click (object sender, EventArgs e)
        {
        }

        #endregion Menu Events
    }
}
