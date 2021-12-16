using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Stone_EventManager : Stone_Manager
{
    public const string Name = "Stone_EventManager";
    public override string GetName()
    {
        return Stone_EventManager.Name;
    }

    private class RegisterInfo
    {
        public object Target;
        public Dictionary<string, List<Delegate>> EventNameToRegisterDict;        
    }

    private Dictionary<string, Stone_EventObject> m_EventObjectDict;
    private Dictionary<object, RegisterInfo> m_RegisterInfoDict;

    public override void Init()
    {
        m_EventObjectDict = new Dictionary<string, Stone_EventObject>();
        m_RegisterInfoDict = new Dictionary<object, RegisterInfo>();
    }

    public override void UnInit()
    {
    }

    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="target"></param>
    /// <param name="listener"></param>
    public void AddListener(string name,object target, Action listener)
    {
        Stone_EventObject eventObject;
        if (!m_EventObjectDict.TryGetValue(name, out eventObject))
        {
            eventObject = new Stone_EventObject();
            eventObject.Init(name);

            m_EventObjectDict.Add(name, eventObject);
        }

        eventObject.AddListener(listener);

        AddRegisterInfoInfo(name, target, listener);
    }

    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="target"></param>
    /// <param name="listener"></param>
    public void AddListener<T>(string name,object target, Action<T> listener) where T: Stone_EventObject.EventCallbackInfo
    {
        Stone_EventObject eventObject;
        if (!m_EventObjectDict.TryGetValue(name, out eventObject))
        {
            eventObject = new Stone_EventObject();
            eventObject.Init(name);

            m_EventObjectDict.Add(name, eventObject);
        }

        eventObject.AddListener(listener);

        AddRegisterInfoInfo(name, target, listener);
    }

    private void AddRegisterInfoInfo(string name, object target, Delegate listener)
    {
        RegisterInfo registerInfo;
        if (!m_RegisterInfoDict.TryGetValue(target, out registerInfo))
        {
            registerInfo = new RegisterInfo();
            registerInfo.Target = target;
            registerInfo.EventNameToRegisterDict = new Dictionary<string, List<Delegate>>();

            m_RegisterInfoDict.Add(target, registerInfo);
        }
        List<Delegate> delegateList;
        if (!registerInfo.EventNameToRegisterDict.TryGetValue(name, out delegateList))
        {
            delegateList = new List<Delegate>();
            registerInfo.EventNameToRegisterDict.Add(name, delegateList);
        }
        delegateList.Add(listener);
    }

    /// <summary>
    /// 删除监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="target"></param>
    /// <param name="listener"></param>
    public void DeleteListener(string name, object target, Delegate listener)
    {
        Stone_EventObject eventObject;
        if (!m_EventObjectDict.TryGetValue(name, out eventObject))
        {
            return;
        }

        eventObject.DeleteListern(listener);

        RegisterInfo registerInfo;
        if (m_RegisterInfoDict.TryGetValue(target, out registerInfo))
        {
            List<Delegate> delegateList;
            if (registerInfo.EventNameToRegisterDict.TryGetValue(name, out delegateList))
            {
                delegateList.Remove(listener);
            }
        }
    }

    /// <summary>
    /// 删除目标的全部监听
    /// </summary>
    /// <param name="target"></param>
    public void DeleteTargetAllListener(object target)
    {
        RegisterInfo registerInfo;
        if (m_RegisterInfoDict.TryGetValue(target, out registerInfo))
        {
            Dictionary<string, List<Delegate>>.Enumerator enumerator = registerInfo.EventNameToRegisterDict.GetEnumerator();
            while(enumerator.MoveNext())
            {
                string eventName = enumerator.Current.Key;

                Stone_EventObject eventObject;
                if (!m_EventObjectDict.TryGetValue(eventName, out eventObject))
                {
                    continue;
                }

                List<Delegate> listenerList = enumerator.Current.Value;

                for (int index = 0;index<listenerList.Count;index++)
                {
                    Delegate listener = listenerList[index];
                    eventObject.DeleteListern(listener);
                }
            }
        }

        m_RegisterInfoDict.Remove(target);
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="name"></param>
    /// <param name="info"></param>
    public void Execute(string name, Stone_EventObject.EventCallbackInfo info = null)
    {
        Stone_EventObject eventObject;
        if (!m_EventObjectDict.TryGetValue(name, out eventObject))
        {
            return;
        }

        eventObject.Execute(info);
    }

}
