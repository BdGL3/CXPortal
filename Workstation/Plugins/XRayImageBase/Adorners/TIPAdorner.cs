using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using L3.Cargo.Common;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    public class TIPAdorner : AdornerBase, IDisposable
    {
        #region Private Members

        private List<RectangleGeometry> m_TIPMarkingsList;

        #endregion Private Members


        #region Constructors

        public TIPAdorner(UIElement element, PanZoomPanel panZoomPanel)
            : base(element, panZoomPanel)
        {
            m_TIPMarkingsList = new List<RectangleGeometry>();
        }

        #endregion Constructors


        #region Protected Methods

        protected override void OnInitialized (EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnRender (DrawingContext drawingContext)
        {
            if (this.Enabled)
            {
                Pen pen = new Pen(Brushes.Blue, 3);
                pen.LineJoin = PenLineJoin.Round;

                foreach (RectangleGeometry rectGeo in m_TIPMarkingsList)
                {
                    drawingContext.DrawGeometry(null, pen, rectGeo);
                }

            }

            base.OnRender(drawingContext);
        }

        #endregion Protected Methods


        #region Public Methods

        public override GeneralTransform GetDesiredTransform (GeneralTransform transform)
        {
            return base.GetDesiredTransform(new MatrixTransform(1, 0, 0, 1, 0, 0));
        }

        public void Dispose ()
        {
            if (m_TIPMarkingsList != null)
            {
                m_TIPMarkingsList.Clear();
                m_TIPMarkingsList = null;
            }
        }

        public void Add (Rect rect)
        {
            m_TIPMarkingsList.Add(new RectangleGeometry(rect));
        }

        #endregion Public Methods
    }
}
