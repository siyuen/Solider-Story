using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;

public class HeroMenuView : UIBase {

    public Image menuBg;
    public Image menuBottom;
    public Image menuLeft;
    public Image menuRight;
    public GameObject uiContent;
    public Image optionCursor;
    private HeroProperty.HeroOption heroOption;
    private int countOptions;
    //记录cursor初始位置
    private Vector3 defaultCursorPos;
    //记录左右边界初始size跟pos
    private Vector3 leftDefaultPos;
    private Vector3 rightDefaultPos;
    private Vector2 leftDefaultSize;
    private Vector2 rightDefaultSize;
    //计算所需增加的高度
    private float bgStartHeight;
    private Vector2 bgSize;
    private Vector2 bottomSize;
    private Vector2 defaultSize;
    //存选项btn
    private List<GameObject> optionButton = new List<GameObject>();
    private int optionIdx;
    //存方法
    private delegate void OptionFunc();
    //按照顺序存
    private Dictionary<int, OptionFunc> funcDic = new Dictionary<int, OptionFunc>();
    private int[] funcArray = new int[10];
    //方法索引
    private int funcIdx;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        heroOption = HeroProperty.HeroOption.Instance();
        defaultSize = menuBg.rectTransform.sizeDelta;
        defaultCursorPos = optionCursor.transform.position / 100;
        leftDefaultPos = menuLeft.transform.position / 100;
        rightDefaultPos = menuRight.transform.position / 100;
        leftDefaultSize = menuLeft.rectTransform.sizeDelta;
        rightDefaultSize = menuRight.rectTransform.sizeDelta;
    }

    /// <summary>
    /// 初始化，判断要显示几种选项
    /// </summary>
    public void InitMenu()
    {
        countOptions = 0;
        optionIdx = 0;
        funcIdx = 0;
        GetOptions();

        InitMenuRect();
        //计算背景显示
        bgStartHeight = bgSize.y;
        bgSize.y = bgStartHeight * countOptions;
        bottomSize.y = bgSize.y - bgStartHeight;
        menuBg.rectTransform.sizeDelta = bgSize;
        menuBottom.rectTransform.position -= new Vector3(0, bottomSize.y / 100, 0);
        //处理左右边界
        menuRight.transform.position -= new Vector3(0, defaultSize.y / 100 * (countOptions - 1) / 2, 0);
        menuRight.rectTransform.sizeDelta = new Vector2(rightDefaultSize.x, rightDefaultSize.y * countOptions);
        menuLeft.transform.position -= new Vector3(0, defaultSize.y / 100 * (countOptions - 1) / 2, 0);
        menuLeft.rectTransform.sizeDelta = new Vector2(leftDefaultSize.x, leftDefaultSize.y * countOptions);
    }

    //menu的ui数据初始化
    private void InitMenuRect()
    {
        bgSize = defaultSize;
        bottomSize = defaultSize;
        menuRight.transform.position = rightDefaultPos;
        menuRight.rectTransform.sizeDelta = rightDefaultSize;
        menuLeft.transform.position = leftDefaultPos;
        menuLeft.rectTransform.sizeDelta = leftDefaultSize;
    }

    public override void Display()
    {
        InitMenu();
        base.Display();
        RegisterKeyBoardEvent();
        UpdateOption();
    }

    public override void Hiding()
    {
        base.Hiding();
        GameObjectPool.Instance().PushPool(optionButton, MainProperty.BUTTON_PATH);
        optionButton.Clear();
        UnRegisterKeyBoardEvent();
        funcDic.Clear();
        funcArray = new int[10];
        menuBottom.rectTransform.position += new Vector3(0, bottomSize.y / 100, 0);
        optionCursor.transform.position = defaultCursorPos;
    }

    /// <summary>
    /// 添加选项进content
    /// </summary>
    private void AddItem(HeroProperty.HeroOptions option)
    {
        countOptions += 1;
        //GameObject btn = Instantiate(optionPrefab) as GameObject;
        GameObject btn = GameObjectPool.Instance().GetPool(MainProperty.BUTTON_PATH, Vector3.zero);
        btn.transform.SetParent(uiContent.transform);
        btn.transform.SetSiblingIndex(heroOption.optionValue[option]);
        btn.transform.localScale = Vector3.one;
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
        
        //排序还待优化
        //待机为默认
        HeroProperty.HeroOptions option = HeroProperty.HeroOptions.Standby;
        //AddItem(option);
        //RegisterButtonObjectEvent(name, p => Standby());
        funcDic.Add(heroOption.optionValue[option], Standby);
        SortFunc(option);
        //检测敌人
        int id = MainManager.Instance().curHero.CheckIsEnemyAround();
        if (id != -1)
        {
            option = HeroProperty.HeroOptions.Attack;
            //AddItem(option);
            //RegisterButtonObjectEvent(name, p => Attack());
            funcDic.Add(heroOption.optionValue[option], Attack);
            SortFunc(option);
        }
        //检测物品
        option = HeroProperty.HeroOptions.Item;
        //AddItem(option);
        funcDic.Add(heroOption.optionValue[option], Item);
        SortFunc(option);
        //添加到content
        for (int i = 0; i < funcIdx; i++)
        {
            AddItem(heroOption.GetOption(funcArray[i]));
        }
    }

    /// <summary>
    /// 将方法排序
    /// </summary>
    private void SortFunc(HeroProperty.HeroOptions option)
    {
        funcArray[funcIdx] = heroOption.optionValue[option];
        funcIdx++;
        if (funcIdx > 1)
        {
            for (int i = 0; i < funcIdx - 1; i++)
            {
                for (int j = 1; j < funcIdx; j++)
                {
                    if (funcArray[i] > funcArray[j])
                    {
                        int z = funcArray[i];
                        funcArray[i] = funcArray[j];
                        funcArray[j] = z;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 注册键盘事件
    /// </summary>
    private void RegisterKeyBoardEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void UnRegisterKeyBoardEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
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
        UpdateOption();
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
        UpdateOption();
    }

    private void OnConfirmDown()
    {
        funcDic[funcArray[optionIdx]]();
    }

    private void OnCancelDown()
    {
        MainManager.Instance().curHero.CancelMoveDone();
    }

    /// <summary>
    /// 更新选项状态
    /// </summary>
    private void UpdateOption()
    {
        //攻击选项会显示攻击范围
        HeroProperty.HeroOptions option = HeroProperty.HeroOptions.Attack;
        if (funcDic.ContainsKey(heroOption.optionValue[option]) && optionIdx == heroOption.optionValue[option])
            MainManager.Instance().ShowAttackRange();
        else
            MainManager.Instance().HideAttackRange();
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
        UIManager.Instance().CloseUIForms("HeroMenu");
        OpenUIForm("WeaponSelectMenu");
    }

    private void Item()
    {
        Debug.Log("物品");
    }
    #endregion
}
