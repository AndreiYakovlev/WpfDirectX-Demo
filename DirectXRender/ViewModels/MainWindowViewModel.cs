using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        MainWindow _window;

        public MainWindowViewModel(MainWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        public static Command CloseWindowCommand
        {
            get => new Command((obj) =>
            {
                if (obj is Window window)
                {
                    window.Close();
                }
            }, (obj) =>
            {
                return obj != null;
            });
        }
    }
}
