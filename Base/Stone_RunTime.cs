using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_RunTime
{
    public static Stone_RunTime Current;

    private Stone_CoreControl m_CoreControl;

    public Stone_RunTime()
    {
        m_CoreControl = new Stone_CoreControl();
    }

    public void Update()
    {
        m_CoreControl.Update();
    }

    public void Destroy()
    {
        m_CoreControl.Destroy();
    }

    // 获取核心控制器
    public Stone_CoreControl GetCoreControl()
    {
        return m_CoreControl;
    }

    // --- 静态函数

    // 获取管理器
    public static Stone_Manager GetManager(string managerName)
    {
        return Current?.GetCoreControl()?.GetManager(managerName);
    }

    // 添加管理器
    public static Stone_Manager AddManager(Stone_Manager manager,bool isUpdate)
    {
        return Current?.GetCoreControl()?.AddManager(manager, isUpdate);
    }
}
