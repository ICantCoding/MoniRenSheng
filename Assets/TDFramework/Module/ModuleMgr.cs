

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.Utils;

    public class ModuleMgr : MonoBehaviour
    {
        #region 字段
        //锁
        private readonly object m_loadlockobj = new object();
        #endregion

        #region 单例
        private static ModuleMgr m_moduleMgr = null;
        public static ModuleMgr Instance
        {
            get
            {
                return Util.GetInstance(ref m_moduleMgr, typeof(ModuleMgr).Name + "_Singleton", true);
            }
        }
        #endregion

        #region 方法
        //加载模块
        public T LoadModule<T>() where T : MonoModule<T>
        {
            T com = null;
            lock (m_loadlockobj)
            {
                com = GetComponent<T>();
                if (com == null)
                {
                    com = this.gameObject.AddComponent<T>();
                    com.Run();
                }
            }
            return com;
        }
        //卸载模块
        public void UnloadModule<T>() where T : MonoModule<T>
        {
            lock (m_loadlockobj)
            {
                T com = GetComponent<T>();
                if (com != null)
                {
                    com.Exit();
                    Destroy(com);
                    com = null;
                }
            }
        }
        #endregion
    }
}