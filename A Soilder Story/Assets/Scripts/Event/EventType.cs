using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventType
{
    #region 键盘鼠标输入事件
    public static int PLAYER_CLICK = 1;  //玩家鼠标点击
    public static int PLAYER_MOVE = 2;   //玩家鼠标移动
    public static int KEY_UPARROW = 3;   //上下左右
    public static int KEY_DOWNARROW = 4;
    public static int KEY_LEFTARROW = 5;
    public static int KEY_RIGHTARROW = 6;
    public static int KEY_Z = 7;         //Z
    public static int KEY_X = 8;         //x
    public static int KEY_S = 9;         //s
    #endregion

    #region 人物相关事件
    public static string SELECTHERO = "SelectHero";
    public static string SELECTENEMY = "SelectEnemy";
    #endregion

    #region UI事件
    public static string UPDATEROLEUI = "UpdateRoleUI";
    public static string UPDATEMAPNODEUI = "UpdateMapNodeUI";
    #endregion

    #region 游戏主体MainManager事件
    public static string UPDATEROUND = "UpdateRound";
    public static string HEROSTANDBY = "HeroStandby";
    public static string UPDATECURHERO = "UpdateCurHero";
    public static string UPDATECURENEMY = "UpdateCurEnemy";
    #endregion
}
