using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using QFramework;
using UnityEngine.UI;

public class GameOptionMenu : UIBase {

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

    public void Init()
    {
        //暂时写死
        GameObject btn = ResourcesMgr.Instance().GetPool(MainProperty.BUTTON_PATH);
        Text txt = btn.GetComponentInChildren<Text>();
        txt.text = "中断";
        txt.color = ItemManager.COLOR_USEITEM;
        menuView.AddItem(MainProperty.BUTTON_PATH, btn, StopGame, Test);
        optionList.Add(btn);

        btn = ResourcesMgr.Instance().GetPool(MainProperty.BUTTON_PATH);
        txt = btn.GetComponentInChildren<Text>();
        txt.text = "结束";
        txt.color = ItemManager.COLOR_USEITEM;
        menuView.AddItem(MainProperty.BUTTON_PATH, btn, RoundEnd, Test);
        optionList.Add(btn);

        menuView.cancleFunc = OnCancle;
        menuView.bAnim = true;
        menuView.DisplayInit();
    }

    public void Clear()
    {
        menuView.Hide();
        optionList.Clear();
    }

    /// <summary>
    /// 游戏中断，回到初始界面
    /// </summary>
    private void StopGame()
    {
        UIManager.Instance().CloseUIForms("GameOption");
        GameManager.Instance().TemporarySave();
        UIManager.Instance().ShowUIForms("Login");
    }

    /// <summary>
    /// 结束回合
    /// </summary>
    private void RoundEnd()
    {
        UIManager.Instance().CloseUIForms("GameOption");
        HeroManager.Instance().SetAllStandby();
    }

    private void OnCancle()
    {
        UIManager.Instance().CloseUIForms("GameOption");
        MainManager.Instance().RegisterKeyBoardEvent();
        MainManager.Instance().ShowAllUI();
    }

    private void Test()
    {
    }
}
