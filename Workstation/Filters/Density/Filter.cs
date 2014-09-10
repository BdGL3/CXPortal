using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Controls.Primitives;
using L3.Cargo.Common.Xml.History_1_0;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using L3.Cargo.Common;
using System.Windows.Media;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Filters.DensityAlarm
{
    public class DensityAlarm : IFilter
    {
        #region Private Members

        private DockPanel dockPanel;

        private Int32 Width;

        private Int32 Height;

        private History m_History;

        private Button m_ToolBarItem;

        private Button m_FilterButton;

        private Button m_PopupButton;

        private Popup m_PopUpWin;

        private Slider m_ValueSlider;

        private TextBox m_ValTextBox;

        private ProfileObject m_profile;

        private bool m_EffectChanged = false;

        private double m_defaultDensityDefaultValue;

        private bool m_defaultDensitySetOnCaseOpen;

        private string m_Name = "DensityAlarm";

        private string m_Version = "1.0.0";

        #endregion Private Members


        #region Public Members

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public string Version
        {
            get { return m_Version; }
        }

        public UIElement ToolBarItem
        {
            get
            {
                return m_ToolBarItem;
            }
        }

        #endregion Public Members


        #region Constructors

        public DensityAlarm()
        {
            StackPanel buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            var gsc = new GradientStopCollection(3);
            gsc.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#EFEFEF"), 0.0));
            gsc.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#CFCFCF"), 0.5));
            gsc.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#EFEFEF"), 1.0));
            var lgb = new LinearGradientBrush(gsc);
            buttonStackPanel.Background = lgb;

            m_FilterButton = new Button();
            m_FilterButton.Margin = new Thickness(1);
            m_FilterButton.Padding = new Thickness(-1);
            Image image = new Image();
            image.Width = 32;
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3Filter-DensityAlarm;component/DensityAlarmOff.png", UriKind.Relative));
            m_FilterButton.Content = buttonStackPanel;

            buttonStackPanel.Children.Add(image);


            m_ToolBarItem = m_FilterButton;

            // Bind the tool tip to the resource
            var binding = new Binding("DensityAlarm");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(m_ToolBarItem, Button.ToolTipProperty, binding);

            m_PopupButton = new Button();
            m_PopupButton.Margin = new Thickness(-2, 1, 1, 1);
            m_PopupButton.Padding = new Thickness(-2, -3, -3, -3);
            m_PopupButton.BorderBrush = null;
            m_PopupButton.Focusable = false;
            image = new Image();
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3Filter-DensityAlarm;component/DownArrow.png", UriKind.Relative));
            m_PopupButton.Content = image;

            buttonStackPanel.Children.Add(m_PopupButton);

            m_FilterButton.Click += new RoutedEventHandler(m_FilterButton_Click);
            m_PopupButton.Click += new RoutedEventHandler(m_PopupButton_Click);

            m_ValueSlider = new Slider();
            m_ValueSlider.Background = Brushes.White;

            m_ValueSlider.Margin = new Thickness(8);
            m_ValueSlider.Width = 200;
            m_ValueSlider.Height = 22;
            m_ValueSlider.Maximum = 100;
            m_ValueSlider.Minimum = 0;
            m_ValueSlider.TickFrequency = 1;
            m_ValueSlider.IsSnapToTickEnabled = true;
            m_ValueSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(UIControl_ValueChanged);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Background = Brushes.White;
            stackPanel.Children.Add(m_ValueSlider);

            m_ValTextBox = new TextBox();
            m_ValTextBox.IsEnabled = false;
            m_ValTextBox.Width = 50;
            m_ValTextBox.Height = 22;
            m_ValTextBox.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(m_ValTextBox);


            Button resetDefaultsButton = new Button();
            resetDefaultsButton.Margin = new Thickness(8);
            resetDefaultsButton.Padding = new Thickness(5, 0, 5, 0);
            resetDefaultsButton.Height = 22;
            binding = new Binding("ResetDefaults");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(resetDefaultsButton, Button.ContentProperty, binding);
            resetDefaultsButton.Click += new RoutedEventHandler(resetDefaults_Click);
            stackPanel.Children.Add(resetDefaultsButton);

            m_PopUpWin = new Popup();
            m_PopUpWin.PlacementTarget = m_ToolBarItem;
            m_PopUpWin.Placement = PlacementMode.Bottom;
            m_PopUpWin.StaysOpen = false;
            m_PopUpWin.Child = stackPanel;
            m_PopUpWin.Margin = new Thickness(5);
            m_PopUpWin.Closed += new EventHandler(m_PopUpWin_Closed);

        }

        #endregion Constructors


        #region Private Methods

        private void m_FilterButton_Click(Object sender, RoutedEventArgs e)
        {
            m_ValueSlider.IsEnabled = false;
            // toggle the effect
            if (dockPanel.Effect == null)
            {
                ApplyFilter(m_ValueSlider.Value / 100.0);
            }
            else
            {
                ApplyFilter(0.0);
            }
            m_ValueSlider.IsEnabled = true;
        }

        private void m_PopupButton_Click(object sender, RoutedEventArgs e)
        {
            m_PopUpWin.IsOpen = !m_PopUpWin.IsOpen;
            e.Handled = true;
        }

        private void resetDefaults_Click(object sender, RoutedEventArgs e)
        {
            m_ValueSlider.Value = m_defaultDensityDefaultValue * 100.0;
            // clear the value from the profile
            SaveToProfile(0.0);
        }

        private void UIControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_ValueSlider.IsEnabled)
            {
                m_ValTextBox.Text = (m_ValueSlider.Value).ToString("F0") + " %";
                ApplyFilter(m_ValueSlider.Value / 100.0);
                SaveToProfile(m_ValueSlider.Value / 100.0);
            }
        }

        private void m_PopUpWin_Closed (object sender, EventArgs e)
        {
            if (m_EffectChanged)
            {
                m_addHistoryStep();
            }
            m_EffectChanged = false;
        }

        private void m_addHistoryStep()
        {
            HistoryFilter filter = new HistoryFilter();
            filter.name = m_Name;
            filter.parameter = (m_ValueSlider.Value / 100.0).ToString();
            m_History.AddStep(filter);
        }

        #endregion Private Methods


        #region Public Methods

        public void Initialize(Object passedObj)
        {
            FilterParameter filterParam = passedObj as FilterParameter;
            if (filterParam != null)
            {
                dockPanel = filterParam.dockPanel;
                Width = filterParam.Width;
                Height = filterParam.Height;
                m_History = filterParam.History;
                if (filterParam.SysConfig != null)
                {
                    m_defaultDensityDefaultValue = filterParam.SysConfig.DensityAlarmDefaultValue;
                    m_defaultDensitySetOnCaseOpen = filterParam.SysConfig.DensityAlarmSetOnCaseOpen;
                    m_profile = filterParam.SysConfig.Profile;
                }
                else
                {
                    m_defaultDensityDefaultValue = 0.0;
                    m_defaultDensitySetOnCaseOpen = false;
                }
                

                HistoryFilter filter = new HistoryFilter();
                filter.name = m_Name;
                if (m_defaultDensitySetOnCaseOpen)
                {
                    if (m_profile != null && m_profile.DensityAlarmValue != 0.0)
                    {
                        filter.parameter = m_profile.DensityAlarmValue.ToString();
                    }
                    else
                    {
                        filter.parameter = m_defaultDensityDefaultValue.ToString();
                    }
                }
                else
                {
                    filter.parameter = "0.0";
                }
                m_History.SetFirstStep(filter);

                m_ValueSlider.IsEnabled = false;
                if (m_profile != null && m_profile.DensityAlarmValue != 0.0)
                {
                    m_ValueSlider.Value = m_profile.DensityAlarmValue * 100.0;
                }
                else
                {
                    m_ValueSlider.Value = m_defaultDensityDefaultValue * 100.0;
                }
                m_ValueSlider.IsEnabled = true;

                m_ValTextBox.Text = (m_ValueSlider.Value).ToString("F0") + " %";
            }
        }

        public void ApplyFilter (Object passedObj, Object dontCare = null)
        {
            double value = Convert.ToDouble(passedObj);

            if (value == 0)
            {
                Image image = (m_FilterButton.Content as StackPanel).Children[0] as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-DensityAlarm;component/DensityAlarmOff.png", UriKind.Relative));

                if (dockPanel.Effect != null)
                {
                    dockPanel.Effect = null;
                    m_EffectChanged = true;
                }
            }
            else
            {
                Image image = (m_FilterButton.Content as StackPanel).Children[0] as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-DensityAlarm;component/DensityAlarmOn.png", UriKind.Relative));

                DensityAlarmEffect densityEffect = new DensityAlarmEffect();
                densityEffect.SampleI = value;

                if (dockPanel.Effect == null || (dockPanel.Effect as DensityAlarmEffect).SampleI != value)
                {
                    dockPanel.Effect = densityEffect;
                    m_EffectChanged = true;
                }
            }

            //var currSliderVal = m_ValueSlider.Value / 100.0;
            //if (currSliderVal != value && m_ValueSlider.IsEnabled)
            //{
            //    m_ValueSlider.IsEnabled = false;
            //    m_ValueSlider.Value = value * 100.0;
            //    m_ValTextBox.Text = (m_ValueSlider.Value).ToString("F0") + " %";
            //    m_ValueSlider.IsEnabled = true;
            //}
        }

        private void SaveToProfile(double value)
        {
            if (m_profile != null)
            {
                m_profile.DensityAlarmValue = value;
            }
        }

        public void Dispose()
        {
        }

        #endregion Public Methods
    }
}
