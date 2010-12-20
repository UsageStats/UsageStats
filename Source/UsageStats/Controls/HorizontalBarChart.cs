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
    public class HorizontalBarChart : ItemsControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HorizontalBarChart), new UIPropertyMetadata(null));

        public string VerticalAxisTitle
        {
            get { return (string)GetValue(VerticalAxisTitleProperty); }
            set { SetValue(VerticalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisTitleProperty =
            DependencyProperty.Register("VerticalAxisTitle", typeof(string), typeof(HorizontalBarChart), new UIPropertyMetadata(null));

        
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(HorizontalBarChart), new UIPropertyMetadata(1.0));

        public double BarHeight
        {
            get { return (double)GetValue(BarHeightProperty); }
            set { SetValue(BarHeightProperty, value); }
        }

        public static readonly DependencyProperty BarHeightProperty =
            DependencyProperty.Register("BarHeight", typeof(double), typeof(HorizontalBarChart), new UIPropertyMetadata(30.0));

        public Binding ValueBinding
        {
            get { return (Binding)GetValue(ValueBindingProperty); }
            set { SetValue(ValueBindingProperty, value); }
        }

        public static readonly DependencyProperty ValueBindingProperty =
            DependencyProperty.Register("ValueBinding", typeof(Binding), typeof(HorizontalBarChart), new UIPropertyMetadata(null));


        public Binding KeyBinding
        {
            get { return (Binding)GetValue(KeyBindingProperty); }
            set { SetValue(KeyBindingProperty, value); }
        }

        public static readonly DependencyProperty KeyBindingProperty =
            DependencyProperty.Register("KeyBinding", typeof(Binding), typeof(HorizontalBarChart), new UIPropertyMetadata(null));



        static HorizontalBarChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HorizontalBarChart), new FrameworkPropertyMetadata(typeof(HorizontalBarChart)));
        }
       

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SizeChanged += OnSizeChanged;
        }


        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoScale();
        }
       
        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
         //   Trace.WriteLine("ItemsSourceChanged: " + newValue.ToString());
            AutoScale();
        }

        private void AutoScale()
        {
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
            double width = this.ActualWidth - 160;
            if (width > 0 && max > 0)
                Scale = width / max;
        }
    }
}
