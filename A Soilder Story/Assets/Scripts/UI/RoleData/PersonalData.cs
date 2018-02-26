using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PersonalData : MonoBehaviour {

    public Text powerText;
    public Text skillText;
    public Text speedText;
    public Text luckyText;
    public Text pDefenseText;
    public Text mDefenseText;
    public Text moveText;

    public Slider powerValue;
    public Slider skillValue;
    public Slider speedValue;
    public Slider luckyValue;
    public Slider pDefenseValue;
    public Slider mDefenseValue;
    public Slider moveValue;

    public void InitData(HeroController hero)
    {
        powerText.text = hero.mPower.ToString();
        skillText.text = hero.mSkill.ToString();
        speedText.text = hero.mSpeed.ToString();
        luckyText.text = hero.mLucky.ToString();
        pDefenseText.text = hero.pDefense.ToString();
        mDefenseText.text = hero.mDefense.ToString();
        moveText.text = hero.mMove.ToString();

        powerValue.value = hero.mPower;
        skillValue.value = hero.mSkill;
        speedValue.value = hero.mSpeed;
        luckyValue.value = hero.mLucky;
        pDefenseValue.value = hero.pDefense;
        mDefenseValue.value = hero.mDefense;
        moveValue.value = hero.mMove;
    }

    public void InitData(EnemyController enemy)
    {
        powerText.text = enemy.mPower.ToString();
        skillText.text = enemy.mSkill.ToString();
        speedText.text = enemy.mSpeed.ToString();
        luckyText.text = enemy.mLucky.ToString();
        pDefenseText.text = enemy.pDefense.ToString();
        mDefenseText.text = enemy.mDefense.ToString();
        moveText.text = enemy.mMove.ToString();

        powerValue.value = enemy.mPower;
        skillValue.value = enemy.mSkill;
        speedValue.value = enemy.mSpeed;
        luckyValue.value = enemy.mLucky;
        pDefenseValue.value = enemy.pDefense;
        mDefenseValue.value = enemy.mDefense;
        moveValue.value = enemy.mMove;
    }
}
