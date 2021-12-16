using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_TimerUpdateList : Stone_UpdateListHelper
{
    private Stone_TimerManager stone_TimerManager;
    private List<int> m_DeleteTimerIdList;

    protected override void Init()
    {
        m_DeleteTimerIdList = new List<int>();
    }

    public void Update(float delta)
    {
        UpdateBeforeRight();
        m_DeleteTimerIdList.Clear();

        int updateCount = m_UpdateObjectList.Count;
        if (updateCount > 0)
        {
            if(stone_TimerManager== null)
            {
                stone_TimerManager = Stone_RunTime.GetManager<Stone_TimerManager>(Stone_TimerManager.Name);
            }

            UpdateObjcect updateObjcect;
            Stone_Timer timer;

            for (int index = 0; index < updateCount; index++)
            {
                updateObjcect = m_UpdateObjectList[index];
                timer = (Stone_Timer)updateObjcect;

                timer.SetDelta(delta);
                timer.Update();

                if(timer.IsFinish())
                {
                    Delete(timer);
                    m_DeleteTimerIdList.Add(timer.GetId());
                }
            }
        }
    }

    public List<int> GetDeleteTimerIdList()
    {
        return m_DeleteTimerIdList;
    }
}
