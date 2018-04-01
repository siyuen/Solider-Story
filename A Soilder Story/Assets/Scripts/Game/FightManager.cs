using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class FightManager : QMonoSingleton<FightManager> {

    public static int HEROROUND = 0;
    public static int ENEMYROUND = 1;
    public static int ATTACKLAND = 2; 
    //进攻队列
    private List<Character> attackList = new List<Character>();
    private HeroController curHero;
    private EnemyController curEnemy;

    private FightMainView fightView;
    private int curRound;
    //是否完成关卡
    private bool bSuccess;

    /// <summary>
    /// 初始化战斗
    /// </summary>
    public void Init(int round)
    {
        if (round != HEROROUND && round != ENEMYROUND && round != ATTACKLAND)
            return;
        curRound = round;
        if (round == HEROROUND || round == ENEMYROUND)
        {
            MainManager.Instance().mainState = MainManager.MainState.NormalAttack;
            HeroManager.Instance().InitFight();
            EnemyManager.Instance().InitFight();

            curHero = MainManager.Instance().curHero;
            curHero.fightRole.SetActive(true);
            curEnemy = MainManager.Instance().curEnemy;
            curEnemy.fightRole.SetActive(true);
            if (round == ENEMYROUND)
                ResourcesMgr.Instance().PushPool(curEnemy.attackCursor, MainProperty.ATTACKCURSOR_PATH);
            InitAttackList();
            UIManager.Instance().ShowUIForms("FightMain");
            fightView = UIManager.Instance().GetUI("FightMain").GetComponent<FightMainView>(); ;
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { Attack(); }, 1f));
        }
        else
        {
            MainManager.Instance().mainState = MainManager.MainState.AttackLand;
            UIManager.Instance().ShowUIForms("FightLand");
        }
    }

    private void Attack()
    {
        //武器扣耐久
        WeaponData weapon = attackList[0].curWeapon;
        int dur = DataManager.Value(weapon.durability);
        dur -= 1;
        weapon.durability = dur.ToString();
        //这里还应该考虑没有武器的情况
        if (dur <= 0)
        {
            attackList[0].GiveUpItem(weapon.tag);
            attackList[0].SetCurWeapon();
        }

        Animation anim = attackList[0].fightRole.GetComponent<Animation>();
        anim.Stop();
        anim.Play();
        int random = Random.Range(0, 100);
        if (attackList[0].isHero)
        {
            int hit = DataManager.GetHit(curHero, curHero.curWeapon);
            int dmg = DataManager.GetDamge(curHero, curEnemy);
            if (random < hit)
            {
                curEnemy.rolePro.SetProValue(RolePro.PRO_CHP, curEnemy.rolePro.cHp - dmg);
                if (curEnemy.rolePro.cHp <= 0)
                {
                    curEnemy.rolePro.SetProValue(RolePro.PRO_CHP, 0);
                    fightView.SetHp(false);
                    StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { SetExp(); }, 1f));
                    return;
                }
                else
                    fightView.SetHp(false);
            }
            else
            {
                Vector3 pos = fightView.enemyName.rectTransform.localPosition + new Vector3(200, -100, 0);
                EffectManager.Instance().AddEffect(fightView.enemyName.rectTransform, MainProperty.EFFECT_MISS, pos);
            }
        }
        else
        {
            int hit = DataManager.GetHit(curEnemy, curEnemy.curWeapon);
            int dmg = DataManager.GetDamge(curEnemy, curHero);
            if (random < hit)
            {
                curHero.rolePro.SetProValue(RolePro.PRO_CHP, curHero.rolePro.cHp - dmg);
                if (curHero.rolePro.cHp <= 0)
                {
                    curHero.rolePro.SetProValue(RolePro.PRO_CHP, 0);
                    fightView.SetHp(true);
                    StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { SetExp(); }, 1f));
                    return;
                }
                else
                    fightView.SetHp(true);
            }
            else
            {
                Vector3 pos = fightView.heroName.rectTransform.localPosition + new Vector3(-200, -100, 0);
                EffectManager.Instance().AddEffect(fightView.heroName.rectTransform, MainProperty.EFFECT_MISS, pos);
            }
        }

        attackList.RemoveAt(0);
        if (attackList.Count != 0)
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { Attack(); }, 1f));
        else
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { SetExp(); }, 1f));
    }

    /// <summary>
    /// 战斗结束，获取经验
    /// </summary>
    private void SetExp()
    {
        bSuccess = false;
        int exp = 0;
        //是否显示经验条
        bool show = false;
        //三种情况:1.enemy死亡 2.hero死亡 3.都没死亡
        if (curEnemy.rolePro.cHp <= 0)
        {
            exp = DataManager.GetExp(curHero, curEnemy, true);
            show = true;
            //判断敌人是否为空
            if (curEnemy.Dead())
            {
                if (LevelManager.Instance().IsFinish())
                    bSuccess = true;
                show = true;
            }
            curEnemy = null;
        }
        else if (curHero.rolePro.cHp <= 0)
        {
            Debug.Log(curHero.rolePro.mName);
            if (!curHero.Dead())
            {
                curHero = null;
                StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
                {
                    HideRole();
                    FightEnd();
                }, 1f));
            }
            else
                GameManager.Instance().GameOver(GameManager.PLAYERDEAD);
        }
        else
        {
            exp = DataManager.GetExp(curHero, curEnemy, false);
            show = true;
        }

        if (show)
        {
            fightView.SetExp(curHero.rolePro.mExp, exp);
            curHero.rolePro.SetProValue(RolePro.PRO_EXP, curHero.rolePro.mExp + exp);
            if (curHero.rolePro.mExp >= 100)
            {
                StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
                {
                    HideRole();
                    curHero.LevelUp();
                }, 1f));
            }
            else
            {
                StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
                {
                    HideRole();
                    FightEnd();
                }, 1f));
            }
        }
    }

    public void HideRole()
    {
        //排除死亡的情况
        if (curHero)
            curHero.Clear();
        if (curEnemy)
            curEnemy.Clear();
    }

    public void FightEnd()
    {
        if (!bSuccess)
        {
            UIManager.Instance().CloseUIForms("FightMain");
            HeroManager.Instance().FightEnd();
            EnemyManager.Instance().FightEnd();
            if (curRound == HEROROUND)
            {
                if (curHero)
                    curHero.Standby();
                else
                {
                    //判断是否全部待机了
                    if (HeroManager.Instance().IsAllStandby())
                    {
                        MainManager.Instance().HideAllUI();
                        MainManager.Instance().UnRegisterKeyBoradEvent();
                        MainManager.Instance().SetCursorActive(false);
                        UIManager.Instance().ShowUIForms("Round");
                    }
                    else
                    {
                        MainManager.Instance().SetCursorActive(true);
                        MainManager.Instance().RegisterKeyBoardEvent();
                        MainManager.Instance().CursorUpdate();
                        MainManager.Instance().UpdateUIPos();
                    }
                }
            }
            else if (curRound == ENEMYROUND)
            {
                if (curEnemy != null)
                    curEnemy.Standby();
                else
                {
                    if (EnemyManager.Instance().GetEnemyCount() != 0 && EnemyManager.Instance().standbyCount < EnemyManager.Instance().GetEnemyCount())
                        EnemyManager.Instance().SetEnemyRounnd();
                    else
                        UIManager.Instance().ShowUIForms("Round");
                }
            }
            Clear();
        }
        else
            GameManager.Instance().GameOver(GameManager.SUCCESS);
    }

    /// <summary>
    /// 当前关卡结束
    /// </summary>
    public void GameOver()
    {
        UIManager.Instance().CloseUIForms("FightMain");
        HideRole();
        HeroManager.Instance().FightEnd();
        EnemyManager.Instance().FightEnd();
        Clear();
    }

    /// <summary>
    /// 初始化进攻队列
    /// </summary>
    private void InitAttackList()
    {
        if (curRound == HEROROUND)
        {
            attackList.Add(curHero);
            if (curEnemy.curWeapon != null)
            {
                attackList.Add(curEnemy);
                if (DataManager.CanDoubleAttack(curHero, curEnemy))
                    attackList.Add(curHero);
                if (DataManager.CanDoubleAttack(curEnemy, curHero))
                    attackList.Add(curEnemy);
            }
            else
            {
                if (DataManager.CanDoubleAttack(curHero, curEnemy))
                    attackList.Add(curHero);
            }
        }
        else
        {
            attackList.Add(curEnemy);
            if (curHero.curWeapon != null)
            {
                attackList.Add(curHero);
                if (DataManager.CanDoubleAttack(curEnemy, curHero))
                    attackList.Add(curEnemy);
                if (DataManager.CanDoubleAttack(curHero, curEnemy))
                    attackList.Add(curHero);
            }
            else
            {
                if (DataManager.CanDoubleAttack(curEnemy, curHero))
                    attackList.Add(curEnemy);
            }
        }
    }

    public void Clear()
    {
        curHero = null;
        curEnemy = null;
        fightView = null;
        attackList.Clear();
        EffectManager.Instance().Clear();
    }
}
