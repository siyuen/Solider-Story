using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using QFramework;

public class EnemyController : Character
{
    public const int NULLTARGET = -1;
    public enum EnemyState
    {
        normal,
        selcted,
        stop,
        dead
    }
    //private EnemyState enemyState;
    private List<int> attackHeroList = new List<int>();
    //private List<int> heroList = new List<int>();  //在攻击范围内的hero
    private int targetIdx;
    //cursor
    public GameObject attackCursor;
	// Use this for initialization
	void Start () {
        dirStr = "bNormal";
        fightRole = ResourcesMgr.Instance().GetPool(fightPrefab);
        fightRole.transform.SetParent(EnemyManager.Instance().enemyContent.transform);
        fightRole.SetActive(false);
        isHero = false;
	}

    public override void Init()
    {
        base.Init();
        //enemyState = EnemyState.normal;
        bStandby = false;
        mIdx = mainInstance.Pos2Idx(this.transform.position);
        mainInstance.GetMapNode(mIdx).locatedEnemy = this;
    }

    public void Clear()
    {
        MainManager.Instance().GetMapNode(mIdx).locatedEnemy = null;
        ResourcesMgr.Instance().PushPool(fightRole, fightPrefab);
    }

    public void InitData(EnemyProData data)
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
    public EnemyProData SaveData()
    {
        EnemyProData data = EnemyManager.Instance().enemyList[mID];
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

	// Update is called once per frame
	void Update () {
        if (bMove)
            Move();
	}

    public override void MoveTo(int to)
    {
        mainInstance.GetMapNode(mIdx).locatedEnemy = null;
        mainInstance.curMouseEnemy = null;
        base.MoveTo(to);
    }

    public override void MoveTo(MapNode to)
    {
        mainInstance.GetMapNode(mIdx).locatedEnemy = null;
        mainInstance.curMouseEnemy = null;
        base.MoveTo(to);
    }

    //搜索附近hero
    public void SearchHero(bool move = true)
    {
        targetIdx = NULLTARGET;
        MainManager.Instance().curEnemy = this;
        //boss不移动只判断周围
        if (move)
        {
            //暂时规则
            //优先判断周围四个格子，若没有目标，则获取移动范围内的
            attackHeroList = CheckHero();
            if (attackHeroList.Count > 0)
            {
                targetIdx = mainInstance.GetMapNode(attackHeroList[0]).GetID();
                MainManager.Instance().curHero = mainInstance.GetMapNode(targetIdx).locatedHero;
                Attack();
            }
            else
            {
                //不显示范围，获取周围移动范围内的hero
                MoveManager.Instance().ReSet();
                MoveManager.Instance().DoMoveRange(this.transform.position, this.mMove, 1, true);
                MoveManager.Instance().AddAttackNodeInList(1, mIdx);
                attackHeroList = MoveManager.Instance().GetAttackHeroList();
                Debug.Log(mIdx + ",移动范围内：" + attackHeroList.Count);
                if (attackHeroList.Count > 0)
                {
                    //判断是否能移动到hero周围的块
                    int target = 0;
                    //默认最近的hero
                    int min = 100;
                    for (int i = 0; i < attackHeroList.Count; i++)
                    {
                        if (mainInstance.Idx2IdxDis(mIdx, attackHeroList[i]) < min)
                        {
                            target = i;
                            min = mainInstance.Idx2IdxDis(mIdx, attackHeroList[i]);
                        }
                    }
                    //目标hero的idx
                    targetIdx = attackHeroList[target];
                    Debug.Log("目标:" + targetIdx);
                    int to = NULLTARGET;
                    //目标hero的周围
                    List<int> node = MoveManager.Instance().GetRoundMapNode(attackHeroList[target]);
                    for (int i = 0; i < node.Count; i++)
                    {
                        MapNode m = mainInstance.GetMapNode(node[i]);
                        //node可以移动到且没有hero
                        if (m.bVisited && !m.locatedHero)
                        {
                            //如果存在enemy
                            if (m.locatedEnemy)
                            {
                                if (m.locatedEnemy == this)
                                    to = node[i];
                            }
                            else
                                to = node[i];
                        }
                    }
                    if (to != NULLTARGET)
                    {
                        MainManager.Instance().curHero = mainInstance.GetMapNode(targetIdx).locatedHero;
                        MoveTo(to);
                    }
                    else
                        Standby();
                }
                else
                    Standby();
            }
        }
        else
        {
            attackHeroList = CheckHero();
            if (attackHeroList.Count > 0)
            {
                targetIdx = mainInstance.GetMapNode(attackHeroList[0]).GetID();
                MainManager.Instance().curHero = mainInstance.GetMapNode(targetIdx).locatedHero;
                Attack();
            }
            else
                Standby();
        }
    }

    /// <summary>
    /// 选择enemy，显示移动范围，隐藏UI
    /// </summary>
    public void Selected()
    {
        if (!bSelected)
        {
            bSelected = true;
            MoveManager.Instance().ShowMoveRange(this.transform.position, mMove, 1);
            mainInstance.HideAllUI();
        }
    }

    /// <summary>
    /// 取消选择，消除移动范围,显示UI
    /// </summary>
    public void CancelSelected()
    {
        bSelected = false;
        HideMoveRange();
        mainInstance.ShowAllUI();
    }

    public override void MoveDone()
    {
        base.MoveDone();
        mainInstance.GetMapNode(mIdx).locatedEnemy = this;
        Attack();
        MoveManager.Instance().ReSet();
    }

    public void Attack()
    {
        attackCursor = ResourcesMgr.Instance().GetPool(MainProperty.ATTACKCURSOR_PATH);
        attackCursor.transform.position = MainManager.Instance().Idx2Pos(targetIdx);
        StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { FightManager.Instance().Init(FightManager.ENEMYROUND); }, 1f));
    }

    public override void Standby()
    {
        base.Standby();
        mainInstance.curEnemy = null;
        if (EnemyManager.Instance().SetStandby())
            UIManager.Instance().ShowUIForms("Round");
        else
            EnemyManager.Instance().SetEnemyRounnd();
    }

    public bool Dead()
    {
        Clear();
        mainInstance.curEnemy = null;
        mainInstance.GetMapNode(mIdx).locatedEnemy = null;
        return EnemyManager.Instance().SetDead(listIdx);
    }

    public override void LevelUp(int add = 1)
    {
        base.LevelUp(add);
    }
}
