using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace FireBlade.NotificationEditor.Markup
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => value is Visibility vs && vs == Visibility.Visible;
    }
}
