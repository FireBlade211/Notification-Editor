using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace FireBlade.NotificationEditor.Controls
{
    /// <summary>
    /// A Slider that has a CommitedValue property that only updates on release.
    /// </summary>
    /// <remarks>Used because UpdateSourceTrigger doesn't exist in 10240.</remarks>
    public sealed class CommitSlider : Slider
    {
        public double CommittedValue
        {
            get => (double)GetValue(CommittedValueProperty);
            set => SetValue(CommittedValueProperty, value);
        }

        public static readonly DependencyProperty CommittedValueProperty =
            DependencyProperty.Register(
                nameof(CommittedValue),
                typeof(double),
                typeof(CommitSlider),
                new PropertyMetadata(0.0, OnCommitedValueChanged));

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            CommittedValue = Value;
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            CommittedValue = Value;
        }

        protected override void OnPreviewKeyUp(KeyRoutedEventArgs e)
        {
            // make it not get captured by Slider
            base.OnPreviewKeyUp(e);
            CommittedValue = Value;
        }

        private static void OnCommitedValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is CommitSlider slider)
                if (slider.Value != slider.CommittedValue)
                    slider.Value = slider.CommittedValue;
        }
    }
}
