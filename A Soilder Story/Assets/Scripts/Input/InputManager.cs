using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using System;

public class InputManager : QMonoSingleton<InputManager> {

    //操作类型：0为键盘，1为鼠标
    public int inputType = 0;
    //鼠标操作
    public delegate void MouseHandle();
    //鼠标进入
    public event MouseHandle MouseEnterEvent;
    //public Action mouseAction;
    //鼠标点击
    public event MouseHandle MouseClickEvent; 

    //键盘操作
    public delegate void KeyBoardHandle();
    //上下左右
    public event KeyBoardHandle keyDownEvent;
    
    private Vector3 mousePos;
    //事件列表
    private List<int> mouseEventList= new List<int>();
    private Dictionary<int, KeyBoardHandle> eventTypeDic = new Dictionary<int, KeyBoardHandle>();

    void Awake()
    {
        inputType = 0;
        //初始化EventType对应的event
        eventTypeDic.Add(EventType.KEY_UPARROW, keyDownEvent);
        eventTypeDic.Add(EventType.KEY_DOWNARROW, keyDownEvent);
        eventTypeDic.Add(EventType.KEY_LEFTARROW, keyDownEvent);
        eventTypeDic.Add(EventType.KEY_RIGHTARROW, keyDownEvent);
        eventTypeDic.Add(EventType.KEY_Z, keyDownEvent);
        eventTypeDic.Add(EventType.KEY_X, keyDownEvent);
        eventTypeDic.Add(EventType.KEY_S, keyDownEvent);
    }

    void Start()
    {
        mousePos = Vector3.zero;
    }

	// Update is called once per frame
	void Update () {
        if (inputType == 0)
        {
            if (Input.GetKeyDown(KeyCode.Z))
                KeyZDown();
            else if (Input.GetKeyDown(KeyCode.X))
                KeyXDown();
            else if (Input.GetKeyDown(KeyCode.S))
                KeySDown();
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                KeyUpDown();
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                KeyDownDown();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                KeyLeftDown();
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                KeyRightDown();
        }
        else
        {
            if (mousePos != Input.mousePosition)
                MouseMove();
            if (Input.GetMouseButtonDown(0))
                MouseClick();
        }
	}

    #region 键盘事件
    /// <summary>
    /// 注册键盘按下事件
    /// </summary>
    public void RegisterKeyDownEvent(KeyBoardHandle action, int type)
    {
        if (!eventTypeDic.ContainsKey(type) || eventTypeDic[type] != null)
            return;
        eventTypeDic[type] += action;
    }

    /// <summary>
    /// 注销键盘按下事件
    /// </summary>
    public void UnRegisterKeyDownEvent(KeyBoardHandle action, int type)
    {
        if (!eventTypeDic.ContainsKey(type) || eventTypeDic[type] == null)
            return;
        eventTypeDic[type] -= action;
    }

    protected void KeyZDown()
    {
        if (eventTypeDic[EventType.KEY_Z] != null)
            eventTypeDic[EventType.KEY_Z]();
    }

    protected void KeyXDown()
    {
        if (eventTypeDic[EventType.KEY_X] != null)
            eventTypeDic[EventType.KEY_X]();
    }

    protected void KeySDown()
    {
        if (eventTypeDic[EventType.KEY_S] != null)
            eventTypeDic[EventType.KEY_S]();
    }

    protected void KeyUpDown()
    {
        if (eventTypeDic[EventType.KEY_UPARROW] != null)
            eventTypeDic[EventType.KEY_UPARROW]();
    }
    protected void KeyDownDown()
    {
        if (eventTypeDic[EventType.KEY_DOWNARROW] != null)
            eventTypeDic[EventType.KEY_DOWNARROW]();
    }
    protected void KeyLeftDown()
    {
        if (eventTypeDic[EventType.KEY_LEFTARROW] != null)
            eventTypeDic[EventType.KEY_LEFTARROW]();
    }
    protected void KeyRightDown()
    {
        if (eventTypeDic[EventType.KEY_RIGHTARROW] != null)
            eventTypeDic[EventType.KEY_RIGHTARROW]();
    }
    #endregion

    #region 鼠标事件
    //鼠标移动
    protected void MouseMove()
    {
        mousePos = Input.mousePosition;
        //mouseAction();
        if (MouseEnterEvent != null)
            MouseEnterEvent();
    }
    //鼠标点击
    protected void MouseClick()
    {
        if (MouseClickEvent != null)
            MouseClickEvent();
    }

    /// <summary>
    /// 注册鼠标移动事件
    /// </summary>
    public void RegisterMouseEnterEvent(MouseHandle action, int type, bool b = true)
    {
        if (b)
        {
            if (!mouseEventList.Contains(type))
            {
                mouseEventList.Add(type);
                MouseEnterEvent += action;
            }
        }
        else
        {
            if (mouseEventList.Contains(type))
            {
                mouseEventList.Remove(type);
                MouseEnterEvent -= action;
            }
        }
    }

    /// <summary>
    /// 注册鼠标点击事件
    /// </summary>
    public void RegisterMouseClickEvent(MouseHandle action, int type, bool b = true)
    {
        if (b)
        {
            if (!mouseEventList.Contains(type))
            {
                mouseEventList.Add(type);
                MouseClickEvent += action;
            }
        }
        else
        {
            if (mouseEventList.Contains(type))
            {
                mouseEventList.Remove(type);
                MouseClickEvent -= action;
            }
        }
    }
    #endregion
}
