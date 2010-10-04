using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PropertyEditorLibrary;

namespace UsageStats
{
    // http://mousetracker.jbfreeman.net/index.htm

    // http://www.codeproject.com/KB/system/KeyLogger.aspx
    // http://www.axino.net/tutorial/2009/05/keylogger-in-c-hooking-and-unhooking-keyboard-hook

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly MainViewModel vm;

        public Window1()
        {
            InitializeComponent();
            vm = new MainViewModel();
            DataContext = vm;

            Loaded += Window1_Loaded;
            Closed += Window1_Closed;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenCmdExecuted));
        }

        public bool CanClose { get; set; }

        private void Window1_Closed(object sender, EventArgs e)
        {
            vm.OnClosed();
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ShowAndActivate();
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            // this will remove the application from the task manager list (alt+tab list)
            SetDesktopAsOwner();
        }

        public void SetDesktopAsOwner()
        {
            var wih = new WindowInteropHelper(this) { Owner = GetDesktopWindow() };
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        protected override void OnClosed(EventArgs e)
        {
            vm.SaveReports();
            vm.Dispose();
            base.OnClosed(e);
        }

        private void ReportFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(vm.Settings.ReportPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var d = new PropertyDialog { Icon = Icon, DataContext = vm.Settings, Title = "Application preferences" };
            d.ShowDialog();
            vm.Settings.Save();
            vm.AddApplications();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            CanClose = true;
            Close();
        }

        private void ProjectWebPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://usagestats.codeplex.com");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var d = new AboutDialog(this);
            d.Image = new BitmapImage(new Uri(@"pack://application:,,,/UsageStats;component/Images/chart.png"));
            d.Title = "About Application Usage Statistics";
            d.UpdateStatus = "The program is updated.";
            d.ShowDialog();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            ShowAndActivate();
        }

        private void ShowAndActivate()
        {
            WindowState = WindowState.Normal;
            Activate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = !CanClose;
            if (e.Cancel)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                NotifyIcon.Dispose();
            }
        }
    }
}