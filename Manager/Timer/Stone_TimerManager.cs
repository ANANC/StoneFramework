using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_TimerManager : Stone_Manager
{
    public const string Name = "Stone_TimerManager";
    public override string GetName()
    {
        return Stone_TimerManager.Name;
    }

    private Dictionary<int, Stone_Timer> m_TimerDict;
    private Stone_TimerUpdateList m_TimerUpdateList;

    private int m_AutoId;

    public override void Init()
    {
        m_TimerDict = new Dictionary<int, Stone_Timer>();
        m_TimerUpdateList = new Stone_TimerUpdateList();

        m_AutoId = 1;
    }

    public override void UnInit()
    {

    }

    public override void Update()
    {
        m_TimerUpdateList.Update(Time.deltaTime);

        List<int> deleteTimerIdList = m_TimerUpdateList.GetDeleteTimerIdList();
        for (int index = 0; index < deleteTimerIdList.Count; index++)
        {
            int deleteTimerId = deleteTimerIdList[index];
            m_TimerDict.Remove(deleteTimerId);
        }
    }

    public int StarTimer(Action callback, Action<bool> finish = null,float interval = -1, float updateTime = -1)
    {
        int id = m_AutoId++;

        Stone_Timer stone_Timer = new Stone_Timer();
        stone_Timer.Init();

        stone_Timer.Active(id, updateTime, interval, callback, finish);

        m_TimerUpdateList.Add(stone_Timer);
        m_TimerDict.Add(id, stone_Timer);

        return id;
    }

    public void StopTimer(int id)
    {
        Stone_Timer stone_Timer;
        if (m_TimerDict.TryGetValue(id, out stone_Timer))
        {
            stone_Timer.ForceFinish();

            m_TimerUpdateList.Delete(stone_Timer);
            m_TimerDict.Remove(id);
        }
    }
}
