

namespace TDFramework.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Util
    {
        //查找子物体
        public static T FindObject<T>(Transform parent, string name) where T : UnityEngine.Object
        {
            Transform obj = GetChild(parent, name);
            if (obj != null)
            {
                if (typeof(T).Equals(typeof(UnityEngine.GameObject)))
                    return obj.gameObject as T;
                if (typeof(T).Equals(typeof(UnityEngine.Transform)))
                    return obj as T;
                return obj.gameObject.GetComponent<T>();
            }
            return null;
        }
        static Transform GetChild(Transform parent, string name)
        {
            if (parent.gameObject.name == name)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform curr = GetChild(parent.GetChild(i), name);
                if (curr != null && curr.gameObject.name == name)
                    return curr;
            }
            return null;
        }
        //创建单例
        public static T GetInstance<T>(ref T instance, string name, bool isDontDestroy = true) where T : UnityEngine.Object
        {
            if (instance != null) return instance;
            if (GameObject.FindObjectOfType<T>() != null)
            {
                instance = GameObject.FindObjectOfType<T>();
                return instance;
            }
            GameObject go = new GameObject(name, typeof(T));
            if (isDontDestroy) UnityEngine.Object.DontDestroyOnLoad(go);
            instance = go.GetComponent(typeof(T)) as T;
            return instance;
        }
    }
}
