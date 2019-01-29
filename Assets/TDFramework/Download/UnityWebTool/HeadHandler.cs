

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class HeadHandler : DownloadHandlerScript
    {
        public int ContentLegth {get; private set;}

        //文件数据长度
        protected override void ReceiveContentLength(int contentLength)
        {
            ContentLegth = contentLength;
        }
        //接收到数据时
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            return true;
        }
        public new void Dispose()
        {

        }
    }
}
