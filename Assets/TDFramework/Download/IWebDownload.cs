



namespace TDFramework.Download
{
    using System;

    public interface IWebDownload
    {
        /// 获取远程文件大小
        void DownloadFileSize(string url, int timeout, Action<bool, int, int> complete);
        /// 下载一个文件
        void DownloadFile(string download_url, string localPath_url, int timeout,
            Action<SDownloadFileResult> progress,
            Action<int> complete);

        void Close();
    }
}
