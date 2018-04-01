using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using System;
using UIFramework;

public class CameraManager : QMonoSingleton<CameraManager> {

    public Camera mainCamera;
    public Camera roleCamera;
    public Camera fightCamera;
    public Camera effectCamera;

    //设定宽高
    private float standardWidth;
    private float standardHeight;

    //当前设备宽高
    private float deviceWidth;
    private float deviceHeight;

    void Awake()
    {
        standardWidth = 900;
        standardHeight = 600;
    }


    void Update()
    {
        deviceWidth = Screen.width;
        deviceHeight = Screen.height;
        if (Math.Abs(deviceWidth / deviceHeight - standardWidth / standardHeight) > 0.05f)
        {
            float width = deviceHeight * (standardWidth / standardHeight);
            Screen.SetResolution((int)width, (int)deviceHeight, false);
        }
    }
    /// <summary>
    /// 获取人物位置比例
    /// </summary>
    public Vector2 GetRolePos(Vector3 pos)
    {
        //在地图中的pos比例
        LevelManager main = LevelManager.Instance();
        int x = main.mapXNode;
        int width = main.nodeWidth;
        int y = main.mapYNode;
        int height = main.nodeHeight;
        Vector2 v = new Vector2(pos.x / (x * width), pos.y /(y * height));
        return v;
    }

    /// <summary>
    /// 世界坐标转换为UI坐标,按比例
    /// </summary>
    public Vector2 World2UIPos(Vector2 pos)
    {
        Vector2 zero = new Vector2(-450,300);
        Vector2 p = zero + new Vector2(pos.x * standardWidth, pos.y * standardHeight);
        return p;
    }

    /// <summary>
    /// 世界坐标转换为UI坐标,人物pos
    /// </summary>
    public Vector2 World2UIPos(Vector3 pos)
    {
        Vector2 role = GetRolePos(pos);
        Vector2 zero = new Vector2(-450, 300);
        Vector2 p = zero + new Vector2(role.x * standardWidth, role.y * standardHeight);
        return p;
    }

    /// <summary>
    /// 获取cameraSize
    /// </summary>
    public Vector2 GetCameraSize(Camera camera)
    {
        float hegiht = camera.orthographicSize * 2;
        float width = deviceWidth / deviceHeight * hegiht;
        return new Vector2(width, hegiht);
    }

}
