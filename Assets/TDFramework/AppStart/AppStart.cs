

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








        

        #region 回调
        public void DownloadVersionFileSuccessedCallback(VersionStatus status)
        {
            switch (status)
            {
                case VersionStatus.High:
                    {
                        //本地与远端Md5File做比较,下载新的资源, 开始下载资源
                        ResourcesDownloadCallbackTable table = new ResourcesDownloadCallbackTable()
                        {
                            Error = ResourcesDownloadError,
                            Befor = ResourcesDownloadBefor,
                            Progress = ResourcesDownloadProgress,
                            OneComplete = ResourcesDownloadOneComplete,
                            AllComplete = ResourcesDownloadAllComplete,
                        };
                        DownloadResources resourcesUpdate = new DownloadResources(table);
                        break;
                    }
                case VersionStatus.Equal:
                    {
                        break;
                    }
                default:
                    break;
            }
        }
        public void DownloadVersionFileFailedCallback()
        {
            Debug.Log("版本信息下载失败!请检查网络!");
        }
        public void ResourcesDownloadError(string content)
        {

        }
        public void ResourcesDownloadBefor(int i)
        {

        }
        public void ResourcesDownloadProgress(string fileName, SDownloadFileResult result)
        {

        }
        public void ResourcesDownloadOneComplete(string content, int i, int j)
        {

        }
        public void ResourcesDownloadAllComplete()
        {

        }
        #endregion
    }

    
}