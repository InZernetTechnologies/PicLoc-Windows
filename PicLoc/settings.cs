using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicLoc
{
    class settings
    {
        public static readonly String API_URL = "http://localhost/PicLoc"; // API CALLBACK URL
        public static readonly String API_version = "1.01";
        public static readonly String API = API_URL + "/" + API_version;
        public static readonly String APP_version = "1.0.0"; // Version used in headers
    }
}
