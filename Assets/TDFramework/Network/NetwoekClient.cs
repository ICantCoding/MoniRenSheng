

namespace TDFramework.Network
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class NetwoekClient : MonoBehaviour
    {

        #region 字段
        private NetworkEngine m_networkEngine;
        #endregion

        #region Unity生命周期
        void Update()
        {
            m_networkEngine.UpdateInMainThread();
        }
        #endregion

    }
}
