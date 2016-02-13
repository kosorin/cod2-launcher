using System;
using System.Globalization;
using System.Windows.Data;

namespace CoD2_Launcher.Converters
{
    public class RefreshRateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is int)
            {
                int minutes = (int)value;

                if (minutes == 0) return "neobnovovat";
                if (minutes == 1) return "každou minutu";
                if (minutes > 1 && minutes < 5) return string.Format("každé {0} minuty", minutes);
                if (minutes >= 5) return string.Format("každých {0} minut", minutes);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}