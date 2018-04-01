using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

[RequireComponent(typeof(CharacterMove))]
public class Character : MonoBehaviour
{
    //人物数据
    public RolePro rolePro;
    //在当前list中的idx                       
    public int listIdx;    
    //背包
    protected CharacterBag roleBag;
    public WeaponData curWeapon { get { return roleBag.curWeapon; } }
    public List<ItemData> bagList { get { return roleBag.bagList; } }
    public List<ItemData> itemList { get { return roleBag.itemList; } }
    public List<WeaponData> weaponList { get { return roleBag.weaponList; } }
    //战斗模型
    public GameObject fightRole;
    //人物移动
    protected CharacterMove roleMove;
    public string dirStr { get { return roleMove.dirStr; } set { dirStr = value; } }
    //对应地图块的idx                            
    public int mIdx;                                        
    public bool isHero;
    //人物动画
    public Animator mAnimator;

    protected InputManager inputInstance;
    protected MainManager mainInstance;
    protected LevelManager levelInstance;
    
    private Transform mTransform;

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
        roleBag = new CharacterBag(rolePro);
        roleMove = this.GetComponent<CharacterMove>();
        roleMove.Init(this, mTransform);
    }

    /// <summary>
    /// 升级,默认升一级
    /// </summary>
    public virtual void LevelUp(int add = 1)
    {
        CareerManager career = CareerManager.Instance();
        rolePro.SetProValue(RolePro.PRO_LEVEL, rolePro.mLevel + 1);
        for (int i = 0; i < add; i++)
        {
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_THP))
                rolePro.SetProValue(RolePro.PRO_THP, rolePro.tHp + 1);
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_POWER))
                rolePro.SetProValue(RolePro.PRO_POWER, rolePro.mPower + 1);
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_SKILL))
                rolePro.SetProValue(RolePro.PRO_SKILL, rolePro.mSkill + 1);
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_SPEED))
                rolePro.SetProValue(RolePro.PRO_SPEED, rolePro.mSpeed + 1);
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_LUCKY))
                rolePro.SetProValue(RolePro.PRO_LUCKY, rolePro.mLucky + 1);
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_PDEFENSE))
                rolePro.SetProValue(RolePro.PRO_PDEFENSE, rolePro.pDefense + 1);
            if (career.LevelUP(rolePro.mCareer, RolePro.PRO_MDEFENSE))
                rolePro.SetProValue(RolePro.PRO_MDEFENSE, rolePro.mDefense + 1);
        }      
    }

    #region 检查周围
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
        mAnimator.SetBool(roleMove.dirStr, false);
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
    /// 移动结束
    /// </summary>
    public virtual void MoveDone() { }

    #region 背包
    /// <summary>
    /// 添加武器
    /// </summary>
    public void AddWeapon(WeaponData weapon)
    {
        roleBag.weaponList.Add(weapon);
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    public void AddItem(WeaponData weapon, bool bag = true)
    {
        roleBag.AddItem(weapon, bag);
    }

    public void AddItem(ItemData item, bool bag = true)
    {
        roleBag.AddItem(item, bag);
    }

    /// <summary>
    /// 设置当前武器
    /// </summary>
    public void SetCurWeapon()
    {
        roleBag.SetCurWeapon();
    }

    public void SetCurWeapon(int idx)
    {
        roleBag.SetCurWeapon(idx);
    }

    public void SetCurWeapon(string tag)
    {
        roleBag.SetCurWeapon(tag);
    }

    /// <summary>
    /// 清理背包
    /// </summary>
    public void ClearBag()
    {
        roleBag.ClearBag();
    }

    /// <summary>
    /// 丢弃物品
    /// </summary>
    public void GiveUpItem(string tag, bool bag = true)
    {
        if (roleBag.bagList.Count == 0)
            return;
        roleBag.GiveUpItem(tag, bag);
    }

    /// <summary>
    /// 更新武器
    /// </summary>
    public void CurWeaponUpdate()
    {
        roleBag.CurWeaponUpdate();
    }

    /// <summary>
    /// 武器匹配
    /// </summary>
    public bool WeaponMatching(string tag)
    {
        return roleBag.WeaponMatching(tag);
    }
    #endregion
}
