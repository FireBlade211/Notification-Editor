using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace FireBlade.NotificationEditor.Markup
{
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int i)
                return i.ToString();

            return parameter is string s ? s : "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string s)
            {
                if (double.TryParse(s, out double result) &&
                    result >= int.MinValue &&
                    result <= int.MaxValue)
                {
                    return (int)Math.Round(result, MidpointRounding.AwayFromZero);
                }
            }

            return 0;
            //return DependencyProperty.UnsetValue;
        }
    }
}
