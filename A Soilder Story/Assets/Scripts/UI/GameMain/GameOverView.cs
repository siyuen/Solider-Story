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
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_X);
    }

    private void Clear()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_X);
    }

    private void OnConfirmDown()
    {
        UIManager.Instance().CloseUIForms("GameOver");
        OpenUIForm("Login");
    }
}
