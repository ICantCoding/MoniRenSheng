

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.Utils;


    public class ObjectManager : Singleton<ObjectManager>
    {
        #region 类对象池相关
        #region 字段和属性
        private Dictionary<Type, object> m_classObjectPoolDict = new Dictionary<Type, object>();
        #endregion

        #region 方法
        public ClassObjectPool<T> GetOrCreateClassObjectPool<T>(int maxCount)
            where T : class, new()
        {
            Type type = typeof(T);
            object obj = null;
            if (!m_classObjectPoolDict.TryGetValue(type, out obj) || obj == null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
                m_classObjectPoolDict.Add(type, newPool);
                return newPool;
            }
            return obj as ClassObjectPool<T>;
        }
        #endregion
        #endregion

        #region 对象池相关
        #region 字段和属性
        public Transform m_goPool; //对象池父物体节点
        public Transform m_sceneGos;
        //对象池
        public Dictionary<uint, List<GameObjectItem>> m_gameObjectItemPoolDict = new Dictionary<uint, List<GameObjectItem>>();
        //GameObjectItem的类对象池
        protected ClassObjectPool<GameObjectItem> m_gameObjectItemClassPool = null;
        //GameObjectItem的Guid为Key, GameObjectItem实例为对象的字典集合
        protected Dictionary<long, GameObjectItem> m_gameObjectItemDict = new Dictionary<long, GameObjectItem>();
        //异步加载的GameObjectItem统计， key为Guid
        protected Dictionary<long, GameObjectItem> m_asyncGameObjectItemDict = new Dictionary<long, GameObjectItem>(); 
        #endregion

        #region 方法
        public void InitGoPool(Transform goPool, Transform sceneGos)
        {
            m_gameObjectItemClassPool = ObjectManager.Instance.GetOrCreateClassObjectPool<GameObjectItem>(1000);
            m_goPool = goPool;
            m_sceneGos = sceneGos;
        }
        //是否正在异步加载
        public bool IsAsyncLoading(long guid)
        {
            return m_asyncGameObjectItemDict[guid] != null;
        }
        //判断是否GameObject是否是ObjectManager创建，是否是对象池形式创建的. 
        public bool IsObjectManagerCreated(GameObject go)
        {
            GameObjectItem item = m_gameObjectItemDict[go.GetInstanceID()];
            return item != null; 
        }
        //清空对象池
        public void ClearCache()
        {
            List<uint> tempList = new List<uint>();
            foreach(uint key in m_gameObjectItemPoolDict.Keys)
            {
                List<GameObjectItem> list = m_gameObjectItemPoolDict[key];
                for(int i = list.Count - 1; i >= 0; --i)
                {
                    GameObjectItem item = list[i];
                    if(System.Object.ReferenceEquals(item.Obj, null) && item.Clear)
                    {
                        list.Remove(item);                        
                        GameObject.Destroy(item.Obj);
                        m_gameObjectItemDict.Remove(item.Obj.GetInstanceID());
                        item.Reset();
                        m_gameObjectItemClassPool.Recycle(item);
                    }
                }
                if(list.Count <= 0)
                {
                    tempList.Add(key);
                }
            }
            for(int i = 0; i < tempList.Count; i++)
            {
                uint temp = tempList[i];
                if(m_gameObjectItemPoolDict.ContainsKey(temp))
                {
                    m_gameObjectItemPoolDict.Remove(temp);
                }
            }
            tempList.Clear();
            tempList = null;
        }
        //清除某个资源在对象池中的所有对象
        public void ClearPoolObject(uint crc)
        {
            List<GameObjectItem> st = null;
            if(m_gameObjectItemPoolDict.TryGetValue(crc, out st) == false || st == null)
            {
                return;
            }
            for(int i = st.Count - 1; i >= 0; --i)
            {
                GameObjectItem item = st[i];
                if(item.Clear)
                {
                    st.Remove(item);
                    int tempId = item.Obj.GetInstanceID();
                    GameObject.Destroy(item.Obj);
                    item.Reset();
                    m_gameObjectItemDict.Remove(tempId);
                    m_gameObjectItemClassPool.Recycle(item);
                }
            }
            if(st.Count <= 0)
            {
                m_gameObjectItemPoolDict.Remove(crc);
            }
        }
        //取消某个GameObject的异步加载
        public void CancelAsyncLoad(long guid)
        {
            GameObjectItem item = null;
            if(m_asyncGameObjectItemDict.TryGetValue(guid, out item) && item != null && ResourceMgr.Instance.CancelAsyncLoad(item))
            {
                m_asyncGameObjectItemDict.Remove(guid);
                item.Reset();
                m_gameObjectItemClassPool.Recycle(item);
            }
        }
        //预加载GameObject对象, 路径, 预加载个数, 跳场景是否清除
        public void Preload(string path, int count = -1, bool clear = false)
        {
            List<GameObject> tempGameObjectList = new List<GameObject>();
            for(int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(path, false, clear);
                tempGameObjectList.Add(obj);
            }
            for(int i = 0; i < count; i++)
            {
                GameObject obj = tempGameObjectList[i];
                ReleaseGameObjectItem(obj);
                obj = null;
            }
            tempGameObjectList.Clear();
        }
        //参数3：资源在跳转场景是否需要清空
        public GameObject Instantiate(string path, bool setSceneObj = false, bool bClear = true)
        {
            uint crc = CrcHelper.StringToCRC32(path);
            //先尝试从缓存中取实例化Obj
            GameObjectItem gameObjectItem = GetGameObjectItemFromPool(crc);
            if (gameObjectItem == null)
            {
                gameObjectItem = m_gameObjectItemClassPool.Spawn(true);
                gameObjectItem.Crc = crc;
                gameObjectItem.Clear = bClear;
                ResourceMgr.Instance.LoadGameObjectItem(path, gameObjectItem);
                if (gameObjectItem.ResourceItem.Obj != null)
                {
                    gameObjectItem.Obj = GameObject.Instantiate(gameObjectItem.ResourceItem.Obj) as GameObject;
                    gameObjectItem.OfflineData = gameObjectItem.Obj.GetComponent<OfflineData>();
                }
            }
            if (setSceneObj)
            {
                gameObjectItem.Obj.transform.SetParent(m_sceneGos, false);
            }
            gameObjectItem.Guid = gameObjectItem.Obj.GetInstanceID();
            if (!m_gameObjectItemDict.ContainsKey(gameObjectItem.Guid))
            {
                m_gameObjectItemDict.Add(gameObjectItem.Guid, gameObjectItem);
            }
            return gameObjectItem.Obj;
        }
        //GameObject异步加载资源
        public long InstantiateAsync(string path, OnAsyncResourceObjFinished dealFinish, LoadAssetPriority priority,
         bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null,
         bool bClear = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }
            uint crc = CrcHelper.StringToCRC32(path);
            GameObjectItem gameObjectItem = GetGameObjectItemFromPool(crc);
            if (gameObjectItem != null)
            {
                if (setSceneObject)
                {
                    gameObjectItem.Obj.transform.SetParent(m_sceneGos, false);
                }
                if (dealFinish != null)
                {
                    dealFinish(path, gameObjectItem.Obj, param1, param2, param3);
                }
                return gameObjectItem.Guid;
            }
            long guid = ResourceMgr.Instance.CreateGuid();
            gameObjectItem = m_gameObjectItemClassPool.Spawn(true);
            gameObjectItem.Crc = crc;
            gameObjectItem.SetSceneParent = setSceneObject;
            gameObjectItem.Clear = bClear;
            gameObjectItem.DealFinishCallback = dealFinish;
            gameObjectItem.Param1 = param1;
            gameObjectItem.Param2 = param2;
            gameObjectItem.Param3 = param3;
            //调用ResourceManager异步加载接口
            ResourceMgr.Instance.AsyncLoadGameObjectItem(path, gameObjectItem, OnAsyncLoadGameObjectFinish, priority);
            return guid;
        }
        //GameObject异步加载资源ResourceItem成功后的回调
        private void OnAsyncLoadGameObjectFinish(string path, GameObjectItem gameObjectItem,
         object param1 = null, object param2 = null, object param3 = null)
        {
            if (gameObjectItem == null) return;
            if (gameObjectItem.ResourceItem.Obj == null)
            {
#if UNITY_EDITOR
                Debug.LogError("异步资源加载的资源为空: " + path);
#endif
            }
            else
            {
                gameObjectItem.Obj = GameObject.Instantiate(gameObjectItem.ResourceItem.Obj) as GameObject;
                gameObjectItem.OfflineData = gameObjectItem.Obj.GetComponent<OfflineData>();
            }
            //加载完成， 就从正在加载的异步中移除
            if(m_asyncGameObjectItemDict.ContainsKey(gameObjectItem.Guid))
            {
                m_asyncGameObjectItemDict.Remove(gameObjectItem.Guid);
            }
            if(gameObjectItem.Obj != null && gameObjectItem.SetSceneParent)
            {
                gameObjectItem.Obj.transform.SetParent(m_sceneGos, false);
            }
            if(gameObjectItem.DealFinishCallback != null)
            {
                int guid = gameObjectItem.Obj.GetInstanceID();
                if(!m_gameObjectItemDict.ContainsKey(guid))
                {
                    m_gameObjectItemDict.Add(guid, gameObjectItem);
                }
                gameObjectItem.DealFinishCallback(path, gameObjectItem.Obj, 
                 gameObjectItem.Param1, gameObjectItem.Param2, gameObjectItem.Param3);
            }
        }
        //卸载资源
        public void ReleaseGameObjectItem(GameObject obj, int maxCacheCount = -1,
        bool destoryCache = false, bool recycleParent = true)
        {
            if (obj == null) return;
            GameObjectItem gameObjectItem = null;
            int tempGuid = obj.GetInstanceID();
            if (!m_gameObjectItemDict.TryGetValue(tempGuid, out gameObjectItem) || gameObjectItem == null)
            {
                Debug.Log(obj.name + "并非对象池技术创建,不能回收到对象池!");
                return;
            }
            if (gameObjectItem.AlreadyRelease)
            {
                Debug.LogError(obj.name + "该对象已经放回对象池, 检查是否清空该对象的引用!");
                return;
            }
#if UNITY_EDITOR
            obj.name += "(Recycle)";
#endif
            if (maxCacheCount == 0) //表示不缓存
            {
                m_gameObjectItemDict.Remove(tempGuid);
                GameObject.Destroy(gameObjectItem.Obj);
                ResourceMgr.Instance.UnLoadGameObjectItem(gameObjectItem, destoryCache);
                gameObjectItem.Reset();
                m_gameObjectItemClassPool.Recycle(gameObjectItem);
            }
            else
            {
                //回收到对象池
                List<GameObjectItem> list = null;
                if (!m_gameObjectItemPoolDict.TryGetValue(gameObjectItem.Crc, out list) || list == null)
                {
                    list = new List<GameObjectItem>();
                    m_gameObjectItemPoolDict.Add(gameObjectItem.Crc, list);
                }
                if (gameObjectItem.Obj)
                {
                    if (recycleParent)
                    {
                        gameObjectItem.Obj.transform.SetParent(m_goPool);
                    }
                    else
                    {
                        gameObjectItem.Obj.SetActive(false);
                    }
                }
                if (maxCacheCount < 0 || list.Count < maxCacheCount) //<0表示可以无限缓存, <maxCacheCount表示需要放入缓存
                {
                    list.Add(gameObjectItem);
                    gameObjectItem.AlreadyRelease = true;
                    ResourceMgr.Instance.DecrementResourceRef(gameObjectItem, 1);
                }
                else //不需要缓存GameObject到对象池
                {
                    m_gameObjectItemDict.Remove(tempGuid);
                    GameObject.Destroy(gameObjectItem.Obj);
                    ResourceMgr.Instance.UnLoadGameObjectItem(gameObjectItem);
                    gameObjectItem.Reset();
                    m_gameObjectItemClassPool.Recycle(gameObjectItem);
                }
            }
        }
        //从对象池中获取对象
        public GameObjectItem GetGameObjectItemFromPool(uint crc)
        {
            List<GameObjectItem> list = null;
            if (m_gameObjectItemPoolDict.TryGetValue(crc, out list) && list != null && list.Count > 0)
            {
                ResourceMgr.Instance.IncrementResourceRef(crc, 1);
                GameObjectItem gameObjectItem = list[0];
                list.RemoveAt(0);
                GameObject go = gameObjectItem.Obj;
                if (!System.Object.ReferenceEquals(go, null))
                {
                    //离线数据还原
                    if(!System.Object.ReferenceEquals(gameObjectItem.OfflineData, null))
                    {
                        gameObjectItem.OfflineData.ResetProperty();
                    }
                    gameObjectItem.AlreadyRelease = false;
#if UNITY_EDITOR
                    if (go.name.EndsWith("(Recycle)"))
                    {
                        go.name = go.name.Replace("(Recycle)", "");
                    }
#endif
                }
                return gameObjectItem;
            }
            return null;
        }
        //获取离线数据OfflineData
        public OfflineData FindOfflineData(GameObject obj)
        {
            OfflineData offlineData = null;
            GameObjectItem item = null;
            if(m_gameObjectItemDict.TryGetValue(obj.GetInstanceID(), out item) && item != null)
            {
                offlineData = item.OfflineData;
            }
            return offlineData;
        }
        #endregion
        #endregion
    }
}
