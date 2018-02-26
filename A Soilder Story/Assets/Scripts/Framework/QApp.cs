using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UIFramework;

public class QApp : QMonoSingleton<QApp>
{
    public AppMode mode = AppMode.Developing;

    private QApp() { }

    void Awake()
    {
        // 确保不被销毁
        DontDestroyOnLoad(gameObject);

        instance = this;

        // 进入欢迎界面
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        StartCoroutine(ApplicationDidFinishLaunching());
    }

    /// <summary>
    /// 进入游戏
    /// </summary>
    IEnumerator ApplicationDidFinishLaunching()
    {
        //yield return GameManager.Instance();

        // 进入测试逻辑
        if (QApp.Instance().mode == AppMode.Developing)
        {
            // 测试资源加载
            UIManager.Instance().ShowUIForms("Login");
            //UIManager.Instance().ShowUIForms("LevelUp");
            yield return null;
            // 进入正常游戏逻辑
        }
        else if (QApp.Instance().mode == AppMode.QA)
        {
            //GameManager.Instance().StartGame(0);
            DataManager.Instance().Test();
            yield return null;
        }
        yield return null;
    }
}