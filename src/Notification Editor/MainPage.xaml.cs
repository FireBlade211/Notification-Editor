using FireBlade.NotificationEditor.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using FireBlade.NotificationEditor.Helpers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FireBlade.NotificationEditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            Current = this;
            RequestedTheme = SettingsHelper.Current.Theme;
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;

            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
                ApplicationView.GetForCurrentView().Title = string.Empty;
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                contentFrame.CanGoBack && !(contentFrame.Content is SettingsPage)
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            contentFrame.Navigate(typeof(HomePage), null, new SuppressNavigationTransitionInfo());

            if (e.Parameter is MainPageNavigationArgs args)
            {
                if (args.Type != MainPageActivationType.Normal)
                {
                    ApplicationView.GetForCurrentView().Title = args.Arguments.MruItem?.Title ?? string.Empty;

                    contentFrame.Navigate(args.Type == MainPageActivationType.XmlTile ? typeof(TilePage) : typeof(ToastPage),
                        args.Arguments, new DrillInNavigationTransitionInfo());
                }
            }
        }
    }
}
