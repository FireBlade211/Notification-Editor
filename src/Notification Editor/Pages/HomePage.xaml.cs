using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel;
using System;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml.Documents;
using Windows.System;
using System.Collections.ObjectModel;
using FireBlade.NotificationEditor.Helpers;
using Windows.Storage.AccessCache;
using System.Linq;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FireBlade.NotificationEditor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        private void ToastLink_Click(object sender, RoutedEventArgs e)
        {
            ToastChooserFlyout.ShowAt(toastLink);
        }

        private void TileLink_Click(object sender, RoutedEventArgs e)
        {
            TileChooserFlyout.ShowAt(tileLink);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsWriteablePropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutItem", "Icon"))
                newItem.Icon = newItem2.Icon = new SymbolIcon(Symbol.Page2);

            var dir1 = await(await Package.Current.InstalledLocation
                .GetFolderAsync("Samples").AsTask())
                .GetFolderAsync("Tile").AsTask();

            foreach (var item in await dir1.GetItemsAsync())
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = System.IO.Path.GetFileNameWithoutExtension(item.Name),
                    Tag = item
                };

                menuItem.Click += async (s, args) =>
                {
                    var mru = new MruItem((s as MenuFlyoutItem).Text, MruItemType.Tile);

                    MRUHelper.Items.Insert(0, mru);
                    await MRUHelper.CleanItems();

                    ApplicationView.GetForCurrentView().Title = (s as MenuFlyoutItem).Text;
                    MainPage.Current.contentFrame.Navigate(typeof(TilePage), new NotificationEditorPageNavArgs()
                    {
                        Xml = await FileIO.ReadTextAsync((s as MenuFlyoutItem).Tag as StorageFile),
                        File = null,
                        MruItem = mru
                    },
                    new DrillInNavigationTransitionInfo());
                };

                templatesItem2.Items.Add(menuItem);
            }

            var dir2 = await (await Package.Current.InstalledLocation
                .GetFolderAsync("Samples").AsTask())
                .GetFolderAsync("Toast").AsTask();

            foreach (var item in await dir2.GetItemsAsync())
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = System.IO.Path.GetFileNameWithoutExtension(item.Name),
                    Tag = item
                };

                menuItem.Click += async (s, args) =>
                {
                    var mru = new MruItem((s as MenuFlyoutItem).Text, MruItemType.Toast);

                    MRUHelper.Items.Insert(0, mru);
                    await MRUHelper.CleanItems();

                    ApplicationView.GetForCurrentView().Title = (s as MenuFlyoutItem).Text;
                    MainPage.Current.contentFrame.Navigate(typeof(ToastPage), 
                        new NotificationEditorPageNavArgs()
                        {
                            Xml = await FileIO.ReadTextAsync((s as MenuFlyoutItem).Tag as StorageFile),
                            File = null,
                            MruItem = mru
                        },
                        new DrillInNavigationTransitionInfo());
                };

                templatesItem.Items.Add(menuItem);
            }

            foreach (var item in MRUHelper.Items)
                MruItems.Add(item);
        }

        private async void NewItem_Click(object sender, RoutedEventArgs e)
        {
            var mru = new MruItem("Untitled Toast", MruItemType.Toast);

            MRUHelper.Items.Insert(0, mru);
            await MRUHelper.CleanItems();

            ApplicationView.GetForCurrentView().Title = "Untitled Toast";
            MainPage.Current.contentFrame.Navigate(typeof(ToastPage), new NotificationEditorPageNavArgs()
            {
                Xml = string.Empty,
                File = null,
                MruItem = mru
            }, new DrillInNavigationTransitionInfo());
        }

        private async void NewItem2_Click(object sender, RoutedEventArgs e)
        {
            var mru = new MruItem("Untitled Tile", MruItemType.Tile);

            MRUHelper.Items.Insert(0, mru);
            await MRUHelper.CleanItems();

            ApplicationView.GetForCurrentView().Title = "Untitled Tile";
            MainPage.Current.contentFrame.Navigate(typeof(TilePage), new NotificationEditorPageNavArgs()
            {
                Xml = string.Empty,
                File = null,
                MruItem = mru
            }, new DrillInNavigationTransitionInfo());
        }

        private async Task<StorageFile> PickXmlFileWithFilePicker()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".xml");

            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.ViewMode = PickerViewMode.List;

            var file = await picker.PickSingleFileAsync();
            return file;
        }

        private async void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickXmlFileWithFilePicker();
            if (file != null)
            {
                try
                {
                    string xml = await FileIO.ReadTextAsync(file);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    var args = new NotificationEditorPageNavArgs()
                    {
                        Xml = xml,
                        File = file
                    };

                    ApplicationView.GetForCurrentView().Title = file.DisplayName;

                    if (doc.DocumentElement?.NodeName.Equals("toast") == true)
                    {
                        args.MruItem = await MRUHelper.FindOrCreateForFileAsync(file, MruItemType.Toast);
                        MainPage.Current.contentFrame.Navigate(typeof(ToastPage), args, new DrillInNavigationTransitionInfo());
                    }
                    else if (doc.DocumentElement?.NodeName.Equals("tile") == true)
                    {
                        args.MruItem = await MRUHelper.FindOrCreateForFileAsync(file, MruItemType.Tile);
                        MainPage.Current.contentFrame.Navigate(typeof(TilePage), args, new DrillInNavigationTransitionInfo());
                    }
                    else
                        throw new Exception("The type of notification this file represents could not be detected.");
                }
                catch (Exception ex)
                {
                    var dlg = new ContentDialog
                    {
                        Title   = "An error occured.",
                        Content = ex.Message,
                        PrimaryButtonText = "OK"
                    };

                    _ = await dlg.ShowAsync();

                    ApplicationView.GetForCurrentView().Title = string.Empty;
                }
            }
        }

        private async void SysImportButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the listener
            UserNotificationListener listener = UserNotificationListener.Current;

            // And request access to the user's notifications
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            if (accessStatus == UserNotificationListenerAccessStatus.Denied)
            {
                var dlg = new ContentDialog
                {
                    Title = "Access denied",
                    Content = new TextBlock
                    {
                        Inlines =
                        {
                            new Bold
                            {
                                Inlines =
                                {
                                    new Run
                                    {
                                        Text = "Notification Editor"
                                    }
                                }
                            },
                            new Run
                            {
                                Text = " cannot access your notifications. Go to "
                            },
                            new Hyperlink
                            {
                                Inlines =
                                {
                                    new Run
                                    {
                                        Text = "Windows Settings"
                                    }
                                },
                                NavigateUri = new Uri("ms-settings:privacy-notifications")
                            },
                            new Run
                            {
                                Text = " and allow Notification Editor to read your notifications, in order to use this feature."
                            }
                        }
                    },
                    PrimaryButtonText = "OK"
                };

                await dlg.ShowAsync();
            }
            else if (accessStatus == UserNotificationListenerAccessStatus.Allowed)
            {
                MainPage.Current.contentFrame.Navigate(typeof(SystemImportPage), listener, new DrillInNavigationTransitionInfo());
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.contentFrame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
        }

        private async void GithubButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/FireBlade211/Notification-Editor"));
        }

        public ObservableCollection<MruItem> MruItems { get; set; } = new ObservableCollection<MruItem>();

        private void Grid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Control ctrl)
                VisualStateManager.GoToState(ctrl, "PointerOver", true);
        }

        private void Grid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Control ctrl)
                VisualStateManager.GoToState(ctrl, "Normal", true);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement f && f.Tag is MruItem mru)
            {
                MruItems.Remove(mru);
                await mru.Remove();
            }
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MruItem mru)
            {
                var args = new NotificationEditorPageNavArgs
                {
                    MruItem = mru,
                    Xml = mru.Xml
                };

                if (!string.IsNullOrEmpty(mru.Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(mru.Token))
                    args.File = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(mru.Token);

                ApplicationView.GetForCurrentView().Title = mru.Title;

                if (mru.Type == MruItemType.Toast)
                    MainPage.Current.contentFrame.Navigate(typeof(ToastPage), args, new DrillInNavigationTransitionInfo());
                else
                    MainPage.Current.contentFrame.Navigate(typeof(TilePage), args, new DrillInNavigationTransitionInfo());
            }
        }

        private async void SaveRenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Parent is StackPanel p && p.FindName("renameBox") is TextBox renameBox &&
                element.Tag is MruItem mru)
            {
                mru.Title = renameBox.Text;

                if (p.Parent is FlyoutPresenter presenter && presenter.Parent is Popup popup)
                    popup.IsOpen = false;

                if (App.IsUniversalContractV2Present)
                {
                    if (JumpList.IsSupported())
                    {
                        var list = await JumpList.LoadCurrentAsync();
                        var found = list.Items.FirstOrDefault(x => Guid.TryParse(x.Arguments.Substring("mru:".Length), out Guid id) && id == mru.Id);

                        if (found != null)
                        {
                            found.DisplayName = mru.Title;
                        }

                        await list.SaveAsync();
                    }
                }
            }
        }
    }
}
