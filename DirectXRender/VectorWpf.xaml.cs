using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class VectorWpf : UserControl
    {
        public static readonly DependencyProperty ValueProperty;
        public static readonly DependencyProperty DefaultValueProperty;

        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;
        public static readonly DependencyProperty ZProperty;

        public VectorWpf()
        {
            InitializeComponent();
        }

        static VectorWpf()
        {
            ValueProperty = DependencyProperty.Register("Value", typeof(Vector3), typeof(VectorWpf),
                new FrameworkPropertyMetadata(Vector3.Zero, ValueChanged));

            DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(Vector3), typeof(VectorWpf),
                new FrameworkPropertyMetadata(Vector3.Zero));

            XProperty = DependencyProperty.Register("X", typeof(float), typeof(VectorWpf),
                 new FrameworkPropertyMetadata(0.0f, XChanged));
            YProperty = DependencyProperty.Register("Y", typeof(float), typeof(VectorWpf),
                 new FrameworkPropertyMetadata(0.0f, YChanged));
            ZProperty = DependencyProperty.Register("Z", typeof(float), typeof(VectorWpf),
                 new FrameworkPropertyMetadata(0.0f, ZChanged));
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vector = d as VectorWpf;
            var value = e.NewValue as Vector3?;
            if (vector != null)
            {
                vector.SetValue(XProperty, value?.X ?? 0);
                vector.SetValue(YProperty, value?.Y ?? 0);
                vector.SetValue(ZProperty, value?.Z ?? 0);
            }
        }

        private static void XChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as VectorWpf;
            var vector = element?.Value ?? Vector3.Zero;
            var newValue = (float)e.NewValue;
            element.Value = new Vector3(newValue, vector.Y, vector.Z);
        }

        private static void YChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as VectorWpf;
            var vector = element?.Value ?? Vector3.Zero;
            var newValue = (float)e.NewValue;
            element.Value = new Vector3(vector.X, newValue, vector.Z);
        }

        private static void ZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as VectorWpf;
            var vector = element?.Value ?? Vector3.Zero;
            var newValue = (float)e.NewValue;
            element.Value = new Vector3(vector.X, vector.Y, newValue);
        }

        public Vector3 Value
        {
            get { return (Vector3)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public Vector3 DefaultValue
        {
            get { return (Vector3)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Value = DefaultValue;
        }

        public float X
        {
            get => (float)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public float Y
        {
            get => (float)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public float Z
        {
            get => (float)GetValue(ZProperty);
            set => SetValue(ZProperty, value);
        }
    }
}