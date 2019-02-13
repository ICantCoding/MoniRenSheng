

namespace TDFramework.Network
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;

    using UnityEngine;

    public class NetworkServer
    {
        #region 字段和属性
        private TcpListener m_tcpListener = null;
        private int m_tcpPort = 0;  //tcp连接服务器端口号

        private Thread m_thread = null;
        #endregion

        #region 构造函数
        public NetworkServer()
        {

        }
        #endregion

        #region 方法
        public void Close()
        {
            if(m_tcpListener != null)
            {
                m_tcpListener.Stop();
                m_tcpListener = null;
            }
        }
        public bool Start(int tcpPort)
        {
            try
            {
                m_tcpPort = tcpPort;
                m_tcpListener = new TcpListener(IPAddress.Any, m_tcpPort);
                m_tcpListener.Start(50); //设置最大挂载连接数为50
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
            m_thread = new Thread(new ThreadStart(ThreadFunction));
            m_thread.Start();
            return true;
        }
        private Socket m_clientSocket = null;
        private void ThreadFunction()
        {
            while (true)
            {
                //确定是否有挂起的连接请求, Pending()返回true, 表示有从客户端来的连接请求
                if (m_tcpListener != null && m_tcpListener.Pending())
                {
                    m_clientSocket = m_tcpListener.AcceptSocket();
                    ReceiveData(m_clientSocket);
                }
                Thread.Sleep(1);
            }
        }
        private byte[] mTemp = new byte[0x2000];
        private void ReceiveData(Socket socket)
        {
            if (socket != null && socket.Connected == true)
            {
                try
                {
                    socket.BeginReceive(mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, socket);
                    Debug.Log("1111111111111");
                }
                catch (System.Exception e)
                {

                }
            }
        }
        void OnReceive(IAsyncResult result) {
            int bytes = 0;
            try{
                bytes = m_clientSocket.EndReceive(result);

            }catch(Exception exception) {
                Debug.LogError(exception.Message);
            }
            if(bytes <= 0){
                Debug.LogError("bytes "+ bytes);
            }else {
                uint num = (uint)bytes;
                try{
                    m_clientSocket.BeginReceive(mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, m_clientSocket);
                }catch(Exception exception2) {
                }
            }
        }
        #endregion
    }
}
