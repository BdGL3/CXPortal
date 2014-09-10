using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace L3.Cargo.Controls
{
    public class AdornerContentPresenter : Adorner
    {
        #region Protected Members

        protected VisualCollection _Visuals;

        protected ContentPresenter _ContentPresenter;

        protected override int VisualChildrenCount
        {
            get
            {
                return _Visuals.Count;
            }
        }

        #endregion Protected Members


        #region Public Members

        public object Content
        {
            get
            {
                return _ContentPresenter.Content;
            }
            set
            {
                _ContentPresenter.Content = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public AdornerContentPresenter (UIElement adornedElement)
            : base(adornedElement)
        {
            _Visuals = new VisualCollection(this);
            _ContentPresenter = new ContentPresenter();
            _Visuals.Add(_ContentPresenter);
        }

        public AdornerContentPresenter (UIElement adornedElement, Visual content)
            : this(adornedElement)
        {
            Content = content;
        }

        #endregion Constructors


        #region Protected Methods

        protected override Visual GetVisualChild (int index)
        {
            return _Visuals[index];
        }

        protected override Size MeasureOverride (Size constraint)
        {
            _ContentPresenter.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride (Size finalSize)
        {
            _ContentPresenter.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return base.ArrangeOverride(finalSize);
        }

        #endregion Protected Methods
    }
}
