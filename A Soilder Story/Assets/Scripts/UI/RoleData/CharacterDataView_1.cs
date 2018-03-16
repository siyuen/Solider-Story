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
        hidePos = UIManager.Instance().GetUIDefaultPos(0) + new Vector3(-bgRect.rect.width / 2, -bgRect.rect.height / 2 - offset * 10, 0);
        //不太懂怎么设计好，这里用的正交摄像机，用doTween移动时给的实参pos是以camera的size来给的
        //但是直接设置ui的pos时是canvas的坐标，这两个pos的坐标刚好是100倍的差距？不知道是不是因为
        //1个unity单位等于100个像素这个设置的原因，所以暂时在这里写死。
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
    /// 设置数据,text跟slider
    /// </summary>
    public void SetData()
    {
        if (MainManager.Instance().curMouseHero)
        {
            curHero = MainManager.Instance().curMouseHero;
            bgImage.sprite = ResourcesMgr.Instance().LoadSprite(MainProperty.HERO_BG);
            headImage.sprite = ResourcesMgr.Instance().LoadSprite(curHero.rolePro.sImage);
            nameText.text = curHero.rolePro.mName;
            hpText.text = curHero.rolePro.cHp.ToString() + "/" + curHero.rolePro.tHp.ToString();
            hpSlider.maxValue = curHero.rolePro.tHp;
            hpSlider.value = curHero.rolePro.cHp;
        }
        else if (MainManager.Instance().curMouseEnemy)
        {
            curEnemy = MainManager.Instance().curMouseEnemy;
            bgImage.sprite = ResourcesMgr.Instance().LoadSprite(MainProperty.ENEMY_BG);
            headImage.sprite = ResourcesMgr.Instance().LoadSprite(curEnemy.rolePro.sImage);
            nameText.text = curEnemy.rolePro.mName;
            hpText.text = curEnemy.rolePro.cHp.ToString() + "/" + curEnemy.rolePro.tHp.ToString();
            hpSlider.maxValue = curEnemy.rolePro.tHp;
            hpSlider.value = curEnemy.rolePro.cHp;
        }
    }
}
