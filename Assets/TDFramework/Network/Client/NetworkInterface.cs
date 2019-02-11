
namespace TDFramework.Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public delegate void MessageHandler(Packet msg);

    public class NetworkInterface
    {
        #region 字段和属性
        private static ManualResetEvent TimeoutManualResetEvent = new ManualResetEvent(false); //初始没有信号量, 即出现阻塞
        private Socket m_clientSocket;
        public Socket ClientSocket
        {
            get { return m_clientSocket; }
        }
        private MessageReader msgReader = new MessageReader();
        private static byte[] m_datas = new byte[MemoryStream.BUFFER_MAX];
        //请求报文接收响应flowId对应消息的处理回调
        public Dictionary<uint, MessageHandler> flowHandlers = new Dictionary<uint, MessageHandler>();
        #endregion

        #region 构造函数
        public NetworkInterface(NetworkEngine engine)
        {
            msgReader.MainLoop = engine;
        }
        #endregion

        #region 方法
        //客户端Socket请求连接服务器
        public bool Connect(string ip, int port)
        {
            int tryCount = 3; //连接尝试次数, 遇见连接错误我们才去重新请求连接
        __RETRY:
            Reset();
            TimeoutManualResetEvent.Reset();

            m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_clientSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, MemoryStream.BUFFER_MAX);
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                m_clientSocket.BeginConnect(endPoint, new AsyncCallback(ClientConnectCallback), m_clientSocket);
                //如果当前实例收到信号，则为 true, 否则为 false. 超过10s还未收到信号，返回false
                if (TimeoutManualResetEvent.WaitOne(10000))  
                {
                    if (Valid() == false)
                    {
                        #warning try client socket connected.
                    }
                }else{
                    Reset();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                if (tryCount > 0)
                {
                    Debug.Log("connect(" + ip + ":" + port + ") is error, try=" + (tryCount--) + "!");
                    goto __RETRY;
                }
            }
            return Valid();
        }
        //发送数据包
        public void Send(byte[] datas, uint flowId, MessageHandler handler)
        {
            if(Valid() == false)
            {
                Debug.Log("Client Socket Disconnected.");
                return;
            }
            if(datas == null || datas.Length == 0)
            {
                Debug.Log("Client Socket Send Data is null.");
                return;
            }
            //添加接收响应报文时候的回调函数
            flowHandlers[flowId] = handler == null ? p => {} : handler;
            try
            {
                m_clientSocket.Send(datas);
            }
            catch(SocketException e)
            {
                if(e.ErrorCode == 10054 || e.ErrorCode == 10053)
                {
                    Debug.Log(string.Format("Client Socket Send Datas, disable connect!"));
                    if(Valid())
                    {
                        m_clientSocket.Close();
                    }
                    m_clientSocket = null;
                }else
                {
                    Debug.Log(string.Format("Client Socket Send Datas, socket error(" + e.ErrorCode + ")!"));
                }
            }
        }
        //接收数据包
        public void Recv()
        {
            if(Valid() == false)
            {
                Debug.Log("Client Socket Disconnected.");
                return;
            }
            if(m_clientSocket.Poll(0, SelectMode.SelectRead)) //避免recv方法阻塞, Poll是否会阻塞？
            {
                if(Valid() == false)
                {
                    Debug.Log("Client Socket Disconnected.");
                    return;
                }
                int successReceiveBytes = 0;
                try
                {
                    successReceiveBytes = m_clientSocket.Receive(m_datas, MemoryStream.BUFFER_MAX, 0);
                }
                catch(SocketException e)
                {
                    if(e.ErrorCode == 10054 || e.ErrorCode == 10053)
                    {
                        Debug.Log(string.Format("Client Socket Recv Disable Connected!"));
                        if(Valid())
                        {
                            m_clientSocket.Close();
                        }
                        m_clientSocket = null;
                    }else
                    {
                        Debug.Log(string.Format("Client Socket Recv Datas, socket error(" + e.ErrorCode + ")!"));
                    }
                    return;
                }
                if(successReceiveBytes > 0)
                {

                }
                else if(successReceiveBytes == 0)
                {
                    Debug.Log(string.Format("Client Socket Recv Datas, Disconnected!"));
                    if(Valid())
                    {
                        m_clientSocket.Close();
                    }
                    m_clientSocket = null;
                }
                else
                {
                    Debug.Log(string.Format("Client Socket Recv Datas, socket error!"));
                    if(Valid())
                    {
                        m_clientSocket.Close();
                    }
                    m_clientSocket = null;
                    return;
                }
                Debug.Log("Client Socket Success Received Data: " + successReceiveBytes);
                msgReader.Process(m_datas, (System.UInt32)successReceiveBytes, flowHandlers);
            }
        }
        public void Process() 
		{
			if(Valid())
			{
				Recv();
			}
			else
			{
                //网络连接断开避免子线程死循环卡死
				System.Threading.Thread.Sleep(50);
			}
		}
        //检查m_clientSocket是否已经连接
        public bool Valid()
        {
            return ((m_clientSocket != null) && (m_clientSocket.Connected == true));
        }
        //关闭Socket
        public void Close()
        {
            try
            {
                m_clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Debug.LogError("Close Client Socket Error " + e);
            }
            m_clientSocket.Close(0);
            m_clientSocket = null;
        }
        //重置Socket
        public void Reset()
        {
            if (Valid() == true)
            {
                Close();
            }
            msgReader = new MessageReader();
            TimeoutManualResetEvent.Set();
        }
        #endregion

        #region 回调方法实现
        public void ClientConnectCallback(IAsyncResult ar)
        {
            if(Valid())
            {
                m_clientSocket.EndConnect(ar);
            }else
            {
                #warning 需要尝试客户端重新连接
            }
            TimeoutManualResetEvent.Set();
        }
        #endregion


    }
}