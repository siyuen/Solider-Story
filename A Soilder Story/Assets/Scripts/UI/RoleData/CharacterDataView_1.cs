using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;
using QFramework;

public class CharacterDataView_1 : UIBase
{
    public Image bgImage;
    public Image headImage;
    public Text nameText;
    public Text hpText;
    public Slider hpSlider;

    private RectTransform mTransform;
    private RectTransform bgRect;
    //两种情况的pos
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
        hidePos = UIManager.Instance().GetUIDefaultPos(0) + new Vector3(-bgRect.rect.width / 2, -bgRect.rect.height / 2 - offset * 10, 0);
        hidePos = hidePos / 100;
        showPos = hidePos + new Vector3(offset, 0, 0);
    }

    public override void Display()
    {
        MessageCenter.Instance().AddListener(EventType.UPDATEROLEUI, SetData);
        base.Display();
        mTransform.position = hidePos;
        this.transform.DOMove(showPos, 0.5f);
    }

    public override void Hiding()
    {
        MessageCenter.Instance().RemoveListener(EventType.UPDATEROLEUI, SetData);
        this.transform.DOMove(hidePos, 0.5f);
        base.Hiding();
    }

    /// <summary>
    /// 设置数据,text跟slider
    /// </summary>
    public void SetData(MessageEvent e)
    {
        UIRoleData data = (UIRoleData)e.Data;
        Character role = data.role;
        if (role.isHero)
            bgImage.sprite = ResourcesMgr.Instance().LoadSprite(MainProperty.HERO_BG);
        else
            bgImage.sprite = ResourcesMgr.Instance().LoadSprite(MainProperty.ENEMY_BG);
        headImage.sprite = ResourcesMgr.Instance().LoadSprite(role.rolePro.sImage);
        nameText.text = role.rolePro.mName;
        hpText.text = role.rolePro.cHp.ToString() + "/" + role.rolePro.tHp.ToString();
        hpSlider.maxValue = role.rolePro.tHp;
        hpSlider.value = role.rolePro.cHp;
    }
}
