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
        public double InterruptionThreshold
        {
            get { return settings.InterruptionThreshold; }
            set
            {
                settings.InterruptionThreshold = value;
                RaisePropertyChanged("InterruptionThreshold");
            }
        }

        [Category("Applications|List")]
        [Height(200)]
        public string ApplicationList
        {
            get { return settings.ApplicationList; }
            set
            {
                settings.ApplicationList = value;
                RaisePropertyChanged("ApplicationList");
            }
        }


        [Category("Style|Mouse maps")]
        [Slidable(1, 20)]
        public double MouseDownSize
        {
            get { return settings.MouseDownSize; }
            set
            {
                settings.MouseDownSize = value;
                RaisePropertyChanged("MouseDownSize");
            }
        }

        [Category("Style|Mouse maps")]
        [Slidable(1, 10)]
        public double MouseTrackWidth
        {
            get { return settings.MouseTrackWidth; }
            set
            {
                settings.MouseTrackWidth = value;
                RaisePropertyChanged("MouseTrackWidth");
            }
        }

        [Category("Preferences|Auto update")]
        public bool CheckForUpdates
        {
            get { return settings.CheckForUpdates; }
            set
            {
                settings.CheckForUpdates = value;
                RaisePropertyChanged("CheckForUpdates");
            }
        }

        public void Save()
        {
            settings.Save();
        }
    }
}