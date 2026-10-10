using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace FireBlade.NotificationEditor.Markup
{
    public class StringBoolConverter : IValueConverter
    {
        private bool ParamToBool(object param)
        {
            if (param is bool b)
                return b;

            string str = param?.ToString() ?? "false";
            return bool.Parse(str);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result = false;

            if (value is string s)
                result = !string.IsNullOrEmpty(s);

            if (ParamToBool(parameter))
                return !result;
            else
                return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
