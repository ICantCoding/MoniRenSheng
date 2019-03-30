using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TDFramework
{
    public class OfflineData : MonoBehaviour
    {
        public Rigidbody m_rigidbody;
        public Collider m_collider;
        //预制件对应的节点
        public Transform[] m_allNode;
        //预制件对应的节点下边子节点的个数
        public int[] m_allNodeChildCount;
        //预制件对应的节点activeSelf属性
        public bool[] m_allNodeActive;
        //预制件对应的节点的位置信息
        public Vector3[] m_allNodePosition;
        //预制件对应的节点的大小信息
        public Vector3[] m_allNodeScale;
        //预制件对应的节点的旋转信息
        public Quaternion[] m_allNodeRotation;

        //还原预制件属性
        public virtual void ResetProperty()
        {
            int allNodeCount = m_allNode.Length;
            for (int i = 0; i < allNodeCount; ++i)
            {
                Transform tempTrans = m_allNode[i];
                if (tempTrans != null)
                {
                    tempTrans.localPosition = m_allNodePosition[i];
                    tempTrans.localScale = m_allNodeScale[i];
                    tempTrans.localRotation = m_allNodeRotation[i];
                    if (m_allNodeActive[i] == true)
                    {
                        if (!tempTrans.gameObject.activeSelf)
                        {
                            tempTrans.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (tempTrans.gameObject.activeSelf)
                        {
                            tempTrans.gameObject.SetActive(false);
                        }
                    }
                    if (tempTrans.childCount > m_allNodeChildCount[i])
                    {
                        int childCount = tempTrans.childCount;
                        for (int j = m_allNodeChildCount[i]; j < childCount; ++j)
                        {
                            GameObject tempObj = tempTrans.GetChild(j).gameObject;
                            if (!ObjectManager.Instance.IsObjectManagerCreated(tempObj))
                            {
                                GameObject.Destroy(tempObj);
                            }
                        }
                    }
                }
            }
        }

        //获取预制件初始属性, 编辑器下保存初始数据
        public virtual void BindData()
        {
            //true表示，隐藏的子节点，也要去找
            m_collider = gameObject.GetComponentInChildren<Collider>(true);
            m_rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
            m_allNode = gameObject.GetComponentsInChildren<Transform>(true);
            int allNodeCount = m_allNode.Length;
            m_allNodeChildCount = new int[allNodeCount];
            m_allNodeActive = new bool[allNodeCount];
            m_allNodePosition = new Vector3[allNodeCount];
            m_allNodeScale = new Vector3[allNodeCount];
            m_allNodeRotation = new Quaternion[allNodeCount];

            for (int i = 0; i < allNodeCount; i++)
            {
                Transform temp = m_allNode[i] as Transform;
                m_allNodeChildCount[i] = temp.childCount;
                m_allNodeActive[i] = temp.gameObject.activeSelf;
                m_allNodePosition[i] = temp.localPosition;
                m_allNodeScale[i] = temp.localScale;
                m_allNodeRotation[i] = temp.localRotation;
            }
        }
    }

}

