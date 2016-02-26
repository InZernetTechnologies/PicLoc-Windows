using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace PicLoc
{
    public sealed partial class snap_screen : Page
    {
        public static Boolean fromLogin;
        public static String JSON;

        account a = new account();
        helper h = new helper();

        DispatcherTimer dt;

        public snap_screen()
        {
            this.InitializeComponent();
            if (fromLogin)
            {
                setSnaps();
                setFriends();
            }
        }

        public void setFriends()
        {
            JObject array = JObject.Parse(JSON);
            ObservableCollection<classes.Friend> friendList = new ObservableCollection<classes.Friend>();

            classes.Friend myself = new classes.Friend() { username = main.static_user, display_name = ">> Myself <<", status = "friends" };
            friendList.Add(myself);

            try
            {
                if (array["friends"] == null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
            JObject items = array["friends"].Value<JObject>();
            Debug.WriteLine("snap-screen | friends | Friend count: " + items.Count);

            foreach (var x in items)
            {
                classes.Friend tmp_friend = new classes.Friend() { username = x.Key, display_name = "Temp Display Name", /*x.Value["display_name"].ToString()*/ status = x.Value["status"].ToString() };
                friendList.Add(tmp_friend);
            }
            listView_friends.ItemsSource = friendList;
        }

        public void setSnaps()
        {
            JObject array = JObject.Parse(JSON);
            ObservableCollection<classes.Snap> snapList = new ObservableCollection<classes.Snap>();
            try
            {
                if (array["snaps"] == null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
            JObject items = array["snaps"].Value<JObject>();
            Debug.WriteLine("snap-screen | setSnaps | Snap count: " + items.Count);

            foreach (var x in items)
            {
                classes.Snap tmp_snap = new classes.Snap() { id = int.Parse(x.Key), from = x.Value.SelectToken("user_from").ToString(), status = x.Value.SelectToken("status").ToString(), ImageSource = new BitmapImage(new Uri(this.BaseUri, "/Assets/snapscreen/Images/" + x.Value.SelectToken("status").ToString() + ".png")), time = int.Parse(x.Value.SelectToken("time").ToString()) };
                snapList.Add(tmp_snap);
            }
            listView_snaps.ItemsSource = snapList;
        }

        private async void listView_snaps_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            int ssnap_id = 0;
            int ssnap_time = 0;
            if (listView_snaps.SelectedIndex == -1)
            {
                return; // Sometimes UWP shows a click's index as -1 so we will just cancel this so no crashes happen :)
            }
            ssnap_id = ((classes.Snap)listView_snaps.SelectedItem).id;
            ssnap_time = ((classes.Snap)listView_snaps.SelectedItem).time;
            listView_snaps.SelectedIndex = -1;
            if (await a.getSnap(main.static_user, main.static_pass, ssnap_id, progressBar_top))
            {
                snap_grid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                snap_image.Source = new BitmapImage(new Uri("ms-appdata:///local/images/" + ssnap_id + ".jpg", UriKind.Absolute));
                snap_time.Text = ssnap_time.ToString();
            }
        }

        private async void AppBarButton_refresh_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (pivot_main.SelectedIndex == 0)
            {
                Debug.WriteLine("Refresh snaps");
                String loginJSON = await a.getSnaps(main.static_user, main.static_pass, progressBar_top);
                JObject jo = JObject.Parse(loginJSON);

                if (jo["status"].ToString() == "True")
                {
                    JSON = loginJSON;
                    listView_snaps.ItemsSource = null;
                    setSnaps();
                }
                else
                {
                    h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"] != null) ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
                }
            } else if (pivot_main.SelectedIndex == 2)
            {
                Debug.WriteLine("Refresh friends");
                String loginJSON = await a.getFriends(main.static_user, main.static_pass, progressBar_top);
                JObject jo = JObject.Parse(loginJSON);

                if (jo["status"].ToString() == "True")
                {
                    JSON = loginJSON;
                    listView_friends.ItemsSource = null;
                    setFriends();
                }
                else
                {
                    h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"] != null) ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
                }
            }
        }

        private void dt_tick(object sender, object e)
        {
            snap_time.Text = (int.Parse(snap_time.Text) - 1).ToString();
            if (int.Parse(snap_time.Text) == -1)
            {
                dt.Stop();
                restSnapView();
            }
        }

        private async void restSnapView()
        {
            dt.Stop();

            String file = System.IO.Path.GetFileName(((BitmapImage)snap_image.Source).UriSource.ToString());
            snap_image.Source = null;

            Debug.WriteLine("Snap file: " + file);

            snap_grid.Visibility = Visibility.Collapsed;

            StorageFolder img = await ApplicationData.Current.LocalFolder.GetFolderAsync("images");
            StorageFile snap = await img.GetFileAsync(file);
            await snap.DeleteAsync();

            // not deleting

        }

        private async void AppBarButton_camera_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Debug.WriteLine("Camera button tapped");
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                BitmapImage tmp_bitmap = new BitmapImage();
                await tmp_bitmap.SetSourceAsync(await file.OpenReadAsync());

                send_snap.snap_preview = tmp_bitmap;
                send_snap.friends = (ObservableCollection<classes.Friend>)listView_friends.ItemsSource;
                send_snap.pb = progressBar_top;
                send_snap.snap_file = file;

                Frame.Navigate(typeof(send_snap));
            }
        }

        private void AppBarButton_friend_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(add_friend));
        }

        private void AppBarButton_logout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (h.clearAutoLogin())
            {
                Application.Current.Exit();
            } else
            {
                h.showSingleButtonDialog("Error", "There was an issue with clearning the auto login. Please contact support.", "Dismiss");
            }
        }

        private void snap_image_ImageOpened(object sender, RoutedEventArgs e)
        {
            dt = new DispatcherTimer();
            dt.Tick += dt_tick;
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Start();
        }

        private async void listView_friends_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            String friend_username = null;
            String friend_status = null;
            if (listView_friends.SelectedIndex == -1)
            {
                return; // Sometimes UWP shows a click's index as -1 so we will just cancel this so no crashes happen :)
            }
            friend_username = ((classes.Friend)listView_friends.SelectedItem).username;
            friend_status = ((classes.Friend)listView_friends.SelectedItem).status;

            listView_friends.SelectedIndex = -1;

            if (friend_status == "pending")
            {
                if (await h.showDialog2Buttons("Accept friend?", "Do you want to accept [" + friend_username + "]", "Yes", "No") == 1)
                {
                    Debug.WriteLine("Accept friend");
                    String accJSON = await a.acceptFriend(main.static_user, main.static_pass, progressBar_top, friend_username);
                    JObject jo = JObject.Parse(accJSON);

                    if (jo["status"].ToString() == "True")
                    {
                        Debug.WriteLine("Refresh friends");
                        String loginJSON = await a.getFriends(main.static_user, main.static_pass, progressBar_top);
                        JObject jo1 = JObject.Parse(loginJSON);

                        if (jo1["status"].ToString() == "True")
                        {
                            listView_friends.ItemsSource = null;
                            JSON = loginJSON;
                            setFriends();
                        }
                        else
                        {
                            h.showSingleButtonDialog("Server error [" + jo1["code"] + "]", ((jo1["message"] != null) ? jo1["message"].ToString() : "No server message was provided"), "Dismiss");
                        }
                    }
                    else
                    {
                        h.showSingleButtonDialog("Server error [" + jo["code"] + "]", ((jo["message"] != null) ? jo["message"].ToString() : "No server message was provided"), "Dismiss");
                    }
                } else
                {
                    Debug.WriteLine("don't Accept friend");
                }

            }
        }
    }
}
