using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using QFramework;

public class ChangeItemView : UIBase {

    //selectView
    public GameObject selectView;
    public Image topImg;
    public Image contentImg;
    public Image bottomImg;
    public Text heroName;
    public Image heroImg;
    public GridLayoutGroup itemContent;
    public GameObject tipsObj;
    //changeView
    public ChangeView changeView;

    private GameObject attackCursor;
    private List<int> heroList;
    private HeroController cursorHero;
    private int cursorIdx;
    private HeroController selectedHero;
    private List<GameObject> itemList = new List<GameObject>();
    //初始size
    private float contentHeight;

    void Awake()
    {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
        contentHeight = contentImg.rectTransform.sizeDelta.y;
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

    private void Init()
    {
        selectView.SetActive(true);
        changeView.gameObject.SetActive(false);
        heroList = MainManager.Instance().curHero.CheckHero();
        attackCursor = ResourcesMgr.Instance().GetPool(MainProperty.ATTACKCURSOR_PATH);
        attackCursor.transform.position = LevelManager.Instance().Idx2Pos(heroList[0]);
        cursorHero = LevelManager.Instance().GetMapNode(heroList[0]).locatedHero;
        cursorIdx = 0;
        SetData();
        RegisterEvent();
    }

    private void Clear()
    {
        ResourcesMgr.Instance().PushPool(attackCursor, MainProperty.ATTACKCURSOR_PATH);
        UnRegisterEvent();
        heroList.Clear();
        cursorHero = null;
        selectedHero = null;
        ResourcesMgr.Instance().PushPool(itemList, MainProperty.ITEM2_PATH);
        itemList.Clear();
    }

    private void SetData()
    {
        tipsObj.SetActive(false);
        heroName.text = cursorHero.rolePro.mName;
        for (int i = 0; i < cursorHero.bagList.Count; i++)
        {
            GameObject item = ResourcesMgr.Instance().GetPool(MainProperty.ITEM2_PATH);
            item.transform.SetParent(itemContent.transform);
            item.transform.localScale = Vector3.one;
            item.GetComponent<Image>().sprite = ResourcesMgr.Instance().LoadSprite(cursorHero.bagList[i].sprite);
            Text name = item.transform.Find("Name").GetComponent<Text>();
            name.text = cursorHero.bagList[i].name;
            Text count = item.transform.Find("Count").GetComponent<Text>();
            count.text = cursorHero.bagList[i].durability;
            name.color = ItemManager.COLOR_USEITEM;
            count.color = ItemManager.COLOR_WEPONDATA;
            itemList.Add(item);
        }
        //计算背景显示
        //无道具则显示一行
        if (cursorHero.bagList.Count != 0)
            contentImg.rectTransform.sizeDelta = new Vector2(contentImg.rectTransform.sizeDelta.x, contentHeight * cursorHero.bagList.Count);
        else
        {
            tipsObj.SetActive(true);
            contentImg.rectTransform.sizeDelta = new Vector2(contentImg.rectTransform.sizeDelta.x, contentHeight);
        }
        contentImg.rectTransform.localPosition = new Vector3(
            contentImg.rectTransform.localPosition.x,
            -topImg.rectTransform.sizeDelta.y / 2 - contentImg.rectTransform.sizeDelta.y / 2,
            0);
        bottomImg.rectTransform.localPosition = new Vector3(
            bottomImg.rectTransform.localPosition.x,
            -topImg.rectTransform.sizeDelta.y / 2 - contentImg.rectTransform.sizeDelta.y - bottomImg.rectTransform.sizeDelta.y / 2,
            0);
    }

    private void RegisterEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancleDown, EventType.KEY_X);
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_RIGHTARROW);   
    }

    private void UnRegisterEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_RIGHTARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancleDown, EventType.KEY_X);
    }

    private void OnConfirmDown()
    {
        if (!selectedHero)
        {
            UnRegisterEvent();
            selectedHero = cursorHero;
            selectView.SetActive(false);
            changeView.gameObject.SetActive(true);
            changeView.Init(MainManager.Instance().curHero, selectedHero);
        }
    }

    private void OnCancleDown()
    {
        UIManager.Instance().CloseUIForms("ChangeItem");
        OpenUIForm("HeroMenu");
    }

    private void OnUpArrowDown()
    {
        if (!selectedHero)
        {
            cursorIdx -= 1;
            if (cursorIdx < 0)
                cursorIdx += heroList.Count;
            UpdateData();
        }
    }

    private void OnDownArrowDown()
    {
        if (!selectedHero)
        {
            cursorIdx += 1;
            if (cursorIdx >= heroList.Count)
                cursorIdx -= heroList.Count;
            UpdateData();
        }
    }

    private void UpdateData()
    {
        attackCursor.transform.position = LevelManager.Instance().Idx2Pos(heroList[cursorIdx]);
        //将上一次的item clear
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].transform.SetParent(null);
        }
        ResourcesMgr.Instance().PushPool(itemList, MainProperty.ITEM2_PATH);
        itemList.Clear();
        cursorHero = LevelManager.Instance().GetMapNode(heroList[cursorIdx]).locatedHero;
        SetData();
    }
}
