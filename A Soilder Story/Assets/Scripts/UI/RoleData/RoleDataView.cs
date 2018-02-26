using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using UnityEngine.UI;

public class RoleDataView : UIBase {
    
    public PersonalData firstPage;
    public Image secondPage;
    public Image thirdPage;
    public Image heroImage;

    public Text careerText;
    public Text levelText;
    public Text expText;
    public Text cHpText;
    public Text tHpText;

    private HeroController curHero;
    private EnemyController curEnemy;
    private int mPage;
    private int roleId;

    void Awake()
    {
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        CurrentUIType.UIForms_Type = UIFormType.Normal;

        firstPage.gameObject.SetActive(true);
        secondPage.gameObject.SetActive(false);
        thirdPage.gameObject.SetActive(false);
        mPage = 1;
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
        RegisterEvent();
        SetData();
    }

    private void Clear()
    {
        UnRegisterEvent();
        curHero = null;
        curEnemy = null;
    }

    private void SetData()
    {
        if(MainManager.Instance().curMouseHero)
        {
            curHero = MainManager.Instance().curMouseHero;
            roleId = curHero.mID;
            heroImage.sprite = ResourcesMgr.Instance().LoadSprite(curHero.lImage);
            careerText.text = curHero.mCareer.ToString();
            levelText.text = curHero.mLevel.ToString();
            expText.text = curHero.mExp.ToString();
            cHpText.text = curHero.cHp.ToString();
            tHpText.text = curHero.tHp.ToString();
        }
        else if (MainManager.Instance().curMouseEnemy)
        {
            curEnemy = MainManager.Instance().curMouseEnemy;
            heroImage.sprite = ResourcesMgr.Instance().LoadSprite(curEnemy.lImage);
            careerText.text = curEnemy.mCareer.ToString();
            levelText.text = curEnemy.mLevel.ToString();
            expText.text = curEnemy.mExp.ToString();
            cHpText.text = curEnemy.cHp.ToString();
            tHpText.text = curEnemy.tHp.ToString();
        }
        if (mPage == 1)
        {
            if(curHero)
                firstPage.InitData(curHero);
            if (curEnemy)
                firstPage.InitData(curEnemy);
        }
    }

    #region 事件
    private void RegisterEvent()
    {
        InputManager input = InputManager.Instance();
        input.RegisterKeyDownEvent(TurnUp, EventType.KEY_UPARROW);
        input.RegisterKeyDownEvent(TurnDown, EventType.KEY_DOWNARROW);
        input.RegisterKeyDownEvent(TurnLeft, EventType.KEY_LEFTARROW);
        input.RegisterKeyDownEvent(TurnRight, EventType.KEY_RIGHTARROW);
        input.RegisterKeyDownEvent(TurnConfirm, EventType.KEY_Z);
        input.RegisterKeyDownEvent(TurnCancel, EventType.KEY_X);
        input.RegisterKeyDownEvent(TurnRT, EventType.KEY_S);
    }

    private void UnRegisterEvent()
    {
        InputManager input = InputManager.Instance();
        input.UnRegisterKeyDownEvent(TurnUp, EventType.KEY_UPARROW);
        input.UnRegisterKeyDownEvent(TurnDown, EventType.KEY_DOWNARROW);
        input.UnRegisterKeyDownEvent(TurnLeft, EventType.KEY_LEFTARROW);
        input.UnRegisterKeyDownEvent(TurnRight, EventType.KEY_RIGHTARROW);
        input.UnRegisterKeyDownEvent(TurnConfirm, EventType.KEY_Z);
        input.UnRegisterKeyDownEvent(TurnCancel, EventType.KEY_X);
        input.UnRegisterKeyDownEvent(TurnRT, EventType.KEY_S);
    }


    private void TurnUp()
    {
        int total = HeroManager.Instance().liveHeroList.Count;
        roleId -= 1;
        if (roleId < 0)
            roleId += total;
        MainManager.Instance().SetMouseRole(roleId);
        SetData();
    }

    private void TurnDown()
    {
        int total = HeroManager.Instance().liveHeroList.Count;
        roleId += 1;
        if (roleId >= total)
            roleId -= total;
        MainManager.Instance().SetMouseRole(roleId);
        SetData();
    }

    private void TurnLeft()
    {
    }

    private void TurnRight()
    {
    }

    private void TurnConfirm()
    {
    }

    private void TurnCancel()
    {
        UIManager.Instance().CloseUIForms("RoleData");
        MainManager.Instance().RegisterKeyBoardEvent();
        MainManager.Instance().ShowAllUI();
    }

    private void TurnRT()
    {
    }
    #endregion
}
