using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageEvent{
    //事件类型
    private string eventType;
    public string EventType { get { return eventType; }}
    //数据类型
    private MsgEventData data;
    public MsgEventData Data { get { return data; } }
    //事件发送者
    private object target;
    public object Target { get { return target; }}

    public MessageEvent(string type, MsgEventData d = null, Object t = null)
    {
        eventType = type;
        data = d;
        target = t;
    }
}