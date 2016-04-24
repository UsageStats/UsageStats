using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PropertyTools.Wpf;
using System.IO;
using System.Runtime.Serialization.Json;

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
            this.InitializeComponent();
            this.vm = new MainViewModel();
            this.DataContext = vm;

            this.Loaded += this.Window1_Loaded;
            this.Closed += this.Window1_Closed;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, this.OpenCmdExecuted));
        }

        public bool CanClose { get; set; }

        private void Window1_Closed(object sender, EventArgs e)
        {
            this.vm.OnClosed();
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShowAndActivate();
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            // this will remove the application from the task manager list (alt+tab list)
            this.SetDesktopAsOwner();
        }

        public void SetDesktopAsOwner()
        {
            var wih = new WindowInteropHelper(this) { Owner = GetDesktopWindow() };
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        protected override void OnClosed(EventArgs e)
        {
            this.vm.SaveReports();
            this.vm.Dispose();
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

        private void SaveReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.SaveReports();                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var d = new PropertyDialog
                        {
                            Icon = Icon,
                            DataContext = vm.Settings,
                            Title = "Application preferences",
                            Topmost = this.Topmost
                        };
            d.ShowDialog();
            this.vm.OnSettingsChanged();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.CanClose = true;
            this.Close();
        }

        private void ProjectWebPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/objorke/UsageStats");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var d = new AboutDialog(this)
                        {
                            Topmost = this.Topmost,
                            Image =
                                new BitmapImage(new Uri(@"pack://application:,,,/UsageStats;component/Images/chart.png")),
                            Title = "About Application Usage Statistics",
                            UpdateStatus = "The program is updated."
                        };
            d.ShowDialog();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            this.ShowAndActivate();
        }

        private void ShowAndActivate()
        {
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = !this.CanClose;
            if (e.Cancel)
            {
                this.WindowState = WindowState.Minimized;
            }
        }
    }
}