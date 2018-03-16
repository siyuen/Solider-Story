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
        fightRole = ResourcesMgr.Instance().GetPool(rolePro.fightPrefab);
        fightRole.transform.SetParent(EnemyManager.Instance().enemyContent.transform);
        fightRole.SetActive(false);
        isHero = false;
	}

    public override void Init()
    {
        base.Init();
        //enemyState = EnemyState.normal;
        mIdx = levelInstance.Pos2Idx(this.transform.position);
        levelInstance.GetMapNode(mIdx).locatedEnemy = this;
    }

    public void Clear()
    {
        levelInstance.GetMapNode(mIdx).locatedEnemy = null;
        ResourcesMgr.Instance().PushPool(fightRole, rolePro.fightPrefab);
    }

	// Update is called once per frame
	void Update () {
        if (bMove)
            Move();
	}

    public override void MoveTo(int to)
    {
        levelInstance.GetMapNode(mIdx).locatedEnemy = null;
        mainInstance.curMouseEnemy = null;
        base.MoveTo(to);
    }

    public override void MoveTo(MapNode to)
    {
        levelInstance.GetMapNode(mIdx).locatedEnemy = null;
        mainInstance.curMouseEnemy = null;
        base.MoveTo(to);
    }

    //搜索附近hero
    public void SearchHero(bool move = true)
    {
        targetIdx = NULLTARGET;
        MainManager.Instance().curEnemy = this;
        if (curWeapon == null)
        {
            Standby();
            return;
        }
        //boss不移动只判断周围
        if (move)
        {
            //暂时规则
            //优先判断周围四个格子，若没有目标，则获取移动范围内的
            attackHeroList = CheckHero();
            if (attackHeroList.Count > 0)
            {
                targetIdx = levelInstance.GetMapNode(attackHeroList[0]).GetID();
                MainManager.Instance().curHero = levelInstance.GetMapNode(targetIdx).locatedHero;
                Attack();
            }
            else
            {
                //不显示范围，获取周围移动范围内的hero
                MoveManager.Instance().ReSet();
                MoveManager.Instance().DoMoveRange(this.transform.position, rolePro.mMove, 1, true);
                MoveManager.Instance().AddAttackNodeInList(1, mIdx);
                attackHeroList = MoveManager.Instance().GetAttackHeroList();
                if (attackHeroList.Count > 0)
                {
                    //判断是否能移动到hero周围的块
                    int target = 0;
                    //默认最近的hero
                    int min = 100;
                    for (int i = 0; i < attackHeroList.Count; i++)
                    {
                        if (levelInstance.Idx2IdxDis(mIdx, attackHeroList[i]) < min)
                        {
                            target = i;
                            min = levelInstance.Idx2IdxDis(mIdx, attackHeroList[i]);
                        }
                    }
                    //目标hero的idx
                    targetIdx = attackHeroList[target];
                    int to = NULLTARGET;
                    //目标hero的周围
                    List<int> node = MoveManager.Instance().GetRoundMapNode(attackHeroList[target]);
                    //记录enemy到周围节点的距离，算最小节点
                    int value = 100;
                    for (int i = 0; i < node.Count; i++)
                    {
                        MapNode m = levelInstance.GetMapNode(node[i]);
                        //node可以移动到且没有hero
                        if (m.bVisited && !m.locatedHero && !m.locatedEnemy)
                        {
                            //若距离小于最小值
                            if (LevelManager.Instance().Idx2IdxDis(mIdx, node[i]) < value)
                            {
                                to = node[i];
                                value = LevelManager.Instance().Idx2IdxDis(mIdx, node[i]);
                            }
                        }
                    }
                    if (to != NULLTARGET)
                    {
                        MainManager.Instance().curHero = levelInstance.GetMapNode(targetIdx).locatedHero;
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
                targetIdx = levelInstance.GetMapNode(attackHeroList[0]).GetID();
                MainManager.Instance().curHero = levelInstance.GetMapNode(targetIdx).locatedHero;
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
            MoveManager.Instance().ShowMoveRange(this.transform.position, rolePro.mMove, 1);
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
        levelInstance.GetMapNode(mIdx).locatedEnemy = this;
        Attack();
        MoveManager.Instance().ReSet();
    }

    public void Attack()
    {
        attackCursor = ResourcesMgr.Instance().GetPool(MainProperty.ATTACKCURSOR_PATH);
        attackCursor.transform.position = levelInstance.Idx2Pos(targetIdx);
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
        levelInstance.GetMapNode(mIdx).locatedEnemy = null;
        return EnemyManager.Instance().SetDead(listIdx);
    }

    public override void LevelUp(int add = 1)
    {
        base.LevelUp(add);
    }
}
