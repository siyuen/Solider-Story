using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class Character : MonoBehaviour {
                
    public float moveSpeed = 5f;                            //移动速度
    public Vector3 moveDir = Vector3.zero;                  //移动方向
    public Vector3 lastDir = Vector3.zero;                  //上一步方向
    public Vector3 curDir = Vector3.zero;                   //当前方向
    public string dirStr;                                   //当前方向的string
    public int posIndex;                                    //当前位置index
    public int moveRange;                                   //移动范围
    public bool bMove;                                      //是否移动
    public bool bSelected;                                  //是否被选中
    public bool bStandby;                                   //是否待机
    public int mID;                                         //对应地图块的idx
    public int attackRange;                                 //攻击范围

    protected InputManager inputInstance;
    protected MainManager mainInstance;
    protected Animator mAnimator;

    private Transform mTransform;
    private List<int> path = new List<int>();
    private List<int> close = new List<int>();
    private int[] fourDic = new int[4];       //四个方位的node
    
	// Use this for initialization
    void Awake()
    {
        inputInstance = InputManager.Instance();
        mainInstance = MainManager.Instance();
        mTransform = this.transform;
        mAnimator = mTransform.GetComponent<Animator>();
    }

    public virtual void ShowMoveRange()
    {
        MoveManager.Instance().ShowMoveRange(this.transform.position, 3, 1);
    }

    public virtual void HideMoveRange()
    {
        MoveManager.Instance().HideMoveRange();
        MoveManager.Instance().HideRoad();
    }

    /// <summary>
    /// 检测周围的块中是否有hero
    /// </summary>
    public virtual int CheckIsHeroAround()
    {
        if (mainInstance.GetMapNode(mID + 1).locatedHero)
        {
            return (mID + 1);
        }
        else
            return -1;
    }

    /// <summary>
    /// 检测周围的块中是否有enemy
    /// </summary>
    public virtual int CheckIsEnemyAround()
    {
        int col = mainInstance.GetXNode();
        if (mainInstance.IsInMap(mID + 1) && mainInstance.GetMapNode(mID + 1).locatedEnemy)
            return (mID + 1);
        else if (mainInstance.IsInMap(mID + col) && mainInstance.GetMapNode(mID + col).locatedEnemy)
            return (mID + col);
        else if (mainInstance.IsInMap(mID - 1) && mainInstance.GetMapNode(mID - 1).locatedEnemy)
            return (mID - 1);
        else if (mainInstance.IsInMap(mID - col) && mainInstance.GetMapNode(mID - col).locatedEnemy)
            return (mID - col);
        return -1;
    }

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
            mID = to.GetID();
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
        if (to == mID)
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
        if (to.GetID() == mID)
        {
            HideMoveRange();
            MoveDone();
            return;
        }

        int from = mainInstance.Pos2Idx(this.transform.position);
        //DoAStar(from, to.GetID());  //所有的移动路径存在colse中
        path.Clear();
        //int parentIdx = close[close.Count - 1];
        //MapNode parent = mainInstance.GetMapNode(parentIdx);
        while (to.parentMapNode != null)  //通过父节点计算路径存于path中
        {
            path.Add(to.GetID());
            Debug.Log(to.GetID());
            to = to.parentMapNode;
        }
        close.Clear();
        if (path.Count == 0)
            return;
        moveDir = mainInstance.GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        bMove = true;
    }

    public void DoAStar(int from, int to)
    {
        path.Add(from);
        int col = mainInstance.GetXNode();
        while (path.Count != 0)
        {
            float min = Mathf.Infinity;
            int minPathIndex = 0;       //最小值在path的index
            int minIndex = 0;           //最小值的index
            //取path中fn值最小的节点
            for (int i = 0; i < path.Count; i++)
            {
                int fn = DoFn(path[i], from, to);
                if (fn < min)
                {
                    minPathIndex = i;
                    min = fn;
                }
            }
            minIndex = path[minPathIndex];
            //放入close列表
            close.Add(path[minPathIndex]);

            //如果minIndex是目标节点则break
            if (close[close.Count - 1] == to)
                break;
            else
            {
                //右
                if (mainInstance.IsInMap(minIndex + 1) && mainInstance.GetMapNode(minIndex + 1).bVisited && (minIndex + 1) / col == minIndex / col)
                    fourDic[0] = minIndex + 1;
                else
                    fourDic[0] = -1;
                //下
                if (mainInstance.IsInMap(minIndex - col) && mainInstance.GetMapNode(minIndex - col).bVisited)
                    fourDic[1] = minIndex - col;
                else
                    fourDic[1] = -1;
                //左
                if (mainInstance.IsInMap(minIndex - 1) && mainInstance.GetMapNode(minIndex - 1).bVisited && (minIndex - 1) / col == minIndex / col)
                    fourDic[2] = minIndex - 1;
                else
                    fourDic[2] = -1;
                //上
                if (mainInstance.IsInMap(minIndex + col) && mainInstance.GetMapNode(minIndex + col).bVisited)
                    fourDic[3] = minIndex + col;
                else
                    fourDic[3] = -1;

                for (int i = 0; i < 4; i++)
                {
                    if (!path.Contains(fourDic[i]) && !close.Contains(fourDic[i]) && fourDic[i] != -1)
                    {
                        //不在path列表里 且不在 close列表里
                        int idx = fourDic[i];
                        int pidx = path[minPathIndex];
                        mainInstance.GetMapNode(idx).parentMapNode = mainInstance.GetMapNode(pidx);
                        mainInstance.GetMapNode(idx).bVisited = true;
                        path.Add(fourDic[i]);
                    }
                }
            }
            path.RemoveAt(minPathIndex);
        }
    }

    private int DoFn(int index, int from, int to)
    {
        //计算gn
        int col = mainInstance.GetXNode();
        int a = Mathf.Abs(index - from);
        int gn;
        if (a / col == 0 || a % col == 0)
            gn = Mathf.Abs(a / col - a % col) * 10;
        else
            gn = a / col * 14 + Mathf.Abs(a / col - a % col) * 10;

        //计算hn
        int hnX = Mathf.Abs(to / col - index / col);      //计算目标点距离当前点的单元格
        int hnY = Mathf.Abs(to % col - index % col);
        int hn = (hnX + hnY) * 10;

        return (gn + hn);
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
    /// 攻击
    /// </summary>
    public virtual void Attack(int to)
    {

    }

    public virtual void AttackDown()
    {
        Standby();
    }
}
