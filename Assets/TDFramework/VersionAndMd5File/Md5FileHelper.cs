
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
// 文件名(File Name):             Md5FileHelper.cs
// 作者(Author):                  田山杉
// 创建时间(CreateTime):          2019-01-28 20:52:40
// 修改者列表(modifier):
// 模块描述(Module description):
// ***************************************************************

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class Md5FileHelper
    {
        //Md5File文本内容转Dictionary
        public static Dictionary<string, string> Md5Text2Dict(string text)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(text)) return dict;
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                string[] keyValue = line.Split('|');
                dict.Add(keyValue[0], keyValue[1]);
            }
            return dict;
        }
        //读取本地Md5File文件转Dictionary
        public static Dictionary<string, string> LocalMd5File2Dict(string fileName = "")
        {
            string path = string.IsNullOrEmpty(fileName) ? string.Format("{0}{1}", Util.DeviceResPath(), AppConfig.Md5FilePath) : fileName;
            if (!File.Exists(path)) return new Dictionary<string, string>();
            try
            {
                string text = File.ReadAllText(path);
                return Md5Text2Dict(text);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return new Dictionary<string, string>();
            }
        }
        //Dictionary写入本地Md5File
        public static void Dict2LocalMd5File(Dictionary<string, string> dict, string fileName = "")
        {
            string outPath = string.IsNullOrEmpty(fileName) ? string.Format("{0}{1}", Util.DeviceResPath(), AppConfig.Md5FilePath) : fileName;
            if (dict == null) return;
            Dictionary<string, string>.Enumerator e = dict.GetEnumerator();
            if (File.Exists(outPath)) File.Delete(outPath);
            try
            {
                FileStream fs = new FileStream(outPath, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs);
                while (e.MoveNext())
                {
                    sw.WriteLine(string.Format("{0}|{1}", e.Current.Key, e.Current.Value));
                }
                e.Dispose();
                fs.Flush();
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }           
        }
    }
}
