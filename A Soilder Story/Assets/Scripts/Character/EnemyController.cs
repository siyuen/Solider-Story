using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using QFramework;

public class EnemyController : Character
{
    public enum EnemyState
    {
        normal,
        selcted,
        stop,
        dead
    }
    private EnemyState enemyState;
    private List<int> attackHeroList = new List<int>();
    private List<int> heroList = new List<int>();  //在攻击范围内的hero
    private int targetIdx;
	// Use this for initialization
	void Start () {
        dirStr = "bNormal";
	}

    public override void Init()
    {
        base.Init();
        enemyState = EnemyState.normal;
        mID = mainInstance.Pos2Idx(this.transform.position);
        mainInstance.GetMapNode(mID).locatedEnemy = this;
    }

	// Update is called once per frame
	void Update () {
        if (bMove)
            Move();
	}

    //搜索附近hero
    public void SearchHero()
    {
        mainInstance.DoMoveRange(this.transform.position, 2, 1);
        mainInstance.AddAttackNodeInList(1);
        attackHeroList = mainInstance.GetAttackHeroList();
        if (attackHeroList.Count > 0)
        {
            targetIdx = mainInstance.GetMapNode(attackHeroList[0]).GetID();
            MoveTo(mainInstance.GetMapNode(attackHeroList[0]).parentMapNode.GetID());
        }
        else
        {
            Standby();
        }
    }

    public override void MoveDone()
    {
        base.MoveDone();
        Attack(targetIdx);
    }

    public override void Attack(int to)
    {
        base.Attack(to);
        AttackDown();
    }

    public override void Standby()
    {
        base.Standby();
        if(EnemyManager.Instance().SetStandby())
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { MainManager.Instance().SetHeroRound(); }, 1f));
    }

    /// <summary>
    /// 鼠标进入enemy
    /// </summary>
    public void Moved(bool b)
    {
        if (b)
            UIManager.Instance().ShowUIForms("CharacterData");
        else
            UIManager.Instance().CloseUIForms("CharacterData"); 
    }
}
