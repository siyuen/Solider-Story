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
        this.gameObject.SetActive(true);
        if (MainManager.Instance().curMouseHero)
            bgImage.sprite = ResourcesMgr.Instance().LoadResource<Sprite>(MainProperty.HERO_BG, true);
        else if (MainManager.Instance().curMouseEnemy)
            bgImage.sprite = ResourcesMgr.Instance().LoadResource<Sprite>(MainProperty.ENEMY_BG, true);

        mTransform.position = hidePos;
        this.transform.DOMove(showPos, 0.5f);
    }

    public override void Hiding()
    {
        this.transform.DOMove(hidePos, 0.5f);
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData()
    {
        if (MainManager.Instance().curMouseHero)
            bgImage.sprite = ResourcesMgr.Instance().LoadResource<Sprite>(MainProperty.HERO_BG, true);
        else if (MainManager.Instance().curMouseEnemy)
            bgImage.sprite = ResourcesMgr.Instance().LoadResource<Sprite>(MainProperty.ENEMY_BG, true);
    }
}
