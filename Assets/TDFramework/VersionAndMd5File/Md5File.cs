
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
// 文件名(File Name):             Md5File.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-28 20:49:20
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************

namespace TDFramework
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Md5File
    {
        #region 字段
        private Dictionary<string, string> m_localMd5Dict;
        private Dictionary<string, string> m_remoteMd5Dict;
        private Dictionary<string, string> m_tempMd5Dict;
        private Queue<SDownloadConfig> m_needDownloadConfigQueue;
        public Queue<SDownloadConfig> NeedDownloadConfigQueue
        {
            get { return m_needDownloadConfigQueue; }
            set{m_needDownloadConfigQueue = value;}
        }
        #endregion

        #region 构造函数
        public Md5File(string remoteMd5)
        {
            m_localMd5Dict = Md5FileHelper.LocalMd5File2Dict();
            m_remoteMd5Dict = Md5FileHelper.Md5Text2Dict(remoteMd5);
            m_tempMd5Dict = Md5FileHelper.LocalMd5File2Dict(AppConfig.Temp_Md5FilePath);
            m_needDownloadConfigQueue = new Queue<SDownloadConfig>();
            InitNeedDownloadFileQueue();
        }
        #endregion

        #region 方法
        public string GetRemoteMd5(string key)
        {
            string md5 = string.Empty;
            m_remoteMd5Dict.TryGetValue(key, out md5);
            return md5;
        }
        public void InitNeedDownloadFileQueue()
        {
            Dictionary<string, string>.Enumerator e = m_remoteMd5Dict.GetEnumerator();
            while (e.MoveNext())
            {
                string file = e.Current.Key;
                string remoteMd5 = e.Current.Value;
                string localMd5 = string.Empty;
                string md5FilePath = string.Format("{0}{1}", Util.DeviceResPath(), file);
                m_localMd5Dict.TryGetValue(e.Current.Key, out localMd5);
                if (string.IsNullOrEmpty(localMd5) || localMd5.Trim() != remoteMd5.Trim() || File.Exists(md5FilePath) == false)
                {
                    if (string.IsNullOrEmpty(localMd5))
                    {
                        Debug.Log(file + "资源需要更新: localMd5 = null.");
                    }
                    else if (localMd5.Trim() != remoteMd5.Trim())
                    {
                        Debug.Log(file + "资源需要更新: 文件内容改变.");
                    }
                    else if (File.Exists(md5FilePath) == false)
                    {
                        Debug.Log(file + "资源需要更新: 本地没有该资源文件.");
                    }

                    string tempMd5 = string.Empty;
                    if (m_tempMd5Dict.TryGetValue(file, out tempMd5))
                    {
                        if (tempMd5.Trim() != remoteMd5.Trim())
                        {
                            string tempFile = string.Format("{0}{1}", Util.DeviceResPath(), file);
                            if (File.Exists(tempFile))
                            {
                                Debug.Log(file + "部分文件已经下载, 但是远程文件发生了改变, 所以需要删除缓存文件重新下载.");
                                File.Delete(tempFile);
                            }
                        }
                    }
                    // m_needDownloadFileQueue.Enqueue(file);
                }
            }
            e.Dispose();
            m_tempMd5Dict.Clear();
        }
        public void EnqueueNeedDownloadConfigQuque(string fileName)
        {
            //资源本身
            SDownloadConfig config = new SDownloadConfig();
            config.fileName = fileName;
            config.download_url = AppConfig.ResourcesDownloadUrl + fileName;
            config.local_url = string.Format("{0}{1}", Util.DeviceResPath() + fileName);
            config.download_timeout = AppConfig.DownloadFileTimeout;
            config.download_failed_retry = AppConfig.DownloadFileFailedTryCount;
            m_needDownloadConfigQueue.Enqueue(config);
            //资源manifest文件
            SDownloadConfig manifestConfig = new SDownloadConfig();
            manifestConfig.fileName = string.Format("{0}.manifest", fileName);
            manifestConfig.local_url = string.Format("{0}{1}", Util.DeviceResPath() + manifestConfig.fileName);
            manifestConfig.download_url = string.Format("{0}.manifest", config.download_url);
            manifestConfig.download_timeout = AppConfig.DownloadFileTimeout;
            manifestConfig.download_failed_retry = AppConfig.DownloadFileFailedTryCount;
            m_needDownloadConfigQueue.Enqueue(manifestConfig);
        }
        //把正在下载的资源文件写入到TempMd5中, 下载完成后需要从TempMd5中删除
        public void PushTempFile(string file)
        {
            string md5 = string.Empty;
            if (m_remoteMd5Dict.TryGetValue(file, out md5))
            {
                if (m_tempMd5Dict.ContainsKey(file))
                {
                    m_tempMd5Dict[file] = md5;
                }
                else
                {
                    m_tempMd5Dict.Add(file, md5);
                }
            }
        }
        //下载完成时移除它
        public void PopTmpFile(string file)
        {
            if (m_tempMd5Dict.ContainsKey(file))
            {
                m_tempMd5Dict.Remove(file);
            }
        }
        //下载完成时更新本地md5
        public void UpdateLocalMd5File(string file)
        {
            if (file.EndsWith(".manifest")) return;
            string md5 = m_remoteMd5Dict[file];
            if (m_localMd5Dict.ContainsKey(file))
            {
                m_localMd5Dict[file] = md5;
            }
            else
            {
                m_localMd5Dict.Add(file, md5);
            }
        }
        //退出时将md5写到文件
        public void Destroy()
        {
            Md5FileHelper.Dict2LocalMd5File(m_localMd5Dict, string.Format("{0}/{1}", Util.DeviceResPath() + AppConfig.Md5FilePath));
            Md5FileHelper.Dict2LocalMd5File(m_tempMd5Dict, string.Format("{0}/{1}", Util.DeviceResPath() + AppConfig.Temp_Md5FilePath));
        }
        #endregion
    }
}

