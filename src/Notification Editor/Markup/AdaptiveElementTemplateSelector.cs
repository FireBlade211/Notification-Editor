using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FireBlade.NotificationEditor.DataModel;

namespace FireBlade.NotificationEditor.Markup
{
    public class AdaptiveElementTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate  { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is AdaptiveText)
                return TextTemplate;

            if (item is AdaptiveImage)
                return ImageTemplate;

            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            => SelectTemplateCore(item);
    }
}
