using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NodeTapGui.Converters
{
    public class HostDelayColorTextConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value.ToString();
            if (!string.IsNullOrWhiteSpace(str) && str != "time out")
            {
                int o = int.Parse(str.Substring(0, str.IndexOf(" ms")));
                if (0 < o && o <= 120)
                    return "Green";
                else
                    return "Red";
            }
            return "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
