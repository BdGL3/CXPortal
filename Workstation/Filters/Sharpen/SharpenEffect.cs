using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace L3.Cargo.Workstation.Filters.Sharpen
{
	
	/// <summary>An effect that sharpens the input.</summary>
	public class SharpenEffect : ShaderEffect {
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(SharpenEffect), 0);
		public static readonly DependencyProperty AmountProperty = DependencyProperty.Register("Amount", typeof(double), typeof(SharpenEffect), new UIPropertyMetadata(((double)(1)), PixelShaderConstantCallback(0)));
		public static readonly DependencyProperty InputSizeProperty = DependencyProperty.Register("InputSize", typeof(Size), typeof(SharpenEffect), new UIPropertyMetadata(new Size(800, 600), PixelShaderConstantCallback(1)));
		public SharpenEffect() {
			PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri(@"/L3Filter-Sharpen;component/sharpenA.ps", UriKind.Relative);
			this.PixelShader = pixelShader;

			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(AmountProperty);
			this.UpdateShaderValue(InputSizeProperty);
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		/// <summary>The amount of sharpening.</summary>
		public double Amount {
			get {
				return ((double)(this.GetValue(AmountProperty)));
			}
			set {
				this.SetValue(AmountProperty, value);
			}
		}
		/// <summary>The size of the input (in pixels).</summary>
		public Size InputSize {
			get {
				return ((Size)(this.GetValue(InputSizeProperty)));
			}
			set {
				this.SetValue(InputSizeProperty, value);
			}
		}
	}
}
