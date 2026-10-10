using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NotificationsVisualizerLibrary.Model;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NotificationsVisualizerLibrary.Controls
{
    public sealed partial class ProgressView : UserControl
    {
        public ProgressView()
        {
            this.InitializeComponent();
            Loaded += ProgressView_Loaded;
        }

        private void ProgressView_Loaded(object sender, RoutedEventArgs e)
        {
            //var progress = DataContext as AdaptiveProgress;

            //if (progress.BindingValue?.Equals("indeterminate", StringComparison.OrdinalIgnoreCase) == true)
            //    valueBlock.Visibility = !string.IsNullOrEmpty(progress.ValueStringOverride) ? Visibility.Visible : Visibility.Collapsed;
            //else
            //    valueBlock.Visibility = Visibility.Visible;
        }
    }
}
