namespace TimeRecorderStatistics
{
    using System;

    public class CheckableItem : Observable
    {
        private readonly Action<CheckableItem> checkedChanged;

        private bool isChecked;

        public CheckableItem(Action<CheckableItem> checkedChanged)
        {
            this.checkedChanged = checkedChanged;
            this.isChecked = true;
        }

        public string Header { get; set; }

        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }

            set
            {
                this.isChecked = value;
                this.checkedChanged(this);
                this.RaisePropertyChanged("IsChecked");
            }
        }
    }
}