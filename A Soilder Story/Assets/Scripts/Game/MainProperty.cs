using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainProperty {

    #region 预制体路径
    //mainManager
    public static string RANGENODE_PATH = "Prefabs/Game/moveTile";
    public static string ATTACKNODE_PATH = "Prefabs/Game/attackTile";
    public static string HEROMENU_PATH = "Prefabs/UI/Character/HeroMenu";
    //enemy
    public static string ENEMY_PATH = "Prefabs/Game/enemy";  
    //hero
    public static string HERO_PATH = "Prefabs/Game/hero";
    //cursor
    public static string ATTACKCURSOR_PATH = "Prefabs/Game/AttackCursor";
    public static string NORMALCURSOR_PATH = "Prefabs/Game/Cursor";
    //UI
    public static string HERO_BG = "Sprites/UI/HeroDataBg";
    public static string ENEMY_BG = "Sprites/UI/EnemyDataBg";
    public static string BUTTON_PATH = "Prefabs/UI/Normal/Button";
    public static string ITEM_PATH = "Prefabs/UI/Normal/Item";
    //Road
    public static string ROADBODY_LEFT_PATH = "Prefabs/Game/Road/RoadBody_left";
    public static string ROADBODY_UP_PATH = "Prefabs/Game/Road/RoadBody_up";
    public static string ROADCORNER_LEFT1_PATH = "Prefabs/Game/Road/RoadCorner_left1";
    public static string ROADCORNER_LEFT2_PATH = "Prefabs/Game/Road/RoadCorner_left2";
    public static string ROADCORNER_RIGHT1_PATH = "Prefabs/Game/Road/RoadCorner_right1";
    public static string ROADCORNER_RIGHT2_PATH = "Prefabs/Game/Road/RoadCorner_right2";
    public static string ROADHEAD_DOWN_PATH = "Prefabs/Game/Road/Roadhead_down";
    public static string ROADHEAD_LEFT_PATH = "Prefabs/Game/Road/RoadHead_left";
    public static string ROADHEAD_RIGHT_PATH = "Prefabs/Game/Road/RoadHead_right";
    public static string ROADHEAD_UP_PATH = "Prefabs/Game/Road/RoadHead_up";
    #endregion

    #region 层
    public static string LAYER_CHARACTER = "Character";      //检测层
    public static string LAYER_MAP = "Map";
    #endregion
}
