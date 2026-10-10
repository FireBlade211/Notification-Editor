using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Globalization;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ToastProgress : ToastElement
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(ToastProgress),
            new PropertyMetadata(string.Empty, OnTitleChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static void OnTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastProgress tp)
            {
                if (!string.IsNullOrEmpty(tp.Title))
                    tp.Element.SetAttribute("title", tp.Title);
                else
                    tp.Element.RemoveAttribute("title");

                tp.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status",
            typeof(string),
            typeof(ToastProgress),
            new PropertyMetadata(string.Empty, OnStatusChanged));

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static void OnStatusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastProgress tp)
            {
                tp.Element.SetAttribute("status", tp.Status);
                tp.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty IndeterminateProperty = DependencyProperty.Register(
            "Indeterminate",
            typeof(bool),
            typeof(ToastProgress),
            new PropertyMetadata(false, OnIndeterminateChanged));

        public bool Indeterminate
        {
            get => (bool)GetValue(IndeterminateProperty);
            set => SetValue(IndeterminateProperty, value);
        }

        public static void OnIndeterminateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastProgress tp)
            {
                if (tp.Indeterminate)
                    tp.Element.SetAttribute("value", "indeterminate");
                else
                    tp.Element.SetAttribute("value", (tp.Value / 100.0).ToString(CultureInfo.InvariantCulture));

                tp.Page.UpdatePreview();
                tp.UpdateCanEdit();
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(ToastProgress),
            new PropertyMetadata(0.0, OnValueChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastProgress tp)
            {
                if (!string.IsNullOrEmpty(tp.ValueBind)) return;

                tp.Element.SetAttribute("value", (tp.Value / 100.0).ToString(CultureInfo.InvariantCulture));

                tp.Page.UpdatePreview();
                tp.UpdateCanEdit();
            }
        }

        public static readonly DependencyProperty ValueStringOverrideProperty = DependencyProperty.Register(
            "ValueStringOverride",
            typeof(string),
            typeof(ToastProgress),
            new PropertyMetadata(string.Empty, OnValueStringOverrideChanged));

        public string ValueStringOverride
        {
            get => (string)GetValue(ValueStringOverrideProperty);
            set => SetValue(ValueStringOverrideProperty, value);
        }

        public static void OnValueStringOverrideChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastProgress tp)
            {
                if (!string.IsNullOrEmpty(tp.ValueStringOverride))
                    tp.Element.SetAttribute("valueStringOverride", tp.ValueStringOverride);
                else
                    tp.Element.RemoveAttribute("valueStringOverride");

                tp.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty ValueBindProperty = DependencyProperty.Register(
            "ValueBind",
            typeof(string),
            typeof(ToastProgress),
            new PropertyMetadata(string.Empty, OnValueBindChanged));

        public string ValueBind
        {
            get => (string)GetValue(ValueBindProperty);
            set => SetValue(ValueBindProperty, value);
        }

        public static void OnValueBindChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ToastProgress tp)
            {
                if (!string.IsNullOrEmpty(tp.ValueBind))
                    tp.Element.SetAttribute("value", "{" + tp.ValueBind + "}");
                else
                    tp.Element.SetAttribute("value", (tp.Value / 100.0).ToString(CultureInfo.InvariantCulture));

                tp.Page.UpdatePreview();
                tp.UpdateCanEdit();
            }
        }

        public static readonly DependencyProperty CanEditValueProperty = DependencyProperty.Register(
            "CanEditValue",
            typeof(bool),
            typeof(ToastProgress),
            new PropertyMetadata(true));

        public bool CanEditValue
        {
            get => (bool)GetValue(CanEditValueProperty);
            set => SetValue(CanEditValueProperty, value);
        }

        private void UpdateCanEdit()
        {
            CanEditValue = string.IsNullOrEmpty(ValueBind) && !Indeterminate;
        }
    }
}
