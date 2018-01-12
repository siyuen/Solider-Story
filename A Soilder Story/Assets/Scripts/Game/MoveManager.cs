using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

/// <summary>
/// 处理移动&攻击范围相关
/// </summary>
public class MoveManager : QSingleton<MoveManager> {
    //地图的所有图块
    private List<MapNode> mapNodeList;
    //用于计算移动范围
    private Queue<int> rangeQueue = new Queue<int>();  
    //移动范围的块的parent
    private GameObject moveRangeObj;  
    //移动范围的块的idx
    private List<int> rangeNodeList = new List<int>();
    //移动范围的块obj
    private List<GameObject> nodeObjList = new List<GameObject>();
    //记录周围有移动不了的块的idx
    private List<int> endNodeList = new List<int>();
    //攻击范围的块的idx
    private List<int> attackNodeList = new List<int>();
    //攻击范围的块obj
    private List<GameObject> attackObjList = new List<GameObject>();
    //在攻击范围内的hero
    private List<int> attackHeroList = new List<int>();  
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

    private MoveManager()
    {
        moveRangeObj = new GameObject();
        moveRangeObj.name = "MoveRange";
        roadContent = new GameObject();
        roadContent.name = "Road";
    }

    public void SetMap(List<MapNode> mapNodeList)
    {
        this.mapNodeList = mapNodeList;
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
    /// 获取图块
    /// </summary>
    public MapNode GetMapNode(int idx)
    {
        return mapNodeList[idx];
    }

    #region 移动范围
    /// <summary>
    /// 显示移动范围
    /// </summary>
    public void ShowMoveRange(Vector2 pos, int moveRange, int attackRange)
    {
        ReSet();
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
        //敌人的块
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
        ShowAttackRange();
    }

    /// <summary>
    /// 隐藏移动范围
    /// </summary>
    public void HideMoveRange()
    {
        if (nodeObjList.Count > 0)
        {
            GameObjectPool.Instance().PushPool(nodeObjList, MainProperty.RANGENODE_PATH);
            nodeObjList.Clear();
            GameObjectPool.Instance().PushPool(attackObjList, MainProperty.ATTACKNODE_PATH);
            attackObjList.Clear();
            ReSet();
        }
    }

    /// <summary>
    /// 计算移动范围
    /// </summary>
    public void DoMoveRange(Vector2 pos, int moveRange, int attackRange)
    {
        int id = MainManager.Instance().Pos2Idx(pos);
        mapNodeList[id].bVisited = true;
        rangeQueue.Enqueue(id);
        int range = 0;
        //能移动到的index
        while (rangeQueue.Count != 0)
        {
            int w = rangeQueue.Dequeue();
            int mapXNode =  MainManager.Instance().GetXNode();
            int nowRow = w / mapXNode;
            //判断走了几步
            MapNode parent = mapNodeList[w];
            if (parent.parentMapNode != null)
            {
                range++;
                parent = parent.parentMapNode;
            }

            //未超出移动范围;按照右下左上顺序来算
            if (range <= moveRange)
            {
                if (MainManager.Instance().IsInMap(w + 1) && !mapNodeList[w + 1].bVisited && (w + 1) / mapXNode == nowRow)  //如果右边节点存在且未被访问
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
                if (MainManager.Instance().IsInMap(w + mapXNode) && !mapNodeList[w + mapXNode].bVisited)
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
                if (MainManager.Instance().IsInMap(w - 1) && !mapNodeList[w - 1].bVisited && (w - 1) / mapXNode == nowRow)
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
                if (MainManager.Instance().IsInMap(w - mapXNode) && !mapNodeList[w - mapXNode].bVisited)
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
        if (mapNodeList[idx].parentMapNode != null)
            return;
        mapNodeList[idx].moveValue = mapNodeList[idx].nodeValue + mapNodeList[w].moveValue;
        rangeQueue.Enqueue(idx);
        mapNodeList[idx].parentMapNode = mapNodeList[w];
        mapNodeList[idx].bVisited = true;
    }
    #endregion

    #region 攻击范围
    /// <summary>
    /// 显示攻击范围
    /// </summary>
    public void ShowAttackRange()
    {
        if (MainManager.Instance().curEnemy)
            AddAttackNodeInList(1, MainManager.Instance().curEnemy.mID);
        if (MainManager.Instance().curHero)
            AddAttackNodeInList(1, MainManager.Instance().curHero.mID);
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
            attackNodeList.Clear();
        }
    }

    /// <summary>
    /// 添加idx显示attackRange
    /// </summary>
    public void AddAttackNodeInList(int attackRange, int idx)
    {
        attackNodeList.Clear();
        if (endNodeList.Count == 0)
        {
            int mapXNode = MainManager.Instance().GetXNode();
            int nowRow = idx / mapXNode;
            if (MainManager.Instance().IsInMap(idx + 1) && !mapNodeList[idx + 1].bVisited && (idx + 1) / mapXNode == nowRow)
            {
                if (!attackNodeList.Contains(idx + 1))
                    attackNodeList.Add(idx + 1);
            }
            if (MainManager.Instance().IsInMap(idx + mapXNode) && !mapNodeList[idx + mapXNode].bVisited)
            {
                if (!attackNodeList.Contains(idx + mapXNode))
                    attackNodeList.Add(idx + mapXNode);
            }
            if (MainManager.Instance().IsInMap(idx - 1) && !mapNodeList[idx - 1].bVisited && (idx - 1) / mapXNode == nowRow)
            {
                if (!attackNodeList.Contains(idx - 1))
                    attackNodeList.Add(idx - 1);
            }
            if (MainManager.Instance().IsInMap(idx + mapXNode) && !mapNodeList[idx - mapXNode].bVisited)
            {
                if (!attackNodeList.Contains(idx - mapXNode))
                    attackNodeList.Add(idx - mapXNode);
            }
        }
        else
        {
            for (int i = 0; i < endNodeList.Count; i++)
            {
                int mapXNode = MainManager.Instance().GetXNode();
                int id = endNodeList[i];
                int nowRow = id / mapXNode;
                if (MainManager.Instance().IsInMap(id + 1) && !mapNodeList[id + 1].bVisited && (id + 1) / mapXNode == nowRow)
                {
                    if (!attackNodeList.Contains(id + 1))
                    {
                        attackNodeList.Add(id + 1);
                        mapNodeList[id + 1].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id + 1].locatedHero && !attackHeroList.Contains(id + 1))
                            attackHeroList.Add(id + 1);
                    }
                }
                if (MainManager.Instance().IsInMap(id + mapXNode) && !mapNodeList[id + mapXNode].bVisited)
                {
                    if (!attackNodeList.Contains(id + mapXNode))
                    {
                        attackNodeList.Add(id + mapXNode);
                        mapNodeList[id + mapXNode].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id + mapXNode].locatedHero && !attackHeroList.Contains(id + mapXNode))
                            attackHeroList.Add(id + mapXNode);
                    }
                }
                if (MainManager.Instance().IsInMap(id - 1) && !mapNodeList[id - 1].bVisited && (id - 1) / mapXNode == nowRow)
                {
                    if (!attackNodeList.Contains(id - 1))
                    {
                        attackNodeList.Add(id - 1);
                        mapNodeList[id - 1].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id - 1].locatedHero && !attackHeroList.Contains(id - 1))
                            attackHeroList.Add(id - 1);
                    }
                }
                if (MainManager.Instance().IsInMap(id - mapXNode) && !mapNodeList[id - mapXNode].bVisited)
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

    /// <summary>
    /// 获取攻击块的idx
    /// </summary>
    public List<int> GetAttackHeroList()
    {
        return attackHeroList;
    }
    #endregion

    #region 路径显示
    /// <summary>
    /// 显示移动路径
    /// </summary>
    public void ShowRoad(int id)
    {
        HideRoad();
        MainManager mainInstance = MainManager.Instance();
        //原点不需要显示
        if (id == mainInstance.curHero.mID)
            return;
        //获取当前路线idx
        MapNode node = GetMapNode(id);
        while (node.parentMapNode)
        {
            roadIdxList.Add(node.GetID());
            node = node.parentMapNode;
        }
        roadIdxList.Add(mainInstance.curHero.mID);
        //实例化路线
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
                dir = mainInstance.GetIdx2IdxPos(roadIdxList[j + 1], roadIdxList[j]);
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
                GameObject head = GameObjectPool.Instance().GetPool(headpath, mainInstance.Idx2Pos(roadIdxList[j]));
                head.transform.SetParent(roadContent.transform);
                roadObjList.Add(new RoadObj(headpath, head));
                GameObject tail = GameObjectPool.Instance().GetPool(tailpath, mainInstance.Idx2Pos(roadIdxList[j + 1]));
                tail.transform.SetParent(roadContent.transform);
                roadObjList.Add(new RoadObj(tailpath, tail));
            }
            else
            {
                string headpath = MainProperty.ROADHEAD_UP_PATH;
                string tailpath = MainProperty.ROADBODY_UP_PATH;
                dir = MainManager.Instance().GetIdx2IdxPos(roadIdxList[j + 1], roadIdxList[j]);
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
                            headpath = MainProperty.ROADCORNER_RIGHT1_PATH;
                        else
                            headpath = MainProperty.ROADCORNER_RIGHT2_PATH;
                    }
                    lastdir = dir;
                    RoadObj road = roadObjList[roadObjList.Count - 1];
                    GameObjectPool.Instance().PushPool(road.road, road.path);
                    GameObject head = GameObjectPool.Instance().GetPool(headpath, mainInstance.Idx2Pos(roadIdxList[j]));
                    head.transform.SetParent(roadContent.transform);
                    roadObjList[roadObjList.Count - 1] = new RoadObj(headpath, head);

                    GameObject tail = GameObjectPool.Instance().GetPool(tailpath, mainInstance.Idx2Pos(roadIdxList[j + 1]));
                    tail.transform.SetParent(roadContent.transform);
                    roadObjList.Add(new RoadObj(tailpath, tail));
                }
                else
                {
                    if (dir == 1 || dir == 2)
                        tailpath = MainProperty.ROADBODY_UP_PATH;
                    else if (dir == 3 || dir == 4)
                        tailpath = MainProperty.ROADBODY_LEFT_PATH;
                    GameObject tail = GameObjectPool.Instance().GetPool(tailpath, mainInstance.Idx2Pos(roadIdxList[j + 1]));
                    tail.transform.SetParent(roadContent.transform);
                    roadObjList.Add(new RoadObj(tailpath, tail));
                }
            }
        }
    }

    /// <summary>
    /// 隐藏路径
    /// </summary>
    public void HideRoad()
    {
        roadIdxList.Clear();
        if (roadObjList.Count > 0)
        {
            for (int j = 0; j < roadObjList.Count; j++)
                GameObjectPool.Instance().PushPool(roadObjList[j].road, roadObjList[j].path);
        }
        roadObjList.Clear();
    }
    #endregion
}
