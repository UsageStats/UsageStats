using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UsageStats
{
    public class MouseStatistics : Observable
    {
        public static double ScreenResolution = 96;

        private readonly Brush LeftBrush = new SolidColorBrush(Color.FromArgb(60, 0, 200, 0));
        private readonly Brush MiddleBrush = new SolidColorBrush(Color.FromArgb(60, 0, 0, 255));
        private readonly Brush RightBrush = new SolidColorBrush(Color.FromArgb(60, 255, 0, 0));
        private readonly Stopwatch moveTimer = new Stopwatch();
        private DateTime _lastMouseDownTime;
        private bool isLeftButtonDown;
        private Point lastPoint = WindowHelper.GetCursorPos();
        private bool wasLeftButtonDown;

        public MouseStatistics(ActiveTime total, TimePerHour activityPerHour, double screenMapScale)
        {
            ClicksPerCountPerHour = new CountPerHour(activityPerHour);
            DistancePerCountPerHour = new CountPerHour(activityPerHour);
            MouseActivity = new ActiveTime(total);
            DoubleClickSpeed = new Histogram(0.01);
            MovementSpeed = new Histogram(50);
            MovementDirection = new Histogram(45);

            var w = (int)(SystemParameters.PrimaryScreenWidth / screenMapScale);
            ClickMap = new ScreenBitmap(w);
            DoubleClickMap = new ScreenBitmap(w);
            TraceMap = new ScreenBitmap(w);
            DragTraceMap = new ScreenBitmap(w);
        }

        public int LeftMouseClicks { get; set; }
        public int RightMouseClicks { get; set; }
        public int MiddleMouseClicks { get; set; }
        public int MouseDoubleClicks { get; set; }
        public double TotalMouseDistance { get; set; }
        public double MouseWheelDistance { get; set; }
        public CountPerHour ClicksPerCountPerHour { get; set; }
        public CountPerHour DistancePerCountPerHour { get; set; }
        public ActiveTime MouseActivity { get; set; }
        public Histogram DoubleClickSpeed { get; set; }
        public Histogram MovementSpeed { get; set; }
        public Histogram MovementDirection { get; set; }

        public ScreenBitmap ClickMap { get; set; }
        public ScreenBitmap DoubleClickMap { get; set; }
        public ScreenBitmap TraceMap { get; set; }
        public ScreenBitmap DragTraceMap { get; set; }

        public double MouseClicksPerMinute
        {
            get
            {
                double min = MouseActivity.TotalSeconds / 60;
                int clicks = LeftMouseClicks + MiddleMouseClicks + RightMouseClicks;
                return min > 0 ? clicks / min : 0;
            }
        }

        public double MouseDistance
        {
            get { return TotalMouseDistance / ScreenResolution * 0.0254; }
        }

        public string MouseDistanceText
        {
            get { return String.Format("{0:0.00} m", MouseDistance); }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(" Left clicks:   {0}", LeftMouseClicks));
            sb.AppendLine(String.Format(" Right clicks:  {0}", RightMouseClicks));
            sb.AppendLine(String.Format(" Middle clicks: {0}", MiddleMouseClicks));
            sb.AppendLine(String.Format(" Double-clicks: {0}", MouseDoubleClicks));
            sb.AppendLine(String.Format(" Distance:      {0}", MouseDistanceText));
            sb.AppendLine(String.Format(" Mousewheel:    {0}", MouseWheelDistance));
            sb.AppendLine(String.Format(" Activity:      {0}", MouseActivity));
            sb.AppendLine();
            sb.AppendLine(" Clicks per hour:");
            sb.AppendLine(ClicksPerCountPerHour.Report(false));
            sb.AppendLine();
            //sb.AppendLine(" Double click speed:");
            //sb.AppendLine(DoubleClickSpeed.Report());
            //sb.AppendLine();
            //sb.AppendLine(" Moving speed:");
            //sb.AppendLine(MovementSpeed.Report());
            //sb.AppendLine();
            //sb.AppendLine(" Movement direction:");
            //sb.AppendLine(MovementDirection.Report());
            return sb.ToString();
        }

        public void MouseDown(MouseButton mb)
        {
            Brush brush = LeftBrush;
            if (mb == MouseButton.Middle)
                brush = MiddleBrush;
            if (mb == MouseButton.Right)
                brush = RightBrush;

            ClickMap.AddPoint(lastPoint, brush);
            double timeSinceLastClick = (DateTime.Now - _lastMouseDownTime).TotalSeconds;
            if (timeSinceLastClick < 0.5)
            {
                MouseDoubleClicks++;
                DoubleClickSpeed.Add(timeSinceLastClick);
                DoubleClickMap.AddPoint(lastPoint, brush);
            }

            _lastMouseDownTime = DateTime.Now;

            switch (mb)
            {
                case MouseButton.Left:
                    LeftMouseClicks++;
                    isLeftButtonDown = true;
                    break;
                case MouseButton.Middle:
                    MiddleMouseClicks++;
                    break;
                case MouseButton.Right:
                    RightMouseClicks++;
                    break;
            }
            ClicksPerCountPerHour.Increase();
            RegisterActivity();
        }

        public void MouseUp(MouseButton mb)
        {
            isLeftButtonDown = false;
        }

        private void RegisterActivity()
        {
            MouseActivity.Update(Statistics.InactivityThreshold);
        }

        public void MouseWheel()
        {
            MouseWheelDistance += 1;
            RegisterActivity();
        }

        public void MouseDblClk()
        {
            MouseDoubleClicks++;
            RegisterActivity();
        }

        public void MouseMove(Point pt)
        {
            double dx = pt.X - lastPoint.X;
            double dy = pt.Y - lastPoint.Y;
            double dl = Math.Sqrt(dx * dx + dy * dy);

            double deltaTime = moveTimer.ElapsedMilliseconds * 0.001;
            moveTimer.Restart();

            // speed: pixels/second
            double speed = deltaTime != 0 ? dl / deltaTime : 0;

            double direction = Math.Atan2(dy, dx) / Math.PI * 180;

            if (speed > 0)
            {
                MovementSpeed.Add(speed);
                RaisePropertyChanged("MovementSpeed");
            }

            MovementDirection.Add(direction);

            TraceMap.AddStroke(lastPoint, pt);

            if (wasLeftButtonDown && isLeftButtonDown)
                DragTraceMap.AddStroke(lastPoint, pt);

            RegisterActivity();

            TotalMouseDistance += dl;

            lastPoint = pt;
            wasLeftButtonDown = isLeftButtonDown;
        }
    }
}