using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfApp1.Converters
{
    internal class QuaternionToVectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rotation = value as Quaternion?;
            return rotation?.Axis ?? Vector3.Zero;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vector = value as Vector3?;
            return new Quaternion(vector ?? Vector3.Zero, 1);
        }
    }
}