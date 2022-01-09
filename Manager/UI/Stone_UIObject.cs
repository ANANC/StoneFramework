using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_UIObject
{
    private string m_UIName;
    private string m_ResourceName;

    private GameObject m_GameObject;
    private Transform m_Transform;

    private Dictionary<string, Stone_UIObject> m_ChildDict;
    private List<Stone_UIObject> m_ChildList;

    public void Init(string uiName, string resourceName, GameObject gameObject)
    {
        m_UIName = uiName;
        m_ResourceName = resourceName;

        m_GameObject = gameObject;
        m_Transform = gameObject.transform;

        m_ChildDict = new Dictionary<string, Stone_UIObject>();
        m_ChildList = null;
    }
    
    /// <summary>
    /// 初始化 只执行一次
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// 打开
    /// </summary>
    public virtual void Open() { }

    /// <summary>
    /// 关闭UI
    /// </summary>
    public virtual void Close() { }

    /// <summary>
    /// 销毁 只执行一次
    /// </summary>
    public virtual void Destroy() { }

    public void UnInit()
    {
        m_GameObject = null;
        m_Transform = null;

        m_ChildDict = null;
        m_ChildList = null;
    }


    public string GetUIName()
    {
        return m_UIName;
    }

    public string GetResourceName()
    {
        return m_ResourceName;
    }
    public Transform GetTransform()
    {
        return m_Transform;
    }

    public GameObject GetGameObject()
    {
        return m_GameObject;
    }

    public void AddSubUI(Stone_UIObject child)
    {
        m_ChildDict.Add(child.GetUIName(), child);
        m_ChildList.Add(child);
    }

    public Stone_UIObject GetChild(string uiName)
    {
        return m_ChildDict[uiName];
    }

    public List<Stone_UIObject> GetAllChild()
    {
        return m_ChildList;
    }


}
