﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using Tiled2Unity;

public class MainManager : QMonoSingleton<MainManager>
{
    public enum MainState
    {
        Normal,       //普通
        SelectedHero, //选中Hero
        SelectedEnemy,//选中Enemy
        UseItem,      //使用道具
        CheckLand,    //检查地形
        AttackLand,   //攻击地形
        NormalAttack, //攻击
        Stop,         //停止操作
    }
    //当前选择的hero
    public HeroController curHero;
    //当前选择的enemy
    public EnemyController curEnemy;  
    public MapNode curNode;
    //当前光标处的hero
    public HeroController curMouseHero;  
    //光标处的enemy
    public EnemyController curMouseEnemy;  
    //当前回合
    public bool heroRound;
    public MainState mainState;

    //正常鼠标光标
    private GameObject mouseCursor;
    //光标当前位置的idx
    private int cursorIdx;  
    private Animator cursorAnimator;
    private LevelManager levelInstance;
    //定义需要的数据格式
    private SelectRoleData selectHero = new SelectRoleData();
    private SelectRoleData selectEnemy = new SelectRoleData();
    private UIRoleData uiRole = new UIRoleData();
    private UIMapNodeData uiNode = new UIMapNodeData();
    private MessageCenter msgCenter;

    void Awake()
    {
        levelInstance = LevelManager.Instance();
        msgCenter = MessageCenter.Instance();
    }

    public void Init()
    {
        HeroManager.Instance().Init();
        EnemyManager.Instance().Init();
        //初始化英雄回合
        SetHeroRound();
        //初始化光标
        InitMouseCursor();
        CursorUpdate();
        UpdateUIPos();
        mainState = MainState.Normal;
        AddEventListener();
    }

    public void Clear()
    {
        curHero = null;
        curEnemy = null;
        curMouseHero = null;
        curMouseEnemy = null;
        if (GameManager.Instance().gameState == GameManager.GameState.Fail)
            HeroManager.Instance().HeroClear();
        else if (GameManager.Instance().gameState == GameManager.GameState.Success)
            HeroManager.Instance().SaveHeroClear();
        EnemyManager.Instance().EnemyClear();
        HideAllUI();
        UnRegisterKeyBoradEvent();
        ResourcesMgr.Instance().PushPool(mouseCursor, MainProperty.NORMALCURSOR_PATH);
        RemoveEventListener();
    }

    /// <summary>
    /// 初始化光标，定义初始位置跟idx,添加到鼠标移动事件
    /// </summary>
    private void InitMouseCursor()
    {
        mouseCursor = ResourcesMgr.Instance().GetPool(MainProperty.NORMALCURSOR_PATH);
        cursorIdx = HeroManager.Instance().GetHero(0).mIdx;
        curNode = levelInstance.GetMapNode(cursorIdx);
        mouseCursor.transform.position = LevelManager.Instance().Idx2Pos(cursorIdx);
        cursorAnimator = mouseCursor.GetComponent<Animator>();
    }

    public void TemporarySave()
    {
        UnRegisterKeyBoradEvent();
        HideAllUI();
        RemoveEventListener();

        ResourcesMgr.Instance().PushPool(mouseCursor, MainProperty.NORMALCURSOR_PATH); 
        HeroManager.Instance().TemporarySave();
        EnemyManager.Instance().TemporarySave();
    }

    public void ContinueGame()
    {
        HeroManager.Instance().Continue();
        EnemyManager.Instance().Continue();
        RegisterKeyBoardEvent();
        ShowAllUI();
        InitMouseCursor();
        CursorUpdate();
        UpdateUIPos();
        mainState = MainState.Normal;
        heroRound = true;
        AddEventListener();
    }

    #region 游戏事件
    /// <summary>
    /// 添加订阅事件
    /// </summary>
    public void AddEventListener()
    {
        msgCenter.AddListener(EventType.UPDATEROUND, UpdateRound);
        msgCenter.AddListener(EventType.HEROSTANDBY, HeroStandby);
        msgCenter.AddListener(EventType.UPDATECURHERO, UpdateCurHero);
        msgCenter.AddListener(EventType.UPDATECURENEMY, UpdateCurEnemy);
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    public void RemoveEventListener()
    {
        msgCenter.RemoveListener(EventType.UPDATEROUND, UpdateRound);
        msgCenter.RemoveListener(EventType.HEROSTANDBY, HeroStandby);
        msgCenter.RemoveListener(EventType.UPDATECURHERO, UpdateCurHero);
        msgCenter.RemoveListener(EventType.UPDATECURENEMY, UpdateCurEnemy);
    }

    /// <summary>
    /// 更新回合
    /// </summary>
    private void UpdateRound(MessageEvent e)
    {
        curHero = null;
        UnRegisterKeyBoradEvent();
        SetCursorActive(false);
        HideAllUI();
        UIManager.Instance().ShowUIForms("Round");
    }

    /// <summary>
    /// Hero待机，继续注册键盘事件，恢复UI
    /// </summary>
    private void HeroStandby(MessageEvent e)
    {
        curHero = null;
        SetCursorActive(true);
        CursorUpdate();
        ShowAllUI();
        RegisterKeyBoardEvent();
    }

    /// <summary>
    /// 更新当前Hero
    /// </summary>
    private void UpdateCurHero(MessageEvent e)
    {
        UpdateCurHero data = (UpdateCurHero)e.Data;
        curHero = data.hero;
        HideAllUI();
        mainState = MainState.SelectedHero;
    }

    /// <summary>
    /// 更新选择的Enemy
    /// </summary>
    /// <param name="e"></param>
    private void UpdateCurEnemy(MessageEvent e)
    {
        UpdateCurEnemy data = (UpdateCurEnemy)e.Data;
        curEnemy = data.enemy;
        HideAllUI();
        mainState = MainState.SelectedEnemy;
    }
    #endregion

    #region 回合设定
    /// <summary>
    /// hero回合初始化
    /// </summary>
    public void SetHeroRound()
    {
        UIManager.Instance().CloseUIForms("Round");
        Debug.Log("heroRound");
        heroRound = true;
        HeroManager.Instance().SetHeroRound();
        EnemyManager.Instance().SetEnemyInit();
        HeroManager.Instance().CheckLand();
    }

    /// <summary>
    /// 检查地形结束
    /// </summary>
    public void CheckEnd()
    {
        curHero = null;
        curEnemy = null;
        mainState = MainManager.MainState.Normal;
        RegisterKeyBoardEvent();
        if (mouseCursor)
        {
            mouseCursor.SetActive(true);
            SetCursorPos(HeroManager.Instance().GetHero(0).mIdx);
            CursorUpdate();
        }
        ShowAllUI();
    }

    /// <summary>
    /// enemy回合初始化
    /// </summary>
    public void SetEnemyRound()
    {
        UIManager.Instance().CloseUIForms("Round");
        Debug.Log("enemyRound");
        heroRound = false;
        EnemyManager.Instance().SetEnemyRounnd();
        HideAllUI();
        HeroManager.Instance().SetHeroRound();
    }
    #endregion

    #region UI相关
    /// <summary>
    /// 显示所有game相关UI
    /// </summary>
    public void ShowAllUI()
    {
        UpdateUIPos();
    }

    /// <summary>
    /// 隐藏相关UI
    /// </summary>
    public void HideAllUI()
    {
        UIManager.Instance().CloseUIForms("LandData_1");
        UIManager.Instance().CloseUIForms("LandData_2");
        UIManager.Instance().CloseUIForms("GameGoal_1");
        UIManager.Instance().CloseUIForms("GameGoal_2");
        UIManager.Instance().CloseUIForms("CharacterData_1");
        UIManager.Instance().CloseUIForms("CharacterData_2");
    }

    /// <summary>
    /// 更新UIpos
    /// </summary>
    public void UpdateUIPos()
    {
        if (mainState == MainState.SelectedHero)
            return;
        UIManager uiInstance = UIManager.Instance();
        //处理人物信息ui
        if (curMouseHero || curMouseEnemy)
        {
            if (curMouseHero)
                uiRole.role = curMouseHero;
            else if (curMouseEnemy)
                uiRole.role = curMouseEnemy;
            if (levelInstance.Idx2ListPos(cursorIdx).y >= levelInstance.mapYNode / 2)
            {
                if (!uiInstance.GetUIActive("CharacterData_1"))
                {
                    uiInstance.ShowUIForms("CharacterData_1");               
                    uiInstance.CloseUIForms("CharacterData_2");
                }
            }
            else
            {
                if (!uiInstance.GetUIActive("CharacterData_2"))
                {
                    uiInstance.ShowUIForms("CharacterData_2");
                    uiInstance.CloseUIForms("CharacterData_1");
                }
            }
            MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.UPDATEROLEUI, uiRole));
        }
        else
        {
            uiInstance.CloseUIForms("CharacterData_2");
            uiInstance.CloseUIForms("CharacterData_1");
        }    
        //地图MapNode
        if (levelInstance.Idx2ListPos(cursorIdx).x >= levelInstance.mapXNode / 2)
        {
            //一四象限
            if (uiInstance.GetUIActive("CharacterData_2"))
            {
                uiInstance.ShowUIForms("LandData_2");
                uiInstance.CloseUIForms("LandData_1");
                uiInstance.ShowUIForms("GameGoal_1");
                uiInstance.CloseUIForms("GameGoal_2");
            }
            else
            {
                uiInstance.ShowUIForms("LandData_1");
                uiInstance.CloseUIForms("LandData_2");
                //第四象限
                if (levelInstance.Idx2ListPos(cursorIdx).y >= levelInstance.mapYNode / 2)
                {
                    uiInstance.ShowUIForms("GameGoal_1");
                    uiInstance.CloseUIForms("GameGoal_2");
                }
                else
                {
                    uiInstance.ShowUIForms("GameGoal_2");
                    uiInstance.CloseUIForms("GameGoal_1");
                }
            }      
        }
        else
        {
            //二三象限;需要判断GameGoalUI是否在第四象限
            if (uiInstance.GetUIActive("GameGoal_2"))
            {
                uiInstance.ShowUIForms("GameGoal_1");
                uiInstance.CloseUIForms("GameGoal_2");
            }
            else
                uiInstance.ShowUIForms("GameGoal_1");
            uiInstance.ShowUIForms("LandData_2");
            uiInstance.CloseUIForms("LandData_1");
        }
        //通知更新landData
        uiNode.node = curNode;
        MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.UPDATEMAPNODEUI, uiNode));
    }
    #endregion

    #region 鼠标操作
    /// <summary>
    /// 点击获取hit
    /// </summary>
    public RaycastHit2D Click(string layer, bool IsMouseClick = true)
    {
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector2 position = Vector2.zero;
        if(IsMouseClick)
            position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        else
            position = levelInstance.Idx2Pos(cursorIdx);
        RaycastHit2D hit = new RaycastHit2D();
        LayerMask mask = -1;
        mask = LayerMask.GetMask(layer);
        hit = Physics2D.Raycast(position, Vector2.zero, 0, mask);
        return hit;
    }
    /// <summary>
    /// 注册player鼠标点击事件
    /// </summary>
    public void RegisterClickEvent()
    {
        InputManager.Instance().RegisterMouseClickEvent(PlayerClick, EventType.PLAYER_CLICK);
    }

    /// <summary>
    /// 取消Player鼠标点击事件
    /// </summary>
    public void UnregisterClickEvent()
    {
        InputManager.Instance().RegisterMouseClickEvent(PlayerClick, EventType.PLAYER_CLICK, false);
    }

    /// <summary>
    /// player点击事件
    /// </summary>
    private void PlayerClick()
    {
        if (!IsInMap())
            return;
        //判断当前是否有选择hero
        if (curHero == null)
        {
            RaycastHit2D hit = Click(MainProperty.LAYER_CHARACTER);
            if (hit == null || hit.collider == null)
                return;
            if (hit.collider.CompareTag("Hero"))
            {
                HeroController hero = hit.collider.GetComponent<HeroController>();
                //hero.Selected();
                //hero.SetAnimator("bSelected", hero.bSelected);
                hero.SetAnimator("bNormal", false);
            }
        }
        else
        {
            RaycastHit2D hit2 = Click(MainProperty.LAYER_MAP);
            if (hit2 == null || hit2.collider == null)
                return;
            //如果点击的块在移动范围内且没有其他人物则可以移动
            MapNode node = hit2.collider.GetComponent<MapNode>();
            if (node.bVisited && !node.locatedEnemy)
            {
                if (!node.locatedHero)
                {
                    levelInstance.GetMapNode(curHero.mIdx).locatedHero = null;
                    node.locatedHero = curHero;
                    curHero.MoveTo(node.GetID());
                }
                else
                {
                    if (node.GetID() != curHero.mIdx)
                        return;
                    levelInstance.GetMapNode(curHero.mIdx).locatedHero = null;
                    node.locatedHero = curHero;
                    curHero.MoveTo(node.GetID());
                }
            }
        }
    }

    private bool IsInMap()
    {
        RaycastHit2D hit = Click(MainProperty.LAYER_MAP);
        if (hit && hit.collider)
            return true;
        else
            return false;
    }

    private void MouseMove()
    {
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector2 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (!levelInstance.IsInMap(position))
            return;
        //if (!IsChangeIdx(position))
        //    return;
        mouseCursor.transform.position = levelInstance.Idx2Pos(cursorIdx);
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, 0, LayerMask.GetMask("Character"));
        if (hit)
            MouseEnterCharacter(hit);
        else
        {
            //鼠标移出hero
            if (curMouseHero != null)
            {
                //curMouseHero.Moved(false);
                curMouseHero = null;
            }
            if (mouseCursor.activeSelf)
                cursorAnimator.SetBool("Hero", false);
            //鼠标移出enemy
            if (curMouseEnemy != null)
            {
                //curMouseEnemy.Moved(false);
                curMouseEnemy = null;
            }
        }
        //RaycastHit2D hit2 = Physics2D.Raycast(position, Vector2.zero, 0, LayerMask.GetMask("Map"));
    }

    /// <summary>
    /// 鼠标进入的各种情况
    /// </summary>
    private void MouseEnterCharacter(RaycastHit2D hit)
    {
        if (hit.collider.CompareTag("Hero"))
        {
            //鼠标进入hero
            curMouseHero = hit.collider.GetComponent<HeroController>();
            //curMouseHero.Moved(true);
            if (mouseCursor.activeSelf)
                cursorAnimator.SetBool("Hero", true);
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            //鼠标进入enemy
            curMouseEnemy = hit.collider.GetComponent<EnemyController>();
            if (!curMouseEnemy)
                return;
           // curMouseEnemy.Moved(true);
        }
    }

    /// <summary>
    /// 鼠标进入mapnode,更新UI的pos
    /// </summary>
    private void MouseEnterMapNode(RaycastHit2D hit)
    {
        UIManager uiInstance = UIManager.Instance();
        //地图MapNode
        int id = hit.collider.GetComponent<MapNode>().GetID();
        if (levelInstance.Idx2ListPos(id).x >= levelInstance.mapXNode / 2)
        {
            //一四象限
            uiInstance.ShowUIForms("LandData_1");
            uiInstance.CloseUIForms("LandData_2");
            //第四象限
            if (levelInstance.Idx2ListPos(id).y >= levelInstance.mapYNode / 2)
            {
                uiInstance.ShowUIForms("GameGoal_1");
                uiInstance.CloseUIForms("GameGoal_2");
            }
            else
            {
                uiInstance.ShowUIForms("GameGoal_2");
                uiInstance.CloseUIForms("GameGoal_1");
            }
        }
        else
        {
            //二三象限;需要判断GameGoalUI是否在第四象限
            if (uiInstance.GetUIActive("GameGoal_2"))
            {
                uiInstance.ShowUIForms("GameGoal_1");
                uiInstance.CloseUIForms("GameGoal_2");
            }
            uiInstance.ShowUIForms("LandData_2");
            uiInstance.CloseUIForms("LandData_1");
        }
    }

    public void AddMouseMoveEvent()
    {
        mouseCursor.SetActive(true);
        InputManager.Instance().RegisterMouseEnterEvent(MouseMove, EventType.PLAYER_MOVE);
    }

    public void DelMouseMoveEvent()
    {
        mouseCursor.SetActive(false);
        InputManager.Instance().RegisterMouseEnterEvent(MouseMove, EventType.PLAYER_MOVE, false);
    }
    #endregion

    #region 键盘操作
    /// <summary>
    /// 注册相应键盘事件
    /// </summary>
    public void RegisterKeyBoardEvent()
    {
        InputManager input = InputManager.Instance();
        input.RegisterKeyDownEvent(TurnUp, EventType.KEY_UPARROW);
        input.RegisterKeyDownEvent(TurnDown, EventType.KEY_DOWNARROW);
        input.RegisterKeyDownEvent(TurnLeft, EventType.KEY_LEFTARROW);
        input.RegisterKeyDownEvent(TurnRight, EventType.KEY_RIGHTARROW);
        input.RegisterKeyDownEvent(TurnConfirm, EventType.KEY_Z);
        input.RegisterKeyDownEvent(TurnCancel, EventType.KEY_X);
        input.RegisterKeyDownEvent(TurnRT, EventType.KEY_S);
    }

    /// <summary>
    /// 注销相应键盘事件
    /// </summary>
    public void UnRegisterKeyBoradEvent()
    {
        InputManager input = InputManager.Instance();
        input.UnRegisterKeyDownEvent(TurnUp, EventType.KEY_UPARROW);
        input.UnRegisterKeyDownEvent(TurnDown, EventType.KEY_DOWNARROW);
        input.UnRegisterKeyDownEvent(TurnLeft, EventType.KEY_LEFTARROW);
        input.UnRegisterKeyDownEvent(TurnRight, EventType.KEY_RIGHTARROW);
        input.UnRegisterKeyDownEvent(TurnConfirm, EventType.KEY_Z);
        input.UnRegisterKeyDownEvent(TurnCancel, EventType.KEY_X);
        input.UnRegisterKeyDownEvent(TurnRT, EventType.KEY_S);
    }

    /// <summary>
    /// 上下左右;确定取消;
    /// </summary>
    private void TurnUp()
    {
        int id = cursorIdx - levelInstance.mapXNode;
        if (levelInstance.IsInMap(id))
        {
            cursorIdx = id;
            mouseCursor.transform.position = levelInstance.Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            //选择hero情况下，显示路径
            if (mainState == MainState.SelectedHero)
            {
                if (levelInstance.GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }

    private void TurnDown()
    {
        int id = cursorIdx + levelInstance.mapXNode;
        if (levelInstance.IsInMap(id))
        {
            cursorIdx = id;
            mouseCursor.transform.position = levelInstance.Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            if (mainState == MainState.SelectedHero)
            {
                if (levelInstance.GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }

    private void TurnLeft()
    {
        int id = cursorIdx - 1;
        int nowRow = (int)levelInstance.Idx2ListPos(cursorIdx).y;
        int turnRow = (int)levelInstance.Idx2ListPos(id).y;
        if (levelInstance.IsInMap(id) && nowRow == turnRow)
        {
            cursorIdx = id;
            mouseCursor.transform.position = levelInstance.Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            if (mainState == MainState.SelectedHero)
            {
                if (levelInstance.GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }

    private void TurnRight()
    {
        int id = cursorIdx + 1;
        int nowRow = (int)levelInstance.Idx2ListPos(cursorIdx).y;
        int turnRow = (int)levelInstance.Idx2ListPos(id).y;
        if (levelInstance.IsInMap(id) && nowRow == turnRow)
        {
            cursorIdx = id;
            mouseCursor.transform.position = levelInstance.Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            if (mainState == MainState.SelectedHero)
            {
                if (levelInstance.GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }
    
    /// <summary>
    /// 确认事件
    /// </summary>
    private void TurnConfirm()
    {
        switch (mainState)
        {
            case MainState.Normal:
                {
                    if (curNode.locatedHero)
                    {
                        //选择Hero
                        selectHero.id = curNode.locatedHero.listIdx;
                        MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.SELECTHERO, selectHero));
                    }
                    else if (curNode.locatedEnemy)
                    {
                        //选择Enemy
                        selectEnemy.id = curNode.locatedEnemy.listIdx;
                        MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.SELECTENEMY, selectEnemy));
                    }
                    else
                    {
                        UnRegisterKeyBoradEvent();
                        HideAllUI();
                        UIManager.Instance().ShowUIForms("GameOption");
                    }
                }
                break;
            case MainState.SelectedHero:
                {
                    //如果点击的块在移动范围内且没有其他人物则可以移动或者是点击当前hero
                    if (curNode.bVisited && !curNode.locatedEnemy && !curNode.locatedHero || curNode.locatedHero == curHero)
                    {
                        UnRegisterKeyBoradEvent();
                        SetCursorActive(false);
                        curMouseHero = null;
                        curHero.MoveTo(curNode.GetID());
                    }
                }
                break;
            case MainState.SelectedEnemy:
                {
                    curEnemy.CancelSelected();
                    curEnemy = null;
                    mainState = MainState.Normal;
                }
                break;
            default:
                return;
        }    
    }

    /// <summary>
    /// 取消事件
    /// </summary>
    private void TurnCancel()
    {
        switch (mainState)
        {
            case MainState.SelectedHero:
                {
                    curHero.CancelSelected();
                    SetCursorPos(curHero.mIdx);
                    curHero = null;
                    CursorUpdate();
                    UpdateUIPos();
                    mainState = MainState.Normal;
                }
                break;
            case MainState.SelectedEnemy:
                {
                    curEnemy.CancelSelected();
                    curEnemy = null;
                    mainState = MainState.Normal;
                }
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 显示信息
    /// </summary>
    private void TurnRT()
    {
        if (!curMouseHero && !curMouseEnemy)
            return;
        UnRegisterKeyBoradEvent();
        HideAllUI();
        UIManager.Instance().ShowUIForms("RoleData");
    }

    /// <summary>
    /// 光标移动后的update
    /// </summary>
    public void CursorUpdate()
    {
        curNode = levelInstance.GetMapNode(cursorIdx);
        if (curMouseHero)
        {
            curMouseHero.SetAnimator("bMouse", false);
            curMouseHero = null;
            cursorAnimator.SetBool("Hero", false);
        }
        if (curMouseEnemy)
        {
            curMouseEnemy = null;
        }

        //当前mapNode是否有hero
        if (curNode.locatedHero)
        {
            //判断当前是否有选择hero
            if (!curHero)
            {
                //鼠标进入hero
                curMouseHero = curNode.locatedHero;
                curMouseHero.SetAnimator("bMouse", true);
                if (mouseCursor.activeSelf)
                    cursorAnimator.SetBool("Hero", true);
            }
        }
        //当前块是否有ennemy
        else if (curNode.locatedEnemy)
        {
            if (!curHero)
            {
                //鼠标进入enemy
                curMouseEnemy = curNode.locatedEnemy;
            }  
        }
    }

    #endregion

    #region 公有方法:UI显示;判断两个idx的方位;设置光标可见性
    /// <summary>
    /// 设置光标位置
    /// </summary>
    public void SetCursorPos(int idx)
    {
        if (cursorIdx == idx)
            return;
        cursorIdx = idx;
        mouseCursor.transform.position = LevelManager.Instance().Idx2Pos(idx);
        CursorUpdate();
    }

    /// <summary>
    /// 获取光标idx
    /// </summary>
    public int GetCursorIdx()
    {
        return cursorIdx;
    }

    /// <summary>
    /// 设置光标可见性
    /// </summary>
    public void SetCursorActive(bool bval)
    {
        mouseCursor.SetActive(bval);
    }

    /// <summary>
    /// 获取第二个idx在第一个idx哪个方向
    /// </summary>
    public int GetIdx2IdxPos(int idx1, int idx2)
    {
        Vector2 pos1 = LevelManager.Instance().Idx2ListPos(idx1);
        Vector2 pos2 = LevelManager.Instance().Idx2ListPos(idx2);
        if (pos1.x != pos2.x && pos1.y != pos2.y)
            return -1;
        if (pos1.x == pos2.x)
        {
            //up & down
            if (pos1.y > pos2.y)
                return 1;
            else
                return 2;
        }
        else
        {
            //left & right
            if (pos1.x > pos2.x)
                return 3;
            else
                return 4;
        }
    }

    /// <summary>
    /// 人物信息页切换当前人物
    /// </summary>
    public void SetMouseRole(int id)
    {
        if (curMouseHero)
        {
            curMouseHero = HeroManager.Instance().liveHeroList[id];
            SetCursorPos(curMouseHero.mIdx);
        }
    }


    #endregion

}
