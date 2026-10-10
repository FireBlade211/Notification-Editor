using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Data.Xml.Dom;
using System.Globalization;
using Windows.UI.Xaml.Controls;

namespace FireBlade.NotificationEditor.DataModel
{
    public class AdaptiveText : AdaptiveElement
    {
        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "Id",
            typeof(int),
            typeof(AdaptiveText),
            new PropertyMetadata(1, OnIdChanged));

        public static void OnIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                at.Element.SetAttribute("id", at.Id.ToString(CultureInfo.InvariantCulture));
                at.Page.UpdateXml();
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
            typeof(AdaptiveText),
            new PropertyMetadata(string.Empty, OnTextChanged));

        public static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                try
                {
                    at.Element.InnerText = at.Text;
                    at.Page.UpdatePreview();
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

        public string Language
        {
            get => (string)GetValue(LanguageProperty);
            set => SetValue(LanguageProperty, value);
        }

        public static readonly DependencyProperty LanguageProperty = DependencyProperty.Register(
            "Language",
            typeof(string),
            typeof(AdaptiveText),
            new PropertyMetadata(string.Empty, OnLangChanged));

        public static void OnLangChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                if (!string.IsNullOrEmpty(at.Language))
                    at.Element.SetAttribute("lang", at.Language);
                else
                    at.Element.RemoveAttribute("lang");

                at.Page.UpdatePreview();
            }
        }

        public string Style
        {
            get => (string)GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        public static readonly DependencyProperty StyleProperty = DependencyProperty.Register(
            "Style",
            typeof(string),
            typeof(AdaptiveText),
            new PropertyMetadata(string.Empty, OnStyleChanged));

        public static void OnStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                if (!string.IsNullOrEmpty(at.Style))
                    at.Element.SetAttribute("hint-style", at.Style);
                else
                    at.Element.RemoveAttribute("hint-style");

                at.Page.UpdatePreview();
            }
        }

        public bool Wrap
        {
            get => (bool)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
        }

        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register(
            "Wrap",
            typeof(bool),
            typeof(AdaptiveText),
            new PropertyMetadata(false, OnWrapChanged));

        public static void OnWrapChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                if (at.Wrap)
                    at.Element.SetAttribute("hint-wrap", "true");
                else
                    at.Element.RemoveAttribute("hint-wrap");

                at.Page.UpdatePreview();
            }
        }

        public int MinLines
        {
            get => (int)GetValue(MinLinesProperty);
            set => SetValue(MinLinesProperty, value);
        }

        public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register(
            "MinLines",
            typeof(int),
            typeof(AdaptiveText),
            new PropertyMetadata(-1, OnMinLinesChanged));

        public static void OnMinLinesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                if (at.MinLines != -1)
                    at.Element.SetAttribute("hint-minLines", at.MinLines.ToString(CultureInfo.InvariantCulture));
                else
                    at.Element.RemoveAttribute("hint-minLines");

                at.Page.UpdatePreview();
            }
        }

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
            "MaxLines",
            typeof(int),
            typeof(AdaptiveText),
            new PropertyMetadata(-1, OnMaxLinesChanged));

        public static void OnMaxLinesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                if (at.MaxLines != -1)
                    at.Element.SetAttribute("hint-maxLines", at.MaxLines.ToString(CultureInfo.InvariantCulture));
                else
                    at.Element.RemoveAttribute("hint-maxLines");

                at.Page.UpdatePreview();
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
            typeof(AdaptiveText),
            new PropertyMetadata(string.Empty, OnAlignChanged));

        public static void OnAlignChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveText at)
            {
                if (!string.IsNullOrEmpty(at.Align))
                    at.Element.SetAttribute("hint-align", at.Align);
                else
                    at.Element.RemoveAttribute("hint-align");

                at.Page.UpdatePreview();
            }
        }
    }
}
