

namespace TDFramework.AppStart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TDFramework.Utils.Ini;

    public class AppStart : MonoBehaviour
    {

        #region Unity生命周期
        IEnumerator Start()
        {
            //读取应用初始化配置文件
            IniHelper.LoadConfig(AppConfig.ApplicatioinConfigFileName);
            while(!IniHelper.IsDone)
            {
                yield return null;
            }
        }
        #endregion
    }
}
