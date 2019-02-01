


namespace TDFramework.Network
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;


    public class NetworkRunLoop
    {
        #region 字段
        public bool over = false;
        private NetworkInterface m_networkInterface = null;
        #endregion

        #region 构造函数
        public NetworkRunLoop(NetworkInterface networkInterface)
        {
            m_networkInterface = networkInterface;
        }
        #endregion

        public void run()
        {
            Debug.Log("Network RunLoop start...");
            int count = 0;
        START_RUN:
            over = false;
            try
            {
                m_networkInterface.Process();
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
            Debug.Log("Network RunLoop end...");
        }
    }


    public class NetworkEngine : IMainLoop
    {
        #region 字段和属性
        private Thread m_networkThread = null;
        public Thread NetworkThread
        {
            get{return m_networkThread;}
        }
        private NetworkRunLoop m_networkRunLoop = null;

        private NetworkInterface m_networkInterface = null;
        public NetworkInterface NetworkInterface
        {
            get { return m_networkInterface; }
        }
        //回调集合
        public List<System.Action> m_pendingCallbacks = new List<System.Action>();
        #endregion

        #region 构造函数
        public NetworkEngine()
        {
            m_networkInterface = new NetworkInterface(this);
            m_networkRunLoop = new NetworkRunLoop(m_networkInterface);
            m_networkThread = new Thread(m_networkRunLoop.run);
            m_networkThread.Start();   //开始线程
        }
        #endregion



        //在主线程中增加回调
        public void QueueInLoop(System.Action cb)
        {
            lock(this)
            {
                m_pendingCallbacks.Add(cb);
            }
        }
        // Main Thread Update, 每帧在主线程中执行完所有的回调
		public void UpdateInMainThread() {
			lock (this) {
				foreach(var callback in m_pendingCallbacks) {
					callback();
				}
				m_pendingCallbacks.Clear();
			}
		}




    }
}
