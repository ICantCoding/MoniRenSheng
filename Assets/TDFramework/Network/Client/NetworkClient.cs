

namespace TDFramework.Network
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class NetworkClient : MonoBehaviour
    {

        #region 字段
        public NetworkEngine m_networkEngine = new NetworkEngine();
        #endregion

        #region Unity生命周期
        void Update()
        {
            // m_networkEngine.UpdateInMainThread();
        }
        #endregion

    }
}
