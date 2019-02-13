using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDFramework.Network;

public class ServerTest : MonoBehaviour
{

    private NetworkServer server = null;
    public NetworkClient client = null;

    #region Unity生命周期
    void OnGUI()
    {
        if (GUILayout.Button("开启服务器"))
        {
            server = new NetworkServer();
            server.Start(20102);
        }
        if (GUILayout.Button("连接服务器"))
        {
            client.m_networkEngine.NetworkInterface.Connect("127.0.0.1", 20102);
        }
    }
    void OnApplicationQuit()
    {
        server.Close();
        server = null;
    }
    #endregion

}
