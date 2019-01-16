
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
    public enum LoadResPriority
    {
        RES_HIGH = 0,       //最高优先级
        RES_MIDDLE = 1,     //一般优先级
        RES_LOW = 2,        //低优先级
        RES_NUM = 3         //优先级等级个数
    }

    public class AsyncLoadAssetParam
    {
        public List<AsyncCallBack> m_callbackList = new List<AsyncCallBack>();
        public uint m_crc;
        public string m_path;
        public LoadResPriority m_priority = LoadResPriority.RES_LOW;

        public void Reset()
        {
            m_callbackList.Clear();
            m_crc = 0;
            m_path = "";
            m_priority = LoadResPriority.RES_LOW;
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
     object param2  = null, object param3 = null);

    public class ResourceMgr : TDSingleton<ResourceMgr>
    {
        public bool m_loadFromAssetBundle = true; //是否从AssetBundle中加载资源

        #region 字段和属性
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
         new List<AsyncLoadAssetParam>[(int)LoadResPriority.RES_NUM];
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
            for(int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                m_loadingAssetList[i] = new List<AsyncLoadAssetParam>();
            }
            m_startMono = mono;
            m_startMono.StartCoroutine(AsyncLoadAssetCoroutine()); //开启异步加载资源的协程
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
        //卸载资源
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
        //异步加载资源, 仅仅是不需要实例化的资源,音频,图片等
        public void AsyncLoadAsset<T>(string path, OnAsyncObjFinished dealFinished, 
         LoadResPriority priority, object param1 = null,
         object param2 = null, object param3 = null, uint crc = 0) where T : UnityEngine.Object
        {
            if(crc == 0)
            {
                crc = CrcHelper.StringToCRC32(path);
            }
            ResourceItem item = GetAssetFromAssetCache(crc);
            if(item != null)
            {
                if(dealFinished != null)
                {
                    dealFinished(path, item.Obj, param1, param2, param3);
                }
                return;
            }
            //判断是否在加载中
            AsyncLoadAssetParam para = null;
            if(!m_loadingAssetDict.TryGetValue(crc, out para) || para == null)
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
            while(true)
            {
                yield return null;
            }
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
            if (!AssetCacheDict.Remove(item.Crc))
            {
                return;
            }
            if (destroyCache == false)
            {
                m_noReferenceResourceItemMapList.Insert(item);
                return;
            }
            AssetBundleManager.Instance().UnLoadResourceItem(item);
            if (item.Obj != null)
            {
                item.Obj = null;
            }
        }
        #endregion
    }
}


