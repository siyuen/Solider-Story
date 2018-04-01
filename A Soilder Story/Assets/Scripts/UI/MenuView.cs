using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;
using UIFramework;

/// <summary>
/// 菜单
/// 1.作为其他view的子对象实例，手动设定初始size跟pos
/// 2.作为其他view的子对象动态创建，需要代码初始化
/// </summary>
public class MenuView : UIBase {

    public delegate void NormalFunc();
    //换算
    public static int UNIT = 100;
    //menu的基本属性：背景+四个边界+容器+光标
    public Image menuBg;
    public Image menuTop;
    public Image menuBottom;
    public Image menuLeft;
    public Image menuRight;
    public GridLayoutGroup uiContent;
    public Image optionCursor;
    //当前cursor的idx
    public int cursorIdx;
    //回退事件
    public NormalFunc cancleFunc;
    //记录左右边界初始size跟pos
    public struct MenuFrame
    {
        public MenuFrame(Vector2 s, Vector3 p)
        {
            size = s;
            pos = p;
        }
        public Vector2 size;
        public Vector3 pos;
    }
    //是否开启动画
    public bool bAnim;
    public Dictionary<string, MenuFrame> menuRectTransform = new Dictionary<string, MenuFrame>();
    //子对象高度
    private float fHeight;
    //记录子对象
    private Dictionary<string, List<GameObject>> childObjDic = new Dictionary<string, List<GameObject>>();
    //idx对应的func
    private List<NormalFunc> childFuncList = new List<NormalFunc>();
    //光标移动后的更新
    private List<NormalFunc> moveFuncList = new List<NormalFunc>();
    //动画
    private Tween cursorTw;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        menuRectTransform.Add("Bg", new MenuFrame(menuBg.rectTransform.sizeDelta, menuBg.rectTransform.localPosition));
        menuRectTransform.Add("Top", new MenuFrame(menuTop.rectTransform.sizeDelta, menuTop.rectTransform.localPosition));
        menuRectTransform.Add("Bottom", new MenuFrame(menuBottom.rectTransform.sizeDelta, menuBottom.rectTransform.localPosition));
        menuRectTransform.Add("Left", new MenuFrame(menuLeft.rectTransform.sizeDelta, menuLeft.rectTransform.localPosition));
        menuRectTransform.Add("Right", new MenuFrame(menuRight.rectTransform.sizeDelta, menuRight.rectTransform.localPosition));
        //menuRectTransform.Add("Cursor", new MenuFrame(optionCursor.rectTransform.sizeDelta, optionCursor.rectTransform.position / UNIT));
        GridLayoutGroup grid = uiContent.GetComponent<GridLayoutGroup>();
        fHeight = grid.cellSize.y + grid.spacing.y;
    }

    /// <summary>
    /// 显示初始化，计算显示的size
    /// </summary>
    public void DisplayInit()
    {
        //Init();  
        if (childFuncList.Count == 0)
            return;
        //初始化
        cursorIdx = 0;
        menuBg.rectTransform.sizeDelta = menuRectTransform["Bg"].size;
        menuBottom.rectTransform.position = menuRectTransform["Bottom"].pos;
        menuRight.rectTransform.position = menuRectTransform["Right"].pos;
        menuRight.rectTransform.sizeDelta = menuRectTransform["Right"].size;
        menuLeft.rectTransform.position = menuRectTransform["Left"].pos;
        menuLeft.rectTransform.sizeDelta = menuRectTransform["Left"].size;
        //bg
        menuBg.rectTransform.sizeDelta = new Vector2(menuRectTransform["Bg"].size.x, menuRectTransform["Bg"].size.y * childFuncList.Count);
        //处理左右边界
        Vector3 pos = new Vector3(
            menuBg.rectTransform.sizeDelta.x / 2 + menuRectTransform["Right"].size.x / 2,
            -menuRectTransform["Bg"].size.y / 2 * (childFuncList.Count - 1), 
            0);
        menuRight.rectTransform.localPosition = pos;
        menuRight.rectTransform.sizeDelta = new Vector2(menuRectTransform["Right"].size.x, menuRectTransform["Right"].size.y * childFuncList.Count);
        pos = new Vector3(
            -menuBg.rectTransform.sizeDelta.x / 2 - menuRectTransform["Left"].size.x / 2,
            -menuRectTransform["Bg"].size.y / 2 * (childFuncList.Count - 1),
            0);
        menuLeft.rectTransform.localPosition = pos;
        menuLeft.rectTransform.sizeDelta = new Vector2(menuRectTransform["Left"].size.x, menuRectTransform["Left"].size.y * childFuncList.Count);
        
        //上下边界
        menuTop.rectTransform.sizeDelta = new Vector2(menuBg.rectTransform.sizeDelta.x + menuRectTransform["Right"].size.x * 2, menuRectTransform["Top"].size.y);
        pos = new Vector3(
            0, 
            menuTop.rectTransform.localPosition.y - menuBg.rectTransform.sizeDelta.y - menuRectTransform["Bottom"].size.y, 
            0);
        menuBottom.rectTransform.sizeDelta = new Vector2(menuBg.rectTransform.sizeDelta.x + menuRectTransform["Right"].size.x * 2, menuRectTransform["Top"].size.y);
        menuBottom.rectTransform.localPosition = pos;
        //cursor
        pos = new Vector3(
            -menuBg.rectTransform.sizeDelta.x / 2 - optionCursor.rectTransform.sizeDelta.y / 2, 
            0,
            0);
        if (!menuRectTransform.ContainsKey("Cursor"))
            menuRectTransform.Add("Cursor", new MenuFrame(optionCursor.rectTransform.sizeDelta, pos));
        optionCursor.rectTransform.localPosition = pos;
        //注册
        RegisterEvent();
        //更新一哈
        UpdateOption();
    }

    /// <summary>
    /// 隐藏
    /// </summary>
    public void Hide()
    {
        Clear();
        UnRegisterEvent();
    }

    /// <summary>
    /// 添加子对象
    /// </summary>
    public void AddItem(string path, GameObject child, NormalFunc func, NormalFunc mfunc)
    {
        if (!childObjDic.ContainsKey(path))
            childObjDic[path] = new List<GameObject>();
        childObjDic[path].Add(child);
        childFuncList.Add(func);
        moveFuncList.Add(mfunc);
        child.transform.SetParent(uiContent.transform);
        child.transform.localScale = Vector3.one;
        child.transform.SetSiblingIndex(childFuncList.Count - 1);
    }

    /// <summary>
    /// 清空容器
    /// </summary>
    public void Clear()
    {
        foreach (var c in childObjDic)
        {
            ResourcesMgr.Instance().PushPool(c.Value, c.Key);
        }
        childObjDic.Clear();
        childFuncList.Clear();
        moveFuncList.Clear();
        cursorTw.Pause();
        cursorTw = null;
        bAnim = false;
        cancleFunc = null;
    }

    /// <summary>
    /// 设置cursor
    /// </summary>
    public void SetCursorActive(bool bVal)
    {
        optionCursor.gameObject.SetActive(bVal);
    }

    #region 注册基本事件
    public void RegisterEvent()
    {
        RegisterKeyBoardEvent();
    }

    public void UnRegisterEvent()
    {
        UnRegisterKeyBoardEvent();
    }

    public override void OnUpArrowDown()
    {
        cursorIdx -= 1;
        if (cursorIdx < 0)
            cursorIdx += childFuncList.Count;
        UpdateOption();
    }

    public override void OnDownArrowDown()
    {
        cursorIdx += 1;
        if (cursorIdx >= childFuncList.Count)
            cursorIdx -= childFuncList.Count;
        UpdateOption();
    }

    public override void OnConfirmDown()
    {
        childFuncList[cursorIdx]();
    }

    public override void OnCancelDown()
    {
        cancleFunc();
    }

    /// <summary>
    /// 移动cursor后的更新
    /// </summary>
    public void UpdateOption()
    {
        optionCursor.rectTransform.localPosition = menuRectTransform["Cursor"].pos - new Vector3(0, fHeight * cursorIdx, 0);
        if (moveFuncList.Count > 0 && moveFuncList[cursorIdx] != null)
        {
            moveFuncList[cursorIdx]();
        }
        if(bAnim)
            SetAnimation();
    }

    /// <summary>
    /// 设置动画
    /// </summary>
    private void SetAnimation()
    {
        DOTween.Clear();
        //Vector3 pos = menuRectTransform["Cursor"].pos + new Vector3(0, (fHeight / UNIT) * cursorIdx, 0);
        cursorTw = optionCursor.transform.DOMove(optionCursor.transform.position + new Vector3(0.2f, 0, 0), 0.7f);
        cursorTw.SetLoops(-1);
    }
    #endregion
}
