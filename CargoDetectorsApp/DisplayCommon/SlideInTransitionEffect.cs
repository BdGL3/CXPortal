
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.IO;
using System.Reflection;


namespace L3.Cargo.Detectors.Display.Common
    {
	
	/// <summary>A transition effect </summary>
	public class SlideInTransitionEffect : ShaderEffect
    {
        #region Public Members

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof                                           (SlideInTransitionEffect), 0);
		public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof                                         (SlideInTransitionEffect), new UIPropertyMetadata(((double)(30D)), PixelShaderConstantCallback(0)));
		public static readonly DependencyProperty SlideAmountProperty = DependencyProperty.Register("SlideAmount", typeof(Point), typeof                                    (SlideInTransitionEffect), new UIPropertyMetadata(new Point(1D, 0D), PixelShaderConstantCallback(1)));
		public static readonly DependencyProperty Texture2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture2", typeof                                     (SlideInTransitionEffect), 1);

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

        public double Progress
        {
            get
            {
                return ((double)(this.GetValue(ProgressProperty)));
            }
            set
            {
                this.SetValue(ProgressProperty, value);
            }
        }

        public Point SlideAmount
        {
            get
            {
                return ((Point)(this.GetValue(SlideAmountProperty)));
            }
            set
            {
                this.SetValue(SlideAmountProperty, value);
            }
        }

        public Brush Texture2
        {
            get
            {
                return ((Brush)(this.GetValue(Texture2Property)));
            }
            set
            {
                this.SetValue(Texture2Property, value);
            }
        }

        #endregion


        #region Constructor

        public SlideInTransitionEffect() 
        {
			PixelShader pixelShader = new PixelShader();
            //string fullPath = System.Reflection.Assembly.GetCallingAssembly().Location + ";component/Transition_SlideIn.ps";
            string fullpath = @"/L3.Cargo.Detectors.Display.Common;component/Transition_SlideIn.ps";

            pixelShader.UriSource = new Uri(fullpath, UriKind.RelativeOrAbsolute);
           
			this.PixelShader = pixelShader;

			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(ProgressProperty);
			this.UpdateShaderValue(SlideAmountProperty);
			this.UpdateShaderValue(Texture2Property);
		}

        #endregion

    }
}
