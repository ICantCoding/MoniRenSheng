
namespace TDFramework.Download
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine.Networking;
    using UnityEngine;
    using Utils;

    public class UnityDownloadFileHandler : DownloadHandlerScript
    {
        public Action<SDownloadFileResult> Progress;
        public int ContentLength {get; private set;}
        public int DownloadedLength {get; private set;}

        private FileStream m_fileStream;
        private string m_download_url;
        private string m_localPath_url;
        private SDownloadFileResult m_downloadFileResult;

        public UnityDownloadFileHandler(string download_url, string localPath_url) : base(new byte[1024 * 200])
        {
            m_download_url = download_url;
            m_localPath_url = localPath_url;
            m_downloadFileResult = new SDownloadFileResult();
            string dir = Path.GetDirectoryName(localPath_url);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            m_fileStream = new FileStream(m_localPath_url + ".tmp", FileMode.Append, FileAccess.Write);
            DownloadedLength = (int)m_fileStream.Length;
            ContentLength = 0;
            m_downloadFileResult.downloadedLength = DownloadedLength;
            m_downloadFileResult.downloadedLengthStr = HumanRead.HumanReadableFilesize(DownloadedLength);
        }
        protected override void ReceiveContentLength(int contentLength)
        {
            ContentLength = contentLength + DownloadedLength;
            m_downloadFileResult.contentLength = ContentLength;
            m_downloadFileResult.contentLengthStr = HumanRead.HumanReadableFilesize(ContentLength);
        }
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if(data == null || dataLength == 0 || m_fileStream == null)
                return false;
            m_fileStream.Write(data, 0, dataLength);
            DownloadedLength += dataLength;
            m_downloadFileResult.downloadedLength = DownloadedLength;
            m_downloadFileResult.downloadedLengthStr = HumanRead.HumanReadableFilesize(DownloadedLength);
            Progress(m_downloadFileResult);
            return true;
        }
        protected override void CompleteContent()
        {
            Debug.Log("download_url: " + m_download_url + "下载完毕!");
        }
        public new void Dispose()
        {
            if(m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
                m_fileStream = null;
            }
        }
    }
}