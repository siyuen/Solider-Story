using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class ItemManager : QSingleton<ItemManager> {

    public static int ITEMENNU = 0;
    public static int WEAPONMENU = 1;

    public static Color COLOR_INSTALL = new Color(0, 168, 0);
    public static Color COLOR_WEPONDATA = new Color(0.47f, 0.83f, 1);
    public static Color COLOR_USEITEM = new Color(255, 255, 255);
    public static Color COLOR_CANNOTUSE = new Color(0.65f, 0.65f, 0.65f);
    
    //当前item / weapon
    public ItemData curItem;
    public WeaponData curWeapon;

    //武器
    public Dictionary<string, WeaponData> weaponDic;
    public Dictionary<string, WeaponData> keyWeaponDic = new Dictionary<string,WeaponData>();
    public List<WeaponData> weaponList;
    //物品
    public Dictionary<string, ItemData> itemDic;
    public Dictionary<string, ItemData> keyItemDic = new Dictionary<string,ItemData>();
    public List<ItemData> itemList;
    //显示的菜单
    public int curMenu;
    //给予item唯一的tag做标识
    private int tag;

    private ItemManager()
    {
        tag = 0;
        itemDic = DataManager.Load<ItemData>("Data/ItemData");
        for (int i = 0; i < itemDic.Count; i++)
        {
            keyItemDic.Add(itemDic[i.ToString()].name, itemDic[i.ToString()]);
        }

        weaponDic = DataManager.Load<WeaponData>("Data/WeponData");
        for (int i = 0; i < weaponDic.Count; i++)
        {
            keyWeaponDic.Add(weaponDic[i.ToString()].name, weaponDic[i.ToString()]);
        }
    }

    public void Init(int menu)
    {
        curMenu = menu;
        UIManager.Instance().ShowUIForms("ItemMenu");
    }

    /// <summary>
    /// 克隆一个武器
    /// </summary>
    public WeaponData CloneWeapon(int id)
    {
        tag++;
        WeaponData parent = weaponDic[id.ToString()];
        WeaponData weapon = new WeaponData();
        weapon.key = parent.key;
        weapon.name = parent.name;
        weapon.type = parent.type;
        weapon.attack = parent.attack;
        weapon.hit = parent.hit;
        weapon.critical = parent.critical;
        weapon.level = parent.level;
        weapon.range = parent.range;
        weapon.weight = parent.weight;
        weapon.durability = parent.durability;
        weapon.sprite = parent.sprite;
        weapon.logo = parent.logo;
        weapon.tag = tag.ToString();
        return weapon;
    }

    public WeaponData CloneWeapon(string key)
    {
        WeaponData parent = keyWeaponDic[key];
        WeaponData weapon = new WeaponData();
        weapon.key = parent.key;
        weapon.name = parent.name;
        weapon.type = parent.type;
        weapon.attack = parent.attack;
        weapon.hit = parent.hit;
        weapon.critical = parent.critical;
        weapon.level = parent.level;
        weapon.range = parent.range;
        weapon.weight = parent.weight;
        weapon.durability = parent.durability;
        weapon.sprite = parent.sprite;
        weapon.logo = parent.logo;
        tag++;
        weapon.tag = tag.ToString();
        return weapon;
    }

    /// <summary>
    /// 克隆item
    /// </summary>
    public ItemData CloneItem(int id)
    {
        ItemData parent = itemDic[id.ToString()];
        ItemData item = new ItemData();
        item.name = parent.name;
        item.durability = parent.durability;
        item.tips = parent.tips;
        item.sprite = parent.sprite;
        tag++;
        item.tag = tag.ToString();
        return item;
    }

    public ItemData CloneItem(string key)
    {
        ItemData parent = keyItemDic[key];
        ItemData item = new ItemData();
        item.name = parent.name;
        item.durability = parent.durability;
        item.tips = parent.tips;
        item.sprite = parent.sprite;
        tag++;
        item.tag = tag.ToString();
        return item;
    }

    public void Clear()
    {
        curWeapon = null;
        curItem = null;
    }

    /// <summary>
    /// 获取当前tag
    /// </summary>
    public int GetTag()
    {
        return tag;
    }

    public void SetTag(int tag)
    {
        this.tag = tag;
    }

    /// <summary>
    /// 判断道具是否能用
    /// </summary>
    public bool CanUse(string key)
    {
        HeroController hero = MainManager.Instance().curHero;
        if (key == "伤药")
        {
            if (hero.rolePro.tHp == hero.rolePro.cHp)
                return false;
            else
                return true;
        }
        return false;
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    public void UseItem(int idx)
    {
        MainManager.Instance().mainState = MainManager.MainState.UseItem;
        if (curItem == null)
            return;
        UIManager.Instance().ShowUIForms("UseItem");
        HeroController hero = MainManager.Instance().curHero;
        if (curItem.name == "伤药")
        {
            if (!CanUse(curItem.name))
                return;
            hero.rolePro.SetProValue(RolePro.PRO_CHP, hero.rolePro.cHp + 10);
            if (hero.rolePro.cHp > hero.rolePro.tHp)
                hero.rolePro.SetProValue(RolePro.PRO_THP, hero.rolePro.tHp);
            UIManager.Instance().GetUI("UseItem").GetComponent<UseItemView>().UpdateUI("hp");
        }
        int dur = DataManager.Value(curItem.durability);
        dur -= 1;
        if (dur == 0)
        {
            hero.GiveUpItem(itemList[idx].tag);
        }
        else
            curItem.durability = dur.ToString();
    }

    /// <summary>
    /// 判断是否是武器
    /// </summary>
    public bool IsWeapon(string key)
    {
        if (keyWeaponDic.ContainsKey(key))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 道具交换
    /// </summary>
    public void ChangeItem(HeroController hero1, string tag1, HeroController hero2, string tag2)
    {
        //背包中都是Itemdata直接互换
        int idx1 = -1;
        int idx2 = -1;
        for (int i = 0; i < hero1.bagList.Count; i++)
        {
            if (hero1.bagList[i].tag == tag1)
            {
                idx1 = i;
                break;
            }
        }
        for (int i = 0; i < hero2.bagList.Count; i++)
        {
            if (hero2.bagList[i].tag == tag2)
            {
                idx2 = i;
                break;
            }
        }
        if (idx1 == -1 || idx2 == -1)
            return;
        //交换bag中的
        ItemData item = hero1.bagList[idx1];
        hero1.bagList[idx1] = hero2.bagList[idx2];
        hero2.bagList[idx2] = item;
        //武器道具列表有区分
        bool weapon1 = false;
        for (int i = 0; i < hero1.weaponList.Count; i++)
        {
            if (hero1.weaponList[i].tag == tag1)
            {
                weapon1 = true;
                hero2.AddItem(hero1.weaponList[i], false);
                hero1.GiveUpItem(tag1, false);
                break;
            }
        }
        if (!weapon1)
        {
            //道具交换
            for (int i = 0; i < hero1.itemList.Count; i++)
            {
                if (hero1.itemList[i].tag == tag1)
                {
                    hero2.AddItem(hero1.itemList[i], false);
                    hero1.GiveUpItem(tag1, false);
                    break;
                }
            }
        }
        bool weapon2 = false;
        for (int i = 0; i < hero2.weaponList.Count; i++)
        {
            if (hero2.weaponList[i].tag == tag2)
            {
                weapon2 = true;
                hero1.AddItem(hero2.weaponList[i], false);
                hero2.GiveUpItem(tag2, false);
                break;
            }
        }
        if (!weapon2)
        {
            for (int i = 0; i < hero2.itemList.Count; i++)
            {
                if (hero2.itemList[i].tag == tag2)
                {
                    hero1.AddItem(hero2.itemList[i], false);
                    hero2.GiveUpItem(tag2, false);
                    break;
                }
            }
        }
    }
}
