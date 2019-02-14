
namespace TDFramework.Network
{

    using System;
    using System.Collections.Concurrent;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    //SynchronizationContext在通讯中充当传输者的角色，实现功能就是一个线程和另外一个线程的通讯
    //用于保证所有的Task在同一个线程上执行
    public class ActorSynchronizationContext : SynchronizationContext
    {
        #region 字段
        private readonly SynchronizationContext m_syncContext;
        private readonly ConcurrentQueue<Action> m_pendingQueue = new ConcurrentQueue<Action>();
        private int m_pendingCount = 0;
        #endregion

        #region 构造函数
        public ActorSynchronizationContext(SynchronizationContext context = null)
        {
            m_syncContext = context ?? new SynchronizationContext();
        }
        #endregion

        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null)
            {
                throw new ArgumentNullException("SendOrPostCallback");
            }
            m_pendingQueue.Enqueue(() => d(state));
            if (Interlocked.Increment(ref m_pendingCount) == 1) //返回原子操作递增的结果值
            {
                m_syncContext.Post(Consume, null);
            }
        }
        private void Consume(object state)
        {
            var surroundContext = Current;
            try
            {
                SetSynchronizationContext(this);
                do
                {
                    Action a;
                    m_pendingQueue.TryDequeue(out a);
                    a.Invoke();
                } while (Interlocked.Decrement(ref m_pendingCount) > 0);
            }
            catch (System.Exception e)
            {

            }
            finally
            {
                SetSynchronizationContext(surroundContext);
            }
        }
        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException();
        }
        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}

