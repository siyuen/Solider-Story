using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

/// <summary>
/// 1.泛型
/// 2.反射
/// 3.抽象类
/// 4.命名空间
/// </summary>
namespace QFramework
{
    public abstract class QSingleton<T> where T : QSingleton<T>
    {
        protected static T instance = null;

        protected QSingleton()
        {

        }

        public static T Instance()
        {
            if (instance == null)
            {
                // 先获取所有非public的构造方法
                ConstructorInfo[] ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                // 从ctors中获取无参的构造方法
                ConstructorInfo ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                if (ctor == null)
                    Debug.Log("ctor = null");
                else
                {
                    Debug.Log("Instance Name: " + typeof(T).Name);
                    instance = ctor.Invoke(null) as T;
                }
                    
            }
            return instance;
        }
    }
    //生命周期
    public abstract class QMonoSingleton<T> : MonoBehaviour where T : QMonoSingleton<T>
    {
        protected static T instance = null;

        public static T Instance()
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (FindObjectsOfType<T>().Length > 1)
                {
                    Debug.Log("More than 1!");
                    return instance;
                }

                if (instance == null)
                {
                    string instanceName = typeof(T).Name;
                    Debug.Log("Instance Name: " + instanceName);
                    GameObject instanceGO = GameObject.Find(instanceName);

                    if (instanceGO == null)
                        instanceGO = new GameObject(instanceName);
                    instance = instanceGO.AddComponent<T>();
                    DontDestroyOnLoad(instanceGO);  //保证实例不会被释放
                    //Debug.Log("Add New Singleton " + instance.name + " in Game!");
                }
                else
                {
                    //Debug.Log("Already exist: " + instance.name);
                }
            }

            return instance;
        }


        protected virtual void OnDestroy()
        {
            instance = null;
        }
    }

    /// <summary>
    /// 延时执行函数
    /// </summary>
    public class DelayToInvoke : QMonoSingleton<DelayToInvoke>
    {
        public static IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            action();
        }
    }
}
