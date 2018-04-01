using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;

public class StoryView : UIBase
{
    public Image bgImage;
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
        //暂时显示个剧情图片当通关
        string path = "Sprites/UI/Level/LevelOver_" + LevelManager.Instance().GetCurLevel().ToString();
        Debug.Log(path);
        bgImage.sprite = ResourcesMgr.Instance().LoadSprite(path);

        RegisterKeyBoardEvent();
    }

    private void Clear()
    {
        UnRegisterKeyBoardEvent();
    }

    public override void OnConfirmDown()
    {
        GameManager.Instance().gameState = GameManager.GameState.Save;
        UIManager.Instance().CloseUIForms("Story");
        OpenUIForm("StartGame");
    }
}
