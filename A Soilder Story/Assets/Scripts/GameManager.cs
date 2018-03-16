using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class GameManager : QSingleton<GameManager>
{
    public const string FILEPATH = "Data/GameData";
    public Dictionary<int, GameData> gameDataDic;
    //没在进行的存档/空档
    public const int NULLGAME = -1;
    //开始游戏
    public const int PLAYGAME = 0;
    //通关
    public const int SUCCESS = 1;
    //主角死亡
    public const int PLAYERDEAD = 2;
    //最大关卡
    public const int MAXLEVEL = 3;
    //正在进行的存档
    public const int PLAYING = 4;

    public bool bTemporary;
    //正在进行的存档
    public int curGameIdx;

    public enum GameState
    {
        Login,  //登录
        Load,   //读取
        Save,   //保存
        Delete, //删除
        Play,   //开始
        Success,//成功
        Fail,   //失败
    }

    public GameState gameState = GameState.Play;

    private GameManager()
    {
        Dictionary<string, GameData> file = DataManager.Load<GameData>(FILEPATH);
        gameDataDic = new Dictionary<int,GameData>();
        for (int i = 0; i < file.Count; i++)
        {
            gameDataDic.Add(i, file[i.ToString()]);
        }
        bTemporary = false;
        curGameIdx = NULLGAME;
        for (int i = 0; i < gameDataDic.Count; i++)
        {
            if (DataManager.Value(gameDataDic[i].curplay) != NULLGAME)
            {
                bTemporary = true;
                curGameIdx = i;
                break;
            }
        }
    }

    public void TemporarySave()
    {
        LevelManager.Instance().TemporarySave();
        bTemporary = true;
    }

    /// <summary>
    /// 开始游戏,idx为存档id
    /// 更新中断游戏数据
    /// </summary>
    public void StartGame(int idx)
    {
        gameState = GameState.Play;
        //若当前选择存档不为有中断记录的存档则更换当前存档
        if (idx != curGameIdx)
        {
            if (curGameIdx != NULLGAME)
                gameDataDic[curGameIdx].curplay = NULLGAME.ToString();
            curGameIdx = idx;
            gameDataDic[idx].curplay = PLAYING.ToString();
        }
        int level = DataManager.Value(gameDataDic[curGameIdx].level);
        //若为空存档则开始第一关
        if (level == NULLGAME)
        {
            gameDataDic[curGameIdx].level = "0";
            level = 0;
        }
        UIManager.Instance().CloseUIForms("StartGame");
        ItemManager.Instance().SetTag(DataManager.Value(gameDataDic[idx].itemtag));
        LevelManager.Instance().Init(level);
        SaveGameData();
    }

    /// <summary>
    /// 判断是否有存档
    /// </summary>
    public bool HaveGameFile()
    {
        bool b = false;
        for (int i = 0; i < gameDataDic.Count; i++)
        {
            if (gameDataDic[i].level != NULLGAME.ToString())
            {
                b = true;
                break;
            }
        }
        return b;
    }

    /// <summary>
    /// 判断是否有中断记录,最大关卡时不算中断
    /// </summary>
    public bool HaveTemporary()
    {
        bool b = false;
        for (int i = 0; i < gameDataDic.Count; i++)
        {
            if (gameDataDic[i].curplay != NULLGAME.ToString() && gameDataDic[i].level != MAXLEVEL.ToString())
            {
                b = true;
                break;
            }
        }
        return b;
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ContinueGame()
    {
        gameState = GameState.Play;
        int level = DataManager.Value(gameDataDic[curGameIdx].level);
        LevelManager.Instance().ContinueGame(level);   
    }

    /// <summary>
    /// 保存游戏(三个存档)，用于通关时
    /// 保存相应的hero数据跟Item数据
    /// </summary>
    public void SaveGame(int idx)
    {
        if (idx != curGameIdx)
        {
            gameDataDic[curGameIdx].curplay = NULLGAME.ToString();
            curGameIdx = idx;
            gameDataDic[idx].curplay = PLAYING.ToString();
        }
        HeroManager.Instance().SaveHeroData();
        gameDataDic[idx].level = (LevelManager.Instance().GetCurLevel() + 1).ToString();
        gameDataDic[idx].itemtag = ItemManager.Instance().GetTag().ToString();
        SaveGameData();
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public void DeleteGame(int idx)
    {
        gameDataDic[idx].level = NULLGAME.ToString();
        gameDataDic[idx].curplay = NULLGAME.ToString();
        if (curGameIdx == idx)
        {
            bTemporary = false;
            curGameIdx = NULLGAME;
        }
        HeroManager.Instance().DeleteHeroFile(idx);
        SaveGameData();
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    public void SaveGameData()
    {
        Dictionary<string, GameData> game = new Dictionary<string, GameData>();
        for (int i = 0; i < gameDataDic.Count; i++)
        {
            game.Add(i.ToString(), gameDataDic[i]);
        }
        DataManager.Instance().SaveDicData<GameData>(game, FILEPATH);
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver(int state)
    {
        Debug.Log(state);
        if (state == PLAYERDEAD)
        {
            gameState = GameState.Fail;
            FightManager.Instance().GameOver();
            MainManager.Instance().Clear();
            UIManager.Instance().ShowUIForms("GameOver");
        }
        else if (state == SUCCESS)
        {
            gameState = GameState.Success;
            FightManager.Instance().GameOver();
            MainManager.Instance().Clear();
            UIManager.Instance().ShowUIForms("Story");
        }
    }
}

