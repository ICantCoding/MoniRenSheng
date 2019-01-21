

namespace TDFramework
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.TDDesignMode;
    using TDFramework.Utils;

    //ResourceItem中包含AssetBundle
    public class ResourceItem
    {
        //资源路径的crc
        public uint Crc = 0;
        //资源名字
        public string AssetName = string.Empty;
        //资源所在AssetBundle
        public string ABName = string.Empty;
        //资源依赖的AssetBundle
        public List<string> DependABList = null;
        //资源的加载完的AB包
        public AssetBundle Ab = null;

        //资源对象本身,从AB包中加载出来
        public Object Obj = null;
        //资源唯一标识
        public int Guid = 0;
        //最后使用该资源的时间
        public float LastUseTime = 0.0f;
        //跳转场景时,该资源是否需要清除
        public bool Clear = false;
        //该资源的引用计数
        private int refCount = 0;
        public int RefCount
        {
            get{ return refCount; } 
            set
            {
                refCount = value;
                if(refCount < 0)
                {
                    Debug.LogError(AssetName + "的资源引用计数<0");
                }
            }
        }
    }
    //AssetBundleItem用于维护AssetBundle的引用计数, ResouceItem中的Ab包来源于AssetBundleItem
    public class AssetBundleItem
    {
        public AssetBundle assetBundle = null;
        public int refCount; //引用计数

        public void Reset()
        {
            assetBundle = null;
            refCount = 0;
        }
    }

    public class AssetBundleManager : TDSingleton<AssetBundleManager>
    {

        #region 字段和属性
        protected Dictionary<uint, ResourceItem> m_resourceItemDict =
         new Dictionary<uint, ResourceItem>();
        protected Dictionary<uint, AssetBundleItem> m_assetBundleItemDict =
         new Dictionary<uint, AssetBundleItem>();
        //AssetBundleItem的类对象池
        protected ClassObjectPool<AssetBundleItem> m_assetBundleItemPool =
         ObjectManager.Instance().GetOrCreateClassObjectPool<AssetBundleItem>(500);
        #endregion	

        #region 构造函数
        public AssetBundleManager()
        {
            LoadAssetBundleConfig(); //初始化
        }
        #endregion

        #region 方法
        public bool LoadAssetBundleConfig()
        {
            m_resourceItemDict.Clear();

            string configPath = ABPathConfig.DependenceFile4AssetBundle;
            AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
            TextAsset textAsset = configAB.LoadAsset<TextAsset>(ABPathConfig.DependenceFileName);
            if (textAsset == null)
            {
                Debug.LogError("AssetBundleConfig is not exists!");
                return false;
            }
            MemoryStream ms = new MemoryStream(textAsset.bytes);
            BinaryFormatter bf = new BinaryFormatter();
            AssetBundleConfig abConfig = (AssetBundleConfig)bf.Deserialize(ms);
            ms.Close();

            for (int i = 0; i < abConfig.ABList.Count; i++)
            {
                ResourceItem item = new ResourceItem();
                ABBase abBase = abConfig.ABList[i];
                item.Crc = abBase.Crc;
                item.AssetName = abBase.AssetName;
                item.ABName = abBase.ABName;
                item.DependABList = new List<string>(abBase.DependABList);
                if (m_resourceItemDict.ContainsKey(item.Crc) == false)
                {
                    m_resourceItemDict.Add(item.Crc, item);
                }
                else
                {
                    Debug.Log("重复的Crc, 资源名: " + item.AssetName + ", AB包名: " + item.ABName);
                }
            }
            return true;
        }
        //根据crc获取ResourceItem
        public ResourceItem LoadResourceItem(uint crc)
        {
            ResourceItem resourceItem = null;
            if (!m_resourceItemDict.TryGetValue(crc, out resourceItem) || resourceItem == null)
            {
                Debug.LogError("没有找到Crc: " + crc.ToString() + "对应的资源!");
                return null;
            }
            if (resourceItem.Ab != null)
            {
                return resourceItem;
            }
            if (resourceItem.DependABList != null)
            {
                for (int i = 0; i < resourceItem.DependABList.Count; i++)
                {
                    LoadAssetBundle(resourceItem.DependABList[i]);
                }
            }
            resourceItem.Ab = LoadAssetBundle(resourceItem.ABName);
            return resourceItem;
        }
        //根据文件路径名获取ResourceItem
        public ResourceItem LoadResourceItem(string filePath)
        {
            uint crc = CrcHelper.StringToCRC32(filePath);
            return LoadResourceItem(crc);
        }
        public void UnLoadResourceItem(ResourceItem item)
        {
            if (item == null) return;
            if (item.DependABList != null && item.DependABList.Count > 0)
            {
                for (int i = 0; i < item.DependABList.Count; i++)
                {
                    UnLoadAssetBundle(item.DependABList[i]);
                }
            }
            UnLoadAssetBundle(item.ABName);
        }
        public AssetBundle LoadAssetBundle(string abName)
        {
            AssetBundleItem abItem = null;
            uint crc = CrcHelper.StringToCRC32(abName); //AB包名字创建crc
            if (!m_assetBundleItemDict.TryGetValue(crc, out abItem) || abItem == null)
            {
                AssetBundle assetBundle = null;
                string abFullPath = ABPathConfig.AssetBundleBuildTargetPath + "/" + abName;
                if (File.Exists(abFullPath))
                {
                    assetBundle = AssetBundle.LoadFromFile(abFullPath);
                }
                if (assetBundle == null)
                {
                    Debug.LogError("Load AssetBundle Error: " + abFullPath);
                    return null;
                }
                abItem = m_assetBundleItemPool.Spawn(true);
                abItem.assetBundle = assetBundle;
                abItem.refCount++;
                m_assetBundleItemDict.Add(crc, abItem);
            }
            else
            {
                abItem.refCount++;
            }
            return abItem.assetBundle;
        }
        public void UnLoadAssetBundle(string abName)
        {
            AssetBundleItem abItem = null;
            uint crc = CrcHelper.StringToCRC32(abName);
            if (m_assetBundleItemDict.TryGetValue(crc, out abItem) && abItem != null)
            {
                abItem.refCount--;
                if (abItem.refCount <= 0 && abItem.assetBundle != null)
                {
                    abItem.assetBundle.Unload(true);
                    abItem.Reset();
                    m_assetBundleItemPool.Recycle(abItem);
                    m_assetBundleItemDict.Remove(crc);
                }
            }
        }
        public ResourceItem FindResourceItem(uint crc)
        {
            return m_resourceItemDict[crc];
        }
        #endregion
    }
}