using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Client;

namespace L3.Cargo.Detectors.Display.Common
{
    /// <summary>
    /// Interaction logic for RealTimeViewerDisplay.xaml
    /// </summary>
    public partial class RealTimeViewerDisplay : UserControl, IDisposable
    {
        private WriteableBitmap _writeableBitmapOne;
        private WriteableBitmap _writeableBitmapTwo;
        private RealTimeDataAccess _activeScanEndPoint;
        private int _imageWidth;
        private bool _Flip = false;
        private bool _Firsttime = true;
        private SlideInTransitionEffect _effect;

        public RealTimeViewerDisplay(string multicastAddr, int dataPort)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);;

            _activeScanEndPoint = new RealTimeDataAccess(multicastAddr, dataPort);
            _effect = new SlideInTransitionEffect();
            _updateThread = Threads.Create(UpdateAgent, ref _updateEnd, "Real Time View Update thread");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(UserControl_Loaded);
            _updateEnd.Reset();
            _updateThread.Start();
        }

        private void UpdateAgent()
        {
            Point pt = new Point(0, 0);
            while (!_updateEnd.WaitOne(0))
            {
                try
                {
                    byte[] receivedData = _activeScanEndPoint.ReceiveDataLines();

                    if (receivedData != null && receivedData.Length > 0)
                    {
                        if (_writeableBitmapOne == null || _writeableBitmapTwo == null)
                        {
                            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
                            {
                                _imageWidth = (int)(Math.Round(ScrollableImageBorder.ActualWidth * (receivedData.Length / ScrollableImageBorder.ActualHeight)));

                                if (_imageWidth != 0)
                                {
                                    _writeableBitmapOne = new WriteableBitmap(_imageWidth, receivedData.Length, 96, 96, PixelFormats.Gray8, null);
                                    _writeableBitmapTwo = new WriteableBitmap(_imageWidth, receivedData.Length, 96, 96, PixelFormats.Gray8, null);

                                    ScrollableImage.Source = _writeableBitmapOne;

                                    _effect.Input = new ImageBrush(_writeableBitmapOne);
                                    _effect.Texture2 = new ImageBrush(_writeableBitmapTwo);
                                    _effect.Progress = 0;
                                }
                            }));
                        }

                        if (_writeableBitmapOne != null && _writeableBitmapTwo != null)
                        {
                            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
                            {
                                Int32Rect rect = new Int32Rect(0, 0, 1, receivedData.Length);
                                if (!_Flip)
                                    _writeableBitmapTwo.WritePixels(rect, receivedData, 1, (int)pt.X, (int)pt.Y);
                                else
                                    _writeableBitmapOne.WritePixels(rect, receivedData, 1, (int)pt.X, (int)pt.Y);

                                pt.X++;

                                if (!_Flip && !_Firsttime)
                                {
                                    _effect.Progress = (pt.X / _imageWidth) * 100;
                                    _effect.Input = new ImageBrush(_writeableBitmapOne);
                                    _effect.Texture2 = new ImageBrush(_writeableBitmapTwo);
                                    ScrollableImage.Effect = _effect;
                                }

                                if (_Flip && !_Firsttime)
                                {
                                    _effect.Progress = (pt.X / _imageWidth) * 100;
                                    _effect.Input = new ImageBrush(_writeableBitmapTwo);
                                    _effect.Texture2 = new ImageBrush(_writeableBitmapOne);
                                    ScrollableImage.Effect = _effect;
                                }

                                if (_Firsttime)
                                {
                                    _effect.Input = new ImageBrush(_writeableBitmapOne);
                                    _effect.Texture2 = new ImageBrush(_writeableBitmapTwo);
                                    _effect.Progress = 0;
                                    ScrollableImage.Effect = _effect;
                                }

                                if (pt.X > _imageWidth - 1)
                                {
                                    _Flip ^= true;
                                    pt.X = 0;

                                    if (_Firsttime == true)
                                    {
                                        _writeableBitmapOne = _writeableBitmapTwo;
                                        _Firsttime = false;
                                    }
                                }

                            }));
                        }
                    }
                }
                catch { }
            }
        }
        private CancellationTokenSource _updateCancel = new CancellationTokenSource();
        private ManualResetEvent _updateEnd = new ManualResetEvent(false);
        private Thread _updateThread;

        public void Dispose()
        {
            _activeScanEndPoint.Dispose();
            _updateCancel.Cancel();
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _updateThread != null)
                    _updateThread = Threads.Dispose(_updateThread, ref _updateEnd);
            }
            catch { }
            finally { _updateThread = null; }
        }
    }
}
