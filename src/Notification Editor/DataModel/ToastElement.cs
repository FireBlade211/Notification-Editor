using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using FireBlade.NotificationEditor.Pages;

namespace FireBlade.NotificationEditor.DataModel
{
    public abstract class ToastElement : DependencyObject
    {
        public XmlElement Element { get; set; }
        public ToastPage Page { get; set; }
    }
}
