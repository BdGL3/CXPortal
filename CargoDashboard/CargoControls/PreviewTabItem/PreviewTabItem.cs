using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace L3.Cargo.Controls
{
    public class PreviewTabItem : TabItem
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty PreviewWidthProperty =
                DependencyProperty.Register("PreviewWidth", typeof(double), typeof(PreviewTabItem),
                                            new FrameworkPropertyMetadata(150D, PreviewMaxWidth_PropertyChanged));

        public static readonly DependencyProperty PreviewHeightProperty =
                DependencyProperty.Register("PreviewHeight", typeof(double), typeof(PreviewTabItem),
                                            new FrameworkPropertyMetadata(100D, PreviewMaxHeight_PropertyChanged));

        #endregion Dependency Property Definitions


        #region Private Members

        private Border _Border;

        #endregion Private Members


        #region Public Members

        public double PreviewWidth
        {
            get
            {
                return (double)GetValue(PreviewWidthProperty);
            }
            set
            {
                SetValue(PreviewWidthProperty, value);
            }
        }

        public double PreviewHeight
        {
            get
            {
                return (double)GetValue(PreviewHeightProperty);
            }
            set
            {
                SetValue(PreviewHeightProperty, value);
            }
        }

        #endregion Public Members


        #region Constructors

        static PreviewTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PreviewTabItem), new FrameworkPropertyMetadata(typeof(PreviewTabItem)));
        }

        #endregion Constructors


        #region Private Methods

        private static void PreviewMaxWidth_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PreviewTabItem previewTabItem = sender as PreviewTabItem;
            if (previewTabItem != null && previewTabItem._Border != null)
            {
                previewTabItem._Border.MaxWidth = (double)e.NewValue;
            }
        }

        private static void PreviewMaxHeight_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PreviewTabItem previewTabItem = sender as PreviewTabItem;
            if (previewTabItem != null && previewTabItem._Border != null)
            {
                previewTabItem._Border.MaxHeight = (double)e.NewValue;
            }
        }

        #endregion Private Methods


        #region Protected Methods

        #endregion Protected Methods


        #region Public Methods

        public override void OnApplyTemplate ()
        {
            base.OnApplyTemplate();

            _Border = this.Template.FindName("PART_Border", this) as Border;
            if (_Border != null)
            {
                _Border.Width = PreviewWidth;
                _Border.Height = PreviewHeight;
            }
        }

        #endregion Public Methods
    }
}
