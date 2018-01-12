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

    public override void MoveTo(int to)
    {
        mainInstance.GetMapNode(mID).locatedHero = null;
        base.MoveTo(to);
    }

    public override void MoveTo(MapNode to)
    {
        mainInstance.GetMapNode(mID).locatedHero = null;
        base.MoveTo(to);
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
        MoveManager.Instance().HideAttackRange();
        HideMenuUI();
        if (HeroManager.Instance().SetStandby())
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { mainInstance.SetEnemyRound(); }, 1f));
        else
            mainInstance.ShowAllUI();
    }

    /// <summary>
    /// 检测攻击范围内的敌人:根据攻击type
    /// </summary>
    public List<int> CheckEnemy()
    {
        //范围一
        List<int> enemy = new List<int>();
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mID + 1) && mainInstance.GetMapNode(mID + 1).locatedEnemy)
            enemy.Add(mID + 1);
        else if (mainInstance.IsInMap(mID + col) && mainInstance.GetMapNode(mID + col).locatedEnemy)
            enemy.Add(mID + col);
        else if (mainInstance.IsInMap(mID - 1) && mainInstance.GetMapNode(mID - 1).locatedEnemy)
            enemy.Add(mID - 1);
        else if (mainInstance.IsInMap(mID - col) && mainInstance.GetMapNode(mID - col).locatedEnemy)
            enemy.Add(mID - col);
        return enemy;
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
            SetAnimator("Mouse", false);
            ShowMoveRange();
            mainInstance.HideAllUI();
        }
    }

    /// <summary>
    /// 取消选择，回到正常状态
    /// </summary>
    public void CancelSelected()
    {
        heroState = HeroState.normal;
        bSelected = false;
        SetAnimator("bSelected", bSelected);
        SetAnimator("bNormal", true);
        mID = fromIdx;
        HideMoveRange();
    }

    /// <summary>
    /// 取消移动，返回选择人物状态
    /// </summary>
    public void CancelMoveDone()
    {
        heroState = HeroState.normal;
        mainInstance.GetMapNode(mID).locatedHero = null;
        this.transform.position = mainInstance.Idx2Pos2(fromIdx);
        mID = fromIdx;
        SetAnimator("bSelected", true);
        SetAnimator(dirStr, false);
        MoveManager.Instance().HideAttackRange();
        ShowMoveRange();
        MoveManager.Instance().ShowRoad(mainInstance.GetCursorIdx());
        HideMenuUI();
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
