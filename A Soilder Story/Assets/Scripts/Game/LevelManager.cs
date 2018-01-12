using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using Tiled2Unity;

public class LevelManager : QMonoSingleton<LevelManager> {
    public const string MAP_PATH = "Prefabs/Game/Map/Level_";
    public const int LEVEL_LIMIT = 10;
    
    struct NodeData
    {
        public NodeData(int value, int moveValue)
        {
            Value = value;
            MoveValue = moveValue;
        }
        public int Value;
        public int MoveValue;
    }
    private int curLevel;     //当前Level
    private GameObject curMap;  //当前LevelMap
    private Dictionary<string, NodeData> mapNodeValueDic = new Dictionary<string, NodeData>(); //地图相应图块的数据
    
    void Awake()
    {
        curLevel = 1;
        InitNodeData();
        MainManager.Instance();
        UIManager.Instance();
    }

    private void InitNodeData()
    {
        mapNodeValueDic.Add("Grass", new NodeData(1, 1));
        mapNodeValueDic.Add("Tree", new NodeData(1, 1));
        mapNodeValueDic.Add("Mountain", new NodeData(3, 1));
        mapNodeValueDic.Add("Wall", new NodeData(4, 1));
        mapNodeValueDic.Add("Water", new NodeData(1, 1));
        mapNodeValueDic.Add("Bridge", new NodeData(6, 1));
        mapNodeValueDic.Add("Untag", new NodeData(7, 1));
    }

    public int GetCurLevel()
    {
        return curLevel;
    }

    /// <summary>
    /// 设置关卡，初始化图块数据
    /// </summary>
    /// <param name="level"></param>
    public void SetLevel(int level)
    {
        curLevel = level;
        string path = MAP_PATH + level;
        curMap = ResourcesMgr.Instance().LoadAsset(path, true);
        TiledMap map = curMap.GetComponent<TiledMap>();
        InitMapNode(map);
    }

    /// <summary>
    /// 初始化图块数据，通过id排序存入list
    /// </summary>
    private void InitMapNode(TiledMap map)
    {
        MapNode[] tiles = curMap.GetComponentsInChildren<MapNode>();
        MapNode[] Tiles = new MapNode[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2 pos = tiles[i].transform.position;
            int id = (int)(pos.x / map.TileWidth + Mathf.Abs(pos.y / map.TileHeight) * map.NumTilesWide);
            NodeData data;
            mapNodeValueDic.TryGetValue(tiles[i].TileType, out data);
            tiles[i].Init(id, data.Value, data.MoveValue);
            Tiles[id] = tiles[i];
        }
        MainManager.Instance().SetMapData(map);
        MoveManager.Instance().SetMap(new List<MapNode>(Tiles));
    }

    public int GetNodeID(Vector2 pos)
    {
        return 0;
    }


}
