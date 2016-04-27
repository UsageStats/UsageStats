using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UsageStats.Properties;

namespace UsageStats
{
    public class ScreenBitmap
    {
        private readonly RenderTargetBitmap rtb;

        public ScreenBitmap(int width)
        {
            double w = SystemParameters.VirtualScreenWidth;
            double h = SystemParameters.VirtualScreenHeight;
            Scale = width / w;
            var height = (int)(h * Scale);

            int dpi = 96;
            rtb = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
            }
            rtb.Render(dv);

            Fill = new SolidColorBrush(Color.FromArgb(60, 0, 200, 0));
            Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0));
        }

        public RenderTargetBitmap Source
        {
            get { return rtb; }
        }

        public double PointRadius
        {
            get { return Settings.Default.MouseDownSize / 2; }
        }

        public double StrokeThickness
        {
            get { return Settings.Default.MouseTrackWidth; }
        }

        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }
        public double Scale { get; set; }

        private Point Transform(Point pt)
        {
            return new Point(pt.X * Scale, pt.Y * Scale);
        }

        public void AddPoint(Point pt, Brush fill)
        {
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawEllipse(fill, null, Transform(pt), PointRadius, PointRadius);
            }
            rtb.Render(dv);
        }

        public void AddStroke(Point pt1, Point pt2)
        {
            var dv = new DrawingVisual();
            var pen = new Pen(Stroke, StrokeThickness);
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawLine(pen, Transform(pt1), Transform(pt2));
            }
            rtb.Render(dv);
        }

        public void DrawDate(string text)
        {
            var dv = new DrawingVisual();
            RenderOptions.SetClearTypeHint(dv, ClearTypeHint.Enabled);
            using (var ctx = dv.RenderOpen())
            {
                var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 18, Brushes.Black);
                ctx.DrawText(ft, new Point(5, 5));
            }
            rtb.Render(dv);

        }
    }
}