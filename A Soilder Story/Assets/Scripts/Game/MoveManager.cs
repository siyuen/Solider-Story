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
    private List<int> attackNode = new List<int>();
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
    private LevelManager levelInstance;

    private MoveManager()
    {
        levelInstance = LevelManager.Instance();
        moveRangeObj = new GameObject();
        moveRangeObj.name = "MoveRange";
        roadContent = new GameObject();
        roadContent.name = "Road";
    }

    public void SetMap(List<MapNode> mapNodeList)
    {
        this.mapNodeList = mapNodeList;
    }

    #region 公有方法：获取node;重置;获取方位;
    /// <summary>
    /// 重置
    /// </summary>
    public void ReSet()
    {
        for (int i = 0; i < mapNodeList.Count; i++)
        {
            mapNodeList[i].ReSet();
        }
        endNodeList.Clear();
        rangeNodeList.Clear();
        attackNode.Clear();
        attackHeroList.Clear();
        roadIdxList.Clear();
    }

    /// <summary>
    /// 获取周围的四个图块idx
    /// </summary>
    public List<int> GetRoundMapNode(int idx)
    {
        List<int> node = new List<int>();
        int mapXNode = levelInstance.mapXNode;
        int nowRow = idx / mapXNode;
        if (levelInstance.IsInMap(idx + 1) && (idx + 1) / mapXNode == nowRow)
            node.Add(idx + 1);
        if (levelInstance.IsInMap(idx + mapXNode))
            node.Add(idx + mapXNode);
        if (levelInstance.IsInMap(idx - 1) && (idx - 1) / mapXNode == nowRow)
            node.Add(idx - 1);
        if (levelInstance.IsInMap(idx - mapXNode))
            node.Add(idx - mapXNode);   
        return node;
    }

    /// <summary>
    /// 获取两个idx之间的方位，只分上下左右;以idx1为原点
    /// </summary>
    public string GetDir(int idx1, int idx2)
    {
        if (idx1 == idx2 || !levelInstance.IsInMap(idx1) || !levelInstance.IsInMap(idx2))
            return null;
        int mapXNode = levelInstance.mapXNode;
        int row1 = idx1 / mapXNode;
        int row2 = idx2 / mapXNode;
        int col1 = idx1 % mapXNode;
        int col2 = idx2 % mapXNode;
        if (row1 != row2 && col1 != col2)
            return null;
        if (row1 == row2)
        {
            if (idx1 > idx2)
                return "Left";
            else
                return "Right";
        }
        else
        {
            if (idx1 > idx2)
                return "Up";
            else
                return "Down";
        }
    }

    #endregion

    #region 移动范围
    /// <summary>
    /// 显示移动范围
    /// </summary>
    public void ShowMoveRange(Vector2 pos, int moveRange, int attackRange)
    {
        ReSet();
        if(MainManager.Instance().curHero)
            DoMoveRange(pos, moveRange, attackRange);
        else
            DoMoveRange(pos, moveRange, attackRange, true);
        if (rangeNodeList.Count > 0)
        {
            for (int i = 0; i < rangeNodeList.Count; i++)
            {
                Vector3 nodePos = mapNodeList[rangeNodeList[i]].transform.position;
                GameObject node = ResourcesMgr.Instance().GetPool(MainProperty.RANGENODE_PATH);
                node.transform.position = nodePos;
                node.transform.SetParent(moveRangeObj.transform);
                nodeObjList.Add(node);
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
            ResourcesMgr.Instance().PushPool(nodeObjList, MainProperty.RANGENODE_PATH);
            nodeObjList.Clear();
            ResourcesMgr.Instance().PushPool(attackObjList, MainProperty.ATTACKNODE_PATH);
            attackObjList.Clear();
            ReSet();
        }
    }

    /// <summary>
    /// 计算移动范围,默认为hero
    /// </summary>
    public void DoMoveRange(Vector2 pos, int moveRange, int attackRange, bool enemy = false)
    {
        int id = levelInstance.Pos2Idx(pos);
        mapNodeList[id].bVisited = true;
        rangeQueue.Enqueue(id);
        //能移动到的index
        while (rangeQueue.Count != 0)
        {
            int w = rangeQueue.Dequeue();
            int mapXNode = levelInstance.mapXNode;
            int nowRow = w / mapXNode;

            //简单判断是否将上下左右四个节点放入list
            List<int> roundnode = new List<int>();
            if (levelInstance.IsInMap(w + 1) && !mapNodeList[w + 1].bVisited && (w + 1) / mapXNode == nowRow)
                roundnode.Add(w + 1);
            if (levelInstance.IsInMap(w - 1) && !mapNodeList[w - 1].bVisited && (w - 1) / mapXNode == nowRow)
                roundnode.Add(w - 1);
            if (levelInstance.IsInMap(w + mapXNode) && !mapNodeList[w + mapXNode].bVisited)
                roundnode.Add(w + mapXNode);
            if (levelInstance.IsInMap(w - mapXNode) && !mapNodeList[w - mapXNode].bVisited)
                roundnode.Add(w - mapXNode);

            for (int i = 0; i < roundnode.Count; i++)
            {
                int idx = roundnode[i];
                if (mapNodeList[w].moveValue + mapNodeList[idx].nodeValue <= moveRange)  //未超出移动能力
                {
                    //判断crack
                    if (mapNodeList[idx].TileType == "Crack")
                    {
                        if (mapNodeList[idx].mLife != 0)
                            endNodeList.Add(w);
                        else
                            AddNodeInList(w, idx, enemy);
                    }
                    else
                        AddNodeInList(w, idx, enemy);
                }
                else
                    endNodeList.Add(w);
            }
            roundnode = null;
            rangeNodeList.Add(w);
            mapNodeList[w].canMove = true;
        }
    }

    /// <summary>
    /// 添加idx显示移动范围
    /// </summary>
    private void AddNodeInList(int w, int idx, bool enemy)
    {
        if (!enemy)
        {
            if (mapNodeList[idx].locatedEnemy)
            {
                if (!attackNode.Contains(idx))
                    attackNode.Add(idx);
            }
            else
                AddMoveNodeInList(w, idx);
        }
        else
        {
            if (mapNodeList[idx].locatedHero)
            {
                if (!attackNode.Contains(idx))
                    attackNode.Add(idx);
                if (!attackHeroList.Contains(idx))
                    attackHeroList.Add(idx);
            }
            else
                AddMoveNodeInList(w, idx);
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
            AddAttackNodeInList(1, MainManager.Instance().curEnemy.mIdx);
        if (MainManager.Instance().curHero)
            AddAttackNodeInList(1, MainManager.Instance().curHero.mIdx);
        if (attackNode.Count > 0)
        {
            for (int i = 0; i < attackNode.Count; i++)
            {
                Vector3 nodePos = mapNodeList[attackNode[i]].transform.position;
                GameObject node = ResourcesMgr.Instance().GetPool(MainProperty.ATTACKNODE_PATH);
                node.transform.position = nodePos;
                node.transform.SetParent(moveRangeObj.transform);
                attackObjList.Add(node);
            }
        }
    }

    /// <summary>
    /// 隐藏攻击范围
    /// </summary>
    public void HideAttackRange()
    {
        if (attackNode.Count > 0)
        {
            ResourcesMgr.Instance().PushPool(attackObjList, MainProperty.ATTACKNODE_PATH);
            attackObjList.Clear();
            attackNode.Clear();
        }
    }

    /// <summary>
    /// 添加idx显示attackRange
    /// </summary>
    public void AddAttackNodeInList(int attackRange, int idx)
    {
        if (endNodeList.Count == 0)
        {
            int mapXNode = levelInstance.mapXNode;
            int nowRow = idx / mapXNode;
            if (levelInstance.IsInMap(idx + 1) && !mapNodeList[idx + 1].bVisited && (idx + 1) / mapXNode == nowRow)
            {
                if (!attackNode.Contains(idx + 1))
                    attackNode.Add(idx + 1);
            }
            if (levelInstance.IsInMap(idx + mapXNode) && !mapNodeList[idx + mapXNode].bVisited)
            {
                if (!attackNode.Contains(idx + mapXNode))
                    attackNode.Add(idx + mapXNode);
            }
            if (levelInstance.IsInMap(idx - 1) && !mapNodeList[idx - 1].bVisited && (idx - 1) / mapXNode == nowRow)
            {
                if (!attackNode.Contains(idx - 1))
                    attackNode.Add(idx - 1);
            }
            if (levelInstance.IsInMap(idx - mapXNode) && !mapNodeList[idx - mapXNode].bVisited)
            {
                if (!attackNode.Contains(idx - mapXNode))
                    attackNode.Add(idx - mapXNode);
            }
        }
        else
        {
            for (int i = 0; i < endNodeList.Count; i++)
            {
                int mapXNode = levelInstance.mapXNode;
                int id = endNodeList[i];
                int nowRow = id / mapXNode;
                if (levelInstance.IsInMap(id + 1) && !mapNodeList[id + 1].bVisited && (id + 1) / mapXNode == nowRow)
                {
                    if (!attackNode.Contains(id + 1))
                    {
                        attackNode.Add(id + 1);
                        mapNodeList[id + 1].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id + 1].locatedHero && !attackHeroList.Contains(id + 1))
                            attackHeroList.Add(id + 1);
                    }
                }
                if (levelInstance.IsInMap(id + mapXNode) && !mapNodeList[id + mapXNode].bVisited)
                {
                    if (!attackNode.Contains(id + mapXNode))
                    {
                        attackNode.Add(id + mapXNode);
                        mapNodeList[id + mapXNode].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id + mapXNode].locatedHero && !attackHeroList.Contains(id + mapXNode))
                            attackHeroList.Add(id + mapXNode);
                    }
                }
                if (levelInstance.IsInMap(id - 1) && !mapNodeList[id - 1].bVisited && (id - 1) / mapXNode == nowRow)
                {
                    if (!attackNode.Contains(id - 1))
                    {
                        attackNode.Add(id - 1);
                        mapNodeList[id - 1].parentMapNode = mapNodeList[id];
                        if (mapNodeList[id - 1].locatedHero && !attackHeroList.Contains(id - 1))
                            attackHeroList.Add(id - 1);
                    }
                }
                if (levelInstance.IsInMap(id - mapXNode) && !mapNodeList[id - mapXNode].bVisited)
                {
                    if (!attackNode.Contains(id - mapXNode))
                    {
                        attackNode.Add(id - mapXNode);
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
        if (id == mainInstance.curHero.mIdx)
            return;
        //获取当前路线idx
        MapNode node = levelInstance.GetMapNode(id);
        while (node.parentMapNode)
        {
            roadIdxList.Add(node.GetID());
            node = node.parentMapNode;
        }
        roadIdxList.Add(mainInstance.curHero.mIdx);
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
                GameObject head = ResourcesMgr.Instance().GetPool(headpath);
                head.transform.position = levelInstance.Idx2Pos(roadIdxList[j]); 
                head.transform.SetParent(roadContent.transform);
                roadObjList.Add(new RoadObj(headpath, head));
                GameObject tail = ResourcesMgr.Instance().GetPool(tailpath);
                tail.transform.position = levelInstance.Idx2Pos(roadIdxList[j + 1]);
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
                    ResourcesMgr.Instance().PushPool(road.road, road.path);
                    GameObject head = ResourcesMgr.Instance().GetPool(headpath);
                    head.transform.position = levelInstance.Idx2Pos(roadIdxList[j]);
                    head.transform.SetParent(roadContent.transform);
                    roadObjList[roadObjList.Count - 1] = new RoadObj(headpath, head);

                    GameObject tail = ResourcesMgr.Instance().GetPool(tailpath);
                    tail.transform.position = levelInstance.Idx2Pos(roadIdxList[j + 1]);
                    tail.transform.SetParent(roadContent.transform);
                    roadObjList.Add(new RoadObj(tailpath, tail));
                }
                else
                {
                    if (dir == 1 || dir == 2)
                        tailpath = MainProperty.ROADBODY_UP_PATH;
                    else if (dir == 3 || dir == 4)
                        tailpath = MainProperty.ROADBODY_LEFT_PATH;
                    GameObject tail = ResourcesMgr.Instance().GetPool(tailpath);
                    tail.transform.position = levelInstance.Idx2Pos(roadIdxList[j + 1]);
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
                ResourcesMgr.Instance().PushPool(roadObjList[j].road, roadObjList[j].path);
        }
        roadObjList.Clear();
    }
    #endregion
}
