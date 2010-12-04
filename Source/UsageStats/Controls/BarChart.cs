using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UsageStats
{
    public class BarChart : ItemsControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(BarChart), new UIPropertyMetadata(null));

        public string HorizontalAxisTitle
        {
            get { return (string)GetValue(HorizontalAxisTitleProperty); }
            set { SetValue(HorizontalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisTitleProperty =
            DependencyProperty.Register("HorizontalAxisTitle", typeof(string), typeof(BarChart), new UIPropertyMetadata(null));

        
        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }

        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register("ScaleY", typeof(double), typeof(BarChart), new UIPropertyMetadata(1.0));

        public double BarWidth
        {
            get { return (double)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); }
        }

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register("BarWidth", typeof(double), typeof(BarChart), new UIPropertyMetadata(30.0));

        public Binding ValueBinding
        {
            get { return (Binding)GetValue(ValueBindingProperty); }
            set { SetValue(ValueBindingProperty, value); }
        }

        public static readonly DependencyProperty ValueBindingProperty =
            DependencyProperty.Register("ValueBinding", typeof(Binding), typeof(BarChart), new UIPropertyMetadata(null));


        public Binding KeyBinding
        {
            get { return (Binding)GetValue(KeyBindingProperty); }
            set { SetValue(KeyBindingProperty, value); }
        }

        public static readonly DependencyProperty KeyBindingProperty =
            DependencyProperty.Register("KeyBinding", typeof(Binding), typeof(BarChart), new UIPropertyMetadata(null));



        static BarChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BarChart), new FrameworkPropertyMetadata(typeof(BarChart)));
        }

        public BarChart()
        {
            // var pts = new PointCollection();
            // pts.Add(new Point(10, 20));
            // pts.Add(new Point(20, 30));
            // ItemsSource = pts;

            //            TypeDescriptor.GetProperties(this)["ItemsSource"].AddValueChanged(this, new EventHandler(ItemsSourceChanged)); 
        }

        private ItemsControl itemsControl;
        private TextBlock titleControl;
        private TextBlock horizontalAxisTitleControl;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.itemsControl = this.GetTemplateChild("PART_Items") as ItemsControl;
            this.titleControl = this.GetTemplateChild("PART_Title") as TextBlock;
            this.horizontalAxisTitleControl = this.GetTemplateChild("PART_HorizontalAxisTitle") as TextBlock;
            itemsControl.SizeChanged += OnSizeChanged;
        }


        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoScale();
        }

        private void ItemsSourceChanged(object sender, EventArgs e)
        {
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
         //   Trace.WriteLine("ItemsSourceChanged: " + newValue.ToString());
            AutoScale();
        }

        private void AutoScale()
        {
            if (itemsControl == null)
                return;
            double max = double.MinValue;
            foreach (var item in Items)
            {
                object o = item.GetType().GetProperty("Value").GetValue(item, null);
                double value = double.NaN;
                if (o is double)
                    value = (double)o;
                if (o is int)
                    value = (int)o;

                if (value > max) max = value;
            }
            double height = itemsControl.ActualHeight - 40;
            if (height > 0 && max > 0)
                ScaleY = height / max;
        }
    }
}
