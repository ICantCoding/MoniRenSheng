

namespace TDFramework
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.Utils.Ini;
    using TDFramework.Utils;

    public class AppStart : MonoBehaviour
    {
        #region Unity生命周期
        void Start()
        {
            // GameObject.DontDestroyOnLoad(gameObject);
            // //下载版本信息
            new DownloadVersionFile(DownloadVersionFileSuccessedCallback, 
             DownloadVersionFileFailedCallback).Download();
            // //对象池加载
            // ObjectManager.Instance().InitGoPool(transform.Find("GoPool"), transform.Find("SceneGos"));
        }
        #endregion








        

        #region 回调方法
        //版本信息下载成功回调
        public void DownloadVersionFileSuccessedCallback(VersionStatus status)
        {
            Debug.Log("版本信息下载成功!");
            switch (status)
            {
                case VersionStatus.High:
                    {
                        Debug.Log("需要更新资源...");
                        //本地与远端Md5File做比较,下载新的资源
                        new DownloadMd5File(DownloadMd5FileSuccessedCallback, DownloadMd5FileFailedCallback).Download();
                        break;
                    }
                case VersionStatus.Equal:
                    {
                        Debug.Log("不需要更新资源...");
                        break;
                    }
                default:
                    break;
            }
        }
        //版本信息下载失败回调
        public void DownloadVersionFileFailedCallback()
        {
            Debug.Log("版本信息下载失败!请检查网络!");
        }
        //Md5File下载成功回调
        public void DownloadMd5FileSuccessedCallback(string content)
        {
            Debug.Log("Md5File下载成功, 服务器Md5File: " + content);
        }
        //Md5File下载失败回调
        public void DownloadMd5FileFailedCallback()
        {
            Debug.Log("Md5File下载失败.");
        }
        #endregion
    }
}