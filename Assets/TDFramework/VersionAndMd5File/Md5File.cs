
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
    using System.IO;
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

        #region 构造函数
        public Md5File(string remoteMd5)
        {
            m_localMd5Dict = Md5FileHelper.LocalMd5File2Dict();
            m_remoteMd5Dict = Md5FileHelper.Md5Text2Dict(remoteMd5);
            m_tempMd5Dict = Md5FileHelper.LocalMd5File2Dict(AppConfig.Temp_Md5FilePath);
            GetNeedUpdateFile();
        }
        #endregion

        #region 方法
        public void GetNeedUpdateFile()
        {
            Dictionary<string, string>.Enumerator e = m_remoteMd5Dict.GetEnumerator();
            while(e.MoveNext())
            {
                string file = e.Current.Key;
                string remoteMd5 = e.Current.Value;
                string localMd5 = string.Empty;
                string md5FilePath = string.Format("{0}{1}", Util.DeviceResPath(), file);
                m_localMd5Dict.TryGetValue(e.Current.Key, out localMd5);
                if(string.IsNullOrEmpty(localMd5) || localMd5.Trim() != remoteMd5.Trim() || File.Exists(md5FilePath) == false)
                {
                    if(string.IsNullOrEmpty(localMd5))
                    {
                        Debug.Log(file + "资源需要更新: localMd5 = null.");
                    }
                    else if(localMd5.Trim() != remoteMd5.Trim())
                    {
                        Debug.Log(file + "资源需要更新: 文件内容改变.");
                    }
                    else if(File.Exists(md5FilePath) == false)
                    {
                        Debug.Log(file + "资源需要更新: 本地没有该资源文件.");
                    }

                    string tempMd5 = string.Empty;
                    if(m_tempMd5Dict.TryGetValue(file, out tempMd5))
                    {
                        if(tempMd5.Trim() != remoteMd5.Trim())
                        {
                            string tempFile = string.Format("{0}{1}", Util.DeviceResPath(), file);
                            if(File.Exists(tempFile))
                            {
                                Debug.Log(file + "部分文件已经下载, 但是远程文件发生了改变, 所以需要删除缓存文件重新下载.");
                                File.Delete(tempFile);
                            }
                        }
                    }
                    #warning 把需要更新的文件添加到记录....
                }
            }
            e.Dispose();
            m_tempMd5Dict.Clear();
        }
        #endregion
    }
}

