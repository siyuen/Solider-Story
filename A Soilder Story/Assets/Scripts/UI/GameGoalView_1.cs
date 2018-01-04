using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;

public class GameGoalView_1 : UIBase {

    //目标
    public Text goalText1;
    //剩余
    public Text goalText2;

    private RectTransform mTransform;
    private Vector3 hidePos;
    private Vector3 showPos;
    //显示需要移动的pos
    private float offset;

    void Awake()
    {
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        mTransform = this.GetComponent<RectTransform>();
        offset = 3.3f;

        hidePos = UIManager.Instance().GetUIDefaultPos(2) + new Vector3(mTransform.rect.width / 2, -mTransform.rect.height / 2, 0);
        hidePos = hidePos / 100;
        showPos = hidePos + new Vector3(-offset, 0, 0);
    }

    public override void Display()
    {
        this.gameObject.SetActive(true);
        mTransform.position = hidePos;
        mTransform.DOMove(showPos, 0.5f);
    }

    public override void Hiding()
    {
        mTransform.DOMove(hidePos, 0.5f);
        this.gameObject.SetActive(false);
    }
}
