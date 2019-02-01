
namespace TDFramework.Network
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IMainLoop
    {
        void QueueInLoop(System.Action callback);   //插入回调
    }
}
