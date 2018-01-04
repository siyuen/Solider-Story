using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class GameManager : QSingleton<GameManager>
{
    public static GameManager gameInstance = GameManager.Instance();
    public static InputManager inputInstance = InputManager.Instance();
    //public static TileMap mapInstance = TileMap.Instance();
    //public static PlayerManager playerInstance = PlayerManager.Instance();
    //public static EnemyManager enemyInstance = EnemyManager.Instance();

    public enum GameState
    {
        Login,
        Play
    }

    public GameState gameState = GameState.Play;

    private GameManager()
    {
        LevelManager.Instance();
    }

    public static void main()
    {
        
    }
}

