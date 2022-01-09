using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_CoreControl
{
    private List<Stone_Manager> m_ManagerList;                  //管理器列表 list
    private Dictionary<string,Stone_Manager> m_ManagerDict;     //管理器列表 dict

    private Dictionary<string, Stone_Manager> m_ActiveManagerDict;   //激活的管理器列表 dict
    private Stone_UpdateListHelper m_UpdateManagerListHelper;   //更新中的管理器集合

    public Stone_CoreControl()
    {
        m_ManagerList = new List<Stone_Manager>();
        m_ManagerDict = new Dictionary<string, Stone_Manager>();

        m_ActiveManagerDict = new Dictionary<string, Stone_Manager>();
        m_UpdateManagerListHelper = new Stone_UpdateListHelper();
    }

    // --- 生命周期

    public void Update()
    {
        m_UpdateManagerListHelper.Update();
    }

    public void Destroy()
    {
        for (int index = m_ManagerList.Count-1; index >= 0; index--)
        {
            m_ManagerList[index].UnInit();
        }
        m_ManagerList.Clear();
    }

    // --- 管理器功能

    //获取管理器
    public T GetManager<T>(string managerName) where T : Stone_Manager
    {
        Stone_Manager manager;
        if(m_ManagerDict.TryGetValue(managerName, out manager))
        {
            return (T)manager;
        }

        return null;
    }


    // 添加管理器
    public Stone_Manager AddManager(Stone_Manager manager,bool isUpdate)
    {
        if(m_ManagerDict.ContainsKey(manager.GetName()))
        {
            return null;
        }

        manager.Init();

        Stone_IManagerLifeControl lifeControl = manager.GetLifeControl();
        if(lifeControl !=null)
        {
            lifeControl.InitAfter(manager);
        }

        m_ManagerList.Add(manager);
        m_ManagerDict.Add(manager.GetName(), manager);

        ActiveManager(manager, isUpdate);

        return manager;
    }

    // 激活管理器
    public void ActiveManager(Stone_Manager manager, bool isUpdate)
    {
        if (m_ActiveManagerDict.ContainsKey(manager.GetName()))
        {
            return;
        }

        m_ActiveManagerDict.Add(manager.GetName(),manager);
        ChangeManagerUpdate(manager, isUpdate);

        manager.Active();
    }

    // 休眠管理器
    public void DormancyManager(Stone_Manager manager)
    {
        if (!m_ActiveManagerDict.ContainsKey(manager.GetName()))
        {
            return;
        }

        m_ActiveManagerDict.Remove(manager.GetName());
        ChangeManagerUpdate(manager, false);

        manager.Dormancy();
    }

    // 删除管理器
    public void DeleteManager(Stone_Manager manager)
    {
        if (!m_ManagerDict.ContainsKey(manager.GetName()))
        {
            return;
        }

        DormancyManager(manager);

        m_ManagerList.Remove(manager);
        m_ManagerDict.Remove(manager.GetName());

        manager.UnInit();
    }


    // 改变管理器的更新
    public void ChangeManagerUpdate(Stone_Manager manager, bool isUpdate)
    {
        if(isUpdate)
        {
            m_UpdateManagerListHelper.Add(manager);
        }else
        {
            m_UpdateManagerListHelper.Delete(manager);
        }
    }
}
