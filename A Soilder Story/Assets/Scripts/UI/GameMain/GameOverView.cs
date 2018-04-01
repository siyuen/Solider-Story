using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using QFramework;
using UnityEngine.UI;

public class GameOverView : UIBase {

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
        RegisterKeyBoardEvent();
    }

    private void Clear()
    {
        UnRegisterKeyBoardEvent();
    }

    public override void OnConfirmDown()
    {
        UIManager.Instance().CloseUIForms("GameOver");
        OpenUIForm("Login");
    }
}
