
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
// 文件名(File Name):             ResourceLoadTest.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-16 00:01:30
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDFramework;

public class ResourceLoadTest : MonoBehaviour
{

    public AudioSource audioSource;
    private GameObject obj = null;
    void Start()
    {
        ResourceMgr.Instance.Init(this);
        ObjectManager.Instance.InitGoPool(null, null);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ObjectManager.Instance.InstantiateAsync("Assets/Res/Prefab/Robot_Blue.prefab", OnXX, LoadAssetPriority.HIGH);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ObjectManager.Instance.ReleaseGameObjectItem(obj, 0, false, false);
        }
    }
    private void OnXX(string path, Object objx, object param1 = null,
     object param2 = null, object param3 = null)
     {
         obj = objx as GameObject;
     }
}
