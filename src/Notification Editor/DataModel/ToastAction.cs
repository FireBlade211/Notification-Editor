using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using FireBlade.NotificationEditor.Pages;
using Windows.Data.Xml.Dom;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ToastAction : DependencyObject
    {
        public ToastPage Page { get; set; }
        public XmlElement Element { get; set; }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content",
            typeof(string),
            typeof(ToastAction),
            new PropertyMetadata(string.Empty, OnContentChanged_Static));

        public string Content
        {
            get => (string)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ArgsProperty = DependencyProperty.Register(
            "Args",
            typeof(string),
            typeof(ToastAction),
            new PropertyMetadata(string.Empty, OnArgsChanged_Static));

        public string Args
        {
            get => (string)GetValue(ArgsProperty);
            set => SetValue(ArgsProperty, value);
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type",
            typeof(string),
            typeof(ToastAction),
            new PropertyMetadata(string.Empty, OnTypeChanged_Static));

        public string Type
        {
            get => (string)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty ActivationTypeProperty = DependencyProperty.Register(
            "ActivationType",
            typeof(string),
            typeof(ToastAction),
            new PropertyMetadata("foreground", OnActivationTypeChanged_Static));

        public string ActivationType
        {
            get => (string)GetValue(ActivationTypeProperty);
            set => SetValue(ActivationTypeProperty, value);
        }

        public static readonly DependencyProperty AfterActivationBehaviorProperty = DependencyProperty.Register(
            "AfterActivationBehavior",
            typeof(string),
            typeof(ToastAction),
            new PropertyMetadata("default", OnAfterActivationBehaviorChanged_Static));

        public string AfterActivationBehavior
        {
            get => (string)GetValue(AfterActivationBehaviorProperty);
            set => SetValue(AfterActivationBehaviorProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement",
            typeof(string),
            typeof(ToastAction),
            new PropertyMetadata(string.Empty, OnPlacementChanged_Static));

        public string Placement
        {
            get => (string)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty ImageUriProperty = DependencyProperty.Register(
            "ImageUri",
             typeof(string),
             typeof(ToastAction),
             new PropertyMetadata(string.Empty, OnImageUriChanged_Static));

        public string ImageUri
        {
            get => (string)GetValue(ImageUriProperty);
            set => SetValue(ImageUriProperty, value);
        }

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            "ButtonStyle",
             typeof(string),
             typeof(ToastAction),
             new PropertyMetadata(string.Empty, OnButtonStyleChanged_Static));

        public string ButtonStyle
        {
            get => (string)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(
            "ToolTip",
             typeof(string),
             typeof(ToastAction),
             new PropertyMetadata(string.Empty, OnToolTipChanged_Static));

        public string ToolTip
        {
            get => (string)GetValue(ToolTipProperty);
            set => SetValue(ToolTipProperty, value);
        }

        public static readonly DependencyProperty InputIdProperty = DependencyProperty.Register(
            "InputId",
             typeof(string),
             typeof(ToastAction),
             new PropertyMetadata(string.Empty, OnInputIdChanged_Static));

        public string InputId
        {
            get => (string)GetValue(InputIdProperty);
            set => SetValue(InputIdProperty, value);
        }

        private static void OnContentChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnContentChanged(args);
        }

        private void OnContentChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                Element?.SetAttribute("content", args.NewValue as string);
                Page.UpdatePreview();
            }
            catch
            {
            }
        }

        private static void OnArgsChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnArgsChanged(args);
        }

        private void OnArgsChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                Element?.SetAttribute("arguments", args.NewValue as string);
                Page.UpdatePreview();
            }
            catch { }
        }

        private static void OnTypeChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnTypeChanged(args);
        }

        private void OnTypeChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                Element?.SetAttribute("type", args.NewValue as string);
                Page.UpdateXml();
            }
            catch { }
        }

        private static void OnActivationTypeChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnActivationTypeChanged(args);
        }

        private void OnActivationTypeChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                Element?.SetAttribute("activationType", args.NewValue as string);
                Page.UpdatePreview();
            }
            catch { }
        }

        private static void OnAfterActivationBehaviorChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnAfterActivationBehaviorChanged(args);
        }

        private void OnAfterActivationBehaviorChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                if (args.NewValue is string s)
                    Element?.SetAttribute("afterActivationBehavior", s);
                else
                    Element?.RemoveAttribute("afterActivationBehavior");

                Page.UpdateXml();
            }
            catch { }
        }

        private static void OnPlacementChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnPlacementChanged(args);
        }

        private void OnPlacementChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var placement = args.NewValue as string;

                if (string.IsNullOrEmpty(placement))
                    Element?.RemoveAttribute("placement");
                else
                    Element?.SetAttribute("placement", placement);

                Page.UpdatePreview();
            }
            catch { }
        }

        private static void OnImageUriChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnImageUriChanged(args);
        }

        private void OnImageUriChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var uri = args.NewValue as string;

                if (string.IsNullOrEmpty(uri))
                    Element?.RemoveAttribute("imageUri");
                else
                    Element?.SetAttribute("imageUri", uri);

                Page.UpdatePreview();
            }
            catch { }
        }

        private static void OnButtonStyleChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnButtonStyleChanged(args);
        }

        private void OnButtonStyleChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var style = args.NewValue as string;

                if (string.IsNullOrEmpty(style))
                    Element?.RemoveAttribute("hint-buttonStyle");
                else
                    Element?.SetAttribute("hint-buttonStyle", style);

                Page.UpdatePreview();
            }
            catch { }
        }

        private static void OnToolTipChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnToolTipChanged(args);
        }

        private void OnToolTipChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var tip = args.NewValue as string;

                if (string.IsNullOrEmpty(tip))
                    Element?.RemoveAttribute("hint-toolTip");
                else
                    Element?.SetAttribute("hint-toolTip", tip);

                Page.UpdatePreview();
            }
            catch { }
        }

        private static void OnInputIdChanged_Static(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastAction action)
                action.OnInputIdChanged(args);
        }

        private void OnInputIdChanged(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var id = args.NewValue as string;

                if (string.IsNullOrEmpty(id))
                    Element?.RemoveAttribute("hint-inputId");
                else
                    Element?.SetAttribute("hint-inputId", id);

                Page.UpdatePreview();
            }
            catch { }
        }

        public ToastAction(ToastPage p, XmlElement el = null)
        {
            Page = p;
            Element = el;
        }
    }
}
