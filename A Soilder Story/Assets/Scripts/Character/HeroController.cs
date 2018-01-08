using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using QFramework;

public class HeroController : Character
{
    public enum HeroState
    {
        normal,
        select,
        stop,
        dead,
    }
    private HeroState heroState;
    //移动的起点
    private int fromIdx;
    // Use this for initialization
    void Start()
    {
        dirStr = "bNormal";
    }

    public override void Init()
    {
        base.Init();
        heroState = HeroState.normal;
        bSelected = false;
        bStandby = false;
        bMove = false;
        mID = mainInstance.Pos2Idx(this.transform.position);
        mainInstance.GetMapNode(mID).locatedHero = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (bMove)
            Move();
    }

    public override void Move()
    {
        mAnimator.SetBool("bSelected", false);
        base.Move();
    }

    public override void MoveDone()
    {
        base.MoveDone();
        heroState = HeroState.select;
        ShowMenuUI();
    }

    public override void Standby()
    {
        base.Standby();
        mAnimator.SetBool("bSelected", false);
        heroState = HeroState.stop;
        mainInstance.curHero = null;
        mainInstance.HideAttackRange();
        HideMenuUI();
        if (HeroManager.Instance().SetStandby())
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { mainInstance.SetEnemyRound(); }, 1f));
        else
            mainInstance.ShowAllUI();
    }

    /// <summary>
    /// 设置动画s
    /// </summary>
    public void SetAnimator(string name, bool b)
    {
        mAnimator.SetBool(name, b);
    }

    /// <summary>
    /// 选择人物
    /// </summary>
    public void Selected()
    {
        if (heroState != HeroState.normal)
            return;
        if (!bSelected)
        {
            bSelected = true;
            fromIdx = mID;
            SetAnimator("bSelected", bSelected);
            SetAnimator("bNormal", false);
            ShowMoveRange();
            Moved(false);
            mainInstance.HideAllUI();
        }
    }

    /// <summary>
    /// 取消选择
    /// </summary>
    public void CancelSelected()
    {
        heroState = HeroState.normal;
        bSelected = false;
        SetAnimator("bSelected", bSelected);
        SetAnimator("bNormal", true);
        mID = fromIdx;
        mainInstance.GetMapNode(mID).locatedHero = this;
        mainInstance.SetCursorPos(mID);
        mainInstance.HideRoad();

        HideMoveRange();
        mainInstance.ShowAllUI();
        Moved(true);
    }

    /// <summary>
    /// 取消移动，返回选择人物状态
    /// </summary>
    public void CancelMoveDone()
    {
        heroState = HeroState.normal;
        if (fromIdx != mID)
        {
            mainInstance.GetMapNode(mID).locatedHero = null;
            this.transform.position = mainInstance.Idx2Pos2(fromIdx);
            mID = fromIdx;
            mainInstance.ShowRoad(mainInstance.GetCursorIdx());
        }
        SetAnimator("bSelected", true);
        SetAnimator(dirStr, false);
        mainInstance.HideAttackRange();
        ShowMoveRange();
        HideMenuUI();
    }

    /// <summary>
    /// 鼠标移动到人物
    /// </summary>
    public void Moved(bool b)
    {
        SetAnimator("bMouse", b);
        if (b)
        {
            UIManager.Instance().ShowUIForms("CharacterData");
        }
        else
        {
            UIManager.Instance().CloseUIForms("CharacterData");
        }
    }

    /// <summary>
    /// 显示人物选项
    /// </summary>
    private void ShowMenuUI()
    {
        mainInstance.UnRegisterKeyBoradEvent();
        mainInstance.SetCursorActive(false);
        UIManager.Instance().ShowUIForms("HeroMenu");
        mainInstance.HideAllUI();
    }

    /// <summary>
    /// 隐藏人物选项
    /// </summary>
    private void HideMenuUI()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
        mainInstance.RegisterKeyBoardEvent();
        mainInstance.SetCursorActive(true);
    }

}
