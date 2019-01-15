

namespace TDFramework.UIFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIPage : MonoBehaviour
    {
        #region UI字段和属性
        protected CanvasGroup m_canvasGroup;
        #endregion

        #region Unity生命周期
        protected void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }
        #endregion

        #region 可继承重写方法
        public virtual void Hide()
        {
            m_canvasGroup.alpha = 0.0f;
            m_canvasGroup.interactable = false;
            m_canvasGroup.blocksRaycasts = false;
        }
        public virtual void Show()
        {
            m_canvasGroup.alpha = 0.0f;
            m_canvasGroup.interactable = true;
            m_canvasGroup.blocksRaycasts = true;
        }
        public virtual void OnEnter() { }
        public virtual void OnPause() { }
        public virtual void OnResume() { }
        public virtual void OnExit() { }
        #endregion
    }
}