using System.ComponentModel;
using PropertyEditorLibrary;
using UsageStats.Properties;

namespace UsageStats
{
    public class SettingsViewModel : Observable
    {
        private static Settings settings
        {
            get { return Settings.Default; }
        }

        [Category("Reports|Output")]
        [DisplayName("Report path")]
        public string ReportPath
        {
            get { return settings.ReportPath; }
            set
            {
                settings.ReportPath = value;
                RaisePropertyChanged("ReportPath");
            }
        }

        [Category("Statistics|Limits")]
        [DisplayName("Inactivity threshold (sec)")]
        public double InactivityThreshold
        {
            get { return settings.InactivityThreshold; }
            set
            {
                settings.InactivityThreshold = value;
                RaisePropertyChanged("InactivityThreshold");
            }
        }

        [Category("Statistics|Limits")]
        [DisplayName("Interruption threshold (sec)")]
        public double InterruptionThreshold
        {
            get { return settings.InterruptionThreshold; }
            set
            {
                settings.InterruptionThreshold = value;
                RaisePropertyChanged("InterruptionThreshold");
            }
        }


        [Category("Statistics|Limits")]
        [DisplayName("Screen resolution (dpi)")]
        public double ScreenResolution
        {
            get { return settings.ScreenResolution; }
            set
            {
                settings.ScreenResolution = value;
                RaisePropertyChanged("InterruptionThreshold");
            }
        }

        [Category("Applications|List")]
        [Height(200)]
        [DisplayName("Applications")]
        public string ApplicationList
        {
            get { return settings.ApplicationList; }
            set
            {
                settings.ApplicationList = value;
                RaisePropertyChanged("ApplicationList");
            }
        }


        [Category("Maps|Mouse maps")]
        [Slidable(1, 20)]
        [FormatString("0.0")]
        [DisplayName("Mouse click diameter")]
        public double MouseDownSize
        {
            get { return settings.MouseDownSize; }
            set
            {
                settings.MouseDownSize = value;
                RaisePropertyChanged("MouseDownSize");
            }
        }

        [Category("Maps|Mouse maps")]
        [Slidable(1, 10)]
        [FormatString("0.0")]
        [DisplayName("Mouse track width")]
        public double MouseTrackWidth
        {
            get { return settings.MouseTrackWidth; }
            set
            {
                settings.MouseTrackWidth = value;
                RaisePropertyChanged("MouseTrackWidth");
            }
        }

        [Category("Window|Settings")]
        [Slidable(1, 10)]
        [DisplayName("Keep window always on top")]
        public bool AlwaysOnTop
        {
            get { return settings.AlwaysOnTop; }
            set
            {
                settings.AlwaysOnTop = value;
                RaisePropertyChanged("AlwaysOnTop");
            }
        }

        //[Category("Preferences|Auto update")]
        //public bool CheckForUpdates
        //{
        //    get { return settings.CheckForUpdates; }
        //    set
        //    {
        //        settings.CheckForUpdates = value;
        //        RaisePropertyChanged("CheckForUpdates");
        //    }
        //}

        public void Save()
        {
            settings.Save();
        }
    }
}