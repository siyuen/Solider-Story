using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UIFramework;
using DG.Tweening;

public class ChangeView : MonoBehaviour {

    public const int NULLROLE = 0;
    public const int ROLE1 = 1;
    public const int ROLE2 = 2;
    //role1
    public Text heroName1;
    public Image heroImg1;
    public GridLayoutGroup itemContent1;
    private List<GameObject> itemList1 = new List<GameObject>();
    private HeroController hero1;
    //role2
    public Text heroName2;
    public Image heroImg2;
    public GridLayoutGroup itemContent2;
    private List<GameObject> itemList2 = new List<GameObject>();
    private HeroController hero2;
    
    //标记两次选择的idx
    private int cursorIdx;
    //记录第一次选择的道具的role
    private int itemIdx;
    private int firstItem;
    //操作的finger
    public GameObject fingerCursor1;
    //标记选择的finger
    public GameObject fingerCursor2;
    private int curSelected;
    private bool bChangeing;
    //两个默认pos
    private Vector3 contentPos1;
    private Vector3 contentPos2;
    private Tween cursorTw;

    void Awake()
    {
        contentPos1 = itemContent1.transform.position + new Vector3(-itemContent1.cellSize.x / 100, itemContent1.cellSize.y / 200, 0);
        contentPos2 = itemContent2.transform.position + new Vector3(-itemContent2.cellSize.x / 100, itemContent2.cellSize.y / 200, 0);
    }

    public void Init(HeroController hero1, HeroController hero2)
    {
        firstItem = NULLROLE;
        bChangeing = false;
        this.hero1 = hero1;
        this.hero2 = hero2;
        cursorIdx = 0;
        fingerCursor1.SetActive(true);
        fingerCursor2.SetActive(false);
        if (hero1.bagList.Count > 0)
            curSelected = ROLE1;
        else 
            curSelected = ROLE2;
        RegisterEvent();
        SetData();
        UpdateData();
    }

    private void SetData()
    {
        heroName1.text = hero1.mName;
        heroImg1.sprite = ResourcesMgr.Instance().LoadSprite(hero1.lImage);
        for (int i = 0; i < hero1.bagList.Count; i++)
        {
            GameObject item = ResourcesMgr.Instance().GetPool(MainProperty.ITEM3_PATH);
            item.transform.SetParent(itemContent1.transform);
            item.transform.localScale = Vector3.one;
            item.GetComponent<Image>().sprite = ResourcesMgr.Instance().LoadSprite(hero1.bagList[i].sprite);
            Text name = item.transform.Find("Name").GetComponent<Text>();
            name.text = hero1.bagList[i].name;
            Text count = item.transform.Find("Count").GetComponent<Text>();
            count.text = hero1.bagList[i].durability;
            //判断一下武器是否匹配
            if (ItemManager.Instance().IsWeapon(hero1.bagList[i].name))
            {
                if (!hero1.WeaponMatching(hero1.bagList[i].tag))
                {
                    name.color = ItemManager.COLOR_CANNOTUSE;
                    count.color = ItemManager.COLOR_CANNOTUSE;
                }
                else
                {
                    name.color = ItemManager.COLOR_USEITEM;
                    count.color = ItemManager.COLOR_WEPONDATA;
                }
            }
            else
            {
                name.color = ItemManager.COLOR_USEITEM;
                count.color = ItemManager.COLOR_WEPONDATA;
            }
            itemList1.Add(item);
        }

        heroName2.text = hero2.mName;
        heroImg2.sprite = ResourcesMgr.Instance().LoadSprite(hero2.lImage);
        for (int i = 0; i < hero2.bagList.Count; i++)
        {
            GameObject item = ResourcesMgr.Instance().GetPool(MainProperty.ITEM3_PATH);
            item.transform.SetParent(itemContent2.transform);
            item.transform.localScale = Vector3.one;
            item.GetComponent<Image>().sprite = ResourcesMgr.Instance().LoadSprite(hero2.bagList[i].sprite);
            Text name = item.transform.Find("Name").GetComponent<Text>();
            name.text = hero2.bagList[i].name;
            Text count = item.transform.Find("Count").GetComponent<Text>();
            count.text = hero2.bagList[i].durability;
            if (ItemManager.Instance().IsWeapon(hero2.bagList[i].name))
            {
                if (!hero2.WeaponMatching(hero2.bagList[i].tag))
                {
                    name.color = ItemManager.COLOR_CANNOTUSE;
                    count.color = ItemManager.COLOR_CANNOTUSE;
                }
                else
                {
                    name.color = ItemManager.COLOR_USEITEM;
                    count.color = ItemManager.COLOR_WEPONDATA;
                }
            }
            else
            {
                name.color = ItemManager.COLOR_USEITEM;
                count.color = ItemManager.COLOR_WEPONDATA;
            }
            itemList2.Add(item);
        }
        if (itemList1.Count == 0)
            curSelected = ROLE2;
        if (itemList2.Count == 0)
            curSelected = ROLE1;
    }

    private void ClearData()
    {
        for (int i = 0; i < itemList1.Count; i++)
        {
            itemList1[i].transform.SetParent(null);
        }
        ResourcesMgr.Instance().PushPool(itemList1, MainProperty.ITEM3_PATH);
        itemList1.Clear();
        for (int i = 0; i < itemList2.Count; i++)
        {
            itemList2[i].transform.SetParent(null);
        }
        ResourcesMgr.Instance().PushPool(itemList2, MainProperty.ITEM3_PATH);
        itemList2.Clear();
    }

    public void Clear()
    {
        UnRegisterEvent();
        ClearData();
    }


    #region 事件
    private void RegisterEvent()
    {
        InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().RegisterKeyDownEvent(OnCancleDown, EventType.KEY_X);
        InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().RegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);
    }

    private void UnRegisterEvent()
    {
        InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
        InputManager.Instance().UnRegisterKeyDownEvent(OnCancleDown, EventType.KEY_X);
        InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
        InputManager.Instance().UnRegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);
    }

    private void OnConfirmDown()
    {
        if (!bChangeing)
        {
            //标记一哈
            fingerCursor2.SetActive(true);
            itemIdx = cursorIdx;
            if (curSelected == ROLE1)
            {
                Vector3 pos = contentPos1 - new Vector3(0, cursorIdx * itemContent1.cellSize.y / 100, 0);
                fingerCursor2.transform.position = pos;
                curSelected = ROLE2;
                cursorIdx = itemList2.Count;
                firstItem = ROLE1;
            }
            else
            {
                Vector3 pos = contentPos2 - new Vector3(0, cursorIdx * itemContent2.cellSize.y / 100, 0);
                fingerCursor2.transform.position = pos;
                curSelected = ROLE1;
                cursorIdx = itemList1.Count;
                firstItem = ROLE2;
            }
            bChangeing = true;
            UpdateData();
        }
        else
        {
            hero1.bChangeItem = true;
            if (curSelected == ROLE1)
            {
                //1给1
                if (firstItem == ROLE1)
                {
                    string tag1 = hero1.bagList[itemIdx].tag;
                    string tag2 = hero1.bagList[itemIdx].tag;
                    ItemManager.Instance().ChangeItem(hero1, tag1, hero1, tag2);
                }
                //2给1
                else if (firstItem == ROLE2)
                {
                    //判断是交换还是给予
                    if (cursorIdx >= itemList1.Count)
                    {
                        bool b = false;
                        for (int i = 0; i < hero2.weaponList.Count; i++)
                        {
                            if (hero2.bagList[itemIdx].tag == hero2.weaponList[i].tag)
                            {
                                b = true;
                                WeaponData weapon = hero2.weaponList[i]; ;
                                hero1.AddItem(weapon);
                                break;
                            }
                        }
                        if (!b)
                            hero1.AddItem(hero2.bagList[itemIdx]);
                        hero2.GiveUpItem(hero2.bagList[itemIdx].tag);
                    }
                    else
                    {
                        string tag1 = hero1.bagList[cursorIdx].tag;
                        string tag2 = hero2.bagList[itemIdx].tag;
                        ItemManager.Instance().ChangeItem(hero1, tag1, hero2, tag2);
                    }
                }
            }
            else
            {
                //1给2
                if (firstItem == ROLE1)
                {
                    if (cursorIdx >= itemList2.Count)
                    {
                        bool b = false;
                        for (int i = 0; i < hero1.weaponList.Count; i++)
                        {
                            if (hero1.bagList[itemIdx].tag == hero1.weaponList[i].tag)
                            {
                                b = true;
                                WeaponData weapon = hero1.weaponList[i];
                                hero2.AddItem(weapon);
                                break;
                            }
                        }
                        if (!b)
                            hero2.AddItem(hero1.bagList[itemIdx]);
                        hero1.GiveUpItem(hero1.bagList[itemIdx].tag);
                    }
                    else
                    {
                        string tag1 = hero1.bagList[itemIdx].tag;
                        string tag2 = hero2.bagList[cursorIdx].tag;
                        ItemManager.Instance().ChangeItem(hero1, tag1, hero2, tag2);
                    }
                }
                //2给2
                else if (firstItem == ROLE2)
                {
                    string tag1 = hero2.bagList[itemIdx].tag;
                    string tag2 = hero2.bagList[cursorIdx].tag;
                    ItemManager.Instance().ChangeItem(hero2, tag1, hero2, tag2);
                }
            }
            ClearData();
            SetData();
            OnCancleDown();
        }
    }

    private void OnCancleDown()
    {
        if (!bChangeing)
        {
            Clear();
            UIManager.Instance().CloseUIForms("ChangeItem");
            UIManager.Instance().ShowUIForms("HeroMenu");       
        }
        else
        {
            bChangeing = false;
            curSelected = firstItem;
            if (itemList1.Count == 0)
                curSelected = ROLE2;
            if (itemList2.Count == 0)
                curSelected = ROLE1;
            cursorIdx = itemIdx;
            firstItem = NULLROLE;
            fingerCursor2.SetActive(false);
            UpdateData();
        }
    }

    private void OnUpArrowDown()
    {
        if (!bChangeing)
        {
            if (curSelected == ROLE1)
            {
                cursorIdx -= 1;
                if (cursorIdx < 0)
                    cursorIdx += hero1.bagList.Count;
            }
            else
            {
                cursorIdx -= 1;
                if (cursorIdx < 0)
                    cursorIdx += hero2.bagList.Count;
            }
        }
        else
        {
            cursorIdx -= 1;
            //选择道具后,再不同对象列表会多一个空的位置
            if (firstItem == ROLE1)
            {
                if (curSelected == ROLE1)
                {
                    if (cursorIdx < 0)
                        cursorIdx += hero1.bagList.Count;
                }
                else
                {
                    if (cursorIdx < 0)
                        cursorIdx += hero2.bagList.Count + 1;
                }
            }
            else if(firstItem == ROLE2)
            {
                if (curSelected == ROLE1)
                {
                    if (cursorIdx < 0)
                        cursorIdx += hero1.bagList.Count + 1;
                }
                else
                {
                    if (cursorIdx < 0)
                        cursorIdx += hero2.bagList.Count;
                }
            }
        }
        UpdateData();
    }

    private void OnDownArrowDown()
    {
        if (!bChangeing)
        {
            if (curSelected == ROLE1)
            {
                cursorIdx += 1;
                if (cursorIdx >= hero1.bagList.Count)
                    cursorIdx -= hero1.bagList.Count;
            }
            else
            {
                cursorIdx += 1;
                if (cursorIdx >= hero2.bagList.Count)
                    cursorIdx -= hero2.bagList.Count;
            }
        }
        else
        {
            cursorIdx += 1;
            if (firstItem == ROLE1)
            {
                if (curSelected == ROLE1)
                {
                    if (cursorIdx >= hero1.bagList.Count)
                        cursorIdx -= hero1.bagList.Count;
                }
                else
                {
                    if (cursorIdx > hero2.bagList.Count)
                        cursorIdx -= (hero2.bagList.Count + 1);
                }
            }
            else if(firstItem == ROLE2)
            {
                if (curSelected == ROLE1)
                {
                    if (cursorIdx > hero1.bagList.Count)
                        cursorIdx -= (hero1.bagList.Count + 1);
                }
                else
                {
                    if (cursorIdx >= hero2.bagList.Count)
                        cursorIdx -= hero2.bagList.Count;
                }
            }
        }
        UpdateData();    
    }

    private void OnLeftArrowDown()
    {
        if (curSelected == ROLE1)
            return;
        else
        {
            if(itemList1.Count != 0)
                curSelected = ROLE1;
        }
        UpdateData();
    }

    private void OnRightArrowDown()
    {
        if (curSelected == ROLE2)
            return;
        else
        {
            if(itemList2.Count != 0)
                curSelected = ROLE2;
        }
        UpdateData();
    }

    private void UpdateData()
    {
        //当前在1
        if (curSelected == ROLE1)
        {
            //idx在道具列表内
            if (cursorIdx < itemList1.Count)
            {
                Vector3 pos = contentPos1 - new Vector3(0, cursorIdx * itemContent1.cellSize.y / 100, 0);
                fingerCursor1.transform.position = pos;
            }
            //idx不在道具列表内
            else
            {
                //未选择道具则默认为当前列表最后一个道具
                if (!bChangeing)
                {
                    Vector3 pos = contentPos1 - new Vector3(0, (itemList1.Count - 1) * itemContent1.cellSize.y / 100, 0);
                    cursorIdx = itemList1.Count - 1;
                    fingerCursor1.transform.position = pos;
                }
                else
                {
                    //如果是同一对象则不能选空位
                    if (firstItem == ROLE1)
                    {
                        Vector3 pos = contentPos1 - new Vector3(0, (itemList1.Count - 1) * itemContent1.cellSize.y / 100, 0);
                        cursorIdx = itemList1.Count - 1;
                        fingerCursor1.transform.position = pos;
                    }
                    else if(firstItem == ROLE2)
                    {
                        Vector3 pos = contentPos1 - new Vector3(0, cursorIdx * itemContent1.cellSize.y / 100, 0);
                        fingerCursor1.transform.position = pos;
                    }
                }
            }
        }
        else
        {
            if (cursorIdx < itemList2.Count)
            {
                Vector3 pos = contentPos2 - new Vector3(0, cursorIdx * itemContent2.cellSize.y / 100, 0);
                fingerCursor1.transform.position = pos;  
            }
            else
            {
                if (!bChangeing)
                {
                    Vector3 pos = contentPos2 - new Vector3(0, (itemList2.Count - 1) * itemContent2.cellSize.y / 100, 0);
                    cursorIdx = itemList2.Count - 1;
                    fingerCursor1.transform.position = pos;
                }
                else
                {
                    if (firstItem == ROLE2)
                    {
                        Vector3 pos = contentPos2 - new Vector3(0, (itemList2.Count - 1) * itemContent2.cellSize.y / 100, 0);
                        cursorIdx = itemList2.Count - 1;
                        fingerCursor1.transform.position = pos;
                    }
                    else if(firstItem == ROLE1)
                    {
                        Vector3 pos = contentPos2 - new Vector3(0, cursorIdx * itemContent2.cellSize.y / 100, 0);
                        fingerCursor1.transform.position = pos;
                    }
                }
            }
        }
        DOTween.Clear();
        cursorTw = fingerCursor1.transform.DOMove(fingerCursor1.transform.position + new Vector3(0.2f, 0, 0), 0.7f);
        cursorTw.SetLoops(-1);
    }
    #endregion
}
