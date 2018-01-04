using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapNode : MonoBehaviour {
    public string TileType = "<no type>";  //类型

    [SerializeField]
    private int ID;

    public int nodeValue;  //node对应的值
    public int moveValue = 0;  //移动值，不同地形不同
    public MapNode parentMapNode;  //父节点
    public bool canMove = false;
    public bool bVisited = false;  //是否访问
    public HeroController locatedHero;  //hero在这个块上
    public EnemyController locatedEnemy;  //enemy在这个块上

    public void Init(int id, int value, int moveValue)
    {
        ID = id;
        nodeValue = value;
        //this.moveValue = moveValue;
        parentMapNode = null;
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
