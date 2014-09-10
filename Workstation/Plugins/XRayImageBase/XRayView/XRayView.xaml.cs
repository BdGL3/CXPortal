using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Manager;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Common;
using System.Threading.Tasks;
using L3.Cargo.Controls;
using System.Windows.Documents;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;
using System.Windows.Data;
using System.Collections.Generic;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    /// <summary>
    /// Interaction logic for XRayImage.xaml
    /// </summary>
    public partial class XRayView : UserControl, IDisposable
    {
        #region Private Members

        private ViewObject m_ViewObject;


        private AdornerLayerManager _AdornerLayerManager;

        private SourceObject m_SourceObject;

        private string PseudoColorPath;

        private string TrimatPath;

        private BufferPluginManager m_BufferManager;

        private BufferInstances m_Buffers;

        private FilterPluginManager m_FilterManager;

        private FilterInstances m_Filters;

        private History m_History;

        private Boolean IsSetup = false;

        private Boolean XRay_PanAndZoomViewerLoadedOnce = false;

        private SysConfiguration m_sysConfig;

        private Boolean IsMacroButtonClicked = false;

        private Boolean IsApplyHistoryFromSetup = false;

        private TrimatEffect _TrimatEffect;

        private XrayImageEffect _XrayImageEffect;

        #endregion Private Members


        #region Public Members

        public AdornerLayerManager AdornerLayerManager
        {
            get
            {
                return _AdornerLayerManager;
            }
        }

        public DockPanel HistogramDockPanel
        {
            get
            {
                return XrayImage_Panel;
            }
        }

        public Image Image
        {
            get
            {
                return MainImage;
            }
        }

        public History History
        {
            get
            {
                return m_History;
            }
        }

        public SourceObject CurrentSource
        {
            get
            {
                return m_SourceObject;
            }
        }

        public BitmapSource RenderedImage
        {
            get
            {
                return GetRenderedXrayImage();
            }

        }
        public XrayImageEffect XrayImageEffect
        {
            get
            {
                return _XrayImageEffect;
            }
        }

        public event AlgServerRequestEventHandler AlgServerRequestEvent;

        public string ViewName
        {
            get
            {
                return m_ViewObject.Name;
            }
        }

        public bool CanCreateNewAnnot {
            get;
            set;
        }

        #endregion Public Members


        #region Constructor

        public XRayView ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            PseudoColorPath = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\PseudoColor\\";
            _AdornerLayerManager = new AdornerLayerManager(AdornerLayer.GetAdornerLayer(PanAndZoomControl));
            _XrayImageEffect = new XrayImageEffect();
             
            XrayImage_Panel.Effect = _XrayImageEffect;
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // reset the data on the combo box so the values are refreshed.
            if (PseudoColor_ComboBox != null)
            {
                var data = PseudoColor_ComboBox.DataContext;
                PseudoColor_ComboBox.DataContext = null;
                PseudoColor_ComboBox.DataContext = data;
            }
        }

        #endregion Constructor


        #region Private Methods

        private void OnLoaded (object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(OnLoaded);

            MeasureAdorner measureAdorner = new MeasureAdorner(MainImage, PanAndZoomControl);
            measureAdorner.SamplingSpace = m_ViewObject.SamplingSpace;
            measureAdorner.SamplingSpeed = m_ViewObject.SamplingSpeed;
            _AdornerLayerManager.Add(AdornerLayerManager.MEASUREMENT_ADORNER, measureAdorner);

            AnnotationAdorner annotationAdorner = new AnnotationAdorner(MainImage, PanAndZoomControl);
            _AdornerLayerManager.Add(AdornerLayerManager.ANNOTATION_ADORNER, annotationAdorner);

            if (m_ViewObject.Annotations != null && m_ViewObject.Annotations.Count > 0)
            {
                var annotAdorner = (_AdornerLayerManager.GetAdorner(AdornerLayerManager.ANNOTATION_ADORNER) as AnnotationAdorner);
                annotAdorner.SetCanCreateAnnotation(CanCreateNewAnnot);
                if (annotAdorner != null)
                {
                    foreach (AnnotationInfo info in m_ViewObject.Annotations)
                    {
                        annotAdorner.AddAnnotationInfo(info, true);
                    }
                }
            }
            _AdornerLayerManager.Hide(AdornerLayerManager.ANNOTATION_ADORNER);
            _AdornerLayerManager.Show(AdornerLayerManager.ANNOTATION_ADORNER);

            AOIAdorner aoiAdorner = new AOIAdorner(MainImage, PanAndZoomControl);
            aoiAdorner.AlgServerRequestEvent += new AlgServerRequestEventHandler(AoiAdorner_AlgServerRequestEvent);
            _AdornerLayerManager.Add(AdornerLayerManager.AOI_ADORNER, aoiAdorner);
        }

        private void AoiAdorner_AlgServerRequestEvent(object sender, AlgServerRequestEventArgs e)
        {
            if (AlgServerRequestEvent != null)
            {
                AlgServerRequestEvent(sender, e);
            }
        }

        private void CreateBufferControls ()
        {
            m_BufferManager = new BufferPluginManager();

            m_Buffers = m_BufferManager.GetInstances();

            foreach (BufferInstance buffer in m_Buffers)
            {
                BufferParameter filterParam = new BufferParameter(XrayImage_Panel, m_History);
                try
                {
                    buffer.Instance.Initialize(filterParam);

                    Buffer_ToolBar.Items.Add(buffer.Instance.ToolBarItem);

                    Button button = buffer.Instance.ToolBarItem as Button;
                    if (button != null)
                    {
                        button.Click += new RoutedEventHandler(BufferButton_Clicked);
                    }
                }
                catch
                {
                    //TODO: Error Logs
                }
            }
        }

        private void BufferButton_Clicked(object sender, RoutedEventArgs e)
        {
            Clear_DualEnergySettings();
            Button button = sender as Button;

            if (button != null)
            {
                foreach (BufferInstance buffer in m_Buffers)
                {
                    if (string.Compare(button.Name, buffer.Instance.Name, true) == 0)
                    {
                        buffer.Instance.ApplyFilter(true);
                    }
                    else
                    {
                        buffer.Instance.ApplyFilter(false);
                    }
                }
            }
        }

        private void CreateFilterControls()
        {
            m_FilterManager = new FilterPluginManager();

            m_Filters = m_FilterManager.GetInstances();

            UIElement previousControl = PanZoom_Border.Child;

            PanZoom_Border.Child = null;

            foreach (FilterInstance filter in m_Filters)
            {
                DockPanel dockPanel = new DockPanel();
                dockPanel.Height = PanZoomBounds.Height;
                dockPanel.Width = PanZoomBounds.Width;
                dockPanel.Children.Add(previousControl);

                FilterParameter filterParam = new FilterParameter(dockPanel, m_SourceObject.Width, m_SourceObject.Height, m_History, m_sysConfig);
                try
                {
                    filter.Instance.Initialize(filterParam);

                    Filter_ToolBar.Items.Add(filter.Instance.ToolBarItem);
                    ToolBar.SetOverflowMode(filter.Instance.ToolBarItem, OverflowMode.AsNeeded);
                }
                catch
                {
                    //TODO: Error Logs
                }

                previousControl = dockPanel;
            }

            PanZoom_Border.Child = previousControl;
        }

        private void CreateColorMappingControls ()
        {
            foreach (string fileOn in Directory.GetFiles(PseudoColorPath))
            {
                if (fileOn.ToLower().Contains(".lut"))
                {
                    try
                    {
                        CreatePseudoColorComboBoxItem(fileOn);
                    }
                    catch { }
                }
            }

            HistoryPseudoColor pseudoColor = new HistoryPseudoColor();
            pseudoColor.name = "None";
            m_History.SetFirstStep(pseudoColor);
        }

        private void CreatePseudoColorComboBoxItem (string file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            Image image = new Image();
            image.Margin = new Thickness(2);
            image.Width = 130;
            image.Height = 26;
            image.Stretch = Stretch.Fill;
            image.Source = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute)); ;

            ComboBoxItem cbi = new ComboBoxItem();
            cbi.Content = image;
            cbi.Name = fileName;

            if (L3.Cargo.Common.Resources.ResourceManager.GetString(fileName) == null)
            {
                // if there is no resource string just use the filename for the tooltip
                cbi.ToolTip = fileName;
            }
            else
            {
                // otherwise bind the tooltip to the resources
                var binding = new Binding(fileName);
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(cbi, ComboBoxItem.ToolTipProperty, binding);
            }
            PseudoColor_ComboBox.Items.Add(cbi);
        }

        private void CreateUserMacroControls ()
        {
            if (m_sysConfig.Profile != null)
            {
                for (int index = 0; index < m_sysConfig.Profile.UserMacros.Count; index++)
                {
                    Button button = CreateMacroButton(m_sysConfig.Profile.UserMacros[index]);
                    Macro_Toolbar.Items.Add(button);

                    MacroList.Items.Add(m_sysConfig.Profile.UserMacros[index]);
                }
            }
        }

        private Button CreateMacroButton (Macro macro)
        {
            Button button = new Button();
            button.Margin = new Thickness(1);
            button.Padding = new Thickness(0);
            Image image = new Image();
            image.Width = 32;
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/MacroOff.png", UriKind.Relative));
            button.Content = image;            
            button.ToolTip = macro.Name;
            button.Click += new RoutedEventHandler(Macro_button_Click);

            return button;
        }

        private void ColorMapping_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (PseudoColor_Panel != null)
            {
                ComboBox comboBox = sender as ComboBox;
                if (comboBox != null)
                {
                    ComboBoxItem comboBoxItem = comboBox.SelectedItem as ComboBoxItem;

                    if (comboBoxItem != null)
                    {
                        PseudoColor_Panel.Effect = null;

                        string name = comboBoxItem.Name.ToString();

                        if (!String.IsNullOrWhiteSpace(name) && name != "None")
                        {
                            string file = PseudoColorPath + name + ".lut";

                            ColorMappingEffect colorMappingEffect = new ColorMappingEffect();

                            ImageBrush colorMappingBrush = new ImageBrush();
                            colorMappingBrush.ImageSource = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));

                            colorMappingEffect.MappingTextureSampler = colorMappingBrush;

                            PseudoColor_Panel.Effect = colorMappingEffect;
                        }
                        else
                        {
                            name = "None";
                        }

                        HistoryPseudoColor pseudoColor = new HistoryPseudoColor();
                        pseudoColor.name = name;

                        if (!HistoryList.IsOpen && !IsMacroButtonClicked && !IsApplyHistoryFromSetup)
                        {
                            m_History.AddStep(pseudoColor);
                        }
                        IsMacroButtonClicked = false;
                        IsApplyHistoryFromSetup = false;
                    }
                }
            }
        }

        private void History_ListView_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            m_History.ApplyStep(listView.SelectedIndex);
        }

        private void MacroList_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            Macro macro = listView.SelectedItem as Macro;
            if (macro != null)
            {
                MacroNameField.Text = macro.Name;
            }
        }

        private void MacroList_MouseLeftButtonUp (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView listView = sender as ListView;
            Macro macro = listView.SelectedItem as Macro;
            if (macro != null)
            {
                MacroNameField.Text = macro.Name;
            }
        }

        private void SaveImage_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveImagePopup.IsOpen = !SaveImagePopup.IsOpen;
        }

        private void SaveImage_Opened(object sender, EventArgs e)
        {
            Save_Image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/SaveOn.png", UriKind.Relative));

            ImagePreview.GenerateLayers(MainImage.Source, _AdornerLayerManager, GetListOfEffects());
        }

        private void SaveImage_Closed(object sender, EventArgs e)
        {
            Save_Image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/SaveOff.png", UriKind.Relative));
        }

        private void Clear_DualEnergySettings ()
        {
            HEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/HEOff.png", UriKind.Relative));
            LEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/LEOff.png", UriKind.Relative));
            Trimat_Panel.Effect = null;
        }

        private void DualEnergy_Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                ApplyDualEnergy(button.Name, true);
            }
        }

        private void ApplyDualEnergy(string name, bool logHistory)
        {
            if (!String.IsNullOrWhiteSpace(name))
            {
                Clear_DualEnergySettings();

                if (name == "HighEnergy")
                {
                    if (m_ViewObject.HighEnergy != null)
                    {
                        MainImage.Source = m_ViewObject.HighEnergy.Source;
                        XrayImage_Panel.Effect = _XrayImageEffect;
                        HEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/HEOn.png", UriKind.Relative));
                    }
                }
                else if (name == "LowEnergy")
                {
                    if (m_ViewObject.LowEnergy != null)
                    {
                        MainImage.Source = m_ViewObject.LowEnergy.Source;
                        XrayImage_Panel.Effect = _XrayImageEffect;
                        LEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/LEOn.png", UriKind.Relative));
                    }
                }
                else if (name == "Trimat")
                {
                    if (AlgServerRequestEvent != null)
                    {
                        XrayImage_Panel.Effect = null;
                        AlgServerRequestEvent(this, new AlgServerRequestEventArgs(AlgServerRequest.TrimatImage));
                    }
                }
                else if (name == "Quadmat")
                {
                    if (AlgServerRequestEvent != null)
                    {
                        XrayImage_Panel.Effect = null;
                        AlgServerRequestEvent(this, new AlgServerRequestEventArgs(AlgServerRequest.QuadmatImage));
                    }
                }
                else if (name == "OrganicStrip")
                {
                    if (AlgServerRequestEvent != null)
                    {
                        XrayImage_Panel.Effect = null;
                        AlgServerRequestEvent(this, new AlgServerRequestEventArgs(AlgServerRequest.OrganicStripImage));
                    }
                }
                else if (name == "InorganicStrip")
                {
                    if (AlgServerRequestEvent != null)
                    {
                        XrayImage_Panel.Effect = null;
                        AlgServerRequestEvent(this, new AlgServerRequestEventArgs(AlgServerRequest.InOrganicStripImage));
                    }
                }
                else if (name == "MetalStrip")
                {
                    if (AlgServerRequestEvent != null)
                    {
                        XrayImage_Panel.Effect = null;
                        AlgServerRequestEvent(this, new AlgServerRequestEventArgs(AlgServerRequest.MetalStripImage));
                    }
                }

                if (logHistory)
                {
                    HistoryDualEnergy dualEnergy = new HistoryDualEnergy();
                    dualEnergy.name = name;
                    History.AddStep(dualEnergy);
                }
            }
        }


        private void Macro_button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                Macro macro = m_sysConfig.Profile.UserMacros.Find(button.ToolTip.ToString());
                if (macro != null)
                {
                    HistoryStep step = new HistoryStep();
                    step.number = -1;
                    step.category = "Macro";
                    step.description = button.ToolTip.ToString();
                    step.Buffer = macro.Buffer;
                    step.Filter = macro.Filter;
                    step.Histogram = macro.Histogram;
                    step.PseudoColor = macro.PseudoColor;

                    m_History.AddStep(step);

                    IsMacroButtonClicked = true;

                    m_History.ApplyStep();                   
                }
            }
        }

        private void History_View_Click (object sender, RoutedEventArgs e)
        {
            HistoryList.IsOpen = true;

            History_Image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/HistoryOn.png", UriKind.Relative));
        }

        private void ApplyHistory (HistoryStep step)
        {
            foreach (BufferInstance buffer in m_Buffers)
            {
                if (string.Compare(buffer.Instance.Name, step.Buffer.name, true) == 0)
                {
                    buffer.Instance.ApplyFilter(true);
                }
                else
                {
                    buffer.Instance.ApplyFilter(false);
                }
            }

            foreach (ComboBoxItem cbi in PseudoColor_ComboBox.Items)
            {
                if (string.Compare(cbi.Name, step.PseudoColor.name, true) == 0)
                {
                    PseudoColor_ComboBox.SelectedItem = cbi;
                    break;
                }
            }

            foreach (HistoryFilter element in step.Filter)
            {
                FilterInstance filter = m_Filters.Find(element.name);

                if (filter != null)
                {
                    filter.Instance.ApplyFilter(element.parameter, element.optparameter1);
                }
            }

            if (step.DualEnergy != null)
            {
                ApplyDualEnergy(step.DualEnergy.name, false);
            }
        }

        private void SaveImageCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveImagePopup.IsOpen = false;
        }

        private void SaveImagePrintBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveImagePopup.IsOpen = false;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Bitmap files (*.bmp)|*.bmp|Jpeg files (*.jpg)|*.jpg|Tiff files (*.tif)|*.tif|PNG files (*.png)|*.png";
            dlg.AddExtension = true;
            dlg.DefaultExt = "jpg";
            dlg.FilterIndex = 2;
            BitmapEncoder be;
            FileStream fs;
            if (dlg.ShowDialog() == true)//System.Windows.Forms.DialogResult.OK)
            {
                var bsource = MainImage.Source as BitmapSource;
                BitmapSource bms = XRayImageRenderer.GetRenderedXrayImage(bsource,
                                        m_SourceObject,
                                        m_ViewObject,
                                        (ShaderEffect)XrayImage_Panel.Effect,
                                        GetListOfEffects(),
                                        bsource.Width,
                                        bsource.Height,
                                        _AdornerLayerManager,
                                        ImagePreview.IsAnnotationShown,
                                        ImagePreview.IsMeasurementShown);

                string fname = dlg.FileName;
                string ext = System.IO.Path.GetExtension(fname).ToLower();

                fs = new FileStream(fname, FileMode.Create);
                switch (ext)
                {
                    case ".bmp":
                        be = new BmpBitmapEncoder();
                        be.Frames.Add(BitmapFrame.Create(bms));
                        be.Save(fs);
                        break;
                    case ".jpg":
                        be = new JpegBitmapEncoder();
                        be.Frames.Add(BitmapFrame.Create(bms));
                        be.Save(fs);
                        break;
                    case ".png":
                        be = new PngBitmapEncoder();
                        be.Frames.Add(BitmapFrame.Create(bms));
                        be.Save(fs);
                        break;
                    case ".tif":
                        be = new TiffBitmapEncoder();
                        be.Frames.Add(BitmapFrame.Create(bms));
                        be.Save(fs);
                        break;
                }
                fs.Close();
            }
        }

        private BitmapSource GetRenderedXrayImage ()
        {
            BitmapSource bsource = MainImage.Source as BitmapSource;

            return XRayImageRenderer.GetRenderedXrayImage(bsource,
                m_SourceObject,
                m_ViewObject,
                (ShaderEffect)XrayImage_Panel.Effect,
                GetListOfEffects(),
                PanZoom_Border.ActualWidth,
                PanZoom_Border.ActualHeight,
                _AdornerLayerManager,
                IsAnnotationsShowing(),
                IsMeasurementsShowing());
        }

        private List<System.Windows.Media.Effects.Effect> GetListOfEffects()
        {
            List<Effect> effects = new List<Effect>();

            FrameworkElement element = MainImage;
            // loop up from the image to the pan and zoom border
            while (element != PanZoom_Border)
            {
                effects.Add(element.Effect);
                element = (FrameworkElement)element.Parent;
            }
            // reverse the list because we went in the opposite order
            effects.Reverse();
            return effects;
        }

        private void MacroEdit_Button_Click (object sender, RoutedEventArgs e)
        {
            MacroEditPopup.IsOpen = true;
            MacroEdit_Text.Foreground = Brushes.Red;
            MacroEdit_Text.FontWeight = FontWeights.Bold;
        }

        private void AddMacro_Click (object sender, RoutedEventArgs e)
        {
            string macroName = MacroNameField.Text;

            MacroNameField.Text = string.Empty;

            Macro macro = m_sysConfig.Profile.UserMacros.Find(macroName);

            if (String.IsNullOrWhiteSpace(macroName))
            {
                MessageBox.Show(L3.Cargo.Common.Resources.PleaseEnterANameForTheMacro);
            }
            else if (m_sysConfig.Profile.UserMacros.IsFull && macro == null)
            {
                MessageBox.Show(L3.Cargo.Common.Resources.MaxNumberOfMacrosHaveBeenMetPleaseReplaceOneOfTheMacros);
            }
            else
            {
                try
                {
                    Macro newMacro = new Macro(macroName, m_History.GetStep());

                    if (macro != null)
                    {
                        m_sysConfig.Profile.UserMacros.Update(newMacro);
                    }
                    else
                    {
                        m_sysConfig.Profile.UserMacros.Add(newMacro);

                        Macro_Toolbar.Items.Add(CreateMacroButton(newMacro));

                        MacroList.Items.Add(newMacro);
                    }
                }
                catch (Exception ex)
                {
                    // threw this while adding the macro why?  TODO: Log this
                    throw ex;
                }
            }

        }

        private void RemoveMacro_Click (object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(MacroNameField.Text))
            {
                for (int index = MacroList.Items.Count - 1; index >= 0; index--)
                {
                    Macro macro = MacroList.Items[index] as Macro;

                    if (macro != null && String.Compare(macro.Name, MacroNameField.Text, true) == 0)
                    {
                        MacroList.Items.RemoveAt(index);
                        break;
                    }
                }

                for (int index = Macro_Toolbar.Items.Count - 1; index >= 0; index--)
                {
                    Button button = Macro_Toolbar.Items[index] as Button;

                    if (button != null && String.Compare(button.ToolTip.ToString(), MacroNameField.Text, true) == 0)
                    {
                        Macro_Toolbar.Items.RemoveAt(index);
                        break;
                    }
                }

                m_sysConfig.Profile.UserMacros.Remove(MacroNameField.Text);

                MacroNameField.Text = string.Empty;
            }
        }

        private void HistoryList_Closed (object sender, EventArgs e)
        {
            int index = History_ListView.SelectedIndex;

            if ((m_History.Step.Count - 1) != index)
            {
                HistoryStep step = m_History.GetStep(index);
                step.number = -1;
                step.category = "Reapply";
                step.description = "Step " + index.ToString();
                m_History.AddStep(step);
            }

            History_Image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/HistoryOff.png", UriKind.Relative));
        }

        private void HistoryList_Opened (object sender, EventArgs e)
        {
            ObservableList<object> newContext = new ObservableList<object>();

            foreach (HistoryStep step in m_History.Step)
            {
                // translate the entries
                string cat = L3.Cargo.Common.Resources.ResourceManager.GetString(step.category.Replace(" ", ""), CultureResources.getCultureSetting());
                if (cat == null)
                {
                    // if there is no resource string for the key just use the key
                    cat = step.category;
                }
                string desc = L3.Cargo.Common.Resources.ResourceManager.GetString(step.description.Replace(" ",""), CultureResources.getCultureSetting());
                if (desc == null)
                {
                    if (step.description.StartsWith("Step"))
                    {
                        desc = L3.Cargo.Common.Resources.Step + " ";
                        desc += step.description.Substring(desc.Length);
                    }
                    else if (step.description.StartsWith("Range:"))
                    {
                        desc = L3.Cargo.Common.Resources.Range_Colon + " ";
                        desc += step.description.Substring(desc.Length);
                    }
                    else
                    {
                        // if there is no resource string for the key just use the key
                        desc = step.description;
                    }
                }

                var entry = new { number = step.number, category = cat, description = desc };
                newContext.Add(entry);
            }
            History_ListView.DataContext = newContext;
            History_ListView.SelectedIndex = (m_History.Step.Count - 1);

            History_ListView.InvalidateVisual();
        }

        private void MacroEditPopup_Closed(object sender, EventArgs e)
        {
            MacroEdit_Text.Foreground = Brushes.Black;
            MacroEdit_Text.FontWeight = FontWeights.DemiBold;
        }

        #endregion Private Methods


        #region Public Methods

        public List<Annotation> GetAnnotations()
        {
            try
            {
                return (_AdornerLayerManager.GetAdorner(AdornerLayerManager.ANNOTATION_ADORNER) as AnnotationAdorner).GetAnnotations();
            }
            catch (KeyNotFoundException)
            {
                return new List<Annotation>();
            }
        }

        public bool IsAnnotationsShowing()
        {
            return _AdornerLayerManager.IsShowing(AdornerLayerManager.ANNOTATION_ADORNER);
        }

        public bool IsMeasurementsShowing()
        {
            return _AdornerLayerManager.IsShowing(AdornerLayerManager.MEASUREMENT_ADORNER);
        }

        public void Setup (ViewObject content, History history, SysConfiguration sysConfig)
        {
            if (!IsSetup)
            {
                m_ViewObject = content;

                m_History = history;
                m_History.CurrentHistoryChangedEvent += new CurrentHistoryChanged(ApplyHistory);

                if (m_ViewObject.HighEnergy != null)
                {
                    m_SourceObject = m_ViewObject.HighEnergy;
                    HEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/HEOn.png", UriKind.Relative));
                    LEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/LEOff.png", UriKind.Relative));
                }
                else if (m_ViewObject.LowEnergy != null)
                {
                    m_SourceObject = m_ViewObject.LowEnergy;
                    HEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/HEOff.png", UriKind.Relative));
                    LEImage.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/LEOn.png", UriKind.Relative));
                }
                else
                {
                    throw new Exception();
                }

                if (m_ViewObject.LowEnergy == null ||
                    m_ViewObject.HighEnergy == null)
                {
                    XRayDualEnergy_ToolBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    HistoryDualEnergy dualEnergy = new HistoryDualEnergy();
                    dualEnergy.name = "HighEnergy";
                    m_History.SetFirstStep(dualEnergy);
                }

                if (sysConfig != null)
                {
                    m_sysConfig = sysConfig;
                    CreateUserMacroControls();
                    Macro_Toolbar.Visibility = Visibility.Visible;
                }

                CreateBufferControls();

                CreateFilterControls();

                CreateColorMappingControls();

                MainImage.Source = m_SourceObject.Source;
                MainImage.Height = m_SourceObject.Height;
                MainImage.Width = m_SourceObject.Width;
                
                HistoryBuffer buffer = new HistoryBuffer();
                buffer.name = "Original Gray Scale";

                m_History.SetFirstStep(buffer);

                HistoryList.DataContext = m_History.Step;

                IsApplyHistoryFromSetup = true;

                m_History.ApplyStep();

                IsApplyHistoryFromSetup = false;

                IsSetup = true;
            }
        }

        public void CollapseDualEnergy(bool collapse)
        {
            XRayDualEnergy_ToolBar.Visibility = collapse ? Visibility.Collapsed : Visibility.Visible;
        }

        public void Dispose ()
        {
            CultureResources.getDataProvider().DataChanged -= this.CultureResources_DataChanged;
            HistoryList.IsOpen = false;

            PseudoColor_ComboBox.Items.Clear();
            PseudoColor_ComboBox = null;

            m_Filters.Clear();
            m_Filters = null;

            m_FilterManager.Dispose();
            m_FilterManager = null;

            m_SourceObject = null;
            m_History = null;

            PanAndZoomControl.Content = null;
            PanAndZoomControl = null;

            m_ViewObject.Dispose();
            m_ViewObject = null;
        }

        public void XRayView_ContextMenu_Changed(bool loaded)
        {
            // fix for annotations when the main context menu is open and a user clicks
            (AdornerLayerManager.GetAdorner(AdornerLayerManager.ANNOTATION_ADORNER) as AnnotationAdorner).MainContextMenuOpen = loaded;
        }

        #endregion Public Methods
    }
}
