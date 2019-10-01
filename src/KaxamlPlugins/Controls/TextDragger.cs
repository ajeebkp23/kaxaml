using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Kaxaml.Core;
using KaxamlPlugins;

namespace Kaxaml.Plugins.Controls
{
    public class TextDragger : Decorator
    {
        static TextDragger()
        {
            CursorProperty.OverrideMetadata(typeof(TextDragger), new FrameworkPropertyMetadata(Cursors.Hand));
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextDragger), new UIPropertyMetadata(string.Empty));

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(TextDragger), new UIPropertyMetadata(null));

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var text = !string.IsNullOrEmpty(Text) ? Text : (Data != null ? Data.ToString() : null);

            if (!string.IsNullOrEmpty(text))
            {
                if (e.ClickCount == 1)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Clipboard.SetDataObject(text);
                    }));
                }
                else if (e.ClickCount == 2)
                {
                    KaxamlInfo.Editor.InsertStringAtCaret(text);
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // need this to ensure hittesting
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            base.OnRender(drawingContext);
        }

        bool IsDragging = false;

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && (!IsDragging))
            {
                StartDrag();
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                IsDragging = false;
            }

            base.OnPreviewMouseMove(e);
        }

        private void StartDrag()
        {
            DataObject obj = new DataObject(DataFormats.Text, Text);

            if (obj != null)
            {
                if (Data != null) obj.SetData(Data.GetType(), Data);

                try
                {
                    DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
                }
                catch (Exception ex)
                {
                    if (ex.IsCriticalException())
                    {
                        throw;
                    }
                }
            }
        }

    }
}