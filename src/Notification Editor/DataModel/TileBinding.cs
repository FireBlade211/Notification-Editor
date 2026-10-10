using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using FireBlade.NotificationEditor.Pages;
using Windows.Data.Xml.Dom;
using System.Globalization;
using Windows.UI.Xaml.Data;
using System.Collections.ObjectModel;

namespace FireBlade.NotificationEditor.DataModel
{
    public class TileBinding : DependencyObject
    {
        public TilePage Page { get; set; }
        public XmlElement Element { get; set; }

        public TileBinding(TilePage page, XmlElement el)
        {
            Page = page;
            Element = el;

            AdaptiveGroups = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = AdaptiveGroupsSource
            };
        }

        public static readonly DependencyProperty TemplateNameProperty = DependencyProperty.Register(
            "TemplateName",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty, OnTemplateNameChanged));

        public string TemplateName
        {
            get => (string)GetValue(TemplateNameProperty);
            set => SetValue(TemplateNameProperty, value);
        }

        private static void OnTemplateNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.TemplateName))
                    tb.Element.SetAttribute("template", tb.TemplateName);

                switch (tb.TemplateName)
                {
                    case "TileSmall":
                        tb.DisplayString = "Small";
                        break;
                    case "TileMedium":
                        tb.DisplayString = "Medium";
                        break;
                    case "TileWide":
                        tb.DisplayString = "Wide";
                        break;
                    case "TileLarge":
                        tb.DisplayString = "Large";
                        break;
                    case "TileSquare71x71IconWithBadge":
                        tb.DisplayString = "Small (Iconic)";
                        break;
                    case "TileSquare150x150IconWithBadge":
                        tb.DisplayString = "Large (Iconic)";
                        break;
                }

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty FallbackNameProperty = DependencyProperty.Register(
            "FallbackName",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty, OnFallbackNameChanged));

        public string FallbackName
        {
            get => (string)GetValue(FallbackNameProperty);
            set => SetValue(FallbackNameProperty, value);
        }

        private static void OnFallbackNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.FallbackName))
                    tb.Element.SetAttribute("fallback", tb.FallbackName);
                else
                    tb.Element.RemoveAttribute("fallback");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty LanguageProperty = DependencyProperty.Register(
            "Language",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty, OnLanguageChanged));

        public string Language
        {
            get => (string)GetValue(LanguageProperty);
            set => SetValue(LanguageProperty, value);
        }

        private static void OnLanguageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.Language))
                    tb.Element.SetAttribute("lang", tb.Language);
                else
                    tb.Element.RemoveAttribute("lang");

                tb.Page.UpdateXml();
            }
        }

        public static readonly DependencyProperty PresentationProperty = DependencyProperty.Register(
            "Presentation",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty, OnPresentationChanged));

        public string Presentation
        {
            get => (string)GetValue(PresentationProperty);
            set => SetValue(PresentationProperty, value);
        }

        private static void OnPresentationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.Presentation))
                    tb.Element.SetAttribute("hint-presentation", tb.Presentation);
                else
                    tb.Element.RemoveAttribute("hint-presentation");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty BaseUriProperty = DependencyProperty.Register(
            "BaseUri",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty, OnBaseUriChanged));

        public string BaseUri
        {
            get => (string)GetValue(BaseUriProperty);
            set => SetValue(BaseUriProperty, value);
        }

        private static void OnBaseUriChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.BaseUri))
                    tb.Element.SetAttribute("baseUri", tb.BaseUri);
                else
                    tb.Element.RemoveAttribute("baseUri");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty BrandingProperty = DependencyProperty.Register(
            "Branding",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty, OnBrandingChanged));

        public string Branding
        {
            get => (string)GetValue(BrandingProperty);
            set => SetValue(BrandingProperty, value);
        }

        private static void OnBrandingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.Branding))
                    tb.Element.SetAttribute("branding", tb.Branding);
                else
                    tb.Element.RemoveAttribute("branding");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty AddImageQueryProperty = DependencyProperty.Register(
            "AddImageQuery",
            typeof(bool?),
            typeof(TileBinding),
            new PropertyMetadata(null, OnAddImageQueryChanged));

        public bool? AddImageQuery
        {
            get => (bool?)GetValue(AddImageQueryProperty);
            set => SetValue(AddImageQueryProperty, value);
        }

        private static void OnAddImageQueryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (tb.AddImageQuery != null)
                    tb.Element.SetAttribute("addImageQuery", tb.AddImageQuery == true ? "true" : "false");
                else
                    tb.Element.RemoveAttribute("addImageQuery");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty ContentIdProperty = DependencyProperty.Register(
    "ContentId",
    typeof(string),
    typeof(TileBinding),
    new PropertyMetadata(string.Empty, OnContentIdChanged));

        public string ContentId
        {
            get => (string)GetValue(ContentIdProperty);
            set => SetValue(ContentIdProperty, value);
        }

        private static void OnContentIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.ContentId))
                    tb.Element.SetAttribute("contentId", tb.ContentId);
                else
                    tb.Element.RemoveAttribute("contentId");

                tb.Page.UpdateXml();
            }
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
    "DisplayName",
    typeof(string),
    typeof(TileBinding),
    new PropertyMetadata(string.Empty, OnDisplayNameChanged));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        private static void OnDisplayNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.DisplayName))
                    tb.Element.SetAttribute("displayName", tb.DisplayName);
                else
                    tb.Element.RemoveAttribute("displayName");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty TextStackingProperty = DependencyProperty.Register(
    "TextStacking",
    typeof(string),
    typeof(TileBinding),
    new PropertyMetadata(string.Empty, OnTextStackingChanged));

        public string TextStacking
        {
            get => (string)GetValue(TextStackingProperty);
            set => SetValue(TextStackingProperty, value);
        }

        private static void OnTextStackingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (!string.IsNullOrEmpty(tb.TextStacking))
                    tb.Element.SetAttribute("hint-textStacking", tb.TextStacking);
                else
                    tb.Element.RemoveAttribute("hint-textStacking");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty OverlayProperty = DependencyProperty.Register(
    "Overlay",
    typeof(double),
    typeof(TileBinding),
    new PropertyMetadata(0.0d, OnOverlayChanged));

        public double Overlay
        {
            get => (double)GetValue(OverlayProperty);
            set => SetValue(OverlayProperty, value);
        }

        private static void OnOverlayChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TileBinding tb)
            {
                if (tb.UseOverlay)
                    tb.Element.SetAttribute("hint-overlay", tb.Overlay.ToString(CultureInfo.InvariantCulture));
                else
                    tb.Element.RemoveAttribute("hint-overlay");

                tb.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty UseOverlayProperty = DependencyProperty.Register(
    "UseOverlay",
    typeof(bool),
    typeof(TileBinding),
    new PropertyMetadata(false, OnOverlayChanged));

        public bool UseOverlay
        {
            get => (bool)GetValue(UseOverlayProperty);
            set => SetValue(UseOverlayProperty, value);
        }

        public static readonly DependencyProperty DisplayStringProperty = DependencyProperty.Register(
            "DisplayString",
            typeof(string),
            typeof(TileBinding),
            new PropertyMetadata(string.Empty));

        public string DisplayString
        {
            get => (string)GetValue(DisplayStringProperty);
            set => SetValue(DisplayStringProperty, value);
        }

        public ObservableCollection<AdaptiveGroup> AdaptiveGroupsSource { get; set; } = new ObservableCollection<AdaptiveGroup>();

        public CollectionViewSource AdaptiveGroups { get; set; }

        public ObservableCollection<AdaptiveElement> Content { get; set; } = new ObservableCollection<AdaptiveElement>();
    }
}
