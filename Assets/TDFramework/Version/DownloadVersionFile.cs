

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using TDDesignMode;
    using Utils.Ini;

    public class DownloadVersionFile
    {
        
        #region 代理
        public delegate void DownloadSuccessedCallback();
        public delegate void DownloadFailedCallback();
        public DownloadSuccessedCallback downloadSuccessedCallback;
        public DownloadFailedCallback downloadFailedCallback;
        #endregion

        #region 字段和属性
        private MonoBehaviour m_mono;
        private VersionInfo m_localVersionInfo;
        private VersionInfo m_remoteVersionInfo;
        private int m_downloadRetryCount;
        private bool m_downloadStatus = false; //下载版本文件状态, 下载成功/下载失败
        #endregion

        #region 构造函数
        public DownloadVersionFile(MonoBehaviour mono)
        {
            if (mono != null)
            {
                m_mono = mono;
            }
        }
        #endregion

        #region 方法
        public void Download(DownloadSuccessedCallback successed_callback, DownloadFailedCallback failed_callback)
        {
            downloadSuccessedCallback = successed_callback;
            downloadFailedCallback = failed_callback;
            m_mono.StartCoroutine(DownloadRemoteVersionInfo());
        }
        IEnumerator DownloadRemoteVersionInfo()
        {
            ReadLocalVersionInfo();
            while (m_downloadStatus == false && m_downloadRetryCount > 0)
            {
                UnityWebRequest request = UnityWebRequest.Get(m_localVersionInfo.app_download_url);
                request.timeout = m_localVersionInfo.timeout;
                yield return request.SendWebRequest();
                byte[] bytes = request.downloadHandler.data;

                if (request.responseCode != 200)
                {
                    //下载配置文件失败
                    m_downloadRetryCount--;
                }
                else
                {
                    m_downloadStatus = true;
                    ReadRemoteVersionInfo(bytes);
                }
                request.Abort();
                request.Dispose();
            }
            if (m_downloadStatus)
            {
                if (downloadSuccessedCallback != null)
                {
                    downloadSuccessedCallback();
                }
            }
            else
            {
                if (downloadFailedCallback != null)
                {
                    downloadFailedCallback();
                }
            }
        }
        #endregion

        #region 辅助方法
        void ReadLocalVersionInfo()
        {
            IniHelper.LoadConfigFromStreamingAssets(AppConfig.VersionFilePath);
            m_localVersionInfo = new VersionInfo()
            {
                version_num = IniHelper.GetContent("VersionInfo", "version"),
                app_download_url = IniHelper.GetContent("VersionInfo", "app_download_url"),
                res_download_url = IniHelper.GetContent("VersionInfo", "res_download_url"),
                download_retry = IniHelper.GetInt("VersionInfo", "download_fail_retry")
            };
            m_downloadRetryCount = m_localVersionInfo.download_retry;
        }
        void ReadRemoteVersionInfo(byte[] bytes)
        {
            string str = System.Text.Encoding.Default.GetString ( bytes );
            IniHelper.LoadConfigFromBytes(bytes);
            m_remoteVersionInfo = new VersionInfo()
            {
                version_num = IniHelper.GetContent("VersionInfo", "version"),
                app_download_url = IniHelper.GetContent("VersionInfo", "app_download_url"),
                res_download_url = IniHelper.GetContent("VersionInfo", "res_download_url"),
                download_retry = IniHelper.GetInt("VersionInfo", "download_fail_retry")
            };
        }
        #endregion
    }
}

