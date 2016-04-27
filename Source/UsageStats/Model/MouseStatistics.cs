using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Runtime.Serialization;
namespace UsageStats
{
    public class MouseStatistics : Observable
    {
      
        public MouseStats Stats { get; set; }
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
            Stats = new MouseStats();
            Stats.ClicksPerHour = new CountPerHour(activityPerHour);
            Stats.DistancePerHour = new CountPerHour(activityPerHour);
            Stats.MouseActivity = new ActiveTime(total);
            Stats.DoubleClickTime = new Histogram(0.01);
            Stats.MovementSpeed = new Histogram(50);
            Stats.MovementDirection = new Histogram(45);

            var w = (int)(SystemParameters.VirtualScreenWidth / screenMapScale);
            ClickMap = new ScreenBitmap(w);
            DoubleClickMap = new ScreenBitmap(w);
            TraceMap = new ScreenBitmap(w);
            DragTraceMap = new ScreenBitmap(w);

            // Get the top left corner of the screen
            Point origin = new Point();
            foreach( System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
            {
                origin.X = Math.Min(origin.X, s.Bounds.X);
                origin.Y = Math.Min(origin.Y, s.Bounds.Y);
            }

            Stats.Origin = origin;
        }


        [DataContract]
        public class MouseStats
        {
            [DataMember]
            public double ScreenResolution = 96;

            [DataMember]
            public Point Origin { get; set; }
            [DataMember]
            public int LeftMouseClicks { get; set; }
            [DataMember]
            public int RightMouseClicks { get; set; }
            [DataMember]
            public int MiddleMouseClicks { get; set; }
            [DataMember]
            public int MouseDoubleClicks { get; set; }
            [DataMember]
            public double TotalMouseDistance { get; set; }
            [DataMember]
            public double MouseWheelDistance { get; set; }
            [DataMember]
            public CountPerHour ClicksPerHour { get; set; }
            [DataMember]
            public CountPerHour DistancePerHour { get; set; }
            [DataMember]
            public ActiveTime MouseActivity { get; set; }
            [DataMember]
            public Histogram DoubleClickTime { get; set; }
            [DataMember]
            public Histogram MovementSpeed { get; set; }
            [DataMember]
            public Histogram MovementDirection { get; set; }

        }
        public ScreenBitmap ClickMap { get; set; }
        public ScreenBitmap DoubleClickMap { get; set; }
        public ScreenBitmap TraceMap { get; set; }
        public ScreenBitmap DragTraceMap { get; set; }

        public double MouseClicksPerMinute
        {
            get
            {
                double min = Stats.MouseActivity.TotalSeconds / 60;
                int clicks = Stats.LeftMouseClicks + Stats.MiddleMouseClicks + Stats.RightMouseClicks;
                return min > 0 ? clicks / min : 0;
            }
        }

        public double MouseDistance
        {
            get { return Stats.TotalMouseDistance / Stats.ScreenResolution * 0.0254; }
        }

        public string MouseDistanceText
        {
            get { return String.Format("{0:0.00} m", MouseDistance); }
        }

        public Dictionary<string, int> ClicksPerHourList
        {
            get { return Stats.ClicksPerHour.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value); }
        }

        public Dictionary<string, int> MovementSpeedList
        {
            get { return Stats.MovementSpeed.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
        }

        public Dictionary<string, int> MovementDirectionList
        {
            get { return Stats.MovementDirection.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
        }

        public Dictionary<string, int> DoubleClickTimeList
        {
            get { return Stats.DoubleClickTime.Data.ToDictionary(v => v.Key.ToString(), v => v.Value); }
        }

        public string Report
        {
            get { return ToString(); }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(" Left clicks:   {0}", Stats.LeftMouseClicks));
            sb.AppendLine(String.Format(" Right clicks:  {0}", Stats.RightMouseClicks));
            sb.AppendLine(String.Format(" Middle clicks: {0}", Stats.MiddleMouseClicks));
            sb.AppendLine(String.Format(" Double-clicks: {0} ({1:0} ms)", Stats.MouseDoubleClicks, Stats.DoubleClickTime.Average * 1000));
            sb.AppendLine(String.Format(" Distance:      {0} ({1:0} pixels)", MouseDistanceText, Stats.TotalMouseDistance));
            sb.AppendLine(String.Format(" Average speed: {0:0} pixels/sec", Stats.MovementSpeed.Average));
            sb.AppendLine(String.Format(" Mousewheel:    {0}", Stats.MouseWheelDistance));
            sb.AppendLine(String.Format(" Activity:      {0}", Stats.MouseActivity));
            sb.AppendLine();
            sb.AppendLine(" Clicks per hour:");
            sb.AppendLine(Stats.ClicksPerHour.Report(false));
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
                Stats.MouseDoubleClicks++;
                Stats.DoubleClickTime.Add(timeSinceLastClick);
                DoubleClickMap.AddPoint(lastPoint, brush);
                RaisePropertyChanged("DoubleClickTimeList");
            }

            lastMouseDownTime = DateTime.Now;

            switch (mb)
            {
                case MouseButton.Left:
                    Stats.LeftMouseClicks++;
                    isLeftButtonDown = true;
                    break;
                case MouseButton.Middle:
                    Stats.MiddleMouseClicks++;
                    break;
                case MouseButton.Right:
                    Stats.RightMouseClicks++;
                    break;
            }
            Stats.ClicksPerHour.Increase();
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
            Stats.MouseActivity.Update(Statistics.InactivityThreshold);
        }

        public void MouseWheel()
        {
            Stats.MouseWheelDistance += 1;
            RegisterActivity();
            RaisePropertyChanged("Report");
        }

        public void MouseDblClk()
        {
            Stats.MouseDoubleClicks++;
            RegisterActivity();
        }

        public void MouseMove(Point pt)
        {
           // pt might be negative when there is multiple desktops, and the primary screen is not the left-most one
            pt.X -= Stats.Origin.X;
            pt.Y -= Stats.Origin.Y;

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
                Stats.MovementSpeed.Add(speed);
                RaisePropertyChanged("MovementSpeed");
            }

            Stats.MovementDirection.Add(direction);

            TraceMap.AddStroke(lastPoint, pt);

            if (wasLeftButtonDown && isLeftButtonDown)
                DragTraceMap.AddStroke(lastPoint, pt);

            RegisterActivity();

            Stats.TotalMouseDistance += dl;

            lastPoint = pt;
            wasLeftButtonDown = isLeftButtonDown;
            RaisePropertyChanged("Report");
        }
    }
}