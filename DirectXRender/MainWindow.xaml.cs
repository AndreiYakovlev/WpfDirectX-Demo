using SharpDX.Direct3D;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel Context;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Context = new MainWindowViewModel(this);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Context.BackgroundColor = BackgroundColorPicker.SelectedColor ?? Color.FromArgb(0, 0, 0, 0);
        }
    }
}