using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class HeroProperty{
    public enum HeroOptions
    {
        //待机
        Standby,
        //物品
        Item,
        //攻击
        Attack,
    }

    public class HeroOption : QSingleton<HeroOption>
    {
        public Dictionary<HeroOptions, string> option2Str = new Dictionary<HeroOptions, string>();
        //选项排序
        public Dictionary<HeroOptions, int> optionValue = new Dictionary<HeroOptions,int>();
        private HeroOption()
        {
            optionValue.Add(HeroOptions.Standby, 2);
            optionValue.Add(HeroOptions.Item, 1);
            optionValue.Add(HeroOptions.Attack, 0);

            option2Str.Add(HeroOptions.Standby, "待机");
            option2Str.Add(HeroOptions.Item, "物品");
            option2Str.Add(HeroOptions.Attack, "攻击");
        }

        public void Init()
        {
            //optionList.Add(HeroOptions.Standby);
            //optionList.Add(HeroOptions.Item);
        }
    }
}
