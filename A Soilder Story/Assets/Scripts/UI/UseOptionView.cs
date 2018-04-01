using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UIFramework;

public class UseOptionView : UIBase
{
    public MenuView menuView;
    public GameObject tipsObj;
    public GameObject tipsObj2;

    //是否在显示tips
    private bool bTips;
    //选项
    private List<GameObject> optionList = new List<GameObject>();

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
    }

    public override void Display()
    {
        base.Display();
    }

    public override void Hiding()
    {
        base.Hiding();
        Clear();
    }

    public void Init()
    {
        tipsObj.SetActive(false);
        tipsObj2.SetActive(false);
        bTips = false;
        if (ItemManager.Instance().curWeapon == null && ItemManager.Instance().curItem == null)
            return;
        else if (ItemManager.Instance().curWeapon != null)
        {
            //武器：装备/丢弃
            GameObject option = ResourcesMgr.Instance().GetPool(MainProperty.USEOPTION_PATH);
            Text text = option.GetComponent<Text>();
            if (MainManager.Instance().curHero.WeaponMatching(ItemManager.Instance().curWeapon.tag))
                text.color = ItemManager.COLOR_INSTALL;
            else
                text.color = ItemManager.COLOR_CANNOTUSE;
            text.text = "装备";
            menuView.AddItem(MainProperty.USEOPTION_PATH, option, InstallWeapon, null);
            optionList.Add(option);

            GameObject option1 = ResourcesMgr.Instance().GetPool(MainProperty.USEOPTION_PATH);
            option1.GetComponent<Text>().color = ItemManager.COLOR_INSTALL;
            option1.GetComponent<Text>().text = "丢弃";
            menuView.AddItem(MainProperty.USEOPTION_PATH, option1, Abandon, null);
            optionList.Add(option1);
        }
        else if (ItemManager.Instance().curItem != null)
        {
            //道具：使用/丢弃
            GameObject option = ResourcesMgr.Instance().GetPool(MainProperty.USEOPTION_PATH);
            Text text = option.GetComponent<Text>();
            if (ItemManager.Instance().CanUse(ItemManager.Instance().curItem.name))
                text.color = ItemManager.COLOR_USEITEM;
            else
                text.color = ItemManager.COLOR_CANNOTUSE;
            text.text = "使用";
            menuView.AddItem(MainProperty.USEOPTION_PATH, option, UseItem, null);
            optionList.Add(option);

            GameObject option1 = ResourcesMgr.Instance().GetPool(MainProperty.USEOPTION_PATH);
            option1.GetComponent<Text>().color = ItemManager.COLOR_INSTALL;
            option1.GetComponent<Text>().text = "丢弃";
            menuView.AddItem(MainProperty.USEOPTION_PATH, option1, Abandon, null);
            optionList.Add(option1);
        }
        menuView.cancleFunc = OnCancle;
        menuView.bAnim = true;
        menuView.DisplayInit();
    }

    /// <summary>
    /// 设置pos,重置menuView
    /// </summary>
    public void SetLocalPos(Vector3 pos)
    {
        this.transform.localPosition = pos;
        Init();
    }

    public void Clear()
    {
        menuView.Hide();
        ResourcesMgr.Instance().PushPool(optionList, MainProperty.USEOPTION_PATH);
        optionList.Clear();
    }

    /// <summary>
    /// 装备
    /// </summary>
    private void InstallWeapon()
    {
        if (MainManager.Instance().curHero.WeaponMatching(ItemManager.Instance().curWeapon.tag))
        {
            UIManager.Instance().CloseUIForms("UseOption");
            ItemMenuView view = UIManager.Instance().GetUI("ItemMenu").GetComponent<ItemMenuView>();
            MainManager.Instance().curHero.SetCurWeapon(ItemManager.Instance().curWeapon.tag);
            view.UpdateItemView();
        }
        else
        {
            bTips = true;
            tipsObj2.SetActive(true);
        }
    }

    /// <summary>
    /// 丢弃
    /// </summary>
    private void Abandon()
    {
        if (ItemManager.Instance().curItem != null)
            MainManager.Instance().curHero.GiveUpItem(ItemManager.Instance().curItem.tag);
        else if(ItemManager.Instance().curWeapon != null)
            MainManager.Instance().curHero.GiveUpItem(ItemManager.Instance().curWeapon.tag);
        UIManager.Instance().CloseUIForms("UseOption");
        UIManager.Instance().GetUI("ItemMenu").GetComponent<ItemMenuView>().UpdateItemView();
    }

    /// <summary>
    /// 使用
    /// </summary>
    private void UseItem()
    {
        if (ItemManager.Instance().CanUse(ItemManager.Instance().curItem.name))
        {
            int idx = UIManager.Instance().GetUI("ItemMenu").GetComponent<ItemMenuView>().itemIdx;
            ItemManager.Instance().UseItem(idx);
            UIManager.Instance().CloseUIForms("UseOption");
            UIManager.Instance().CloseUIForms("ItemMenu");
        }
        else
        {
            bTips = true;
            tipsObj.SetActive(true);
        }
    }

    /// <summary>
    /// 回退
    /// </summary>
    private void OnCancle()
    {
        if (!bTips)
        {
            UIManager.Instance().CloseUIForms("UseOption");
            UIManager.Instance().GetUI("ItemMenu").GetComponent<ItemMenuView>().CancleOption();
        }
        else
        {
            bTips = false;
            tipsObj.SetActive(false);
            tipsObj2.SetActive(false);
        }
    }
}
