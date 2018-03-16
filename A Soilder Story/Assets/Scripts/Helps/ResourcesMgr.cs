using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    public class ResourcesMgr : QMonoSingleton<ResourcesMgr>
    {
        //private Hashtable ht = null;                        //容器键值对集合
        private Dictionary<string, object> objDic = null;
        private Dictionary<string, List<GameObject>> objPool = null;
        private GameObject content;
        void Awake()
        {
            //ht = new Hashtable();
            objDic = new Dictionary<string, object>();
            objPool =  new Dictionary<string, List<GameObject>>();
            content = new GameObject();
            content.name = "ObjContent";
        }

        /// <summary>
        /// 获取obj
        /// </summary>
        /// <returns></returns>
        public GameObject GetPool(string name)
        {
            GameObject obj;
            if (objPool.ContainsKey(name) && objPool[name].Count > 0)
            {
                obj = objPool[name][0];
                objPool[name].RemoveAt(0);
            }
            else if (objPool.ContainsKey(name) && objPool[name].Count == 0)
            {
                obj = ResourcesMgr.Instance().LoadAsset(name, true);
            }
            else
            {
                obj = ResourcesMgr.Instance().LoadAsset(name, true);
                objPool.Add(name, new List<GameObject>());
            }
            obj.SetActive(true);
            return obj;
        }

        public void PushPool(GameObject obj, string name)
        {
            objPool[name].Add(obj);
            obj.transform.SetParent(content.transform);
            obj.SetActive(false);
        }

        public void PushPool(List<GameObject> obj, string name)
        {
            for (int i = 0; i < obj.Count; i++)
            {
                obj[i].transform.SetParent(content.transform);
                obj[i].SetActive(false);
                objPool[name].Add(obj[i]);
            }
        }

        public Sprite LoadSprite(string path)
        {
            return Resources.Load<GameObject>(path).GetComponent<SpriteRenderer>().sprite;
        }
        /// <summary>
        /// 调用资源（带对象缓冲技术）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="isCatch"></param>
        /// <returns></returns>
        public T LoadResource<T>(string path, bool isCatch) where T : UnityEngine.Object
        {
            if (objDic.ContainsKey(path))
            {
                return objDic[path] as T;
            }

            T TResource = Resources.Load<T>(path);
            if (TResource == null)
            {
                Debug.LogError(GetType() + "/GetInstance()/TResource 提取的资源找不到，请检查。 path=" + path);
            }
            else if (isCatch)
            {
                objDic.Add(path, TResource);
            }
            
            return TResource;
        }

        /// <summary>
        /// 调用资源（带对象缓冲技术）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isCatch"></param>
        /// <returns></returns>
        public GameObject LoadAsset(string path, bool isCatch)
        {
            GameObject goObj = LoadResource<GameObject>(path, isCatch);
            GameObject goObjClone = Instantiate(goObj) as GameObject;
            if (goObjClone == null)
            {
                Debug.LogError(GetType() + "/LoadAsset()/克隆资源不成功，请检查。 path=" + path);
            }
            goObj = null;
            return goObjClone;
        }
    }//Class_end
}
