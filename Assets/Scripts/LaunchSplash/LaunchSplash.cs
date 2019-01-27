
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
// 文件名(File Name):             LaunchSplash.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-26 23:00:45
// 修改者列表(modifier):
// 模块描述(Module description):  App的LaunchSplash场景, 用于启动画面的设计
// ***************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDFramework;
using UnityEngine.UI;

public class LaunchSplash : MonoBehaviour
{

    #region 字段
    private float m_showTime = 3.0f;
    #endregion

    #region UI字段
    
    #endregion

    #region Unity生命周期
    void Start()
    {
        StartCoroutine(JumpAppStartScene());
    }
    #endregion

    #region 方法
    private IEnumerator JumpAppStartScene()
    {
        yield return new WaitForSeconds(m_showTime);
        //LoadSceneMgr.Instance.LoadScene(GlobalHelper.SceneInfoMgr.AppStartScene); //同步直接跳转
        LoadSceneMgr.Instance.LoadLoadingSceneToOtherScene(GlobalHelper.SceneInfoMgr.AppStartScene); //Loading过渡跳转
    }
    #endregion
}
