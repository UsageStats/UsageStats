namespace TimeRecorderStatistics
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class ToolTipBehavior
    {
        public static Statistics GetToolTipSource(DependencyObject obj)
        {
            return (Statistics)obj.GetValue(ToolTipSourceProperty);
        }

        public static void SetToolTipSource(DependencyObject obj, Statistics value)
        {
            obj.SetValue(ToolTipSourceProperty, value);
        }

        public static readonly DependencyProperty ToolTipSourceProperty =
            DependencyProperty.RegisterAttached("ToolTipSource", typeof(Statistics), typeof(ToolTipBehavior), new PropertyMetadata(null, ToolTipSourceChanged));

        private static void ToolTipSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var image = (Image)d;
            var source = (Statistics)args.NewValue;
            var toolTip = new ToolTip { PlacementTarget = image, Placement = PlacementMode.RelativePoint };

            image.MouseMove += (s, e) =>
                {
                    int minute = (int)e.GetPosition(image).X;
                    var text = source.GetToolTip(minute);
                    image.ToolTip = text == null ? null : toolTip;
                    if (text != null)
                    {
                        toolTip.Content = text;
                        toolTip.HorizontalOffset = e.GetPosition(image).X;
                        toolTip.VerticalOffset = e.GetPosition(image).Y - toolTip.ActualHeight;
                    }
                    toolTip.IsOpen = text != null;
                };
            image.MouseLeave += (s, e) => toolTip.IsOpen = false;
        }
    }
}