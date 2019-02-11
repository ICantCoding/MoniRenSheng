


namespace TDFramework.Network
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;


    public class NetworkRecvRunLoop
    {
        #region 字段
        public bool over = false;
        private NetworkEngine m_networkEngine = null;
        #endregion

        #region 构造函数
        public NetworkRecvRunLoop(NetworkEngine engine)
        {
            m_networkEngine = engine;
        }
        #endregion

        public void run()
        {
            Debug.Log("Network Recv RunLoop start...");
            int count = 0;
        START_RUN:
            over = false;
            try
            {
                m_networkEngine.Process();
                count = 0;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                count++;
                if (count < 10)
                {
                    goto START_RUN;
                }
            }
            over = true;
            Debug.Log("Network Recv RunLoop end...");
        }
    }


    public class NetworkEngine : IMainLoop
    {
        #region 字段和属性
        private Thread m_networkRecvThread = null;
        public Thread NetworkRecvThread
        {
            get { return m_networkRecvThread; }
        }
        private NetworkRecvRunLoop m_networkRecvRunLoop = null;
        private NetworkInterface m_networkInterface = null;
        public NetworkInterface NetworkInterface
        {
            get { return m_networkInterface; }
        }
        //回调集合
        public List<System.Action> m_pendingCallbacks = new List<System.Action>();
        //是否中断网络引擎
        private bool m_isBreak = false;
        #endregion

        #region 构造函数
        public NetworkEngine()
        {
            m_networkInterface = new NetworkInterface(this);
            m_networkRecvRunLoop = new NetworkRecvRunLoop(this);
            m_networkRecvThread = new Thread(m_networkRecvRunLoop.run);
            m_networkRecvThread.Start();   //开始线程
        }
        #endregion

        #region 方法
        //死循环调用NetworkInterface的Process()， 用于一直接收网络数据
        public void Process()
        {
            while (m_isBreak != true)
            {
                m_networkInterface.Process();
            }
            Debug.Log("NetworkEngine.Process() Break, Break, Break!!!");
        }
        #endregion

        //在主线程中增加回调
        public void QueueInLoop(System.Action cb)
        {
            lock (this)
            {
                m_pendingCallbacks.Add(cb);
            }
        }
        // Main Thread Update, 每帧在主线程中执行完所有的回调
        public void UpdateInMainThread()
        {
            lock (this)
            {
                foreach (var callback in m_pendingCallbacks)
                {
                    callback();
                }
                m_pendingCallbacks.Clear();
            }
        }

    }
}
