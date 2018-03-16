using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class CareerManager : QSingleton<CareerManager> {

    //用于计算概率
    public const int HUNDRED = 100;
    public Dictionary<string, CareerData> careerDic = new Dictionary<string, CareerData>();
    //以name作为key
    public Dictionary<string, CareerData> keyCareerDic = new Dictionary<string, CareerData>();
    //职业key跟name转换
    public Dictionary<string, string> key2NameDic = new Dictionary<string, string>();
    public Dictionary<string, string> name2KeyDic = new Dictionary<string, string>();

    private CareerManager()
    {
        careerDic = DataManager.Load<CareerData>("Data/CareerData");
        for (int i = 0; i < careerDic.Count; i++)
        {
            keyCareerDic.Add(careerDic[i.ToString()].name, careerDic[i.ToString()]);
            key2NameDic.Add(careerDic[i.ToString()].key, careerDic[i.ToString()].name);
            name2KeyDic.Add(careerDic[i.ToString()].name, careerDic[i.ToString()].key);
        }
    }

    /// <summary>
    /// 武器是否匹配
    /// </summary>
    public bool WeaponMatching(string career, string key)
    {
        string weapon1 = keyCareerDic[career].weaponkey1;
        string weapon2 = keyCareerDic[career].weaponkey2;
        if (key == weapon1 || key == weapon2)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 升级，计算增加属性
    /// </summary>
    public bool LevelUP(string key, string point)
    {
        if(!keyCareerDic.ContainsKey(key))
            return false;
        CareerData career = keyCareerDic[key];
        int random = Random.Range(0, HUNDRED);
        //hp
        if (point == "hp")
        {
            if (random < DataManager.Value(career.hp))
                return true;
            else
                return false;
        }
        //power
        if (point == "power")
        {
            if (random < DataManager.Value(career.power))
                return true;
            else
                return false;
        }
        //skill
        if (point == "skill")
        {
            if (random < DataManager.Value(career.skill))
                return true;
            else
                return false;
        }
        //speed
        if (point == "speed")
        {
            if (random < DataManager.Value(career.speed))
                return true;
            else
                return false;
        }
        //lucky
        if (point == "lucky")
        {
            if (random < DataManager.Value(career.lucky))
                return true;
            else
                return false;
        }
        //pdefense
        if (point == "pdefense")
        {
            if (random < DataManager.Value(career.pdefense))
                return true;
            else
                return false;
        }
        //mdefense
        if (point == "mdefense")
        {
            if (random < DataManager.Value(career.mdefense))
                return true;
            else
                return false;
        }
        return false;
    }
}
