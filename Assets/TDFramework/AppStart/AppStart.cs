

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

        public AudioSource m_audio;
        private AudioClip m_clip;

        #region Unity生命周期
        void OnFinished(string path, object obj,
         object param1, object param2, object param3)
        {
            m_clip = obj as AudioClip;
            m_audio.clip = m_clip;
            m_audio.Play();
        }


        void Start()
        {
            //检查版本文件
            //Editor平台下Version.json信息文件放在Application.dataPath目录下
            //PC平台下Version.json信息文件放在Application.streamingAssetsPath目录下
            //Mobile平台下Version.json信息文件放在Application.persistentDataPath目录下
            string versionFileName = "Config/Version/version.json";
            versionFileName = Util.DeviceResPath() + versionFileName;
            if (File.Exists(versionFileName))
            {
                //已经下载过version.json, DeviceResPath目录下才会有version.json文件

            }
            else
            {
                //没有下载过version.json, 所以DeviceResPath目录下才不会有version.json文件
                //那么需要把StreamingAssets/GameStartConfig/version.json写入到Util.DeviceResPath目录下

            }
            //从Util.DeviceResPath目录中读取本地version.json信息

            //从服务器下载version.json信息, 将最新的version.json信息写入到Util.DeviceResPath目录中(更新version.json)

        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResourceMgr.Instance().Init(this);
                ResourceMgr.Instance().AsyncLoadAsset<AudioClip>("Assets/GameData/Happy.mp3", OnFinished, LoadAssetPriority.MIDDLE);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                ResourceMgr.Instance().UnLoadAsset(m_clip, true);
                m_clip = null;
                m_audio.Stop();
                m_audio.clip = null;
            }
        }
        void OnApplicationQuit()
        {
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#endif
        }
        #endregion


        #region 回调方法
        public void DownloadVersionFileCallback()
        {

        }
        #endregion
    }
}
