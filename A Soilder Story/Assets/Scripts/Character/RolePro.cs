using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RolePro{

    public int mID;                                         //人物Id
    public string mName;                                    //名字
    public string mCareer;                                  //职业
    public int mLevel;                                      //等级
    public int mExp;                                        //经验
    public int tHp;                                         //总血量
    public int cHp;                                         //当前血量

    public int mPower;                                      //力量
    public int mSkill;                                      //技术
    public int mSpeed;                                      //速度
    public int mLucky;                                      //幸运
    public int pDefense;                                    //守备
    public int mDefense;                                    //魔防
    public int mMove;                                       //移动
    public int mStrength;                                   //体格
    public string sImage;                                   //小头像路径
    public string lImage;                                   //大头像路径
    public string mPrefab;                                  //预制体路径
    public string fightPrefab;                              //战斗预制体路径

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
}
