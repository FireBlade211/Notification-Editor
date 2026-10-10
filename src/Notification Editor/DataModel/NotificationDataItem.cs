using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FireBlade.NotificationEditor.DataModel
{
    public class NotificationDataItem : DependencyObject
    {
        public INotificationEditorPage Page { get; set; }

        public NotificationDataItem(INotificationEditorPage p)
        {
            Page = p;
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            "Key",
            typeof(string),
            typeof(NotificationDataItem),
            new PropertyMetadata(string.Empty, OnPropChanged));
        
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(string),
            typeof(NotificationDataItem),
            new PropertyMetadata(string.Empty, OnPropChanged));

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void OnPropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is NotificationDataItem item)
                item.Page.UpdatePreview();
        }
    }
}
