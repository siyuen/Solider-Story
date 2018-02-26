using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using DG.Tweening;

public class EffectManager : QSingleton<EffectManager> {

    private Dictionary<string, List<GameObject>> effectDic = new Dictionary<string, List<GameObject>>();
    private List<Tween> tweenList = new List<Tween>();

    private EffectManager()
    {
    }

    /// <summary>
    /// 增加特效
    /// </summary>
    public GameObject AddEffect(RectTransform parent, string name, Vector3 pos)
    {
        GameObject effect = ResourcesMgr.Instance().GetPool(name);
        effect.transform.SetParent(parent.transform);
        effect.transform.localScale = Vector3.one;
        effect.GetComponent<RectTransform>().localPosition = pos;
        if (!effectDic.ContainsKey(name))
            effectDic[name] = new List<GameObject>();
        effectDic[name].Add(effect);
        return effect;
    }

    /// <summary>
    /// 增加特效
    /// </summary>
    public GameObject AddEffect(GameObject parent, string name, Vector3 pos)
    {
        GameObject effect = ResourcesMgr.Instance().GetPool(name);
        effect.transform.SetParent(parent.transform);
        effect.transform.localScale = Vector3.one;
        effect.transform.localPosition = pos;
        if (!effectDic.ContainsKey(name))
            effectDic[name] = new List<GameObject>();
        effectDic[name].Add(effect);
        return effect;
    }

    /// <summary>
    /// 设置特效移动
    /// </summary>
    public void SetEffectMove(GameObject effect, Vector3 pos, float time, int loop)
    {
        Tweener tw = effect.transform.DOMove(pos, time);
        tw.SetLoops(loop);
        tweenList.Add(tw);
    }

    public void Clear()
    {
        foreach (var p in effectDic)
            ResourcesMgr.Instance().PushPool(p.Value, p.Key);
        effectDic.Clear();
        for (int i = 0; i < tweenList.Count; i++)
        {
            tweenList[i].Pause();
        }
        tweenList.Clear();
    }
}
