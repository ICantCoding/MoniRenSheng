

namespace TDFramework.Network
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Actor
    {
        #region 字段
        //邮箱 消息队列
        protected readonly ActorSynchronizationContext m_messageQueue = new ActorSynchronizationContext();
        public int m_Id; //地址
        protected bool m_isStop = false; //停止
        #endregion

        #region 构造函数
        public Actor()
        {

        }
        #endregion

        private async Task Dispatch()
        {
            while(!m_isStop)
            {
                
            }
        }
        public virtual void Init()
        {

        }
        public void Stop()
        {
            m_isStop = true;
        }
    }
}