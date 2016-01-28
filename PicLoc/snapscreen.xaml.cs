using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PicLoc
{
    public sealed partial class snapscreen : Page
    {

        public static string json;
        private StorageFolder image_folder;
        DispatcherTimer dt;

        // Testing
        private HttpClient httpClient;
        private CancellationTokenSource cts;


        public snapscreen()
        {

            httpClient = new HttpClient();
            cts = new CancellationTokenSource();

            this.InitializeComponent();

            // hide status bar

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var i = Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }

            // <!-- hide status bar
            createImageFolder();

            set_snaps();
            set_friends(); //rename set function

        }

        private async void createImageFolder()
        {

            try

            {

                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("images");

            }

            catch (FileNotFoundException ex)

            {

                StorageFolder new_images = await ApplicationData.Current.LocalFolder.CreateFolderAsync("images", CreationCollisionOption.ReplaceExisting);

            }

            image_folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("images");

        }

        private void set_friends()
        {

            list_friends.ItemsSource = null;

            JObject array = JObject.Parse(json);

            JObject friends = array["friends"].Value<JObject>();

            Debug.WriteLine("Friends: " + friends.Count);

            ObservableCollection<Friend> friendList = new ObservableCollection<Friend>();

            foreach (var x in friends)
            {

                Friend tmp_friend = new Friend() { username = x.Key, status = x.Value.SelectToken("status").ToString(), display_name = x.Value.SelectToken("display_name").ToString() };

                friendList.Add(tmp_friend);

                //Debug.WriteLine("Key: " + x.Key); // A.K.A the ID of the snap
                //Debug.WriteLine("Value: " + x.Value.SelectToken("user_from")); // Get value from snap
            }

            list_friends.ItemsSource = friendList;

        }

        private void set_snaps()
        {
            JObject array = JObject.Parse(json);

            JObject items = array["snaps"].Value<JObject>();

            Debug.WriteLine("Snaps: " + items.Count);

            ObservableCollection<Snap> snapList = new ObservableCollection<Snap>();

            foreach (var x in items)
            {

                Snap tmp_snap = new Snap() { id = int.Parse(x.Key), from = x.Value.SelectToken("user_from").ToString(), status = x.Value.SelectToken("status").ToString(), ImageSource = new BitmapImage(new Uri(this.BaseUri, "/Assets/snapscreen/Images/" + x.Value.SelectToken("status").ToString() + ".png")), time = int.Parse(x.Value.SelectToken("time").ToString()) };

                snapList.Add(tmp_snap);

                //Debug.WriteLine("Key: " + x.Key); // A.K.A the ID of the snap
                //Debug.WriteLine("Value: " + x.Value.SelectToken("user_from")); // Get value from snap
            }

            listView.ItemsSource = snapList;

        }

        private void ProgressHandler(HttpProgress progress)
        {
            //Debug.WriteLine("Stage: " + progress.Stage.ToString());
            //Debug.WriteLine("Retires: " + progress.Retries.ToString(CultureInfo.InvariantCulture));
            //Debug.WriteLine("Bytes Sent: " + progress.BytesSent.ToString(CultureInfo.InvariantCulture));
            //Debug.WriteLine("Bytes Received: " + progress.BytesReceived.ToString(CultureInfo.InvariantCulture));

            ulong totalBytesToSend = 0;
            if (progress.TotalBytesToSend.HasValue)
            {
                totalBytesToSend = progress.TotalBytesToSend.Value;
                //Debug.WriteLine("Total Bytes To Send: " + totalBytesToSend.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                //Debug.WriteLine("Total Bytes To Send: " + "unknown");
            }

            ulong totalBytesToReceive = 0;
            if (progress.TotalBytesToReceive.HasValue)
            {
                totalBytesToReceive = progress.TotalBytesToReceive.Value;
                //Debug.WriteLine("Total Bytes To Receive: " + totalBytesToReceive.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                //Debug.WriteLine("Total Bytes To Receive: " + "unknown");
            }

            double requestProgress = 0;
            if (progress.Stage == HttpProgressStage.SendingContent && totalBytesToSend > 0)
            {
                requestProgress = progress.BytesSent * 50 / totalBytesToSend;
            }
            else if (progress.Stage == HttpProgressStage.ReceivingContent)
            {
                // Start with 50 percent, request content was already sent.
                requestProgress += 50;

                if (totalBytesToReceive > 0)
                {
                    requestProgress += progress.BytesReceived * 50 / totalBytesToReceive;
                }
            }
            else
            {
                return;
            }
            //Debug.WriteLine("Progress: " + requestProgress);
            snap_progress.Value = requestProgress;
        }

        private async void snap_tapped(ListView lv)
        {
            var lv_item = lv.SelectedItem;
            Snap current_snap = (Snap)lv.SelectedItem;

            listView.SelectedIndex = -1;

            if (snap_progress.Value != 100)
            {
                Debug.WriteLine("Not finished downloading snap");
                return;
            }
            
            //((TextBlock)sender).IsTapEnabled = false;

            HttpResponseMessage response = null;
            String responseString = null;

            Debug.WriteLine("Get snap: " + current_snap.id);
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
            catch (Exception ex)
            {

            }

            snap_progress.Value = 0;

            try
            {
                var values = new Dictionary<string, string>
            {
                { "username", main.static_user },
                { "password", main.static_pass },
                { "device_id", deviceid.ToString() },
                { "snap_id", current_snap.id.ToString() }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);

                IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                response = await httpClient.PostAsync(new Uri(settings.API + "/get_snap/"), formContent).AsTask(cts.Token, progress);
                responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("******* DONE *******");
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Cancelled");
            } catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.StackTrace);
            } finally
            {
                snap_progress.Value = 100;
                Debug.WriteLine("* COMPLETED *");
            }


            JArray array = null;

            if (response.Content.Headers.ContentType.ToString() == "image/png")
            {
                Debug.WriteLine("Snap is good");

                // download snap

                try
                {
                    StorageFile file = await image_folder.CreateFileAsync(current_snap.id + ".jpg", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());
                }
                catch (System.Exception)
                {

                }

                // display snap
                snap_image.Visibility = Visibility.Visible;
                snap_num.Visibility = Visibility.Visible;
                snap_ellipse.Visibility = Visibility.Visible;
                listView.Visibility = Visibility.Collapsed;

                snap_image.Source = new BitmapImage(new Uri("ms-appdata:///local/images/" + current_snap.id + ".jpg", UriKind.Absolute));

                // remove from list

                Debug.WriteLine("Snap removal location");

                ObservableCollection<Snap> snapList = new ObservableCollection<Snap>();
                snapList = (ObservableCollection<Snap>)listView.ItemsSource;
                listView.ItemsSource = null;

                foreach (Snap s in snapList)
                {
                    if (s == current_snap)
                    {
                        int ind = snapList.IndexOf(s);
                        Debug.WriteLine("snap to remove: " + ind);
                        snapList.RemoveAt(ind);
                        break;
                        //snapList.RemoveAt(snapList.IndexOf(s));
                    }
                }

                //listView.Items.Clear();
                listView.ItemsSource = snapList;

                //listView.Items.RemoveAt(current_snap_location);


                // start timer

                dt = new DispatcherTimer();
                dt.Tick += dt_tick;
                dt.Interval = new TimeSpan(0, 0, 1);
                dt.Start();

                snap_num.Text = current_snap.time.ToString();


            }
            else
            {

                Debug.WriteLine("snap error: " + responseString);

                try
                {
                    Debug.WriteLine("trying to parse message");
                    array = JArray.Parse(responseString);
                    Debug.WriteLine("parsing success");
                }
                catch (Exception ex)
                {
                    var dlg1 = new MessageDialog("Fatal error: " + responseString);
                    dlg1.Commands.Add(new UICommand("Dismiss", null, "1"));
                    var op1 = await dlg1.ShowAsync();
                    return;
                }

                String message;

                // TODO: fix array[0] stuff

                try
                {
                    message = array[0]["message"].ToString();
                } catch (Exception ex)
                {
                    message = "No error message";
                }

                var dlg = new MessageDialog(message);
                dlg.Title = "Something went wrong: " + array[0]["code"];
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
                return;
            }
        }

        private void dt_tick(object sender, object e)
        {
            snap_num.Text = (int.Parse(snap_num.Text) - 1).ToString();
            if (int.Parse(snap_num.Text) == -1)
            {
                dt.Stop();
                restSnapView();
            }
        }

        private void image_solid_square_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            pivot_snap.SelectedIndex = 0;
        }

        private void restSnapView()
        {
            dt.Stop();
            snap_image.Visibility = Visibility.Collapsed;
            snap_num.Visibility = Visibility.Collapsed;
            snap_ellipse.Visibility = Visibility.Collapsed;
            listView.Visibility = Visibility.Visible;
        }

        private void snap_image_Tapped(object sender, TappedRoutedEventArgs e)
         { 
             Debug.WriteLine("tapped"); 
             restSnapView(); 
         }


    private void listView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine(listView.SelectedIndex);
            Debug.WriteLine("Snap ID Selected: " + ((Snap)listView.SelectedItem).id);
            Debug.WriteLine("Object Type: " + sender.GetType());
            snap_tapped((ListView)sender);
        }

        private void image_list_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pivot_snap.SelectedIndex = 2;
        }

        private async void getSnaps()
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

            var values = new Dictionary<string, string>
            {
                { "username", main.static_user },
                { "password", main.static_pass },
                { "device_id", deviceid.ToString() }
            };

            var client = new HttpClient();

            var content = new HttpFormUrlEncodedContent(values);

            client.DefaultRequestHeaders.Add("device", "WindowsClient/" + settings.APP_version);

            var response = await client.PostAsync(new Uri(settings.API + "/get_snaps/"), content);

            var responseString = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("getSnaps response: " + responseString);

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

            Debug.WriteLine("getSnaps: " + array["status"].ToString());

            if (array["status"].ToString() == "True")
            {
                json = responseString;
                listView.ItemsSource = null;
                set_snaps();
            }
            else
            {
                var dlg = new MessageDialog(array["message"].ToString());
                dlg.Title = "Something went wrong: " + array["code"];
                dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                var op = await dlg.ShowAsync();
            }
        }

        private async void image_camera_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (snap_progress.Value != 100)
            {
                Debug.WriteLine("Not finished downloading snap");
                return;
            }

            HttpResponseMessage response = null;
            String responseString = null;

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
            catch (Exception ex)
            {

            }

            snap_progress.Value = 0;


            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();

            IInputStream inputStream = await file.OpenAsync(FileAccessMode.Read);


            if (file != null)
            {

                try
                {
                    HttpMultipartFormDataContent multipartContent = new HttpMultipartFormDataContent();


                    multipartContent.Add(
                        new HttpStreamContent(inputStream),
                        "snap",
                        file.Name);

                    multipartContent.Add(new HttpStringContent(main.static_user), "username");
                    multipartContent.Add(new HttpStringContent(main.static_pass), "password");
                    multipartContent.Add(new HttpStringContent(deviceid), "device_id");
                    multipartContent.Add(new HttpStringContent("test"), "send_snap_to");
                    multipartContent.Add(new HttpStringContent("10"), "send_snap_time");

                    IProgress<HttpProgress> progress = new Progress<HttpProgress>(ProgressHandler);
                    response = await httpClient.PostAsync(new Uri(settings.API + "/send_snap/"), multipartContent).AsTask(cts.Token, progress);
                    responseString = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Send snap response: " + responseString);
                    Debug.WriteLine("Send snap code: " + response.StatusCode.ToString());
                    Debug.WriteLine("******* DONE *******");
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("Cancelled");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.StackTrace);
                }
                finally
                {
                    snap_progress.Value = 100;
                    Debug.WriteLine("* COMPLETED *");
                }

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

                Debug.WriteLine("send_snap: " + array["status"].ToString());

                if (array["status"].ToString() == "True")
                {
                    listView.ItemsSource = null;
                    getSnaps();
                }
                else
                {
                    Debug.WriteLine("error of some sorts");
                    var dlg = new MessageDialog(array["message"].ToString());
                    dlg.Title = "Something went wrong: " + array["code"];
                    dlg.Commands.Add(new UICommand("Dismiss", null, "1"));
                    var op = await dlg.ShowAsync();
                }

            } else {

                // file is null

            }

        }
    }

    class Snap
    {
        public int id { get; set; }
        public string from { get; set; }
        public string status { get; set; }
        public BitmapImage ImageSource { get; set; }
        public int time { get; set; }
    }

    class Friend
    {
        public string username { get; set; }
        public string status { get; set; }
        public string display_name { get; set; }
    }

}