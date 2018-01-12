﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using Tiled2Unity;

public class MainManager : QMonoSingleton<MainManager>
{
    public static Vector2 PIVOT = new Vector2(8, -8);

    public HeroController curHero;  //当前选择的hero
    public EnemyController curEnemy;  //当前选择的enemy
    public HeroController curMouseHero;  //当前光标处的hero
    public EnemyController curMouseEnemy;  //光标处的enemy

    private GameObject mouseCursor;  //正常鼠标光标
    private int cursorIdx;  //光标当前位置的idx
    private Animator cursorAnimator;
    private TiledMap map;
    private int mapXNode;  //map横轴有多少个node
    private int mapYNode;  //map纵轴有多少个node
    private int nodeWidth;  //node的宽高
    private int nodeHeight;


    void Awake()
    {
        LevelManager.Instance().SetLevel(1);  //测试用
        HeroManager.Instance().Init(1);
        EnemyManager.Instance().Init(1);

        //初始化英雄回合
        SetHeroRound();
        //初始化光标
        InitMouseCursor();
        //注册事件
        RegisterKeyBoardEvent();
        CursorUpdate();
        UpdateUIPos();
    }

    /// <summary>
    /// 初始化光标，定义初始位置跟idx,添加到鼠标移动事件
    /// </summary>
    private void InitMouseCursor()
    {
        GameObject prefab = ResourcesMgr.Instance().LoadResource<GameObject>(MainProperty.NORMALCURSOR_PATH, true);
        mouseCursor = Instantiate<GameObject>(prefab);
        cursorIdx = HeroManager.Instance().GetHero(0).mID;
        mouseCursor.transform.position = Idx2Pos(cursorIdx);
        cursorAnimator = mouseCursor.GetComponent<Animator>();
    }

    /// <summary>
    /// 设置地图信息
    /// </summary>
    public void SetMapData(TiledMap map)
    {
        this.map = map;
        mapXNode = map.NumTilesWide;
        mapYNode = map.NumTilesHigh;
        nodeWidth = map.TileWidth;
        nodeHeight = map.TileHeight;
    }

    #region 回合设定
    /// <summary>
    /// hero回合初始化
    /// </summary>
    public void SetHeroRound()
    {
        Debug.Log("heroRound");
        HeroManager.Instance().SetHeroRound();
        EnemyManager.Instance().SetEnemyInit();
        RegisterKeyBoardEvent();
        if (mouseCursor)
        {
            mouseCursor.SetActive(true);
            CursorUpdate();
        }
        ShowAllUI();
    }

    /// <summary>
    /// enemy回合初始化
    /// </summary>
    public void SetEnemyRound()
    {
        Debug.Log("enemyRound");
        EnemyManager.Instance().SetEnemyRounnd();
        UnRegisterKeyBoradEvent();
        if (mouseCursor)
            mouseCursor.SetActive(false);
        HideAllUI();
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
        if (curHero)
            return;
        UIManager uiInstance = UIManager.Instance();
        //处理人物信息ui
        if (curMouseHero || curMouseEnemy)
        {
            if (Idx2ListPos(cursorIdx).y >= mapYNode / 2)
            {
                if (!uiInstance.GetUIActive("CharacterData_1"))
                {
                    uiInstance.ShowUIForms("CharacterData_1");               
                    uiInstance.CloseUIForms("CharacterData_2");
                }
                uiInstance.GetUI("CharacterData_1").GetComponent<CharacterDataView_1>().SetData();
            }
            else
            {
                if (!uiInstance.GetUIActive("CharacterData_2"))
                {
                    uiInstance.ShowUIForms("CharacterData_2");
                    uiInstance.CloseUIForms("CharacterData_1");
                }
                uiInstance.GetUI("CharacterData_2").GetComponent<CharacterDataView_2>().SetData();
            }
        }
        else
        {
            uiInstance.CloseUIForms("CharacterData_2");
            uiInstance.CloseUIForms("CharacterData_1");
        }    
        //地图MapNode
        if (Idx2ListPos(cursorIdx).x >= mapXNode / 2)
        {
            //一四象限
            if (uiInstance.GetUIActive("CharacterData_2"))
            {
                uiInstance.ShowUIForms("LandData_2");
                uiInstance.GetUI("LandData_2").GetComponent<LandDataView_2>().UpdataData(GetMapNode(cursorIdx));
                uiInstance.CloseUIForms("LandData_1");
                uiInstance.ShowUIForms("GameGoal_1");
                uiInstance.CloseUIForms("GameGoal_2");
            }
            else
            {
                uiInstance.ShowUIForms("LandData_1");
                uiInstance.GetUI("LandData_1").GetComponent<LandDataView_1>().UpdataData(GetMapNode(cursorIdx));
                uiInstance.CloseUIForms("LandData_2");
                //第四象限
                if (Idx2ListPos(cursorIdx).y >= mapYNode / 2)
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
            uiInstance.GetUI("LandData_2").GetComponent<LandDataView_2>().UpdataData(GetMapNode(cursorIdx));
            uiInstance.CloseUIForms("LandData_1");
        }  
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
            position = Idx2Pos(cursorIdx);
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
                hero.Selected();
                hero.SetAnimator("bSelected", hero.bSelected);
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
                    GetMapNode(curHero.mID).locatedHero = null;
                    node.locatedHero = curHero;
                    curHero.MoveTo(node.GetID());
                }
                else
                {
                    if (node.GetID() != curHero.mID)
                        return;
                    GetMapNode(curHero.mID).locatedHero = null;
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
        if (!IsInMap(position))
            return;
        if (!IsChangeIdx(position))
            return;
        mouseCursor.transform.position = Idx2Pos(cursorIdx);
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
        if (Idx2ListPos(id).x >= mapXNode / 2)
        {
            //一四象限
            uiInstance.ShowUIForms("LandData_1");
            uiInstance.GetUI("LandData_1").GetComponent<LandDataView_1>().UpdataData(GetMapNode(id));
            uiInstance.CloseUIForms("LandData_2");
            //第四象限
            if (Idx2ListPos(id).y >= mapYNode / 2)
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
            uiInstance.GetUI("LandData_2").GetComponent<LandDataView_2>().UpdataData(GetMapNode(id));
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
    }

    /// <summary>
    /// 上下左右;确定取消;
    /// </summary>
    private void TurnUp()
    {
        int id = cursorIdx - mapXNode;
        if (IsInMap(id))
        {
            cursorIdx = id;
            mouseCursor.transform.position = Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            //选择hero情况下，显示路径
            if (curHero)
            {
                if (GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }

    private void TurnDown()
    {
        int id = cursorIdx + mapXNode;
        if (IsInMap(id))
        {
            cursorIdx = id;
            mouseCursor.transform.position = Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            if (curHero)
            {
                if (GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }

    private void TurnLeft()
    {
        int id = cursorIdx - 1;
        int nowRow = (int)Idx2ListPos(cursorIdx).y;
        int turnRow = (int)Idx2ListPos(id).y;
        if (IsInMap(id) && nowRow == turnRow)
        {
            cursorIdx = id;
            mouseCursor.transform.position = Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            if (curHero)
            {
                if (GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }

    private void TurnRight()
    {
        int id = cursorIdx + 1;
        int nowRow = (int)Idx2ListPos(cursorIdx).y;
        int turnRow = (int)Idx2ListPos(id).y;
        if (IsInMap(id) && nowRow == turnRow)
        {
            cursorIdx = id;
            mouseCursor.transform.position = Idx2Pos(id);
            CursorUpdate();
            UpdateUIPos();
            if (curHero)
            {
                if (GetMapNode(id).canMove)
                    MoveManager.Instance().ShowRoad(id);
            }
        }
    }
    
    /// <summary>
    /// 确认事件
    /// </summary>
    private void TurnConfirm()
    {
        //判断当前是否有选择hero
        if (!curHero)
        {
            if (GetMapNode(cursorIdx).locatedHero)
            {
                HeroController hero = GetMapNode(cursorIdx).locatedHero;
                curHero = hero;
                hero.Selected();
            }
        }
        else
        {
            MapNode node = GetMapNode(cursorIdx);
            //如果点击的块在移动范围内且没有其他人物则可以移动
            if (node.bVisited && !node.locatedEnemy && !node.locatedHero)
            {
                node.locatedHero = curHero;  
                curHero.MoveTo(node.GetID());
            }
            //是否点击当前hero
            if (node.locatedHero == curHero)
            {
                node.locatedHero = curHero;
                curHero.MoveTo(node.GetID());
            }
        }      
        //判断当前是否有选择enemy
        if (!curEnemy)
        {
            if (GetMapNode(cursorIdx).locatedEnemy)
            {
                EnemyController enemy = GetMapNode(cursorIdx).locatedEnemy;
                curEnemy = enemy;
                enemy.Selected();
            }
        }
        else
        {
            curEnemy.CancelSelected();
            curEnemy = null;
        }
    }

    /// <summary>
    /// 取消事件
    /// </summary>
    private void TurnCancel()
    {
        if (curHero)
        {
            curHero.CancelSelected();
            GetMapNode(curHero.mID).locatedHero = curHero;
            SetCursorPos(curHero.mID);
            curHero = null;
            CursorUpdate();
            UpdateUIPos();
        }
        else if (curEnemy)
        {
            curEnemy.CancelSelected();
            curEnemy = null;
        }
    }

    /// <summary>
    /// 光标移动后的update
    /// </summary>
    public void CursorUpdate()
    {
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
        if (GetMapNode(cursorIdx).locatedHero)
        {
            //判断当前是否有选择hero
            if (!curHero)
            {
                //鼠标进入hero
                curMouseHero = GetMapNode(cursorIdx).locatedHero;
                curMouseHero.SetAnimator("bMouse", true);
                if (mouseCursor.activeSelf)
                    cursorAnimator.SetBool("Hero", true);
            }
        }

        //当前块是否有ennemy
        if (GetMapNode(cursorIdx).locatedEnemy )
        {
            if (!curHero)
            {
                //鼠标进入enemy
                curMouseEnemy = GetMapNode(cursorIdx).locatedEnemy;
            }  
        }
    }

    #endregion

    #region 公有方法:判断是否在地图内;转换idx跟pos;UI显示;判断两个idx的方位;设置光标可见性
    /// <summary>
    /// 判断是否在地图内
    /// </summary>
    public bool IsInMap(int idx)
    {
        int count = mapXNode * mapYNode;
        if (idx >= count || idx < 0)
            return false;
        else
            return true;
    }

    public bool IsInMap(Vector3 pos)
    {
        if (map.GetMapRect().Contains(pos))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 根据pos计算idx
    /// </summary>
    public int Pos2Idx(Vector3 pos)
    {
        int x = (int)(pos.x / nodeWidth);
        int y = (int)(Mathf.Abs(pos.y / nodeHeight));
        int idx = x + y * mapXNode;
        return idx;
    }
    
    /// <summary>
    /// 根据idx计算pos,锚点为中间
    /// </summary>
    public Vector3 Idx2Pos(int idx)
    {
        int x = idx % mapXNode;
        int y = idx / mapXNode;
        Vector3 pos = new Vector3(x * nodeWidth + PIVOT.x, -(y * nodeHeight - PIVOT.y), 0);
        return pos;
    }

    public Vector3 Idx2Pos2(int idx)
    {
        int x = idx % mapXNode;
        int y = idx / mapXNode;
        Vector3 pos = new Vector3(x * nodeWidth, -(y * nodeHeight), 0);
        return pos;
    }

    /// <summary>
    /// 获取几行几列
    /// </summary>
    public int GetXNode()
    {
        return mapXNode;
    }

    public int GetYNode()
    {
        return mapYNode;
    }

    /// <summary>
    /// 将idx转换成在list中的pos
    /// </summary>
    public Vector2 Idx2ListPos(int idx)
    {
        int x = idx % mapXNode;
        int y = idx / mapXNode;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 获取图块
    /// </summary>
    public MapNode GetMapNode(int idx)
    {
        if (!IsInMap(idx))
            return null;
        return MoveManager.Instance().GetMapNode(idx);
    }

    /// <summary>
    /// 设置光标位置
    /// </summary>
    public void SetCursorPos(int idx)
    {
        if (cursorIdx == idx)
            return;
        cursorIdx = idx;
        mouseCursor.transform.position = Idx2Pos(idx);
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
        Vector2 pos1 = Idx2ListPos(idx1);
        Vector2 pos2 = Idx2ListPos(idx2);
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

    #endregion

    /// <summary>
    /// 判断光标idx是否改变
    /// </summary>
    private bool IsChangeIdx(Vector3 pos)
    {
        int idx = Pos2Idx(pos);
        if (idx != cursorIdx)
        {
            cursorIdx = idx;
            return true;
        }
        else
            return false;
    }


}
