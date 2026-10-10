using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using FireBlade.NotificationEditor.DataModel;

namespace FireBlade.NotificationEditor.Markup
{
    public class ToastElementTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ToastTextTemplate     { get; set; }
        public DataTemplate ToastImageTemplate    { get; set; }
        public DataTemplate ToastProgressTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ToastText)
                return ToastTextTemplate;

            if (item is ToastImage)
                return ToastImageTemplate;

            if (item is ToastProgress)
                return ToastProgressTemplate;

            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            => SelectTemplateCore(item);
    }
}
