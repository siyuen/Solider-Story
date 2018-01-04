using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class GameObjectPool : QMonoSingleton<GameObjectPool> {

    private Dictionary<string, List<GameObject>> objPool = new Dictionary<string, List<GameObject>>();

    public GameObject GetPool(string name, Vector3 pos)
    {
        GameObject obj;
        if (objPool.ContainsKey(name) && objPool[name].Count > 0)
        {
            obj = objPool[name][0];
            objPool[name].RemoveAt(0);
        }
        else if (objPool.ContainsKey(name) && objPool[name].Count == 0)
        {
            //obj = Instantiate(Resources.Load(name)) as GameObject;       
            obj = ResourcesMgr.Instance().LoadAsset(name, true);
        }
        else
        {
            obj = ResourcesMgr.Instance().LoadAsset(name, true);
            //obj = Instantiate(Resources.Load(name)) as GameObject;
            objPool.Add(name, new List<GameObject>());
        }
        obj.SetActive(true);
        obj.transform.position = pos;
        return obj;
    }

    public void PushPool(GameObject obj, string name)
    {
        objPool[name].Add(obj);
        obj.SetActive(false);
    }

    public void PushPool(List<GameObject> obj, string name)
    {
        for(int i=0;i<obj.Count;i++)
        {
            obj[i].SetActive(false);
            objPool[name].Add(obj[i]);
        }
    }
}
