using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class EnemyManager : QSingleton<EnemyManager> {

    public const string ENEMY_PATH = "Prefabs/Game/enemy";
    private List<EnemyController> enemyList = new List<EnemyController>();
    private GameObject enemyContent;
    //记录当前enemy
    private int enemyIdx;
    private int standbyCount;

    private EnemyManager()
    {
    }

    public void Init(int level)
    {
        standbyCount = 0;
        enemyContent = new GameObject();
        enemyContent.name = "Enemy";
        enemyIdx = 0;
        GameObject enemy = GameObjectPool.Instance().GetPool(ENEMY_PATH, MainManager.Instance().Idx2Pos2(49));
        enemy.transform.SetParent(enemyContent.transform);
        enemy.GetComponent<EnemyController>().Init();
        enemyList.Add(enemy.GetComponent<EnemyController>());
    }

    public void SetEnemyRounnd()
    {
        enemyList[enemyIdx].Init();
        enemyList[enemyIdx].SearchHero();          
    }

    /// <summary>
    /// 初始化enemy
    /// </summary>
    public void SetEnemyInit()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].Init();
        }
    }

    /// <summary>
    /// 记录待机数
    /// </summary>
    public bool SetStandby()
    {
        standbyCount++;
        if (standbyCount == enemyList.Count)
        {
            standbyCount = 0;
            return true;
        }
        else
            return false;
    }
}
