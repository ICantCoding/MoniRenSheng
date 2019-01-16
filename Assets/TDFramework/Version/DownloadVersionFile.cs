

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using TDDesignMode;
    using Utils.Ini;

    public class DownloadVersionFile : TDSingleton<DownloadVersionFile>
    {

        #region 字段和属性
        private Version m_oldVersion;
        #endregion

        #region 构造函数
        public DownloadVersionFile()
        {
            
        }
        #endregion

        #region 方法
        IEnumerator ReadOldVersionInfo()
        {
            IniHelper.LoadConfig(AppConfig.VersionFilePath);
            while(!IniHelper.IsDone)
            {   
                yield return null;
            }
            m_oldVersion = new VersionInfo()
            {
                version_num = IniHelper.GetContent("Version",""),
            };
        }
        #endregion
    }
}

