using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PicLoc
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class snap_screen : Page
    {
        public static Boolean fromLogin;
        public static String JSON;


        public snap_screen()
        {
            this.InitializeComponent();
            setSnaps();
        }

        public void setSnaps()
        {
            JObject array = JObject.Parse(JSON);
            ObservableCollection<Snap> snapList = new ObservableCollection<Snap>();
            try
            {
                if (array["snaps"] == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }
            JObject items = array["snaps"].Value<JObject>();
            Debug.WriteLine("snap-screen | setSnaps | Snap count: " + items.Count);

            foreach (var x in items)
            {
                Snap tmp_snap = new Snap() { id = int.Parse(x.Key), from = x.Value.SelectToken("user_from").ToString(), status = x.Value.SelectToken("status").ToString(), ImageSource = new BitmapImage(new Uri(this.BaseUri, "/Assets/snapscreen/Images/" + x.Value.SelectToken("status").ToString() + ".png")), time = int.Parse(x.Value.SelectToken("time").ToString()) };
                snapList.Add(tmp_snap);
            }
            listView_snaps.ItemsSource = snapList;
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
}
