using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Data.Xml.Dom;
using FireBlade.NotificationEditor.Pages;
using System.Collections.ObjectModel;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ToastInput : DependencyObject
    {
        public ToastPage Page { get; set; }
        public XmlElement Element { get; set; }

        private bool _empty;
        public bool IsEmpty => _empty;

        private ToastInput()
        {
            _empty = true;

            UpdateDisplayString();
        }

        public ToastInput(ToastPage p, XmlElement el)
        {
            Page = p;
            Element = el;
        }

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
            if (obj is ToastInput inp)
                inp.OnIdChanged(args);
        }

        private void OnIdChanged(DependencyPropertyChangedEventArgs args)
        {
            if (_empty) return;

            try
            {
                var id = args.NewValue as string;

                if (string.IsNullOrEmpty(id))
                    Element.RemoveAttribute("id");
                else
                    Element.SetAttribute("id", id);

                Page.UpdatePreview();
                UpdateDisplayString();
            }
            catch { }
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type",
            typeof(string),
            typeof(ToastInput),
            new PropertyMetadata("text", OnTypeChanged_Static));

        public string Type
        {
            get => (string)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        private static void OnTypeChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastInput inp)
                inp.OnTypeChanged(args);
        }

        private void OnTypeChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var type = args.NewValue as string;

                if (string.IsNullOrEmpty(type))
                    Element.RemoveAttribute("type");
                else
                    Element.SetAttribute("type", type);

                Page.UpdatePreview();
            }
            catch { }
        }

        public static readonly DependencyProperty PlaceholderContentProperty = DependencyProperty.Register(
            "PlaceholderContent",
            typeof(string),
            typeof(ToastInput),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged_Static));

        public string PlaceholderContent
        {
            get => (string)GetValue(PlaceholderContentProperty);
            set => SetValue(PlaceholderContentProperty, value);
        }

        private static void OnPlaceholderChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastInput inp)
                inp.OnPlaceholderChanged(args);
        }

        private void OnPlaceholderChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var placeholder = args.NewValue as string;

                if (string.IsNullOrEmpty(placeholder))
                    Element.RemoveAttribute("placeHolderContent");
                else
                    Element.SetAttribute("placeHolderContent", placeholder);

                Page.UpdatePreview();
                UpdateDisplayString();
            }
            catch { }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(ToastInput),
            new PropertyMetadata(string.Empty, OnTitleChanged_Static));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitleChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastInput inp)
                inp.OnTitleChanged(args);
        }

        private void OnTitleChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var title = args.NewValue as string;

                if (string.IsNullOrEmpty(title))
                    Element.RemoveAttribute("title");
                else
                    Element.SetAttribute("title", title);

                Page.UpdatePreview();
                UpdateDisplayString();
            }
            catch { }
        }

        public ObservableCollection<ToastSelection> Options { get; set; } = new ObservableCollection<ToastSelection>();

        public static readonly DependencyProperty DisplayStringProperty = DependencyProperty.Register(
            "DisplayString",
            typeof(string),
            typeof(ToastInput),
            new PropertyMetadata(string.Empty));

        public string DisplayString
        {
            get => (string)GetValue(DisplayStringProperty);
            set => SetValue(DisplayStringProperty, value);
        }

        public static readonly ToastInput Empty = new ToastInput();

        public void UpdateDisplayString()
        {
            DisplayString = _empty ? "None" : ((string.IsNullOrWhiteSpace(Title) ? PlaceholderContent : Title) + $" (ID: {Id})");
        }
    }
}
