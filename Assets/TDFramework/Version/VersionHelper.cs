

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class VersionInfo
    {
        public string version_num;
        public string version_download_url;
        public string app_download_url;
        public string res_download_url;
        public int download_retry;
        public int timeout;

        public override string ToString()
        {
            return "version_num: " + version_num + 
            ",version_download_url: " + version_download_url + 
            ", app_download_url: " + app_download_url + 
            ", res_download_url: " + res_download_url + 
            ", download_retry: " + download_retry +
            ", timeout: " + timeout;
        }
    }
}
