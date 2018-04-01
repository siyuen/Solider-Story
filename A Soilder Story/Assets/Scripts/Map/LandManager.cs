using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class LandManager : QSingleton<LandManager>{
    
    //特殊功能
    //普通
    public const int NORMAL = 0;
    //恢复
    public const int RECURE = 1;
    //裂缝,可攻击
    public const int CRACK = 2;

    public Dictionary<string, LandData> landDic;
    public Dictionary<string, LandData> keyLandDic = new Dictionary<string, LandData>();
    public List<LandData> landList = new List<LandData>();

    //记录人物
    private List<int> roleList;

    private LandManager()
    {
        landDic = DataManager.Load<LandData>("Data/LandData");
        for (int i = 0; i < landDic.Count; i++)
        {
            keyLandDic.Add(landDic[i.ToString()].key, landDic[i.ToString()]);
            landList.Add(landDic[i.ToString()]);
        }
    }

    /// <summary>
    /// 特殊地形，恢复生命
    /// </summary>
    public void RecureInit(List<int> hero)
    {
        if (hero.Count == 0)
            MainManager.Instance().CheckEnd();
        else
        {
            roleList = hero;
            RecureHp();
        }
    }

    public void RecureHp()
    {
        MainManager.Instance().curHero = HeroManager.Instance().liveHeroList[roleList[0]];
        HeroController hero = MainManager.Instance().curHero;
        hero.SetAnimator("bMouse", true);
        hero.SetAnimator("bNormal", false);
        UIManager.Instance().ShowUIForms("UseItem");
        hero.rolePro.SetProValue(RolePro.PRO_CHP, hero.rolePro.cHp + 4);
        if (hero.rolePro.cHp > hero.rolePro.tHp)
            hero.rolePro.SetProValue(RolePro.PRO_CHP, hero.rolePro.tHp);
        UIManager.Instance().GetUI("UseItem").GetComponent<UseItemView>().UpdateUI("hp");
    }

    public void RecureEnd()
    {
        UIManager.Instance().CloseUIForms("UseItem");
        HeroController hero = MainManager.Instance().curHero;
        hero.SetAnimator("bNormal", true);
        hero.SetAnimator("bMouse", false);
        roleList.RemoveAt(0);
        //开始新回合
        if (roleList.Count == 0)
            MainManager.Instance().CheckEnd();
        else
            RecureHp();
    }

    public void Recure(EnemyController enemy)
    {
        MainManager.Instance().curEnemy = enemy;
        UIManager.Instance().ShowUIForms("UseItem");
        enemy.rolePro.SetProValue(RolePro.PRO_CHP, enemy.rolePro.cHp + 4);
        if (enemy.rolePro.cHp > enemy.rolePro.tHp)
            enemy.rolePro.SetProValue(RolePro.PRO_CHP, enemy.rolePro.tHp);
        UIManager.Instance().GetUI("UseItem").GetComponent<UseItemView>().UpdateUI("hp");
    }

}
