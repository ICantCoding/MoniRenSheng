
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
        #endregion

        #region 公有方法
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


