﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_Timer : Stone_UpdateListHelper.UpdateObjcect
{
    private int m_Id;
    private float m_Delta;

    private float m_UpdateCount;
    private float m_Interval;
    private Action m_Callback;
    private Action m_FinishCallback;

    private float m_CurTime;
    private float m_CurTotalCount;

    private bool m_IsFinish;

    public void Init()
    {
        m_IsFinish = false;
    }

    public void UnInit()
    {
        m_Callback = null;
    }

    public int GetId()
    {
        return m_Id;
    }

    public void SetDelta(float delta)
    {
        m_Delta = delta;
    }

    public void Active(int id,float updateCount,float interval,Action callback,Action finsih)
    {
        m_Id = id;

        m_UpdateCount = updateCount;
        m_Interval = interval;
        m_Callback = callback;
        m_FinishCallback = finsih;

        m_CurTime = 0;
        m_CurTotalCount = 0;
        m_IsFinish = false;
    }

    public void Update()
    {
        if(m_IsFinish)
        {
            return;
        }

        m_CurTime += m_Delta;

        if (m_CurTime >= m_Interval)
        {
            m_CurTime = 0;
            m_CurTotalCount += 1;

            try
            {
                m_Callback?.Invoke();
            }
            catch (Exception exception)
            {
                LogHelper.Error?.Log(Stone_TimerManager.Name, "timer execute error.", exception.Message, exception.StackTrace);
            }
        }

        if(m_UpdateCount != -1)
        {
            if(m_CurTotalCount >= m_UpdateCount)
            {
                m_IsFinish = true;

                try
                {
                    m_FinishCallback?.Invoke();
                }
                catch (Exception exception)
                {
                    LogHelper.Error?.Log(Stone_TimerManager.Name, "timer finish execute error.", exception.Message, exception.StackTrace);
                }
            }
        }
    }

    public void ForceFinish()
    {
        m_IsFinish = true;

        try
        {
            m_FinishCallback?.Invoke();
        }
        catch (Exception exception)
        {
            LogHelper.Error?.Log(Stone_TimerManager.Name, "timer finish execute error.", exception.Message, exception.StackTrace);
        }
    }

    public bool IsFinish()
    {
        return m_IsFinish;
    }

}
