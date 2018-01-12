using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;

public class HeroMenu : UIBase
{
    public MenuView menuView;

    private HeroProperty.HeroOption heroOption;

    //选项button

    private struct OptionButton
    {
        public OptionButton(int i, GameObject b, HeroProperty.HeroOptions o, HeroProperty.normalFunc f, HeroProperty.normalFunc m)
        {
            idx = i;
            btn = b;
            option = o;
            func = f;
            mfunc = m;
        }
        public int idx;
        public GameObject btn;
        public HeroProperty.HeroOptions option;
        //点击func
        public HeroProperty.normalFunc func;
        //移动到的func
        public HeroProperty.normalFunc mfunc;
    }

    //存选项btn
    private List<OptionButton> optionButton = new List<OptionButton>();

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        heroOption = HeroProperty.HeroOption.Instance();
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

    private void Init()
    {
        //待机为默认
        HeroProperty.HeroOptions option = HeroProperty.HeroOptions.Standby;
        AddItem(option, Standby, MoveToAnother);
        //检测敌人
        int id = MainManager.Instance().curHero.CheckIsEnemyAround();
        if (id != -1)
        {
            option = HeroProperty.HeroOptions.Attack;
            AddItem(option, Attack, MoveToAttack);
        }
        //检测物品
        option = HeroProperty.HeroOptions.Item;
        AddItem(option, Item, MoveToAnother);
        //排序添加进menuView
        SortFunc();
        menuView.cancleFunc = OnCancle;
        menuView.DisplayInit();
    }

    /// <summary>
    /// 清理
    /// </summary>
    private void Clear()
    {
        optionButton.Clear();
    }

    /// <summary>
    /// 添加选项进content
    /// </summary>
    private void AddItem(HeroProperty.HeroOptions option, HeroProperty.normalFunc func, HeroProperty.normalFunc m)
    {
        GameObject btn = GameObjectPool.Instance().GetPool(MainProperty.BUTTON_PATH, Vector3.zero);
        //设置
        Text txt = btn.GetComponentInChildren<Text>();
        string str;
        heroOption.option2Str.TryGetValue(option, out str);
        txt.text = str;
        btn.name = option.ToString();
        optionButton.Add(new OptionButton(heroOption.optionValue[option], btn, option, func, m));
    }

    /// <summary>
    /// 将optionButton排序
    /// </summary>
    private void SortFunc()
    {
        if (optionButton.Count == 0)
            return;
        for (int i = 0; i < optionButton.Count - 1; i++)
        {
            for (int j = i + 1; j < optionButton.Count; j++)
            {
                if (optionButton[i].idx > optionButton[j].idx)
                {
                    OptionButton option = optionButton[i];
                    optionButton[i] = optionButton[j];
                    optionButton[j] = option;
                }
            }
        }
        //添加到menu中
        for(int i=0;i<optionButton.Count;i++)
        {
            menuView.AddItem(MainProperty.BUTTON_PATH, optionButton[i].btn, optionButton[i].func, optionButton[i].mfunc);
        }
    }

    #region 选项功能
    /// <summary>
    /// 待机
    /// </summary>
    private void Standby()
    {
        menuView.Hide();
        MainManager.Instance().curHero.Standby();
    }

    private void Attack()
    {
        menuView.Hide();
        UIManager.Instance().CloseUIForms("HeroMenu");
        OpenUIForm("WeaponSelectMenu");
    }

    /// <summary>
    /// 光标在attack选项时的显示
    /// </summary>
    private void MoveToAttack()
    {
        MoveManager.Instance().ShowAttackRange();
    }

    /// <summary>
    /// 其它选项
    /// </summary>
    private void MoveToAnother()
    {
        MoveManager.Instance().HideAttackRange();
    }

    private void Item()
    {
        Debug.Log("物品");
    }

    private void OnCancle()
    {
        MainManager.Instance().curHero.CancelMoveDone();
    }
    #endregion
}
