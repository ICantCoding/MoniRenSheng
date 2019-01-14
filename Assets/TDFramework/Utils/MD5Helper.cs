

namespace TDFramework.Utils
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    public class MD5Helper
    {

        //对文件进行MD5值计算
        public static string Md5File(string filePath)
        {
            if (null == filePath) return null;
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }
    }
}
