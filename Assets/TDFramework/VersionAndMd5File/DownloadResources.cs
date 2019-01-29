
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
// 文件名(File Name):             ResourcesUpdate.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-29 20:47:53
// 修改者列表(modifier):
// 模块描述(Module description):  资源更新功能, 针对AssetBundle进行更新
// ***************************************************************

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public struct ResourcesDownloadCallbackTable
    {
        public Action<string> Error;
        public Action<int> Befor;
        public Action AllComplete;
        public Action<string, int, int> OneComplete;
        public Action<string, SDownloadFileResult> Progress;
    }

    public struct SDownloadConfig
    {
        public string fileName;
        public string download_url;
        public string local_url;
        public int download_timeout;
        public int download_failed_retry;
    }

    public struct SDownloadEventResult
    {
        public SDownloadConfig Info;
        public string Error;
        public SDownloadFileResult FileResult;
        public DownloadEventType EventType;

        public SDownloadEventResult(DownloadEventType eventType)
            : this(eventType, new SDownloadFileResult(), new SDownloadConfig(), string.Empty)
        { }

        public SDownloadEventResult(DownloadEventType eventType, SDownloadFileResult fileResult)
            : this(eventType, fileResult, new SDownloadConfig(), string.Empty)
        { }

        public SDownloadEventResult(DownloadEventType eventType, string error)
            : this(eventType, new SDownloadFileResult(), new SDownloadConfig(), error)
        { }

        public SDownloadEventResult(DownloadEventType eventType, SDownloadConfig info)
            : this(eventType, new SDownloadFileResult(), info, string.Empty)
        { }
        public SDownloadEventResult(DownloadEventType eventType, SDownloadFileResult fileResult, SDownloadConfig info, string error)
        {
            Error = error;
            Info = info;
            FileResult = fileResult;
            EventType = eventType;
        }
    }

    public class DownloadResources
    {
        #region 字段
        private Md5File m_md5File = null;
        private List<SDownloadConfig> m_downloadConfigFailedList = null;
        private float m_downloadTotalSize = 0;
        private int m_downloadedCount = 0;
        private int m_downloadTotal = 0;

        private Queue<SDownloadConfig> m_downloadConfigQueue = null;
        private IWebDownload m_downloader = null;
        private SDownloadConfig m_currentConfig;    //正在下载的资源Config
        private ResourcesDownloadCallbackTable m_callbackTable; //下载资源回调函数Table
        #endregion

        #region 构造方法
        public DownloadResources(ResourcesDownloadCallbackTable table)
        {
            m_downloadConfigFailedList = new List<SDownloadConfig>();
            m_downloader = new GameObject("ResourceDownloader").AddComponent<UnityWeb>();
            m_callbackTable = table;
        }
        #endregion

        #region 方法
        //检查计算需要更新的资源文件和这些资源文件的大小总和, 并接着下载资源
        public void CheckAndDownloadResources()
        {
            new DownloadMd5File(DownloadMd5FileSuccssedCallback, DownloadMd5FileFailedCallback).Download();
        }
        public void Download()
        {
            m_downloadConfigQueue = new Queue<SDownloadConfig>(m_md5File.NeedDownloadConfigQueue);
            if (m_downloadConfigQueue == null || m_downloadConfigQueue.Count <= 0)
            {
                return;
            }
            m_downloadedCount = 0;
            m_downloadTotal = m_downloadConfigQueue.Count;
            m_callbackTable.Befor(m_downloadTotal);
            DownloadNext();
        }
        #endregion

        private void DownloadMd5FileSuccssedCallback(string content)
        {
            m_md5File = new Md5File(content);
            m_downloadConfigQueue = new Queue<SDownloadConfig>(m_md5File.NeedDownloadConfigQueue);
            if (m_downloadConfigQueue == null || m_downloadConfigQueue.Count <= 0)
            {
                return;
            }
            DownloadTotalSize();        //先下载资源总大小
        }
        private void DownloadMd5FileFailedCallback()
        {
            Debug.LogError("远程Md5File下载失败.");
        }
        private void DownloadTotalSize()
        {
            if (m_downloadConfigQueue == null || m_downloadConfigQueue.Count <= 0)
            {
                m_downloadTotalSize = 0;
                return;
            }
            DownloadSizeNext();
        }
        private void DownloadSizeNext()
        {
            if (m_downloadConfigQueue.Count <= 0)
            {
                Download();     //开始逐个下载资源
                return;
            }
            SDownloadConfig config = m_downloadConfigQueue.Dequeue();
            m_downloader.DownloadFileSize(config.download_url, config.download_timeout, (status, code, size) =>
            {
                if (status)
                {
                    m_downloadTotalSize += size;
                }
                DownloadSizeNext();
            });
        }
        private void DownloadNext()
        {
            if (m_downloadConfigQueue.Count == 0)
            {
                if (m_downloadConfigFailedList.Count > 0)
                {
                    //有资源下载失败
                    for (int i = 0; i < m_downloadConfigFailedList.Count; i++)
                    {
                        Debug.LogError("下载失败的资源文件: " + m_downloadConfigFailedList[i].download_url);
                        m_callbackTable.Error(m_downloadConfigFailedList[i].fileName);
                    }
                }
                else
                {
                    //资源全部下载成功
                    m_callbackTable.AllComplete();
                }
                return;
            }
            m_currentConfig = m_downloadConfigQueue.Dequeue();
            m_md5File.PushTempFile(m_currentConfig.fileName);
            m_downloader.DownloadFile(m_currentConfig.download_url, m_currentConfig.local_url,
            m_currentConfig.download_timeout,
            OnProgress,
            OnComplete);
        }

        private void OnProgress(SDownloadFileResult result)
        {
            m_callbackTable.Progress(m_currentConfig.fileName, result);
        }
        private void OnComplete(int code)
        {
            if (code == 206)
            {
                DownloadSuccess();
            }
            else if (code == 416)
            {
                ///在使用暂停下载的时候有几率会出现此问题
                ///因为是线程下载，在下载完成的瞬间暂停后 会把当前文件重新加入下载队列导致重复下载， 实际上暂停之后的同时或者下一帧这个文件已经下载完毕
                /// 所以临时文件 aaa.mp4.tmp 和 远程 aaa.mp4 的大小一样 只不过没有被移动 在续传的时候会返回一次416
                /// 所以这里判断如果临时文件的md5和远程md5相等 直接判定下载成功 不走下面下载失败流程 否则会删除重新下载
                /// 这里跳过了manifest文件 因为这个文件没有对应的md5字符串，也没有必要做对比，一般都是1k大小 重新下载没毛病
                if (m_currentConfig.fileName.EndsWith(".manifest") == false)
                {
                    string fileMd5 = Md5Helper.Md5File(m_currentConfig.local_url + ".tmp");
                    string remoteMd5 = m_md5File.GetRemoteMd5(m_currentConfig.fileName);
                    if (fileMd5.Trim() == remoteMd5.Trim())
                    {
                        Debug.LogWarning(m_currentConfig.fileName + " 返回了416 但是文件已经下载完毕 不需要重新下载" + fileMd5 + "==" + remoteMd5);
                        DownloadSuccess();
                    }
                    else
                    {
                        Debug.LogWarning(m_currentConfig.fileName + " 返回了416 文件不一样 重新下载" + fileMd5 + "!=" + remoteMd5);
                        DownloadFail(code);
                    }
                }
            }
            else
            {
                DownloadFail(code);
            }

            m_currentConfig = new SDownloadConfig();
            DownloadNext();
        }

        private void DownloadSuccess()
        {
            m_downloadedCount++;
            string tmp = m_currentConfig.local_url + ".tmp";
            if (File.Exists(tmp))
            {
                if (File.Exists(m_currentConfig.local_url))
                    File.Delete(m_currentConfig.local_url);
                File.Move(tmp, m_currentConfig.local_url);
            }
            //将这个文件从下载中移除
            m_md5File.PopTmpFile(m_currentConfig.fileName);
            //下载完成 更新md5
            m_md5File.UpdateLocalMd5File(m_currentConfig.fileName);
            m_callbackTable.OneComplete(m_currentConfig.fileName, m_downloadedCount, m_downloadTotal);
        }
        private void DownloadFail(int code)
        {
            string tmp = m_currentConfig.local_url + ".tmp";
            //code=0 超时 不需要删除 重试后会续传
            if (code != 0 && File.Exists(tmp))
            {
                File.Delete(tmp);
            }
            if (code != 404 && m_currentConfig.download_failed_retry > 0)
            {
                m_currentConfig.download_failed_retry--;
                //重新加入下载队列中
                m_downloadConfigQueue.Enqueue(m_currentConfig);
            }
            else
            {
                //已经无能为力
                Debug.LogError(m_currentConfig.download_url + " 下载失败 code:" + code);
                m_downloadConfigFailedList.Add(m_currentConfig);
            }
        }
        void OnDestroy()
        {
            m_downloader.Close();
            if (m_md5File != null)
            {
                m_md5File.Destroy();
            }
            if (!default(SDownloadConfig).Equals(m_currentConfig))
            {
                Debug.Log(m_currentConfig.fileName + "没下载完.");
                List<SDownloadConfig> list = new List<SDownloadConfig>(m_md5File.NeedDownloadConfigQueue.ToArray());
                list.Insert(0, m_currentConfig);
                m_md5File.NeedDownloadConfigQueue = new Queue<SDownloadConfig>(list);
            }
        }



    }
}
