using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;


public class LevelUpView : UIBase {

    public Image heroImg;
    public Text careerName;
    public Text levelText;
    //data
    public Text hpText;
    public Text powerText;
    public Text skillText;
    public Text speedText;
    public Text luckyText;
    public Text pDefenseText;
    public Text mDefensepText;
    public Text strengthText;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        CurrentUIType.UIForm_LucencyType = UIFormLucenyType.ImPenetrable;
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
        HeroController hero = MainManager.Instance().curHero;
        heroImg.sprite = ResourcesMgr.Instance().LoadSprite(hero.rolePro.lImage);
        careerName.text = hero.rolePro.mCareer;
        levelText.text = hero.rolePro.mLevel.ToString();

        hpText.text = hero.rolePro.tHp.ToString();
        powerText.text = hero.rolePro.mPower.ToString();
        skillText.text = hero.rolePro.mSkill.ToString();
        speedText.text = hero.rolePro.mSpeed.ToString();
        luckyText.text = hero.rolePro.mLucky.ToString();
        pDefenseText.text = hero.rolePro.pDefense.ToString();
        mDefensepText.text = hero.rolePro.mDefense.ToString();
        strengthText.text = hero.rolePro.mStrength.ToString();
    }

    private void Clear()
    {
        EffectManager.Instance().Clear();
    }

    public void LevelUp()
    {
        HeroController hero = MainManager.Instance().curHero;
        float time = 0.7f;
        Vector3 pos = levelText.transform.localPosition - new Vector3(90, 0, 0);
        ShowBling(levelText, hero.rolePro.mLevel.ToString(), pos, time);

        float add = 0.15f;
        float time1 = 0.15f;
        Vector3 pos1 = new Vector3(90, 10, 0);
        if (hero.rolePro.tHp.ToString() != hpText.text)
        {
            ShowAddPoint(hpText, hero.rolePro.tHp.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.mPower.ToString() != powerText.text)
        {
            ShowAddPoint(powerText, hero.rolePro.mPower.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.mSkill.ToString() != skillText.text)
        {
            ShowAddPoint(skillText, hero.rolePro.mSkill.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.mSpeed.ToString() != speedText.text)
        {
            ShowAddPoint(speedText, hero.rolePro.mSpeed.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.mLucky.ToString() != luckyText.text)
        {
            ShowAddPoint(luckyText, hero.rolePro.mLucky.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.pDefense.ToString() != pDefenseText.text)
        {
            ShowAddPoint(pDefenseText, hero.rolePro.pDefense.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.mDefense.ToString() != mDefensepText.text)
        {
            ShowAddPoint(mDefensepText, hero.rolePro.mDefense.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.rolePro.mStrength.ToString() != strengthText.text)
        {
            ShowAddPoint(strengthText, hero.rolePro.mStrength.ToString(), pos1, time + time1);
            time1 += add;
        }
        //通知fightMgr结束
        StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
        {
            UIManager.Instance().CloseUIForms("LevelUp");
            FightManager.Instance().FightEnd();
        }, time1 + 2));
    }

    public void ShowBling(Text parent, string text, Vector3 pos, float time)
    {
        StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
        {
            EffectManager.Instance().AddEffect(parent.rectTransform, MainProperty.EFFECT_BLING, pos);
            parent.text = text;
        }, time));
    }

    public void ShowAddPoint(Text parent, string text, Vector3 pos, float time)
    {
        StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
        {
            EffectManager.Instance().AddEffect(parent.rectTransform, MainProperty.EFFECT_ADDPOINT, pos);
            parent.text = text;
        }, time));
    }
}
