
namespace TDFramework.Network
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ProtoBufModuleMgr : Singleton<ProtoBufModuleMgr>
    {
        //将ProtoBuf的Message按照模块管理起来, 模块id, Message的id, Message类名
        public Dictionary<System.UInt32, Dictionary<System.UInt32, string>> ProtoBufModuleDict = 
        new Dictionary<System.UInt32, Dictionary<System.UInt32, string>>();

        public ProtoBufModuleMgr()
        {
            //为模块1， 添加该模块下边的Message, 进行管理
            Dictionary<System.UInt32, string> messageDict = new Dictionary<System.UInt32, string>();
            messageDict.Add(1, "Message1");
            messageDict.Add(2, "Message2");
            messageDict.Add(3, "Message3");
            ProtoBufModuleDict.Add(1, messageDict);
        }

        //根据模块Id和MessageId来找到Message类名
        public string GetMessageName(System.UInt32 moduleId, System.UInt32 messageId)
        {
            Dictionary<System.UInt32, string> dict = null;
            string messageName = null;
            if(ProtoBufModuleDict.TryGetValue(moduleId, out dict) && dict != null)
            {   
                if(dict.TryGetValue(messageId, out messageName) == false)
                {
                    Debug.LogError("没有找到Protobuf ModuleId: " + moduleId + ", MessageId: " + messageId +　" 的Message.");
                }
            }
            return messageName;
        }
    }
}
