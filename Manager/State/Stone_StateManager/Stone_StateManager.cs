using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_StateManager : Stone_Manager
{
    public const string Name = "Stone_StateManager";
    public override string GetName()
    {
        return Stone_StateManager.Name;
    }

    private Dictionary<string, Type> m_StateTypeDict;
    private Dictionary<string, Stone_IState> m_StateDict;
    private Stone_IState m_CurState;

    public Stone_StateManager(Stone_IManagerLifeControl stone_ManagerLifeControl) : base(stone_ManagerLifeControl)
    {
    }

    public override void Init()
    {
        m_StateTypeDict = new Dictionary<string, Type>();
        m_StateDict = new Dictionary<string, Stone_IState>();

        m_CurState = null;
    }

    /// <summary>
    /// 添加State的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stateName"></param>
    public void AddStateType<T>(string stateName)where T: Stone_IState
    {
        if(m_StateTypeDict.ContainsKey(stateName))
        {
            return;
        }

        Type stateType = typeof(T);
        m_StateTypeDict.Add(stateName, stateType);
    }

    /// <summary>
    /// 添加State的实例
    /// </summary>
    /// <param name="state"></param>
    public void AddState(Stone_IState state)
    {
        state.Init();

        m_StateDict.Add(state.GetName(),state);
    }

    /// <summary>
    /// 删除State的实例
    /// </summary>
    /// <param name="stateName"></param>
    public void DeleteState(string stateName)
    {
        if(m_CurState?.GetName() == stateName)
        {
            return;
        }

        Stone_IState state;
        if(!m_StateDict.TryGetValue(stateName,out state))
        {
            return;
        }

        state.UnInit();
        m_StateDict.Remove(stateName);
    }

    private Stone_IState CreateState(string stateName)
    {
        Type stateType;
        if(!m_StateTypeDict.TryGetValue(stateName,out stateType))
        {
            return null;
        }

       object stateObj =  Activator.CreateInstance(stateType);
        if(stateObj == null)
        {
            return null;
        }

        return (Stone_IState)stateObj;
    }

    /// <summary>
    /// 进入State
    /// </summary>
    /// <param name="stateName"></param>
    public void EnterState(string stateName,bool isDeleteCurState = false)
    {
        Stone_IState nextState;
        if (!m_StateDict.TryGetValue(stateName, out nextState))
        {
            nextState = CreateState(stateName);
            AddState(nextState);
        }

        if(nextState == null)
        {
            LogHelper.Error?.Log(Stone_StateManager.Name, "EnterState Fail.",stateName);
            return;
        }

        if(m_CurState!=null)
        {
            string curStateName = m_CurState.GetName();

            m_CurState.Exist();
            m_CurState = null;
            
            if(isDeleteCurState == true)
            {
                DeleteState(curStateName);
            }
        }

        m_CurState = nextState;
        nextState.Enter();

    }


}
