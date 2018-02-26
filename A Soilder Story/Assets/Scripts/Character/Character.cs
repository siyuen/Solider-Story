using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class Character : MonoBehaviour {

    public const int BAGLIMIT = 5;
    //Data
    public int mID;                                         //人物Id
    public string mName;                                    //名字
    public string mCareer;                                  //职业
    public int mLevel;                                      //等级
    public int mExp;                                        //经验
    public int tHp;                                         //总血量
    public int cHp;                                         //当前血量

    public int mPower;                                      //力量
    public int mSkill;                                      //技术
    public int mSpeed;                                      //速度
    public int mLucky;                                      //幸运
    public int pDefense;                                    //守备
    public int mDefense;                                    //魔防
    public int mMove;                                       //移动
    public int mStrength;                                   //体格
    public string sImage;                                   //小头像路径
    public string lImage;                                   //大头像路径
    public string mPrefab;                                  //预制体路径
    public string fightPrefab;                              //战斗预制体路径
    public WeaponData curWeapon;                            //当前装备的武器
    public int listIdx;                                     //在当前list中的idx

    public GameObject fightRole;                      

    public float moveSpeed = 30f;                           //移动速度
    public Vector3 moveDir = Vector3.zero;                  //移动方向
    public Vector3 lastDir = Vector3.zero;                  //上一步方向
    public Vector3 curDir = Vector3.zero;                   //当前方向
    public string dirStr;                                   //当前方向的string
    //public int posIndex;                                    //当前位置index
    public int moveRange;                                   //移动范围
    public bool bMove;                                      //是否移动
    public bool bSelected;                                  //是否被选中
    public bool bStandby;                                   //是否待机
    public int mIdx;                                        //对应地图块的idx
    public int attackRange;                                 //攻击范围
    public bool isHero;  

    protected InputManager inputInstance;
    protected MainManager mainInstance;
    public Animator mAnimator;

    private Transform mTransform;
    private List<int> path = new List<int>();
    private List<int> close = new List<int>();

    //背包:武器道具分开统计
    public List<ItemData> bagList = new List<ItemData>();
    public List<WeaponData> weaponList = new List<WeaponData>();
    public List<ItemData> itemList = new List<ItemData>();
    
	// Use this for initialization
    void Awake()
    {
        inputInstance = InputManager.Instance();
        mainInstance = MainManager.Instance();
        mTransform = this.transform;
        mAnimator = mTransform.GetComponent<Animator>();
    }

    public virtual void HideMoveRange()
    {
        MoveManager.Instance().HideMoveRange();
        MoveManager.Instance().HideRoad();
    }

    /// <summary>
    /// 升级
    /// </summary>
    public virtual void LevelUp(int add = 1)
    {
        CareerManager career = CareerManager.Instance();
        mLevel += add;
        for (int i = 0; i < add; i++)
        {
            if (career.LevelUP(mCareer, "hp"))
                tHp += 1;
            if (career.LevelUP(mCareer, "power"))
                mPower += 1;
            if (career.LevelUP(mCareer, "skill"))
                mSkill += 1;
            if (career.LevelUP(mCareer, "speed"))
                mSpeed += 1;
            if (career.LevelUP(mCareer, "lucky"))
                mLucky += 1;
            if (career.LevelUP(mCareer, "pdefense"))
                pDefense += 1;
            if (career.LevelUP(mCareer, "mdefense"))
                mDefense += 1;
        }      
    }

    #region 各种公有方法（武器、检查周围等）
    /// <summary>
    /// 设置当前装备的武器
    /// </summary>
    public virtual void SetCurWeapon()
    {
        if (weaponList.Count == 0)
            return;
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (WeaponMatching(weaponList[i]))
            {
                curWeapon = weaponList[i];
                return;
            }
        }
    }

    /// <summary>
    /// 更新武器，用于交换过后或者武器损坏
    /// </summary>
    public virtual void CurWeaponUpdate()
    {
        if (weaponList.Count == 0)
            curWeapon = null;
        else
        {
            //当前武器不为null，需要判断是否还存在这个武器,不存在就更新列表看看是否有匹配的武器
            if (curWeapon != null)
            {
                bool have = false;
                for (int i = 0; i < weaponList.Count; i++)
                {
                    if (weaponList[i].tag == curWeapon.tag)
                        have = true;
                }
                if (have)
                    return;
                else
                {
                    for (int i = 0; i < weaponList.Count; i++)
                    {
                        if (WeaponMatching(weaponList[i]))
                        {
                            curWeapon = weaponList[i];
                            return;
                        }
                    }
                    curWeapon = null;
                }
            }
            //当前武器为null则直接更新列表看看是否有匹配的武器
            else
            {
                for (int i = 0; i < weaponList.Count; i++)
                {
                    if (WeaponMatching(weaponList[i]))
                    {
                        curWeapon = weaponList[i];
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 判断能否装备这个武器
    /// </summary>
    public virtual bool WeaponMatching(WeaponData weapon)
    {
        string weapon1 = CareerManager.Instance().keyCareerDic[mCareer].weaponkey1;
        string weapon2 = CareerManager.Instance().keyCareerDic[mCareer].weaponkey2;
        if (weapon.key == weapon1 || weapon.key == weapon2)
            return true;
        else
            return false;
    }

    public virtual bool WeaponMatching(string tag)
    {
        int idx = -1;
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i].tag == tag)
            {
                idx = i;
                break;
            }
        }
        if (idx == -1)
            return false;
        WeaponData weapon = weaponList[idx];
        string weapon1 = CareerManager.Instance().keyCareerDic[mCareer].weaponkey1;
        string weapon2 = CareerManager.Instance().keyCareerDic[mCareer].weaponkey2;
        if (weapon.key == weapon1 || weapon.key == weapon2)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 检测周围的块中是否有hero
    /// </summary>
    public virtual int CheckIsHeroAround()
    {
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mIdx + 1) && mainInstance.GetMapNode(mIdx + 1).locatedHero)
            return (mIdx + 1);
        else if (mainInstance.IsInMap(mIdx + col) && mainInstance.GetMapNode(mIdx + col).locatedHero)
            return (mIdx + col);
        else if (mainInstance.IsInMap(mIdx - 1) && mainInstance.GetMapNode(mIdx - 1).locatedHero)
            return (mIdx - 1);
        else if (mainInstance.IsInMap(mIdx - col) && mainInstance.GetMapNode(mIdx - col).locatedHero)
            return (mIdx - col);
        else
            return -1;
    }

    /// <summary>
    /// 检测周围的块中是否有enemy
    /// </summary>
    public virtual int CheckIsEnemyAround()
    {
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mIdx + 1) && mainInstance.GetMapNode(mIdx + 1).locatedEnemy)
            return (mIdx + 1);
        else if (mainInstance.IsInMap(mIdx + col) && mainInstance.GetMapNode(mIdx + col).locatedEnemy)
            return (mIdx + col);
        else if (mainInstance.IsInMap(mIdx - 1) && mainInstance.GetMapNode(mIdx - 1).locatedEnemy)
            return (mIdx - 1);
        else if (mainInstance.IsInMap(mIdx - col) && mainInstance.GetMapNode(mIdx - col).locatedEnemy)
            return (mIdx - col);
        return -1;
    }

    /// <summary>
    /// 检查周围是否有crack
    /// </summary>
    public virtual bool CheckIsCrackAround()
    {
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mIdx + 1) && mainInstance.GetMapNode(mIdx + 1).TileType == "Crack")
        {
            if (mainInstance.GetMapNode(mIdx + 1).mLife != 0)
                return true;
            else
                return false;
        }
        else if (mainInstance.IsInMap(mIdx + col) && mainInstance.GetMapNode(mIdx + col).TileType == "Crack")
        {
            if (mainInstance.GetMapNode(mIdx + col).mLife != 0)
                return true;
            else
                return false;
        }
        else if (mainInstance.IsInMap(mIdx - 1) && mainInstance.GetMapNode(mIdx - 1).TileType == "Crack")
        {
            if (mainInstance.GetMapNode(mIdx - 1).mLife != 0)
                return true;
            else
                return false;
        }
        else if (mainInstance.IsInMap(mIdx - col) && mainInstance.GetMapNode(mIdx - col).TileType == "Crack")
        {
            if (mainInstance.GetMapNode(mIdx - col).mLife != 0)
                return true;
            else
                return false;
        }
        return false;
    }

    /// <summary>
    /// 检测攻击范围内的敌人:根据攻击type
    /// </summary>
    public List<int> CheckEnemy()
    {
        //范围一
        List<int> enemy = new List<int>();
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mIdx + 1) && mainInstance.GetMapNode(mIdx + 1).locatedEnemy)
            enemy.Add(mIdx + 1);
        if (mainInstance.IsInMap(mIdx + col) && mainInstance.GetMapNode(mIdx + col).locatedEnemy)
            enemy.Add(mIdx + col);
        if (mainInstance.IsInMap(mIdx - 1) && mainInstance.GetMapNode(mIdx - 1).locatedEnemy)
            enemy.Add(mIdx - 1);
        if (mainInstance.IsInMap(mIdx - col) && mainInstance.GetMapNode(mIdx - col).locatedEnemy)
            enemy.Add(mIdx - col);
        return enemy;
    }

    public List<int> CheckCrack()
    {
        //范围一
        List<int> crack = new List<int>();
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mIdx + 1) && mainInstance.GetMapNode(mIdx + 1).TileType == "Crack")
            crack.Add(mIdx + 1);
        if (mainInstance.IsInMap(mIdx + col) && mainInstance.GetMapNode(mIdx + col).TileType == "Crack")
            crack.Add(mIdx + col);
        if (mainInstance.IsInMap(mIdx - 1) && mainInstance.GetMapNode(mIdx - 1).TileType == "Crack")
            crack.Add(mIdx - 1);
        if (mainInstance.IsInMap(mIdx - col) && mainInstance.GetMapNode(mIdx - col).TileType == "Crack")
            crack.Add(mIdx - col);
        return crack;
    }

    public List<int> CheckHero()
    {
        //范围一
        List<int> hero = new List<int>();
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mIdx + 1) && mainInstance.GetMapNode(mIdx + 1).locatedHero)
            hero.Add(mIdx + 1);
        if (mainInstance.IsInMap(mIdx + col) && mainInstance.GetMapNode(mIdx + col).locatedHero)
            hero.Add(mIdx + col);
        if (mainInstance.IsInMap(mIdx - 1) && mainInstance.GetMapNode(mIdx - 1).locatedHero)
            hero.Add(mIdx - 1);
        if (mainInstance.IsInMap(mIdx - col) && mainInstance.GetMapNode(mIdx - col).locatedHero)
            hero.Add(mIdx - col);
        return hero;
    }
    #endregion

    #region 人物移动
    public virtual void Move()
    {
        HideMoveRange();
        MapNode to = mainInstance.GetMapNode(path[path.Count - 1]);
        if (Vector3.Distance(mTransform.position, to.transform.position) > 0.5)
        {
            mTransform.Translate(moveSpeed * moveDir.normalized * Time.deltaTime);
            ChangeDir();
        }
        else
        {
            mIdx = to.GetID();
            mTransform.position = to.transform.position;
            //Index = path[path.Count - 1].ID;  //记录当前idx
            path.RemoveAt(path.Count - 1);
            if (path.Count != 0)
            {
                to = mainInstance.GetMapNode(path[path.Count - 1]);
                moveDir = to.transform.position - mTransform.position;
            }
            else
            {
                bMove = false;
                moveDir = Vector3.zero;

                //if (levelInstance.playerRound)
                //    levelInstance.playerRound = false;
                //else
                //    levelInstance.playerRound = true;
                MoveDone();
            }
        }
    }

    public void ChangeDir()
    {
        lastDir = curDir;
        curDir = moveDir.normalized;
        if (lastDir == curDir)
            return;
        if (curDir == Vector3.up)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bUp";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector3.right)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bRight";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector3.down)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bDown";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector3.left)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bLeft";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector3.zero)
        {
            //mAnimator.SetBool(dirStr, false);
        }
    }

    public virtual void MoveDone()
    {
        bSelected = false;
        curDir = Vector3.zero;
    }

    public virtual void MoveTo(int to)
    {
        if (!mainInstance.GetMapNode(to).canMove)
            return;
        //原地
        if (to == mIdx)
        {
            HideMoveRange();
            MoveDone();
            return;
        }

        //int from = mainInstance.Pos2Idx(this.transform.position);
        //DoAStar(from, to);  //所有的移动路径存在colse中
        path.Clear();
        //int parentIdx = close[close.Count - 1];
        //MapNode parent = mainInstance.GetMapNode(parentIdx);
        MapNode parent = mainInstance.GetMapNode(to);
        while (parent.parentMapNode != null)  //通过父节点计算路径存于path中
        {
            path.Add(parent.GetID());
            parent = parent.parentMapNode;
        }
        close.Clear();
        if (path.Count == 0)
            return;
        moveDir = mainInstance.GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        bMove = true;
    }

    public virtual void MoveTo(MapNode to)
    {
        if (!to.canMove)
            return;
        //原地
        if (to.GetID() == mIdx)
        {
            HideMoveRange();
            MoveDone();
            return;
        }

        //int from = mainInstance.Pos2Idx(this.transform.position);
        //DoAStar(from, to.GetID());  //所有的移动路径存在colse中
        path.Clear();
        //int parentIdx = close[close.Count - 1];
        //MapNode parent = mainInstance.GetMapNode(parentIdx);
        while (to.parentMapNode != null)  //通过父节点计算路径存于path中
        {
            path.Add(to.GetID());
            to = to.parentMapNode;
        }
        close.Clear();
        if (path.Count == 0)
            return;
        moveDir = mainInstance.GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        bMove = true;
    }
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        mAnimator.SetBool("bStandby", false);
        mAnimator.SetBool("bNormal", true);
    }

    /// <summary>
    /// 待机
    /// </summary>
    public virtual void Standby()
    {
        bStandby = true;
        mAnimator.SetBool(dirStr, false);
        mAnimator.SetBool("bStandby", true);
    }

    /// <summary>
    /// 设置动画s
    /// </summary>
    public void SetAnimator(string name, bool b)
    {
        mAnimator.SetBool(name, b);
    }

    /// <summary>
    /// 增加物品,默认添加到背包
    /// </summary>
    public virtual void AddItem(ItemData item, bool bag = true)
    {
        if (bagList.Count < BAGLIMIT)
        {
            if (bag)
                bagList.Add(item);
            itemList.Add(item);
        }
    }

    public virtual void AddItem(WeaponData item, bool bag = true)
    {
        if (bagList.Count < BAGLIMIT)
        {
            if (bag)
                bagList.Add(item);
            weaponList.Add(item);
        }
    }

    /// <summary>
    /// 销毁item,默认清理背包中的
    /// </summary>
    public virtual void GiveUpItem(string tag, bool bag = true)
    {
        if(bag)
        {
            for (int i = 0; i < bagList.Count; i++)
            {
                if (tag == bagList[i].tag)
                    bagList.RemoveAt(i);
            }
        }
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (tag == weaponList[i].tag)
            {
                weaponList.RemoveAt(i);
                if (curWeapon != null)
                {
                    if (tag == curWeapon.tag)
                        UpdateCurWeapon();
                }
                return;
            }
        }
        for (int i = 0; i < itemList.Count; i++)
        {
            if (tag == itemList[i].tag)
            {
                itemList.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// 更新当前武器
    /// </summary>
    public void UpdateCurWeapon()
    {
        Debug.Log("更新当前武器");
        if (weaponList.Count == 0)
            curWeapon = null;
        else
        {
            for (int i = 0; i < weaponList.Count; i++)
            {
                if (WeaponMatching(weaponList[i]))
                {
                    curWeapon = weaponList[i];
                    ItemData item = bagList[0];
                    bagList[0] = curWeapon;
                    bagList.Add(item);
                    return;
                }
            }
        }
    }
}
