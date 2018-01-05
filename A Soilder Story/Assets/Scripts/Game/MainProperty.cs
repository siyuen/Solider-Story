using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainProperty {

    #region 预制体路径
    //mainManager
    public static string RANGENODE_PATH = "Prefabs/Game/moveTile";
    public static string ATTACKNODE_PATH = "Prefabs/Game/attackTile";
    public static string HEROMENU_PATH = "Prefabs/UI/Character/HeroMenu";
    public static string NORMALCURSOR_PATH = "Prefabs/Game/Cursor";
    //enemy
    public static string ENEMY_PATH = "Prefabs/Game/enemy";  
    //hero
    public static string HERO_PATH = "Prefabs/Game/hero";
    //UI
    public static string HERO_BG = "Sprites/UI/HeroDataBg";
    public static string ENEMY_BG = "Sprites/UI/EnemyDataBg";
    public static string BUTTON_PATH = "Prefabs/UI/Normal/Button";

    #endregion

    #region 层
    public static string LAYER_CHARACTER = "Character";      //检测层
    public static string LAYER_MAP = "Map";
    #endregion
}
