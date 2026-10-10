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
using Windows.UI.Notifications.Management;
using System.Collections.ObjectModel;
using Windows.UI.Notifications;
using NotificationsVisualizerLibrary;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
// NOTE: Halfway through making this I realized that User Notification Listener doesn't give the raw XML so pretend this doesn't exist


namespace FireBlade.NotificationEditor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SystemImportPage : Page
    {
        public ObservableCollection<UserNotification> ToastNotifications { get; set; }

        public SystemImportPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is UserNotificationListener listener)
            {
                var toasts = await listener.GetNotificationsAsync(NotificationKinds.Toast);
                
                foreach (var toast in toasts)
                {
                    var preview = new PreviewToast();
                    preview.IsHitTestVisible = false;
                    //preview.Initialize(toast.)

                    var item = new ToastImportItem
                    {
                        Notification = toast
                    };
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.contentFrame.GoBack();
        }
    }

    public class ToastImportItem
    {
        public PreviewToast Preview { get; set; }
        public UserNotification Notification { get; set; }
    }
}
