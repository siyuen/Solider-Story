using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    public class ResourcesMgr : QMonoSingleton<ResourcesMgr>
    {
        private Hashtable ht = null;                        //容器键值对集合
        private Dictionary<string, List<GameObject>> objPool = new Dictionary<string, List<GameObject>>(); 

        void Awake()
        {
            ht = new Hashtable();
        }

        /// <summary>
        /// 获取obj
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetPool(string path, int length)
        {
            if (objPool.ContainsKey(path))
            {
                if (objPool[path].Count >= length)
                {
                    return objPool[path];
                }
                else
                {
                    for (int i = 0; i < length - objPool[path].Count; i++)
                    {
                        GameObject obj = LoadAsset(path, true);
                        objPool[path].Add(obj);
                    }
                    return objPool[path]; ;
                }
            }
            else
            {
                objPool[path] = new List<GameObject>();
                for (int i = 0; i < length; i++)
                {
                    GameObject obj = LoadAsset(path, true);
                    objPool[path].Add(obj);
                }
                return objPool[path];
            }
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
            if (ht.Contains(path))
            {
                return ht[path] as T;
            }

            T TResource = Resources.Load<T>(path);
            if (TResource == null)
            {
                Debug.LogError(GetType() + "/GetInstance()/TResource 提取的资源找不到，请检查。 path=" + path);
            }
            else if (isCatch)
            {
                ht.Add(path, TResource);
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
