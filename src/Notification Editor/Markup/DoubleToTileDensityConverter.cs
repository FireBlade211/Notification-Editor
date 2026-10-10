using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using NotificationsVisualizerLibrary;

namespace FireBlade.NotificationEditor.Markup
{
    public partial class DoubleToTileDensityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is double)) return TileDensity.Desktop();

            TileDensity density = null;
            double d = (double)value;

            if (parameter?.ToString().Equals("Mobile", StringComparison.OrdinalIgnoreCase) == true)
                density = TileDensity.Mobile(d / 100.0f);
            else if (parameter?.ToString().Equals("Tablet", StringComparison.OrdinalIgnoreCase) == true)
                density = TileDensity.Tablet();
            else
                density = TileDensity.Desktop();

            return density;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
