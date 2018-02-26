using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

public class FightDataView : UIBase
{
    public static Vector3 RIGHTPOS = new Vector3(278, 30, 0);
    public static Vector3 LEFTPOS = new Vector3(-250, 30, 0);
    //hero
    public Image heroWeaponImg;
    public Text heroName;
    public Text heroHp;
    public Text heroDmg;
    public Text heroHit;
    public Text heroCrt;
    //enemy
    public Image enemyWeaponImg;
    public Text enemyName;
    public Text enemyHp;
    public Text enemyDmg;
    public Text enemyHit;
    public Text enemyCrt;
    public Text weaponName;
    //cursor
    private GameObject attackCursor;
    //role
    private HeroController curHero;
    private WeaponData heroWeapon;
    private EnemyController curEnemy;
    private WeaponData enemyWeapon;
    //可攻击单位的list
    private List<int> enemyList = new List<int>();
    private int idx;
    private MapNode curNode;

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
        //肯定有敌人或者可攻击的地形才能进入这界面
        enemyList = MainManager.Instance().curHero.CheckEnemy();
        List<int> crack = MainManager.Instance().curHero.CheckCrack();
        for (int i = 0; i < crack.Count; i++)
        {
            enemyList.Add(crack[i]);
        }
        attackCursor = ResourcesMgr.Instance().GetPool(MainProperty.ATTACKCURSOR_PATH);
        curHero = MainManager.Instance().curHero;
        //默认右边
        if (curHero.mAnimator.GetBool("bSelected"))
        {
            curHero.SetAnimator("bRight", true);
            curHero.dirStr = "bRight";
            curHero.SetAnimator("bSelected", false);
        }
        if (MainManager.Instance().Idx2ListPos(curHero.mIdx).x > MainManager.Instance().GetXNode() / 2)
            this.transform.localPosition = LEFTPOS;
        else
            this.transform.localPosition = RIGHTPOS;
        idx = 0;
        SetCursor(idx);
        //MoveManager.Instance().ShowAttackRange();

        RegisterEvent();
    }

    private void Clear()
    {
        ResourcesMgr.Instance().PushPool(attackCursor, MainProperty.ATTACKCURSOR_PATH);
        UnRegisterEvent();
        EffectManager.Instance().Clear();
        enemyList.Clear();
        curNode = null;
        curEnemy = null;
        curHero = null;
    }

    public void SetData()
    {
        EffectManager.Instance().Clear();
        SetHeroData();
        if (curEnemy != null)
            SetEnemyData();
        else if (curNode != null)
            SetLandData();
    }

    private void SetHeroData()
    {
        //hero
        heroWeapon = curHero.curWeapon;
        heroWeaponImg.sprite = ResourcesMgr.Instance().LoadSprite(heroWeapon.sprite);  
        heroName.text = curHero.mName;
        heroHp.text = curHero.cHp.ToString();
        if (curEnemy != null)
        {
            //伤害
            heroDmg.text = DataManager.GetDamge(curHero, curEnemy).ToString();
            //命中
            heroHit.text = DataManager.GetFightHit(curHero, curEnemy).ToString();
            //必杀
            heroCrt.text = DataManager.GetCrt(curHero, curHero.curWeapon).ToString();
            //连击效果
            if (DataManager.CanDoubleAttack(curHero, curEnemy))
            {
                Vector3 pos = new Vector3(heroDmg.rectTransform.sizeDelta.x, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(heroDmg.gameObject, MainProperty.EFFECT_DOUBLE, pos);
                DOTween.Sequence().
                    Append(effect.transform.DOMove(effect.transform.position + new Vector3(-0.2f, 0.2f, 0), 0.4f)).
                    Append(effect.transform.DOMove(effect.transform.position + new Vector3(-0.4f, 0, 0), 0.4f)).
                    Append(effect.transform.DOMove(effect.transform.position + new Vector3(-0.2f, -0.2f, 0), 0.4f)).
                    Append(effect.transform.DOMove(effect.transform.position + new Vector3(0, 0f, 0), 0.4f)).
                    SetLoops(-1);

            }
            //克制效果
            if (DataManager.GetWeaponCounter(curHero.curWeapon, curEnemy.curWeapon) == 0)
            {
                Vector3 pos = new Vector3(heroWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(heroWeaponImg.rectTransform, MainProperty.EFFECT_UP, pos);
                EffectManager.Instance().SetEffectMove(effect, effect.transform.position + new Vector3(0, 0.1f, 0), 0.5f, -1);
            }
            else if (DataManager.GetWeaponCounter(curHero.curWeapon, curEnemy.curWeapon) == 1)
            {
                Vector3 pos = new Vector3(heroWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(heroWeaponImg.rectTransform, MainProperty.EFFECT_DOWN, pos);
                EffectManager.Instance().SetEffectMove(effect, effect.transform.position + new Vector3(0, -0.1f, 0), 0.5f, -1);
            }
        }
        else
        {
            heroDmg.text = DataManager.GetAttack(curHero, curHero.curWeapon).ToString();
            heroHit.text = DataManager.GetHit(curHero, curHero.curWeapon).ToString();
            heroCrt.text = DataManager.GetCrt(curHero, curHero.curWeapon).ToString();
        }
       
        
    }

    private void SetEnemyData()
    {
        //enemy
        enemyWeapon = curEnemy.curWeapon;
        enemyName.text = curEnemy.mName;
        enemyHp.text = curEnemy.cHp.ToString();
        if (enemyWeapon != null)
        {
            enemyWeaponImg.sprite = ResourcesMgr.Instance().LoadSprite(enemyWeapon.sprite);
            enemyWeaponImg.color = new Color(1, 1, 1, 1);
            weaponName.text = curEnemy.curWeapon.name;
            //伤害
            enemyDmg.text = DataManager.GetDamge(curEnemy, curHero).ToString();
            //命中
            enemyHit.text = DataManager.GetFightHit(curEnemy, curHero).ToString();
            //必杀
            enemyCrt.text = DataManager.GetCrt(curEnemy, curEnemy.curWeapon).ToString();
            if (DataManager.CanDoubleAttack(curEnemy, curHero))
            {
                Vector3 pos = new Vector3(heroDmg.rectTransform.sizeDelta.x, 0, 0);
                EffectManager.Instance().AddEffect(enemyDmg.gameObject, MainProperty.EFFECT_DOUBLE, pos);
            }
            //克制效果
            if (DataManager.GetWeaponCounter(curEnemy.curWeapon, curHero.curWeapon) == 0)
            {
                Vector3 pos = new Vector3(enemyWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(enemyWeaponImg.rectTransform, MainProperty.EFFECT_UP, pos);
                EffectManager.Instance().SetEffectMove(effect, effect.transform.position + new Vector3(0, 0.1f, 0), 0.5f, -1);
            }
            else if (DataManager.GetWeaponCounter(curEnemy.curWeapon, curHero.curWeapon) == 1)
            {
                Vector3 pos = new Vector3(enemyWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(enemyWeaponImg.rectTransform, MainProperty.EFFECT_DOWN, pos);
                EffectManager.Instance().SetEffectMove(effect, effect.transform.position + new Vector3(0, -0.1f, 0), 0.5f, -1);
            }
        }
        else
        {
            enemyWeaponImg.sprite = null;
            enemyWeaponImg.color = new Color(1, 1, 1, 0);
            weaponName.text = "";
            //伤害
            enemyDmg.text = "--";
            //命中
            enemyHit.text = "--";
            //必杀
            enemyCrt.text = "--";
        }
    }

    private void SetLandData()
    {
        enemyName.text = curNode.fightName;
        enemyHp.text = curNode.mLife.ToString();
        enemyWeaponImg.sprite = null;
        enemyWeaponImg.color = new Color(1, 1, 1, 0);
        weaponName.text = "";
        enemyDmg.text = "--";
        enemyHit.text = "--";
        enemyCrt.text = "--";
    }

    /// <summary>
    /// 注册事件，选择敌人；如果范围内敌人不止一个则注册转换事件
    /// </summary>
    public void RegisterEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
        //大于一个敌人
        if (enemyList.Count > 1)
        {
            InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
            InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
            InputManager.Instance().RegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
            InputManager.Instance().RegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);
        }
    }

    private void UnRegisterEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
    }

    private void OnUpArrowDown()
    {
        idx -= 1;
        if (idx < 0)
            idx += enemyList.Count;
        SetCursor(idx);
    }

    private void OnDownArrowDown()
    {
        idx += 1;
        if (idx >= enemyList.Count)
            idx -= enemyList.Count;
        SetCursor(idx);
    }

    private void OnLeftArrowDown()
    {
        idx -= 1;
        if (idx < 0)
            idx += enemyList.Count;
        SetCursor(idx);
    }

    private void OnRightArrowDown()
    {
        idx += 1;
        if (idx >= enemyList.Count)
            idx -= enemyList.Count;
        SetCursor(idx);
    }

    private void SetCursor(int idx)
    {
        curEnemy = null;
        curNode = null;
        //根据方位调整hero的朝向
        attackCursor.transform.position = MainManager.Instance().Idx2Pos(enemyList[idx]);
        string curDir = curHero.dirStr;
        string dir = MoveManager.Instance().GetDir(curHero.mIdx, enemyList[idx]);
        if (dir == "Left")
        {
            curHero.SetAnimator("bLeft", true);
            curHero.dirStr = "bLeft";
        }
        else if (dir == "Right")
        {
            curHero.SetAnimator("bRight", true);
            curHero.dirStr = "bRight";
        }
        else if (dir == "Up")
        {
            curHero.SetAnimator("bUp", true);
            curHero.dirStr = "bUp";
        }
        else if (dir == "Down")
        {
            curHero.SetAnimator("bDown", true);
            curHero.dirStr = "bDown";
        }
        curHero.SetAnimator(curDir, false);

        if (MainManager.Instance().GetMapNode(enemyList[idx]).locatedEnemy != null)
            curEnemy = MainManager.Instance().GetMapNode(enemyList[idx]).locatedEnemy;
        else if (MainManager.Instance().GetMapNode(enemyList[idx]).TileType == "Crack")
            curNode = MainManager.Instance().GetMapNode(enemyList[idx]);
        SetData();
    }

    /// <summary>
    /// 确定选择的敌人，进入战斗主界面
    /// </summary>
    private void OnConfirmDown()
    {
        MoveManager.Instance().HideAttackRange();
        if (curEnemy != null)
        {
            MainManager.Instance().curEnemy = curEnemy;
            UIManager.Instance().CloseUIForms("FightData");
            FightManager.Instance().Init(FightManager.HEROROUND);
        }
        else
        {
            MainManager.Instance().curNode = curNode;
            UIManager.Instance().CloseUIForms("FightData");
            FightManager.Instance().Init(FightManager.ATTACKLAND);
        }
    }

    /// <summary>
    /// 回退到选择武器界面
    /// </summary>
    private void OnCancelDown()
    {
        MainManager.Instance().curEnemy = null;
        UIManager.Instance().CloseUIForms("FightData");
        ItemManager.Instance().Init(ItemManager.WEAPONMENU);
    }
}
