using System;
using Windows.UI.Xaml.Data;
using FireBlade.NotificationEditor.DataModel;

namespace FireBlade.NotificationEditor.Markup
{
    public sealed class GroupNameConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            return (value as AdaptiveSubgroup)?.Name;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            throw new NotSupportedException();
        }
    }
}
