using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;

public class RoundView : UIBase
{
    public const string HEROROUND = "HERO  PHASE";
    public const string ENEMYROUND = "ENEMY  PHASE";
    public static Color HEROCOLOR = new Color(0.38f, 0.77f, 1f);
    public static Color ENEMYCOLOR = new Color(1f, 0.11f, 0.05f);

    public Text roundName;

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
        if (MainManager.Instance().heroRound)
        {
            if (EnemyManager.Instance().GetEnemyCount() == 0)
            {
                roundName.text = HEROROUND;
                roundName.color = HEROCOLOR;
                StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { MainManager.Instance().SetHeroRound(); }, 1.5f));
            }
            else
            {
                roundName.text = ENEMYROUND;
                roundName.color = ENEMYCOLOR;
                StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { MainManager.Instance().SetEnemyRound(); }, 1.5f));
            }
        }
        else
        {
            roundName.text = HEROROUND;
            roundName.color = HEROCOLOR;
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { MainManager.Instance().SetHeroRound(); }, 1.5f));
        }
    }

    private void Clear()
    {
    }
}
