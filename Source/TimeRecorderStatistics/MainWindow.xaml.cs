// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace TimeRecorderStatistics
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using UsageStats;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime firstDayOfWeek;

        public MainWindow()
        {
            this.InitializeComponent();
            this.PreviousWeekCommand = new DelegateCommand(x => this.FirstDayOfWeek = this.FirstDayOfWeek.AddDays(-7));
            this.NextWeekCommand = new DelegateCommand(x => this.FirstDayOfWeek = this.FirstDayOfWeek.AddDays(7));
            this.CreateReportCommand = new DelegateCommand(x => CreateReport());
            this.ClearCategoriesCommand = new DelegateCommand(
                x =>
                    {
                        foreach (var c in this.Categories)
                        {
                            c.IsChecked = false;
                        }
                    });
            this.Week = new ObservableCollection<Statistics>();
            this.Header = Statistics.RenderHeader();

            this.Machines = new ObservableCollection<CheckableItem>();
            this.Categories = new ObservableCollection<CheckableItem>();

            var folder = TimeRecorder.RootFolder;
            foreach (var dir in Directory.GetDirectories(folder))
            {
                this.Machines.Add(new CheckableItem(this.MachineChanged) { Header = Path.GetFileName(dir) });
            }

            this.Categories.Add(new CheckableItem(this.CategoryChanged) { Header = "Unknown" });
            foreach (var cat in TimeRecorder.LoadCategories())
            {
                this.Categories.Add(new CheckableItem(this.CategoryChanged) { Header = Path.GetFileName(cat) });
            }

            var today = DateTime.Now.Date;
            this.FirstDayOfWeek = today.FirstDayOfWeek(CultureInfo.CurrentCulture);

            this.DataContext = this;
        }

        private void CreateReport()
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<Statistics> Week { get; private set; }

        public ObservableCollection<CheckableItem> Machines { get; private set; }

        public ObservableCollection<CheckableItem> Categories { get; private set; }

        public ICommand ClearCategoriesCommand { get; private set; }

        public ICommand PreviousWeekCommand { get; private set; }

        public ICommand NextWeekCommand { get; private set; }

        public ICommand CreateReportCommand { get; private set; }

        public ImageSource Header { get; private set; }

        public DateTime FirstDayOfWeek
        {
            get
            {
                return this.firstDayOfWeek;
            }

            set
            {
                this.firstDayOfWeek = value;
                this.Refresh();
            }
        }

        private void Refresh()
        {
            this.Week.Clear();
            for (int i = 0; i < 7; i++)
            {
                this.Week.Add(this.Load(this.FirstDayOfWeek.AddDays(i)));
            }
        }

        private void CategoryChanged(CheckableItem category)
        {
            this.Refresh();
        }

        private void MachineChanged(CheckableItem machine)
        {
            this.Refresh();
        }

        private Statistics Load(DateTime date)
        {
            var s = new Statistics(date);
            var categories = this.Categories.Where(c => c.IsChecked).Select(c => c.Header).ToList();
            foreach (var m in this.Machines.Where(m => m.IsChecked))
            {
                s.Add(m.Header, categories);
            }

            return s;
        }
    }

    public class MachineStatistics
    {
    }
}