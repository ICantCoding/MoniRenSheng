

//////////////////////////////////////////////////////////////////
//                            _ooOoo_                           //
//                           o8888888o                          //
//                           88" . "88                          //
//                           (| -_- |)                          //
//                           O\  =  /O                          //    
//                        ____/`---'\____                       //
//                      .'  \\|     |//  `.                     //
//                     /  \\|||  :  |||//  \                    //
//                    /  _||||| -:- |||||-  \                   //
//                    |   | \\\  -  /// |   |                   //
//                    | \_|  ''\---/''  |   |                   //
//                    \  .-\__  `-`  ___/-. /                   //
//                  ___`. .'  /--.--\  `. . __                  //
//               ."" '<  `.___\_<|>_/___.'  >'"".               //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |             //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /             //
//         ======`-.____`-.___\_____/___.-`____.-'======        //
//                            `=---='                           //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^         //
//           佛祖保佑 程序员一生平安,健康,快乐,没有Bug!            //
//////////////////////////////////////////////////////////////////

// ***************************************************************
// Copyright (C) 2017 The company name
//
// 文件名(File Name):             DownloadMd5File.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-28 22:07:55
// 修改者列表(modifier):
// 模块描述(Module description):  用于下载服务器远端Version.json文件
// ***************************************************************


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
        #region 代理
        public delegate void DownloadSuccessedCallback(VersionStatus status);
        public delegate void DownloadFailedCallback();
        public DownloadSuccessedCallback downloadSuccessedCallback;
        public DownloadFailedCallback downloadFailedCallback;
        #endregion

        #region 字段和属性
        private string m_downloadurl;
        private int m_downloadRetryCount = 3;
        private int m_downloadTimeout = 10;
        private float m_downloadTryAgainDelay = 2.0f;

        private Version m_localVersion;
        private Version m_remoteVersion;
        private VersionStatus m_versionStatus = VersionStatus.None; //版本号比较结果
        #endregion

        #region 构造方法
        public DownloadVersionFile(DownloadSuccessedCallback successed_callback, DownloadFailedCallback failed_callback)
        {
            m_downloadurl = AppConfig.RemoteVersionFileUrl;
            m_downloadTimeout = AppConfig.DownloadFileTimeout;
            m_downloadRetryCount = AppConfig.DownloadFileFailedTryCount;
            m_downloadTryAgainDelay = AppConfig.DownloadFileTryAgainDelay;
            downloadSuccessedCallback = successed_callback;
            downloadFailedCallback = failed_callback;
        }
        #endregion

        #region 方法
        public void Download()
        {
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
            DownloadRemoteVersion();
        }
        private void DownloadRemoteVersion()
        {
            m_downloadRetryCount--;
            DownloadTextFile.Instance.Download(m_downloadurl, DownloadRemoteVersionCompelete,
            m_downloadTryAgainDelay, m_downloadTimeout);
        }
        private void DownloadRemoteVersionCompelete(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                //下载失败, 判断是否需要继续下载
                if (m_downloadRetryCount > 0)
                {
                    DownloadRemoteVersion(); //再次下载
                }
                else
                {
                    if (downloadFailedCallback != null)
                    {
                        downloadFailedCallback();
                    }
                }
            }
            else
            {
                if (downloadSuccessedCallback != null)
                {
                    m_remoteVersion = VersionHelper.JsonText2Version(content);
                    #warning 这里不要立即写入, 需要等Game版本完全更新成功后才能写入最新版本号
                    VersionHelper.WriteLocalVersionFile(m_remoteVersion); 
                    Debug.Log("RemoteVersion: " + m_remoteVersion.ToString());
                    VersionStatus versionStatus = VersionHelper.CompareLocalAndRemoteVersion(m_localVersion, m_remoteVersion);
                    downloadSuccessedCallback(versionStatus);
                }
            }
        }
        #endregion
    }
}

