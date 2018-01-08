using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class HeroManager : QSingleton<HeroManager> {

    //活着的herolist
    private List<HeroController> liveHeroList = new List<HeroController>();
    //死亡的herolist
    private List<HeroController> deadHeroList = new List<HeroController>();
    private GameObject heroContent;
    private int standbyCount;

    private HeroManager()
    {
    }

    public void Init(int level)
    {
        standbyCount = 0;
        heroContent = new GameObject();
        heroContent.name = "Hero";
        GameObject hero = GameObjectPool.Instance().GetPool(MainProperty.HERO_PATH, MainManager.Instance().Idx2Pos2(50));
        hero.transform.SetParent(heroContent.transform);
        liveHeroList.Add(hero.GetComponent<HeroController>());
        //liveHeroList[0].Init();
    }

    public void SetHeroRound()
    {
        for (int i = 0; i < liveHeroList.Count; i++)
        {
            liveHeroList[i].Init();
        }
    }

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

    /// <summary>
    /// 根据idx获取hero
    /// </summary>
    public HeroController GetHero(int idx)
    {
        if (idx > liveHeroList.Count)
            return null;
        return liveHeroList[idx];
    }
}
