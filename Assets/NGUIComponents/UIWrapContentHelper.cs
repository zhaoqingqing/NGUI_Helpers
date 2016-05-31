using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 对NGUI的 UIWrapContent的封装，如果低于NGUI3.7.x，请使用高版本的UIWrapContent替换
/// 目录结构：(NGUI3.9.7)
///     -GameObject绑定 UIPanel，UIScrollView
///         -GameObject绑定 UIWrapContent，[UIGrid]
///             -Item (具体的滑动内容)
/// 使用方法：
///         var WrapContentHelper = UIWrapContentHelper.Create(WrapContent);
///         WrapContentHelper.OnRenderEvent = OnRenderWrapContent;
/// by 赵青青  
/// </summary>
public class UIWrapContentHelper
{
    public delegate void UIWrapContentRenderDelegate(GameObject obj, int index);
    /// <summary>
    ///obj:要渲染的对象; index:索引，从0开始
    /// </summary>
    public UIWrapContentRenderDelegate OnRenderEvent;

    private int _count;//总数
    private bool _hasRefresh = false;//是否已刷新

    private UIWrapContent _wrapContent;
    private UIPanel _panel;
    private UIScrollView _scrollView;
    private Vector2 _initPanelClipOffset;
    private Vector3 _initPanelLocalPos;
    /// <summary>
    /// 缓存起来上次渲染的对象对应索引
    /// </summary>
    private Dictionary<GameObject, int> CacheObject2Index = new Dictionary<GameObject, int>();

    private UIWrapContentHelper(){}

    private UIWrapContentHelper(UIWrapContent uiWrapContent)
    {
        if (uiWrapContent == null)
        {
            Debug.LogError("UIWrapContentHelper 传入了NULL");
            return;
        }
        _wrapContent = uiWrapContent;
        //_wrapContent.hideInactive = false;
        _wrapContent.onInitializeItem = OnInitItem; //NOTE NGUI 3.7.x以上版本才有此功能
        //NOTE UIPanel 建议挂在UIWrapContent的父级,NGUI3.9.7非父级我这儿出现异怪现象
        _panel = _wrapContent.gameObject.GetComponent<UIPanel>();
        var panelParent = _wrapContent.transform.parent;
        if (_panel == null && panelParent != null)
        {
            _panel = panelParent.GetComponent<UIPanel>();
        }
        if (_panel == null)
        {
            Debug.LogError(uiWrapContent.name + "的父节点没有UIPanel");
            return;
        }
        _scrollView = _panel.GetComponent<UIScrollView>();
        _initPanelClipOffset = _panel.clipOffset;
        _initPanelLocalPos = _panel.cachedTransform.localPosition;
    }

    //初始化数据，Init或Open时调用
    public void ResetScroll()
    {
        if (_panel == null || _wrapContent == null || _scrollView == null)
        {
            Debug.LogWarning("panel or  wrapContent , scrollView is null ");
            return;
        }
        _panel.clipOffset = _initPanelClipOffset;
        _panel.cachedTransform.localPosition = _initPanelLocalPos;

        // 重设组件~索引和位置
        var index = 0;
        foreach (var oChildTransform in _wrapContent.transform)
        {
            var childTransform = (Transform)oChildTransform;
            // NOTE: 横方向未测试
            if (_scrollView.movement == UIScrollView.Movement.Vertical)
            {
                childTransform.SetLocalPositionY(-_wrapContent.itemSize * index);
            }
            else if (_scrollView.movement == UIScrollView.Movement.Horizontal)
            {
                childTransform.SetLocalPositionX(-_wrapContent.itemSize * index);
            }
            CacheObject2Index[childTransform.gameObject] = index;
            index++;
        }

        //fix soft clip panel
        if (_panel.clipping == UIDrawCall.Clipping.SoftClip) _panel.SetDirty();
    }

    /// <summary>
    /// 设置多少项
    /// </summary>
    /// <param name="count"></param>
    /// <param name="invertOrder">是否反转</param>
    private void SetCount(int count, bool invertOrder = false)
    {
        if (_panel == null || _wrapContent == null)
        {
            Debug.LogWarning("panel or  wrapContent is null ");
            return;
        }
        _count = count;
        //TODO: invertOrder有bug ，NGUI 3.7.x有此功能
        //if (invertOrder)
        //{
        //    _wrapContent.minIndex = 0;
        //    _wrapContent.maxIndex = count - 1;
        //}
        //else
        {
            _wrapContent.minIndex = -count + 1;
            _wrapContent.maxIndex = 0;
        }
        //fix: 按字母排序有bug:显示错乱
        //_wrapContent.SortAlphabetically();

        if (_scrollView != null)
        {
            var canDrag = _count >= GetActiveChilds(_wrapContent.transform).Count;
            if (count == 1) canDrag = false;
            _scrollView.restrictWithinPanel = canDrag;
            _scrollView.disableDragIfFits = !canDrag; // 只有一个的时候，不能滑动
        }
    }

    private void OnInitItem(GameObject go, int wrapindex, int realindex)
    {
        var index = Mathf.Abs(realindex);// 取绝对值
        CacheObject2Index[go] = index; 
        if (CheckActive(go, index) && _hasRefresh)
        {
            DoRender(go, index);
        }
    }

    /// <summary>
    /// 检查是否应该隐藏起来
    /// </summary>
    private bool CheckActive(GameObject go, int index)
    {
        bool needActive = index <= (_count - 1);//小于总数才显示
        go.SetActive(needActive);
        return needActive;
    }

    //触发渲染事件
    private void DoRender(GameObject go, int index)
    {
        if (OnRenderEvent == null)
        {
            Debug.LogError("UIWrapContent必须设置RenderFunc!");
            return;
        }
        OnRenderEvent(go, index);
    }

    /// <summary>
    /// 执行刷新，单个单个地渲染
    /// </summary>
    /// <param name="count"></param>
    /// <param name="invertOrder">反转：当有Scrollbar时才设置此值。指scrollbar的拖动方向,反转有bug，需完善</param>
    public void Refresh(int count, bool invertOrder = false)
    {
        SetCount(count, invertOrder);
        //fix:使用GetEnumerator 替代foreach，减少GC
        var enumerator = CacheObject2Index.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (CheckActive(enumerator.Current.Key, enumerator.Current.Value))
            {
                DoRender(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        _hasRefresh = true;
    }

    //强制设置scrollview是否可以滑动,
    //fix 前面在SetCount中有设此值，但判断依据不一定
    public void CanDragScrollview(bool canDrag)
    {
        if (_scrollView != null)
        {
            _scrollView.restrictWithinPanel = canDrag;
            _scrollView.disableDragIfFits = !canDrag; // 只有一个的时候，不能滑动
        }
    }

    public static UIWrapContentHelper Create(UIWrapContent uiWrapContent)
    {
        return new UIWrapContentHelper(uiWrapContent);
    }

    // 获取一个Transfrom下所有active=true的child
    public static List<GameObject> GetActiveChilds(Transform parent)
    {
        var list = new List<GameObject>();
        if (parent == null) return list;
        var max = parent.childCount;
        for (int idx = 0; idx < max; idx++)
        {
            var childObj = parent.GetChild(idx).gameObject;
            if (childObj.activeInHierarchy) list.Add(childObj);
        }
        return list;
    }
}