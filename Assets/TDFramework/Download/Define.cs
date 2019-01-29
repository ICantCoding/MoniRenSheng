
namespace TDFramework
{
    using System;
    using UnityEngine.Networking;
    
    public struct SWebDownloadParams
    {
        public string download_url;
        public string localPath_url;
        public int timeout;
        public Action<SDownloadFileResult> OnProgress;
        public Action<int> OnComplete;
        public Action<bool, int, int> OnSizeComplete;
    }

    public enum DownloadEventType
    {
        SizeComplete,
        Progress,
        OneComplete,
        AllComplete,
        Error,
    }

    public struct SWebDownloadEvent
    {
        public DownloadEventType EventType;
        public object[] objs;
        public SWebDownloadEvent(DownloadEventType type, params object[] objs)
        {
            EventType = type;
            this.objs = objs;
        }
    }

    public struct SDownloadFileResult
    {
        //文件总大小
        public int contentLength;
        //已下载的大小
        public int downloadedLength;
        //文件总大小字符串 （人类可读的）
        public string contentLengthStr;
        //已下载的大小字符串 （人类可读的）
        public string downloadedLengthStr;
    }

    public struct RequestHandler
    {
        public UnityWebRequest request;
        public UnityDownloadFileHandler handler;
        public RequestHandler(UnityWebRequest request, UnityDownloadFileHandler handler)
        {
            this.request = request;
            this.handler = handler;
        }
    }
}