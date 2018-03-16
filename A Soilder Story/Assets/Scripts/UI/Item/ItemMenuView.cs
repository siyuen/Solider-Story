using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

public class ItemMenuView : UIBase {

    public Image heroImage;
    public GameObject weaponData;
    public Text itemTips;

    public Image weaponImg;
    public Text attackText;
    public Text hitText;
    public Text crtText;
    public Text missText;

    public MenuView menuView;
    public int itemIdx;
    //武器道具分开
    private List<WeaponData> weaponList = new List<WeaponData>();
    private List<ItemData> itemList = new List<ItemData>();

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        weaponData.SetActive(false);
        itemTips.gameObject.SetActive(false);
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

    private void Clear()
    {
        menuView.Hide();
        EffectManager.Instance().Clear();
        ItemManager.Instance().Clear();
    }

    /// <summary>
    /// 初始化,检测武器
    /// </summary>
    private void Init()
    {
        itemIdx = 0;
        //设置人物头像,有武器时设置武器类型图标
        heroImage.sprite = ResourcesMgr.Instance().LoadSprite(MainManager.Instance().curHero.rolePro.lImage);
        if (ItemManager.Instance().curMenu == ItemManager.ITEMENNU)
            AddItem();
        else if (ItemManager.Instance().curMenu == ItemManager.WEAPONMENU)
            AddWeapon();

        menuView.SetCursorActive(true);
        menuView.cancleFunc = OnCancel;
        menuView.bAnim = true;
        menuView.DisplayInit();
    }

    /// <summary>
    /// 添加物品（包括武器）
    /// </summary>
    private void AddItem()
    {
        //需要计算顺序
        weaponList = MainManager.Instance().curHero.weaponList;
        for (int i = 0; i < weaponList.Count; i++)
        {
            GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.ITEM_PATH);
            btn.GetComponent<Image>().sprite = ResourcesMgr.Instance().LoadSprite(weaponList[i].sprite);
            Text name = btn.transform.Find("Name").GetComponent<Text>();
            name.text = weaponList[i].name;
            Text count = btn.transform.Find("Count").GetComponent<Text>();
            count.text = weaponList[i].durability;
            if (!MainManager.Instance().curHero.WeaponMatching(weaponList[i]))
            {
                name.color = ItemManager.COLOR_CANNOTUSE;
                count.color = ItemManager.COLOR_CANNOTUSE;
            }
            else
            {
                name.color = ItemManager.COLOR_USEITEM;
                count.color = ItemManager.COLOR_WEPONDATA;
            }
            menuView.AddItem(MainProperty.ITEM_PATH, btn, UseItem, UpdateData);
        }

        itemList = MainManager.Instance().curHero.itemList;
        for (int i = 0; i < itemList.Count; i++)
        {
            GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.ITEM_PATH);
            btn.GetComponent<Image>().sprite = ResourcesMgr.Instance().LoadSprite(itemList[i].sprite);
            Text name = btn.transform.Find("Name").GetComponent<Text>();
            name.text = itemList[i].name;
            Text count = btn.transform.Find("Count").GetComponent<Text>();
            count.text = itemList[i].durability;
            if (!ItemManager.Instance().CanUse(itemList[i].name))
            {
                name.color = ItemManager.COLOR_CANNOTUSE;
                count.color = ItemManager.COLOR_CANNOTUSE;
            }
            else
            {
                name.color = ItemManager.COLOR_USEITEM;
                count.color = ItemManager.COLOR_WEPONDATA;
            }
            menuView.AddItem(MainProperty.ITEM_PATH, btn, UseItem, UpdateData);
        }
    }

    /// <summary>
    /// 添加武器
    /// </summary>
    private void AddWeapon()
    {
        weaponList = MainManager.Instance().curHero.weaponList;
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (MainManager.Instance().curHero.WeaponMatching(weaponList[i]))
            {
                GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.ITEM_PATH);
                btn.GetComponent<Image>().sprite = ResourcesMgr.Instance().LoadSprite(weaponList[i].sprite);
                Text name = btn.transform.Find("Name").GetComponent<Text>();
                name.text = weaponList[i].name;
                name.color = ItemManager.COLOR_USEITEM;
                Text count = btn.transform.Find("Count").GetComponent<Text>();
                count.color = ItemManager.COLOR_WEPONDATA;
                count.text = weaponList[i].durability;
                menuView.AddItem(MainProperty.ITEM_PATH, btn, UseWeapon, UpdateData);
            }
        }
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    public void UpdateData()
    {
        EffectManager.Instance().Clear();
        int idx = menuView.cursorIdx;
        if (idx < weaponList.Count)
        {
            HeroController hero = MainManager.Instance().curHero;
            weaponData.SetActive(true);
            itemTips.gameObject.SetActive(false);
            //攻击
            int weaponAttack = DataManager.GetAttack(hero, weaponList[idx]);
            attackText.text = weaponAttack.ToString();
            //命中
            int weaponHit = DataManager.GetHit(hero, weaponList[idx]);
            hitText.text = weaponHit.ToString();
            //必杀
            int weaponCrt = DataManager.GetCrt(hero, weaponList[idx]);
            crtText.text = weaponCrt.ToString();
            //回避
            int weaponMiss = DataManager.GetMiss(hero, weaponList[idx]);
            missText.text = weaponMiss.ToString();
            //判断是否能用
            if (!MainManager.Instance().curHero.WeaponMatching(weaponList[idx]))
            {
                attackText.color = ItemManager.COLOR_CANNOTUSE;
                hitText.color = ItemManager.COLOR_CANNOTUSE;
                crtText.color = ItemManager.COLOR_CANNOTUSE;
                missText.color = ItemManager.COLOR_CANNOTUSE;
            }
            else
            {
                attackText.color = ItemManager.COLOR_WEPONDATA;
                hitText.color = ItemManager.COLOR_WEPONDATA;
                crtText.color = ItemManager.COLOR_WEPONDATA;
                missText.color = ItemManager.COLOR_WEPONDATA;
            }
            weaponImg.sprite = ResourcesMgr.Instance().LoadSprite(weaponList[idx].logo);

            if (hero.curWeapon != null)
            {
                //与当前武器相比
                int curAttack = DataManager.GetAttack(hero, hero.curWeapon);
                int curHit = DataManager.GetHit(hero, hero.curWeapon);
                int curCrt = DataManager.GetCrt(hero, hero.curWeapon);
                int curMiss = DataManager.GetMiss(hero, hero.curWeapon);

                Vector3 pos = new Vector3(attackText.rectTransform.sizeDelta.x / 2 + 10, 0, 0);
                if (weaponAttack > curAttack)
                    SetEffect(MainProperty.EFFECT_UP, pos, attackText.rectTransform);
                else if (weaponAttack < curAttack)
                    SetEffect(MainProperty.EFFECT_DOWN, pos, attackText.rectTransform);

                pos = new Vector3(hitText.rectTransform.sizeDelta.x / 2 + 10, 0, 0);
                if (weaponHit > curHit)
                    SetEffect(MainProperty.EFFECT_UP, pos, hitText.rectTransform);
                else if (weaponHit < curHit)
                    SetEffect(MainProperty.EFFECT_DOWN, pos, hitText.rectTransform);

                pos = new Vector3(crtText.rectTransform.sizeDelta.x / 2 + 10, 0, 0);
                if (weaponCrt > curCrt)
                    SetEffect(MainProperty.EFFECT_UP, pos, crtText.rectTransform);
                else if (weaponCrt < curCrt)
                    SetEffect(MainProperty.EFFECT_DOWN, pos, crtText.rectTransform);

                pos = new Vector3(missText.rectTransform.sizeDelta.x / 2 + 10, 0, 0);
                if (weaponMiss > curMiss)
                    SetEffect(MainProperty.EFFECT_UP, pos, missText.rectTransform);
                else if (weaponMiss < curMiss)
                    SetEffect(MainProperty.EFFECT_DOWN, pos, missText.rectTransform);
            }
        }
        else if(weaponList.Count <= idx && idx < weaponList.Count + itemList.Count)
        {
            weaponData.SetActive(false);
            itemTips.gameObject.SetActive(true);
            itemTips.text = itemList[idx - weaponList.Count].tips;
        }
    }

    /// <summary>
    /// 使用道具或者装备武器后更新view,需要判断还有没有道具
    /// </summary>
    public void UpdateItemView()
    {
        if (MainManager.Instance().curHero.bagList.Count != 0)
        {
            Clear();
            Init();
        }
        else
        {
            UIManager.Instance().CloseUIForms("ItemMenu");
            OpenUIForm("HeroMenu");
        }
    }

    /// <summary>
    /// 设置特效:上升 & 下降
    /// </summary>
    private void SetEffect(string path, Vector3 pos, RectTransform rect)
    {
        GameObject effect = EffectManager.Instance().AddEffect(rect, path, pos);
        float distance = 0.1f;
        if (path == MainProperty.EFFECT_DOWN)
            distance = -distance;
        Tweener tw = effect.transform.DOMove(effect.transform.position + new Vector3(0, 0.1f, 0), 0.5f);
        tw.SetLoops(-1);
        //EffectManager.Instance().SetEffectMove(path, pos + new Vector3(0, distance, 0), 0.5f, -1);
    }

    /// <summary>
    /// 选项回退
    /// </summary>
    public void CancleOption()
    {
        menuView.RegisterKeyBoardEvent();
        menuView.SetCursorActive(true);
        menuView.cancleFunc = OnCancel;
        ItemManager.Instance().curWeapon = null;
        ItemManager.Instance().curItem = null;
    }

    private void UseWeapon()
    {
        ItemManager.Instance().curWeapon = weaponList[menuView.cursorIdx];
        MainManager.Instance().curHero.SetCurWeapon(menuView.cursorIdx);
        UIManager.Instance().CloseUIForms("ItemMenu");
        OpenUIForm("FightData");
    }

    /// <summary>
    /// 点击道具显示选项
    /// </summary>
    private void UseItem()
    {
        menuView.UnRegisterKeyBoardEvent();
        menuView.SetCursorActive(false);
        //设置当前item的信息
        //根据idx判断item/weapon
        int curWeaponCount = weaponList.Count;
        int curItemCount = itemList.Count;
        int curIdx = menuView.cursorIdx;

        if (curIdx < weaponList.Count)
        {
            ItemManager.Instance().curWeapon = weaponList[curIdx];
        }
        else
        {
            ItemManager.Instance().curItem = itemList[curIdx - curWeaponCount];
            itemIdx = curIdx - curWeaponCount;
        }
        Vector3 pos = new Vector3(
               menuView.menuBg.rectTransform.sizeDelta.x / 2,
               -curIdx * menuView.menuRectTransform["Bg"].size.y,
               0);
        UIManager.Instance().ShowUIForms("UseOption");
        UseOptionView view = UIManager.Instance().GetUI("UseOption").GetComponent<UseOptionView>();
        view.SetLocalPos(menuView.transform.localPosition + pos);
    }

    private void OnCancel()
    {
        MoveManager.Instance().HideAttackRange();
        UIManager.Instance().CloseUIForms("ItemMenu");
        OpenUIForm("HeroMenu");
    }
}
