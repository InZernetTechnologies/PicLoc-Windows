using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PicLoc
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class main : Page
    {

        public static String static_user;
        public static String static_pass;
        helper h = new helper();
        account a = new account();

        private String currentPage;

        public main()
        {
            this.InitializeComponent();
            if (h.isStatusBarShowing())
            {
                h.hideStatusBar();
            }
            startup();
            currentPage = "signin";
        }

        private async void startup()
        {
            if (h.getDeviceID() == "")
            {
                String didJSON = await a.device_id(progress_bar);
                JObject jo = JObject.Parse(didJSON);
                if (jo["status"].ToString() == "True")
                {
                    h.setDeviceID(jo["device_id"].ToString());
                } else
                {
                    // failure
                }
            }
        }

        private async void button_signin_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (currentPage == "signin")
            {
                Debug.WriteLine("main | Start signin");
                String loginJSON = await a.login(textBox_username.Text, passwordBox_password.Password, progress_bar, false);
                JObject jo = JObject.Parse(loginJSON);

                if (jo["status"].ToString() == "True")
                {

                    snapscreen.json = loginJSON;

                    static_pass = jo["token"].ToString();
                    static_user = textBox_username.Text;

                    snapscreen.fromLogin = true;

                    Frame.Navigate(typeof(snapscreen));
                }
                else
                {
                    h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"].ToString() != "") ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
                }
            } else
            {
                main_signup.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                main_signin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                currentPage = "signin";
            }
        }

        private async void button_signup_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (currentPage == "signup")
            {
                Debug.WriteLine("main | Clicked signup");
                String loginJSON = await a.signup(textBox_register_username.Text, textBox_register_email.Text, passwordBox_register_password.Password, comboBox_register_gender, datePicker_register_birthdate, progress_bar);
                JObject jo = JObject.Parse(loginJSON);

                if (jo["status"].ToString() == "True")
                {
                    h.showSingleButtonDialog("Register Success", "Registering [" + textBox_register_username.Text + "] successful", "Close");
                }
                else
                {
                    h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"].ToString() != "") ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
                }
            }
            else
            {
                main_signin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                main_signup.Visibility = Windows.UI.Xaml.Visibility.Visible;
                currentPage = "signup";
            }
        }
    }
}