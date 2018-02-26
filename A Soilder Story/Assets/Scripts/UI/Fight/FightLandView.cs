using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

public class FightLandView : UIBase
{
    public UseItemView role1View;
    public UseItemView role2View;

    private HeroController curHero;
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
        curHero = MainManager.Instance().curHero;
        curNode = MainManager.Instance().curNode;
        role1View.AttackInit(curHero);
        role2View.AttackInit(curNode);

        StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => {Attack();}, 1f));
    }

    private void Clear()
    {
        curHero.Standby();
        if(curNode.mLife == 0)
            LevelManager.Instance().ShowMap();
    }

    private void Attack()
    {
        int attack = DataManager.GetAttack(curHero, curHero.curWeapon);
        curNode.mLife -= attack;
        WeaponData weapon = curHero.curWeapon;
        int dur = DataManager.Value(weapon.durability);
        dur -= 1;
        weapon.durability = dur.ToString();
        //这里还应该考虑没有武器的情况
        if (dur <= 0)
        {
            curHero.GiveUpItem(weapon.tag);
            curHero.SetCurWeapon();
        }
        if (curNode.mLife <= 0)
            curNode.mLife = 0;
        role2View.UpdateUI("node");
        Vector3 pos = curNode.transform.position - curHero.transform.position;
        DOTween.Sequence().
                Append(curHero.transform.DOMove(curHero.transform.position + pos, 0.5f)).
                Append(curHero.transform.DOMove(curHero.transform.position, 0.5f)).
                SetLoops(1).
                OnComplete(AttackEnd);
    }

    private void AttackEnd()
    {
        StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { UIManager.Instance().CloseUIForms("FightLand"); }, 0.7f));
    }
}
