
namespace TDFramework
{
    using System;
    using UnityEngine.Networking;
    

    #region ------------------------------ 结构体定义
    public struct SWebDownloadParams
    {
        public string download_url;
        public string localPath_url;
        public int timeout;
        public Action<SDownloadFileResult> OnProgress;
        public Action<int> OnComplete;
        public Action<bool, int, int> OnSizeComplete;
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
    #endregion

    #region ------------------------------------- 枚举
    public enum DownloadEventType
    {
        SizeComplete,           //资源大小获取完成
        Progress,               //资源进度
        OneComplete,            //一个资源下载完成
        AllComplete,            //所有资源下载完成
        Error,                  //资源下载出错
    }
    #endregion
}