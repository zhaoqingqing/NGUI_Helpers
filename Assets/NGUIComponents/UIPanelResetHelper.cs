using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 方便对UIPanel进行滚动复位
/// 用法: 
///     var panelResetHelper = UIPanelResetHelper.Create(m_uiPanel);
///     页面重新打开时调用：panelResetHelper.ResetScroll();
/// by 赵青青
/// </summary>
public class UIPanelResetHelper
{
    public UIPanel m_panel { get; private set; }
    public Vector2 m_initPanelClipOffset { get; private set; }
    public Vector3 m_initPanelLocalPos { get; private set; }

    public UIPanelResetHelper(UIPanel uiPanel)
    {
        if (uiPanel == null)
        {
            Debug.LogWarning("需要UIPanel，却传入了NULL，请检查");
            return;
        }
        m_panel = uiPanel;
        m_initPanelClipOffset = m_panel.clipOffset;
        m_initPanelLocalPos = m_panel.cachedTransform.localPosition;
    }

    public void ResetScroll()
    {
        if (m_panel == null) return;
        m_panel.clipOffset = m_initPanelClipOffset;
        m_panel.cachedTransform.localPosition = m_initPanelLocalPos;
    }

    public static UIPanelResetHelper Create(Transform uiPanelTrans)
    {
        UIPanel uiPanel = uiPanelTrans.GetComponent<UIPanel>();
        return Create(uiPanel);
    }

    public static UIPanelResetHelper Create(UIPanel uiPanel)
    {
        return new UIPanelResetHelper(uiPanel);
    }
}
