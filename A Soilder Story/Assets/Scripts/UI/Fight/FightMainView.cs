using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;

public class FightMainView : UIBase {

    //exp
    public GameObject fightExp;
    public Slider expSlider;
    public Text expValue;
    //hero
    public Text heroName;
    public Text heroHit;
    public Text heroDmg;
    public Text heroCrt;
    public Text heroWeaponName;
    public Image heroWeaponImg;
    //hp
    public Text heroHpValue;
    public Image heroHpHead;
    public GameObject heroHpBodyContent;
    public GameObject heroHpValueContent;
    private List<GameObject> heroHpBody = new List<GameObject>();
    private List<GameObject> heroHpValueBody = new List<GameObject>();
    private HeroController curHero;
    //enemy
    public Text enemyName;
    public Text enemyHit;
    public Text enemyDmg;
    public Text enemyCrt;
    public Text enemyWeaponName;
    public Image enemyWeaponImg;

    public Text enemyHpValue;
    public Image enemyHpHead;
    public GameObject enemyHpBodyContent;
    public GameObject enemyHpValueContent;
    private List<GameObject> enemyHpBody = new List<GameObject>();
    private List<GameObject> enemyHpValueBody = new List<GameObject>();
    private EnemyController curEnemy;


    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        CurrentUIType.UIForm_LucencyType = UIFormLucenyType.ImPenetrable;
        fightExp.SetActive(false);
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

    /// <summary>
    /// 初始化hero，enemy数据
    /// </summary>
    private void Init()
    {
        curHero = MainManager.Instance().curHero;
        curEnemy = MainManager.Instance().curEnemy;

        heroName.text = curHero.rolePro.mName;
        heroHpValue.text = curHero.rolePro.cHp.ToString();
        if (curHero.curWeapon != null)
        {
            heroWeaponImg.gameObject.SetActive(true);
            heroWeaponImg.sprite = ResourcesMgr.Instance().LoadSprite(curHero.curWeapon.sprite); 
            heroWeaponName.text = curHero.curWeapon.name;
            heroDmg.text = DataManager.GetDamge(curHero, curEnemy).ToString();
            heroHit.text = DataManager.GetFightHit(curHero, curEnemy).ToString();
            heroCrt.text = DataManager.GetCrt(curHero, curHero.curWeapon).ToString();
        }
        else
        {
            heroWeaponImg.gameObject.SetActive(false);
            heroWeaponName.text = null;
            heroDmg.text = "--";
            heroHit.text = "--";
            heroCrt.text = "--";
        }

        enemyName.text = curEnemy.rolePro.mName;
        enemyHpValue.text = curEnemy.rolePro.cHp.ToString();
        if (curEnemy.curWeapon != null)
        {
            enemyWeaponImg.gameObject.SetActive(true);
            enemyWeaponImg.sprite = ResourcesMgr.Instance().LoadSprite(curEnemy.curWeapon.sprite);
            enemyWeaponName.text = curEnemy.curWeapon.name;
            enemyDmg.text = DataManager.GetDamge(curEnemy, curHero).ToString();
            enemyHit.text = DataManager.GetFightHit(curEnemy, curHero).ToString();
            enemyCrt.text = DataManager.GetCrt(curEnemy, curEnemy.curWeapon).ToString();
        }
        else
        {
            enemyWeaponImg.gameObject.SetActive(false);
            enemyWeaponName.text = null;
            enemyDmg.text = "--";
            enemyHit.text = "--";
            enemyCrt.text = "--";
        }

        //增加武器效果
        if (curEnemy.curWeapon != null && curHero.curWeapon != null)
        {
            int counter = DataManager.GetWeaponCounter(curHero.curWeapon, curEnemy.curWeapon);
            if (counter == 0)
            {
                Vector3 pos = new Vector3(heroWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(heroWeaponImg.rectTransform, MainProperty.EFFECT_UP, pos);
                EffectManager.Instance().SetEffectMove(effect, effect.transform.position + new Vector3(0, 0.1f, 0), 0.5f, -1);

                Vector3 pos1 = new Vector3(enemyWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect1 = EffectManager.Instance().AddEffect(enemyWeaponImg.rectTransform, MainProperty.EFFECT_DOWN, pos1);
                EffectManager.Instance().SetEffectMove(effect1, effect1.transform.position + new Vector3(0, 0.1f, 0), 0.5f, -1);
            }
            else if (counter == 1)
            {
                Vector3 pos = new Vector3(heroWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect = EffectManager.Instance().AddEffect(heroWeaponImg.rectTransform, MainProperty.EFFECT_DOWN, pos);
                EffectManager.Instance().SetEffectMove(effect, effect.transform.position + new Vector3(0, 0.1f, 0), 0.5f, -1);

                Vector3 pos1 = new Vector3(enemyWeaponImg.rectTransform.sizeDelta.x / 2, 0, 0);
                GameObject effect1 = EffectManager.Instance().AddEffect(enemyWeaponImg.rectTransform, MainProperty.EFFECT_UP, pos1);
                EffectManager.Instance().SetEffectMove(effect1, effect1.transform.position + new Vector3(0, 0.1f, 0), 0.5f, -1);
            }
        }
        InitHp();
    }

    /// <summary>
    /// 血条初始化
    /// </summary>
    private void InitHp()
    {
        //hero
        float width1 = heroHpHead.rectTransform.sizeDelta.x / 2;
        for (int i = 0; i < curHero.rolePro.tHp; i++)
        {
            GameObject hp = ResourcesMgr.Instance().GetPool(MainProperty.FIGHT_HP_PATH);
            float width2 = hp.GetComponent<RectTransform>().sizeDelta.x / 2;
            hp.transform.SetParent(heroHpBodyContent.transform);
            hp.transform.localScale = Vector3.one;
            hp.GetComponent<RectTransform>().localPosition = new Vector3(width1 + width2 + (width2 + width2) * i, 0, 0);
            heroHpBody.Add(hp);
        }

        GameObject head = ResourcesMgr.Instance().GetPool(MainProperty.FIGHT_HPHEADVALUE_PATH);
        head.transform.SetParent(heroHpValueContent.transform);
        head.transform.localScale = Vector3.one;
        head.transform.position = heroHpHead.rectTransform.position;
        heroHpValueBody.Add(head);
        for (int i = 0; i < curHero.rolePro.cHp; i++)
        {
            GameObject hp = ResourcesMgr.Instance().GetPool(MainProperty.FIGHT_HPBODYVALUE_PATH);
            float width2 = hp.GetComponent<RectTransform>().sizeDelta.x / 2;
            hp.transform.SetParent(heroHpValueContent.transform);
            hp.transform.localScale = Vector3.one;
            hp.GetComponent<RectTransform>().localPosition = new Vector3(width1 + width2 + (width2 + width2) * i, 0, 0);
            heroHpValueBody.Add(hp);
        }

        //enemy
        for (int i = 0; i < curEnemy.rolePro.tHp; i++)
        {
            GameObject hp = ResourcesMgr.Instance().GetPool(MainProperty.FIGHT_HP_PATH);
            float width2 = hp.GetComponent<RectTransform>().sizeDelta.x / 2;
            hp.transform.SetParent(enemyHpBodyContent.transform);
            hp.transform.localScale = Vector3.one;
            hp.GetComponent<RectTransform>().localPosition = new Vector3(width1 + width2 + (width2 + width2) * i, 0, 0);
            enemyHpBody.Add(hp);
        }

        GameObject head1 = ResourcesMgr.Instance().GetPool(MainProperty.FIGHT_HPHEADVALUE_PATH);
        head1.transform.SetParent(enemyHpValueContent.transform);
        head1.transform.localScale = Vector3.one;
        head1.transform.position = enemyHpHead.rectTransform.position;
        enemyHpValueBody.Add(head);
        for (int i = 0; i < curEnemy.rolePro.cHp; i++)
        {
            GameObject hp = ResourcesMgr.Instance().GetPool(MainProperty.FIGHT_HPBODYVALUE_PATH);
            float width2 = hp.GetComponent<RectTransform>().sizeDelta.x / 2;
            hp.transform.SetParent(enemyHpValueContent.transform);
            hp.transform.localScale = Vector3.one;
            hp.GetComponent<RectTransform>().localPosition = new Vector3(width1 + width2 + (width2 + width2) * i, 0, 0);
            enemyHpValueBody.Add(hp);
        }
    }

    /// <summary>
    /// 设置血量
    /// </summary>
    public void SetHp(bool hero)
    {
        if (hero)
        {
            heroHpValue.text = curHero.rolePro.cHp.ToString();
            if (curHero.rolePro.cHp != 0)
            {
                for (int i = heroHpValueBody.Count - 1; i >= curHero.rolePro.cHp; i--)
                {
                    ResourcesMgr.Instance().PushPool(heroHpValueBody[i], MainProperty.FIGHT_HPBODYVALUE_PATH);
                    heroHpValueBody.RemoveAt(i);
                }
            }
            else
            {
                for (int i = heroHpValueBody.Count - 1; i >= 1; i--)
                {
                    ResourcesMgr.Instance().PushPool(heroHpValueBody[i], MainProperty.FIGHT_HPBODYVALUE_PATH);
                    heroHpValueBody.RemoveAt(i);
                }
                ResourcesMgr.Instance().PushPool(heroHpValueBody[0], MainProperty.FIGHT_HPHEADVALUE_PATH);
                heroHpValueBody.Clear();
            }
        }
        else
        {
            enemyHpValue.text = curEnemy.rolePro.cHp.ToString();
            //逐渐减少效果还没加
            if (curEnemy.rolePro.cHp != 0)
            {
                for (int i = enemyHpValueBody.Count - 1; i >= curEnemy.rolePro.cHp; i--)
                {
                    ResourcesMgr.Instance().PushPool(enemyHpValueBody[i], MainProperty.FIGHT_HPBODYVALUE_PATH);
                    enemyHpValueBody.RemoveAt(i);
                }
            }
            else
            {
                for (int i = enemyHpValueBody.Count - 1; i >= 1; i--)
                {
                    ResourcesMgr.Instance().PushPool(enemyHpValueBody[i], MainProperty.FIGHT_HPBODYVALUE_PATH);
                    enemyHpValueBody.RemoveAt(i);
                }
                ResourcesMgr.Instance().PushPool(enemyHpValueBody[0], MainProperty.FIGHT_HPHEADVALUE_PATH);
                enemyHpValueBody.Clear();
            }
        }
    }

    /// <summary>
    /// 设置经验条
    /// </summary>
    public void SetExp(int exp, int add)
    {
        fightExp.SetActive(true);
        expSlider.value = exp;
        expValue.text = exp.ToString();

        if (exp + add >= 100)
        {
            add = exp + add - 100;
            expSlider.value = add;
        }
        else
            expSlider.value += add;
        expValue.text = expSlider.value.ToString();
    }

    private void Clear()
    {
        ResourcesMgr.Instance().PushPool(heroHpBody, MainProperty.FIGHT_HP_PATH);
        heroHpBody.Clear();
        if (heroHpValueBody.Count > 0)
        {
            for (int i = 1; i < heroHpValueBody.Count; i++)
            {
                ResourcesMgr.Instance().PushPool(heroHpValueBody[i], MainProperty.FIGHT_HPBODYVALUE_PATH);
            }
            ResourcesMgr.Instance().PushPool(heroHpValueBody[0], MainProperty.FIGHT_HPHEADVALUE_PATH);
            heroHpValueBody.Clear();
        } 
        heroHpValueBody.Clear();
        
        ResourcesMgr.Instance().PushPool(enemyHpBody, MainProperty.FIGHT_HP_PATH);
        enemyHpBody.Clear();
        if (enemyHpValueBody.Count > 1)
        {
            for (int i = 1; i < enemyHpValueBody.Count; i++)
            {
                ResourcesMgr.Instance().PushPool(enemyHpValueBody[i], MainProperty.FIGHT_HPBODYVALUE_PATH);
            }
            ResourcesMgr.Instance().PushPool(enemyHpValueBody[0], MainProperty.FIGHT_HPHEADVALUE_PATH);
        } 
        enemyHpValueBody.Clear();

        fightExp.SetActive(false);
        EffectManager.Instance().Clear();
    }
}
