using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PicLoc
{
    public sealed partial class add_friend : Page
    {
        account a = new account();
        helper h = new helper();
        public add_friend()
        {
            this.InitializeComponent();
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += backButton;
        }
        private void backButton(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
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

        private async void button_add_friend_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            String loginJSON = await a.addFriend(main.static_user, main.static_pass, progress_bar, textBox_username.Text);
            JObject jo = JObject.Parse(loginJSON);

            if (jo["status"].ToString() == "True")
            {
                Frame.Navigate(typeof(snap_screen));
                h.showSingleButtonDialog("Success", "Successfully added friend [" + textBox_username.Text + "]", "Dismiss");
            }
            else
            {
                h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"] != null) ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
            }
        }
    }
}
