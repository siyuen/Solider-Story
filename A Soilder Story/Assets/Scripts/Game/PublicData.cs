using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;  
using System.Text;
using QFramework;

#if UNITY_EDITOR
using UnityEditor;
#endif  

#region 人物
//人物
public class PublicRoleData
{
    public string id;
    public string name;
    public string career;
    public string level;
    public string exp;
    public string chp;
    public string thp;
    
    public string power;
    public string skill;
    public string speed;
    public string lucky;
    public string pdefense;
    public string mdefense;
    public string move;
    public string strength;

    public string simage;
    public string limage;
    public string prefab;
    public string fightprefab;
}

public class HeroData
{
    public string id;
    public string name;
    public string key;
}


/// <summary>
/// hero属性
/// </summary>
public class HeroProData : PublicRoleData
{
    public string item;
    public string weapon;
}

public class HeroItemData
{
    public string id;
    public string weapon;
    public string item;
}

public class TemporaryHeroData
{
    public string hero;
    public string pos;
    public string state;
    public string item;
    public string weapon;
}

/// <summary>
/// enemy
/// </summary>
public class EnemyProData : PublicRoleData
{
}

public class TemporaryEnemyData
{
    public string enemy;
    public string pos;
    public string item;
    public string weapon;
}


#endregion

#region 地形
/// <summary>
/// 地形
/// </summary>
public class LandData
{
    public string key;
    public string name;
    public string value;
    public string def;
    public string avo;
    public string func;
    public string life;
    public string name2;
}
#endregion

#region 物品
public class WeaponData:ItemData
{
    public string key;
    public string type;
    public string attack;
    public string hit;
    public string critical;
    public string level;
    public string range;
    public string weight;
    public string logo;
}

public class ItemData
{
    public string id;
    public string name;
    public string durability;
    public string tips;
    public string sprite;
    public string tag;
}

#endregion

#region 职业
public class CareerData
{
    public string id;
    public string key;
    public string name;
    public string weaponkey1;
    public string weaponkey2;

    //成长
    public string hp;
    public string power;
    public string skill;
    public string speed;
    public string lucky;
    public string pdefense;
    public string mdefense;

    //上限
    public string limithp;
    public string limitpower;
    public string limitskill;
    public string limitspeed;
    public string limitpdefense;
    public string limitmdefense;
    public string limitstrength;
}

#endregion

public class LevelData
{
    public string id;
    public string name;
    public string hero;
    public string heropos;
    public string enemy;
    public string enemypos;
    public string enemylevel;
    public string enemyweapon;
    public string boss;
    public string goal;
    public string goaltips;
}

/// <summary>
/// 记录特殊地形
/// </summary>
public class TemporaryLevelData
{
    public string id;
    public string key;
    public string life;
}

/// <summary>
/// 记录存档，当前关卡，是否正在进行
/// </summary>
public class GameData
{
    public string id;
    public string level;
    public string curplay;
    public string itemtag;
}

public class HeroOptionData
{
    public string id;
    public string key;
    public string name;
    public string value;
}