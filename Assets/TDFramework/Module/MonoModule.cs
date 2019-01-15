

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class MonoModule<T> : MonoBehaviour
     where T : MonoModule<T>
    {
        private static T m_instance = null;
        public static T Instance
        {
            get
            {
                return m_instance;
            }
        }

        #region Unity生命周期
        void OnApplicationQuit()
        {
            try
            {
                Exit();
            }
            catch
            {

            }
        }
        #endregion

        #region 方法
        internal void Run()
        {
            m_instance = GetComponent<T>();
            Init();
        }
        internal void Exit()
        {
            if (null == m_instance)
                return;

            Release();
            m_instance = null;
        }
        #endregion

        #region 子类需要继承的虚方法
        protected virtual void Init() { }
        protected virtual void Release() { }
        #endregion
    }
}
