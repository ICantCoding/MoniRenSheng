

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.Utils.Ini;

    public class AppStart : MonoBehaviour
    {

        #region Unity生命周期
        void Start()
        {
            //下载版本号
            DownloadVersionFile downloadVersionFile = new DownloadVersionFile(this);
            downloadVersionFile.Download(DownloadVersionFileCallback, null);
        }
        #endregion



        #region 回调方法
        public void DownloadVersionFileCallback()
        {

        }
        #endregion
    }
}
