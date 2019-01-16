

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TDFramework.TDDesignMode;

    public class ObjectManager : TDSingleton<ObjectManager>
    {

        #region 字段和属性
        private Dictionary<Type, object> m_classObjectPoolDict = new Dictionary<Type, object>();
        #endregion

        #region 方法
        public ClassObjectPool<T> GetOrCreateClassObjectPool<T>(int maxCount)
            where T : class, new()
        {
            Type type = typeof(T);
            object obj = null;
            if(!m_classObjectPoolDict.TryGetValue(type, out obj) || obj == null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
                m_classObjectPoolDict.Add(type, newPool);
                return newPool;
            }
            return obj as ClassObjectPool<T>;
        }
        #endregion
    }
}
