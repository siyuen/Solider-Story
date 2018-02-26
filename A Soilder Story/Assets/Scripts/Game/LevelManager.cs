using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using Tiled2Unity;

public class LevelManager : QSingleton<LevelManager> {
    public const string MAP_PATH = "Prefabs/Game/Map/Level_";
    public const int LEVEL_LIMIT = 10;
    public const int NULLNODE = 100;
    public const string ALLENEMY = "allenemy";
    public const string POS = "pos";
    //当前LevelMap
    public GameObject curMap;  
    //关卡信息
    public Dictionary<string, LevelData> levelDic;
    //当前Level
    private int curLevel;     
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
    /// 初始化图块数据，通过id排序存入list
    /// </summary>
    private void InitMapNode()
    {
        TiledMap map = curMap.GetComponent<TiledMap>();
        MapNode[] tiles = curMap.GetComponentsInChildren<MapNode>();
        MapNode[] Tiles = new MapNode[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2 pos = tiles[i].transform.position;
            int id = (int)(pos.x / map.TileWidth + Mathf.Abs(pos.y / map.TileHeight) * map.NumTilesWide);
            tiles[i].Init(id);
            Tiles[id] = tiles[i];
        }
        mapNodeList = new List<MapNode>(Tiles);
        MoveManager.Instance().SetMap(mapNodeList);
    }

    #endregion

    /// <summary>
    /// 中断隐藏
    /// </summary>
    public void TemporarySave()
    {
        string path = MAP_PATH + curLevel;
        ResourcesMgr.Instance().PushPool(curMap, path);
        MainManager.Instance().TemporarySave();
    }

    public void ContinueGame(int level)
    {
        curLevel = level;
        SetLevel();
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
            if (MainManager.Instance().GetMapNode(hero.mIdx).mName == "大门" && hero.mName == HeroManager.LEADER)
                return true;
        }
        return false;
    }
}
