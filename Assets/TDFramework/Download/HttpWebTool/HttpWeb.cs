
namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.IO;
    using System.Threading;
    using UnityEngine;

    public class HttpWeb : MonoBehaviour, IWebDownload
    {

        #region 状态字段和属性
        private bool m_isStop;
        #endregion  

        #region 字段和属性
        private HttpWebRequest m_request;
        private HttpWebResponse m_response;
        private SDownloadFileResult m_fileResult;
        private HttpDownloadFileHandler m_handler;
        private SWebDownloadParams m_currParams;
        private Queue<SWebDownloadEvent> m_eventQueue = new Queue<SWebDownloadEvent>();
        #endregion

        #region Unity生命周期
        void Update()
        {
            if (default(SDownloadFileResult).Equals(m_fileResult) == false)
            {
                if (m_currParams.OnProgress != null)
                {
                    m_currParams.OnProgress(m_fileResult); //每一帧都更新下载进度
                }
            }

            if (m_eventQueue.Count > 0)
            {
                SWebDownloadEvent e = m_eventQueue.Dequeue();
                switch (e.EventType)
                {
                    case DownloadEventType.OneComplete:
                        {
                            m_currParams.OnComplete((int)e.objs[0]);
                            break;
                        }
                    case DownloadEventType.SizeComplete:
                        {
                            m_currParams.OnSizeComplete((bool)e.objs[0], (int)e.objs[1], (int)e.objs[2]);
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        void OnDestroy()
        {
            m_isStop = true;
            if(m_response != null){
                m_response.Dispose();
            }
            if(m_request != null)
            {
                m_request.Abort();
            }
            if(m_handler != null)
            {
                m_handler.Dispose();
            }
        }
        #endregion

        #region IWebDownload接口实现
        //代理参数1:bool表示操作是否正确获得结果
        //代理参数2:int表示网络请求返回的code编码
        //代理参数3:int表示网路请求返回文件的大小
        public void DownloadFileSize(string url, int timeout, Action<bool, int, int> complete)
        {
            m_eventQueue.Clear();
            m_currParams.download_url = url;
            m_currParams.timeout = timeout;
            m_currParams.OnSizeComplete = complete;
            ThreadPool.SetMaxThreads(1, 1);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFileSizeCallback), null);
        }
        public void DownloadFileSizeCallback(object obj)
        {
            m_request = WebRequest.Create(m_currParams.download_url) as HttpWebRequest;
            m_request.Timeout = m_currParams.timeout;
            m_request.Proxy = WebRequest.DefaultWebProxy;
            try
            {
                m_response = m_request.GetResponse() as HttpWebResponse;
                m_eventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.SizeComplete,
                 true, m_response.StatusCode, (int)m_response.ContentLength));
            }
            catch (WebException e)
            {
                Debug.LogError(e.Message);
                int code = 0;
                m_response = e.Response as HttpWebResponse;
                if (m_response != null)
                {
                    code = (int)m_response.StatusCode;
                }
                m_eventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.SizeComplete,
                 false, code, 0));
            }
            finally
            {
                if (m_response != null)
                {
                    m_response.Close();
                }
                m_request.Abort();
            }
        }
        public void DownloadFile(string download_url, string localPath_url, int timeout,
            Action<SDownloadFileResult> progress,
            Action<int> complete)
        {
            m_eventQueue.Clear();
            m_currParams.download_url = download_url;
            m_currParams.localPath_url = localPath_url;
            m_currParams.timeout = timeout;
            m_currParams.OnProgress = progress;
            m_currParams.OnComplete = complete;
            m_fileResult = new SDownloadFileResult();
            ThreadPool.SetMaxThreads(5, 5);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFileCallback), null);
        }
        private void DownloadFileCallback(object obj)
        {
            m_handler = new HttpDownloadFileHandler(m_currParams.download_url,
             m_currParams.localPath_url);
            m_handler.Progress = result => m_fileResult = result;
            m_request = WebRequest.Create(m_currParams.download_url) as HttpWebRequest;
            m_request.Timeout = m_currParams.timeout;
            m_request.AddRange(m_handler.DownloadedLength); //断点续传关键点
            m_request.Proxy = WebRequest.DefaultWebProxy;
            try
            {
                m_response = m_request.GetResponse() as HttpWebResponse;
                m_handler.ReceiveContentLength((int)m_response.ContentLength);
                byte[] buffer = new byte[1024];
                using (Stream stream = m_response.GetResponseStream())
                {
                    int dataLength = stream.Read(buffer, 0, buffer.Length);
                    while (dataLength > 0)
                    {
                        if (m_isStop || m_handler.ReceiveData(buffer, dataLength) == false) return;
                        dataLength = stream.Read(buffer, 0, buffer.Length);
                    }
                }
                m_handler.Dispose();
                m_eventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.OneComplete,
                 (int)m_response.StatusCode));
            }
            catch (WebException e)
            {
                int code = 0;
                m_response = e.Response as HttpWebResponse;
                if (m_response != null)
                {
                    code = (int)m_response.StatusCode;
                }
                m_handler.Dispose();
                m_eventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.OneComplete, code));
            }
            finally
            {
                if (m_response != null)
                {
                    m_response.Close();
                }
                m_request.Abort();
                m_fileResult = new SDownloadFileResult();
            }
        }
        public void Close()
        {
            m_isStop = true;
        }
        #endregion
    }
}