using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Globalization;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ToastImage : ToastElement
    {
        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "Id",
            typeof(int),
            typeof(ToastImage),
            new PropertyMetadata(1, OnIdChanged));

        public static void OnIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastImage ti)
            {
                ti.Element.SetAttribute("id", ti.Id.ToString(CultureInfo.InvariantCulture));
                ti.Page.UpdateXml();
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
            typeof(ToastImage),
            new PropertyMetadata(string.Empty, OnSrcChanged));

        public static void OnSrcChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastImage ti)
            {
                ti.Element.SetAttribute("src", ti.Src);
                ti.Page.UpdatePreview();
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
            typeof(ToastImage),
            new PropertyMetadata(string.Empty, OnAltChanged));

        public static void OnAltChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastImage ti)
            {
                if (!string.IsNullOrEmpty(ti.Alt))
                    ti.Element.SetAttribute("alt", ti.Alt);
                else
                    ti.Element.RemoveAttribute("alt");

                ti.Page.UpdatePreview();
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
            typeof(ToastImage),
            new PropertyMetadata(null, OnAddQueryChanged));

        public static void OnAddQueryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastImage ti)
            {
                if (ti.AddQuery != null)
                    ti.Element.SetAttribute("addImageQuery", ti.AddQuery == true ? "true" : "false");
                else
                    ti.Element.RemoveAttribute("addImageQuery");

                ti.Page.UpdatePreview();
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
            typeof(ToastImage),
            new PropertyMetadata(string.Empty, OnCropChanged));

        public static void OnCropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastImage ti)
            {
                if (!string.IsNullOrEmpty(ti.Crop))
                    ti.Element.SetAttribute("hint-crop", ti.Crop);
                else
                    ti.Element.RemoveAttribute("hint-crop");

                ti.Page.UpdatePreview();
            }
        }

        public string Placement
        {
            get => (string)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement",
            typeof(string),
            typeof(ToastImage),
            new PropertyMetadata(string.Empty, OnPlacementChanged));

        public static void OnPlacementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastImage ti)
            {
                if (!string.IsNullOrEmpty(ti.Placement))
                    ti.Element.SetAttribute("placement", ti.Placement);
                else
                    ti.Element.RemoveAttribute("placement");

                ti.Page.UpdatePreview();
            }
        }
    }
}
