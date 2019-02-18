
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
            m_pendingQueue.Enqueue(() => d(state)); //将回调放入队列中
            if (Interlocked.Increment(ref m_pendingCount) == 1) //返回原子操作递增的结果值
            {
                m_syncContext.Post(Consume, null); //指定同步上下文的线程执行Consume方法， 在Consume方法中执行队列中的回调
            }
        }
        private void Consume(object state)
        {
            //Consume会在指定的m_syncContext上下文的线程中执行
            var surroundContext = Current; //Current就是m_syncContext
            try
            {
                SetSynchronizationContext(this);
                do
                {
                    Action a;
                    m_pendingQueue.TryDequeue(out a); //取出队列中所有方法，并顺序执行
                    a.Invoke();
                } while (Interlocked.Decrement(ref m_pendingCount) > 0);
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

