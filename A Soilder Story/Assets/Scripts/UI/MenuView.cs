using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour {

    //换算
    public const int UNIT = 100;
    //menu的基本属性：背景+四个边界+容器+光标
    public Image menuBg;
    public Image menuTop;
    public Image menuBottom;
    public Image menuLeft;
    public Image menuRight;
    public GridLayoutGroup uiContent;
    public Image optionCursor;
    //回退事件
    public HeroProperty.normalFunc cancleFunc;
    //记录左右边界初始size跟pos
    private struct MenuFrame
    {
        public MenuFrame(Vector2 s, Vector3 p)
        {
            size = s;
            pos = p;
        }
        public Vector2 size;
        public Vector3 pos;
    }
    private Dictionary<string, MenuFrame> menuRectTransform = new Dictionary<string, MenuFrame>();
    //子对象高度
    private float fHeight;
    //记录子对象
    private Dictionary<string, List<GameObject>> childObjDic = new Dictionary<string, List<GameObject>>();
    //当前cursor的idx
    private int cursorIdx;
    //idx对应的func
    private List<HeroProperty.normalFunc> childFuncList = new List<HeroProperty.normalFunc>();
    //光标移动后的更新
    private List<HeroProperty.normalFunc> moveFuncList = new List<HeroProperty.normalFunc>();

    void Awake()
    {
        menuRectTransform.Add("Bg", new MenuFrame(menuBg.rectTransform.sizeDelta, menuBg.rectTransform.position / UNIT));
        menuRectTransform.Add("Top", new MenuFrame(menuTop.rectTransform.sizeDelta, menuTop.rectTransform.position / UNIT));
        menuRectTransform.Add("Bottom", new MenuFrame(menuBottom.rectTransform.sizeDelta, menuBottom.rectTransform.position / UNIT));
        menuRectTransform.Add("Left", new MenuFrame(menuLeft.rectTransform.sizeDelta, menuLeft.rectTransform.position / UNIT));
        menuRectTransform.Add("Right", new MenuFrame(menuRight.rectTransform.sizeDelta, menuRight.rectTransform.position / UNIT));
        menuRectTransform.Add("Cursor", new MenuFrame(optionCursor.rectTransform.sizeDelta, optionCursor.rectTransform.position / UNIT));
        fHeight = uiContent.GetComponent<GridLayoutGroup>().cellSize.y;
        cancleFunc = null;
    }

    /// <summary>
    /// 显示初始化，计算显示的size
    /// </summary>
    public void DisplayInit()
    {
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
        optionCursor.rectTransform.position = menuRectTransform["Cursor"].pos;
        //处理左右边界
        menuRight.transform.position -= new Vector3(0, fHeight / UNIT * (childFuncList.Count - 1) / 2, 0);
        menuRight.rectTransform.sizeDelta = new Vector2(menuRectTransform["Right"].size.x, menuRectTransform["Right"].size.y * childFuncList.Count);
        menuLeft.transform.position -= new Vector3(0, fHeight / UNIT * (childFuncList.Count - 1) / 2, 0);
        menuLeft.rectTransform.sizeDelta = new Vector2(menuRectTransform["Left"].size.x, menuRectTransform["Left"].size.y * childFuncList.Count);
        //下边界
        menuBottom.rectTransform.position -= new Vector3(0, fHeight / UNIT * (childFuncList.Count - 1), 0);
        //bg
        menuBg.rectTransform.sizeDelta = new Vector2(menuRectTransform["Bg"].size.x, menuRectTransform["Bg"].size.y * childFuncList.Count);
        //注册
        RegisterKeyBoardEvent();
        //更新一哈
        UpdateOption();
    }

    /// <summary>
    /// 隐藏
    /// </summary>
    public void Hide()
    {
        Clear();
        UnRegisterKeyBoardEvent();
    }

    /// <summary>
    /// 添加子对象
    /// </summary>
    public void AddItem(string path, GameObject child, HeroProperty.normalFunc func, HeroProperty.normalFunc mfunc)
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
            GameObjectPool.Instance().PushPool(c.Value, c.Key);
        }
        childObjDic.Clear();
        childFuncList.Clear();
        moveFuncList.Clear();
    }

    #region 注册基本事件
    private void RegisterKeyBoardEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void UnRegisterKeyBoardEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void OnUpArrowDown()
    {
        cursorIdx -= 1;
        
        if (cursorIdx < 0)
        {
            cursorIdx += childFuncList.Count;
            optionCursor.transform.position -= new Vector3(0, fHeight / UNIT * (childFuncList.Count - 1), 0);
        }
        else
        {
            optionCursor.transform.position += new Vector3(0, fHeight / UNIT, 0);
        }
        UpdateOption();
    }

    private void OnDownArrowDown()
    {
        cursorIdx += 1;
        if (cursorIdx >= childFuncList.Count)
        {
            cursorIdx -= childFuncList.Count;
            optionCursor.transform.position += new Vector3(0, fHeight / UNIT * (childFuncList.Count - 1), 0);
        }
        else
        {
            optionCursor.transform.position -= new Vector3(0, fHeight / UNIT, 0);
        }
        UpdateOption();
    }

    private void OnConfirmDown()
    {
        childFuncList[cursorIdx]();
    }

    private void OnCancelDown()
    {
        Hide();
        cancleFunc();
        cancleFunc = null;
    }

    /// <summary>
    /// 移动cursor后的更新
    /// </summary>
    private void UpdateOption()
    {
        if (moveFuncList[cursorIdx] != null)
            moveFuncList[cursorIdx]();
    }

    #endregion
}
