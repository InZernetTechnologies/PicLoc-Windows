using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace PicLoc
{
    class settings
    {
        public static readonly String API_URL = "https://picloc.inzernettechnologies.com"; // API CALLBACK URL [No slash after]
        public static readonly String API_version = "1.03";
        public static readonly String API = API_URL + "/" + API_version;
        private static readonly String APP_dyn_version = Package.Current.Id.Version.Major + "." + Package.Current.Id.Version.Minor + "." + Package.Current.Id.Version.Build + "." + Package.Current.Id.Version.Revision;
        public static readonly String APP_version = APP_dyn_version + " | APIv" + API_version; // Version used in headers
    }
}