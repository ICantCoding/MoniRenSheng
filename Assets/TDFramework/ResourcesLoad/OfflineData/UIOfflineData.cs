using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFramework
{
    public class UIOfflineData : OfflineData
    {
        public Vector2[] m_anchorMin;
        public Vector2[] m_anchorMax;
        public Vector2[] m_pivot;
        public Vector2[] m_sizeDelta;
        public Vector3[] m_anchoredPosition3D;
        //UI可能绑定粒子系统
        public ParticleSystem[] m_particleSystem;

        public override void ResetProperty()
        {
            int allNodeCount = m_allNode.Length;
            for (int i = 0; i < allNodeCount; ++i)
            {
                RectTransform tempTrans = m_allNode[i] as RectTransform;
                if (tempTrans != null)
                {
                    tempTrans.localPosition = m_allNodePosition[i];
                    tempTrans.localScale = m_allNodeScale[i];
                    tempTrans.localRotation = m_allNodeRotation[i];
                    tempTrans.anchorMin = m_anchorMin[i];
                    tempTrans.anchorMax = m_anchorMax[i];
                    tempTrans.pivot = m_pivot[i];
                    tempTrans.sizeDelta = m_sizeDelta[i];
                    tempTrans.anchoredPosition3D = m_anchoredPosition3D[i];
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
			int particleCount = m_particleSystem.Length;
			for(int i = 0; i <particleCount; ++i)
			{
				m_particleSystem[i].Clear(true);
				m_particleSystem[i].Play();
			}
		}

        public override void BindData()
        {
            Transform[] allTrans = gameObject.GetComponentsInChildren<Transform>(true);
            int allTransCount = allTrans.Length;
            for (int i = 0; i < allTransCount; i++)
            {
                if (!(allTrans[i] is RectTransform))
                {
                    allTrans[i] = allTrans[i].gameObject.AddComponent<RectTransform>();
                }
            }
            m_allNode = gameObject.GetComponentsInChildren<RectTransform>(true);
            m_particleSystem = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            int allNodeCount = m_allNode.Length;
            m_allNodeChildCount = new int[allNodeCount];
            m_allNodeActive = new bool[allNodeCount];
            m_allNodePosition = new Vector3[allNodeCount];
            m_allNodeScale = new Vector3[allNodeCount];
            m_allNodeRotation = new Quaternion[allNodeCount];
            m_anchorMin = new Vector2[allNodeCount];
            m_anchorMax = new Vector2[allNodeCount];
            m_pivot = new Vector2[allNodeCount];
            m_sizeDelta = new Vector2[allNodeCount];
            m_anchoredPosition3D = new Vector3[allNodeCount];

            for (int i = 0; i < allNodeCount; i++)
            {
                RectTransform temp = m_allNode[i] as RectTransform;
                m_allNodeChildCount[i] = temp.childCount;
                m_allNodeActive[i] = temp.gameObject.activeSelf;
                m_allNodePosition[i] = temp.localPosition;
                m_allNodeScale[i] = temp.localScale;
                m_allNodeRotation[i] = temp.localRotation;
                m_anchorMin[i] = temp.anchorMin;
                m_anchorMax[i] = temp.anchorMax;
                m_pivot[i] = temp.pivot;
                m_sizeDelta[i] = temp.sizeDelta;
                m_anchoredPosition3D[i] = temp.anchoredPosition3D;
            }
        }
    }
}

