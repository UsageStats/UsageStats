using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MouseTracker
{
    public class LoggerViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged Block
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region CurrentApplication (INotifyPropertyChanged Property)
        private string _currentApplication;

        public string CurrentApplication
        {
            get { return _currentApplication; }
            set
            {
                if (_currentApplication != value)
                {
                    _currentApplication = value;
                    RaisePropertyChanged("CurrentApplication");
                }
            }
        }
        #endregion

        #region CurrentPosition (INotifyPropertyChanged Property)
        private Point _currentPosition;

        public Point CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;
                    RaisePropertyChanged("CurrentPosition");
                }
            }
        }
        #endregion

        #region TotalDistance (INotifyPropertyChanged Property)
        private double _totalDistance;

        public double TotalDistance
        {
            get { return _totalDistance; }
            set
            {
                if (_totalDistance != value)
                {
                    _totalDistance = value;
                    RaisePropertyChanged("TotalDistance");
                }
            }
        }
        #endregion

        #region KeyStrokes (INotifyPropertyChanged Property)
        private int _keyStrokes;

        public int KeyStrokes
        {
            get { return _keyStrokes; }
            set
            {
                if (_keyStrokes != value)
                {
                    _keyStrokes = value;
                    RaisePropertyChanged("KeyStrokes");
                }
            }
        }
        #endregion

        public int MouseClicks { get; set; }

        public DateTime TimeStarted { get; set; }
        public DateTime LastMouseActivity { get; set; }
        public DateTime LastActivity { get; set; }

        public TimeSpan TimeMouseActive { get; set; }
        public TimeSpan TimeActive { get; set; }
        public double PercentMouseActive { get; set; }

        private Stopwatch watch;

        private int[] _directionCount;
        private int[] _speedCount;
        private readonly int[,] _screenCount;
        private readonly DispatcherTimer _timer;
        private readonly int _width;
        private readonly int _height;
        private const int CellHeight = 8;
        private const int CellWidth = 8;
        private Point _lastPoint;

        const double dpi = 96.0;
        private InterceptMouse _mouse;
        private InterceptKeys _keys;


        public LoggerViewModel()
        {

            // initialize the screen counter matrix
            double w = SystemParameters.PrimaryScreenWidth;
            double h = SystemParameters.PrimaryScreenHeight;
            _width = (int)w / CellWidth;
            _height = (int)h / CellHeight;
            _screenCount = new int[_width, _height];
            _directionCount = new int[360];
            _speedCount = new int[100];

            InitBitmap();

            _lastPoint = WindowHelper.GetCursorPos();

            watch = new Stopwatch();
            watch.Start();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += timer_Tick;
            _timer.Start();

            Hook.CreateKeyboardHook(KeyReader);
            _keys = new InterceptKeys();
            _mouse = new InterceptMouse(MouseHandler);
        }

        public void Dispose()
        {
            _mouse.Dispose();
            _keys.Dispose();
        }

        private void MouseHandler(IntPtr wparam, IntPtr lparam)
        {
            switch ((MouseMessages)wparam)
            {
                case MouseMessages.WM_LBUTTONDOWN:
                // case MouseMessages.WM_MBUTTONDOWN:
                case MouseMessages.WM_RBUTTONDOWN:
                    MouseClicks++;
                    RaisePropertyChanged("MouseClicks");
                    break;
            }
        }

        public void KeyReader(IntPtr wParam, IntPtr lParam)
        {
            int key = Marshal.ReadInt32(lParam);
            Hook.VK vk = (Hook.VK)key;

            KeyStrokes++;
            Active();
            RaisePropertyChanged("KeyStrokes");
            RaisePropertyChanged("TimeActive");
        }


        private void Add(Point p)
        {
            int x = (int)p.X / CellWidth;
            int y = (int)p.Y / CellHeight;
            x = x % _width;
            y = y % _height;
            if (x < 0) x += _width;
            if (y < 0) y += _height;
            _screenCount[x, y]++;
        }

        private BitmapSource UpdateBitmap()
        {
            var pd = new byte[_width * _height];
            int max = 0;
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    if (_screenCount[x, y] > max) max = _screenCount[x, y];
                }

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    pd[x + y * _width] = (byte)(255.0 * _screenCount[x, y] / max);
                }

            return BitmapSource.Create(_width, _height, dpi, dpi, PixelFormats.Gray8, null, pd, _width);
        }

        RenderTargetBitmap rtb;
        void InitBitmap()
        {
            rtb = new RenderTargetBitmap(_width, _height, dpi, dpi, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, _width, _height));
            }
            rtb.Render(dv);
        }

        Brush b = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
        BitmapSource UpdateBitmap(Point pt1)
        {
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                var pt = new Point(pt1.X / CellWidth, pt1.Y / CellHeight);
                ctx.DrawEllipse(b, null, pt, 2.0, 2.0);
            }
            rtb.Render(dv);
            return rtb;
        }



        BitmapSource UpdateBitmap2()
        {
            var rtb = new RenderTargetBitmap(_width, _height, dpi, dpi, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, _width, _height));
                var b = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
                for (int x = 0; x < _width; x++)
                    for (int y = 0; y < _height; y++)
                    {
                        var pt = new Point(x, y);
                        if (_screenCount[x, y] > 0)
                            ctx.DrawEllipse(b, null, pt, 2.0, 2.0);
                    }
            }
            rtb.Render(dv);
            return rtb;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            double deltaTime = watch.ElapsedMilliseconds * 0.001;
            watch.Reset(); watch.Start();

            Point pt = WindowHelper.GetCursorPos();
            double dx = pt.X - _lastPoint.X;
            double dy = pt.Y - _lastPoint.Y;
            double dl = Math.Sqrt(dx * dx + dy * dy);
            bool moved = dl > 0.3;
            if (moved)
            {
                var direction = Math.Atan2(dy, dx) / Math.PI * 180;
                AddDirection(direction);
                AddSpeed(dl / deltaTime);
            }

            if (moved)
            {
                Active();
                TimeMouseActive = TimeMouseActive.Add(TimeSpan.FromSeconds(deltaTime));
            }

            PercentMouseActive = TimeMouseActive.TotalSeconds / TimeActive.TotalSeconds * 100;

            TotalDistance += dl;
            if (moved)
                Add(pt);
            _lastPoint = pt;
            CurrentPosition = pt;
            CurrentApplication = WindowHelper.GetForegroundWindowText();
            
            // imageDisplay.Source = UpdateBitmap(pt);

            RaisePropertyChanged("LastActivity");
            RaisePropertyChanged("TimeMouseActive");
            RaisePropertyChanged("TimeActive");
            RaisePropertyChanged("PercentMouseActive");
        }

        private void Active()
        {
            // if less than 30 seconds since mouse was moved last time - define as active
            var activeSpan = DateTime.Now - LastActivity;
            if (activeSpan.TotalSeconds < 30)
            {
                TimeActive = TimeActive.Add(activeSpan);
            }

            LastActivity = DateTime.Now;
        }

        private void AddSpeed(double p)
        {
            int i = (int)p;
            if (i >= _speedCount.Length)
                i = _speedCount.Length - 1;
            _speedCount[i]++;
        }

        private void AddDirection(double dir)
        {
            int d = (int)dir;
            d = d % 360;
            if (d < 0) d += 360;
            _directionCount[d]++;
        }
    }
}
