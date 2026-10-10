using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Globalization;

namespace FireBlade.NotificationEditor.DataModel
{
    public class AdaptiveImage : AdaptiveElement
    {
        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "Id",
            typeof(int),
            typeof(AdaptiveImage),
            new PropertyMetadata(1, OnIdChanged));

        public static void OnIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                ai.Element.SetAttribute("id", ai.Id.ToString(CultureInfo.InvariantCulture));
                ai.Page.UpdateXml();
            }
        }

        public string Src
        {
            get => (string)GetValue(SrcProperty);
            set => SetValue(SrcProperty, value);
        }

        public static readonly DependencyProperty SrcProperty = DependencyProperty.Register(
            "Src",
            typeof(string),
            typeof(AdaptiveImage),
            new PropertyMetadata(string.Empty, OnSrcChanged));

        public static void OnSrcChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                ai.Element.SetAttribute("src", ai.Src);
                ai.Page.UpdatePreview();
            }
        }

        public string Alt
        {
            get => (string)GetValue(AltProperty);
            set => SetValue(AltProperty, value);
        }

        public static readonly DependencyProperty AltProperty = DependencyProperty.Register(
            "Alt",
            typeof(string),
            typeof(AdaptiveImage),
            new PropertyMetadata(string.Empty, OnAltChanged));

        public static void OnAltChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                if (!string.IsNullOrEmpty(ai.Alt))
                    ai.Element.SetAttribute("alt", ai.Alt);
                else
                    ai.Element.RemoveAttribute("alt");

                ai.Page.UpdatePreview();
            }
        }

        public bool? AddQuery
        {
            get => (bool?)GetValue(AddQueryProperty);
            set => SetValue(AddQueryProperty, value);
        }

        public static readonly DependencyProperty AddQueryProperty = DependencyProperty.Register(
            "AddQuery",
            typeof(bool?),
            typeof(AdaptiveImage),
            new PropertyMetadata(null, OnAddQueryChanged));

        public static void OnAddQueryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                if (ai.AddQuery != null)
                    ai.Element.SetAttribute("addImageQuery", ai.AddQuery == true ? "true" : "false");
                else
                    ai.Element.RemoveAttribute("addImageQuery");

                ai.Page.UpdatePreview();
            }
        }

        public string Crop
        {
            get => (string)GetValue(CropProperty);
            set => SetValue(CropProperty, value);
        }

        public static readonly DependencyProperty CropProperty = DependencyProperty.Register(
            "Crop",
            typeof(string),
            typeof(AdaptiveImage),
            new PropertyMetadata(string.Empty, OnCropChanged));

        public static void OnCropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                if (!string.IsNullOrEmpty(ai.Crop))
                    ai.Element.SetAttribute("hint-crop", ai.Crop);
                else
                    ai.Element.RemoveAttribute("hint-crop");

                ai.Page.UpdatePreview();
            }
        }

        public bool RemoveMargin
        {
            get => (bool)GetValue(RemoveMarginProperty);
            set => SetValue(RemoveMarginProperty, value);
        }

        public static readonly DependencyProperty RemoveMarginProperty = DependencyProperty.Register(
            "RemoveMargin",
            typeof(bool),
            typeof(AdaptiveImage),
            new PropertyMetadata(null, OnRemoveMarginChanged));

        public static void OnRemoveMarginChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                if (ai.RemoveMargin)
                    ai.Element.SetAttribute("hint-removeMargin", "true");
                else
                    ai.Element.RemoveAttribute("hint-removeMargin");

                ai.Page.UpdatePreview();
            }
        }

        public string Align
        {
            get => (string)GetValue(AlignProperty);
            set => SetValue(AlignProperty, value);
        }

        public static readonly DependencyProperty AlignProperty = DependencyProperty.Register(
            "Align",
            typeof(string),
            typeof(AdaptiveImage),
            new PropertyMetadata(string.Empty, OnAlignChanged));

        public static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                if (!string.IsNullOrEmpty(ai.Align))
                    ai.Element.SetAttribute("hint-align", ai.Align);
                else
                    ai.Element.RemoveAttribute("hint-align");

                ai.Page.UpdatePreview();
            }
        }

        // Live Tiles only
        public string Placement
        {
            get => (string)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement",
            typeof(string),
            typeof(AdaptiveImage),
            new PropertyMetadata(string.Empty, OnPlacementChanged));

        public static void OnPlacementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveImage ai)
            {
                if (!string.IsNullOrEmpty(ai.Placement))
                    ai.Element.SetAttribute("placement", ai.Placement);
                else
                    ai.Element.RemoveAttribute("placement");

                ai.Page.UpdatePreview();
            }
        }
    }
}
