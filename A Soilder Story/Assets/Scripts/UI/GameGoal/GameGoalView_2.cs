using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;

public class GameGoalView_2 : UIBase
{
    //目标
    public Text goalText1;
    //剩余
    public Text goalText2;
    public Image bgImage1;
    public Image bgImage2;

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

    }

    public override void Display()
    {
        base.Display();

        goalText1.text = LevelManager.Instance().GetLevelTips();
        if (LevelManager.Instance().GetLevelGoal() == "allenemy")
        {
            hidePos = UIManager.Instance().GetUIDefaultPos(8) + new Vector3(bgImage1.rectTransform.sizeDelta.x / 2, bgImage1.rectTransform.sizeDelta.y / 2, 0);
            hidePos = hidePos / 100;
            showPos = hidePos + new Vector3(-offset, 0, 0);
            goalText2.text = "余:" + EnemyManager.Instance().GetEnemyCount().ToString();
            goalText2.gameObject.SetActive(true);
            bgImage1.gameObject.SetActive(true);
            bgImage2.gameObject.SetActive(false);
        }
        else if (LevelManager.Instance().GetLevelGoal() == "pos")
        {
            hidePos = UIManager.Instance().GetUIDefaultPos(8) + new Vector3(bgImage2.rectTransform.sizeDelta.x / 2, bgImage2.rectTransform.sizeDelta.y / 2, 0);
            hidePos = hidePos / 100;
            showPos = hidePos + new Vector3(-offset, 0, 0);
            goalText2.gameObject.SetActive(false);
            bgImage1.gameObject.SetActive(false);
            bgImage2.gameObject.SetActive(true);
        }
        mTransform.position = hidePos;
        mTransform.DOMove(showPos, 0.5f);
    }

    public override void Hiding()
    {
        mTransform.DOMove(hidePos, 0.5f);
        base.Hiding();
    }
}
