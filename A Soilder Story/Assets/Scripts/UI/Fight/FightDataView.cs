using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;

public class FightDataView : UIBase
{
    //hero
    public Image heroWeapon;
    public Text heroName;
    public Text heroHp;
    public Text heroDmg;
    public Text heroHit;
    public Text heroCrt;
    //enemy
    public Image enemyWeapon;
    public Text enemyName;
    public Text enemyHp;
    public Text enemyDmg;
    public Text enemyHit;
    public Text enemyCrt;
    public Text weaponName;
    //cursor
    private GameObject attackCursor;
    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
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

    /// <summary>
    /// 
    /// </summary>
    private void Init()
    {
        //肯定有敌人才能进入这界面
        List<int> enemy = MainManager.Instance().curHero.CheckEnemy();
        attackCursor = GameObjectPool.Instance().GetPool(MainProperty.ATTACKCURSOR_PATH, MainManager.Instance().Idx2Pos(enemy[0]));
        MoveManager.Instance().ShowAttackRange();
        RegisterEvent();
        
    }

    private void Clear()
    {
        GameObjectPool.Instance().PushPool(attackCursor, MainProperty.ATTACKCURSOR_PATH);
        UnRegisterKeyBoardEvent();
    }

    /// <summary>
    /// 注册事件，选择敌人；如果范围内敌人不止一个则注册转换事件
    /// </summary>
    public void RegisterEvent(bool b = false)
    {
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
        //大于一个敌人
        if (b)
        {
            //InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
            //InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        }
    }

    private void UnRegisterKeyBoardEvent()
    {
        //InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        //InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    /// <summary>
    /// 确定选择的敌人，进入战斗主界面
    /// </summary>
    private void OnConfirmDown()
    {
    }

    /// <summary>
    /// 回退到选择武器界面
    /// </summary>
    private void OnCancelDown()
    {
        UIManager.Instance().CloseUIForms("FightData");
        OpenUIForm("WeaponSelectMenu");
    }
}
