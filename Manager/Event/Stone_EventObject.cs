using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Stone_EventObject 
{
    public class EventCallbackInfo { }

    private string m_Name;

    private Dictionary<Delegate, int> m_RegisterSortDict;

    private List<Delegate> m_AddRegisterList;
    private List<Delegate> m_DeleteList;

    private Dictionary<Delegate, int> m_AllRegisterDict;
    private List<Delegate> m_AllRegisterList;

    private object[] m_CallbackParams;

    private bool m_IsDirty;

    public void Init(string name)
    {
        m_Name = name;

        m_RegisterSortDict = new Dictionary<Delegate, int>();

        m_AddRegisterList = new List<Delegate>();
        m_DeleteList = new List<Delegate>();

        m_AllRegisterDict = new Dictionary<Delegate, int>();
        m_AllRegisterList = new List<Delegate>();

        m_CallbackParams = new object[1];

        m_IsDirty = false;
    }

    public void UnInit()
    {
        m_RegisterSortDict.Clear();
        m_AddRegisterList.Clear();
        m_DeleteList.Clear();
        m_AllRegisterDict.Clear();
        m_AllRegisterList.Clear();
    }

    /// <summary>
    /// 添加监听者
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="sort"></param>
    public void AddListener(Action listener,int sort = -1)
    {
        _AddListener(listener, sort);
    }

    /// <summary>
    /// 添加监听者
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="sort"></param>
    public void AddListener<T>(Action<T> listener, int sort = -1) where T : EventCallbackInfo
    {
        _AddListener(listener, sort);
    }

    private void _AddListener(Delegate listener, int sort)
    {
        if (m_AllRegisterDict.ContainsKey(listener))
        {
            return;
        }

        m_IsDirty = true;

        if (sort != -1)
        {
            bool hasNewSort = false;
            Dictionary<Delegate, int>.Enumerator enumerator = m_RegisterSortDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int delegateSort = enumerator.Current.Value;

                if (delegateSort == sort)
                {
                    hasNewSort = true;
                    break;
                }
            }
            if (hasNewSort)
            {
                for (int index = 0; index < m_AllRegisterList.Count; index++)
                {
                    Delegate register = m_AllRegisterList[index];
                    int delegateSort = m_RegisterSortDict[register];
                    m_RegisterSortDict[register] = delegateSort + 1;
                }
                m_RegisterSortDict.Add(listener, sort);
            }
        }

        m_AddRegisterList.Add(listener);
    }


    /// <summary>
    /// 删除监听者
    /// </summary>
    /// <param name="listener"></param>
    public void DeleteListern(Delegate listener)
    {
        m_IsDirty = true;

        m_DeleteList.Add(listener);
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="info"></param>
    public void Execute(EventCallbackInfo info = null)
    {
        TryRefresh();

        for (int index = 0; index < m_AllRegisterList.Count; index++)
        {
            Delegate listener = m_AllRegisterList[index];

            m_CallbackParams[0] = info;

            object target = listener.Target;
            MethodInfo method = listener.Method;

            try
            {
                method.Invoke(target, m_CallbackParams);
            }
            catch (Exception exception)
            {
                LogHelper.Error?.Log("Event", m_Name, "Execute Error.", exception.Message, exception.StackTrace);
            }
        }

        TryRefresh();
    }

    private void TryRefresh()
    {
        if (m_IsDirty == false)
        {
            return;
        }

        m_IsDirty = false;

        for (int index = 0; index < m_AddRegisterList.Count; index++)
        {
            Delegate listener = m_AddRegisterList[index];

            if (m_AllRegisterDict.ContainsKey(listener))
            {
                continue;
            }

            int sort;
            if (!m_RegisterSortDict.TryGetValue(listener, out sort))
            {
                sort = -1;
            }

            m_AllRegisterDict.Add(listener, sort);
            m_AllRegisterList.Add(listener);
        }
        m_AddRegisterList.Clear();

        for (int index = 0; index < m_DeleteList.Count; index++)
        {
            Delegate listener = m_DeleteList[index];

            if (!m_AllRegisterDict.ContainsKey(listener))
            {
                continue;
            }

            m_AllRegisterDict.Remove(listener);
            m_AllRegisterList.Remove(listener);

            m_RegisterSortDict.Remove(listener);
        }
        m_DeleteList.Clear();

        m_AllRegisterList.Sort(RegisterListSort);
    }

    private int RegisterListSort(Delegate left, Delegate right)
    {
        int leftSort = m_AllRegisterDict[left];
        int rightSort = m_AllRegisterDict[right];

        return leftSort.CompareTo(rightSort);
    }
}
