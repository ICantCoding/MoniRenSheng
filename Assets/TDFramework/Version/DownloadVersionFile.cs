

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using Utils.Ini;
    using TDFramework.Utils;
    using System.IO;

    public class DownloadVersionFile : MonoBehaviour
    {

        #region 单例
        private static DownloadVersionFile m_instance = null;
        public static DownloadVersionFile Instance
        {
            get
            {
                return Util.GetInstance(ref m_instance, typeof(DownloadVersionFile).Name, true);
            }
        }
        #endregion

        #region 代理
        public delegate void DownloadSuccessedCallback(VersionStatus status);
        public delegate void DownloadFailedCallback();
        public DownloadSuccessedCallback downloadSuccessedCallback;
        public DownloadFailedCallback downloadFailedCallback;
        #endregion

        #region 字段和属性
        private string m_downloadurl;
        private Version m_localVersion;
        private Version m_remoteVersion;
        private int m_downloadRetryCount = 3;
        private int m_downloadTimeout = 10;
        private bool m_downloadStatus = false; //下载版本文件状态, 下载成功/下载失败
        private VersionStatus m_versionStatus = VersionStatus.None; //版本号比较结果
        #endregion

        #region 方法
        public void Download(DownloadSuccessedCallback successed_callback, DownloadFailedCallback failed_callback)
        {
            downloadSuccessedCallback = successed_callback;
            downloadFailedCallback = failed_callback;
            m_downloadurl = AppConfig.RemoteVersionFileUrl;
            m_downloadTimeout = AppConfig.DownloadVersionFileTimeout;
            m_downloadRetryCount = AppConfig.DownloadVersionFileFailedTryCount;

            //检查版本文件
            //Editor平台下Version.json信息文件放在Application.dataPath目录下
            //PC平台下Version.json信息文件放在Application.streamingAssetsPath目录下
            //Mobile平台下Version.json信息文件首次放在Application.streamingAssetsPath目录下, 之后放在Application.persistentDataPath目录下
            string versionFilePath = AppConfig.VersionFilePath;
            versionFilePath = Util.DeviceResPath() + versionFilePath;
            if (File.Exists(versionFilePath))
            {
                //已经下载过version.json, DeviceResPath目录下才会有version.json文件
                //从Util.DeviceResPath目录中读取本地version.json信息
                m_localVersion = VersionHelper.GetLocalVersionForApp(versionFilePath);
            }
            else
            {
                //没有下载过version.json, 所以DeviceResPath目录下才不会有version.json文件
                //那么需要把StreamingAssets/GameStartConfig/version.json写入到Util.DeviceResPath目录下
                //从Util.DeviceResPath目录中读取本地version.json信息
                Version version = VersionHelper.GetLocalVersionForStreamingAssets();
                if (version != null)
                {
                    VersionHelper.WriteLocalVersionFile(version);
                    m_localVersion = version;
                }
            }
            Debug.Log("LocalVersion: " + m_localVersion.ToString());
            //从服务器下载version.json信息, 将最新的version.json信息写入到Util.DeviceResPath目录中(更新version.json)
            Download();
        }
        private void Download()
        {
            StartCoroutine(DownloadRemoteVersionInfo());
        }
        IEnumerator DownloadRemoteVersionInfo()
        {
            UnityWebRequest request = null;
            while (m_downloadStatus == false && m_downloadRetryCount > 0)
            {
                request = UnityWebRequest.Get(m_downloadurl);
                request.timeout = m_downloadTimeout;
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
                }
            }
            if (m_downloadStatus)
            {
                if (downloadSuccessedCallback != null)
                {
                    byte[] bytes = request.downloadHandler.data;
                    string content = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    m_remoteVersion = VersionHelper.JsonText2Version(content);
                    VersionHelper.WriteLocalVersionFile(m_remoteVersion);
                    Debug.Log("RemoteVersion: " + m_remoteVersion.ToString());
                    VersionStatus versionStatus = VersionHelper.CompareLocalAndRemoteVersion(m_localVersion, m_remoteVersion);
                    downloadSuccessedCallback(versionStatus);
                }
            }
            else
            {
                if (downloadFailedCallback != null)
                {
                    downloadFailedCallback();
                }
            }
            request.Abort();
            request.Dispose();
        }
        #endregion

        #region 辅助方法

        #endregion
    }
}

