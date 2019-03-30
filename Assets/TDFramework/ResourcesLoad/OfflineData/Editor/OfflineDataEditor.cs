
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TDFramework;

public class OfflineDataEditor
{
    [MenuItem("Assets/对象池/生成离线数据")]
    private static void AssetCreateOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改" + objs[i].name + "...", 1.0f / objs.Length * i);
            CreateOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    public static void CreateOfflineData(GameObject obj)
    {
        OfflineData offlineData = obj.GetComponent<OfflineData>();
        if (offlineData == null)
        {
            offlineData = obj.AddComponent<OfflineData>();
            offlineData.BindData();
            EditorUtility.SetDirty(obj);
            Debug.Log("修改了" + obj.name + " Prefab!");
            Resources.UnloadUnusedAssets();
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Assets/对象池/生成UI离线数据")]
    private static void AssetCreateUIOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在修改" + objs[i].name + "...", 1.0f / objs.Length * i);
            CreateUIOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/对象池/生成所有UIPrefab离线数据")]
    private static void CreateAllUIOfflineData()
    {
        string path = "Assets/Prefab/UI/";
        string[] guidStrs = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
        for (int i = 0; i < guidStrs.Length; ++i)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guidStrs[i]);
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在扫面路径:" + prefabPath, 1.0f / guidStrs.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (obj == null) continue;
            CreateUIOfflineData(obj);
        }
        EditorUtility.ClearProgressBar();
        Debug.Log("UIPrefab离线数据全部设置完毕!");
    }
    private static void CreateUIOfflineData(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("UI");
        UIOfflineData uiOfflineData = obj.GetComponent<UIOfflineData>();
        if (uiOfflineData == null)
        {
            uiOfflineData = obj.AddComponent<UIOfflineData>();
        }
        uiOfflineData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了UI " + obj.name + " Prefab.");
        AssetDatabase.Refresh();
    }
    [MenuItem("Assets/对象池/生成特效离线数据")]
    private static void AssetCreateEffectOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在修改" + objs[i].name + "...", 1.0f / objs.Length * i);
            CreateEffectOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }
	[MenuItem("Assets/对象池/生成所有Effect离线数据")]
    private static void CreateAllEffectOfflineData()
    {
        string path = "Assets/Prefab/Effect/";
        string[] guidStrs = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
        for (int i = 0; i < guidStrs.Length; ++i)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guidStrs[i]);
            EditorUtility.DisplayProgressBar("添加Effect离线数据", "正在扫面路径:" + prefabPath, 1.0f / guidStrs.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (obj == null) continue;
            CreateEffectOfflineData(obj);
        }
        EditorUtility.ClearProgressBar();
        Debug.Log("Effect离线数据全部设置完毕!");
    }
    private static void CreateEffectOfflineData(GameObject obj)
    {
        EffectOfflineData effectOfflineData = obj.GetComponent<EffectOfflineData>();
        if (effectOfflineData == null)
        {
            effectOfflineData = obj.AddComponent<EffectOfflineData>();
        }
        effectOfflineData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了特效 " + obj.name + " EffectPrefab.");
        AssetDatabase.Refresh();
    }

}
