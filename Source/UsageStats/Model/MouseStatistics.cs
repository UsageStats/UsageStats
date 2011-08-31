using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UsageStats
{
    public class MouseStatistics : Observable
    {
        public static double ScreenResolution = 96;

        private readonly Brush leftBrush = new SolidColorBrush(Color.FromArgb(60, 0, 200, 0));
        private readonly Brush middleBrush = new SolidColorBrush(Color.FromArgb(60, 0, 0, 255));
        private readonly Brush rightBrush = new SolidColorBrush(Color.FromArgb(60, 255, 0, 0));
        private readonly Stopwatch moveTimer = new Stopwatch();
        private DateTime lastMouseDownTime;
        private bool isLeftButtonDown;
        private Point lastPoint = WindowHelper.GetCursorPos();
        private bool wasLeftButtonDown;

        public MouseStatistics(ActiveTime total, TimePerHour activityPerHour, double screenMapScale)
        {
            ClicksPerHour = new CountPerHour(activityPerHour);
            DistancePerHour = new CountPerHour(activityPerHour);
            MouseActivity = new ActiveTime(total);
            DoubleClickTime = new Histogram(0.01);
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
        public CountPerHour ClicksPerHour { get; set; }
        public CountPerHour DistancePerHour { get; set; }
        public ActiveTime MouseActivity { get; set; }
        public Histogram DoubleClickTime { get; set; }
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

        public Dictionary<string, int> ClicksPerHourList
        {
            get { return ClicksPerHour.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value); }
        }

        public Dictionary<string, int> MovementSpeedList
        {
            get { return MovementSpeed.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
        }

        public Dictionary<string, int> MovementDirectionList
        {
            get { return MovementDirection.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
        }

        public Dictionary<string, int> DoubleClickTimeList
        {
            get { return DoubleClickTime.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
        }

        public string Report
        {
            get { return ToString(); }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(" Left clicks:   {0}", LeftMouseClicks));
            sb.AppendLine(String.Format(" Right clicks:  {0}", RightMouseClicks));
            sb.AppendLine(String.Format(" Middle clicks: {0}", MiddleMouseClicks));
            sb.AppendLine(String.Format(" Double-clicks: {0} ({1:0} ms)", MouseDoubleClicks, DoubleClickTime.Average * 1000));
            sb.AppendLine(String.Format(" Distance:      {0} ({1:0} pixels)", MouseDistanceText, TotalMouseDistance));
            sb.AppendLine(String.Format(" Average speed: {0:0} pixels/sec", MovementSpeed.Average));
            sb.AppendLine(String.Format(" Mousewheel:    {0}", MouseWheelDistance));
            sb.AppendLine(String.Format(" Activity:      {0}", MouseActivity));
            sb.AppendLine();
            sb.AppendLine(" Clicks per hour:");
            sb.AppendLine(ClicksPerHour.Report(false));
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
            var brush = leftBrush;
            if (mb == MouseButton.Middle)
                brush = middleBrush;
            if (mb == MouseButton.Right)
                brush = rightBrush;

            ClickMap.AddPoint(lastPoint, brush);
            double timeSinceLastClick = (DateTime.Now - lastMouseDownTime).TotalSeconds;
            if (timeSinceLastClick < 0.5)
            {
                MouseDoubleClicks++;
                DoubleClickTime.Add(timeSinceLastClick);
                DoubleClickMap.AddPoint(lastPoint, brush);
                RaisePropertyChanged("DoubleClickTimeList");
            }

            lastMouseDownTime = DateTime.Now;

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
            ClicksPerHour.Increase();
            RegisterActivity();
            RaisePropertyChanged("ClicksPerHourList");
            RaisePropertyChanged("Report");

            // Only update movement speed graph when clicking...
            RaisePropertyChanged("MovementSpeedList");
            RaisePropertyChanged("MovementDirectionList");
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
            RaisePropertyChanged("Report");
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
            RaisePropertyChanged("Report");
        }
    }
}