using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;

public class LoginView : UIBase {

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
        InputManager.Instance().RegisterKeyDownEvent(StartGame, EventType.KEY_Z);
    }

    private void Clear()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(StartGame, EventType.KEY_Z);
    }

    private void StartGame()
    {
        UIManager.Instance().CloseUIForms("Login");
        OpenUIForm("StartOption");
    }
}
