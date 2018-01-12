using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;

public class WeaponSelectView : UIBase {

    public Image weaponLogoImg;
    public Text attackText;
    public Text mingZhongText;
    public Text biShaText;
    public Text missText;
    public MenuView menuView;

	void Awake () {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
	}

    public override void Display()
    {
        Init();
        base.Display();
    }

    public override void Hiding()
    {
        base.Hiding();
        Clear();
    }

    private void Clear()
    {
        menuView.Hide();
    }

    /// <summary>
    /// 初始化,检测武器
    /// </summary>
    private void Init()
    {
        //test
        AddItem();
        menuView.cancleFunc = OnCancel;
        menuView.DisplayInit();
    }

    /// <summary>
    /// 添加武器
    /// </summary>
    private void AddItem()
    {
        GameObject btn = GameObjectPool.Instance().GetPool(MainProperty.ITEM_PATH, Vector3.zero);
        //设置
        Text txt = btn.GetComponentInChildren<Text>();
        string str;
        menuView.AddItem(MainProperty.ITEM_PATH, btn, UseWeapon, null); 
    }

    private void UseWeapon()
    {
        UIManager.Instance().CloseUIForms("WeaponSelectMenu");
        OpenUIForm("FightData");
    }

    private void OnCancel()
    {
        UIManager.Instance().CloseUIForms("WeaponSelectMenu");
        OpenUIForm("HeroMenu");
    }
}
