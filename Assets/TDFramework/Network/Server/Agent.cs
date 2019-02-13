
//////////////////////////////////////////////////////////////////
//                            _ooOoo_                           //
//                           o8888888o                          //
//                           88" . "88                          //
//                           (| -_- |)                          //
//                           O\  =  /O                          //    
//                        ____/`---'\____                       //
//                      .'  \\|     |//  `.                     //
//                     /  \\|||  :  |||//  \                    //
//                    /  _||||| -:- |||||-  \                   //
//                    |   | \\\  -  /// |   |                   //
//                    | \_|  ''\---/''  |   |                   //
//                    \  .-\__  `-`  ___/-. /                   //
//                  ___`. .'  /--.--\  `. . __                  //
//               ."" '<  `.___\_<|>_/___.'  >'"".               //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |             //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /             //
//         ======`-.____`-.___\_____/___.-`____.-'======        //
//                            `=---='                           //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^         //
//           佛祖保佑 程序员一生平安,健康,快乐,没有Bug!            //
//////////////////////////////////////////////////////////////////

// ***************************************************************
// Copyright (C) 2017 The company name
//
// 文件名(File Name):             Agent.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-02-13 20:47:33
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************

namespace TDFramework.Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections;
    using System.Collections.Generic;

    public class Agent
    {
        private static uint maxId = 0;
        private const int bufferSize = 2048;

        #region 字段
        private uint m_id;               //客户端代理Id号
        private Socket m_socket;
        private bool m_isClose = false; //客户端代理是否关闭连接
        EndPoint m_endPoint;

        private byte[] m_buffer = new byte[bufferSize]; //接收数据缓存
        #endregion

        #region 属性
        public uint Id
        {
            get{return m_id;}
        }
        #endregion

        #region 构造函数
        public Agent(Socket socket)
        {
            m_id = ++maxId;
            m_socket = socket;
            m_endPoint = m_socket.RemoteEndPoint;
        }
        #endregion

        #region 方法
        public void StartReceive()
        {
            if (Valid())
            {
                try
                {
                    m_socket.BeginReceive(m_buffer, 0, bufferSize, SocketFlags.None, OnReceiveCallback, m_socket); //会开辟子线程, 该子线程阻塞, 等待数据流的到来
                }
                catch (Exception exception)
                {
                    //接收数据失败,就直接关闭掉该客户端的连接
                    Console.WriteLine(exception.Message);
                    Close();
                }
            }
        }
        private void OnReceiveCallback(IAsyncResult ar)
        {
            int bytes = 0;
            try
            {
                bytes = m_socket.EndReceive(ar);
            }
            catch (Exception exception)
            {
                //接收数据失败,就直接关闭掉该客户端的连接
                Console.WriteLine(exception.Message);
                Close();
            }
            if (bytes <= 0)
            {
                //接收数据的大小<=0, 就直接关闭到该客户端的连接
                Close();
            }
            else
            {
                uint num = (uint)bytes;
                try
                {
                    m_socket.BeginReceive(m_buffer, 0, bufferSize, SocketFlags.None, OnReceiveCallback, m_socket);
                }
                catch(Exception exception)
                {
                    //接收数据失败,就直接关闭掉该客户端的连接
                    Console.WriteLine(exception.Message);
                    Close();
                }
            }
        }

        //判断该客户端的Socket连接是否有效
        public bool Valid()
        {
            if (m_socket != null && m_socket.Connected)
            {
                return true;
            }
            return false;
        }
        //关闭该客户端的连接
        public void Close()
        {
            if (m_isClose)
            {
                return;
            }
            if (Valid())
            {
                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                    m_socket.Close();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            m_socket = null;
            m_isClose = true;
        }
        #endregion
    }
}
