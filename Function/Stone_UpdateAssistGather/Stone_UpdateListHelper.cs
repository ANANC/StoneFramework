using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_UpdateListHelper 
{
    public interface UpdateObjcect
    {
        void Update();
    };

    protected List<UpdateObjcect> m_UpdateObjectList;
    protected List<UpdateObjcect> m_AddUpdateObjectList;
    protected List<UpdateObjcect> m_DeleteUpdateObjectList;

    public Stone_UpdateListHelper()
    {
        m_UpdateObjectList = new List<UpdateObjcect>();
        m_AddUpdateObjectList = new List<UpdateObjcect>();

        Init();
    }

    protected virtual void Init() { }

    public virtual void Add(UpdateObjcect updateObjcect)
    {
        if(m_UpdateObjectList.Contains(updateObjcect))
        {
            return;
        }

        m_AddUpdateObjectList.Add(updateObjcect);
    }

    public virtual void Delete(UpdateObjcect updateObjcect)
    {
        if (m_DeleteUpdateObjectList == null)
        {
            m_DeleteUpdateObjectList = new List<UpdateObjcect>();
        }

        m_DeleteUpdateObjectList.Add(updateObjcect);
    }

    public virtual void Update()
    {
        UpdateBeforeRight();

        UpdateObjcect updateObjcect;
        int updateCount = m_UpdateObjectList.Count;
        if(updateCount>0)
        {
            for(int index = 0;index<updateCount;index++)
            {
                updateObjcect = m_UpdateObjectList[index];
                updateObjcect.Update();
            }
        }
    }

    public void UpdateBeforeRight()
    {
        UpdateObjcect updateObjcect;

        int addCount = m_AddUpdateObjectList.Count;
        if (addCount > 0)
        {
            for (int index = 0; index < addCount; index++)
            {
                updateObjcect = m_AddUpdateObjectList[index];
                m_UpdateObjectList.Add(updateObjcect);
            }
            m_AddUpdateObjectList.Clear();
        }

        if (m_DeleteUpdateObjectList != null)
        {
            int deleteCount = m_DeleteUpdateObjectList.Count;
            if (deleteCount > 0)
            {
                for (int index = 0; index < deleteCount; index++)
                {
                    updateObjcect = m_DeleteUpdateObjectList[index];
                    m_UpdateObjectList.Remove(updateObjcect);
                }
                m_DeleteUpdateObjectList.Clear();
            }
        }
    }

}
