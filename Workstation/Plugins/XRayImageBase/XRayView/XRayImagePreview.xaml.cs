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
using L3.Cargo.Common;
using System.Windows.Media.Effects;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Windows.Media.Animation;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    /// <summary>
    /// Interaction logic for XRayImagePreview.xaml
    /// </summary>
    public partial class XRayImagePreview : UserControl
    {
        /// <summary>
        /// The popup containing this control.
        /// </summary>
        public Popup ParentPopup { get; set; }

        /// <summary>
        /// The source of the image
        /// </summary>
        private Image DisplayedImage { get; set; }

        public XRayImagePreview()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            OptionsOverlay.Opacity = 0;

            UpdateOverlays();
        }

        /// <summary>
        /// Generate the thumbnail image and layers.
        /// </summary>
        /// <param name="source">The image source</param>
        /// <param name="effects">The effect list.</param>
        public void GenerateLayers(ImageSource source, AdornerLayerManager adoner, List<Effect> effects)
        {
            // remove all children first
            BaseEffectDockpanel.Children.Clear();
            DisplayedImage = null;

            AnnotationsLayer.Height = source.Height;
            MeasurementsLayer.Height = source.Width;

            AnnotationsLayer.Width = source.Width;
            MeasurementsLayer.Width = source.Width;

            // create the annotations layer
            BitmapSource bitmapSource = (BitmapSource)source;
            DrawingVisual drawingVisual = XRayImageRenderer.GetAnnotationDrawing(bitmapSource.Width, bitmapSource.Height, bitmapSource.Width, bitmapSource.Height, adoner, Brushes.Transparent);
            AnnotationsLayer.RemoveAllLayers();
            AnnotationsLayer.AddLayer(drawingVisual);

            // create the measurements layer
            drawingVisual = XRayImageRenderer.GetMeasurementDrawing(bitmapSource.Width, bitmapSource.Height, bitmapSource.Width, bitmapSource.Height, adoner, Brushes.Transparent);
            MeasurementsLayer.RemoveAllLayers();
            MeasurementsLayer.AddLayer(drawingVisual);

            FrameworkElement scaledElement = ImageCanvas;
            scaledElement.Height = source.Height;
            scaledElement.Width = source.Width;

            // create a scale to size the image into the area of the window
            ScaleTransform scale = new ScaleTransform();
            scale.ScaleY = Math.Min(OuterDock.MaxHeight / scaledElement.Height, OuterDock.MaxWidth / scaledElement.Width);
            scale.ScaleX = scale.ScaleY;

            OuterDock.Height = scale.ScaleY * source.Height;
            OuterDock.Width = scale.ScaleX * source.Width;

            OptionsOverlay.Height = OuterDock.ActualHeight;
            OptionsOverlay.Width = OuterDock.ActualWidth;

            Panel innerPanel = BaseEffectDockpanel;
            BaseEffectDockpanel.Visibility = System.Windows.Visibility.Visible;


            foreach (Effect effect in effects)
            {
                if (effect == null)
                {
                    continue;
                }
                DockPanel panel = new DockPanel();
                panel.Visibility = Visibility.Visible;
                panel.Effect = effect;
                innerPanel.Children.Add(panel);
                innerPanel = panel;
            }

            // add the image as the inner-most element
            Image image = new Image();
            image.Source = source;
            DisplayedImage = image;
            innerPanel.Children.Add(DisplayedImage);

            ImageCanvas.LayoutTransform = scale;
        }

        public void ClosePopup()
        {
            if (ParentPopup != null)
            {
                ParentPopup.IsOpen = false;
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateOverlays();
        }

        private void UpdateOverlays()
        {
            MeasurementsLayer.Visibility = MeasurementCheckBox.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            AnnotationsLayer.Visibility = AnnotationCheckBox.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public bool IsAnnotationShown { get { return AnnotationCheckBox.IsChecked == true; } }

        public bool IsMeasurementShown { get { return MeasurementCheckBox.IsChecked == true; } }

        private void OptionsOverlay_MouseEnter(object sender, MouseEventArgs e)
        {
            OptionsOverlay.Height = OuterDock.ActualHeight;
            OptionsOverlay.Width = OuterDock.ActualWidth;

            var nameScope = NameScope.GetNameScope(this);

            OptionsOverlay.Visibility = System.Windows.Visibility.Visible;
            FadeOpacity(OptionsOverlay, 0.0, 1.0);
        }

        private void OptionsOverlay_MouseLeave(object sender, MouseEventArgs e)
        {
            FadeOpacity(OptionsOverlay, 1.0, 0.0);
        }

        private void FadeOpacity(DependencyObject obj, double from, double to)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(300)));
            Storyboard.SetTarget(fadeAnimation, obj);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);
            storyboard.Begin();
        }
    }

    public class VisualLayers : DockPanel
    {
        private VisualCollection _children;

        public VisualLayers()
        {
            _children = new VisualCollection(this);
        }

        public void AddLayer(DrawingVisual drawingVisual)
        {
            _children.Add(drawingVisual);
        }

        public void RemoveLayer(DrawingVisual annotationVisual)
        {
            _children.Remove(annotationVisual);
        }

        public void RemoveAllLayers()
        {
            _children.Clear();
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }
}