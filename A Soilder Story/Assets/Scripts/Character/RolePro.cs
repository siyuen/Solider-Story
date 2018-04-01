using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RolePro{

    //可更改的属性
    public const string PRO_LEVEL = "level";
    public const string PRO_EXP = "exp";
    public const string PRO_THP = "thp";
    public const string PRO_CHP = "chp";
    public const string PRO_POWER = "power";
    public const string PRO_SKILL = "skill";
    public const string PRO_SPEED = "speed";
    public const string PRO_LUCKY = "lucky";
    public const string PRO_PDEFENSE = "pdefense";
    public const string PRO_MDEFENSE = "mdefense";

    public int mID { get; private set; }                       //人物Id
    public string mName { get; private set; }                  //名字
    public string mCareer { get; private set; }                //职业
    public int mLevel { get; private set; }                    //等级
    public int mExp { get; private set; }                      //经验
    public int tHp { get; private set; }                       //总血量
    public int cHp { get; private set; }                       //当前血量

    public int mPower { get; private set; }                    //力量
    public int mSkill { get; private set; }                    //技术
    public int mSpeed { get; private set; }                    //速度
    public int mLucky { get; private set; }                    //幸运
    public int pDefense { get; private set; }                  //守备
    public int mDefense { get; private set; }                  //魔防
    public int mMove { get; private set; }                     //移动
    public int mStrength { get; private set; }                 //体格
    public string sImage { get; private set; }                 //小头像路径
    public string lImage { get; private set; }                 //大头像路径
    public string mPrefab { get; private set; }                //预制体路径
    public string fightPrefab { get; private set; }            //战斗预制体路径

    public RolePro(PublicRoleData data)
    {
        mID = DataManager.Value(data.id);
        mCareer = CareerManager.Instance().key2NameDic[data.career];
        mLevel = DataManager.Value(data.level);
        mExp = DataManager.Value(data.exp);
        mName = HeroManager.Instance().key2NameDic[data.name];
        tHp = DataManager.Value(data.thp);
        cHp = DataManager.Value(data.chp);
        mPower = DataManager.Value(data.power);
        mSkill = DataManager.Value(data.skill);
        mSpeed = DataManager.Value(data.speed);
        mLucky = DataManager.Value(data.lucky);
        pDefense = DataManager.Value(data.pdefense);
        mDefense = DataManager.Value(data.mdefense);
        mMove = DataManager.Value(data.move);
        mStrength = DataManager.Value(data.strength);
        sImage = data.simage;
        lImage = data.limage;
        mPrefab = data.prefab;
        fightPrefab = data.fightprefab;
    }

    /// <summary>
    /// 只负责更改属性
    /// </summary>
    public void SetProValue(string name, int value)
    {
        switch(name)
        {
            case PRO_LEVEL:
                mLevel = value;
                break;
            case PRO_EXP:
                mExp = value;
                break;
            case PRO_THP:
                tHp = value;
                break;
            case PRO_CHP:
                cHp = value;
                break;
            case PRO_POWER:
                mPower = value;
                break;
            case PRO_SKILL:
                mSkill = value;
                break;
            case PRO_SPEED:
                mSpeed = value;
                break;
            case PRO_LUCKY:
                mLucky = value;
                break;
            case PRO_PDEFENSE:
                pDefense = value;
                break;
            case PRO_MDEFENSE:
                mDefense = value;
                break;
            default:
                break;
        }
    }


    #region 克隆数据
    public HeroProData GetHeroProData()
    {
        HeroProData hero = new HeroProData();
        hero.id = mID.ToString();
        hero.career = CareerManager.Instance().name2KeyDic[mCareer];
        hero.level = mLevel.ToString();
        hero.exp = mExp.ToString();
        hero.name = HeroManager.Instance().name2KeyDic[mName];
        hero.thp = tHp.ToString();
        hero.chp = cHp.ToString();
        hero.power = mPower.ToString();
        hero.skill = mSkill.ToString();
        hero.speed = mSpeed.ToString();
        hero.lucky = mLucky.ToString();
        hero.pdefense = pDefense.ToString();
        hero.mdefense = mDefense.ToString();
        hero.move = mMove.ToString();
        hero.strength = mStrength.ToString();
        hero.simage = sImage.ToString();
        hero.limage = lImage.ToString();
        hero.prefab = mPrefab.ToString();
        hero.fightprefab = fightPrefab.ToString();
        return hero;
    }

    public EnemyProData GetEnemyProData()
    {
        EnemyProData enemy = new EnemyProData();
        enemy.id = mID.ToString();
        enemy.career = CareerManager.Instance().name2KeyDic[mCareer];
        enemy.level = mLevel.ToString();
        enemy.exp = mExp.ToString();
        enemy.name = HeroManager.Instance().name2KeyDic[mName];
        enemy.thp = tHp.ToString();
        enemy.chp = cHp.ToString();
        enemy.power = mPower.ToString();
        enemy.skill = mSkill.ToString();
        enemy.speed = mSpeed.ToString();
        enemy.lucky = mLucky.ToString();
        enemy.pdefense = pDefense.ToString();
        enemy.mdefense = mDefense.ToString();
        enemy.move = mMove.ToString();
        enemy.strength = mStrength.ToString();
        enemy.simage = sImage.ToString();
        enemy.limage = lImage.ToString();
        enemy.prefab = mPrefab.ToString();
        enemy.fightprefab = fightPrefab.ToString();
        return enemy;
    }
    #endregion
}
