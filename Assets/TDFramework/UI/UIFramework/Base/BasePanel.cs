
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
// 文件名(File Name):             BasePanel.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-10 22:52:27
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************

namespace TDFramework.UIFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(CanvasGroup))]
    public class BasePanel : MonoBehaviour
    {

        #region UI字段和属性
        protected CanvasGroup m_canvasGroup;
        #endregion

        #region Unity生命周期
        protected void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }
        #endregion

        #region 可继承重写方法
        public virtual void Hide()
        {
            m_canvasGroup.alpha = 0.0f;
            m_canvasGroup.interactable = false;
            m_canvasGroup.blocksRaycasts = false;
        }
        public virtual void Show()
        {
            m_canvasGroup.alpha = 0.0f;
            m_canvasGroup.interactable = true;
            m_canvasGroup.blocksRaycasts = true;
        }
        public virtual void OnEnter(){}
        public virtual void OnPause(){}
        public virtual void OnResume(){}
        public virtual void OnExit(){}
        #endregion
        
    }
}
