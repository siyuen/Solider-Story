﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;
using QFramework;
using System;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif  

public class DataManager : QMonoSingleton<DataManager>{

    public const int MAXVALUE = 100;

    public List<HeroProData> heroDataList = new List<HeroProData>();

    #region 数据计算
    /// <summary>
    /// 获取攻击力
    /// </summary>
    public static int GetAttack(Character role, WeaponData weapon)
    {
        int attack = DataManager.Value(weapon.attack) + role.mPower;
        return attack;
    }

    /// <summary>
    /// 获取命中
    /// </summary>
    public static int GetHit(Character role, WeaponData weapon)
    {
        int hit = DataManager.Value(weapon.hit) + role.mSkill * 2 + role.mLucky / 2;
        if (hit > MAXVALUE)
            hit = MAXVALUE;
        return hit;
    }

    /// <summary>
    /// 获取必杀
    /// </summary>
    public static int GetCrt(Character role, WeaponData weapon)
    {
        int crt = DataManager.Value(weapon.critical) + role.mSkill / 2;
        return crt;
    }

    /// <summary>
    /// 获取闪避
    /// </summary>
    public static int GetMiss(Character role, WeaponData weapon)
    {
        int miss = GetAttackSpeed(role, weapon) * 2 + role.mLucky;
        int node = MainManager.Instance().GetMapNode(role.mIdx).mAvo;
        if (node == LevelManager.NULLNODE)
            node = 0;
        miss += node;
        return miss;
    }


    /// <summary>
    /// 获取物理防御
    /// </summary>
    public static int GetDefense(Character role)
    {
        int node = MainManager.Instance().GetMapNode(role.mIdx).mdef;
        if(node == LevelManager.NULLNODE)
            node = 0;
        int def = role.mDefense + node;
        return def;
    }

    /// <summary>
    /// 获取武器克制关系;被克制return 1；克制return 0；没关系return -1
    /// </summary>
    public static int GetWeaponCounter(WeaponData weapon1, WeaponData weapon2)
    {
        if (weapon1 == null || weapon2 == null)
            return -1;
        string key1 = weapon1.key;
        string key2 = weapon2.key;
        //剑 -> 斧 -> 枪 ->剑
        if (key1 == "sword")
        {
            if (key2 == "axe")
                return 0;
            else if (key2 == "spear")
                return 1;
        }
        else if (key1 == "axe")
        {
            if (key2 == "spear")
                return 0;
            else if (key2 == "sword")
                return 1;
        }
        else if (key1 == "spear")
        {
            if (key2 == "sword")
                return 0;
            else if (key2 == "axe")
                return 1;
        }
        return -1;
    }

    /// <summary>
    /// 获取伤害值
    /// </summary>
    public static int GetDamge(Character role1, Character role2)
    {
        int attack = DataManager.Value(role1.curWeapon.attack) + role1.mPower;
        int defense = GetDefense(role2);
        int dmg = attack - defense;
        if (role2.curWeapon == null)
            return dmg;
        if (GetWeaponCounter(role1.curWeapon, role2.curWeapon) == 0)
            dmg += 1;
        else if (GetWeaponCounter(role1.curWeapon, role2.curWeapon) == 1)
            dmg -= 1;
        return dmg;
    }

    /// <summary>
    /// 获取实战命中
    /// </summary>
    public static int GetFightHit(Character role1, Character role2)
    {
        WeaponData weapon1 = role1.curWeapon;
        int hit = DataManager.Value(weapon1.hit) + role1.mSkill * 2 + role1.mLucky / 2;
        if (role2.curWeapon != null)
        {
            if (GetWeaponCounter(role1.curWeapon, role2.curWeapon) == 0)
                hit += 15;
            else if (GetWeaponCounter(role1.curWeapon, role2.curWeapon) == 1)
                hit -= 15;
            int miss = GetMiss(role2, role2.curWeapon);
            hit -= miss;
        }
        if (hit > 100)
            hit = 100;
        return hit;
    }

    /// <summary>
    /// 获取攻速
    /// </summary>
    public static int GetAttackSpeed(Character role, WeaponData weapon)
    {
        if (weapon == null)
            return role.mSpeed;
        int speed = DataManager.Value(weapon.weight) - role.mStrength;
        if (speed < 0)
            speed = 0;
        speed += role.mSpeed;
        return speed;
    }

    /// <summary>
    /// 判断能否追击
    /// </summary>
    public static bool CanDoubleAttack(Character role1, Character role2)
    {
        int speed1 = GetAttackSpeed(role1, role1.curWeapon);
        int speed2 = GetAttackSpeed(role2, role2.curWeapon);
        if (speed1 - speed2 >= 4)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 获取经验
    /// </summary>
    public static int GetExp(Character role1, Character role2, bool dead)
    {
        int e = 0;
        if (dead)
            e = 30;
        else
            e = 10;
        int exp = e + (role2.mLevel - role1.mLevel);
        if (exp < 0)
            exp = 0;
        else if (exp > 100)
            exp = 100;
        return exp;
    }

    #endregion

    #region 公有方法
    /// <summary>
    /// 将string转换为int
    /// </summary>
    public static int Value(string m)
    {
        int intA;
        int.TryParse(m, out intA);
        return intA;
    }

    /// <summary>
    /// string转换为List<int>，“，”为分隔
    /// </summary>
    public static List<int> Str2List(string str)
    {
        List<int> list = new List<int>();
        string j = "";
        char[] c = str.ToCharArray();
        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == ',')
            {
                list.Add(int.Parse(j));
                j = "";
            }
            else
            {
                j += c[i].ToString();
            }
        }
        return list;
    }

    /// <summary>
    /// Unicode转字符串
    /// </summary>
    public static string UnicodeToString(string unicode)
    {
        string resultStr = "";
        string[] strList = unicode.Split('u');
        for (int i = 1; i < strList.Length; i++)
        {
            resultStr += (char)int.Parse(strList[i], System.Globalization.NumberStyles.HexNumber);
        }
        return resultStr;
    }

    /// <summary>  
    /// 字符串转为UniCode码字符串  
    /// </summary>  
    public static string StringToUnicode(string s)
    {
        char[] charbuffers = s.ToCharArray();
        byte[] buffer;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < charbuffers.Length; i++)
        {
            buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
            sb.Append(String.Format("//u{0:X2}{1:X2}", buffer[1], buffer[0]));
        }
        return sb.ToString();
    }  

    #endregion

    /// <summary>
    /// 加载初始数据
    /// </summary>
    public static Dictionary<string, T> Load<T>(string rName)
    {
        TextAsset s = Resources.Load(rName) as TextAsset;
        if (!s)
            return null;
        string strData = s.text;
        Dictionary<string, T> data = JsonMapper.ToObject<Dictionary<string, T>>(strData);
        return data;
    }

    /// <summary>
    /// 加载游戏存储的数据
    /// </summary>
    public static Dictionary<int, T> LoadJson<T>(string rName)
    {
        TextAsset s = Resources.Load(rName) as TextAsset;
        if (!s)
            return null;
        string strData = s.text;
        List<T> data = JsonMapper.ToObject<List<T>>(strData);
        Dictionary<int, T> dic = new Dictionary<int, T>();
        if (data.Count == 0)
            return null;
        for (int i = 0; i < data.Count; i++)
        {
            dic.Add(i, data[i]);
        }
        return dic;
    }


    public void Test()
    {
        Dictionary<int, TemporaryHeroData> dic = DataManager.LoadJson<TemporaryHeroData>("Data/TemporaryHeroData");
        if (dic == null)
        {
            Dictionary<string, TemporaryHeroData> test = DataManager.Load<TemporaryHeroData>("Data/TemporaryHeroData");
            dic.Add(0, test["0"]);
        }
        //Dictionary<string, LandData> game = DataManager.Load<LandData>("Data/LandData");
        //SaveDicData<LandData>(game, "Data/TestData");
        Dictionary<string, LandData> testDic = DataManager.Load<LandData>("Data/TestData");
        Debug.Log(testDic.Count);
        //List<ItemData> save = HeroManager.Instance().liveHeroList[0].itemList;
        //string saveObject = JsonMapper.ToJson(save);
        //dic[0].item = saveObject;
        //List<TemporaryHeroData> data = new List<TemporaryHeroData>();
        //data.Add(dic[0]);
        //Debug.Log(data[0].item);
        //SaveNormalData<TemporaryHeroData>(data, "Data/TemporaryHeroData");
        //List<ItemData> gameObject = JsonMapper.ToObject<List<ItemData>>(dic[0].item);
        //Debug.Log(gameObject[0].name);
    }

    public static void Save()
    {
        string tempPath = Application.dataPath + @"/Resources/Data/temp.json";
        FileInfo fileInfo = new FileInfo(tempPath);
        //把类转换为Json格式的String 
        string str = JsonMapper.ToJson(DataManager.Instance().heroDataList);
        //写入本地 
        StreamWriter sw = fileInfo.CreateText();
        sw.WriteLine(str); 
        sw.Close(); 
        sw.Dispose(); 
    }


    #region hero数据
    /// <summary>
    /// 开启一个存档时需要读取相应数据
    /// 保存存档时更新数据
    /// </summary>
    public void SaveHeroData(HeroProData data)
    {
        string game = GameManager.Instance().curGameIdx.ToString();
        string filePath = Application.dataPath + @"/Resources/Data/HeroData_" + game + ".txt";

        if (!File.Exists(filePath))
        {
            heroDataList.Add(data);
        }
        else
        {
            bool bFind = false;
            for (int i = 0; i < heroDataList.Count; i++)
            {
                HeroProData saveData = heroDataList[i];
                if (data.id == saveData.id)
                {
                    saveData.name = data.name;
                    saveData.career = data.career;
                    saveData.level = data.level;
                    saveData.exp = data.exp;
                    saveData.thp = data.thp;
                    saveData.chp = data.chp;
                    saveData.power = data.power;
                    saveData.skill = data.skill;
                    saveData.speed = data.speed;
                    saveData.lucky = data.lucky;
                    saveData.pdefense = data.pdefense;
                    saveData.mdefense = data.mdefense;
                    saveData.move = data.move;
                    saveData.strength = data.strength;
                    bFind = true;
                    break;
                }
            }
            if (!bFind)
                heroDataList.Add(data);
        }
        FileInfo file = new FileInfo(filePath);
        StreamWriter sw = file.CreateText();

        string json = JsonMapper.ToJson(heroDataList);
        sw.WriteLine(json);
        sw.Close();
        sw.Dispose();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    #endregion#endregion

    public void SaveGameData(List<GameData> gameDataList)
    {
        string filePath = Application.dataPath + @"/Resources/Data/GameData.txt";

        FileInfo file = new FileInfo(filePath);
        StreamWriter sw = file.CreateText();

        string json = JsonMapper.ToJson(gameDataList);
        sw.WriteLine(json);
        sw.Close();
        sw.Dispose();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public void SaveNormalData<T>(List<T> dataList, string path)
    {
        string filePath = Application.dataPath + @"/Resources/" + path + ".txt";

        FileInfo file = new FileInfo(filePath);
        StreamWriter sw = file.CreateText();

        string json = JsonMapper.ToJson(dataList);
        sw.WriteLine(json);
        sw.Close();
        sw.Dispose();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public void SaveDicData<T>(Dictionary<string, T> data, string path)
    {
        string filePath = Application.dataPath + @"/Resources/" + path + ".txt";

        FileInfo file = new FileInfo(filePath);
        StreamWriter sw = file.CreateText();

        string json = JsonMapper.ToJson(data);
        sw.WriteLine(json);
        sw.Close();
        sw.Dispose();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}