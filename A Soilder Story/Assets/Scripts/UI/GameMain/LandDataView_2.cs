using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;

public class LandDataView_2 : UIBase
{
    public Text nameText;
    public GameObject objValue;
    public Text defText;
    public Text avoText;
    //特殊地形
    public GameObject objCrack;
    public Text crackLife;

    private RectTransform mTransform;
    private Vector3 showPos;
    private Vector3 hidePos;
    private float offset;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.PopUp;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        mTransform = this.GetComponent<RectTransform>();

        offset = 1.9f;
        hidePos = UIManager.Instance().GetUIDefaultPos(8) + new Vector3(mTransform.rect.width / 2, mTransform.rect.height / 2, 0);
        hidePos = hidePos / 100;
        showPos = hidePos - new Vector3(offset, 0, 0);
    }

    public override void Display()
    {
        base.Display();
        mTransform.position = hidePos;
        mTransform.DOMove(showPos, 0.5f);
        UpdateData();
    }

    public override void Hiding()
    {
        mTransform.DOMove(hidePos, 0.5f);
        base.Hiding();
    }

    /// <summary>
    /// 更新mapNode数据
    /// </summary>
    public void UpdateData()
    {
        MapNode node = MainManager.Instance().curNode;
        if (node == null)
            return;
        if (node.TileType != "Crack")
        {
            objValue.SetActive(true);
            objCrack.SetActive(false);
            nameText.text = node.mName;
            if (node.mdef == LevelManager.NULLNODE)
                defText.text = "";
            else
                defText.text = node.mdef.ToString();
            if (node.mAvo == LevelManager.NULLNODE)
                avoText.text = "";
            else
                avoText.text = node.mAvo.ToString();
        }
        else
        {
            objValue.SetActive(false);
            objCrack.SetActive(true);
            crackLife.text = node.mLife.ToString();
        }
    }
}
