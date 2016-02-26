using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PicLoc
{
    sealed partial class send_snap : Page
    {
        public static BitmapImage snap_preview;
        public static StorageFile snap_file;
        public static ObservableCollection<classes.Friend> friends;
        public static ProgressBar pb;

        account a = new account();
        helper h = new helper();

        public send_snap()
        {
            this.InitializeComponent();
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += backButton;
            setImage();
            listView_friends.ItemsSource = friends;
        }

        public void setImage()
        {
            Debug.WriteLine("Setting image source");
            image_snap_preview.Source = null;
            if (snap_preview != null)
            {
                image_snap_preview.Source = snap_preview;
            }
        }

        private void backButton(object sender, BackRequestedEventArgs e)
        {  
            if (Frame.CanGoBack) {
                e.Handled = true;
                Frame.GoBack();
            }  
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested -= backButton;
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private async void button_send_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            JArray ja = new JArray();

            foreach (var a in ((ListView)listView_friends).SelectedItems)
            {
                classes.Friend friendToSendSnapTo = (classes.Friend)(a);
                ja.Add(friendToSendSnapTo.username);
            }

            String JSON = await a.sendSnap(main.static_user, main.static_pass, pb, ja.ToString(), snap_file, int.Parse(slider_time.Value.ToString()));

            //Frame.Navigate(typeof(snap_screen));

            JObject jo = JObject.Parse(JSON);

            if (jo["status"].ToString() == "True")
            {
                h.showSingleButtonDialog("Snap uploaded", "Snap uploaded successfully!", "Dismiss");
            }
            else
            {
                h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"] != null) ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
            }

        }
    }
}
