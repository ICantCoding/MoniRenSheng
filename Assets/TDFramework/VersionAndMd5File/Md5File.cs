
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
// 文件名(File Name):             Md5File.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-28 20:49:20
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Md5File
    {

        #region 字段
        private Dictionary<string, string> m_localMd5Dict;
        private Dictionary<string, string> m_remoteMd5Dict;
        private Dictionary<string, string> m_tempMd5Dict;
        #endregion
    }
}

