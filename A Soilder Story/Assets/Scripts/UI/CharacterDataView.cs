using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;
using QFramework;

public class CharacterDataView : UIBase
{
    public const string HERO_BG = "Sprites/UI/HeroDataBg";
    public const string ENEMY_BG = "Sprites/UI/EnemyDataBg";

    public Image bgImage;
    public Image headImage;
    public Text nameText;
    public Text hpText;
    public Slider hpSlider;

    private RectTransform mTransform;
    private RectTransform bgRect;
    //两种情况的pos
    private Vector3 hidePos1;
    private Vector3 showPos1;
    private Vector3 hidePos2;
    private Vector3 showPos2;
    //显示需要移动的pos
    private float offset;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        mTransform = this.GetComponent<RectTransform>();
        offset = 2.2f;
        bgRect = bgImage.transform.GetComponent<RectTransform>();
        hidePos1 = UIManager.Instance().GetUIDefaultPos(0) + new Vector3(-bgRect.rect.width / 2, -bgRect.rect.height / 2 - offset * 10, 0);
        hidePos2 = UIManager.Instance().GetUIDefaultPos(6) + new Vector3(-bgRect.rect.width / 2, bgRect.rect.height / 2 + offset * 10, 0);;
        //不太懂怎么设计好，这里用的正交摄像机，用doTween移动时给的实参pos是以camera的size来给的
        //但是直接设置ui的pos时是canvas的坐标，这两个pos的坐标刚好是100倍的差距？不知道是不是因为
        //1个unity单位等于100个像素这个设置的原因，所以暂时在这里写死。
        hidePos1 = hidePos1 / 100;
        showPos1 = hidePos1 + new Vector3(offset, 0, 0);
        hidePos2 = hidePos2 / 100;
        showPos2 = hidePos2 + new Vector3(offset, 0, 0);
    }

    public override void Display()
    {
        this.gameObject.SetActive(true);
        int y = 0;
        int yNode = 0;
        if (MainManager.Instance().curMouseHero)
        {
            y = (int)MainManager.Instance().Idx2ListPos(MainManager.Instance().curMouseHero.mID).y;
            yNode = MainManager.Instance().GetYNode();
            bgImage.sprite = ResourcesMgr.Instance().LoadResource<Sprite>(HERO_BG, true);
        }
        else if (MainManager.Instance().curMouseEnemy)
        {
            y = (int)MainManager.Instance().Idx2ListPos(MainManager.Instance().curMouseEnemy.mID).y;
            yNode = MainManager.Instance().GetYNode();
            bgImage.sprite = ResourcesMgr.Instance().LoadResource<Sprite>(ENEMY_BG, true);
        }
        
        if (y < yNode / 2)
        {
            mTransform.position = hidePos1;
            this.transform.DOMove(showPos1, 0.5f);
        }
        else
        {
            mTransform.position = hidePos2;
            this.transform.DOMove(showPos2, 0.5f);
        }
    }

    public override void Hiding()
    {
        int y = 0;
        int yNode = 0;
        if (MainManager.Instance().curMouseHero)
        {
            y = (int)MainManager.Instance().Idx2ListPos(MainManager.Instance().curMouseHero.mID).y;
            yNode = MainManager.Instance().GetYNode();
        }
        else
        {
            y = (int)MainManager.Instance().Idx2ListPos(MainManager.Instance().curMouseEnemy.mID).y;
            yNode = MainManager.Instance().GetYNode();
        }
        if (y < yNode / 2)
        {
            this.transform.DOMove(hidePos1, 0.5f);
        }
        else
        {
            this.transform.DOMove(hidePos2, 0.5f);
        }   
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData()
    {
    }
}
