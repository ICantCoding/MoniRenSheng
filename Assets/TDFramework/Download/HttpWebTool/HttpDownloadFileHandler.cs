

namespace TDFramework
{
    using System;
    using System.IO;
    using Utils;

    public class HttpDownloadFileHandler
    {
        public Action<SDownloadFileResult> Progress;
        public int ContentLength {get; private set;}
        public int DownloadedLength {get; private set;}

        private FileStream m_fileStream;
        private string m_download_url;
        private string m_localPath_url;
        private SDownloadFileResult m_downloadFileResult;

        public HttpDownloadFileHandler(string download_url, string localPath_url)
        {
            m_download_url = download_url;
            m_localPath_url = localPath_url;
            m_downloadFileResult = new SDownloadFileResult();
            string dir = Path.GetDirectoryName(localPath_url);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            m_fileStream = new FileStream(localPath_url + ".tmp", FileMode.Append, FileAccess.Write);
            DownloadedLength = (int)m_fileStream.Length;
            ContentLength = 0;
            m_downloadFileResult.downloadedLength = DownloadedLength;
            m_downloadFileResult.downloadedLengthStr = HumanRead.HumanReadableFilesize(DownloadedLength);
        }

        public void ReceiveContentLength(int contentLength)
        {
            ContentLength = contentLength + DownloadedLength;
            m_downloadFileResult.contentLength = ContentLength;
            m_downloadFileResult.contentLengthStr = HumanRead.HumanReadableFilesize(ContentLength);
        }

        public bool ReceiveData(byte[] data, int dataLength)
        {
            if(data == null || dataLength == 0 || m_fileStream == null)
            {
                return false;
            }
            m_fileStream.Write(data, 0, dataLength);
            DownloadedLength += dataLength;
            m_downloadFileResult.downloadedLength = DownloadedLength;
            m_downloadFileResult.downloadedLengthStr = HumanRead.HumanReadableFilesize(DownloadedLength);
            Progress(m_downloadFileResult);
            return true;
        }

        public void Dispose()
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