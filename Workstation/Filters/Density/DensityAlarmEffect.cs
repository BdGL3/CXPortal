using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;

namespace L3.Cargo.Workstation.Filters.DensityAlarm
{
    class DensityAlarmEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(DensityAlarmEffect), 0);
        public static readonly DependencyProperty SampleIProperty = DependencyProperty.Register("SampleI", typeof(double), typeof(DensityAlarmEffect), new UIPropertyMetadata(((double)(0.05D)), PixelShaderConstantCallback(0)));
        public DensityAlarmEffect()
        {
			PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri(@"/L3Filter-DensityAlarm;component/DensityAlarm.ps", UriKind.Relative);
			this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(SampleIProperty);
		}
        public Brush Input
        {
            get
            {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set
            {
                this.SetValue(InputProperty, value);
            }
        }
        public double SampleI
        {
            get
            {
                return ((double)(this.GetValue(SampleIProperty)));
            }
            set
            {
                this.SetValue(SampleIProperty, value);
            }
        }
    }
}
