using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class MessageCenter : QSingleton<MessageCenter> {

    public delegate void EventListener(MessageEvent e);
    //管理所有订阅
    private Dictionary<string, EventListener> dicListener;

    private MessageCenter()
    {
        dicListener = new Dictionary<string, EventListener>();
    }

    /// <summary>
    /// 添加订阅
    /// </summary>
    public void AddListener(string eventType, EventListener e)
    {
        if (dicListener.ContainsKey(eventType))
            dicListener[eventType] += e;
        else
        {
            dicListener.Add(eventType, e);
        }
    }

    /// <summary>
    /// 移除订阅
    /// </summary>
    public void RemoveListener(string eventType, EventListener e)
    {
        if (!dicListener.ContainsKey(eventType))
            return;
        dicListener[eventType] -= e;
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    public void DispatchEvent(MessageEvent e)
    {
        if (dicListener.ContainsKey(e.EventType))
        {
            dicListener[e.EventType](e);
        }
    }
}
