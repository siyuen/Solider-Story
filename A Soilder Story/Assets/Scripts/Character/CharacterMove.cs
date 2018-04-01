using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour {

    //移动速度
    public float moveSpeed = 30f;
    //移动方向             
    protected Vector2 moveDir = Vector3.zero;
    //上一步方向            
    protected Vector2 lastDir = Vector3.zero;
    //当前方向             
    protected Vector2 curDir = Vector3.zero;
    //当前方向的string         
    public string dirStr;
    //是否移动                             
    public bool bMove;
    //路径计算
    private Transform mTransform;
    private List<int> path = new List<int>();
    private List<int> close = new List<int>();
    //人物
    private Character mRole;

    public void Init(Character role, Transform tran)
    {
        mRole = role;
        mTransform = tran;
    }

    void Start()
    {
        dirStr = "bNormal";
    }

    void Update()
    {
        if (bMove)
            Move();
    }

    #region 人物移动
    public void Move()
    {
        mRole.HideMoveRange();
        MapNode to = LevelManager.Instance().GetMapNode(path[path.Count - 1]);
        if (Vector2.Distance(mTransform.position, to.transform.position) > 0.5)
        {
            mTransform.Translate(moveSpeed * moveDir.normalized * Time.deltaTime);
        }
        else
        {
            mRole.mIdx = to.GetID();
            mTransform.position = to.transform.position;
            this.transform.position -= new Vector3(0, 0, 1);
            path.RemoveAt(path.Count - 1);
            if (path.Count != 0)
            {
                to = LevelManager.Instance().GetMapNode(path[path.Count - 1]);
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
            mRole.SetAnimator(dirStr, false);
            dirStr = "bUp";
            mRole.SetAnimator(dirStr, true);
        }
        else if (curDir == Vector2.right)
        {
            mRole.SetAnimator(dirStr, false);
            dirStr = "bRight";
            mRole.SetAnimator(dirStr, true);
        }
        else if (curDir == Vector2.down)
        {
            mRole.SetAnimator(dirStr, false);
            dirStr = "bDown";
            mRole.SetAnimator(dirStr, true);
        }
        else if (curDir == Vector2.left)
        {
            mRole.SetAnimator(dirStr, false);
            dirStr = "bLeft";
            mRole.SetAnimator(dirStr, true);
        }
        else if (curDir == Vector2.zero)
        {
            //mAnimator.SetBool(dirStr, false);
        }
    }

    public void MoveDone()
    {
        curDir = Vector3.zero;
        mRole.MoveDone();
    }

    public void MoveTo(int to)
    {
        if (!LevelManager.Instance().GetMapNode(to).canMove)
            return;
        //原地
        if (to == mRole.mIdx)
        {
            mRole.HideMoveRange();
            MoveDone();
            return;
        }
        path.Clear();
        MapNode parent = LevelManager.Instance().GetMapNode(to);
        while (parent.parentMapNode != null)  //通过父节点计算路径存于path中
        {
            path.Add(parent.GetID());
            parent = parent.parentMapNode;
        }
        close.Clear();
        if (path.Count == 0)
            return;
        moveDir = LevelManager.Instance().GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        //将z坐标前移
        this.transform.position -= new Vector3(0, 0, 1);
        ChangeDir();
        bMove = true;
    }

    public void MoveTo(MapNode to)
    {
        if (!to.canMove)
            return;
        //原地
        if (to.GetID() == mRole.mIdx)
        {
            mRole.HideMoveRange();
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
        moveDir = LevelManager.Instance().GetMapNode(path[path.Count - 1]).transform.position - mTransform.position; //初始化方向
        this.transform.position -= new Vector3(0, 0, 1);
        ChangeDir();
        bMove = true;
    }
    #endregion
}