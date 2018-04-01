using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;

public class LevelStartView : UIBase
{
    public Text levelName;

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
        string level = LevelManager.Instance().GetCurLevel().ToString();
        levelName.text = level + " 章:   " + LevelManager.Instance().levelDic[level].name;
        RegisterKeyBoardEvent();

        StartCoroutine( DelayToInvoke.DelayToInvokeDo(() => {OnConfirmDown();}, 3f));
    }

    private void Clear()
    {
        UnRegisterKeyBoardEvent();
    }

    public override void OnConfirmDown()
    {
        UIManager.Instance().CloseUIForms("LevelStart");
        LevelManager.Instance().SetLevel();
        MainManager.Instance().Init();
    }
}
