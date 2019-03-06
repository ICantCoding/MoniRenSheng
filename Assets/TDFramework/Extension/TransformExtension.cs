namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public static class TransformExtension
    {
        public static void AddListener2BtnOnClick(this Transform transf, System.Action action)
        {
            Button btn = transf.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => action());
            }
        }
        public static RectTransform RectTransform(this Transform transf)
        {
            RectTransform rectTransform = transf.GetComponent<RectTransform>();
            return rectTransform;
        }
    }
}

