using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Data.Xml.Dom;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FireBlade.NotificationEditor.DataModel
{
    public class AdaptiveSubgroup : DependencyObject
    {
        public string Name { get; set; } = string.Empty;
        public AdaptiveGroup Group { get; set; }

        public XmlElement Element { get; set; }
        public ObservableCollection<AdaptiveElement> Elements { get; set; } = new ObservableCollection<AdaptiveElement>();

        public INotificationEditorPage Page { get; set; }

        public static readonly DependencyProperty WeightProperty = DependencyProperty.Register(
            "Weight",
            typeof(int),
            typeof(AdaptiveSubgroup),
            new PropertyMetadata(-1, OnWeightChanged));

        public int Weight
        {
            get => (int)GetValue(WeightProperty);
            set => SetValue(WeightProperty, value);
        }

        private static void OnWeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveSubgroup sub)
            {
                if (sub.Weight != -1)
                    sub.Element.SetAttribute("hint-weight", sub.Weight.ToString(CultureInfo.InvariantCulture));
                else
                    sub.Element.RemoveAttribute("hint-weight");

                sub.Page.UpdatePreview();
            }
        }

        public static readonly DependencyProperty TextStackingProperty = DependencyProperty.Register(
            "TextStacking",
            typeof(string),
            typeof(AdaptiveSubgroup),
            new PropertyMetadata(string.Empty, OnTextStackingChanged));

        public string TextStacking
        {
            get => (string)GetValue(TextStackingProperty);
            set => SetValue(TextStackingProperty, value);
        }

        private static void OnTextStackingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AdaptiveSubgroup sub)
            {
                if (!string.IsNullOrEmpty(sub.TextStacking))
                    sub.Element.SetAttribute("hint-textStacking", sub.TextStacking);
                else
                    sub.Element.RemoveAttribute("hint-textStacking");

                sub.Page.UpdatePreview();
            }
        }
    }
}
