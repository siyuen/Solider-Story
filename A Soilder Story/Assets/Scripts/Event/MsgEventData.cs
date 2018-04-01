using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件信息数据
/// </summary> 
public interface MsgEventData
{
};

/// <summary>
/// 选择人物需要通知人物Manager当前人物的Listidx
/// </summary>
public class SelectRoleData : MsgEventData{
    public int id;  //list id
}

/// <summary>
/// 更新人物信息UI
/// </summary>
public class UIRoleData : MsgEventData
{
    public Character role;
}

/// <summary>
/// 更新MapNode信息UI
/// </summary>
public class UIMapNodeData : MsgEventData
{
    public MapNode node;
}

/// <summary>
/// 选中hero
/// </summary>
public class UpdateCurHero : MsgEventData
{
    public HeroController hero;
}

/// <summary>
/// 选中Enemy
/// </summary>
public class UpdateCurEnemy : MsgEventData
{
    public EnemyController enemy;
}