
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;
using PureMVC.Patterns.Mediator;

namespace PureMVC.Core
{
    public class View: IView
    {
        #region 字段和属性
        protected readonly ConcurrentDictionary<string, IMediator> mediatorMap;
        protected readonly ConcurrentDictionary<string, IList<IObserver>> observerMap;
        protected const string Singleton_MSG = "View Singleton already constructed!";
        #endregion
        
        #region 单例
        protected static IView instance;
        public static IView GetInstance(Func<IView> viewFunc)
        {
            if (instance == null) {
                instance = viewFunc();
            }
            return instance;
        }    
        #endregion

        #region 构造方法
        public View()
        {
            if (instance != null) throw new Exception(Singleton_MSG);
            instance = this;
            mediatorMap = new ConcurrentDictionary<string, IMediator>(); //发送消息的对象的字典
            observerMap = new ConcurrentDictionary<string, IList<IObserver>>(); //监听具体执行内容的字典
            InitializeView();
        }
        #endregion

        #region 方法
        protected virtual void InitializeView()
        {

        }
        public virtual void RegisterObserver(string notificationName, IObserver observer) //注册消息执行者
        {
            IList<IObserver> observers = null;
            if (observerMap.TryGetValue(notificationName, out observers))
            {
                observers.Add(observer);
            }
            else
            {
                observerMap.TryAdd(notificationName, new List<IObserver> { observer });
            }
        }
        public virtual void NotifyObservers(INotification notification) //接收到消息具体进行消息的操作执行
        {
            IList<IObserver> observers_ref = null;
            if (observerMap.TryGetValue(notification.Name, out observers_ref))
            {
                var observers = new List<IObserver>(observers_ref);
                foreach (IObserver observer in observers)
                {
                    observer.NotifyObserver(notification);
                }
            }
        }
        public virtual void RemoveObserver(string notificationName, object notifyContext)
        {
            IList<IObserver> observers = null;
            if (observerMap.TryGetValue(notificationName, out observers))
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    if (observers[i].CompareNotifyContext(notifyContext))
                    {
                        observers.RemoveAt(i);
                        break;
                    }
                }
                IList<IObserver> _ = null;
                if (observers.Count == 0)
                    observerMap.TryRemove(notificationName, out _);
            }
        }
        public virtual void RegisterMediator(IMediator mediator) 
        {
            if(mediatorMap.TryAdd(mediator.MediatorName, mediator))
            {
                string[] interests = mediator.ListNotificationInterests();
                if (interests.Length > 0)
                {
                    IObserver observer = new Observer(mediator.HandleNotification, mediator);
                    for (int i = 0; i < interests.Length; i++)
                    {
                        RegisterObserver(interests[i], observer);
                    }
                }
                mediator.OnRegister();
            }
        }
        public virtual IMediator RetrieveMediator(string mediatorName)
        {
            IMediator mediator = null;
            return mediatorMap.TryGetValue(mediatorName, out mediator) ? mediator : null;
        }
        public virtual IMediator RemoveMediator(string mediatorName)
        {
            IMediator mediator = null;
            if (mediatorMap.TryRemove(mediatorName, out mediator))
            {
                string[] interests = mediator.ListNotificationInterests();
                for (int i = 0; i < interests.Length; i++)
                {
                    RemoveObserver(interests[i], mediator);
                }
                mediator.OnRemove();
            }
            return mediator;
        }
        public virtual bool HasMediator(string mediatorName)
        {
            return mediatorMap.ContainsKey(mediatorName);
        }
        #endregion
    }
}
