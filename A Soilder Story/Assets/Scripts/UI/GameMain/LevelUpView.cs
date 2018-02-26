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
        heroImg.sprite = ResourcesMgr.Instance().LoadSprite(hero.lImage);
        careerName.text = hero.mCareer;
        levelText.text = hero.mLevel.ToString();

        hpText.text = hero.tHp.ToString();
        powerText.text = hero.mPower.ToString();
        skillText.text = hero.mSkill.ToString();
        speedText.text = hero.mSpeed.ToString();
        luckyText.text = hero.mLucky.ToString();
        pDefenseText.text = hero.pDefense.ToString();
        mDefensepText.text = hero.mDefense.ToString();
        strengthText.text = hero.mStrength.ToString();
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
        ShowBling(levelText, hero.mLevel.ToString(), pos, time);

        float add = 0.15f;
        float time1 = 0.15f;
        Vector3 pos1 = new Vector3(90, 10, 0);
        if (hero.tHp.ToString() != hpText.text)
        {
            ShowAddPoint(hpText, hero.tHp.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.mPower.ToString() != powerText.text)
        {
            ShowAddPoint(powerText, hero.mPower.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.mSkill.ToString() != skillText.text)
        {
            ShowAddPoint(skillText, hero.mSkill.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.mSpeed.ToString() != speedText.text)
        {
            ShowAddPoint(speedText, hero.mSpeed.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.mLucky.ToString() != luckyText.text)
        {
            ShowAddPoint(luckyText, hero.mLucky.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.pDefense.ToString() != pDefenseText.text)
        {
            ShowAddPoint(pDefenseText, hero.pDefense.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.mDefense.ToString() != mDefensepText.text)
        {
            ShowAddPoint(mDefensepText, hero.mDefense.ToString(), pos1, time + time1);
            time1 += add;
        }
        if (hero.mStrength.ToString() != strengthText.text)
        {
            ShowAddPoint(strengthText, hero.mStrength.ToString(), pos1, time + time1);
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
