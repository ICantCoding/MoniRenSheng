
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
// 文件名(File Name):             DownloadMd5File.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-28 22:07:55
// 修改者列表(modifier):
// 模块描述(Module description):  用于下载服务器远端Md5File.txt文件
// ***************************************************************

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DownloadMd5File
    {

        public delegate void DownloadSuccssedCallback(string content);
        public delegate void DownloadFailedCallback();
        public DownloadSuccssedCallback downloadSuccessedCallback;
        public DownloadFailedCallback downloadFailedCallback;

        #region 字段
        private string m_downloadurl;
        private int m_downloadRetryCount = 3;
        private int m_downloadTimeout = 10;
        private float m_downloadTryAgainDelay = 2.0f;
        #endregion

        #region 构造函数
        public DownloadMd5File(DownloadSuccssedCallback s_callback, DownloadFailedCallback f_callback)
        {
            m_downloadurl = AppConfig.RemoteMd5FileUrl;
            m_downloadTimeout = AppConfig.DownloadFileTimeout;
            m_downloadRetryCount = AppConfig.DownloadFileFailedTryCount;
            m_downloadTryAgainDelay = AppConfig.DownloadFileTryAgainDelay;
            downloadSuccessedCallback = s_callback;
            downloadFailedCallback = f_callback;
        }
        #endregion
        
        #region 方法
        public void Download()
        {
            DownloadRemoteMd5File();
        }
        public void DownloadRemoteMd5File()
        {
            m_downloadRetryCount--;
            DownloadTextFile.Instance.Download(m_downloadurl, DownloadRemoteMd5FileCompelete,
            m_downloadTryAgainDelay, m_downloadTimeout);
        }
        private void DownloadRemoteMd5FileCompelete(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                //下载失败, 判断是否需要继续下载
                if (m_downloadRetryCount > 0)
                {
                    DownloadRemoteMd5File(); //再次下载
                }
                else
                {
                    //下载彻底失败
                    if(downloadFailedCallback != null) downloadFailedCallback();
                }
            }
            else
            {
                //下载成功
                if(downloadSuccessedCallback != null) downloadSuccessedCallback(content);
            }
        }
        #endregion
    }
}
