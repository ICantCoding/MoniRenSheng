

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using TDFramework.Utils;
    using UnityEngine;

    public class Version
    {
        public string version;
        public string app_download_url;
        public string res_download_url;
        public int download_fail_retry;
        public int timeout;

        public override string ToString()
        {
            return "version_num: " + version +
            ", app_download_url: " + app_download_url +
            ", res_download_url: " + res_download_url +
            ", download_retry: " + download_fail_retry +
            ", timeout: " + timeout;
        }
    }

    public enum VersionStatus
    {
        High = 0, //当前版本比服务器版本低，需更新
        Equal = 1,//当前版本与服务器版本一致，不需要更新
        None = 2,
    }

    public class VersionHelper
    {
        public static Version GetLocalVersionForApp(string versionFilePath)
        {
            string version_path = versionFilePath;
            if (!File.Exists(version_path)) return null;
            string text = File.ReadAllText(version_path);
            if (string.IsNullOrEmpty(text)) return null;
            return JsonForVersion(text);
        }
        public static Version GetLocalVersionForStreamingAssets()
        {
            string version_path = string.Format("{0}/{1}", Application.streamingAssetsPath, AppConfig.VersionFilePath);
            if (!File.Exists(version_path)) return null;
            string text = File.ReadAllText(version_path);
            if (string.IsNullOrEmpty(text)) return null;
            return JsonForVersion(text);
        }
        //比较两个Version, 检查App是否需要更新
        public static VersionStatus CompareLocalAndRemoteVersion(Version localVersion, Version remoteVersion)
        {
            if (localVersion == null || remoteVersion == null)
                return VersionStatus.None;
            if (string.IsNullOrEmpty(localVersion.version) || string.IsNullOrEmpty(remoteVersion.version))
                return VersionStatus.None;
            if (localVersion.version == remoteVersion.version)
            {
                return VersionStatus.Equal;
            }
            string[] localVersionNode = localVersion.version.Split('.');
            string[] remoteVersionNode = remoteVersion.version.Split('.');
            for (int i = 0; i < localVersionNode.Length; i++)
            {
                if (int.Parse(localVersionNode[i]) < int.Parse(remoteVersionNode[i]))
                {
                    return VersionStatus.High;
                }
            }
            return VersionStatus.Equal;
        }
        //字符串转Version对象
        public static Version JsonForVersion(string json)
        {
            try
            {
                Version version = JsonUtility.FromJson<Version>(json);
                return version;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
        //Version对象序列化到文件
        public static void WriteLocalVersionFile(Version version)
        {
            string version_path = string.Format("{0}/{1}", Util.DeviceResPath(), AppConfig.VersionFilePath);
            //创建根目录
            if (!Directory.Exists(Path.GetDirectoryName(version_path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(version_path));
            }
            //删除旧文件
            if (File.Exists(version_path))
            {
                File.Delete(version_path);
            }
            string json = JsonUtility.ToJson(version);
            try
            {
                File.WriteAllText(version_path, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                throw;
            }
        }

    }
}
