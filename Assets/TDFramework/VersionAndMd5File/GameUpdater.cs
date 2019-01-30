
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
// 模块描述(Module description):  游戏资源更新器,专门用于更新游戏的新资源
// ***************************************************************

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    //下载资源的回调结构体
    public struct SGameUpdaterCallback
    {
        //下载资源出现错误的回调方法, 参数string表示资源名称
        public Action<string> Error;
        //资源开始下载前的回调方法, 参数int表示GameUpdater一共需要更新多少个资源        
        public Action<int> Befor;
        //资源正在下载中的回调方法, 参数string表示正在下载的资源的名称, SDownloadFileResult表示资源正在下载的进度信息
        public Action<string, SDownloadFileResult> Progress;
        //某个资源下载完成的回调方法, 参数1 string表示下载完成的资源名称, 参数2 int表示当前下载完成的是第几个下载资源, 参数3 int表示GameUpdater一共要下载多少个资源
        public Action<string, int, int> OneComplete;
        //GameUpdater所有的资源都下载完成的回调方法
        public Action AllComplete;
        //表示没有资源需要更新的回调
        public Action NotNeedUpdate;
    }
    //下载资源的参数结构体
    public struct SGameUpdaterDownloadConfig
    {
        public string fileName;         //下载资源名称
        public string downloadUrl;      //下载资源地址url
        public string localUrl;         //本地该资源地址url
        public int downloadTimeout;     //下载资源超时时间
        public int downloadFailRetry;   //下载资源失败的尝试次数
    }

    public struct SDownloadEventResult
    {
        public SGameUpdaterDownloadConfig info;
        public string error;
        public SDownloadFileResult fileResult;
        public DownloadEventType eventType;

        public SDownloadEventResult(DownloadEventType eventType, SDownloadFileResult fileResult, SGameUpdaterDownloadConfig info, string error)
        {
            this.error = error;
            this.info = info;
            this.fileResult = fileResult;
            this.eventType = eventType;
        }
        public SDownloadEventResult(DownloadEventType eventType)
            : this(eventType, new SDownloadFileResult(), new SGameUpdaterDownloadConfig(), string.Empty)
        { }
        public SDownloadEventResult(DownloadEventType eventType, SDownloadFileResult fileResult)
            : this(eventType, fileResult, new SGameUpdaterDownloadConfig(), string.Empty)
        { }
        public SDownloadEventResult(DownloadEventType eventType, string error)
            : this(eventType, new SDownloadFileResult(), new SGameUpdaterDownloadConfig(), error)
        { }
        public SDownloadEventResult(DownloadEventType eventType, SGameUpdaterDownloadConfig info)
            : this(eventType, new SDownloadFileResult(), info, string.Empty)
        { }
    }

    public class GameUpdater
    {
        #region 字段
        private Md5File m_md5File = null;
        private List<SGameUpdaterDownloadConfig> m_downloadFailConfigList = null;  //所有需要下载的资源的失败汇总列表集合
        private float m_downloadTotalSize = 0;  //所有下载资源的总大小
        private int m_downloadedCount = 0;      //当前下载的是第几个资源
        private int m_downloadTotal = 0;        //一共需要下载多少个资源

        private Queue<SGameUpdaterDownloadConfig> m_downloadConfigQueue = null;     //所有需要下载的资源队列
        private IWebDownload m_downloader = null;   //资源下载器, 可以是UnityWeb, 也可以是HttpWeb, 这里我们使用UnityWeb
        private SGameUpdaterDownloadConfig m_currentConfig;    //当前正在下载的资源
        private SGameUpdaterCallback m_downloadCallback; //下载资源回调函数Table
        #endregion

        #region 构造方法
        public GameUpdater(SGameUpdaterCallback scallback)
        {
            m_downloadFailConfigList = new List<SGameUpdaterDownloadConfig>();
            m_downloader = new GameObject("GameUpdater").AddComponent<UnityWeb>();
            m_downloadCallback = scallback;
        }
        #endregion

        #region 方法
        //检查计算需要更新的资源文件和这些资源文件的大小总和, 并接着下载资源
        public void StartDownload()
        {
            new DownloadMd5File(DownloadMd5FileSuccssedCallback, () =>
            {
                if (m_downloadCallback.Error != null)
                {
                    m_downloadCallback.Error("md5File");       //=========================================================================================== Error回调
                }
                Debug.LogError("远程Md5File下载失败.");
            }).Download();
        }
        public void Download()
        {
            m_downloadConfigQueue = new Queue<SGameUpdaterDownloadConfig>((IEnumerable<SGameUpdaterDownloadConfig>)m_md5File.NeedDownloadConfigQueue);
            if (m_downloadConfigQueue == null || m_downloadConfigQueue.Count <= 0)
            {
                //没有资源需要更新
                if (m_downloadCallback.NotNeedUpdate != null)
                {
                    m_downloadCallback.NotNeedUpdate();          //========================================================================================== NotNeedUpdate回调
                }
                return;
            }
            m_downloadedCount = 0;
            m_downloadTotal = m_downloadConfigQueue.Count;
            if (m_downloadCallback.Befor != null)
            {
                m_downloadCallback.Befor(m_downloadTotal);
            }
            DownloadNext();
        }
        #endregion

        private void DownloadMd5FileSuccssedCallback(string content)
        {
            m_md5File = new Md5File(content);
            m_downloadConfigQueue = new Queue<SGameUpdaterDownloadConfig>((IEnumerable<SGameUpdaterDownloadConfig>)m_md5File.NeedDownloadConfigQueue);
            if (m_downloadConfigQueue == null || m_downloadConfigQueue.Count <= 0)
            {
                //没有资源需要更新
                if (m_downloadCallback.NotNeedUpdate != null)
                {
                    m_downloadCallback.NotNeedUpdate();          //========================================================================================== NotNeedUpdate回调
                }
                return;
            }
            DownloadTotalSize();        //先下载资源总大小
        }
        private void DownloadTotalSize()
        {
            DownloadSizeNext();
        }
        private void DownloadSizeNext()
        {
            if (m_downloadConfigQueue.Count <= 0)
            {
                Download();     //开始逐个下载资源内容
                return;
            }
            SGameUpdaterDownloadConfig config = m_downloadConfigQueue.Dequeue();
            m_downloader.DownloadFileSize(config.downloadUrl, config.downloadTimeout, (status, code, size) =>
            {
                if (status)
                {
                    m_downloadTotalSize += size;
                }
                else
                {
                    if (m_downloadCallback.Error != null)
                    {
                        m_downloadCallback.Error(config.fileName); //======================================================================================== Error回调
                    }
                    Debug.LogError(config.fileName + "资源, 在获取该资源文件内容大小的时候, 网络请求失败.");
                }
                DownloadSizeNext();
            });
        }
        private void DownloadNext()
        {
            if (m_downloadConfigQueue.Count == 0)
            {
                if (m_downloadFailConfigList.Count > 0)
                {
                    //有资源下载失败
                    for (int i = 0; i < m_downloadFailConfigList.Count; i++)
                    {
                        if (m_downloadCallback.Error != null)
                        {
                            m_downloadCallback.Error(m_downloadFailConfigList[i].fileName);
                        }
                        Debug.LogError("下载失败的资源文件: " + m_downloadFailConfigList[i].fileName);
                    }
                }
                else
                {
                    //资源全部下载成功
                    if(m_downloadCallback.AllComplete != null)
                    {
                        m_downloadCallback.AllComplete();       //=========================================================================================== AllComplete回调
                    }
                }
                return;
            }
            m_currentConfig = m_downloadConfigQueue.Dequeue();
            m_md5File.PushTempFile(m_currentConfig.fileName);
            m_downloader.DownloadFile(m_currentConfig.downloadUrl, m_currentConfig.localUrl,
            m_currentConfig.downloadTimeout,
            OnProgress,     //资源下载过程中会一直调用这个Progress
            OnComplete);    //资源下载完成后调用OnComplete
        }
        private void OnProgress(SDownloadFileResult result)
        {
            if(m_downloadCallback.Progress != null)
            {
                m_downloadCallback.Progress(m_currentConfig.fileName, result);
            }
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
                    string fileMd5 = Md5Helper.Md5File(m_currentConfig.localUrl + ".tmp");
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
            m_currentConfig = new SGameUpdaterDownloadConfig();
            DownloadNext();
        }
        private void DownloadSuccess()
        {
            m_downloadedCount++;
            string tmp = m_currentConfig.localUrl + ".tmp";
            if (File.Exists(tmp))
            {
                if (File.Exists(m_currentConfig.localUrl))
                    File.Delete(m_currentConfig.localUrl);
                File.Move(tmp, m_currentConfig.localUrl);
            }
            //将这个文件从下载中移除
            m_md5File.PopTmpFile(m_currentConfig.fileName);
            //下载完成 更新md5
            m_md5File.UpdateLocalMd5File(m_currentConfig.fileName);
            if(m_downloadCallback.OneComplete != null)
            {
                m_downloadCallback.OneComplete(m_currentConfig.fileName, m_downloadedCount, m_downloadTotal); //================================================= OneComplete回调
            }
        }
        private void DownloadFail(int code)
        {
            string tmp = m_currentConfig.localUrl + ".tmp";
            //code=0 超时 不需要删除 重试后会续传
            if (code != 0 && File.Exists(tmp))
            {
                File.Delete(tmp);
            }
            if (code != 404 && m_currentConfig.downloadFailRetry > 0)
            {
                m_currentConfig.downloadFailRetry --;
                //重新加入下载队列中
                m_downloadConfigQueue.Enqueue(m_currentConfig);
            }
            else
            {
                //已经无能为力
                Debug.LogError(m_currentConfig.fileName + " 下载失败 code:" + code);
                m_downloadFailConfigList.Add(m_currentConfig);
            }
        }
        void OnDestroy()
        {
            m_downloader.Close();
            if (m_md5File != null)
            {
                m_md5File.UpdateMd5File();
            }
            if (!default(SGameUpdaterDownloadConfig).Equals(m_currentConfig))
            {
                Debug.Log(m_currentConfig.fileName + "没下载完.");
                List<SGameUpdaterDownloadConfig> list = new List<SGameUpdaterDownloadConfig>(m_downloadConfigQueue.ToArray());
                list.Insert(0, m_currentConfig);
                m_md5File.NeedDownloadConfigQueue = new Queue<SGameUpdaterDownloadConfig>(list);
            }
        }
    }
}
