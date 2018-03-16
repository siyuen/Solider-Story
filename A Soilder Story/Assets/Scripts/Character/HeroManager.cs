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
    public GameObject heroContent;
    //转换名字的
    public Dictionary<string, string> key2NameDic = new Dictionary<string, string>();
    public Dictionary<string, string> name2KeyDic = new Dictionary<string, string>();
    //hero原始属性
    public Dictionary<string, HeroProData> heroDic;
    //记录hero当前属性
    public Dictionary<string, HeroProData> curHero;
    //hero选项
    public Dictionary<string, HeroOptionData> optionDic;
    public Dictionary<string, HeroOptionData> keyOptionDic = new Dictionary<string,HeroOptionData>();
    public Dictionary<string, MenuView.NormalFunc> funcDic = new Dictionary<string, MenuView.NormalFunc>();
    //活着的herolist
    public List<HeroController> liveHeroList = new List<HeroController>();
    
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
        //int[] test = {40,50,70};
        //读取相应存档里的数据，如果没有相应id则从原始hero中读取(第0关读取原始数据)
        if (instance.GetCurLevel() != 0)
        {
            string path = "Data/HeroData_" + GameManager.Instance().curGameIdx.ToString();
            curHero = DataManager.Load<HeroProData>(path);
        }
        else
            curHero = new Dictionary<string, HeroProData>();
        //读取到的hero是当前拥有的，还需要添加当前关卡新增加的hero,key只是顺序
        if (heroIdxList.Count > 0)
        {
            for (int i = 0; i < heroIdxList.Count; i++)
            {
                //hero的mID（唯一）
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
                //添加到最后一位，key按照顺序
                curHero.Add(curHero.Count.ToString(), hero);
            }
        }
        //根据当前拥有的hero加载
        for (int i = 0; i < curHero.Count; i++)
        {
            GameObject hero = ResourcesMgr.Instance().GetPool(curHero[i.ToString()].prefab);
            hero.transform.position = LevelManager.Instance().Idx2Pos2(heroPosList[i]);
            //hero.transform.position = LevelManager.Instance().Idx2Pos2(test[i]);
            hero.transform.SetParent(heroContent.transform);
            liveHeroList.Add(hero.GetComponent<HeroController>());
            liveHeroList[i].listIdx = i;
            liveHeroList[i].mIdx = heroPosList[i];
            //liveHeroList[i].mIdx = test[i];
            liveHeroList[i].heroState = HeroController.HeroState.normal;
            liveHeroList[i].ClearBag();
            
            liveHeroList[i].InitData(curHero[i.ToString()]);
            List<WeaponData> weaponList = JsonMapper.ToObject<List<WeaponData>>(curHero[i.ToString()].weapon);
            for (int j = 0; j < weaponList.Count; j++)
            {
                liveHeroList[i].AddItem(weaponList[j]);
            }
            List<ItemData> itemList = JsonMapper.ToObject<List<ItemData>>(curHero[i.ToString()].item);
            for (int j = 0; j < itemList.Count; j++)
            {
                liveHeroList[i].AddItem(itemList[j]);
            }
            liveHeroList[i].rolePro.cHp = liveHeroList[i].rolePro.tHp;
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
            ResourcesMgr.Instance().PushPool(liveHeroList[i].gameObject, liveHeroList[i].rolePro.mPrefab);
        }
        liveHeroList.Clear();
    }

    /// <summary>
    /// 清理hero并且保存数据
    /// </summary>
    public void SaveHeroClear()
    {
        curHero.Clear();
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            HeroProData hero = liveHeroList[i].rolePro.GetHeroProData();
            Debug.Log(hero.name + "," + hero.exp);
            hero.item = JsonMapper.ToJson(liveHeroList[i].itemList);
            hero.weapon = JsonMapper.ToJson(liveHeroList[i].weaponList);
            curHero[i.ToString()] = hero;
            liveHeroList[i].Clear();
            ResourcesMgr.Instance().PushPool(liveHeroList[i].gameObject, liveHeroList[i].rolePro.mPrefab);
        }
        liveHeroList.Clear();
    }

    /// <summary>
    /// 将已有的hero数据存储到相应的存档
    /// </summary>
    public void SaveHeroData()
    {
        ////将清理hero时保存的数据存档并清理list
        string path = "Data/HeroData_" + GameManager.Instance().curGameIdx.ToString();
        DataManager.Instance().SaveDicData<HeroProData>(curHero, path);
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
            if (LevelManager.Instance().GetMapNode(liveHeroList[i].mIdx).func == LandManager.RECURE)
            {
                if (liveHeroList[i].rolePro.cHp != liveHeroList[i].rolePro.tHp)
                    hero.Add(i);
            }
        }
        if (hero.Count > 0)
            LandManager.Instance().RecureInit(hero);
        else
            MainManager.Instance().CheckEnd();
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
        return IsAllStandby();
    }

    /// <summary>
    /// 判断是否全部待机
    /// </summary>
    public bool IsAllStandby()
    {
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
            LevelManager.Instance().GetMapNode(liveHeroList[i].mIdx).locatedHero = liveHeroList[i];
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
        if (idx >= liveHeroList.Count)
            return false;
        if (liveHeroList[idx].rolePro.mName == LEADER)
            return true;
        else
        {
            ResourcesMgr.Instance().PushPool(liveHeroList[idx].gameObject, liveHeroList[idx].rolePro.mPrefab);
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
    /// 删除hero数据
    /// </summary>
    public void DeleteHeroFile(int idx)
    {
        string path = "Data/HeroData_" + idx;
        Dictionary<string, HeroProData> list = new Dictionary<string, HeroProData>();
        DataManager.Instance().SaveDicData<HeroProData>(list, path);
    }

    /// <summary>
    /// 中断,隐藏hero，记录当前hero保存数据
    /// </summary>
    public void TemporarySave()
    {
        TemproaryDataUpdate();
        HeroClear();
        curHero.Clear();
        heroContent.SetActive(false);  
    }

    /// <summary>
    /// 更新中断游戏数据，用于当前关卡初始化时
    /// </summary>
    public void TemproaryDataUpdate()
    {
        Dictionary<string, TemporaryHeroData> temp = new Dictionary<string, TemporaryHeroData>();
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            TemporaryHeroData data = new TemporaryHeroData();
            PublicRoleData heroData = liveHeroList[i].rolePro.GetHeroProData();
            string hero = JsonMapper.ToJson(heroData);
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
            temp.Add(i.ToString(), data);
        }
        DataManager.Instance().SaveDicData<TemporaryHeroData>(temp, TEMDATA);
    }

    /// <summary>
    /// 继续游戏,需要读取中断的数据来初始化hero
    /// </summary>
    public void Continue()
    {
        heroContent.SetActive(true);
        Dictionary<string, TemporaryHeroData> heroDic = DataManager.Load<TemporaryHeroData>(TEMDATA);
        if (heroDic == null)
            return;
        curHero = new Dictionary<string, HeroProData>();
        standbyCount = 0;
        for (int i = 0; i < heroDic.Count; i++)
        {
            HeroProData data = JsonMapper.ToObject<HeroProData>(heroDic[i.ToString()].hero);
            GameObject hero = ResourcesMgr.Instance().GetPool(data.prefab);
            hero.transform.position = LevelManager.Instance().Idx2Pos2(DataManager.Value(heroDic[i.ToString()].pos));
            hero.transform.SetParent(heroContent.transform);
            liveHeroList.Add(hero.GetComponent<HeroController>());
            liveHeroList[i].listIdx = i;
            liveHeroList[i].InitData(data);
            liveHeroList[i].Init();
            if (heroDic[i.ToString()].state == STANDBY)
            {
                standbyCount++;
                liveHeroList[i].heroState = HeroController.HeroState.stop;
                liveHeroList[i].SetAnimator("bStandby", true);
                liveHeroList[i].SetAnimator("bNormal", false);
            }
            curHero.Add(liveHeroList[i].rolePro.mID.ToString(), data);
            //道具 & 武器
            liveHeroList[i].ClearBag();
            List<WeaponData> weaponList = JsonMapper.ToObject<List<WeaponData>>(heroDic[i.ToString()].weapon);
            for (int j = 0; j < weaponList.Count; j++)
            {
                liveHeroList[i].AddItem(weaponList[j]);
            }
            List<ItemData> itemList = JsonMapper.ToObject<List<ItemData>>(heroDic[i.ToString()].item);
            for (int j = 0; j < itemList.Count; j++)
            {
                liveHeroList[i].AddItem(itemList[j]);
            }
            liveHeroList[i].SetCurWeapon();
        }
    }
}
