using System;
using System.Windows;
using System.Windows.Media;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    public class Annotation
    {
        #region Private Members

        private string _CommentText;

        private bool _IsReadOnly;

        private bool _IsSelected;

        private CommentBoxControl _CommentBox;

        private RectangleGeometry _Marking;

        private Pen _Pen;

        #endregion Private Members


        #region Public Members

        public Pen Pen
        {
            get
            {
                return _Pen;
            }
            set
            {
                _Pen = value;
            }
        }

        public RectangleGeometry Marking
        {
            get
            {
                return _Marking;
            }
            set
            {
                _Marking = value;
            }
        }

        public Point StartPoint
        {
            get
            {
                return _Marking.Rect.TopLeft;
            }
            set
            {
                _Marking.Rect = new Rect(value, _Marking.Rect.BottomRight);
            }
        }

        public Point EndPoint
        {
            get
            {
                return _Marking.Rect.BottomRight;
            }
            set
            {
                _Marking.Rect = new Rect(StartPoint, value);

                if (_Marking.RadiusX != 0 || _Marking.RadiusY != 0)
                {
                    _Marking.RadiusX = Double.MaxValue;
                    _Marking.RadiusY = Double.MaxValue;
                }
            }
        }

        public CommentBoxControl CommentBox
        {
            get
            {
                return _CommentBox;
            }
            set
            {
                _CommentBox = value;
            }
        }

        public string CommentText
        {
            get
            {
                return _CommentText;
            }
            set
            {
                _CommentText = value;
            }
        }

        public Point CommentBoxBottomLeft
        {
            get
            {
                return Annotation.CalculateCommentBoxStartPoint(_Marking);
            }
        }

        public Point MarkingTopRight
        {
            get
            {
                return Annotation.CalculateCommentStartPoint(_Marking);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
            set
            {
                _IsReadOnly = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public Annotation (Pen pen, RectangleGeometry marking, CommentBoxControl commentBox, bool isReadOnly)
        {
            _Pen = pen;
            _Marking = marking;
            _CommentBox = commentBox;
            _IsReadOnly = isReadOnly;

            if (_CommentBox != null)
            {
                _CommentBox.TextChangedEvent += new EventHandler(CommentBox_TextChanged);
            }
        }

        public Annotation (Pen pen, RectangleGeometry marking, CommentBoxControl commentBox, string commentText, bool isReadOnly) :
            this(pen, marking, commentBox, isReadOnly)
        {
            _CommentText = commentText;
        }

        public Annotation (Pen pen, RectangleGeometry marking, CommentBoxControl commentBox, string commentText, bool isReadOnly, bool isSelected) :
            this(pen, marking, commentBox, commentText, isReadOnly)
        {
            _IsSelected = isSelected;
        }

        #endregion Constructors


        #region Private Methods

        private static Point CalculateCommentBoxStartPoint (RectangleGeometry shape)
        {
            double start = shape.RadiusY;
            if (start != 0)
            {
                start = shape.Rect.Height / 2;
            }
            return new Point(shape.Rect.TopRight.X + 30, shape.Rect.TopRight.Y - 15 + start);
        }

        private static Point CalculateCommentStartPoint (RectangleGeometry shape)
        {
            double start = shape.RadiusY;
            if (start != 0)
            {
                start = shape.Rect.Height / 2;
            }
            return new Point(shape.Rect.TopRight.X, shape.Rect.TopRight.Y + start);
        }

        private void CommentBox_TextChanged (object sender, EventArgs args)
        {
            CommentBoxControl commentBoxControl = sender as CommentBoxControl;
            if (commentBoxControl != null)
            {
                CommentText = commentBoxControl.Text;
            }
        }

        #endregion Private Methods


        #region Public Methods

        public bool Contains (Point pt)
        {
            return true;
        }

        #endregion Public Methods
    }
}
