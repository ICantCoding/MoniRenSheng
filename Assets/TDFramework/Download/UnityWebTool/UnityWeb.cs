
namespace TDFramework.Download
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class UnityWeb : MonoBehaviour, IWebDownload
    {

        #region 字段和属性
        Dictionary<string, RequestHandler> m_requestDict = new Dictionary<string, RequestHandler>();
        #endregion

        #region Unity生命周期
        void OnDestroy()
        {
            Dictionary<string, RequestHandler>.Enumerator e = m_requestDict.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.handler.Dispose();
                e.Current.Value.request.Abort();
            }
            e.Dispose();
            StopAllCoroutines();
            m_requestDict.Clear();
            m_requestDict = null;
        }
        #endregion

        #region IWebDownload接口实现
        public void DownloadFileSize(string url, int timeout, Action<bool, int, int> complete)
        {
            StartCoroutine(DownloadFileSizeHandler(url, timeout, complete));
        }
        IEnumerator DownloadFileSizeHandler(string url, int timeout, Action<bool, int, int> done)
        {
            HeadHandler handler = new HeadHandler();
            UnityWebRequest request = UnityWebRequest.Head(url);
            request.downloadHandler = handler;
            request.timeout = timeout;
            request.chunkedTransfer = true;
            request.disposeDownloadHandlerOnDispose = true;
            yield return request.SendWebRequest();
            if (done != null)
            {
                done(request.responseCode == 200, (int)request.responseCode, handler.ContentLegth);
            }
            request.Abort();
            request.Dispose();
        }
        public void DownloadFile(string download_url, string localPath_url, int timeout,
         Action<SDownloadFileResult> progress,
         Action<int> complete)
        {
            StartCoroutine(DownloadFileHandler(download_url, localPath_url, timeout, progress, complete));
        }
        IEnumerator DownloadFileHandler(string download_url, string localPath_url, int timeout,
         Action<SDownloadFileResult> progress, Action<int> complete)
        {
            UnityDownloadFileHandler handler = new UnityDownloadFileHandler(download_url, localPath_url);
            handler.Progress = progress;
            UnityWebRequest request = UnityWebRequest.Get(download_url);
            request.SetRequestHeader("Range", string.Format("bytes={0}-", handler.DownloadedLength));
            request.downloadHandler = handler;
            request.timeout = timeout;
            request.chunkedTransfer = true;
            request.disposeCertificateHandlerOnDispose = true;
            m_requestDict.Add(download_url, new RequestHandler(request, handler));
            yield return request.SendWebRequest();
            int code = (int)request.responseCode;
            Dispose(download_url);
            if (complete != null) complete(code);
        }
        public void Close()
        {
            StopAllCoroutines();
        }
        #endregion

        private void Dispose(string key)
        {
            RequestHandler result;
            if (m_requestDict.TryGetValue(key, out result))
            {
                result.handler.Dispose();
                result.request.Abort();
                m_requestDict.Remove(key);
            }
        }
    }
}
