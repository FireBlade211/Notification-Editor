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
using Windows.Foundation.Metadata;
using FireBlade.NotificationEditor.Helpers;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FireBlade.NotificationEditor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                rootGrid.BackgroundTransition = new BrushTransition();

            versionBlock.Text = $"Version {Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}";
            useAcrylicToggle.IsEnabled = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5);
            revealButtonBox.IsEnabled = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5);

            InitControls();
            _ = InitImages();
        }

        private void InitControls()
        {
            _lockUpdatingControls = true;

            switch (SettingsHelper.Current.Theme)
            {
                case ElementTheme.Default:
                    systemThemeRadio.IsChecked = true;
                    break;
                case ElementTheme.Light:
                    lightThemeRadio.IsChecked = true;
                    break;
                case ElementTheme.Dark:
                    darkThemeRadio.IsChecked = true;
                    break;
            }

            previewDisplayNameBox.Text = SettingsHelper.Current.PreviewSettings.AppDisplayName;
            logoColorBox.Color = SettingsHelper.Current.PreviewSettings.LogoBackground;
            tileColorBox.Color = SettingsHelper.Current.PreviewSettings.LiveTileBackground;

            showNameOnSmallLogoCheck.IsChecked = SettingsHelper.Current.PreviewSettings.ShowNameOnSquare150x150Logo;
            showNameOnWideLogoCheck.IsChecked  = SettingsHelper.Current.PreviewSettings.ShowNameOnWide310x150Logo;
            showNameOnLargeLogoCheck.IsChecked = SettingsHelper.Current.PreviewSettings.ShowNameOnSquare310x310Logo;

            useAcrylicToggle.IsOn = SettingsHelper.Current.PreviewSettings.ShowAcrylicPaneBackground;
            revealButtonBox.IsChecked = SettingsHelper.Current.UseRevealButtons;

            _lockUpdatingControls = false;
        }

        private async Task InitImages()
        {
            imgLogo310x310.Source = await GetImageAsync(SettingsHelper.Current.PreviewSettings.Square310x310Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/LargeTile.png"));

            imgLogo310x150.Source = await GetImageAsync(SettingsHelper.Current.PreviewSettings.Wide310x150Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/Wide310x150Logo.png"));

            imgLogo150x150.Source = await GetImageAsync(SettingsHelper.Current.PreviewSettings.Square150x150Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.png"));

            imgLogo71x71.Source = await GetImageAsync(SettingsHelper.Current.PreviewSettings.Square71x71Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/SmallTile.png"));

            imgLogo44x44.Source = await GetImageAsync(SettingsHelper.Current.PreviewSettings.Square44x44Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/Square44x44Logo.png"));
        }

        public static async Task<BitmapImage> GetImageAsync(string token)
        {
            if (string.IsNullOrEmpty(token) || !StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
                return null;

            try
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);

                if (file != null)
                {
                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        return bitmap;
                    }
                }
            }
            catch { }

            return null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.contentFrame.GoBack();
        }

        private void SystemThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            var theme = systemThemeRadio.IsChecked == true
                ? ElementTheme.Default
                : lightThemeRadio.IsChecked == true
                ? ElementTheme.Light
                : ElementTheme.Dark;

            MainPage.Current.RequestedTheme = theme;
            SettingsHelper.Current.Theme = theme;
        }

        private async void EditFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("config.json", CreationCollisionOption.OpenIfExists);
            await SettingsHelper.Current.Flush();

            await Launcher.LaunchFileAsync(file, new LauncherOptions
            {
                DisplayApplicationPicker = true
            });

            var dlg = new ContentDialog
            {
                Title = "Editing settings file",
                Content = "Once you're done editing, press the Reload button to reload the file, or press Cancel to continue without reloading.",
                PrimaryButtonText = "Reload",
                SecondaryButtonText = "Cancel"
            };

            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await SettingsHelper.InitAsync(true);

                    InitControls();
                    await InitImages();
                }
                catch (Exception ex)
                {
                    dlg = new ContentDialog
                    {
                        Title = "Error",
                        Content = ex.Message,
                        PrimaryButtonText = "OK"
                    };

                    await dlg.ShowAsync();
                }
            }
        }

        private void TileColorBox_ColorChanged(object sender, EventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.LiveTileBackground = tileColorBox.Color;
        }

        private void LogoColorBox_ColorChanged(object sender, EventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.LogoBackground = logoColorBox.Color;
        }

        private void PreviewDisplayNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.AppDisplayName = previewDisplayNameBox.Text;
        }

        private async void ResetLogoDefault_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                switch (element.Tag.ToString().ToLowerInvariant())
                {
                    case "310x310":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square310x310Logo) &&
    StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square310x310Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square310x310Logo);

                        SettingsHelper.Current.PreviewSettings.Square310x310Logo = string.Empty;
                        break;
                    case "310x150":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Wide310x150Logo) &&
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Wide310x150Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Wide310x150Logo);

                        SettingsHelper.Current.PreviewSettings.Wide310x150Logo = string.Empty;
                        break;
                    case "150x150":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square150x150Logo) &&
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square150x150Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square150x150Logo);

                        SettingsHelper.Current.PreviewSettings.Square150x150Logo = string.Empty;
                        break;
                    case "71x71":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square71x71Logo) &&
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square71x71Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square71x71Logo);

                        SettingsHelper.Current.PreviewSettings.Square71x71Logo = string.Empty;
                        break;
                    case "44x44":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square44x44Logo) &&
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square44x44Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square44x44Logo);

                        SettingsHelper.Current.PreviewSettings.Square44x44Logo = string.Empty;
                        break;
                }

                await InitImages();
            }
        }

        private async void PickCustomLogo_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;

            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".tiff");
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".ico");

            var file = await picker.PickSingleFileAsync();
            if (file != null && sender is FrameworkElement element)
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(file, element.Tag.ToString());

                // Revoke the old tokens to prevent them from piling up.
                switch (element.Tag.ToString().ToLowerInvariant())
                {
                    case "310x310":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square310x310Logo) &&
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square310x310Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square310x310Logo);

                        SettingsHelper.Current.PreviewSettings.Square310x310Logo = token;
                        break;
                    case "310x150":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Wide310x150Logo) && 
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Wide310x150Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Wide310x150Logo);

                        SettingsHelper.Current.PreviewSettings.Wide310x150Logo = token;
                        break;
                    case "150x150":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square150x150Logo) && 
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square150x150Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square150x150Logo);

                        SettingsHelper.Current.PreviewSettings.Square150x150Logo = token;
                        break;
                    case "71x71":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square71x71Logo) && 
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square71x71Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square71x71Logo);

                        SettingsHelper.Current.PreviewSettings.Square71x71Logo = token;
                        break;
                    case "44x44":
                        if (!string.IsNullOrEmpty(SettingsHelper.Current.PreviewSettings.Square44x44Logo) && 
                            StorageApplicationPermissions.FutureAccessList.ContainsItem(SettingsHelper.Current.PreviewSettings.Square44x44Logo))
                            StorageApplicationPermissions.FutureAccessList.Remove(SettingsHelper.Current.PreviewSettings.Square44x44Logo);

                        SettingsHelper.Current.PreviewSettings.Square44x44Logo = token;
                        break;
                }

                await InitImages();
            }
        }

        private void ShowNameOnLargeLogoCheck_Checked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.ShowNameOnSquare310x310Logo = showNameOnLargeLogoCheck.IsChecked == true;
        }

        private void ShowNameOnWideLogoCheck_Checked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.ShowNameOnWide310x150Logo = showNameOnWideLogoCheck.IsChecked == true;
        }

        private void ShowNameOnSmallLogoCheck_Checked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.ShowNameOnSquare150x150Logo = showNameOnSmallLogoCheck.IsChecked == true;
        }

        private void useAcrylicToggle_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Current.PreviewSettings.ShowAcrylicPaneBackground = useAcrylicToggle.IsOn;
        }

        private bool _lockUpdatingControls = false;

        private void revealButtonBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_lockUpdatingControls) return;

            SettingsHelper.Current.UseRevealButtons = revealButtonBox.IsChecked == true;
            revealButtonsRestartPanel.Visibility = Visibility.Visible;
        }

        private async void revealButtonsRestartButton_Click(object sender, RoutedEventArgs e)
        {
            // Note: The restart prompt is only visible on 1709+ anyway, so we can use this
            await CoreApplication.RequestRestartAsync(string.Empty);
        }
    }
}
