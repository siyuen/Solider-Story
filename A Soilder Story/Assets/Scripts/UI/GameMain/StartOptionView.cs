using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

public class StartOptionView : UIBase
{
    public enum State
    {
        Normal,
        Operate,
    }
    public GridLayoutGroup uiContent;
    public GameObject optionCursor;
    public GameObject operateView;

    private State uiState;
    private int cursorIdx;
    private List<GameObject> objList = new List<GameObject>();
    //idx对应的func
    private List<MenuView.NormalFunc> childFuncList = new List<MenuView.NormalFunc>();
    private Sequence cursorSeq;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
    }

    public override void Display()
    {
        base.Display();
        Init();
    }

    public override void Hiding()
    {
        base.Hiding();
        Clear();
    }

    private void Init()
    {
        uiState = State.Normal;
        optionCursor.SetActive(true);
        operateView.SetActive(false);
        cursorIdx = 0;

        RegisterEvent();
        BtnInit();
        UpdateOption();
    }

    private void Clear()
    {
        UnRegisterEvent();
        BtnClear();
    }

    private void BtnInit()
    {
        if (uiState == State.Normal)
        {
            if (GameManager.Instance().bTemporary)
            {
                GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.OPTION_BUTTON);
                Text txt = btn.GetComponentInChildren<Text>();
                txt.text = "继续游戏";
                btn.transform.SetParent(uiContent.transform);
                btn.transform.localScale = Vector3.one;
                objList.Add(btn);
                childFuncList.Add(ContinueGame);

                GameObject btn1 = ResourcesMgr.Instance().GetPool(MainProperty.OPTION_BUTTON);
                Text txt1 = btn1.GetComponentInChildren<Text>();
                txt1.text = "开始游戏";
                btn1.transform.SetParent(uiContent.transform);
                btn1.transform.localScale = Vector3.one;
                objList.Add(btn1);
                childFuncList.Add(StartGame);
            }
            else
            {
                GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.OPTION_BUTTON);
                Text txt = btn.GetComponentInChildren<Text>();
                txt.text = "开始游戏";
                btn.transform.SetParent(uiContent.transform);
                btn.transform.localScale = Vector3.one;
                objList.Add(btn);
                childFuncList.Add(StartGame);
            }
            GameObject option = ResourcesMgr.Instance().GetPool(MainProperty.OPTION_BUTTON);
            Text text = option.GetComponentInChildren<Text>();
            text.text = "操作说明";
            option.transform.SetParent(uiContent.transform);
            option.transform.localScale = Vector3.one;
            objList.Add(option);
            childFuncList.Add(ExplainOperate);
        }
    }

    private void BtnClear()
    {
        for (int i = 0; i < objList.Count; i++)
        {
            objList[i].transform.SetParent(null);
        }
        ResourcesMgr.Instance().PushPool(objList, MainProperty.OPTION_BUTTON);
        objList.Clear();
        childFuncList.Clear();
    }

    private void RegisterEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void UnRegisterEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void OnUpArrowDown()
    {
        if (uiState == State.Normal)
        {
            cursorIdx -= 1;
            if (cursorIdx < 0)
                cursorIdx += objList.Count;
            UpdateOption();
        }
    }

    private void OnDownArrowDown()
    {
        if (uiState == State.Normal)
        {
            cursorIdx += 1;
            if (cursorIdx >= objList.Count)
                cursorIdx -= objList.Count;
            UpdateOption();
        }
    }

    private void OnConfirmDown()
    {
        if (uiState == State.Normal)
        {
            if (childFuncList[cursorIdx] != null)
                childFuncList[cursorIdx]();
        }
    }

    private void OnCancelDown()
    {
        if (uiState == State.Normal)
        {
            UIManager.Instance().CloseUIForms("StartOption");
            OpenUIForm("Login");
        }
        else if (uiState == State.Operate)
        {
            uiState = State.Normal;
            operateView.SetActive(false);
        }
    }

    /// <summary>
    /// 移动cursor后的更新
    /// </summary>
    public void UpdateOption()
    {
        cursorSeq.Kill();
        float y = uiContent.cellSize.y + uiContent.spacing.y;
        optionCursor.transform.localPosition = new Vector3(0, uiContent.transform.localPosition.y - y * cursorIdx, 0);
        cursorSeq = DOTween.Sequence().
            Append(optionCursor.transform.DOMove(optionCursor.transform.position + new Vector3(0, -0.1f, 0), 0.7f)).
            Append(optionCursor.transform.DOMove(optionCursor.transform.position + new Vector3(0, 0f, 0), 0.7f)).
            SetLoops(-1);
        //if (moveFuncList.Count > 0 && moveFuncList[cursorIdx] != null)
        //{
        //    moveFuncList[cursorIdx]();
        //}
        //if (bAnim)
        //    SetAnimation();
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        GameManager.Instance().gameState = GameManager.GameState.Load;
        UIManager.Instance().CloseUIForms("StartOption");
        OpenUIForm("StartGame");
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    private void ContinueGame()
    {
        UIManager.Instance().CloseUIForms("StartOption");
        GameManager.Instance().ContinueGame();
    }

    /// <summary>
    /// 显示操作
    /// </summary>
    private void ExplainOperate()
    {
        uiState = State.Operate;
        operateView.SetActive(true);
    }
}
