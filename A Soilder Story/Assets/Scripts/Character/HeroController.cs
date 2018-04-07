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
        selectd,
        stop,
        dead,
    }
    public HeroState heroState;
    public bool bChangeItem;
    //移动的起点
    private int fromIdx;

    private UpdateCurHero curHero = new UpdateCurHero();
    // Use this for initialization
    void Start()
    {
        fightRole = ResourcesMgr.Instance().GetPool(rolePro.fightPrefab);
        fightRole.transform.SetParent(HeroManager.Instance().heroContent.transform);
        fightRole.SetActive(false);
        curHero.hero = this;
    }

    public override void Init()
    {
        base.Init();

        heroState = HeroState.normal;
        bChangeItem = false;
        mIdx = levelInstance.Pos2Idx(this.transform.position);
        levelInstance.GetMapNode(mIdx).locatedHero = this;
    }

    public override void InitData(PublicRoleData data)
    {
        base.InitData(data);
        isHero = true;
        
    }

    public void Clear()
    {
        levelInstance.GetMapNode(mIdx).locatedHero = null;
        ResourcesMgr.Instance().PushPool(fightRole, rolePro.fightPrefab);
    }

    public void MoveTo(int to)
    {
        levelInstance.GetMapNode(mIdx).locatedHero = null;
        SetAnimator("bSelected", false);
        roleMove.MoveTo(to);
    }

    public void MoveTo(MapNode to)
    {
        levelInstance.GetMapNode(mIdx).locatedHero = null;
        SetAnimator("bSelected", false);
        roleMove.MoveTo(to);
    }

    public override void MoveDone()
    {
        heroState = HeroState.selectd;
        levelInstance.GetMapNode(mIdx).locatedHero = this;
        UIManager.Instance().ShowUIForms("HeroMenu");
    }

    public override void Standby()
    {
        base.Standby();
        mAnimator.SetBool("bSelected", false);
        mAnimator.SetBool("bMouse", false);
        mAnimator.SetBool("bNormal", false);
        heroState = HeroState.stop;
        if (HeroManager.Instance().SetStandby())
            MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.UPDATEROUND));
        else
            MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.HEROSTANDBY));
    }

    /// <summary>
    /// 死亡,返回游戏是否结束
    /// </summary>
    public bool Dead()
    {
        Clear();
        mainInstance.curHero = null;
        levelInstance.GetMapNode(mIdx).locatedHero = null;
        return HeroManager.Instance().SetDead(listIdx);
    }

    /// <summary>
    /// 选中状态
    /// </summary>
    public void Selected()
    {
        if (heroState != HeroState.normal)
            return;
        MessageCenter.Instance().DispatchEvent(new MessageEvent(EventType.UPDATECURHERO, curHero));
        heroState = HeroState.selectd;
        fromIdx = mIdx;
        SetAnimator("bSelected", true);
        SetAnimator("bNormal", false);
        SetAnimator("bMouse", false);
        MoveManager.Instance().ShowMoveRange(this.transform.position, rolePro.mMove, 1);
    }

    public override void LevelUp(int add = 1)
    {
        UIManager.Instance().ShowUIForms("LevelUp");
        base.LevelUp(add);
        LevelUpView view = UIManager.Instance().GetUI("LevelUp").GetComponent<LevelUpView>();
        view.LevelUp();
        rolePro.SetProValue(RolePro.PRO_EXP, 0);
    }

    /// <summary>
    /// 取消选择，回到正常状态
    /// </summary>
    public void CancelSelected()
    {
        heroState = HeroState.normal;
        SetAnimator("bSelected", false);
        SetAnimator("bNormal", true);
        mIdx = fromIdx;
        levelInstance.GetMapNode(mIdx).locatedHero = this;
        HideMoveRange();
    }

    /// <summary>
    /// 取消移动，返回选择人物状态
    /// </summary>
    public void CancelMoveDone()
    {
        heroState = HeroState.normal;
        levelInstance.GetMapNode(mIdx).locatedHero = null;
        this.transform.position = levelInstance.Idx2Pos2(fromIdx);
        mIdx = fromIdx;
        SetAnimator("bSelected", true);
        SetAnimator(dirStr, false);
        MoveManager.Instance().HideAttackRange();
        MoveManager.Instance().ShowMoveRange(this.transform.position, rolePro.mMove, 1);
        MoveManager.Instance().ShowRoad(mainInstance.GetCursorIdx());
        mainInstance.SetCursorActive(true);
    }
}
