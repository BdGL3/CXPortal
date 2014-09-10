using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for TruckModel.xaml
    /// </summary>
    public partial class TruckModel : UserControl
    {
        #region Private Members

        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;


        private bool _HBoomDeploySwitch;

        private bool _HBoomStowSwitch;

        private bool _MastDeploySwitch;

        private bool _MastStowSwitch;

        private bool _VBoomDeploySwitch;

        private bool _VBoomStowSwitch;


        private bool _LinacDeployLHS;

        private bool _LinacDeployRHS;

        private bool _LinacStowSwitch;

        #endregion Private Members


        #region Constructors

        public TruckModel (WidgetStatusHost widgetStatusHost, Dispatcher dispatcher)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
            _WidgetStatusHost.ErrorMessageUpdate += new UpdateErrorMessageHandler(_WidgetStatusHost_ErrorMessageUpdate);
        }

        private void _WidgetStatusHost_ErrorMessageUpdate(string[] messages)
        {
            if (messages.Length > 0)
            {
                ErrorFadeBorder.Fade(true);
            }
            else
            {
                ErrorFadeBorder.Fade(false);
            }
        }

        #endregion Constructors


        #region Private Members

        private void WidgetUpdate (string name, int value)
        {
            if (name.Contains("ESTOP") || name.Contains("INTERLOCK"))
            {
                UpdateSafetyDisplays(name, value);
            }
            else if (name.Equals(OpcTags.HORIZONTAL_BOOM_ANGLE.Name))
            {
                SetAngle(HorizontalAngle, value);
            }
            else if (name.Equals(OpcTags.HORIZONTAL_BOOM_DEPLOY_AT_POSITION.Name))
            {
                _HBoomDeploySwitch = Convert.ToBoolean(value);
                SetDeploymentMaterial(Horizontal_Boom, _HBoomDeploySwitch, _HBoomStowSwitch);
            }
            else if (name.Equals(OpcTags.HORIZONTAL_BOOM_STOW_AT_POSITION.Name))
            {
                _HBoomStowSwitch = Convert.ToBoolean(value);
                SetDeploymentMaterial(Horizontal_Boom, _HBoomDeploySwitch, _HBoomStowSwitch);
            }
            else if (name.Equals(OpcTags.MAST_ANGLE.Name))
            {
                SetAngle(MastAngle, value);
            }
            else if (name.Equals(OpcTags.MAST_DEPLOY_AT_POSITION.Name))
            {
                _MastDeploySwitch = Convert.ToBoolean(value);
                SetDeploymentMaterial(Mast, _MastDeploySwitch, _MastStowSwitch);
            }
            else if (name.Equals(OpcTags.MAST_STOW_AT_POSITION.Name))
            {
                _MastStowSwitch = Convert.ToBoolean(value);
                SetDeploymentMaterial(Mast, _MastDeploySwitch, _MastStowSwitch);
            }
            else if (name.Equals(OpcTags.VERTICAL_BOOM_ANGLE.Name))
            {
                SetAngle(VerticalAngle, value);
            }
            else if (name.Equals(OpcTags.VERTICAL_BOOM_DEPLOY_AT_POSITION.Name))
            {
                _VBoomDeploySwitch = Convert.ToBoolean(value);
                SetDeploymentMaterial(Vertical_Boom, _VBoomDeploySwitch, _VBoomStowSwitch);
            }
            else if (name.Equals(OpcTags.VERTICAL_BOOM_STOW_AT_POSITION.Name))
            {
                _VBoomStowSwitch = Convert.ToBoolean(value);
                SetDeploymentMaterial(Vertical_Boom, _VBoomDeploySwitch, _VBoomStowSwitch);
            }
            else if (name.Equals(OpcTags.LINAC_DEPLOY_LHS_AT_POSITION.Name))
            {
                _LinacDeployLHS = Convert.ToBoolean(value);
                SetLinacDeployPosition();
            }
            else if (name.Equals(OpcTags.LINAC_DEPLOY_RHS_AT_POSITION.Name))
            {
                _LinacDeployRHS = Convert.ToBoolean(value);
                SetLinacDeployPosition();
            }
            else if (name.Equals(OpcTags.LINAC_STOW_AT_POSITION.Name))
            {
                _LinacStowSwitch = Convert.ToBoolean(value);
                SetLinacDeployPosition();
            }
        }

        private void UpdateSafetyDisplays (string name, int value)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                GeometryModel3D model = FindName(name) as GeometryModel3D;
                if (model != null)
                {
                    if ((value != 1 && value != 2) && name.Contains("ESTOP"))
                    {
                        model.Material = null;
                    }
                    else if ((value != 1 && value != 2) && name.Contains("INTERLOCK"))
                    {
                        model.Material = (Material)FindResource("M_Interlock");
                    }
                    else if (value == 1)
                    {
                        model.Material = (MaterialGroup)FindResource("M_Error");
                    }
                    else if (value == 2)
                    {
                        model.Material = (MaterialGroup)FindResource("M_Warning");
                    }
                }
            }));
        }

        private void SetAngle (AxisAngleRotation3D model, int value)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                model.SetCurrentValue(AxisAngleRotation3D.AngleProperty, (90.0 - (value/100.0)));
            }));
        }

        private void SetMaterial (GeometryModel3D model, string materialResource)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                model.Material = (Material)FindResource(materialResource);
            }));
        }

        private void SetDeploymentMaterial (GeometryModel3D model, bool deploySwitch, bool stowSwitch)
        {
            string material = (deploySwitch || stowSwitch) ? "M_BoomLinac" : "M_BoomLinacMoving";
            SetMaterial(model, material);
        }

        private void SetCurrentValue (DependencyObject tranformation, DependencyProperty dp, double value)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                tranformation.SetCurrentValue(dp, value);
            }));
        }

        private void SetLinacDeployPosition ()
        {
            if (_LinacDeployLHS || _LinacDeployRHS)
            {
               SetCurrentValue(LinacRotation, AxisAngleRotation3D.AngleProperty, 15.0);
               SetCurrentValue(LinacOffSet, TranslateTransform3D.OffsetXProperty, -0.65);
            }
            else if (_LinacStowSwitch)
            {
               SetCurrentValue(LinacRotation, AxisAngleRotation3D.AngleProperty, 0);
               SetCurrentValue(LinacOffSet, TranslateTransform3D.OffsetXProperty, 0);
            }
            else
            {
               SetCurrentValue(LinacRotation, AxisAngleRotation3D.AngleProperty, 7.5);
               SetCurrentValue(LinacOffSet, TranslateTransform3D.OffsetXProperty, -0.325);
            }

               SetDeploymentMaterial(LinacObject, (_LinacDeployLHS && _LinacDeployRHS), _LinacStowSwitch);
        }

        #endregion Private Methods
    }
}
