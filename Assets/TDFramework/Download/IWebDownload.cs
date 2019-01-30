



namespace TDFramework
{
    using System;

    public interface IWebDownload
    {
        /// 获取远程文件大小
        //代理参数1:bool表示操作是否正确获得结果
        //代理参数2:int表示网络请求返回的code编码
        //代理参数3:int表示网路请求返回文件的大小
        void DownloadFileSize(string url, int timeout, Action<bool, int, int> complete);
        /// 下载一个文件
        void DownloadFile(string download_url, string localPath_url, int timeout,
            Action<SDownloadFileResult> progress,
            Action<int> complete);

        void Close();
    }
}
