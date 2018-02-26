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
    public HeroState heroState;
    public bool bChangeItem;
    //移动的起点
    private int fromIdx;
    // Use this for initialization
    void Start()
    {
        dirStr = "bNormal";
        fightRole = ResourcesMgr.Instance().GetPool(fightPrefab);
        fightRole.transform.SetParent(HeroManager.Instance().heroContent.transform);
        fightRole.SetActive(false);
        isHero = true;
    }

    public override void Init()
    {
        base.Init();

        heroState = HeroState.normal;
        bSelected = false;
        bStandby = false;
        bMove = false;
        bChangeItem = false;
        mIdx = mainInstance.Pos2Idx(this.transform.position);
        mainInstance.GetMapNode(mIdx).locatedHero = this;
    }

    public void Clear()
    {
        MainManager.Instance().GetMapNode(mIdx).locatedHero = null;
        ResourcesMgr.Instance().PushPool(fightRole, fightPrefab);
    }

    public void InitData(HeroProData data)
    {
        mID = DataManager.Value(data.id);
        mCareer = CareerManager.Instance().key2NameDic[data.career];
        mLevel = DataManager.Value(data.level);
        mExp = DataManager.Value(data.exp);
        mName = HeroManager.Instance().key2NameDic[data.name];
        tHp = DataManager.Value(data.thp);
        cHp = DataManager.Value(data.chp);
        mPower = DataManager.Value(data.power);
        mSkill = DataManager.Value(data.skill);
        mSpeed = DataManager.Value(data.speed);
        mLucky = DataManager.Value(data.lucky);
        pDefense = DataManager.Value(data.pdefense);
        mDefense = DataManager.Value(data.mdefense);
        mMove = DataManager.Value(data.move);
        mStrength = DataManager.Value(data.strength);
        sImage = data.simage;
        lImage = data.limage;
        mPrefab = data.prefab;
        fightPrefab = data.fightprefab;
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    public HeroProData SaveData()
    {
        HeroProData data = HeroManager.Instance().curHero[mID];
        data.name = HeroManager.Instance().name2KeyDic[mName];
        data.career = CareerManager.Instance().name2KeyDic[mCareer];
        data.level = mLevel.ToString();
        data.exp = mExp.ToString();
        data.chp = cHp.ToString();
        data.thp = tHp.ToString();
        data.power = mPower.ToString();
        data.skill = mSkill.ToString();
        data.speed = mSpeed.ToString();
        data.lucky = mLucky.ToString();
        data.pdefense = pDefense.ToString();
        data.mdefense = mDefense.ToString();
        data.move = mMove.ToString();
        data.strength = mStrength.ToString();
        return data;
    }

    public void SetCurWeapon(int idx)
    {
        if (idx >= weaponList.Count || idx < 0)
            return;
        if (!WeaponMatching(weaponList[idx]))
            return;
        if (curWeapon.tag == weaponList[idx].tag)
            return;
        WeaponData weapon = weaponList[idx];
        if (weapon.tag == curWeapon.tag)
            return;
        curWeapon = null;
        GiveUpItem(weaponList[idx].tag);
        bagList.Insert(0, weapon);
        weaponList.Insert(0, weapon);
        curWeapon = weaponList[0];
    }

    public void SetCurWeapon(string tag)
    {
        if(curWeapon != null && curWeapon.tag == tag)
            return;
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i].tag == tag && WeaponMatching(weaponList[i]))
            {
                //设为当前武器，并且将当前武器放置第一位
                curWeapon = weaponList[i];
                //从bag跟wepaon中清除
                GiveUpItem(tag);
                //插入第一位
                bagList.Insert(0, curWeapon);
                weaponList.Insert(0, curWeapon);
                return;
            }
        }
        curWeapon = null;
    }

    public void ClearBag()
    {
        bagList.Clear();
        weaponList.Clear();
        itemList.Clear();
    }
    // Update is called once per frame
    void Update()
    {
        if (bMove)
            Move();
    }

    public override void MoveTo(int to)
    {
        mainInstance.GetMapNode(mIdx).locatedHero = null;
        mainInstance.curMouseHero = null;
        base.MoveTo(to);
    }

    public override void MoveTo(MapNode to)
    {
        mainInstance.GetMapNode(mIdx).locatedHero = null;
        mainInstance.curMouseHero = null;
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
        mainInstance.GetMapNode(mIdx).locatedHero = this;
        UIManager.Instance().ShowUIForms("HeroMenu");
    }

    public override void Standby()
    {
        base.Standby();
        mAnimator.SetBool("bSelected", false);
        mAnimator.SetBool("bMouse", false);
        mAnimator.SetBool("bNormal", false);
        heroState = HeroState.stop;
        mainInstance.curHero = null;
        //直接结束回合需要先隐藏
        mainInstance.HideAllUI();
        if (HeroManager.Instance().SetStandby())
        {
            mainInstance.UnRegisterKeyBoradEvent();
            mainInstance.SetCursorActive(false);
            UIManager.Instance().ShowUIForms("Round");
        }
        else
        {
            mainInstance.SetCursorActive(true);
            mainInstance.CursorUpdate();
            mainInstance.ShowAllUI();
            mainInstance.RegisterKeyBoardEvent();
        }
    }

    /// <summary>
    /// 死亡,返回游戏是否结束
    /// </summary>
    public bool Dead()
    {
        Clear();
        mainInstance.curHero = null;
        mainInstance.GetMapNode(mIdx).locatedHero = null;
        return HeroManager.Instance().SetDead(listIdx);
    }

    /// <summary>
    /// 选中状态
    /// </summary>
    public void Selected()
    {
        if (heroState != HeroState.normal)
            return;
        if (!bSelected)
        {
            mainInstance.curHero = this;
            bSelected = true;
            fromIdx = mIdx;
            SetAnimator("bSelected", bSelected);
            SetAnimator("bNormal", false);
            SetAnimator("bMouse", false);
            MoveManager.Instance().ShowMoveRange(this.transform.position, mMove, 1);
            mainInstance.HideAllUI();
        }
    }

    public override void LevelUp(int add = 1)
    {
        UIManager.Instance().ShowUIForms("LevelUp");
        base.LevelUp(add);
        LevelUpView view = UIManager.Instance().GetUI("LevelUp").GetComponent<LevelUpView>();
        view.LevelUp();
        mExp -= 100;
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
        mIdx = fromIdx;
        HideMoveRange();
    }

    /// <summary>
    /// 取消移动，返回选择人物状态
    /// </summary>
    public void CancelMoveDone()
    {
        heroState = HeroState.normal;
        mainInstance.GetMapNode(mIdx).locatedHero = null;
        this.transform.position = mainInstance.Idx2Pos2(fromIdx);
        mIdx = fromIdx;
        SetAnimator("bSelected", true);
        SetAnimator(dirStr, false);
        MoveManager.Instance().HideAttackRange();
        MoveManager.Instance().ShowMoveRange(this.transform.position, mMove, 1);
        MoveManager.Instance().ShowRoad(mainInstance.GetCursorIdx());
        mainInstance.SetCursorActive(true);
    }
}
