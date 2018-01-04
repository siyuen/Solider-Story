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
        HideMenuUI();
        if (HeroManager.Instance().SetStandby())
        {
            MainManager.Instance().HideAllUI();
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { MainManager.Instance().SetEnemyRound(); }, 1f));
        }
        else
        {
            MainManager.Instance().ShowAllUI();
        }
    }

    /// <summary>
    /// 设置动画s
    /// </summary>
    public void SetAnimator(string name, bool b)
    {
        mAnimator.SetBool(name, b);
    }

    /// <summary>
    /// 鼠标点击人物
    /// </summary>
    public void Selected()
    {
        if (heroState != HeroState.normal)
            return;
        if (!bSelected)
        {
            bSelected = true;
            mainInstance.curHero = this;
            ShowMoveRange();
            UIManager.Instance().CloseUIForms("CharacterData");
            mainInstance.HideAllUI();
        }
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
        UIManager.Instance().ShowUIForms("HeroMenu");
        mainInstance.HideAllUI();
    }

    /// <summary>
    /// 隐藏人物选项
    /// </summary>
    private void HideMenuUI()
    {
        UIManager.Instance().CloseUIForms("HeroMenu");
    }

}
