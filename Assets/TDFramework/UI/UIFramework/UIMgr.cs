

namespace TDFramework.UIFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UIMgr : MonoBehaviour
    {
        
        #region 字段和属性
        //使用栈结构保存UIPanel, 该栈中只有UIPanel
        private Stack<UIPanel> m_panels = new Stack<UIPanel>();
        //使用字典记录UIPanel， Key为UIPanel类型
        private Dictionary<UIPanelType, UIPanel> m_panelDict = new Dictionary<UIPanelType, UIPanel>();
        #endregion


        #region 方法
        //UI界面Prefab从AssetBundle中加载
        public UIPanel GetUIPanel(UIPanelType type)
        {
            UIPanel panel = null;
            if(!m_panelDict.TryGetValue(type, out panel) || panel == null)
            {
                //从AssetBundle中加载UIPanelPrefab
            }
            return panel;
        }
        //获取栈顶UIPanel
        public UIPanel GetTopUIPanel()
        {
            if(m_panels.Count <= 0) return null;
            return m_panels.Peek();
        }
        //打开指定的Panel
        public void PushPanel(UIPanelType type, bool hidePrePanel = true, bool noInteractPrePanel = true)
        {
            UIPanel panel = GetUIPanel(type);
            if(panel == null) return;
            
            //打开下一个UIPanel的时候, 该UIPanel需要执行进入方法, 上一个UIPanel需要执行暂停方法
            UIPanel prePanel = GetTopUIPanel();
            if(prePanel != null)
            {
                prePanel.OnPause(); 
            }
            m_panels.Push(panel);
            panel.OnEnter();
            if(m_panelDict.ContainsKey(type) == false)
            {
                m_panelDict.Add(type, panel);
            }
        }
        //关闭栈顶页面
        public void PopPanel()
        {
            if(m_panels.Count <= 0) return;
            UIPanel panel = GetTopUIPanel();
            if(panel != null)
            {
                panel.OnExit();
            }
            m_panels.Pop();
            panel = GetTopUIPanel();
            if(panel != null)
            {
                panel.OnResume();
            }
        }
        #endregion

    }
}