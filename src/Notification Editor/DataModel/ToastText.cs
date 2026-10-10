using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Globalization;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ToastText : ToastElement
    {
        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "Id",
            typeof(int),
            typeof(ToastText),
            new PropertyMetadata(1, OnIdChanged));

        public static void OnIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastText tt)
            {
                tt.Element.SetAttribute("id", tt.Id.ToString(CultureInfo.InvariantCulture));
                tt.Page.UpdateXml();
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(ToastText),
            new PropertyMetadata(string.Empty, OnTextChanged));

        public static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastText tt)
            {
                try
                {
                    tt.Element.InnerText = tt.Text;
                    tt.Page.UpdatePreview();
                }
                catch (Exception ex)
                {
                    _ = new ContentDialog
                    {
                        Title = "Error",
                        Content = "An error occured.\n\n" + ex.Message,
                        PrimaryButtonText = "OK"
                    }.ShowAsync();
                }
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
            typeof(ToastText),
            new PropertyMetadata(string.Empty, OnPlacementChanged));

        public static void OnPlacementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastText tt)
            {
                if (string.IsNullOrEmpty(tt.Placement))
                    tt.Element.RemoveAttribute("placement");
                else
                    tt.Element.SetAttribute("placement", tt.Placement);

                tt.Page.UpdatePreview();
            }
        }

        public bool CallScenarioCenterAlign
        {
            get => (bool)GetValue(CallScenarioCenterAlignProperty);
            set => SetValue(CallScenarioCenterAlignProperty, value);
        }

        public static readonly DependencyProperty CallScenarioCenterAlignProperty = DependencyProperty.Register(
            "CallScenarioCenterAlign",
            typeof(bool),
            typeof(ToastText),
            new PropertyMetadata(false, OnCallScenarioCenterAlignChanged));

        public static void OnCallScenarioCenterAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastText tt)
            {
                if (tt.CallScenarioCenterAlign)
                    tt.Element.SetAttribute("hint-callScenarioCenterAlign", "true");
                else
                    tt.Element.RemoveAttribute("hint-callScenarioCenterAlign");

                tt.Page.UpdatePreview();
            }
        }
    }
}
