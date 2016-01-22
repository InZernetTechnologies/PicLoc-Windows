using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Popups;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using System.Globalization;
using Windows.Web.Http;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            set(); //rename set function

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

        private void set()
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

            // OLD CODE //

            /*for (int i = 0; i < items.Count; i++)
            {
                Debug.WriteLine("SnapID: " + items[i]["id"]);

                img = new Image();
                img.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/snapscreen/Images/" + items[i]["status"] + ".png"));
                img.HorizontalAlignment = HorizontalAlignment.Center;
                img.VerticalAlignment = VerticalAlignment.Center;
                img.Width = 30;
                img.Height = 30;

                username = new TextBlock();
                username.Name = items[i]["id"].ToString();
                username.Tapped += snap_tapped;
                username.Text = items[i]["user_from"].ToString();
                username.FontSize = 24;

                txt = new TextBlock();
                txt.Text = "Tap to reply";
                txt.FontSize = 14;

                var snap_grid_item = new Grid();

                RowDefinition r0 = new RowDefinition();
                r0.Height = new GridLength(0, GridUnitType.Auto);
                RowDefinition r1 = new RowDefinition();
                r1.Height = new GridLength(0, GridUnitType.Auto);

                ColumnDefinition c0 = new ColumnDefinition();
                c0.Width = new GridLength(.50, GridUnitType.Star);

                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(5, GridUnitType.Star);

                snap_grid_item.Margin = new Thickness(10);

                snap_grid_item.RowDefinitions.Add(r0);
                snap_grid_item.RowDefinitions.Add(r1);
                snap_grid_item.ColumnDefinitions.Add(c0);
                snap_grid_item.ColumnDefinitions.Add(c1);

                Grid.SetColumn(img, 0);
                Grid.SetRowSpan(img, 2);

                Grid.SetColumn(username, 1);

                Grid.SetColumn(txt, 1);
                Grid.SetRow(txt, 1);

                snap_grid_item.Children.Add(img);
                snap_grid_item.Children.Add(username);
                snap_grid_item.Children.Add(txt);

                stackpanel.Children.Add(snap_grid_item);

            }*/

            // !!! OLD CODE !!! ///
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
                const uint streamLength = 100000;

                HttpMultipartFormDataContent multipartContent = new HttpMultipartFormDataContent();

                HttpStreamContent streamContent = new HttpStreamContent(new SlowInputStream(streamLength));


                var values = new Dictionary<string, string>
            {
                { "username", main.static_user },
                { "password", main.static_pass },
                { "device_id", deviceid.ToString() },
                { "snap_id", current_snap.id.ToString() }
            };

                HttpFormUrlEncodedContent formContent = new HttpFormUrlEncodedContent(values);
                streamContent.Headers.ContentLength = streamLength;

                multipartContent.Add(formContent);
                multipartContent.Add(streamContent);

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

            /// !!! OLD CODE !!! ///

            /*var values = new Dictionary<string, string>
            {
                { "username", main.static_user },
                { "password", main.static_pass },
                { "device_id", deviceid.ToString() },
                { "snap_id", ((TextBlock)sender).Name }
            };

            var client = new System.Net.Http.HttpClient();

            var content = new FormUrlEncodedContent(values);

            client.DefaultRequestHeaders.Add("device", "WindowsClient/" + settings.version);

            var response = await client.PostAsync(settings.url + "/get_snap.php", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseBytes = await response.Content.ReadAsByteArrayAsync();

            Old login code, obsolete.

            */

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

                // start timer (TODO: make it get the snap length


                dt = new DispatcherTimer();
                dt.Tick += dt_tick;
                dt.Interval = new TimeSpan(0, 0, 1);
                dt.Start();

                snap_num.Text = current_snap.time.ToString();
                // delete snap


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
    }

    class Snap
    {
        public int id { get; set; }
        public string from { get; set; }
        public string status { get; set; }
        public BitmapImage ImageSource { get; set; }
        public int time { get; set; }
    }

}