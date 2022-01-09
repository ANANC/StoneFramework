using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_Timer : Stone_UpdateListHelper.UpdateObjcect
{
    private int m_Id;
    private float m_Delta;

    private float m_UpdateTime;
    private float m_Interval;
    private Action m_Callback;
    private Action<bool> m_FinishCallback;

    private float m_CurTime;
    private float m_CurTotalTime;

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

    public void Active(int id,float updateTime,float interval,Action callback,Action<bool> finsih)
    {
        m_Id = id;

        m_UpdateTime = updateTime;
        m_Interval = interval;
        m_Callback = callback;
        m_FinishCallback = finsih;

        m_CurTime = 0;
        m_CurTotalTime = 0;
        m_IsFinish = false;
    }

    public void Update()
    {
        if(m_IsFinish)
        {
            return;
        }

        m_CurTime += m_Delta;
        m_CurTotalTime += m_Delta;

        if (m_CurTime >= m_Interval)
        {
            m_CurTime = 0;

            try
            {
                m_Callback?.Invoke();
            }
            catch (Exception exception)
            {
                LogHelper.Error?.Log(Stone_TimerManager.Name, "timer execute error. ", exception.Message, "\n", exception.StackTrace);
            }
        }

        if(m_UpdateTime != -1)
        {
            if(m_CurTotalTime >= m_UpdateTime)
            {
                m_IsFinish = true;

                try
                {
                    //正常结束
                    m_FinishCallback?.Invoke(false);
                }
                catch (Exception exception)
                {
                    LogHelper.Error?.Log(Stone_TimerManager.Name, "timer finish execute error. ", exception.Message,"\n", exception.StackTrace);
                }
            }
        }
    }

    public void ForceFinish()
    {
        if (m_IsFinish)
        {
            return;
        }

        m_IsFinish = true;

        try
        {
            //强制结束
            m_FinishCallback?.Invoke(true);
        }
        catch (Exception exception)
        {
            LogHelper.Error?.Log(Stone_TimerManager.Name, "timer finish execute error. ", exception.Message, "\n", exception.StackTrace);
        }
    }

    public bool IsFinish()
    {
        return m_IsFinish;
    }

}
