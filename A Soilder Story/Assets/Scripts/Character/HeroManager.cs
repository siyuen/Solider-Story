using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using System.Text.RegularExpressions;
using LitJson;

public class HeroManager : QSingleton<HeroManager> {

    //临时数据存储地址
    public const string TEMDATA = "Data/TemporaryHeroData";
    //hero数据存储地址
    public const string HEROPRODATA = "Data/HeroPropertyData";
    //名字数据存储地址
    public const string NAMEDATA = "Data/HeroData";
    //待机
    public const string STANDBY = "daiji";
    //正常
    public const string NORMAL = "normal";
    //主角
    public const string LEADER = "琳";
    //转换名字的
    public Dictionary<string, string> key2NameDic = new Dictionary<string, string>();
    public Dictionary<string, string> name2KeyDic = new Dictionary<string, string>();
    //hero原始属性
    public Dictionary<string, HeroProData> heroDic;
    //记录hero当前属性
    public Dictionary<int, HeroProData> curHero;
    //hero选项
    public Dictionary<string, HeroOptionData> optionDic;
    public Dictionary<string, HeroOptionData> keyOptionDic = new Dictionary<string,HeroOptionData>();
    public Dictionary<string, MenuView.NormalFunc> funcDic = new Dictionary<string, MenuView.NormalFunc>();
    //活着的herolist
    public List<HeroController> liveHeroList = new List<HeroController>();
    //死亡的herolist
    private List<HeroController> deadHeroList = new List<HeroController>();

    public GameObject heroContent;
    private int standbyCount;


    private HeroManager()
    {
        Dictionary<string, HeroData> name = DataManager.Load<HeroData>(NAMEDATA);
        for (int i = 0; i < name.Count; i++)
        {
            key2NameDic.Add(name[i.ToString()].key, name[i.ToString()].name);
            name2KeyDic.Add(name[i.ToString()].name, name[i.ToString()].key);
        }

        heroDic = DataManager.Load<HeroProData>(HEROPRODATA);

        optionDic = DataManager.Load<HeroOptionData>("Data/HeroOptionData");
        for (int i = 0; i < optionDic.Count; i++)
        {
            keyOptionDic.Add(optionDic[i.ToString()].name, optionDic[i.ToString()]);
        }
        heroContent = new GameObject();
        heroContent.name = "Hero";
        InitOption();

    }

    public HeroProData CloneHeroData(HeroProData data)
    {
        HeroProData hero = new HeroProData();
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
        heroContent.SetActive(true);
        LevelManager instance = LevelManager.Instance();
        List<int> heroPosList = DataManager.Str2List(instance.levelDic[(instance.GetCurLevel()).ToString()].heropos);
        //根据关卡来添加hero
        List<int> heroIdxList = DataManager.Str2List(instance.levelDic[(instance.GetCurLevel()).ToString()].hero);
        int[] test = {32,54,65};
        //读取相应存档里的数据，如果没有相应id则从原始hero中读取
        string path = "Data/HeroData_" + GameManager.Instance().curGameIdx.ToString();
        curHero = DataManager.LoadJson<HeroProData>(path);
        //读取到的hero是当前拥有的，还需要添加当前关卡新增加的hero
        if (heroIdxList.Count > 0)
        {
            if (curHero == null)
                curHero = new Dictionary<int, HeroProData>();
            for (int i = 0; i < heroIdxList.Count; i++)
            {
                if (curHero.ContainsKey(heroIdxList[i]))
                    return;
                string id = heroIdxList[i].ToString();
                //需要new一个新的防止改变原始数据
                HeroProData hero = CloneHeroData(heroDic[id]);
                List<WeaponData> weaponList = new List<WeaponData>();
                List<int> weaponIdx = DataManager.Str2List(heroDic[id].weapon);
                for (int j = 0; j < weaponIdx.Count; j++)
                {
                    WeaponData weapon = ItemManager.Instance().CloneWeapon(weaponIdx[j]);
                    weaponList.Add(weapon);
                }
                hero.weapon = JsonMapper.ToJson(weaponList);
                List<ItemData> itemList = new List<ItemData>();
                List<int> itemIdx = DataManager.Str2List(heroDic[id].item);
                for (int j = 0; j < itemIdx.Count; j++)
                {
                    ItemData item = ItemManager.Instance().CloneItem(itemIdx[j]);
                    itemList.Add(item);
                }
                hero.item = JsonMapper.ToJson(itemList);
                curHero.Add(heroIdxList[i], hero);
            }
        }
        Debug.Log("添加Done");
        for (int i = 0; i < curHero.Count; i++)
        {
            GameObject hero = ResourcesMgr.Instance().GetPool(curHero[i].prefab);
            //hero.transform.position = MainManager.Instance().Idx2Pos2(heroPosList[i]);
            hero.transform.position = MainManager.Instance().Idx2Pos2(test[i]);
            hero.transform.SetParent(heroContent.transform);
            liveHeroList.Add(hero.GetComponent<HeroController>());
            liveHeroList[i].listIdx = i;
            //liveHeroList[i].mIdx = heroPosList[i];
            liveHeroList[i].mIdx = test[i];
            liveHeroList[i].ClearBag();
            if (curHero != null && curHero.Count > 0)
            {
                liveHeroList[i].InitData(curHero[i]);
                List<WeaponData> weaponList = JsonMapper.ToObject<List<WeaponData>>(curHero[i].weapon);
                for (int j = 0; j < weaponList.Count; j++)
                {
                    liveHeroList[i].AddItem(weaponList[j]);
                }
                List<ItemData> itemList = JsonMapper.ToObject<List<ItemData>>(curHero[i].item);
                for (int j = 0; j < itemList.Count; j++)
                {
                    liveHeroList[i].AddItem(itemList[j]);
                }
            }
            liveHeroList[i].cHp = liveHeroList[i].tHp;
            liveHeroList[i].SetCurWeapon();
        }
        TemproaryDataUpdate();
    }

    /// <summary>
    /// 清理hero，用于重新开始/中断游戏
    /// </summary>
    public void HeroClear()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            liveHeroList[i].Clear();
            ResourcesMgr.Instance().PushPool(liveHeroList[i].gameObject, liveHeroList[i].mPrefab);
        }
        liveHeroList.Clear();
    }

    /// <summary>
    /// 清理hero并且保存数据
    /// </summary>
    public void SaveHeroClear()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            //相应id的hero保存数据
            curHero[liveHeroList[i].mID] = liveHeroList[i].SaveData();
            curHero[liveHeroList[i].mID].item = JsonMapper.ToJson(liveHeroList[i].itemList);
            curHero[liveHeroList[i].mID].weapon = JsonMapper.ToJson(liveHeroList[i].weaponList);
            liveHeroList[i].Clear();
            ResourcesMgr.Instance().PushPool(liveHeroList[i].gameObject, liveHeroList[i].mPrefab);
        }
        liveHeroList.Clear();
    }

    /// <summary>
    /// 将已有的hero数据存储到相应的存档
    /// </summary>
    public void SaveHeroData()
    {
        ////将清理hero时保存的数据存档并清理list
        for (int i = 0; i < curHero.Count; i++)
        {
            DataManager.Instance().SaveHeroData(curHero[i]);
        }
        curHero.Clear();
    }

    public void SetHeroRound()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            liveHeroList[i].Init();
        }
    }

    /// <summary>
    /// 检查特殊地形
    /// </summary>
    public void CheckLand()
    {
        MainManager.Instance().mainState = MainManager.MainState.CheckLand;
        List<int> hero = new List<int>();
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            if (MainManager.Instance().GetMapNode(liveHeroList[i].mIdx).func == LandManager.RECURE)
            {
                if(liveHeroList[i].cHp != liveHeroList[i].tHp)
                    hero.Add(i);
            }
        }
        LandManager.Instance().RecureInit(hero);
    }

    #region hero选项
    private void InitOption()
    {
        funcDic.Add("攻击", Attack);
        funcDic.Add("占领", Occupy);
        funcDic.Add("物品", Item);
        funcDic.Add("交换", Change);
        funcDic.Add("待机", Standby);
    }

    /// <summary>
    /// 攻击
    /// </summary>
    private void Attack()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        ItemManager.Instance().Init(ItemManager.WEAPONMENU);
    }

    /// <summary>
    /// 光标在attack选项时的显示
    /// </summary>
    public void MoveToAttack()
    {
        MoveManager.Instance().ShowAttackRange();
    }

    /// <summary>
    /// 占领
    /// </summary>
    private void Occupy()
    {
        Debug.Log("占领咯");
        UIManager.Instance().CloseUIForms("HeroMenu");
        GameManager.Instance().GameOver(GameManager.SUCCESS);
    }

    /// <summary>
    /// 道具
    /// </summary>
    private void Item()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        ItemManager.Instance().Init(ItemManager.ITEMENNU);
    }

    /// <summary>
    /// 交换
    /// </summary>
    private void Change()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        UIManager.Instance().ShowUIForms("ChangeItem");
    }

    /// <summary>
    /// 待机
    /// </summary>
    private void Standby()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        MainManager.Instance().curHero.Standby();
    }

    /// <summary>
    /// 其它选项
    /// </summary>
    public void MoveToAnother()
    {
        MoveManager.Instance().HideAttackRange();
    }
    #endregion

    /// <summary>
    /// 记录待机hero
    /// </summary>
    public bool SetStandby()
    {
        standbyCount++;
        if (standbyCount == liveHeroList.Count)
        {
            standbyCount = 0;
            return true;
        }
        else
            return false;
    }

    public void SetAllStandby()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            if (liveHeroList[i].heroState != HeroController.HeroState.stop)
                liveHeroList[i].Standby();
        }
    }

    /// <summary>
    /// 根据idx获取hero
    /// </summary>
    public HeroController GetHero(int idx)
    {
        if (idx > liveHeroList.Count)
            return null;
        return liveHeroList[idx];
    }

    /// <summary>
    /// 战斗准备
    /// </summary>
    public void InitFight()
    {
        MoveManager.Instance().HideAttackRange();
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            liveHeroList[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 结束战斗,重新设置动画跟mapNode上的hero
    /// </summary>
    public void FightEnd()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            liveHeroList[i].gameObject.SetActive(true);
            MainManager.Instance().GetMapNode(liveHeroList[i].mIdx).locatedHero = liveHeroList[i];
            if (liveHeroList[i].heroState == HeroController.HeroState.stop)
            {
                liveHeroList[i].SetAnimator("bNormal", false);
                liveHeroList[i].SetAnimator("bStandby", true);
            }
        }
    }

    /// <summary>
    /// 设置死亡
    /// </summary>
    public bool SetDead(int idx)
    {
        Debug.Log(idx + "," + liveHeroList[idx].mName);
        if (idx >= liveHeroList.Count)
            return false;
        if (liveHeroList[idx].mName == LEADER)
            return true;
        else
        {
            deadHeroList.Add(liveHeroList[idx]);
            ResourcesMgr.Instance().PushPool(liveHeroList[idx].gameObject, liveHeroList[idx].mPrefab);
            liveHeroList.RemoveAt(idx);
            ResetListIdx();
            return false;
        }
    }

    /// <summary>
    /// 重置hero的listIdx
    /// </summary>
    public void ResetListIdx()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            liveHeroList[i].listIdx = i;
        }
    }

    /// <summary>
    /// 中断,隐藏hero，记录当前hero保存数据
    /// </summary>
    public void TemporarySave()
    {
        List<TemporaryHeroData> temp = new List<TemporaryHeroData>();
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            TemporaryHeroData data = new TemporaryHeroData();
            string hero = JsonMapper.ToJson(liveHeroList[i].SaveData());
            data.hero = hero;
            data.pos = liveHeroList[i].mIdx.ToString();
            if (liveHeroList[i].heroState == HeroController.HeroState.stop)
                data.state = STANDBY;
            else
                data.state = NORMAL;
            string item = JsonMapper.ToJson(liveHeroList[i].itemList);
            data.item = item;
            string weapon = JsonMapper.ToJson(liveHeroList[i].weaponList);
            data.weapon = weapon;
            temp.Add(data);
        }
        DataManager.Instance().SaveNormalData<TemporaryHeroData>(temp, TEMDATA);
        HeroClear();
        curHero.Clear();
        heroContent.SetActive(false);  
    }

    /// <summary>
    /// 更新中断游戏数据，用于当前关卡初始化时
    /// </summary>
    public void TemproaryDataUpdate()
    {
        List<TemporaryHeroData> temp = new List<TemporaryHeroData>();
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            TemporaryHeroData data = new TemporaryHeroData();
            string hero = JsonMapper.ToJson(liveHeroList[i].SaveData());
            data.hero = hero;
            data.pos = liveHeroList[i].mIdx.ToString();
            if (liveHeroList[i].heroState == HeroController.HeroState.stop)
                data.state = STANDBY;
            else
                data.state = NORMAL;
            string item = JsonMapper.ToJson(liveHeroList[i].itemList);
            data.item = item;
            string weapon = JsonMapper.ToJson(liveHeroList[i].weaponList);
            data.weapon = weapon;
            temp.Add(data);
        }
        DataManager.Instance().SaveNormalData<TemporaryHeroData>(temp, TEMDATA);
    }

    /// <summary>
    /// 继续游戏,需要读取中断的数据来初始化hero
    /// </summary>
    public void Continue()
    {
        heroContent.SetActive(true);
        Dictionary<int, TemporaryHeroData> heroDic = DataManager.LoadJson<TemporaryHeroData>(TEMDATA);
        if (heroDic == null)
            return;
        if (curHero == null)
            curHero = new Dictionary<int, HeroProData>();
        for (int i = 0; i < heroDic.Count; i++)
        {
            HeroProData data = JsonMapper.ToObject<HeroProData>(heroDic[i].hero);
            GameObject hero = ResourcesMgr.Instance().GetPool(data.prefab);
            hero.transform.position = MainManager.Instance().Idx2Pos2(DataManager.Value(heroDic[i].pos));
            hero.transform.SetParent(heroContent.transform);
            liveHeroList.Add(hero.GetComponent<HeroController>());
            liveHeroList[i].InitData(data);
            liveHeroList[i].Init();
            if (heroDic[i].state == STANDBY)
            {
                liveHeroList[i].heroState = HeroController.HeroState.stop;
                liveHeroList[i].SetAnimator("bStandby", true);
                liveHeroList[i].SetAnimator("bNormal", false);
            }
            curHero.Add(liveHeroList[i].mID, data);
            //道具 & 武器
            liveHeroList[i].ClearBag();
            List<WeaponData> weaponList = JsonMapper.ToObject<List<WeaponData>>(heroDic[i].weapon);
            for (int j = 0; j < weaponList.Count; j++)
            {
                liveHeroList[i].AddItem(weaponList[j]);
            }
            List<ItemData> itemList = JsonMapper.ToObject<List<ItemData>>(heroDic[i].item);
            for (int j = 0; j < itemList.Count; j++)
            {
                liveHeroList[i].AddItem(itemList[j]);
            }
            liveHeroList[i].SetCurWeapon();
        }
    }
}
