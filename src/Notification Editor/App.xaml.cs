using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Notifications;
using Windows.System.Profile;
using System.Text;
using FireBlade.NotificationEditor.Helpers;
using Windows.Foundation.Metadata;
using Windows.Storage.AccessCache;

namespace FireBlade.NotificationEditor
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static readonly bool IsUniversalContractV2Present = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2);

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            await SettingsHelper.InitAsync();
            await MRUHelper.InitAsync();

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            MainPageNavigationArgs navArgs = null;

            if (e.Arguments.StartsWith("mru:") && Guid.TryParse(e.Arguments.Substring("mru:".Length), out Guid id) &&
                MRUHelper.TryGetItemById(id, out MruItem item))
            {
                StorageFile file = null;

                if (!string.IsNullOrEmpty(item.Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(item.Token))
                    file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(item.Token);

                navArgs = new MainPageNavigationArgs(new NotificationEditorPageNavArgs
                {
                    MruItem = item,
                    File = file,
                    Xml = item.Xml
                }, item.Type == MruItemType.Toast);
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    rootFrame.Navigate(typeof(MainPage), navArgs);
                }
                else if (navArgs != null)
                    rootFrame.Navigate(typeof(MainPage), navArgs);

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        protected async override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            base.OnShareTargetActivated(args);
            args.ShareOperation.ReportStarted();

            if (args.ShareOperation.Data.AvailableFormats.Contains(StandardDataFormats.Text))
                await ReceiveShareData(await args.ShareOperation.Data.GetTextAsync(), args);
            else if (args.ShareOperation.Data.AvailableFormats.Contains(StandardDataFormats.StorageItems))
            {
                var items = (await args.ShareOperation.Data.GetStorageItemsAsync()).OfType<StorageFile>();
                if (items.Count() == 1)
                {
                    string xml = await FileIO.ReadTextAsync(items.FirstOrDefault());
                    await ReceiveShareData(xml, args, items.FirstOrDefault());
                }
                else
                    args.ShareOperation.ReportError("Only one file can be provided.");
            }
            else
                args.ShareOperation.ReportError("Invalid data formats were received.");
        }

        private MruItem CreateMruItem(string title, MruItemType type)
        {
            var item = new MruItem(title, type);
            MRUHelper.Items.Insert(0, item);

            return item;
        }

        private async System.Threading.Tasks.Task ReceiveShareData(string xml, ShareTargetActivatedEventArgs args, StorageFile file = null)
        {
            args.ShareOperation.ReportDataRetrieved();
            try
            {
                await SettingsHelper.InitAsync();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml, new XmlLoadSettings
                {
                    ValidateOnParse = true
                });

                bool toast = false;

                if (doc.DocumentElement.NodeName.Equals("toast", StringComparison.OrdinalIgnoreCase))
                    toast = true;
                else if (doc.DocumentElement.NodeName.Equals("tile", StringComparison.OrdinalIgnoreCase))
                    toast = false;

                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    var a = new NotificationEditorPageNavArgs
                    {
                        Xml = xml
                    };

                    if (file != null)
                        a.MruItem = await MRUHelper.FindOrCreateForFileAsync(file, toast ? MruItemType.Toast : MruItemType.Tile);
                    else
                        a.MruItem = MRUHelper.Items.FirstOrDefault(x => x.Xml.Equals(xml, StringComparison.OrdinalIgnoreCase)) // this will prob be really slow
                            ?? CreateMruItem((toast ? "Toast" : "Tile") + " Received via Share", toast ? MruItemType.Toast : MruItemType.Tile);

                    rootFrame.Navigate(typeof(MainPage), new MainPageNavigationArgs(a, toast));
                }

                // Ensure the current window is active
                Window.Current.Activate();
                args.ShareOperation.ReportCompleted();
            }
            catch (Exception ex)
            {
                args.ShareOperation.ReportError(ex.Message);
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind == ActivationKind.ToastNotification)
            {
                OnToastActivated((ToastNotificationActivatedEventArgs)args);
            }
        }

        private async void OnToastActivated(ToastNotificationActivatedEventArgs args)
        {
            await SettingsHelper.InitAsync();

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage));
            }
            // Ensure the current window is active
            Window.Current.Activate();

            if (!string.IsNullOrEmpty(args.Argument))
            {
                var dlg = new MessageDialog("Arguments received:\r\n\r\n" + args.Argument, "Toast: Foreground Activated");
                _ = dlg.ShowAsync();
            }
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            var details = args.TaskInstance.TriggerDetails;

            if (details is ToastNotificationActionTriggerDetail toastDetails)
            {
                string xml = string.Empty;

                ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);

                int build = (int)((version >> 16) & 0xFFFF);

                if (build >= 14393)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var kvp in toastDetails.UserInput)
                    {
                        // row
                        sb.AppendLine("<group>");

                        // column
                        sb.AppendLine("<subgroup>");

                        sb.AppendLine($"<text>{EscapeXml(kvp.Key)}</text>");

                        sb.AppendLine("</subgroup>");

                        sb.AppendLine("</group>");
                    }

                    xml = $@"
<toast>
    <visual>
        <binding template=""toastGeneric"">
            <text>Background activated</text>
            <text placement=""attribution"">Argument: {EscapeXml(toastDetails.Argument)}</text>
            
            {sb.ToString()}
        </binding>
    </visual>
</toast>";
                }
                else
                {
                    // Before 1607, we can't use Adaptive on toasts, which sadly limits us
                    // to 2 user inputs shown (because there's a max of 3, and the title
                    // already takes one)

                    StringBuilder sb = new StringBuilder();

                    foreach (var kvp in toastDetails.UserInput)
                    {
                        // if we get more than 2, Windows will drop the later ones
                        sb.AppendLine($"<text>{EscapeXml(kvp.Key)}: {EscapeXml(kvp.Value.ToString())}</text>");
                    }

                    xml = $@"
<toast>
    <visual>
        <binding template=""toastGeneric"">
            <text>Background activated</text>
            <text placement=""attribution"">Argument: {EscapeXml(toastDetails.Argument)}</text>
            
            {sb.ToString()}
        </binding>
    </visual>
</toast>";
                }

                var doc = new XmlDocument();
                doc.LoadXml(xml);

                ToastNotification notification   = new ToastNotification(doc);
                ToastNotifier     notifier       = ToastNotificationManager.CreateToastNotifier();

                notifier.Show(notification);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity

            await SettingsHelper.Current.Flush();
            await MRUHelper.FlushAsync();

            deferral.Complete();
        }
    }

    public class MainPageNavigationArgs
    {
        public NotificationEditorPageNavArgs Arguments { get; set; }
        public MainPageActivationType Type { get; set; } = MainPageActivationType.Normal;

        public MainPageNavigationArgs(NotificationEditorPageNavArgs args)
        {
            Arguments = args;
        }

        public MainPageNavigationArgs(NotificationEditorPageNavArgs args, bool toast)
        {
            Arguments = args;
            Type = toast ? MainPageActivationType.XmlToast : MainPageActivationType.XmlTile;
        }
    }

    public enum MainPageActivationType
    {
        Normal,
        XmlToast,
        XmlTile
    }
}
