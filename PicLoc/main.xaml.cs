using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using Windows.Security.Credentials;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

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

        public main()
        {
            this.InitializeComponent();
            checkDeviceID();
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

        private async void checkDeviceID()
        {
            var vault = new PasswordVault();
            try
            {
                PasswordCredential cred = vault.Retrieve("device_id", "device_id");
                if (cred != null)
                {
                    // we have a device id
                    Debug.WriteLine("Device ID: " + cred.Password);
                    return;
                }
            }
            catch (Exception e)
            {

            }

            var values = new Dictionary<string, string>
            { };

            var client = new System.Net.Http.HttpClient();

            var content = new FormUrlEncodedContent(values);

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            {
                var deviceInformation = new EasClientDeviceInformation();
                string Id = deviceInformation.Id.ToString();
                client.DefaultRequestHeaders.Add("device", "Windows/" + Id);
            } else
            {
                client.DefaultRequestHeaders.Add("device", "Windows/NoIdentifierPresent");
            }

            var response = await client.PostAsync(settings.API + "/device_id", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("Login response: " + responseString);

            JArray array = JArray.Parse(responseString);

            Debug.WriteLine(array[0]["device_id"].ToString());

            var nvault = new PasswordVault();
            var ncred = new PasswordCredential("device_id", "device_id", array[0]["device_id"].ToString());
            vault.Add(ncred);
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

        public async void login(String username, String password, bool tokenUsed = false)
        {

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
            catch (Exception e)
            {

            }

            if (!tokenUsed)
            {
                password = sha512(password);
            }

            var values = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
                { "device_id", deviceid.ToString() }
            };

            var client = new System.Net.Http.HttpClient();

            client.DefaultRequestHeaders.ExpectContinue = false;

            var content = new FormUrlEncodedContent(values);

            client.DefaultRequestHeaders.Add("device", "WindowsClient/" + settings.APP_version);

            var response = await client.PostAsync(settings.API + "/login/", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("Login response: " + responseString);

            JObject array = null;

            try {

                array = JObject.Parse(responseString);
            } catch (Exception ex)
            {
                var dlg = new MessageDialog("Unexpected response from server: " + responseString);
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
                return;
            }

            Debug.WriteLine(array["status"].ToString());

            if (array["status"].ToString() == "True")
            {

                var vault1 = new PasswordVault();
                var cred1 = new PasswordCredential("PicLoc_accounts", username, array["token"].ToString());
                vault1.Add(cred1);
                Debug.WriteLine("Saved creds");

                snapscreen.json = responseString;
                Frame.Navigate(typeof(snapscreen));
                static_pass = array["token"].ToString();
                static_user = username;
                /*
                var dlg = new MessageDialog("Good login: " + responseString);
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
                */
            } else
            {
                var dlg = new MessageDialog(array["message"].ToString());
                    dlg.Title = "Something went wrong: " + array["code"];
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
            }

        }

        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void button_signup_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button_login_Click(object sender, RoutedEventArgs e)
        {

            String errorMsg = null;

            if (password.Password == "" && username.Text == "")
            {
                    Debug.WriteLine("No username or password");
                errorMsg = "No username and password";

            } else if(password.Password == "" && username.Text != ""){

                // password empty so we search for creds

                Debug.WriteLine("No password, but we have a username, try to login with token");

                var vault = new PasswordVault();
                try
                {
                    PasswordCredential cred = vault.Retrieve("PicLoc_accounts", username.Text);
                    if (cred != null)
                    {
                        // we have a token stored
                        password.Password = cred.Password;
                        Debug.WriteLine("Username: " + cred.UserName + " | Password: " + cred.Password);
                        login(username.Text, cred.Password, true); // try to login with token
                        return;
                    }
                }
                catch (COMException ex)
                {
                    Debug.WriteLine("token -- Exception handled");
                    /*if (ex.HResult != ElementNotFound)
                    {
                        throw;
                    }*/
                }

            } else if(password.Password != "" && username.Text != "")
            {
                Debug.WriteLine("loggin in normally");
                login(username.Text, password.Password); // try to login with username and passs
            } else
            {
                errorMsg = "Did you only input a password?";
                Debug.WriteLine("uh-oh");
                // should never happen
            }

            if (errorMsg != null)
            {

                var dlg = new MessageDialog(errorMsg);
                dlg.Title = "Login error";
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                dlg.ShowAsync();
            }

        }
    }
}
