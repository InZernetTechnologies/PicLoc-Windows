using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PicLoc
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class signup : Page
    {
        public signup()
        {
            this.InitializeComponent();
            //SetUpPageAnimation();
        }

        protected virtual void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new EntranceNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }

        public string sha512(String password)
        {
            // Convert the message string to binary data.
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);

            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);

            // Demonstrate how to retrieve the name of the hashing algorithm.
            String strAlgNameUsed = objAlgProv.AlgorithmName;

            // Hash the message.
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);

            // Verify that the hash length equals the length specified for the algorithm.
            if (buffHash.Length != objAlgProv.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }

            // Convert the hash to a string (for display).
            String strHashBase64 = CryptographicBuffer.EncodeToBase64String(buffHash);

            // Return the encoded string
            return strHashBase64;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();
            // Make the button visible
            if (Frame.CanGoBack)
            {
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
            // Unregister event
            currentView.BackRequested -= backButton;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();
            // Make the button visible
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            // Register event
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

        private async void signup_button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            String code = "";
            String message = "";

            Debug.WriteLine("Gender index: " + gender.SelectedIndex);

            if (username.Text == "")
            {
                code = "pNU";
                message = "No username";
            }

            if (gender.SelectedIndex == -1)
            {
                code = "pNG";
                message = "No gender selected";
            }

            if (password.Password == "")
            {
                code = "pNP";
                message = "No password";
            }

            if (password_repeat.Password == "")
            {
                code = "pNP";
                message = "No repeated password";
            }

            if (password.Password != password_repeat.Password)
            {
                code = "pPNM";
                message = "Passwords not matching";
            }

            if (code != "")
            {
                var dlg = new MessageDialog(message);
                dlg.Title = "Application code: " + code;
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
                return;
            }


            var vault = new PasswordVault();
            PasswordCredential cred;
            String deviceid = null;
            try
            {
                cred = vault.Retrieve("device_id", "device_id");
                if (cred != null)
                {
                    // we have a device id
                    Debug.WriteLine("Device ID for login: " + cred.Password);
                    deviceid = cred.Password;
                }
            }
            catch (Exception exce)
            {

            }

            String _password = sha512(password.Password.ToString());

            String year = birthdate.Date.Year.ToString();
            String month = birthdate.Date.Month.ToString().PadLeft(2, '0');
            String day = birthdate.Date.Day.ToString().PadLeft(2, '0');
            String formatted_birthdate = year + "-" + month + "-" + day;

            var values = new Dictionary<string, string>
            {
                { "username", username.Text },
                { "email", email.Text },
                { "password", _password },
                { "gender", ((ComboBoxItem)gender.SelectedItem).Content.ToString() },
                { "device_id", deviceid.ToString() },
                { "birthdate",formatted_birthdate }
            };

            Debug.WriteLine("birthdate: " + formatted_birthdate);

            var client = new System.Net.Http.HttpClient();

            client.DefaultRequestHeaders.ExpectContinue = false;

            var content = new FormUrlEncodedContent(values);

            client.DefaultRequestHeaders.Add("device", "WindowsClient/" + settings.APP_version);

            var response = await client.PostAsync(settings.API + "/register/", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("Login response: " + responseString);

            JObject array = null;

            try
            {
                array = JObject.Parse(responseString);
            }
            catch (Exception ex)
            {
                var dlg = new MessageDialog("Unexpected response from server: " + responseString);
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
                return;
            }

            Debug.WriteLine(array["status"].ToString());

            if (array["status"].ToString() == "True")
            {

                var dlg = new MessageDialog("Your account '" + username.Text + "' has been registered! Enjoy :)");
                dlg.Title = "Success!";
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
                Frame.GoBack();
            }
            else
            {
                var dlg = new MessageDialog(array["message"].ToString());
                dlg.Title = "Something went wrong: " + array["code"];
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
            }

        }
    }

}
