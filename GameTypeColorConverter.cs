using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoD2_Launcher
{
    public class GameTypeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string)
            {
                switch ((value as string).ToUpper())
                {
                case "DM": return Colors.DarkRed;
                case "SD": return Colors.Blue;
                case "UTD": return Colors.Purple;
                case "TDM": return Colors.Green;
                case "CTF": return Colors.Orange;
                case "HQ": return Colors.DeepPink;
                default: return Colors.Black;
                }
            }

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}