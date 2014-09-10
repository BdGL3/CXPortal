using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using L3.Cargo.Workstation.Common;
using L3.Cargo.Common;
using System.Globalization;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;
using L3.Cargo.Controls;
using System.Windows.Documents;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    class XRayImageRenderer
    {
        /// <summary>
        /// Renders the Xray image on the CPU.
        /// </summary>
        /// <param name="bsource"></param>
        /// <param name="sourceObject"></param>
        /// <param name="viewObject"></param>
        /// <param name="xrayImageEffect"></param>
        /// <param name="trimatEffect"></param>
        /// <param name="effects"></param>
        /// <param name="displayWidth"></param>
        /// <param name="displayHeight"></param>
        /// <param name="adornerLayerManager"></param>
        /// <param name="displayAnnotations"></param>
        /// <param name="displayMeasurements"></param>
        /// <returns>The newly rendered bitmap.</returns>
        public static BitmapSource GetRenderedXrayImage(BitmapSource bsource,
                                                        SourceObject sourceObject,
                                                        ViewObject viewObject,
                                                        ShaderEffect xrayOrTrimateImageEffect,
                                                        List<Effect> effects,
                                                        double displayWidth,
                                                        double displayHeight,
                                                        AdornerLayerManager adornerLayerManager,
                                                        bool displayAnnotations,
                                                        bool displayMeasurements)
        {
            if (xrayOrTrimateImageEffect != null && xrayOrTrimateImageEffect.GetType() == typeof(XrayImageEffect))
            {
                RenderXrayImage_XRayImageEffect(ref bsource, (XrayImageEffect)xrayOrTrimateImageEffect, sourceObject);
            }
            //else if (xrayOrTrimateImageEffect.GetType() == typeof(TrimatEffect))
            //{
            //    RenderXrayImage_TrimatEffect(ref bsource, (TrimatEffect)xrayOrTrimateImageEffect, sourceObject, viewObject);
            //}
            foreach (Effect effect in effects)
            {
                ApplyEffectToBitmap(ref bsource, effect);
            }
            if (displayAnnotations)
            {
                RenderAnnotations(ref bsource, displayWidth, displayHeight, adornerLayerManager);
            }
            if (displayMeasurements)
            {
                RenderMeasurements(ref bsource, displayWidth, displayHeight, adornerLayerManager);
            }

            return bsource;
        }

        public static void RenderXrayImage_XRayImageEffect(ref BitmapSource bsource, XrayImageEffect xrayImageEffect, SourceObject sourceObject)
        {
            //BitmapSource bsource = XRay_Image.Source as BitmapSource;

            PixelFormat pixelFormat = PixelFormats.Bgr24;

            int pixelOffset = pixelFormat.BitsPerPixel / 8;
            int stride = sourceObject.Width * pixelOffset;

            byte[] newData = new byte[sourceObject.Data.Length * pixelOffset];

            double min = xrayImageEffect.LowerBound;
            double max = xrayImageEffect.UpperBound;

            Color lowerBoundColor = xrayImageEffect.LowerBoundColor;
            Color upperBoundColor = xrayImageEffect.UpperBoundColor;

            bool enableSQRT = xrayImageEffect.EnableSquareroot > 0 ? true : false;
            bool enableSQRE = xrayImageEffect.EnableSquare > 0 ? true : false;

            Parallel.For(0, sourceObject.Data.Length, i =>
            {
                float data = sourceObject.Data[i];
                Color color = new Color();

                if (data < min)
                {
                    color.ScB = (data * lowerBoundColor.ScB);
                    color.ScG = (data * lowerBoundColor.ScG);
                    color.ScR = (data * lowerBoundColor.ScR);
                }
                else if (data > max)
                {
                    if (upperBoundColor == Colors.White)
                    {
                        color.ScB = 1.0F;
                        color.ScG = 1.0F;
                        color.ScR = 1.0F;
                    }
                    else
                    {
                        color.ScB = (data * upperBoundColor.ScB);
                        color.ScG = (data * upperBoundColor.ScG);
                        color.ScR = (data * upperBoundColor.ScR);
                    }
                }
                else if (enableSQRT)
                {
                    double Diff = max - min;
                    color.ScR = color.ScG = color.ScB = (float)(Math.Sqrt(16777216 * ((data - min) / Diff)) / Math.Sqrt(16777216));
                }
                else if (enableSQRE)
                {
                    double Diff = max - min;
                    color.ScR = color.ScG = color.ScB = (float)Math.Pow(((data - min) / Diff), 2);
                }
                else
                {
                    double Diff = max - min;
                    color.ScR = color.ScG = color.ScB = (float)((data - min) / Diff);
                }

                newData[i * pixelOffset + 0] = color.B;
                newData[i * pixelOffset + 1] = color.G;
                newData[i * pixelOffset + 2] = color.R;
            });

            bsource = BitmapSource.Create(sourceObject.Width, sourceObject.Height, 96.0, 96.0, pixelFormat, null, newData, stride);
        }

        //public static void RenderXrayImage_TrimatEffect(ref BitmapSource bsource, TrimatEffect trimatEffect, SourceObject sourceObject, ViewObject viewObject)
        //{
        //    PixelFormat pixelFormat = PixelFormats.Bgr24;

        //    int pixelOffset = pixelFormat.BitsPerPixel / 8;
        //    int stride = sourceObject.Width * pixelOffset;

        //    byte[] newData = new byte[viewObject.Trimat.Data.Length * pixelOffset];
        //    ImageBrush colorMapping = trimatEffect.ColorMapping as ImageBrush;
        //    BitmapImage bs = colorMapping.ImageSource as BitmapImage;
        //    int width = (int)bs.Width;
        //    int height = (int)bs.Height;

        //    PixelFormat pf = bs.Format;
        //    int po = pf.BitsPerPixel / 8;
        //    int rawstride = width * po;

        //    byte[] pixels = new byte[rawstride * height];
        //    bs.CopyPixels(pixels, rawstride, 0);

        //    Parallel.For(0, sourceObject.Data.Length, i =>
        //    {
        //        float data = viewObject.Trimat.Data[i];
        //        float alpha = viewObject.Trimat.AlphaData[i];
        //        Color color = new Color();
        //        float compare = 1.0F - data;

        //        if (compare < 0.15 || compare > 0.907)
        //        {
        //            color.ScR = color.ScG = color.ScB = data;
        //        }
        //        else
        //        {
        //            int y = (int)(alpha * height);
        //            int x = (int)(compare * width);

        //            int location = y * rawstride + x * po;

        //            color.B = pixels[location];
        //            color.G = pixels[location + 1];
        //            color.R = pixels[location + 2];
        //        }

        //        newData[i * pixelOffset + 0] = color.B;
        //        newData[i * pixelOffset + 1] = color.G;
        //        newData[i * pixelOffset + 2] = color.R;
        //    });

        //    bsource = BitmapSource.Create(viewObject.Trimat.Width, viewObject.Trimat.Height, 96.0, 96.0, pixelFormat, null, newData, stride);
        //}

        /// <summary>
        /// Apply an effect to a bitmap. Renders the bitmap again and returns the new bitmap.
        /// </summary>
        /// <param name="source">The starting source.</param>
        /// <param name="effect">The effect to apply.</param>
        /// <returns>The newly rendered bitmap with effect applied.</returns>
        private static void ApplyEffectToBitmap(ref BitmapSource source, Effect effect)
        {
            if (effect != null)
            {
                DrawingVisual dw = new DrawingVisual();

                ImageBrush ib = new ImageBrush();
                ib.ImageSource = source;
                using (DrawingContext dc = dw.RenderOpen())
                {
                    Size sz = new Size((int)source.Width, (int)source.Height);
                    Pen p = new Pen();
                    Point pt = new Point(0, 0);

                    dc.DrawRectangle(ib, p, new System.Windows.Rect(pt, sz));
                    dc.Close();
                }
                dw.Effect = effect;

                // force run the garbage collection before rendering to mitigate the possibility of an OutOfMemoryException
                GC.Collect();
                GC.WaitForPendingFinalizers();

                source = new RenderTargetBitmap((int)source.Width, (int)source.Height, 96, 96, PixelFormats.Default);
                ((RenderTargetBitmap)source).Render(dw);
            }
        }


        /// <summary>
        /// Generates the drawing of the annotations.
        /// </summary>
        /// <param name="sourceWidth">The width of the source image.</param>
        /// <param name="sourceHeight">The height of the source image.</param>
        /// <param name="displayWidth">The width of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="displayHeight">The height of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="adornerLayerManager">The adorner information object.</param>
        /// <param name="background">The background to create the drawing on.</param>
        public static DrawingVisual GetAnnotationDrawing(double sourceWidth, double sourceHeight, double displayWidth, double displayHeight, AdornerLayerManager adornerLayerManager, Brush background)
        {
            DrawingVisual dw = new DrawingVisual();

            using (DrawingContext dc = dw.RenderOpen())
            {
                Size sz = new Size((int)sourceWidth, (int)sourceHeight);
                Pen p = new Pen();
                Point pt = new Point(1, 1);
                dc.DrawRectangle(background, p, new System.Windows.Rect(pt, sz));

                double widthRatio = sourceWidth / displayWidth;
                double heightRatio = sourceHeight / displayHeight;
                double Ratio = (widthRatio < heightRatio) ? widthRatio : heightRatio;

                char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

                int count = 0;

                AnnotationAdorner annotAdorner = (AnnotationAdorner)adornerLayerManager.GetAdorner(AdornerLayerManager.ANNOTATION_ADORNER);
                foreach (Annotation annotation in annotAdorner.GetAnnotations())
                {
                    Rect rect = annotation.Marking.Rect;

                    RectangleGeometry rg = new RectangleGeometry(rect, annotation.Marking.RadiusX, annotation.Marking.RadiusY);
                    //rg.
                    SolidColorBrush scb = Brushes.Green;
                    Pen p1 = new Pen(scb, (5D * Ratio));
                    dc.DrawGeometry(null, p1, rg);

                    string index = string.Empty;

                    int div = (count / 26) - 1;
                    int rem = count % 26;

                    if (div >= 0)
                    {
                        index += letters[div];
                    }

                    index += letters[rem];

                    FormattedText formattedText =
                    new FormattedText(index, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                                        new Typeface("Aharoni"), 32, Brushes.Green);
                    //formattedText.MaxTextWidth = 300;
                    //formattedText.SetFontSize(32 * (96.0 / 72.0));

                    PrintDialog tempPrnDialog = new PrintDialog();

                    if (sourceWidth > sourceHeight)
                    {
                        formattedText.SetFontSize((sourceWidth / tempPrnDialog.PrintableAreaWidth) * 11);
                    }
                    else
                        formattedText.SetFontSize((sourceHeight / tempPrnDialog.PrintableAreaHeight) * 13);

                    Rect LegendRect;

                    if (annotation.Marking.RadiusX > 0)
                        LegendRect = new Rect(annotation.Marking.Rect.TopLeft.X + (annotation.Marking.Rect.Width / 2) - ((formattedText.WidthIncludingTrailingWhitespace + 10) / 2),
                            annotation.Marking.Rect.TopLeft.Y - formattedText.Height - 5, formattedText.WidthIncludingTrailingWhitespace + 12, formattedText.Height + 5);
                    else
                        LegendRect = new Rect(annotation.Marking.Rect.TopLeft.X, annotation.Marking.Rect.TopLeft.Y - formattedText.Height - 5, formattedText.WidthIncludingTrailingWhitespace + 12, formattedText.Height + 5);

                    rg = new RectangleGeometry(LegendRect, 0, 0);
                    dc.DrawGeometry(Brushes.LightYellow, p1, rg);

                    dc.DrawText(formattedText, new Point(LegendRect.TopLeft.X + 5, LegendRect.TopLeft.Y + 5));

                    count++;
                }
            }

            return dw;
        }

        /// <summary>
        /// Renders the annotations on top of the source image passed in.
        /// </summary>
        /// <param name="bsource">The bsource to render to. The passed in source will be modified.</param>
        /// <param name="displayWidth">The width of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="displayHeight">The height of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="adornerLayerManager">The adorner information object.</param>
        public static void RenderAnnotations(ref BitmapSource bsource, double displayWidth, double displayHeight, AdornerLayerManager adornerLayerManager)
        {
            DrawingVisual dw = GetAnnotationDrawing(bsource.Width, bsource.Height, displayWidth, displayHeight, adornerLayerManager, new ImageBrush(bsource));

            bsource = new RenderTargetBitmap((int)bsource.Width, (int)bsource.Height, 96, 96, PixelFormats.Default);
            ((RenderTargetBitmap)bsource).Render(dw);
        }

        /// <summary>
        /// Generates the drawing of the measurements.
        /// </summary>
        /// <param name="sourceWidth">The width of the source image.</param>
        /// <param name="sourceHeight">The height of the source image.</param>
        /// <param name="displayWidth">The width of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="displayHeight">The height of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="adornerLayerManager">The adorner information object.</param>
        /// <param name="background">The background to create the drawing on.</param>
        public static DrawingVisual GetMeasurementDrawing(double sourceWidth, double sourceHeight, double displayWidth, double displayHeight, AdornerLayerManager adornerLayerManager, Brush background)
        {
            DrawingVisual dw = new DrawingVisual();

            using (DrawingContext dc = dw.RenderOpen())
            {
                Size sz = new Size((int)sourceWidth, (int)sourceHeight);
                Pen p = new Pen();
                Point pt = new Point(1, 1);
                dc.DrawRectangle(background, p, new System.Windows.Rect(pt, sz));

                double widthRatio = sourceWidth / displayWidth;
                double heightRatio = sourceHeight / displayHeight;
                double Ratio = (widthRatio < heightRatio) ? widthRatio : heightRatio;

                MeasureAdorner measAdorner = (MeasureAdorner)adornerLayerManager.GetAdorner(AdornerLayerManager.MEASUREMENT_ADORNER);
                foreach (MeasurementLine lineObj in measAdorner.GetMeasurementLines())
                {
                    SolidColorBrush lscb = Brushes.Green;
                    Pen lp1 = new Pen(lscb, 5d * Ratio);

                    dc.DrawLine(lp1, lineObj.StartPoint, lineObj.EndPoint);

                    float lineLength = (lineObj.Length * measAdorner.SamplingSpace) / 1000;

                    FormattedText formattedText =
                        new FormattedText(lineLength.ToString("F") + "m", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                                            new Typeface("Veranda"), 32, Brushes.Green);
                    //formattedText.MaxTextWidth = 300;
                    //formattedText.MaxTextHeight = 240;
                    //formattedText.SetFontSize(32 * (96.0 / 72.0));

                    PrintDialog tempPrnDialog = new PrintDialog();

                    if (sourceWidth > sourceHeight)
                    {
                        formattedText.SetFontSize((sourceWidth / tempPrnDialog.PrintableAreaWidth) * 12);
                    }
                    else
                        formattedText.SetFontSize((sourceHeight / tempPrnDialog.PrintableAreaHeight) * 15);

                    Point textMidPoint = new Point(lineObj.MidPoint.X - (formattedText.Width / 2), lineObj.MidPoint.Y - (formattedText.Height / 2));

                    Rect LegendRect;
                    LegendRect = new Rect(textMidPoint, new Size(formattedText.Width, formattedText.Height));

                    SolidColorBrush scb = Brushes.Green;
                    Pen p1 = new Pen(scb, (5D * Ratio));
                    RectangleGeometry rg = new RectangleGeometry(LegendRect, 0, 0);
                    dc.DrawGeometry(Brushes.White, null, rg);
                    dc.DrawText(formattedText, textMidPoint);
                }
            }

            return dw;
        }

        /// <summary>
        /// Renders the measures on top of the source image passed in.
        /// </summary>
        /// <param name="bsource">The bsource to render to. The passed in source will be modified.</param>
        /// <param name="displayWidth">The width of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="displayHeight">The height of the container to put the bitmap. Scales the size of the drawings.</param>
        /// <param name="adonerImageObject">The adorner information object.</param>
        public static void RenderMeasurements(ref BitmapSource bsource, double displayWidth, double displayHeight, AdornerLayerManager adonerImageObject)
        {
            DrawingVisual dw = GetMeasurementDrawing(bsource.Width, bsource.Height, displayWidth, displayHeight, adonerImageObject, new ImageBrush(bsource));

            bsource = new RenderTargetBitmap((int)bsource.Width, (int)bsource.Height, 96, 96, PixelFormats.Default);
            ((RenderTargetBitmap)bsource).Render(dw);
            
        }
    }
}
