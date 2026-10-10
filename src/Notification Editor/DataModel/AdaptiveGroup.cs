using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.Data.Xml.Dom;

namespace FireBlade.NotificationEditor.DataModel
{
    public class AdaptiveGroup : ObservableCollection<AdaptiveSubgroup>
    {
        public string Name { get; set; } = string.Empty;
        public XmlElement Element { get; set; }
    }
}
