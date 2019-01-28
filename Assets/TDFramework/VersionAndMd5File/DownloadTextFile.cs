
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
// 文件名(File Name):             DownloadFile.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-28 21:23:23
// 修改者列表(modifier):
// 模块描述(Module description):  文本文件下载器, 在目前的项目中用来下载Version.json和Md5Fiel.txt
// ***************************************************************

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using UnityEngine;
    using UnityEngine.Networking;

    public class DownloadTextFile : MonoBehaviour
    {
        #region 单例
        private static DownloadTextFile m_instance = null;
        public static DownloadTextFile Instance
        {
            get { return Util.GetInstance(ref m_instance, "DownloadTextFile", true); }
        }
        #endregion

        #region 方法
        public void Download(string url, Action<string> done, float delay, int timeout)
        {
            StartCoroutine(IEDownload(url, done, delay, timeout));
        }
        private IEnumerator IEDownload(string url, Action<string> done, float delay, int timeout)
        {
            yield return new WaitForSeconds(delay);
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = timeout;
            yield return request.SendWebRequest();
            byte[] bytes = request.downloadHandler.data;
            string content = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            if (request.responseCode != 200)
            {
                if (done != null) done(null);
            }
            else
            {
                if (done != null) done(content);
            }
            request.Abort();
            request.Dispose();
            request = null;
        }
        #endregion
    }
}
