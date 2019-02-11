
namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Net.Sockets;
    using System.Net;

    public class ServerThread
    {
        #region 字段和属性
        Socket m_serverSocket = null;
        #endregion

        #region 构造方法

        #endregion

        #region 方法
        public void InitListenSocket()
        {
            m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_serverSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000);
            m_serverSocket.Bind(ip);
            m_serverSocket.Listen(1);
        }
        public void CloseServerSocket()
        {
            m_serverSocket.Close();
        }
        #endregion
    }

    public class DemoServer : MonoBehaviour
    {
        #region 字段和属性
        public static DemoServer m_demoServer = null;
        public bool m_stop = false;
        #endregion

        #region 构造函数
        public DemoServer()
        {
            m_demoServer = this;
        }
        #endregion

        #region 方法
        public void ShutDown()
        {
            Debug.LogError("ShutDown Demo Server.");
            m_stop = true;
        }
        #endregion
    }
}