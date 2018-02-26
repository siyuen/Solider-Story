using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;
using QFramework;

public class CharacterDataView_2 : UIBase
{
    public Image bgImage;
    public Image headImage;
    public Text nameText;
    public Text hpText;
    public Slider hpSlider;

    private RectTransform mTransform;
    private RectTransform bgRect;
    private Vector3 hidePos;
    private Vector3 showPos;
    //显示需要移动的pos
    private float offset;
    //role
    private HeroController curHero;
    private EnemyController curEnemy;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        mTransform = this.GetComponent<RectTransform>();
        offset = 2.2f;
        bgRect = bgImage.transform.GetComponent<RectTransform>();
        hidePos = UIManager.Instance().GetUIDefaultPos(6) + new Vector3(-bgRect.rect.width / 2, bgRect.rect.height / 2 + offset * 10, 0); ;
        hidePos = hidePos / 100;
        showPos = hidePos + new Vector3(offset, 0, 0);
    }

    public override void Display()
    {
        base.Display();
        mTransform.position = hidePos;
        this.transform.DOMove(showPos, 0.5f);
    }

    public override void Hiding()
    {
        this.transform.DOMove(hidePos, 0.5f);
        base.Hiding();
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData()
    {
        if (MainManager.Instance().curMouseHero)
        {
            curHero = MainManager.Instance().curMouseHero;
            bgImage.sprite = ResourcesMgr.Instance().LoadSprite(MainProperty.HERO_BG);
            headImage.sprite = ResourcesMgr.Instance().LoadSprite(curHero.sImage);
            nameText.text = MainManager.Instance().curMouseHero.mName;
            hpText.text = curHero.cHp.ToString() + "/" + curHero.tHp.ToString();
            hpSlider.maxValue = curHero.tHp;
            hpSlider.value = curHero.cHp;
        }
        else if (MainManager.Instance().curMouseEnemy)
        {
            curEnemy = MainManager.Instance().curMouseEnemy;
            bgImage.sprite = ResourcesMgr.Instance().LoadSprite(MainProperty.ENEMY_BG);
            headImage.sprite = ResourcesMgr.Instance().LoadSprite(curEnemy.sImage);
            nameText.text = curEnemy.mName;
            hpText.text = curEnemy.cHp.ToString() + "/" + curEnemy.tHp.ToString();
            hpSlider.maxValue = curEnemy.tHp;
            hpSlider.value = curEnemy.cHp;

        }
    }
}
