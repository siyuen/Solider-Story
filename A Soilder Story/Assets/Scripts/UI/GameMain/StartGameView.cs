using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

public class StartGameView : UIBase {

    enum State
    {
        Normal,
        Option,
        Tips,
    }

    public const int OPTIONCOUNT = 2;
    public const string NODATA = "--  NO   DATA  --";
    public const string SAVETITLE = "记录游戏";
    public const string LOADTITLE = "读取记录的游戏";

    public Text startTitle;
    public GameObject optionCursor;
    public GameObject recordObj;
    public GridLayoutGroup uiContent;
    //option
    public GameObject optionObj;
    public GameObject cursorFinger;
    public Text option1Text;
    public Text option2Text;
    public GameObject tipsObj;
    public GameObject levelTips;

    private State uiState;
    private int cursorIdx;
    private int optionIdx;
    private Sequence cursorSeq;
    private List<GameObject> objList = new List<GameObject>();

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
        optionObj.SetActive(false);
        cursorFinger.SetActive(false);
        recordObj.SetActive(false);
        tipsObj.SetActive(false);
        levelTips.SetActive(false);
        cursorIdx = 0;
        optionIdx = 0;
        uiState = State.Normal;

        RegisterEvent();
        InitData();
        UpdateOption();
    }

    private void Clear()
    {
        UnRegisterEvent();
        ResourcesMgr.Instance().PushPool(objList, MainProperty.OPTION_BUTTON3);
        objList.Clear();
    }

    private void InitData()
    {
        GameManager game = GameManager.Instance();
        if (GameManager.Instance().gameState == GameManager.GameState.Save)
            startTitle.text = SAVETITLE;
        else if (GameManager.Instance().gameState == GameManager.GameState.Load)
            startTitle.text = LOADTITLE;
        //1.三无 2.有记录
        if (game.curGameIdx == GameManager.NULLGAME)
        {
            for (int i = 0; i < game.gameDataDic.Count; i++)
            {
                GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.OPTION_BUTTON3);
                Text txt = btn.GetComponentInChildren<Text>();
                txt.text = NODATA;
                btn.transform.SetParent(uiContent.transform);
                btn.transform.localScale = Vector3.one;
                objList.Add(btn);
            }
        }
        else
        {
            recordObj.SetActive(true);
            //根据相应的level初始化，若无记录则为null，有中断记录的添加tag
            for (int i = 0; i < game.gameDataDic.Count; i++)
            {
                GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.OPTION_BUTTON3);
                if (DataManager.Value(game.gameDataDic[i].level) != GameManager.NULLGAME)
                {
                    int level = DataManager.Value(game.gameDataDic[i].level);
                    Text txt = btn.GetComponentInChildren<Text>();
                    txt.text = level + " 章:   " + LevelManager.Instance().levelDic[level.ToString()].name;
                    float y = uiContent.cellSize.y + uiContent.spacing.y;
                    if (i == GameManager.Instance().curGameIdx)
                        recordObj.transform.localPosition = new Vector3(uiContent.cellSize.x / 2 - 40, uiContent.transform.localPosition.y - y * i + 10, 0);
                }
                else
                {
                    Text txt = btn.GetComponentInChildren<Text>();
                    txt.text = NODATA;
                }
                btn.transform.SetParent(uiContent.transform);
                btn.transform.localScale = Vector3.one;
                objList.Add(btn);
            }
        }
    }

    public void UpdateData()
    {
        ResourcesMgr.Instance().PushPool(objList, MainProperty.OPTION_BUTTON3);
        objList.Clear();
        InitData();
    }

    private void RegisterEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void UnRegisterEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);
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

    private void OnLeftArrowDown()
    {
        if (uiState == State.Option)
        {
            optionIdx += 1;
            if (optionIdx >= OPTIONCOUNT)
                optionIdx -= OPTIONCOUNT;
            UpdateOption();
        }
    }

    private void OnRightArrowDown()
    {
        if (uiState == State.Option)
        {
            optionIdx -= 1;
            if (optionIdx < 0)
                optionIdx += OPTIONCOUNT;
            UpdateOption();
        }
    }

    private void OnConfirmDown()
    {
        if (uiState == State.Normal)
        {
            if (GameManager.Instance().curGameIdx == -1)
            {
                UIManager.Instance().CloseUIForms("StartGame");
                GameManager.Instance().StartGame(cursorIdx);
            }
            else
                InitOption();
        }
        else if (uiState == State.Option)
        {
            if (optionIdx == 1)
            {
                tipsObj.SetActive(false);
                cursorFinger.SetActive(false);
                optionObj.SetActive(false);
                //删除临时存档
                if (GameManager.Instance().bTemporary)
                {
                    HeroManager.Instance().HeroClear();
                    EnemyManager.Instance().EnemyClear();
                    GameManager.Instance().bTemporary = false;
                }
                //保存游戏
                if (GameManager.Instance().gameState == GameManager.GameState.Save)
                {
                    LevelManager.Instance().Clear();
                    GameManager.Instance().SaveGame(cursorIdx);
                    UpdateData();
                }
                //判断是否达到最大关卡
                if (DataManager.Value(GameManager.Instance().gameDataDic[cursorIdx].level) == GameManager.MAXLEVEL)
                {
                    uiState = State.Tips;
                    levelTips.SetActive(true);
                }
                else
                    GameManager.Instance().StartGame(cursorIdx);
            }
            else
            {
                cursorFinger.SetActive(false);
                tipsObj.SetActive(false);
                optionObj.SetActive(false);
                uiState = State.Normal;
            }
        }
    }

    /// <summary>
    /// 初始化选项 1.保存 2.读取
    /// </summary>
    private void InitOption()
    {
        if (GameManager.Instance().gameState == GameManager.GameState.Load)
        {
            option1Text.text = "开始";
            option2Text.text = "取消";
            tipsObj.SetActive(true);
        }
        else if (GameManager.Instance().gameState == GameManager.GameState.Save)
        {
            option1Text.text = "记录";
            option2Text.text = "取消";
        }
        optionObj.SetActive(true);
        cursorFinger.SetActive(true);
        uiState = State.Option;
        UpdateOption();
    }


    private void OnCancelDown()
    {
        if (uiState == State.Normal)
        {
            UIManager.Instance().CloseUIForms("StartGame");
            OpenUIForm("StartOption");
        }
        else if (uiState == State.Option)
        {
            cursorFinger.SetActive(false);
            optionObj.SetActive(false);
            tipsObj.SetActive(false);
            uiState = State.Normal;
        }
        else if (uiState == State.Tips)
        {
            levelTips.SetActive(false);
            uiState = State.Normal;
        }
    }

    public void UpdateOption()
    {
        if (uiState == State.Normal)
        {
            cursorSeq.Kill();
            float y = uiContent.cellSize.y + uiContent.spacing.y;
            optionCursor.transform.localPosition = new Vector3(-uiContent.cellSize.x / 2, uiContent.transform.localPosition.y - y * cursorIdx, 0);
            cursorSeq = DOTween.Sequence().
                Append(optionCursor.transform.DOMove(optionCursor.transform.position + new Vector3(0, -0.1f, 0), 0.7f)).
                Append(optionCursor.transform.DOMove(optionCursor.transform.position + new Vector3(0, 0f, 0), 0.7f)).
                SetLoops(-1);
        }
        else if (uiState == State.Option)
        {
            DOTween.Clear();
            cursorFinger.transform.localPosition = optionObj.transform.localPosition + new Vector3(-120 * optionIdx, 0, 0);
            Tween tw = cursorFinger.transform.DOMove(cursorFinger.transform.position + new Vector3(-0.1f, 0, 0), 0.7f);
            tw.SetLoops(-1);
        }
    }

}
