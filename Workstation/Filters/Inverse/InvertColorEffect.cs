using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace L3.Cargo.Workstation.Filters.Invert
{
	
	/// <summary>An effect that inverts all colors.</summary>
	public class InvertColorEffect : ShaderEffect {
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(InvertColorEffect), 0);
		public InvertColorEffect() {
            try
            {
                PixelShader pixelShader = new PixelShader();
                pixelShader.UriSource = new Uri(@"/L3Filter-Invert;component/InvertColor.ps", UriKind.Relative);
                this.PixelShader = pixelShader;

                this.UpdateShaderValue(InputProperty);
            }
            catch
            {
                Exception ex = new Exception();
                string em = ex.Message.ToString();  
            }
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
	}
}
