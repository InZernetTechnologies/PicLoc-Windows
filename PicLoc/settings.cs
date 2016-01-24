using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicLoc
{
    class settings
    {
        public static readonly String API_URL = "http://picloc.inzernettechnologies.com"; // API CALLBACK URL
        public static readonly String API_version = "1.02";
        public static readonly String API = API_URL + "/" + API_version;
        public static readonly String APP_version = "1.0.1 | APIv" + API_version; // Version used in headers
    }
}
