using System.Collections;
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
    private Queue<int> rangeQueue = new Queue<int>();  //用于计算移动范围
    [SerializeField]
    private List<MapNode> mapNodeList;  //地图的所有图块
    private List<int> rangeNodeList = new List<int>();  //移动范围的块的idx
    private List<GameObject> nodeObjList = new List<GameObject>();  //移动范围的块
    private List<int> endNodeList = new List<int>();  //记录周围有移动不了的块的idx
    private List<int> attackNodeList = new List<int>();  //攻击范围的块的idx
    private List<GameObject> attackObjList = new List<GameObject>();  //攻击范围的块
    private GameObject moveRangeObj;  //移动范围的块的parent
    private List<int> attackHeroList = new List<int>();  //在攻击范围内的hero
    //显示移动路径
    //移动路径obj容器
    private GameObject roadContent;
    //移动路径list
    private struct RoadObj
    {
        public RoadObj(string path, GameObject obj)
        {
            this.path = path;
            road = obj;
        }
        public string path;
        public GameObject road;
    }
    private List<int> roadIdxList = new List<int>();
    private List<RoadObj> roadObjList = new List<RoadObj>();

    void Awake()
    {
        LevelManager.Instance().SetLevel(1);  //测试用
        HeroManager.Instance().Init(1);
        EnemyManager.Instance().Init(1);

        moveRangeObj = new GameObject();
        moveRangeObj.name = "MoveRange";
        roadContent = new GameObject();
        roadContent.name = "Road";
        //初始化英雄回合
        SetHeroRound();
        //初始化光标
        InitMouseCursor();
        //注册事件
        RegisterKeyBoardEvent();
        CursorUpdate();
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
    public void SetMapData(TiledMap map, List<MapNode> mapNodeList)
    {
        this.map = map;
        mapXNode = map.NumTilesWide;
        mapYNode = map.NumTilesHigh;
        nodeWidth = map.TileWidth;
        nodeHeight = map.TileHeight;
        this.mapNodeList = mapNodeList;
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
    /// 更新UIpos
    /// </summary>
    public void UpdateUIPos()
    {
        if (curHero)
            return;
        UIManager uiInstance = UIManager.Instance();
        //地图MapNode
        if (Idx2ListPos(cursorIdx).x >= mapXNode / 2)
        {
            //一四象限
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
        else
        {
            //二三象限;需要判断GameGoalUI是否在第四象限
            if (uiInstance.GetUIActive("GameGoal_2"))
            {
                uiInstance.ShowUIForms("GameGoal_1");
                uiInstance.CloseUIForms("GameGoal_2");
            }
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
                curMouseHero.Moved(false);
                curMouseHero = null;
            }
            if (mouseCursor.activeSelf)
                cursorAnimator.SetBool("Hero", false);
            //鼠标移出enemy
            if (curMouseEnemy != null)
            {
                curMouseEnemy.Moved(false);
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
            curMouseHero.Moved(true);
            if (mouseCursor.activeSelf)
                cursorAnimator.SetBool("Hero", true);
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            //鼠标进入enemy
            curMouseEnemy = hit.collider.GetComponent<EnemyController>();
            if (!curMouseEnemy)
                return;
            curMouseEnemy.Moved(true);
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
            UpdateUIPos();
            CursorUpdate();
            //选择hero情况下，显示路径
            if (curHero)
            {
                //攻击的块不显示
                if (attackNodeList.Contains(id))
                    ShowRoad(cursorIdx + mapXNode);
                else
                    ShowRoad(id);
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
            UpdateUIPos();
            CursorUpdate();
            if (curHero)
            {
                if (attackNodeList.Contains(id))
                    ShowRoad(cursorIdx - mapXNode);
                else
                    ShowRoad(id);
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
            UpdateUIPos();
            CursorUpdate();
            if (curHero)
            {
                if (attackNodeList.Contains(id))
                    ShowRoad(cursorIdx + 1);
                else
                    ShowRoad(id);
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
            UpdateUIPos();
            CursorUpdate();
            if (curHero)
            {
                if (attackNodeList.Contains(id))
                    ShowRoad(cursorIdx - 1);
                else
                    ShowRoad(id);
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
                //隐藏路径
                ShowRoad(curHero.mID);
                GetMapNode(curHero.mID).locatedHero = null;
                node.locatedHero = curHero;  
                curHero.MoveTo(node.GetID());
            }
            //是否点击当前hero
            if (node.locatedHero == curHero)
            {
                GetMapNode(curHero.mID).locatedHero = null;
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
            curHero = null;
            CursorUpdate();
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
    private void CursorUpdate()
    {
        //当前mapNode是否有hero
        if (GetMapNode(cursorIdx).locatedHero)
        {
            //判断当前是否有选择hero
            if (curHero == null)
            {
                //鼠标进入hero
                curMouseHero = GetMapNode(cursorIdx).locatedHero;
                curMouseHero.Moved(true);
                if (mouseCursor.activeSelf)
                    cursorAnimator.SetBool("Hero", true);
            }
        }
        else
        {
            //鼠标移出hero
            if (curMouseHero != null)
            {
                curMouseHero.Moved(false);
                curMouseHero = null;
            }
            if (mouseCursor.activeSelf)
                cursorAnimator.SetBool("Hero", false);
        }

        //当前块是否有ennemy
        if (GetMapNode(cursorIdx).locatedEnemy && !curHero)
        {
            //鼠标进入enemy
            curMouseEnemy = GetMapNode(cursorIdx).locatedEnemy;
            curMouseEnemy.Moved(true);
        }
        else
        {
            //鼠标移出enemy
            if (curMouseEnemy != null)
            {
                curMouseEnemy.Moved(false);
                curMouseEnemy = null;
            }
        }
    }

    #endregion

    #region 攻击&移动范围;重置
    /// <summary>
    /// 显示移动范围
    /// </summary>
    public void ShowMoveRange(Vector2 pos, int moveRange, int attackRange)
    {
        DoMoveRange(pos, moveRange, attackRange);
        if (rangeNodeList.Count > 0)
        {
            for (int i = 0; i < rangeNodeList.Count; i++)
            {
                Vector3 nodePos = mapNodeList[rangeNodeList[i]].transform.position;
                GameObject node = GameObjectPool.Instance().GetPool(MainProperty.RANGENODE_PATH, nodePos);
                nodeObjList.Add(node);
                node.transform.SetParent(moveRangeObj.transform);
            }
        }
        ShowAttackRange();
    }

    /// <summary>
    /// 显示攻击范围
    /// </summary>
    public void ShowAttackRange()
    {
        if(curEnemy)
            AddAttackNodeInList(curEnemy.attackRange, curEnemy.mID);
        if(curHero)
            AddAttackNodeInList(curHero.attackRange, curHero.mID);
        if (attackNodeList.Count > 0)
        {
            for (int i = 0; i < attackNodeList.Count; i++)
            {
                Vector3 nodePos = mapNodeList[attackNodeList[i]].transform.position;
                GameObject node = GameObjectPool.Instance().GetPool(MainProperty.ATTACKNODE_PATH, nodePos);
                attackObjList.Add(node);
                node.transform.SetParent(moveRangeObj.transform);
            }
        }
    }

    /// <summary>
    /// 隐藏攻击范围
    /// </summary>
    public void HideAttackRange()
    {
        if (attackNodeList.Count > 0)
        {
            GameObjectPool.Instance().PushPool(attackObjList, MainProperty.ATTACKNODE_PATH);
            attackObjList.Clear();
        }
    }

    /// <summary>
    /// 隐藏移动范围
    /// </summary>
    public void HideMoveRange()
    {
        if(nodeObjList.Count > 0)
        {
            //moveRangeObj.SetActive(false);
            GameObjectPool.Instance().PushPool(nodeObjList, MainProperty.RANGENODE_PATH);
            nodeObjList.Clear();
            GameObjectPool.Instance().PushPool(attackObjList, MainProperty.ATTACKNODE_PATH);
            attackObjList.Clear();
            ReSet();
        }
    }

    /// <summary>
    /// 显示移动路径
    /// </summary>
    public void ShowRoad(int id)
    {
        //需要先隐藏
        roadIdxList.Clear();
        if (roadObjList.Count > 0)
        {
            for (int j = 0; j < roadObjList.Count; j++)
            {
                GameObjectPool.Instance().PushPool(roadObjList[j].road, roadObjList[j].path);
            }
        }
        roadObjList.Clear();
        //原点不需要显示
        if (id == curHero.mID)
            return;
        //获取当前路线idx
        MapNode node = GetMapNode(id);
        while (node.parentMapNode)
        {
            roadIdxList.Add(node.GetID());
            node = node.parentMapNode;
        }
        roadIdxList.Add(curHero.mID);
        //实例化路线
        //记录拐点
        int corner = 0;
        //记录上一次方向
        int dir = 0;
        int lastdir = 0;
        for (int j = 0; j < roadIdxList.Count - 1; j++)
        {
            //头
            if (j == 0)
            {
                string headpath = MainProperty.ROADHEAD_UP_PATH;
                string tailpath = MainProperty.ROADBODY_UP_PATH;
                dir = GetIdx2IdxPos(roadIdxList[j + 1], roadIdxList[j]);
                if (dir == -1)
                    return;
                else if (dir == 1)
                {
                    headpath = MainProperty.ROADHEAD_UP_PATH;
                    tailpath = MainProperty.ROADBODY_UP_PATH;
                }
                else if (dir == 2)
                {
                    headpath = MainProperty.ROADHEAD_DOWN_PATH;
                    tailpath = MainProperty.ROADBODY_UP_PATH;
                }
                else if (dir == 3)
                {
                    headpath = MainProperty.ROADHEAD_LEFT_PATH;
                    tailpath = MainProperty.ROADBODY_LEFT_PATH;
                }
                else if (dir == 4)
                {
                    headpath = MainProperty.ROADHEAD_RIGHT_PATH;
                    tailpath = MainProperty.ROADBODY_LEFT_PATH;
                }
                lastdir = dir;
                GameObject head = GameObjectPool.Instance().GetPool(headpath, Idx2Pos(roadIdxList[j]));
                head.transform.SetParent(roadContent.transform);
                roadObjList.Add(new RoadObj(headpath, head));
                GameObject tail = GameObjectPool.Instance().GetPool(tailpath, Idx2Pos(roadIdxList[j + 1]));
                tail.transform.SetParent(roadContent.transform);
                roadObjList.Add(new RoadObj(tailpath, tail));
            }
            else
            {
                string headpath = MainProperty.ROADHEAD_UP_PATH;
                string tailpath = MainProperty.ROADBODY_UP_PATH;
                dir = GetIdx2IdxPos(roadIdxList[j + 1], roadIdxList[j]);
                if (dir == -1)
                    return;
                else if (dir == 1)
                    tailpath = MainProperty.ROADBODY_UP_PATH;
                else if (dir == 2)
                    tailpath = MainProperty.ROADBODY_UP_PATH;
                else if (dir == 3)
                    tailpath = MainProperty.ROADBODY_LEFT_PATH;
                else if (dir == 4)
                    tailpath = MainProperty.ROADBODY_LEFT_PATH;
                //如果与上一次方向不一样
                if (dir != lastdir)
                {
                    //上一次是上的话，这次只可能是左/右
                    if (lastdir == 1)
                    {
                        if (dir == 3)
                            headpath = MainProperty.ROADCORNER_RIGHT2_PATH;
                        else
                            headpath = MainProperty.ROADCORNER_LEFT2_PATH;
                    }
                    else if (lastdir == 2)
                    {
                        if (dir == 3)
                            headpath = MainProperty.ROADCORNER_RIGHT1_PATH;
                        else
                            headpath = MainProperty.ROADCORNER_LEFT1_PATH;
                    }
                    else if (lastdir == 3)
                    {
                        if (dir == 1)
                            headpath = MainProperty.ROADCORNER_LEFT1_PATH;
                        else
                            headpath = MainProperty.ROADCORNER_LEFT2_PATH;
                    }
                    else if (lastdir == 4)
                    {
                        if (dir == 1)
                            headpath = MainProperty.ROADCORNER_RIGHT2_PATH;
                        else
                            headpath = MainProperty.ROADCORNER_RIGHT1_PATH;
                    }
                    lastdir = dir;
                    RoadObj road = roadObjList[roadObjList.Count - 1];
                    GameObjectPool.Instance().PushPool(road.road, road.path);
                    GameObject head = GameObjectPool.Instance().GetPool(headpath, Idx2Pos(roadIdxList[j]));
                    head.transform.SetParent(roadContent.transform);
                    roadObjList[roadObjList.Count - 1] = new RoadObj(headpath, head);

                    GameObject tail = GameObjectPool.Instance().GetPool(tailpath, Idx2Pos(roadIdxList[j + 1]));
                    tail.transform.SetParent(roadContent.transform);
                    roadObjList.Add(new RoadObj(tailpath, tail));
                }
                else
                {
                    if(dir == 1 || dir == 2)
                        tailpath = MainProperty.ROADBODY_UP_PATH;
                    else if(dir == 3 || dir == 4)
                        tailpath = MainProperty.ROADBODY_LEFT_PATH;
                    GameObject tail = GameObjectPool.Instance().GetPool(tailpath, Idx2Pos(roadIdxList[j + 1]));
                    tail.transform.SetParent(roadContent.transform);
                    roadObjList.Add(new RoadObj(tailpath, tail));
                }
            }
        }
    }

    /// <summary>
    /// 获取攻击块的idx
    /// </summary>
    public List<int> GetAttackHeroList()
    {
        return attackHeroList;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void ReSet()
    {
        for (int i = 0; i < mapNodeList.Count; i++)
            mapNodeList[i].ReSet();
        endNodeList.Clear();
        rangeNodeList.Clear();
        attackNodeList.Clear();
        attackHeroList.Clear();
        roadIdxList.Clear();
    }

    /// <summary>
    /// 计算移动范围
    /// </summary>
    public void DoMoveRange(Vector2 pos, int moveRange, int attackRange)
    {
        ReSet();
        int id = Pos2Idx(pos);
        mapNodeList[id].bVisited = true;
        rangeQueue.Enqueue(id);
        int range = 0;
        //能移动到的index
        while (rangeQueue.Count != 0)
        {
            int w = rangeQueue.Dequeue();
            int nowRow = w / mapXNode;
            //判断走了几步
            MapNode parent = mapNodeList[w];
            if (parent.parentMapNode != null)
            {
                range++;
                parent = parent.parentMapNode;
            }

            //未超出移动范围
            if (range <= moveRange)
            {
                if (IsInMap(w + 1) && !mapNodeList[w + 1].bVisited && (w + 1) / mapXNode == nowRow)  //如果右边节点存在且未被访问
                {
                    if (mapNodeList[w].moveValue + mapNodeList[w + 1].nodeValue <= moveRange)  //未超出移动能力
                    {
                        //当前块是否有敌人，有则记录，无则显示移动范围
                        if (mapNodeList[w + 1].locatedEnemy)
                        {
                            if (!attackNodeList.Contains(w + 1))
                                attackNodeList.Add(w + 1);
                        }
                        else
                            AddMoveNodeInList(w, w + 1);
                        //当前块是否有hero
                        if (mapNodeList[w + 1].locatedHero && !attackNodeList.Contains(w + 1))
                            attackHeroList.Add(w + 1);
                    }
                    else
                    {
                        endNodeList.Add(w);
                    }
                }
                if (IsInMap(w + mapXNode) && !mapNodeList[w + mapXNode].bVisited)
                {
                    if (mapNodeList[w].moveValue + mapNodeList[w + mapXNode].nodeValue <= moveRange)
                    {
                        if (mapNodeList[w + mapXNode].locatedEnemy)
                        {
                            if (!attackNodeList.Contains(w + mapXNode))
                                attackNodeList.Add(w + mapXNode);
                        }
                        else
                            AddMoveNodeInList(w, w + mapXNode);
                        if (mapNodeList[w + mapXNode].locatedHero && !attackNodeList.Contains(w + mapXNode))
                            attackHeroList.Add(w + mapXNode);
                    }
                    else
                    {
                        endNodeList.Add(w);
                    }
                }
                if (IsInMap(w - 1) && !mapNodeList[w - 1].bVisited && (w - 1) / mapXNode == nowRow)
                {
                    if (mapNodeList[w].moveValue + mapNodeList[w - 1].nodeValue <= moveRange)
                    {
                        if (mapNodeList[w - 1].locatedEnemy)
                        {
                            if (!attackNodeList.Contains(w - 1))
                                attackNodeList.Add(w - 1);
                        }
                        else
                            AddMoveNodeInList(w, w - 1);
                        if (mapNodeList[w - 1].locatedHero && !attackNodeList.Contains(w - 1))
                            attackHeroList.Add(w - 1);
                    }
                    else
                    {
                        endNodeList.Add(w);
                    }
                }
                if (IsInMap(w - mapXNode) && !mapNodeList[w - mapXNode].bVisited)
                {
                    if (mapNodeList[w].moveValue + mapNodeList[w - mapXNode].nodeValue <= moveRange)
                    {
                        if (mapNodeList[w - mapXNode].locatedEnemy)
                        {
                            if (!attackNodeList.Contains(w - mapXNode))
                                attackNodeList.Add(w - mapXNode);
                        }
                        else
                            AddMoveNodeInList(w, w - mapXNode);
                        if (mapNodeList[w - mapXNode].locatedHero && !attackNodeList.Contains(w - mapXNode))
                            attackHeroList.Add(w - mapXNode);
                    }
                    else
                    {
                        endNodeList.Add(w);
                    }
                }
                rangeNodeList.Add(w);
                mapNodeList[w].canMove = true;
            }
            range = 0;
        }
    }

    /// <summary>
    /// 添加idx显示moveRange
    /// </summary>
    public void AddMoveNodeInList(int w, int idx)
    {
        mapNodeList[idx].moveValue = mapNodeList[idx].nodeValue + mapNodeList[w].moveValue;
        rangeQueue.Enqueue(idx);
        mapNodeList[idx].parentMapNode = mapNodeList[w];
        mapNodeList[idx].bVisited = true;
    }

    /// <summary>
    /// 添加idx显示attackRange
    /// </summary>
    public void AddAttackNodeInList(int attackRange, int idx)
    {
        if (endNodeList.Count == 0)
        {
            int nowRow = idx / mapXNode;
            if (IsInMap(idx + 1) && !mapNodeList[idx + 1].bVisited && (idx + 1) / mapXNode == nowRow)
            {
                if (!attackNodeList.Contains(idx + 1))
                    attackNodeList.Add(idx + 1);
            }
            if (IsInMap(idx + mapXNode) && !mapNodeList[idx + mapXNode].bVisited)
            {
                if (!attackNodeList.Contains(idx + mapXNode))
                    attackNodeList.Add(idx + mapXNode);
            }
            if (IsInMap(idx - 1) && !mapNodeList[idx - 1].bVisited && (idx - 1) / mapXNode == nowRow)
            {
                if (!attackNodeList.Contains(idx - 1))
                    attackNodeList.Add(idx - 1);
            }
            if (IsInMap(idx + mapXNode) && !mapNodeList[idx - mapXNode].bVisited)
            {
                if (!attackNodeList.Contains(idx - mapXNode))
                    attackNodeList.Add(idx - mapXNode);
            }
        }
        else
        {
            for (int i = 0; i < endNodeList.Count; i++)
            {
                int id = endNodeList[i];
                int nowRow = id / mapXNode;
                if (IsInMap(id + 1) && !mapNodeList[id + 1].bVisited && (id + 1) / mapXNode == nowRow)
                {
                    if (!attackNodeList.Contains(id + 1))
                    {
                        attackNodeList.Add(id + 1);
                        mapNodeList[id + 1].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id + 1].locatedHero && !attackHeroList.Contains(id + 1))
                            attackHeroList.Add(id + 1);
                    }
                }
                if (IsInMap(id + mapXNode) && !mapNodeList[id + mapXNode].bVisited)
                {
                    if (!attackNodeList.Contains(id + mapXNode))
                    {
                        attackNodeList.Add(id + mapXNode);
                        mapNodeList[id + mapXNode].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id + mapXNode].locatedHero && !attackHeroList.Contains(id + mapXNode))
                            attackHeroList.Add(id + mapXNode);
                    }
                }
                if (IsInMap(id - 1) && !mapNodeList[id - 1].bVisited && (id - 1) / mapXNode == nowRow)
                {
                    if (!attackNodeList.Contains(id - 1))
                    {
                        attackNodeList.Add(id - 1);
                        mapNodeList[id - 1].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id - 1].locatedHero && !attackHeroList.Contains(id - 1))
                            attackHeroList.Add(id - 1);
                    }
                }
                if (IsInMap(id - mapXNode) && !mapNodeList[id - mapXNode].bVisited)
                {
                    if (!attackNodeList.Contains(id - mapXNode))
                    {
                        attackNodeList.Add(id - mapXNode);
                        mapNodeList[id - mapXNode].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id - mapXNode].locatedHero && !attackHeroList.Contains(id - mapXNode))
                            attackHeroList.Add(id - mapXNode);
                    }
                }
            }
        }
    }
    #endregion

    #region 公有方法:判断是否在地图内;转换idx跟pos;UI显示;判断两个idx的方位
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
        return mapNodeList[idx];
    }

    /// <summary>
    /// 显示所有game相关UI
    /// </summary>
    public void ShowAllUI()
    {
        if (Idx2ListPos(cursorIdx).x >= mapXNode / 2)
        {
            UIManager.Instance().ShowUIForms("LandData_1");
            if (Idx2ListPos(cursorIdx).y >= mapYNode / 2)
                UIManager.Instance().ShowUIForms("GameGoal_1");
            else
                UIManager.Instance().ShowUIForms("GameGoal_2");
        }
        else
        {
            UIManager.Instance().ShowUIForms("LandData_2");
            UIManager.Instance().ShowUIForms("GameGoal_1");
        }
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
