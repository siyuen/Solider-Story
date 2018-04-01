using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;

public class HeroMenu : UIBase
{
    public MenuView menuView;
    private List<GameObject> optionList = new List<GameObject>();

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
    }

    public override void Display()
    {
        base.Display();
        Init();
    }

    public override void Hiding()
    {
        base.Hiding();
        Clear();
    }

    private void Init()
    {
        MainManager main = MainManager.Instance();
        HeroController hero = main.curHero;
        AddOption("待机");
        if ((hero.CheckIsEnemyAround() != -1 || hero.CheckIsCrackAround()) && hero.curWeapon != null)
            AddOption("攻击");
        if (hero.bagList.Count > 0)
            AddOption("物品");
        if (hero.rolePro.mName == HeroManager.LEADER && LevelManager.Instance().GetMapNode(hero.mIdx).mName == "大门")
            AddOption("占领");
        if (hero.CheckIsHeroAround() != -1 && !hero.bChangeItem)
        {
            List<int> list = hero.CheckHero();
            bool b = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (LevelManager.Instance().GetMapNode(list[i]).locatedHero.bagList.Count != 0)
                {
                    b = true;
                    break;
                }
            }
            if (b)
                AddOption("交换");
            else
            {
                if (hero.bagList.Count > 0)
                    AddOption("交换");
            }
        }
        //交换过的更新一下当前武器列表
        if (hero.bChangeItem)
            hero.CurWeaponUpdate();
        AddInMenu();

        menuView.cancleFunc = OnCancle;
        menuView.bAnim = true;
        menuView.DisplayInit();
    }

    private void AddOption(string name)
    {
        GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.BUTTON_PATH);
        Text txt = btn.GetComponentInChildren<Text>();
        txt.text = name;
        if (name == "交换")
            txt.color = ItemManager.COLOR_INSTALL;
        else
            txt.color = ItemManager.COLOR_USEITEM;
        btn.name = name;
        optionList.Add(btn);
    }

    private void AddInMenu()
    {
        for (int i = 0; i < optionList.Count - 1; i++)
        {
            for (int j = i + 1; j < optionList.Count; j++)
            {
                int value = DataManager.Value(HeroOption.Instance().keyOptionDic[optionList[i].name].value);
                int value1 = DataManager.Value(HeroOption.Instance().keyOptionDic[optionList[j].name].value);
                if (value > value1)
                {
                    GameObject btn = optionList[j];
                    optionList[j] = optionList[i];
                    optionList[i] = btn;
                }
            }
        }
        for (int i = 0; i < optionList.Count; i++)
        {
            string name = optionList[i].name;
            if (name == "攻击")
                menuView.AddItem(MainProperty.BUTTON_PATH, optionList[i], HeroOption.Instance().funcDic[name], HeroOption.Instance().MoveToAttack);
            else
                menuView.AddItem(MainProperty.BUTTON_PATH, optionList[i], HeroOption.Instance().funcDic[name], HeroOption.Instance().MoveToAnother);
        }
    }

    /// <summary>
    /// 清理
    /// </summary>
    private void Clear()
    {
        menuView.Hide();
        optionList.Clear();
    }

    private void OnCancle()
    {
        if (!MainManager.Instance().curHero.bChangeItem)
        {
            UIManager.Instance().CloseUIForms("HeroMenu");
            MainManager.Instance().curHero.CancelMoveDone();
            MainManager.Instance().RegisterKeyBoardEvent();
        }
    }
}
