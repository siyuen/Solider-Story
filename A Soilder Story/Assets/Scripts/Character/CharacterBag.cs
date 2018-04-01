using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBag {

    //背包最大容量
    public const int BAGLIMIT = 5;
    //当前装备的武器
    public WeaponData curWeapon;
    //武器道具分开统计
    public List<ItemData> bagList = new List<ItemData>();
    public List<WeaponData> weaponList = new List<WeaponData>();
    public List<ItemData> itemList = new List<ItemData>();
    //人物属性
    private RolePro rolePro;

    public CharacterBag(RolePro pro)
    {
        rolePro = pro;
    }

    public void ClearBag()
    {
        bagList.Clear();
        weaponList.Clear();
        itemList.Clear();
    }

    #region 武器相关方法
    /// <summary>
    /// 设置当前装备的武器
    /// </summary>
    public virtual void SetCurWeapon()
    {
        if (weaponList.Count == 0)
        {
            curWeapon = null;
            return;
        }
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (WeaponMatching(weaponList[i]))
            {
                curWeapon = weaponList[i];
                return;
            }
        }
    }

    public virtual void SetCurWeapon(int idx)
    {
        if (idx >= weaponList.Count || idx < 0)
            return;
        if (!WeaponMatching(weaponList[idx]))
            return;
        if (curWeapon.tag == weaponList[idx].tag)
            return;
        WeaponData weapon = weaponList[idx];
        if (weapon.tag == curWeapon.tag)
            return;
        curWeapon = null;
        GiveUpItem(weaponList[idx].tag);
        bagList.Insert(0, weapon);
        weaponList.Insert(0, weapon);
        curWeapon = weaponList[0];
    }

    public virtual void SetCurWeapon(string tag)
    {
        if (curWeapon != null && curWeapon.tag == tag)
            return;
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i].tag == tag && WeaponMatching(weaponList[i]))
            {
                //设为当前武器，并且将当前武器放置第一位
                WeaponData weapon = weaponList[i];
                //从bag跟wepaon中清除
                GiveUpItem(tag);
                //插入第一位
                bagList.Insert(0, weapon);
                weaponList.Insert(0, weapon);
                curWeapon = weaponList[0];
                return;
            }
        }
        curWeapon = null;
    }

    /// <summary>
    /// 更新武器，用于交换过后或者武器损坏
    /// </summary>
    public virtual void CurWeaponUpdate()
    {
        if (weaponList.Count == 0)
            curWeapon = null;
        else
        {
            //当前武器不为null，需要判断是否还存在这个武器,不存在就更新列表看看是否有匹配的武器
            if (curWeapon != null)
            {
                bool have = false;
                for (int i = 0; i < weaponList.Count; i++)
                {
                    if (weaponList[i].tag == curWeapon.tag)
                        have = true;
                }
                if (have)
                    return;
                else
                {
                    for (int i = 0; i < weaponList.Count; i++)
                    {
                        if (WeaponMatching(weaponList[i]))
                        {
                            curWeapon = weaponList[i];
                            return;
                        }
                    }
                    curWeapon = null;
                }
            }
            //当前武器为null则直接更新列表看看是否有匹配的武器
            else
            {
                for (int i = 0; i < weaponList.Count; i++)
                {
                    if (WeaponMatching(weaponList[i]))
                    {
                        curWeapon = weaponList[i];
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新当前武器（用于交换时）
    /// </summary>
    public void ChangeingUpdate()
    {
        if (weaponList.Count == 0)
            curWeapon = null;
        else
        {
            for (int i = 0; i < weaponList.Count; i++)
            {
                if (WeaponMatching(weaponList[i]))
                {
                    curWeapon = weaponList[i];
                    return;
                }
            }
            curWeapon = null;
        }
    }

    /// <summary>
    /// 判断能否装备这个武器
    /// </summary>
    public virtual bool WeaponMatching(WeaponData weapon)
    {
        return CareerManager.Instance().WeaponMatching(rolePro.mCareer, weapon.key);
    }

    public virtual bool WeaponMatching(string tag)
    {
        int idx = -1;
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i].tag == tag)
            {
                idx = i;
                break;
            }
        }
        if (idx == -1)
            return false;
        WeaponData weapon = weaponList[idx];
        string weapon1 = CareerManager.Instance().keyCareerDic[rolePro.mCareer].weaponkey1;
        string weapon2 = CareerManager.Instance().keyCareerDic[rolePro.mCareer].weaponkey2;
        if (weapon.key == weapon1 || weapon.key == weapon2)
            return true;
        else
            return false;
    }

    #endregion

    #region 道具相关方法
    /// <summary>
    /// 增加物品,默认添加到背包
    /// </summary>
    public virtual void AddItem(ItemData item, bool bag = true)
    {
        if (bagList.Count < BAGLIMIT)
        {
            if (bag)
                bagList.Add(item);
            itemList.Add(item);
        }
    }

    public virtual void AddItem(WeaponData item, bool bag = true)
    {
        if (bagList.Count < BAGLIMIT)
        {
            if (bag)
                bagList.Add(item);
            weaponList.Add(item);
        }
    }

    /// <summary>
    /// 销毁item,默认清理背包中的
    /// </summary>
    public virtual void GiveUpItem(string tag, bool bag = true)
    {
        if (bag)
        {
            for (int i = 0; i < bagList.Count; i++)
            {
                if (tag == bagList[i].tag)
                    bagList.RemoveAt(i);
            }
        }
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (tag == weaponList[i].tag)
            {
                weaponList.RemoveAt(i);
                if (curWeapon != null)
                {
                    if (tag == curWeapon.tag)
                        ChangeingUpdate();
                }
                return;
            }
        }
        for (int i = 0; i < itemList.Count; i++)
        {
            if (tag == itemList[i].tag)
            {
                itemList.RemoveAt(i);
                return;
            }
        }
    } 
    #endregion
}
