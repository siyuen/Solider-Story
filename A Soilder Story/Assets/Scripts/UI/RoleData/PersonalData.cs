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

    public void InitData(Character role)
    {
        powerText.text = role.rolePro.mPower.ToString();
        skillText.text = role.rolePro.mSkill.ToString();
        speedText.text = role.rolePro.mSpeed.ToString();
        luckyText.text = role.rolePro.mLucky.ToString();
        pDefenseText.text = role.rolePro.pDefense.ToString();
        mDefenseText.text = role.rolePro.mDefense.ToString();
        moveText.text = role.rolePro.mMove.ToString();

        powerValue.value = role.rolePro.mPower;
        skillValue.value = role.rolePro.mSkill;
        speedValue.value = role.rolePro.mSpeed;
        luckyValue.value = role.rolePro.mLucky;
        pDefenseValue.value = role.rolePro.pDefense;
        mDefenseValue.value = role.rolePro.mDefense;
        moveValue.value = role.rolePro.mMove;
    }
}
