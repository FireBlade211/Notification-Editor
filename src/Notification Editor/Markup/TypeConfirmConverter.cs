using System;
using Windows.UI.Xaml.Data;

namespace FireBlade.NotificationEditor.Markup
{
    // converter to make xaml shut up when we know the type is right
    // (note: win10 1507 does not support casts in x:Bind)
    public partial class TypeConfirmConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            if (value == null)
                return System.Convert.ChangeType(parameter, targetType);

            if (value.GetType() == targetType)
                return value;

            if (targetType == typeof(int) && value is string)
                return int.Parse((string)value);

            return System.Convert.ChangeType(parameter, targetType);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language)
            => value;
    }
}
