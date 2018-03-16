using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class Character : MonoBehaviour {

    public const int BAGLIMIT = 5;
    public RolePro rolePro;                                 //人物数据
    public WeaponData curWeapon;                            //当前装备的武器
    public int listIdx;                                     //在当前list中的idx

    public GameObject fightRole;                      

    public float moveSpeed = 30f;                           //移动速度
    public Vector2 moveDir = Vector3.zero;                  //移动方向
    public Vector2 lastDir = Vector3.zero;                  //上一步方向
    public Vector2 curDir = Vector3.zero;                   //当前方向
    public string dirStr;                                   //当前方向的string
    public bool bMove;                                      //是否移动
    public bool bSelected;                                  //是否被选中
    public bool bStandby;                                   //是否待机
    public int mIdx;                                        //对应地图块的idx
    public bool isHero;  

    protected InputManager inputInstance;
    protected MainManager mainInstance;
    protected LevelManager levelInstance;
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
        levelInstance = LevelManager.Instance();
        mTransform = this.transform;
        mAnimator = mTransform.GetComponent<Animator>();
    }

    public virtual void HideMoveRange()
    {
        MoveManager.Instance().HideMoveRange();
        MoveManager.Instance().HideRoad();
    }

    public virtual void InitData(PublicRoleData data)
    {
        rolePro = new RolePro(data);
    }

    /// <summary>
    /// 升级
    /// </summary>
    public virtual void LevelUp(int add = 1)
    {
        CareerManager career = CareerManager.Instance();
        rolePro.mLevel += add;
        for (int i = 0; i < add; i++)
        {
            if (career.LevelUP(rolePro.mCareer, "hp"))
                rolePro.tHp += 1;
            if (career.LevelUP(rolePro.mCareer, "power"))
                rolePro.mPower += 1;
            if (career.LevelUP(rolePro.mCareer, "skill"))
                rolePro.mSkill += 1;
            if (career.LevelUP(rolePro.mCareer, "speed"))
                rolePro.mSpeed += 1;
            if (career.LevelUP(rolePro.mCareer, "lucky"))
                rolePro.mLucky += 1;
            if (career.LevelUP(rolePro.mCareer, "pdefense"))
                rolePro.pDefense += 1;
            if (career.LevelUP(rolePro.mCareer, "mdefense"))
                rolePro.mDefense += 1;
        }      
    }

    #region 各种公有方法（武器、检查周围等）
    /// <summary>
    /// 设置当前装备的武器
    /// </summary>
    public virtual void SetCurWeapon()
    {
        if (weaponList.Count == 0)
        {
            curWeapon = null;
            return;
        }
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
    /// 更新当前武器（用于交换时）
    /// </summary>
    public void ChangeingUpdate()
    {
        if (weaponList.Count == 0)
            curWeapon = null;
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

    /// <summary>
    /// 判断能否装备这个武器
    /// </summary>
    public virtual bool WeaponMatching(WeaponData weapon)
    {
        return CareerManager.Instance().WeaponMatching(rolePro.mCareer, weapon.key);
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
        string weapon1 = CareerManager.Instance().keyCareerDic[rolePro.mCareer].weaponkey1;
        string weapon2 = CareerManager.Instance().keyCareerDic[rolePro.mCareer].weaponkey2;
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
        int col = levelInstance.mapXNode;
        if (levelInstance.IsInMap(mIdx + 1) && levelInstance.GetMapNode(mIdx + 1).locatedHero)
            return (mIdx + 1);
        else if (levelInstance.IsInMap(mIdx + col) && levelInstance.GetMapNode(mIdx + col).locatedHero)
            return (mIdx + col);
        else if (levelInstance.IsInMap(mIdx - 1) && levelInstance.GetMapNode(mIdx - 1).locatedHero)
            return (mIdx - 1);
        else if (levelInstance.IsInMap(mIdx - col) && levelInstance.GetMapNode(mIdx - col).locatedHero)
            return (mIdx - col);
        else
            return -1;
    }

    /// <summary>
    /// 检测周围的块中是否有enemy
    /// </summary>
    public virtual int CheckIsEnemyAround()
    {
        int col = levelInstance.mapXNode;
        if (levelInstance.IsInMap(mIdx + 1) && levelInstance.GetMapNode(mIdx + 1).locatedEnemy)
            return (mIdx + 1);
        else if (levelInstance.IsInMap(mIdx + col) && levelInstance.GetMapNode(mIdx + col).locatedEnemy)
            return (mIdx + col);
        else if (levelInstance.IsInMap(mIdx - 1) && levelInstance.GetMapNode(mIdx - 1).locatedEnemy)
            return (mIdx - 1);
        else if (levelInstance.IsInMap(mIdx - col) && levelInstance.GetMapNode(mIdx - col).locatedEnemy)
            return (mIdx - col);
        return -1;
    }

    /// <summary>
    /// 检查周围是否有crack
    /// </summary>
    public virtual bool CheckIsCrackAround()
    {
        int col = levelInstance.mapXNode;
        if (levelInstance.IsInMap(mIdx + 1) && levelInstance.GetMapNode(mIdx + 1).TileType == "Crack")
        {
            if (levelInstance.GetMapNode(mIdx + 1).mLife != 0)
                return true;
            else
                return false;
        }
        else if (levelInstance.IsInMap(mIdx + col) && levelInstance.GetMapNode(mIdx + col).TileType == "Crack")
        {
            if (levelInstance.GetMapNode(mIdx + col).mLife != 0)
                return true;
            else
                return false;
        }
        else if (levelInstance.IsInMap(mIdx - 1) && levelInstance.GetMapNode(mIdx - 1).TileType == "Crack")
        {
            if (levelInstance.GetMapNode(mIdx - 1).mLife != 0)
                return true;
            else
                return false;
        }
        else if (levelInstance.IsInMap(mIdx - col) && levelInstance.GetMapNode(mIdx - col).TileType == "Crack")
        {
            if (levelInstance.GetMapNode(mIdx - col).mLife != 0)
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
        int col = levelInstance.mapXNode;
        if (levelInstance.IsInMap(mIdx + 1) && levelInstance.GetMapNode(mIdx + 1).locatedEnemy)
            enemy.Add(mIdx + 1);
        if (levelInstance.IsInMap(mIdx + col) && levelInstance.GetMapNode(mIdx + col).locatedEnemy)
            enemy.Add(mIdx + col);
        if (levelInstance.IsInMap(mIdx - 1) && levelInstance.GetMapNode(mIdx - 1).locatedEnemy)
            enemy.Add(mIdx - 1);
        if (levelInstance.IsInMap(mIdx - col) && levelInstance.GetMapNode(mIdx - col).locatedEnemy)
            enemy.Add(mIdx - col);
        return enemy;
    }

    public List<int> CheckCrack()
    {
        //范围一
        List<int> crack = new List<int>();
        int col = levelInstance.mapXNode;
        if (levelInstance.IsInMap(mIdx + 1) && levelInstance.GetMapNode(mIdx + 1).TileType == "Crack")
            crack.Add(mIdx + 1);
        if (levelInstance.IsInMap(mIdx + col) && levelInstance.GetMapNode(mIdx + col).TileType == "Crack")
            crack.Add(mIdx + col);
        if (levelInstance.IsInMap(mIdx - 1) && levelInstance.GetMapNode(mIdx - 1).TileType == "Crack")
            crack.Add(mIdx - 1);
        if (levelInstance.IsInMap(mIdx - col) && levelInstance.GetMapNode(mIdx - col).TileType == "Crack")
            crack.Add(mIdx - col);
        return crack;
    }

    public List<int> CheckHero()
    {
        //范围一
        List<int> hero = new List<int>();
        int col = levelInstance.mapXNode;
        if (levelInstance.IsInMap(mIdx + 1) && levelInstance.GetMapNode(mIdx + 1).locatedHero)
            hero.Add(mIdx + 1);
        if (levelInstance.IsInMap(mIdx + col) && levelInstance.GetMapNode(mIdx + col).locatedHero)
            hero.Add(mIdx + col);
        if (levelInstance.IsInMap(mIdx - 1) && levelInstance.GetMapNode(mIdx - 1).locatedHero)
            hero.Add(mIdx - 1);
        if (levelInstance.IsInMap(mIdx - col) && levelInstance.GetMapNode(mIdx - col).locatedHero)
            hero.Add(mIdx - col);
        return hero;
    }
    #endregion

    #region 人物移动
    public virtual void Move()
    {
        HideMoveRange();
        MapNode to = levelInstance.GetMapNode(path[path.Count - 1]);
        if (Vector2.Distance(mTransform.position, to.transform.position) > 0.5)
        {
            mTransform.Translate(moveSpeed * moveDir.normalized * Time.deltaTime);
        }
        else
        {
            mIdx = to.GetID();
            mTransform.position = to.transform.position;
            this.transform.position -= new Vector3(0, 0, 1);
            path.RemoveAt(path.Count - 1);
            if (path.Count != 0)
            {
                to = levelInstance.GetMapNode(path[path.Count - 1]);
                moveDir = to.transform.position - mTransform.position;
                ChangeDir();
            }
            else
            {
                bMove = false;
                moveDir = Vector3.zero;
                this.transform.position += new Vector3(0, 0, 1);
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
        if (curDir == Vector2.up)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bUp";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector2.right)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bRight";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector2.down)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bDown";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector2.left)
        {
            mAnimator.SetBool(dirStr, false);
            dirStr = "bLeft";
            mAnimator.SetBool(dirStr, true);
        }
        else if (curDir == Vector2.zero)
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
        if (!levelInstance.GetMapNode(to).canMove)
            return;
        //原地
        if (to == mIdx)
        {
            HideMoveRange();
            MoveDone();
            return;
        }
        path.Clear();
        MapNode parent = levelInstance.GetMapNode(to);
        while (parent.parentMapNode != null)  //通过父节点计算路径存于path中
        {
            path.Add(parent.GetID());
            parent = parent.parentMapNode;
        }
        close.Clear();
        if (path.Count == 0)
            return;
        moveDir = levelInstance.GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        //将z坐标前移
        this.transform.position -= new Vector3(0, 0, 1);
        ChangeDir();
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

        path.Clear();
        while (to.parentMapNode != null)  //通过父节点计算路径存于path中
        {
            path.Add(to.GetID());
            to = to.parentMapNode;
        }
        close.Clear();
        if (path.Count == 0)
            return;
        moveDir = levelInstance.GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        this.transform.position -= new Vector3(0, 0, 1);
        ChangeDir();
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
                        ChangeingUpdate();
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
}
