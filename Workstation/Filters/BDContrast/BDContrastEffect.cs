using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace L3.Cargo.Workstation.Filters.BDContrast
{
	
	/// <summary>An effect that simultaneously adjusts the brightness/darkness and contrast of the input.</summary>
	public class BDContrastEffect : ShaderEffect {
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BDContrastEffect), 0);
		public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(BDContrastEffect), new UIPropertyMetadata(((double)(0D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(BDContrastEffect), new UIPropertyMetadata(((double)(1D)), PixelShaderConstantCallback(1)));
        public BDContrastEffect()
        {
			PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri(@"/L3Filter-BDContrast;component/BDContrast.ps", UriKind.Relative);
			this.PixelShader = pixelShader;

			this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(BrightnessProperty);
            this.UpdateShaderValue(ContrastProperty);
        }
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		/// <summary>Brightness/Darkness parameter, range: -1..1</summary>
		public double Brightness {
			get {
				return ((double)(this.GetValue(BrightnessProperty)));
			}
			set {
				this.SetValue(BrightnessProperty, value);
			}
		}
        /// <summary>Contrast parameter, range 0..2</summary>
        public double Contrast
        {
            get
            {
                return ((double)(this.GetValue(ContrastProperty)));
            }
            set
            {
                this.SetValue(ContrastProperty, value);
            }
        }
    }
}
