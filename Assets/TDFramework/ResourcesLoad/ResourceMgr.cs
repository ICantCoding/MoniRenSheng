
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
// 文件名(File Name):             ResourceMgr.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-15 22:00:44
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************


namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.TDStruct;
    using Utils;
    using TDFramework.TDDesignMode;

    //异步加载资源的优先等级
    public enum LoadAssetPriority
    {
        HIGH = 0,       //最高优先级
        MIDDLE = 1,     //一般优先级
        LOW = 2,        //低优先级
        NUM = 3         //优先级等级个数
    }

    public class AsyncLoadAssetParam
    {
        public List<AsyncCallBack> m_callbackList = new List<AsyncCallBack>();
        public uint m_crc;
        public string m_path;
        public bool m_Sprite = false;
        public LoadAssetPriority m_priority = LoadAssetPriority.LOW;

        public void Reset()
        {
            m_callbackList.Clear();
            m_crc = 0;
            m_path = "";
            m_Sprite = false;
            m_priority = LoadAssetPriority.LOW;
        }
    }

    public class AsyncCallBack
    {
        //加载完成的回调
        public OnAsyncObjFinished m_dealFinished = null;
        //回调参数
        public object m_param1 = null;
        public object m_param2 = null;
        public object m_param3 = null;

        public void Reset()
        {
            m_dealFinished = null;
            m_param1 = m_param2 = m_param3 = null;
        }
    }

    public delegate void OnAsyncObjFinished(string path, Object obj, object param1 = null,
     object param2 = null, object param3 = null);

    public class ResourceMgr : TDSingleton<ResourceMgr>
    {
        public bool m_loadFromAssetBundle = false; //是否从AssetBundle中加载资源

        #region 字段和属性
        private bool m_isInit = false;
        //最长连续卡着加载资源的时间,单位us
        private const long MAXLOADASSETTIME = 200000;
        //用来缓存已经加载过的ResourceItem
        public Dictionary<uint, ResourceItem> AssetCacheDict
        {
            get; set;
        } = new Dictionary<uint, ResourceItem>();
        //用来存储没有被引用的ResourceItem资源
        private TDMapList<ResourceItem> m_noReferenceResourceItemMapList = new TDMapList<ResourceItem>();
        private MonoBehaviour m_startMono;
        //正在异步加载的资源列表
        private List<AsyncLoadAssetParam>[] m_loadingAssetList =
         new List<AsyncLoadAssetParam>[(int)LoadAssetPriority.NUM];
        //正在异步加载的字典
        private Dictionary<uint, AsyncLoadAssetParam> m_loadingAssetDict =
         new Dictionary<uint, AsyncLoadAssetParam>();
        private ClassObjectPool<AsyncLoadAssetParam> m_asyncLoadAssetParamPool =
         ObjectManager.Instance().GetOrCreateClassObjectPool<AsyncLoadAssetParam>(50);
        private ClassObjectPool<AsyncCallBack> m_asyncCallBackPool =
         ObjectManager.Instance().GetOrCreateClassObjectPool<AsyncCallBack>(100);
        #endregion

        #region 公有方法
        public void Init(MonoBehaviour mono)
        {
            if (m_isInit == false)
            {
                for (int i = 0; i < (int)LoadAssetPriority.NUM; i++)
                {
                    m_loadingAssetList[i] = new List<AsyncLoadAssetParam>();
                }
                m_startMono = mono;
                m_startMono.StartCoroutine(AsyncLoadAssetCoroutine()); //开启异步加载资源的协程
                m_isInit = true;
            }
        }
        //同步加载资源,针对GameObject
        public GameObjectItem LoadGameObject(string path, GameObjectItem gameObjectItem)
        {
            if (gameObjectItem == null)
            {
                return null;
            }
            uint crc = gameObjectItem.Crc == 0 ? CrcHelper.StringToCRC32(path) : gameObjectItem.Crc;
            ResourceItem resouceItem = GetAssetFromAssetCache(crc);
            if (resouceItem != null)
            {
                gameObjectItem.ResourceItem = resouceItem;
                return gameObjectItem;
            }
            Object obj = null;
#if UNITY_EDITOR
            if(!m_loadFromAssetBundle)
            {
                resouceItem = AssetBundleManager.Instance().FindResourceItem(crc);
                if (resouceItem.Obj != null)
                {
                    obj = resouceItem.Obj as Object;
                }
                else
                {
                    obj = LoadAssetByEditor<Object>(path);
                }
            }
#endif
            if (obj == null)
            {
                resouceItem = AssetBundleManager.Instance().LoadResourceItem(crc);
                if (resouceItem != null && resouceItem.Ab != null)
                {
                    if (resouceItem.Obj != null)
                    {
                        obj = resouceItem.Obj as Object;
                    }
                    else
                    {
                        obj = resouceItem.Ab.LoadAsset<Object>(resouceItem.AssetName);
                    }
                }
            }
            CacheAsset2AssetCache(path, ref resouceItem, crc, obj);
            resouceItem.Clear = gameObjectItem.Clear;
            gameObjectItem.ResourceItem = resouceItem;
            return gameObjectItem;
        }
        //卸载资源,针对GameObject
        public void UnLoadGameObjectItem(GameObjectItem gameObjectItem, bool destoryCache = false)
        {
            if(gameObjectItem == null) return;
            UnLoadAsset(gameObjectItem.ResourceItem.Obj, destoryCache);
        }
        //同步资源加载,仅加载不需要实例化的资源(纹理图片,音频,视频,Prefab等)
        public T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            uint crc = CrcHelper.StringToCRC32(path);
            ResourceItem item = GetAssetFromAssetCache(crc);
            if (item != null)
            {
                return item.Obj as T;
            }
            T obj = null;
#if UNITY_EDITOR
            if (!m_loadFromAssetBundle)
            {
                item = AssetBundleManager.Instance().FindResourceItem(crc);
                if (item.Obj != null)
                {
                    obj = item.Obj as T;
                }
                else
                {
                    obj = LoadAssetByEditor<T>(path);
                }
            }
#endif
            if (obj == null)
            {
                item = AssetBundleManager.Instance().LoadResourceItem(crc);
                if (item != null && item.Ab != null)
                {
                    if (item.Obj != null)
                    {
                        obj = item.Obj as T;
                    }
                    else
                    {
                        obj = item.Ab.LoadAsset<T>(item.AssetName);
                    }
                }
            }
            CacheAsset2AssetCache(path, ref item, crc, obj);
            return obj;
        }
        //根据资源对象卸载资源
        public void UnLoadAsset(Object obj, bool destroyObj = false)
        {
            if (obj == null) return;
            ResourceItem item = null;
            foreach (ResourceItem resourceItem in AssetCacheDict.Values)
            {
                if (resourceItem.Guid == obj.GetInstanceID())
                {
                    item = resourceItem;
                    break;
                }
            }
            if (item == null)
            {
                return;
            }
            item.RefCount--;
            DestroyResourceItem(item, destroyObj);
        }
        //根据路径卸载资源
        public void UnloadAsset(string path, bool destroyObj = false)
        {
            if (string.IsNullOrEmpty(path)) return;
            uint crc = CrcHelper.StringToCRC32(path);
            ResourceItem item = null;
            if (!AssetCacheDict.TryGetValue(crc, out item) || item == null)
            {
                return;
            }
            if (item == null)
            {
                return;
            }
            item.RefCount--;
            DestroyResourceItem(item, destroyObj);
        }
        //异步加载资源, 仅仅是不需要实例化的资源,音频,图片等
        public void AsyncLoadAsset<T>(string path, OnAsyncObjFinished dealFinished,
         LoadAssetPriority priority, object param1 = null,
         object param2 = null, object param3 = null, uint crc = 0) where T : UnityEngine.Object
        {
            if (crc == 0)
            {
                crc = CrcHelper.StringToCRC32(path);
            }
            ResourceItem item = GetAssetFromAssetCache(crc);
            if (item != null)
            {
                if (dealFinished != null)
                {
                    dealFinished(path, item.Obj, param1, param2, param3);
                }
                return;
            }
            //判断是否在加载中
            AsyncLoadAssetParam para = null;
            if (!m_loadingAssetDict.TryGetValue(crc, out para) || para == null)
            {
                para = m_asyncLoadAssetParamPool.Spawn(true);
                para.m_crc = crc;
                para.m_path = path;
                para.m_priority = priority;
                m_loadingAssetDict.Add(crc, para);
                m_loadingAssetList[(int)priority].Add(para);
            }
            AsyncCallBack callback = m_asyncCallBackPool.Spawn(true);
            callback.m_dealFinished = dealFinished;
            callback.m_param1 = param1;
            callback.m_param2 = param2;
            callback.m_param3 = param3;
            para.m_callbackList.Add(callback);
        }
        //异步加载资源协程
        IEnumerator AsyncLoadAssetCoroutine()
        {
            List<AsyncCallBack> callbackList = null;
            long lastYieldTime = System.DateTime.Now.Ticks;
            while (true)
            {
                bool haveYield = false;
                for (int i = 0; i < (int)LoadAssetPriority.NUM; i++)
                {
                    List<AsyncLoadAssetParam> loadingList = m_loadingAssetList[i];
                    if (loadingList.Count <= 0)
                        continue;
                    //取出第一个要加载的资源参数
                    AsyncLoadAssetParam param = loadingList[0];
                    loadingList.RemoveAt(0);
                    callbackList = param.m_callbackList;

                    Object obj = null;
                    ResourceItem item = null;
#if UNITY_EDITOR
                    if (!m_loadFromAssetBundle)
                    {
                        obj = LoadAssetByEditor<Object>(param.m_path);
                        //模拟异步加载
                        yield return new WaitForSeconds(0.5f);
                        item = AssetBundleManager.Instance().FindResourceItem(param.m_crc);
                    }
#endif
                    if (obj == null)
                    {
                        item = AssetBundleManager.Instance().LoadResourceItem(param.m_crc);
                        if (item != null && item.Ab != null)
                        {
                            AssetBundleRequest request = null;
                            if (param.m_Sprite) //图片资源需要特殊处理,因为object不能转Sprite
                            {
                                request = item.Ab.LoadAssetAsync<Sprite>(item.AssetName);
                            }
                            else
                            {
                                request = item.Ab.LoadAssetAsync<Object>(item.AssetName);
                            }
                            yield return request;
                            if (request.isDone)
                            {
                                obj = request.asset;
                            }
                            lastYieldTime = System.DateTime.Now.Ticks;
                        }
                    }
                    CacheAsset2AssetCache(param.m_path, ref item, param.m_crc, obj, callbackList.Count);
                    for (int j = 0; j < callbackList.Count; j++)
                    {
                        AsyncCallBack callback = callbackList[j];
                        if (callback != null && callback.m_dealFinished != null)
                        {
                            callback.m_dealFinished(param.m_path, obj, callback.m_param1, callback.m_param2, callback.m_param3);
                            callback.m_dealFinished = null;
                        }
                        callback.Reset();
                        m_asyncCallBackPool.Recycle(callback);
                    }
                    obj = null;
                    callbackList.Clear();
                    m_loadingAssetDict.Remove(param.m_crc);
                    param.Reset();
                    m_asyncLoadAssetParamPool.Recycle(param);

                    if (System.DateTime.Now.Ticks - lastYieldTime > MAXLOADASSETTIME)
                    {
                        yield return null;
                        lastYieldTime = System.DateTime.Now.Ticks;
                        haveYield = true;
                    }
                }
                if (!haveYield || System.DateTime.Now.Ticks - lastYieldTime > MAXLOADASSETTIME)
                {
                    lastYieldTime = System.DateTime.Now.Ticks;
                    yield return null;
                }
            }
        }
        //预加载
        public void PreLoadAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            uint crc = CrcHelper.StringToCRC32(path);
            ResourceItem item = GetAssetFromAssetCache(crc);
            if (item != null)
            {
                return;
            }
            Object obj = null;
#if UNITY_EDITOR
            if (!m_loadFromAssetBundle)
            {
                item = AssetBundleManager.Instance().FindResourceItem(crc);
                if (item.Obj != null)
                {
                    obj = item.Obj;
                }
                else
                {
                    obj = LoadAssetByEditor<Object>(path);
                }
            }
#endif
            if (obj == null)
            {
                item = AssetBundleManager.Instance().LoadResourceItem(crc);
                if (item != null && item.Ab != null)
                {
                    if (item.Obj != null)
                    {
                        obj = item.Obj;
                    }
                    else
                    {
                        obj = item.Ab.LoadAsset<Object>(item.AssetName);
                    }
                }
            }
            CacheAsset2AssetCache(path, ref item, crc, obj);
            //跳场景不清空缓存
            item.Clear = false;
            UnLoadAsset(obj, false); //预加载,放入缓存
        }
        //清空缓存, 当跳转场景时,有些资源需要清空,有些资源又不需要清空
        public void ClearCache()
        {
            List<ResourceItem> tempList = new List<ResourceItem>();
            foreach (ResourceItem item in AssetCacheDict.Values)
            {
                if (item.Clear)
                {
                    tempList.Add(item);
                }
            }
            foreach (ResourceItem item in tempList)
            {
                DestroyResourceItem(item, true);
            }
            tempList.Clear();
            tempList = null;
        }
        #endregion

        #region 私有辅助方法
        //从缓存获取资源
        private ResourceItem GetAssetFromAssetCache(uint crc, int addRefcount = 1)
        {
            ResourceItem item = null;
            if (AssetCacheDict.TryGetValue(crc, out item))
            {
                if (item != null)
                {
                    item.RefCount++;
                    item.LastUseTime = Time.realtimeSinceStartup;
                }
            }
            return item;
        }
        //资源放入缓存
        private void CacheAsset2AssetCache(string path, ref ResourceItem item, uint crc, Object obj, int addRefcount = 1)
        {
            WashOut();

            if (item == null || obj == null)
            {
                return;
            }
            item.Obj = obj;
            item.Guid = obj.GetInstanceID(); //获取资源唯一Id
            item.LastUseTime = Time.realtimeSinceStartup;
            item.RefCount += addRefcount;
            if (AssetCacheDict.ContainsKey(crc) == false)
            {
                AssetCacheDict.Add(crc, item);
            }
        }
#if UNITY_EDITOR
        //从编辑器加载资源
        private T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
        //缓存太多, 清除最早没有使用的资源
        private void WashOut()
        {
#warning 未处理
            // //当当前内存使用大于80%, 进行清除最早没有使用的资源
            // if(m_noReferenceResourceItemMapList.Size() <= 0)
            // {
            //     return;
            // }
            // ResourceItem item = m_noReferenceResourceItemMapList.Back();
            // DestroyResourceItem(item, true);
            // m_noReferenceResourceItemMapList.Pop();
        }
        //回收资源
        private void DestroyResourceItem(ResourceItem item, bool destroyCache = false)
        {
            if (item == null || item.RefCount > 0)
            {
                return;
            }
            if (destroyCache == false)
            {
                m_noReferenceResourceItemMapList.Insert(item);
                return;
            }
            if (!AssetCacheDict.Remove(item.Crc))
            {
                return;
            }
            AssetBundleManager.Instance().UnLoadResourceItem(item);
            if (item.Obj != null)
            {
                item.Obj = null;
#if UNITY_EDITOR
                // Resources.UnloadAsset(item.Obj); //这样子卸载不了Editor加载的资源(非通过AssetBundle方式)
                Resources.UnloadUnusedAssets();
#endif
            }
        }
        #endregion
    }
}


