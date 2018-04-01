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
    public const string DELETETITLE = "删除记录";

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
        if (game.gameState == GameManager.GameState.Save)
            startTitle.text = SAVETITLE;
        else if (game.gameState == GameManager.GameState.Load)
            startTitle.text = LOADTITLE;
        else if (game.gameState == GameManager.GameState.Delete)
            startTitle.text = DELETETITLE;
        //1.三无 2.有记录
        if (!GameManager.Instance().HaveGameFile())
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
                    if (i == GameManager.Instance().curGameIdx && GameManager.Instance().gameDataDic[i].level != GameManager.MAXLEVEL.ToString())
                    {
                        recordObj.SetActive(true);
                        recordObj.transform.localPosition = new Vector3(uiContent.cellSize.x / 2 - 40, uiContent.transform.localPosition.y - y * i + 10, 0);
                    }
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
        RegisterKeyBoardEvent();
    }

    private void UnRegisterEvent()
    {
        UnRegisterKeyBoardEvent();
    }

    public override void OnUpArrowDown()
    {
        if (uiState == State.Normal)
        {
            cursorIdx -= 1;
            if (cursorIdx < 0)
                cursorIdx += objList.Count;
            UpdateOption();
        }
    }

    public override void OnDownArrowDown()
    {
        if (uiState == State.Normal)
        {
            cursorIdx += 1;
            if (cursorIdx >= objList.Count)
                cursorIdx -= objList.Count;
            UpdateOption();
        }
    }

    public override void OnLeftArrowDown()
    {
        if (uiState == State.Option)
        {
            optionIdx += 1;
            if (optionIdx >= OPTIONCOUNT)
                optionIdx -= OPTIONCOUNT;
            UpdateOption();
        }
    }

    public override void OnRightArrowDown()
    {
        if (uiState == State.Option)
        {
            optionIdx -= 1;
            if (optionIdx < 0)
                optionIdx += OPTIONCOUNT;
            UpdateOption();
        }
    }

    public override void OnConfirmDown()
    {
        GameManager game = GameManager.Instance();
        if (uiState == State.Normal)
        {
            if (game.gameState == GameManager.GameState.Load)
            {
                if (game.gameDataDic[cursorIdx].level == GameManager.NULLGAME.ToString())
                {
                    UIManager.Instance().CloseUIForms("StartGame");
                    game.StartGame(cursorIdx);
                }
                else
                {
                    if (game.gameDataDic[cursorIdx].level != GameManager.MAXLEVEL.ToString())
                        InitOption();
                    else
                    {
                        uiState = State.Tips;
                        levelTips.SetActive(true);
                    }
                }
            }
            else if(game.gameState == GameManager.GameState.Save)
            {
                InitOption();
            }
            else if (game.gameState == GameManager.GameState.Delete)
            {
                if (game.gameDataDic[cursorIdx].level != GameManager.NULLGAME.ToString())
                    InitOption();
            }
        }
        else if (uiState == State.Option)
        {
            if (optionIdx == 1)
            {
                optionIdx = 0;
                tipsObj.SetActive(false);
                cursorFinger.SetActive(false);
                optionObj.SetActive(false);
                if (GameManager.Instance().gameState == GameManager.GameState.Load || GameManager.Instance().gameState == GameManager.GameState.Save)
                {
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
                        recordObj.SetActive(false);
                        GameManager.Instance().bTemporary = false;
                    }
                    else
                        GameManager.Instance().StartGame(cursorIdx);
                }
                else if (GameManager.Instance().gameState == GameManager.GameState.Delete)
                {
                    if (cursorIdx == GameManager.Instance().curGameIdx)
                        recordObj.SetActive(false);
                    GameManager.Instance().DeleteGame(cursorIdx);
                    UpdateData();
                    uiState = State.Normal;
                    //若全空则回到startOption
                    if (!GameManager.Instance().HaveGameFile())
                    {
                        UIManager.Instance().CloseUIForms("StartGame");
                        OpenUIForm("StartOption");
                    }
                }
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
        else if (GameManager.Instance().gameState == GameManager.GameState.Delete)
        {
            option1Text.text = "删除";
            option2Text.text = "取消";
        }
        optionObj.SetActive(true);
        cursorFinger.SetActive(true);
        uiState = State.Option;
        UpdateOption();
    }


    public override void OnCancelDown()
    {
        if (uiState == State.Normal)
        {
            GameManager.Instance().gameState = GameManager.GameState.Login;
            UIManager.Instance().CloseUIForms("StartGame");
            OpenUIForm("StartOption");
        }
        else if (uiState == State.Option)
        {
            cursorFinger.SetActive(false);
            optionObj.SetActive(false);
            tipsObj.SetActive(false);
            uiState = State.Normal;
            optionIdx = 0;
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
