

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
            DownloadVersionFile.Instance.Download(DownloadVersionFileSuccessedCallback, 
             DownloadVersionFileFailedCallback);
            // //对象池加载
            // ObjectManager.Instance().InitGoPool(transform.Find("GoPool"), transform.Find("SceneGos"));
            
        }
        #endregion








        

        #region 回调方法
        //服务器中版本信息下载成功回调
        public void DownloadVersionFileSuccessedCallback(VersionStatus status)
        {
            Debug.Log("版本信息下载成功!");
            switch (status)
            {
                case VersionStatus.High:
                    {
                        Debug.Log("需要更新资源...");
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
        //服务器中版本信息下载失败回调
        public void DownloadVersionFileFailedCallback()
        {
            Debug.Log("版本信息下载失败!请检查网络!");
        }
        #endregion
    }
}