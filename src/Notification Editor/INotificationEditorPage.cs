using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using FireBlade.NotificationEditor.Helpers;

namespace FireBlade.NotificationEditor
{
    public interface INotificationEditorPage
    {
        XmlDocument Document { get; set; }

        void UpdatePreview();
        void UpdateXml();
    }

    public class NotificationEditorPageNavArgs : EventArgs
    {
        public string Xml = string.Empty;
        public StorageFile File = null;
        public MruItem MruItem = null;
    }
}
