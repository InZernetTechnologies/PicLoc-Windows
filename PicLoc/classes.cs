using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace PicLoc
{
    public class classes
    {

        public class Snap
        {
            public int id { get; set; }
            public string from { get; set; }
            public string status { get; set; }
            public BitmapImage ImageSource { get; set; }
            public int time { get; set; }
        }

        public class Friend
        {
            public string username { get; set; }
            public string status { get; set; }
            public string display_name { get; set; }
        }
    }
}
