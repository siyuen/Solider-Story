using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;

public class HeroMenuView : UIBase {
    public Image menuBg;
    public Image menuBottom;
    public GameObject uiContent;
    public Image optionCursor;
    public GameObject optionPrefab;
    private HeroProperty.HeroOption heroOption;
    private int countOptions;
    //计算所需增加的高度
    private float bgStartHeight;
    private Vector2 bgSize;
    private Vector2 bottomSize;
    //存选项btn
    private List<GameObject> optionButton = new List<GameObject>();
    private int optionIdx;
    //存方法
    private delegate void OptionFunc();
    //按照顺序存
    private Dictionary<int, OptionFunc> funcDic = new Dictionary<int, OptionFunc>();
    private int[] test = new int[10];


    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        heroOption = HeroProperty.HeroOption.Instance();
        InitMenu();
    }

    /// <summary>
    /// 初始化，判断要显示几种选项
    /// </summary>
    public void InitMenu()
    {
        optionButton.Clear();
        countOptions = 0;
        GetOptions();

        //计算背景显示
        bgSize = menuBg.rectTransform.sizeDelta;
        bottomSize = menuBg.rectTransform.sizeDelta;
        bgStartHeight = bgSize.y;
        bgSize.y = bgStartHeight * countOptions;
        bottomSize.y = bgSize.y - bgStartHeight;
        menuBg.rectTransform.sizeDelta = bgSize;
        menuBottom.rectTransform.position += new Vector3(0, -bottomSize.y, 0);
    }

    public override void Display()
    {
        base.Display();
        optionIdx = 0;
        RegisterKeyBoardEvent();
    }

    public override void Hiding()
    {
        base.Hiding();
        UnRegisterKeyBoardEvent();
        funcDic.Clear();
    }

    /// <summary>
    /// 添加选项进content
    /// </summary>
    private void AddItem(HeroProperty.HeroOptions option)
    {
        countOptions += 1;
        GameObject btn = Instantiate(optionPrefab) as GameObject;
        btn.SetActive(true);
        btn.transform.SetParent(uiContent.transform);
        btn.transform.SetSiblingIndex(heroOption.optionValue[option]);
        //设置
        Text txt = btn.GetComponentInChildren<Text>();
        string str;
        heroOption.option2Str.TryGetValue(option, out str);
        txt.text = str;
        btn.name = option.ToString();
        //RegisterButtonEnterEvent(option.ToString(), p => EnterButton(btn));
        optionButton.Add(btn);
    }

    /// <summary>
    /// 获取选项个数
    /// </summary>
    private void GetOptions()
    {
        heroOption.Init();
        HeroProperty.HeroOptions option = HeroProperty.HeroOptions.Standby;

        //这里目前要按照顺序添加才能实现,暂时没想到好的方法

        int a = 0;
        //检测敌人
        int id = MainManager.Instance().curHero.CheckIsEnemyAround();
        if (id != -1)
        {
            option = HeroProperty.HeroOptions.Attack;
            AddItem(option);
            //RegisterButtonObjectEvent(name, p => Attack());
            funcDic.Add(heroOption.optionValue[option], Attack);
            MainManager.Instance().ShowAttackRange();
            test[a] = heroOption.optionValue[option];
            a++;
        }
        //检测物品
        option = HeroProperty.HeroOptions.Item;
        AddItem(option);
        funcDic.Add(heroOption.optionValue[option], Item);
        test[a] = heroOption.optionValue[option];
        a++;
        Debug.Log(0);
        //待机为默认
        option = HeroProperty.HeroOptions.Standby;
        AddItem(option);
        //RegisterButtonObjectEvent(name, p => Standby());
        funcDic.Add(heroOption.optionValue[option], Standby);
        test[a] = heroOption.optionValue[option];
        a++;
        Debug.Log(1);
    }


    private void Sort()
    {
    }

    /// <summary>
    /// 注册键盘事件
    /// </summary>
    private void RegisterKeyBoardEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
    }

    private void UnRegisterKeyBoardEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
    }

    private void OnUpArrowDown()
    {
        optionIdx -= 1;
        if (optionIdx < 0)
        {
            optionIdx += countOptions;
            optionCursor.transform.position -= new Vector3(0, bgStartHeight / 100 * (countOptions - 1), 0);
        }
        else
        {
            optionCursor.transform.position += new Vector3(0, bgStartHeight / 100, 0);
        }
    }

    private void OnDownArrowDown()
    {
        optionIdx += 1;
        if (optionIdx >= countOptions)
        {
            optionIdx -= countOptions;
            optionCursor.transform.position += new Vector3(0, bgStartHeight / 100 * (countOptions - 1), 0);
        }
        else
        {
            optionCursor.transform.position -= new Vector3(0, bgStartHeight / 100, 0);
        }
    }

    private void OnConfirmDown()
    {
        funcDic[test[optionIdx]]();
    }

    /// <summary>
    /// 进入Button
    /// </summary>
    private void EnterButton(GameObject btn)
    {
        float y = optionCursor.transform.position.y - btn.transform.position.y;
        Vector3 pos = optionCursor.transform.position - new Vector3(0,y,0);
        optionCursor.transform.position = pos;
    }

    #region 选项功能
    /// <summary>
    /// 待机
    /// </summary>
    private void Standby()
    {
        MainManager.Instance().curHero.Standby();
    }

    private void Attack()
    {
        Debug.Log("攻击");
    }

    private void Item()
    {
        Debug.Log("物品");
    }
    #endregion
}
