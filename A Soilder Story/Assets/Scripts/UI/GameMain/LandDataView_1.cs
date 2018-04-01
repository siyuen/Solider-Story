﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using DG.Tweening;

public class LandDataView_1 : UIBase
{
    public Text nameText;
    //普通地形显示
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
        hidePos = UIManager.Instance().GetUIDefaultPos(6) + new Vector3(-mTransform.rect.width / 2, mTransform.rect.height / 2, 0);
        hidePos = hidePos / 100;
        showPos = hidePos + new Vector3(offset, 0, 0);
    }

    public override void Display()
    {
        MessageCenter.Instance().AddListener(EventType.UPDATEMAPNODEUI, UpdateData);
        base.Display();
        mTransform.localPosition = hidePos * 100;
        mTransform.DOMove(showPos, 0.5f);
    }

    public override void Hiding()
    {
        MessageCenter.Instance().RemoveListener(EventType.UPDATEMAPNODEUI, UpdateData);
        mTransform.DOMove(hidePos, 0.5f);
        base.Hiding();
    }

    /// <summary>
    /// 更新mapNode数据
    /// </summary>
    public void UpdateData(MessageEvent e)
    {
        UIMapNodeData data = (UIMapNodeData)e.Data;
        MapNode node = data.node;
        if (node == null)
            return;
        if (node.TileType != "Crack")
        {
            objValue.SetActive(true);
            objCrack.SetActive(false);
            nameText.text = node.mName;
            if (node.mDef == LevelManager.NULLNODE)
                defText.text = "";
            else
                defText.text = node.mDef.ToString();
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
