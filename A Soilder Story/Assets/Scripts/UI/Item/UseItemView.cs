using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;
using UnityEngine.UI;

public class UseItemView : UIBase {

    public Text roleName;
    public Text hpValue;
    public Slider hpSlider;

    private int viewState;
    private HeroController curHero;
    private MapNode curNode;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
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
    /// 攻击地形时的初始化
    /// </summary>
    public void AttackInit(Character role)
    {
        curHero = MainManager.Instance().curHero;
        roleName.text = curHero.mName;
        hpValue.text = curHero.cHp.ToString();
        hpSlider.maxValue = curHero.tHp;
        hpSlider.value = curHero.cHp;
    }

    public void AttackInit(MapNode node)
    {
        curNode = node;
        roleName.text = node.fightName;
        hpValue.text = node.mLife.ToString();
        hpSlider.maxValue = node.maxHp;
        hpSlider.value = node.mLife;
    }

    private void Init()
    {
        if (MainManager.Instance().mainState == MainManager.MainState.UseItem || MainManager.Instance().mainState == MainManager.MainState.CheckLand)
        {
            curHero = MainManager.Instance().curHero;
            roleName.text = curHero.mName;
            hpValue.text = curHero.cHp.ToString();
            hpSlider.maxValue = curHero.tHp;
            hpSlider.value = curHero.cHp;

            Vector2 pos = CameraManager.Instance().GetRolePos(curHero.transform.position);
            if (pos.x < 0.5)
            {
                if (Mathf.Abs(pos.y) < 0.5)
                    this.transform.localPosition = CameraManager.Instance().World2UIPos(curHero.transform.position + new Vector3(50, -20, 0));
                else
                    this.transform.localPosition = CameraManager.Instance().World2UIPos(curHero.transform.position + new Vector3(50, 20, 0));
            }
            else
            {
                if (Mathf.Abs(pos.y) < 0.5)
                    this.transform.localPosition = CameraManager.Instance().World2UIPos(curHero.transform.position + new Vector3(-50, -20, 0));
                else
                    this.transform.localPosition = CameraManager.Instance().World2UIPos(curHero.transform.position + new Vector3(-50, 20, 0));
            }
        }
    }

    private void Clear()
    {
        if (MainManager.Instance().mainState == MainManager.MainState.UseItem)
        {
            curHero = null;
            MainManager.Instance().RegisterKeyBoardEvent();
            MainManager.Instance().CursorUpdate();
            MainManager.Instance().UpdateUIPos();
            MainManager.Instance().curHero.Standby();
            MainManager.Instance().mainState = MainManager.MainState.Normal;
        }
    }

    /// <summary>
    /// 属性更新
    /// </summary>
    public void UpdateUI(string key)
    {
        if (key == "hp")
        {
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { UpdateHp(); }, 1f));
        }
        else if (key == "node")
        {
            UpdateNode();
        }
    }


    /// <summary>
    /// 更新血量
    /// </summary>
    public void UpdateHp()
    {
        hpValue.text = curHero.cHp.ToString();
        hpSlider.value = curHero.cHp;
        if (MainManager.Instance().mainState == MainManager.MainState.UseItem)
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { UIManager.Instance().CloseUIForms("UseItem"); }, 1f));
        else if(MainManager.Instance().mainState == MainManager.MainState.CheckLand)
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { LandManager.Instance().RecureEnd(); }, 1f));
    }

    public void UpdateNode()
    {
        hpValue.text = curNode.mLife.ToString();
        hpSlider.value = curNode.mLife;
    }
}
