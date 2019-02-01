
namespace TDFramework.Network
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MessageReader
    {
        enum READ_STATE
        {
            READ_STATE_FLAG = 0,
            READ_STATE_MSGLEN = 1,
            READ_STATE_FLOWID = 2,
            READ_STATE_MODULEID = 3,
            READ_STATE_MSGID = 4,
            READ_STATE_RESPONSE_TIME = 5,
            READ_STATE_RESPONSE_FLAG = 6,
            READ_STATE_BODY = 7,
        }
        private System.Byte flag;
        private System.UInt32 msglen;
        private System.UInt32 flowId;
        private System.Byte moduleId;
        private System.UInt16 msgId;
        private System.UInt32 responseTime;
        private System.Int16 responseFlag;

        private System.UInt32 expectSize = 1;
        private READ_STATE state = READ_STATE.READ_STATE_FLAG;
        private MemoryStream stream = new MemoryStream();
        
        #region 主线程处理队列
        private IMainLoop mainloop = null;
        public IMainLoop MainLoop{
            get{return mainloop;}
            set{mainloop = value;}
        }
        #endregion

        #region 构造函数
        public MessageReader()
        {
            expectSize = 1;
            state = READ_STATE.READ_STATE_FLAG;
        }
        #endregion

        public void Process(byte[] datas, System.UInt32 length,
        Dictionary<uint, MessageHandler> flowHandlerDict)
        {
            System.UInt32 readStartIndex = 0;
            while (length > 0 && expectSize > 0)
            {
                if (state == READ_STATE.READ_STATE_FLAG)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        flag = stream.ReadByte();
                        // stream.Clear(); //不懂, 为何要清空stream的位置计数
                        state = READ_STATE.READ_STATE_MSGLEN;
                        expectSize = 4; //下个数据是报文总长度
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
                else if (state == READ_STATE.READ_STATE_MSGLEN)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        msglen = stream.ReadUInt32();
                        // stream.Clear();
                        state = READ_STATE.READ_STATE_FLOWID;
                        expectSize = 4;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
                else if (state == READ_STATE.READ_STATE_FLOWID)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        flowId = stream.ReadUInt32();
                        // stream.Clear();
                        state = READ_STATE.READ_STATE_MODULEID;
                        expectSize = 1;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
                else if (state == READ_STATE.READ_STATE_MODULEID)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        moduleId = stream.ReadByte();
                        //stream.Clear();
                        state = READ_STATE.READ_STATE_MSGID;
                        expectSize = 2;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
                else if (state == READ_STATE.READ_STATE_MSGID)
                {
                    if(length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        msgId = stream.ReadUInt16();
                        //stream.Clear();
                        state = READ_STATE.READ_STATE_RESPONSE_TIME;
                        expectSize = 4;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
                else if(state == READ_STATE.READ_STATE_RESPONSE_TIME)
                {
                    if(length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        responseTime = stream.ReadUInt32();
                        //stream.Clear();
                        state = READ_STATE.READ_STATE_RESPONSE_FLAG;
                        expectSize = 2;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
                else if(state == READ_STATE.READ_STATE_RESPONSE_FLAG)
                {
                    if(length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        responseFlag = stream.ReadInt16();
                        //stream.Clear();
                        state = READ_STATE.READ_STATE_BODY;
                        expectSize = msglen - 4 - 1 - 2 - 4 - 2;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }

                if(state == READ_STATE.READ_STATE_BODY)
                {
                    if(length >= expectSize)
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, expectSize);
                        readStartIndex += expectSize;
                        stream.wpos += expectSize;
                        length -= expectSize;
                        //获取到响应报文对应的回调执行方法
                        MessageHandler handler = null;
                        if (flowHandlerDict == null)
                        {
                            handler = null;
                        } else if (flowHandlerDict.ContainsKey(flowId))
                        {
                            handler = flowHandlerDict[flowId];
                            flowHandlerDict.Remove(flowId);
                        }else {
                            handler = null;
                        }
                        //将获取到的数据，封装成Packet包
                        Packet packet = new Packet(flag, msglen, flowId, moduleId, msgId, 
                        responseTime, responseFlag, stream.GetBytes(stream.rpos, stream.wpos - stream.rpos));
                        if(packet.protobufMessageClassName.Contains("Push"))
                        {
                            //接收到服务器主动推送(Push)的消息，需反射指定对应的类来进行相应的处理
                            MainLoop.QueueInLoop(() => {Debug.Log("接收到了Push消息.");});
                        }
                        else if(handler != null)
                        {
                            //接收到请求响应的消息, 用handler来进行处理
                            MainLoop.QueueInLoop(() => handler(packet));
                        }
                        else
                        {
                            Debug.LogError("客户端接收到一个无法处理的数据报文. ModuleId: " + moduleId + ", MessageId: " + msgId);
                        }
                        //stream.Clear();
                        state = READ_STATE.READ_STATE_FLAG;
                        expectSize = 1;
                    }
                    else
                    {
                        Array.Copy(datas, readStartIndex, stream.Data, stream.wpos, length);
                        stream.wpos += length;
                        expectSize -= length;
                        break;
                    }
                }
            }
            if(responseFlag != 0)
            {
                Debug.LogError("MessageReader:: read error packet " + responseFlag);
            }
        }
    }
}
