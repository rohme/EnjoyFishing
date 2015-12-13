using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnjoyFishing
{
    public class Constants
    {
        public const int MAX_LOOP_COUNT = 100;
        public const int FILELOCK_RETRY_COUNT = 10;
        public const string WINDOW_TITLE_FORMAT = "{0} {1} {2}";

        public const string PATH_TEMP = "Temp";

        public const string URL_HTTP = "http://";
        public const string URL_FISHING = "/fishing/";
        public const string URL_FISHING_FISH = URL_FISHING + "fish/";
        public const string URL_FISHING_ZONE = URL_FISHING + "zone/";
        public const string URL_FISHING_BAIT = URL_FISHING + "bait/";
        public const string URL_FISHING_ROD = URL_FISHING + "rod/";
    }
}
