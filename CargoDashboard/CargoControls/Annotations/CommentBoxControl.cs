using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace L3.Cargo.Controls
{

    public class CommentBoxControl : Control
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register("Text", typeof(string), typeof(CommentBoxControl),
                                            new FrameworkPropertyMetadata(Text_PropertyChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
                DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(CommentBoxControl),
                                            new FrameworkPropertyMetadata(false, ReadOnly_PropertyChanged));

        #endregion Dependency Property Definitions


        #region Private Members

        #endregion Private Members


        #region Public Members

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);

                if (TextChangedEvent != null)
                {
                    TextChangedEvent(this, new EventArgs()); 
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public event EventHandler TextChangedEvent;

        #endregion Public Members


        #region Constructors

        static CommentBoxControl ()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommentBoxControl), new FrameworkPropertyMetadata(typeof(CommentBoxControl)));
        }

        #endregion Constructors


        #region Private Methods

        private static void Text_PropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommentBoxControl commentBox = d as CommentBoxControl;
            if (commentBox != null)
            {
                commentBox.Text = (string)e.NewValue;
            }
        }

        private static void ReadOnly_PropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommentBoxControl commentBox = d as CommentBoxControl;
            if (commentBox != null)
            {
                commentBox.IsReadOnly = (bool)e.NewValue;
            }
        }

        #endregion Private Methods
    }
}
