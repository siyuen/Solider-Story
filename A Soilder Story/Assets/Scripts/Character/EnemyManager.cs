using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using LitJson;

public class EnemyManager : QSingleton<EnemyManager> {

    //临时数据存储地址
    public const string TEMDATA = "Data/TemporaryEnemyData";
    //原始数据
    public Dictionary<string, EnemyProData> enemyDic;
    //当前关卡enemy
    public Dictionary<int, EnemyProData> keyEnemyDic = new Dictionary<int, EnemyProData>();
    public List<EnemyProData> enemyList = new List<EnemyProData>();

    private List<EnemyController> curEnemyList = new List<EnemyController>();
    public GameObject enemyContent;
    //记录当前enemy
    public int standbyCount;
    //boss
    private string bossName;
    private GameObject bossTag;

    private EnemyManager()
    {
        enemyDic = DataManager.Load<EnemyProData>("Data/EnemyPropertyData");
        for (int i = 0; i < enemyDic.Count; i++)
        {
            enemyList.Add(CloneEnemyData(enemyDic[i.ToString()]));
        }
        enemyContent = new GameObject();
        enemyContent.name = "Enemy";
    }

    public EnemyProData CloneEnemyData(EnemyProData data)
    {
        EnemyProData hero = new EnemyProData();
        hero.id = data.id;
        hero.name = data.name;
        hero.career = data.career;
        hero.level = data.level;
        hero.exp = data.exp;
        hero.thp = data.thp;
        hero.chp = data.chp;
        hero.power = data.power;
        hero.skill = data.skill;
        hero.speed = data.speed;
        hero.lucky = data.lucky;
        hero.pdefense = data.pdefense;
        hero.mdefense = data.mdefense;
        hero.move = data.move;
        hero.strength = data.strength;
        hero.simage = data.simage;
        hero.limage = data.limage;
        hero.prefab = data.prefab;
        hero.fightprefab = data.fightprefab;
        return hero;
    }

    public void Init()
    {
        standbyCount = 0;
        enemyContent.SetActive(true);
        LevelManager instance =  LevelManager.Instance();
        List<int> enemyIdxList = DataManager.Str2List(instance.levelDic[(instance.GetCurLevel()).ToString()].enemy);
        List<int> enemyPosList = DataManager.Str2List(instance.levelDic[(instance.GetCurLevel()).ToString()].enemypos);
        List<int> enemyWeaponList = DataManager.Str2List(instance.levelDic[(instance.GetCurLevel()).ToString()].enemyweapon);
        List<int> enemyLevelList = DataManager.Str2List(instance.levelDic[(instance.GetCurLevel()).ToString()].enemylevel);
        int bossIdx = DataManager.Value(instance.levelDic[(instance.GetCurLevel()).ToString()].boss);

        if (enemyIdxList.Count == 0)
            return;
        //enemy的id有重复的，所以key不能拿来当id获取
        keyEnemyDic.Clear();
        for (int i = 0; i < enemyIdxList.Count; i++)
        {
            EnemyProData enemy = CloneEnemyData(enemyDic[enemyIdxList[i].ToString()]);
            keyEnemyDic.Add(i, CloneEnemyData(enemy));
        }
        for (int i = 0; i < keyEnemyDic.Count; i++)
        {
            GameObject enemy = ResourcesMgr.Instance().GetPool(keyEnemyDic[i].prefab);
            enemy.transform.position = MainManager.Instance().Idx2Pos2(enemyPosList[i]);
            enemy.transform.SetParent(enemyContent.transform);
            enemy.GetComponent<EnemyController>().Init();
            curEnemyList.Add(enemy.GetComponent<EnemyController>());
            curEnemyList[i].mIdx = enemyPosList[i];
            curEnemyList[i].InitData(keyEnemyDic[i]);
            curEnemyList[i].listIdx = i;
            //设置武器
            WeaponData weapon = ItemManager.Instance().CloneWeapon(enemyWeaponList[i]);
            curEnemyList[i].weaponList.Add(weapon);
            //设置等级
            if (curEnemyList[i].mLevel < enemyLevelList[i])
            {
                curEnemyList[i].LevelUp(enemyLevelList[i] - curEnemyList[i].mLevel);
            }
            //设置boss
            if (curEnemyList[i].mID == bossIdx)
            {
                bossName = curEnemyList[i].mName;
                BoxCollider2D bossSize = curEnemyList[i].GetComponent<BoxCollider2D>();
                //enemy的锚点在左上角，暂时直接写死
                Vector3 pos = new Vector3(bossSize.size.x - 4, -bossSize.size.y + 4, 0);
                bossTag = ResourcesMgr.Instance().GetPool(MainProperty.EFFECT_BOSS);
                bossTag.transform.SetParent(curEnemyList[i].transform);
                bossTag.transform.localScale = Vector3.one;
                bossTag.transform.localPosition = pos;
            }
            curEnemyList[i].SetCurWeapon();
        }
        TemproaryDataUpdate();
    }

    public void EnemyClear()
    {
        if (bossTag)
        {
            bossTag.transform.SetParent(null);
            ResourcesMgr.Instance().PushPool(bossTag, MainProperty.EFFECT_BOSS);
        }
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            curEnemyList[i].Clear();
            ResourcesMgr.Instance().PushPool(curEnemyList[i].gameObject, curEnemyList[i].mPrefab);
        }
        curEnemyList.Clear();
    }

    /// <summary>
    /// 设置enemy行动
    /// </summary>
    public void SetEnemyRounnd()
    {
        if (curEnemyList[standbyCount].mName != bossName)
            curEnemyList[standbyCount].SearchHero();
        else
            curEnemyList[standbyCount].SearchHero(false);
    }

    /// <summary>
    /// 初始化enemy
    /// </summary>
    public void SetEnemyInit()
    {
        standbyCount = 0;
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            curEnemyList[i].Init();
        }
    }

    /// <summary>
    /// 记录待机数
    /// </summary>
    public bool SetStandby()
    {
        standbyCount++;
        if (standbyCount == curEnemyList.Count)
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 战斗准备
    /// </summary>
    public void InitFight()
    {
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            curEnemyList[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    public void FightEnd()
    {
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            curEnemyList[i].gameObject.SetActive(true);
            MainManager.Instance().GetMapNode(curEnemyList[i].mIdx).locatedEnemy = curEnemyList[i];
            if (curEnemyList[i].bStandby)
            {
                curEnemyList[i].SetAnimator("bNormal", false);
                curEnemyList[i].SetAnimator("bStandby", true);
            }
        }
    }

    /// <summary>
    /// 设置死亡
    /// </summary>
    public bool SetDead(int idx)
    {
        if (idx >= curEnemyList.Count)
            return false;
        ResourcesMgr.Instance().PushPool(curEnemyList[idx].gameObject, curEnemyList[idx].mPrefab);
        curEnemyList.RemoveAt(idx);
        ResetListIdx();
        if (curEnemyList.Count == 0)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 重置enemy的listIdx
    /// </summary>
    public void ResetListIdx()
    {
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            curEnemyList[i].listIdx = i;
        }
    }

    /// <summary>
    /// 获取enemy个数
    /// </summary>
    public int GetEnemyCount()
    {
        return curEnemyList.Count;
    }

    /// <summary>
    /// 中断隐藏
    /// </summary>
    public void TemporarySave()
    {
        List<TemporaryEnemyData> temp = new List<TemporaryEnemyData>();
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            TemporaryEnemyData data = new TemporaryEnemyData();
            string enemy = JsonMapper.ToJson(curEnemyList[i].SaveData());
            data.enemy = enemy;
            data.pos = curEnemyList[i].mIdx.ToString();
            string item = JsonMapper.ToJson(curEnemyList[i].itemList);
            data.item = item;
            string weapon = JsonMapper.ToJson(curEnemyList[i].weaponList);
            data.weapon = weapon;
            temp.Add(data);
        }
        DataManager.Instance().SaveNormalData<TemporaryEnemyData>(temp, TEMDATA);
        EnemyClear();
        enemyContent.SetActive(false);
    }

    /// <summary>
    /// 更新中断游戏数据，用于当前关卡初始化时
    /// </summary>
    public void TemproaryDataUpdate()
    {
        List<TemporaryEnemyData> temp = new List<TemporaryEnemyData>();
        for (int i = 0; i < curEnemyList.Count; i++)
        {
            TemporaryEnemyData data = new TemporaryEnemyData();
            string enemy = JsonMapper.ToJson(curEnemyList[i].SaveData());
            data.enemy = enemy;
            data.pos = curEnemyList[i].mIdx.ToString();
            string item = JsonMapper.ToJson(curEnemyList[i].itemList);
            data.item = item;
            string weapon = JsonMapper.ToJson(curEnemyList[i].weaponList);
            data.weapon = weapon;
            temp.Add(data);
        }
        DataManager.Instance().SaveNormalData<TemporaryEnemyData>(temp, TEMDATA);
    }

    /// <summary>
    /// 继续中断
    /// </summary>
    public void Continue()
    {
        enemyContent.SetActive(true);
        Dictionary<int, TemporaryEnemyData> saveDic = DataManager.LoadJson<TemporaryEnemyData>(TEMDATA);
        //enemy临时数据为null则直接return
        if (saveDic == null)
        {
            //Dictionary<string, TemporaryEnemyData> temDic = DataManager.Load<TemporaryEnemyData>(TEMDATA);
            //enemyDic = new Dictionary<int, TemporaryEnemyData>();
            //for (int i = 0; i < temDic.Count; i++)
            //{
            //    enemyDic.Add(i, temDic[i.ToString()]);
            //}
            return;
        }
        LevelManager instance = LevelManager.Instance();
        int bossIdx = DataManager.Value(instance.levelDic[(instance.GetCurLevel()).ToString()].boss);
        for (int i = 0; i < saveDic.Count; i++)
        {
            EnemyProData data = JsonMapper.ToObject<EnemyProData>(saveDic[i].enemy);
            GameObject enemy = ResourcesMgr.Instance().GetPool(data.prefab);
            enemy.transform.position = MainManager.Instance().Idx2Pos2(DataManager.Value(saveDic[i].pos));
            enemy.transform.SetParent(enemyContent.transform);
            enemy.GetComponent<EnemyController>().Init();
            curEnemyList.Add(enemy.GetComponent<EnemyController>());
            curEnemyList[i].InitData(data);
            curEnemyList[i].listIdx = i;

            //道具 & 武器
            List<WeaponData> weaponList = JsonMapper.ToObject<List<WeaponData>>(saveDic[i].weapon);
            for (int j = 0; j < weaponList.Count; j++)
            {
                curEnemyList[i].AddItem(weaponList[j]);
            }
            List<ItemData> itemList = JsonMapper.ToObject<List<ItemData>>(saveDic[i].item);
            for (int j = 0; j < itemList.Count; j++)
            {
                curEnemyList[i].AddItem(itemList[j]);
            }
            //设置boss
            if (curEnemyList[i].mID == bossIdx)
            {
                bossName = curEnemyList[i].mName;
                BoxCollider2D bossSize = curEnemyList[i].GetComponent<BoxCollider2D>();
                //enemy的锚点在左上角，暂时直接写死
                Vector3 pos = new Vector3(bossSize.size.x - 4, -bossSize.size.y + 4, 0);
                bossTag = ResourcesMgr.Instance().GetPool(MainProperty.EFFECT_BOSS);
                bossTag.transform.SetParent(curEnemyList[i].transform);
                bossTag.transform.localScale = Vector3.one;
                bossTag.transform.localPosition = pos;
            }
            curEnemyList[i].SetCurWeapon();
        }
    }
}
