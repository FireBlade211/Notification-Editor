using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ToastSelection : DependencyObject
    {
        public ToastInput Input { get; set; }
        public XmlElement Element { get; set; }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "Id",
            typeof(string),
            typeof(ToastInput),
            new PropertyMetadata(string.Empty, OnIdChanged_Static));

        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        private static void OnIdChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastSelection sel)
                sel.OnIdChanged(args);
        }

        private void OnIdChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var id = args.NewValue as string;

                if (string.IsNullOrEmpty(id))
                    Element.RemoveAttribute("id");
                else
                    Element.SetAttribute("id", id);

                Input.Page.UpdatePreview();
            }
            catch { }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content",
            typeof(string),
            typeof(ToastInput),
            new PropertyMetadata(string.Empty, OnContentChanged_Static));

        public string Content
        {
            get => (string)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private static void OnContentChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastSelection sel)
                sel.OnContentChanged(args);
        }

        private void OnContentChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var content = args.NewValue as string;

                if (string.IsNullOrEmpty(content))
                    Element.RemoveAttribute("content");
                else
                    Element.SetAttribute("content", content);

                Input.Page.UpdatePreview();
            }
            catch { }
        }
    }
}
