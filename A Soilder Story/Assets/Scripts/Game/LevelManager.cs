using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using Tiled2Unity;

public class LevelManager : QSingleton<LevelManager> {

    public const string TEMPORARY = "Data/TemproaryLevelData";
    public const string MAP_PATH = "Prefabs/Game/Map/Level_";
    public const int LEVEL_LIMIT = 10;
    public const int NULLNODE = 100;
    public const string ALLENEMY = "allenemy";
    public const string POS = "pos";
    //关卡信息
    public Dictionary<string, LevelData> levelDic;
    //当前LevelMap
    public GameObject curMap;  
    public int mapXNode;  
    public int mapYNode;  
    public int nodeWidth;  
    public int nodeHeight;
    //特殊地形
    public List<int> crackList = new List<int>();

    //当前Level
    private int curLevel;
    private TiledMap curTiledMap;
    private List<MapNode> mapNodeList;
    

    private LevelManager()
    {
        levelDic = DataManager.Load<LevelData>("Data/LevelData");
    }

    public void Init(int level)
    {
        if (level > levelDic.Count)
            return;
        curLevel = level;
        UIManager.Instance().ShowUIForms("LevelStart");
    }

    public int GetCurLevel()
    {
        return curLevel;
    }

    #region 地图相关
    /// <summary>
    /// 显示地图隐藏的部分
    /// </summary>
    public void ShowMap()
    {
        curMap.GetComponent<TiledMap>().SetSecondLand(true);
    }

    /// <summary>
    /// 设置关卡，初始化图块数据,enemy
    /// </summary>
    public void SetLevel()
    {
        string path = MAP_PATH + curLevel;
        curMap = ResourcesMgr.Instance().GetPool(path);
        InitMapNode();
    }

    public void Clear()
    {
        string path = MAP_PATH + curLevel;
        ResourcesMgr.Instance().PushPool(curMap, path);
    }

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
        if (curTiledMap.GetMapRect().Contains(pos))
            return true;
        else
            return false;
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
        Vector2 PIVOT = new Vector2(8, -8);
        Vector3 pos = new Vector3(x * nodeWidth + PIVOT.x, -(y * nodeHeight - PIVOT.y), 0);
        return pos;
    }

    /// <summary>
    /// 根据idx计算pos,锚点为左上角
    /// </summary>
    public Vector3 Idx2Pos2(int idx)
    {
        int x = idx % mapXNode;
        int y = idx / mapXNode;
        Vector3 pos = new Vector3(x * nodeWidth, -(y * nodeHeight), 0);
        return pos;
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
    /// 两个idx的距离
    /// </summary>
    public int Idx2IdxDis(int i1, int i2)
    {
        int dis = 0;
        Vector2 pos1 = Idx2ListPos(i1);
        Vector2 pos2 = Idx2ListPos(i2);
        dis = (int)(Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y));

        return dis;
    }
    #endregion

    /// <summary>
    /// 初始化图块数据，通过id排序存入list
    /// </summary>
    private void InitMapNode()
    {
        curTiledMap = curMap.GetComponent<TiledMap>();
        MapNode[] tiles = curMap.GetComponentsInChildren<MapNode>();
        MapNode[] Tiles = new MapNode[tiles.Length];
        //记录crack
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2 pos = tiles[i].transform.position;
            int id = (int)(pos.x / curTiledMap.TileWidth + Mathf.Abs(pos.y / curTiledMap.TileHeight) * curTiledMap.NumTilesWide);
            tiles[i].Init(id);
            Tiles[id] = tiles[i];
        }
        if (crackList.Count > 0)
            curTiledMap.SetSecondLand(false);
        mapNodeList = new List<MapNode>(Tiles);
        mapXNode = curTiledMap.NumTilesWide;
        mapYNode = curTiledMap.NumTilesHigh;
        nodeWidth = curTiledMap.TileWidth;
        nodeHeight = curTiledMap.TileHeight;
        MoveManager.Instance().SetMap(mapNodeList);
        TemporaryUpdate();
    }
    /// <summary>
    /// 中断隐藏
    /// </summary>
    public void TemporarySave()
    {
        string path = MAP_PATH + curLevel;
        TemporaryUpdate();
        ResourcesMgr.Instance().PushPool(curMap, path);
        MainManager.Instance().TemporarySave();
    }

    public void TemporaryUpdate()
    {
        //记录特殊地形
        if (crackList.Count > 0)
        {
            Dictionary<string, TemporaryLevelData> save = new Dictionary<string, TemporaryLevelData>();
            for (int i = 0; i < crackList.Count; i++)
            {
                TemporaryLevelData data = new TemporaryLevelData();
                data.id = crackList[i].ToString();
                data.key = GetMapNode(crackList[i]).TileType;
                data.life = GetMapNode(crackList[i]).mLife.ToString();
                save.Add(crackList[i].ToString(), data);
            }
            DataManager.Instance().SaveDicData<TemporaryLevelData>(save, TEMPORARY);
            crackList.Clear();
        }
    }

    public void ContinueGame(int level)
    {
        curLevel = level;
        string path = MAP_PATH + curLevel;
        curMap = ResourcesMgr.Instance().GetPool(path);
        curTiledMap = curMap.GetComponent<TiledMap>();
        crackList = new List<int>();
        //tiles是乱序的,计算顺序存入Tiles
        MapNode[] tiles = curMap.GetComponentsInChildren<MapNode>();
        MapNode[] Tiles = new MapNode[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2 pos = tiles[i].transform.position;
            int id = (int)(pos.x / curTiledMap.TileWidth + Mathf.Abs(pos.y / curTiledMap.TileHeight) * curTiledMap.NumTilesWide);
            tiles[i].Init(id);
            Tiles[id] = tiles[i];
        }
        mapNodeList = new List<MapNode>(Tiles);
        mapXNode = curTiledMap.NumTilesWide;
        mapYNode = curTiledMap.NumTilesHigh;
        nodeWidth = curTiledMap.TileWidth;
        nodeHeight = curTiledMap.TileHeight;
        //加载node跟特殊数据
        Dictionary<string, TemporaryLevelData> load = DataManager.Load<TemporaryLevelData>(TEMPORARY);
        //是否隐藏crack
        bool show = false;
        if (load.Count > 0)
        {
            foreach (var i in load)
            {
                int id = DataManager.Value(i.Key);
                if (GetMapNode(id).TileType == i.Value.key)
                {
                    GetMapNode(id).mLife = DataManager.Value(i.Value.life);
                    if (GetMapNode(id).mLife == 0)
                        show = true;
                }
            }
        }
        if (crackList.Count > 0)
            curTiledMap.SetSecondLand(show);
        MoveManager.Instance().SetMap(mapNodeList);
        MainManager.Instance().ContinueGame();
    }

    /// <summary>
    /// 获取关卡目的tips
    /// </summary>
    public string GetLevelTips()
    {
        return levelDic[curLevel.ToString()].goaltips;
    }

    /// <summary>
    /// 获取关卡目的t
    /// </summary>
    public string GetLevelGoal()
    {
        return levelDic[curLevel.ToString()].goal;
    }

    /// <summary>
    /// 判断是否完成任务
    /// </summary>
    public bool IsFinish()
    {
        if (levelDic[curLevel.ToString()].goal == ALLENEMY)
        {
            if (EnemyManager.Instance().GetEnemyCount() == 0)
                return true;
        }
        else if (levelDic[curLevel.ToString()].goal == POS)
        {
            HeroController hero = MainManager.Instance().curHero;
            if (GetMapNode(hero.mIdx).mName == "大门" && hero.rolePro.mName == HeroManager.LEADER)
                return true;
        }
        return false;
    }
}
