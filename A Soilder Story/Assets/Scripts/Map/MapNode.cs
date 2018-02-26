using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;

public class MapNode : MonoBehaviour {

    public const string NULL = "--";
    //data
    public string TileType = "<no type>";  //类型
    public string mName;                   //名字
    public int nodeValue;                  //移动值
    public int mdef;                       //防御值
    public int mAvo;                       //回避率
    public int func;                       //特殊功能
    //特殊地形
    public int mLife;
    public string fightName;

    public int maxHp;

    [SerializeField]
    private int ID;

    public int moveValue = 0;  //移动叠加值，用于计算移动
    public MapNode parentMapNode;  //父节点
    public bool canMove = false;
    public bool bVisited = false;  //是否访问
    public HeroController locatedHero;  //hero在这个块上
    public EnemyController locatedEnemy;  //enemy在这个块上
    

    public void Init(int id)
    {
        ID = id;
        parentMapNode = null;

        LandManager instance = LandManager.Instance();
        if (instance.keyLandDic.ContainsKey(TileType))
        {
            mName = instance.keyLandDic[TileType].name;
            nodeValue = DataManager.Value(instance.keyLandDic[TileType].value);
            mdef = DataManager.Value(instance.keyLandDic[TileType].def);
            mAvo = DataManager.Value(instance.keyLandDic[TileType].avo);
            func = DataManager.Value(instance.keyLandDic[TileType].func);
            //有crack的地图先隐藏第二层
            if (func == LandManager.CRACK)
            {
                LevelManager.Instance().curMap.GetComponent<TiledMap>().SetSecondLand(false);
                fightName = instance.keyLandDic[TileType].name2;
                mLife = DataManager.Value(instance.keyLandDic[TileType].life);
                maxHp = mLife;
            }
        }
        else
        {
            mName = NULL;
            nodeValue = 100;
            mdef = 100;
            mAvo = 100;
        }
    }

    public void ReSet()
    {
        bVisited = false;
        parentMapNode = null;
        canMove = false;
        moveValue = 0;
    }

    public int GetID()
    {
        return ID;
    }
}
