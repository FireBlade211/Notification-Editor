using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Data.Xml.Dom;

namespace FireBlade.NotificationEditor.DataModel
{
    public abstract class AdaptiveElement : DependencyObject
    {
        public XmlElement Element { get; set; }
        public INotificationEditorPage Page { get; set; }
        public AdaptiveSubgroup Subgroup { get; set; }
        public TileBinding Binding { get; set; }
    }
}
