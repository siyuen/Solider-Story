using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using QFramework;

/// <summary>
/// Hero的选项功能
/// </summary>
public class HeroOption : QSingleton<HeroOption>{

    //hero选项
    public Dictionary<string, HeroOptionData> optionDic;
    public Dictionary<string, HeroOptionData> keyOptionDic = new Dictionary<string, HeroOptionData>();
    //方法
    public Dictionary<string, MenuView.NormalFunc> funcDic = new Dictionary<string, MenuView.NormalFunc>();

    private HeroOption()
    {
        optionDic = DataManager.Load<HeroOptionData>("Data/HeroOptionData");
        for (int i = 0; i < optionDic.Count; i++)
        {
            keyOptionDic.Add(optionDic[i.ToString()].name, optionDic[i.ToString()]);
        }
        funcDic.Add("攻击", Attack);
        funcDic.Add("占领", Occupy);
        funcDic.Add("物品", Item);
        funcDic.Add("交换", Change);
        funcDic.Add("待机", Standby);
    }

    #region hero选项
    /// <summary>
    /// 攻击
    /// </summary>
    private void Attack()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        ItemManager.Instance().Init(ItemManager.WEAPONMENU);
    }

    /// <summary>
    /// 光标在attack选项时的显示
    /// </summary>
    public void MoveToAttack()
    {
        MoveManager.Instance().ShowAttackRange();
    }

    /// <summary>
    /// 占领
    /// </summary>
    private void Occupy()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        GameManager.Instance().GameOver(GameManager.SUCCESS);
    }

    /// <summary>
    /// 道具
    /// </summary>
    private void Item()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        ItemManager.Instance().Init(ItemManager.ITEMENNU);
    }

    /// <summary>
    /// 交换
    /// </summary>
    private void Change()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        UIManager.Instance().ShowUIForms("ChangeItem");
    }

    /// <summary>
    /// 待机
    /// </summary>
    private void Standby()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        MainManager.Instance().curHero.Standby();
    }

    /// <summary>
    /// 其它选项
    /// </summary>
    public void MoveToAnother()
    {
        MoveManager.Instance().HideAttackRange();
    }
    #endregion
}
