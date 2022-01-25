using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_UIManager : Stone_Manager
{
    public const string Name = "Stone_UIManager";
    public override string GetName()
    {
        return Stone_UIManager.Name;
    }

    private string m_ResourceRootPath;

    private Transform m_UICanvas;

    private Dictionary<string, Func<Stone_UIObject>> m_CreateUIObjectFuncDict;

    private Dictionary<string, Stone_UIObject> m_UIName2UIObjectDict;
    private Dictionary<string, List<Stone_UIObject>> m_ResourceName2UIObjectDict;
    private Dictionary<Stone_UIObject, bool> m_UIObjectOpenDict;
    private Dictionary<string, int> m_UINameDict;

    private Stone_ResourceManager ResourceManager;

    public Stone_UIManager(Stone_IManagerLifeControl stone_ManagerLifeControl) : base(stone_ManagerLifeControl)
    {
    }

    public override void Init()
    {
        m_CreateUIObjectFuncDict = new Dictionary<string, Func<Stone_UIObject>>();

        m_UIName2UIObjectDict = new Dictionary<string, Stone_UIObject>();
        m_ResourceName2UIObjectDict = new Dictionary<string, List<Stone_UIObject>>();
        m_UINameDict = new Dictionary<string, int>();
        m_UIObjectOpenDict = new Dictionary<Stone_UIObject, bool>();

        ResourceManager = Stone_RunTime.GetManager<Stone_ResourceManager>(Stone_ResourceManager.Name);
    }

    public override void UnInit()
    {
        ResourceManager = null;
    }

    public void SetResourceRootPath(string path)
    {
        m_ResourceRootPath = path;
    }

    public void SetUICanvasResourcesPath(string path)
    {
        GameObject uiCanvas = GameObject.Instantiate(Resources.Load<GameObject>(path));
        m_UICanvas = uiCanvas.transform;
        m_UICanvas.name = "UICanvas";
    }

    public void AddCreateUIObjectFunc(string resourceName,Func<Stone_UIObject> createFunc)
    {
        m_CreateUIObjectFuncDict.Add(resourceName, createFunc);
    }

    public Stone_UIObject OpenUI(string name)
    {
        Stone_UIObject uiObject = GetUI(name);

        if (uiObject == null)
        {
            uiObject = CreateUI(name, m_UICanvas);
        }

        if (uiObject == null)
        {
            LogHelper.Error?.Log(Stone_UIManager.Name, "Open UI Fail. find not ui.", name);
            return null;
        }

        bool isOpen;
        if(!m_UIObjectOpenDict.TryGetValue(uiObject,out isOpen))
        {
            isOpen = false;
            m_UIObjectOpenDict.Add(uiObject, isOpen);
        }

        if(isOpen)
        {
            return null;
        }

        m_UIObjectOpenDict[uiObject] = true;

        uiObject.Open();

        return uiObject;
    }

    public Stone_UIObject CreateSubUI(string parentName,string resourceName,Transform parentRoot = null, bool isOpen = true)
    {
        Stone_UIObject parentUIObject = GetUI(parentName);
        if(parentUIObject == null)
        {
            LogHelper.Error?.Log(Stone_UIManager.Name, "CreateSubUI Fail. parent is nil.", parentName);
            return null;
        }

        if(parentRoot == null)
        {
            parentRoot = parentUIObject.GetTransform();
        }

        Stone_UIObject childUI = CreateUI(resourceName, parentRoot);
        if(childUI == null)
        {
            LogHelper.Error?.Log(Stone_UIManager.Name, "CreateSubUI Fail. subUI is nil.", resourceName);
            return null;
        }

        if(isOpen)
        {
            OpenUI(childUI.GetUIName());
        }

        return childUI;
    }

    private Stone_UIObject CreateUI(string resourceName, Transform parent)
    {
        Func<Stone_UIObject> createFunc;
        if (!m_CreateUIObjectFuncDict.TryGetValue(resourceName, out createFunc))
        {
            LogHelper.Error?.Log(Stone_UIManager.Name, "CreateUI Fail. createFunc is nil.", resourceName);
            return null;
        }

        GameObject gameObject = ResourceManager.Instance(m_ResourceRootPath + "/" + resourceName + ".prefab");

        if (gameObject == null)
        {
            return null;
        }

        gameObject.name = resourceName;

        Transform transform = gameObject.transform;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        string uiName = resourceName;
        if (!m_UINameDict.ContainsKey(resourceName))
        {
            m_UINameDict.Add(resourceName, 1);
        }
        int count = m_UINameDict[resourceName];
        uiName += "_" + count;

        m_UINameDict[resourceName] = count + 1;

        Stone_UIObject uiObject = createFunc();
        uiObject.Init(uiName, resourceName, gameObject);

        List<Stone_UIObject> resourceList;
        if (!m_ResourceName2UIObjectDict.TryGetValue(resourceName, out resourceList))
        {
            resourceList = new List<Stone_UIObject>();
            m_ResourceName2UIObjectDict.Add(resourceName, resourceList);
        }
        resourceList.Add(uiObject);
        m_UIName2UIObjectDict.Add(uiName, uiObject);

        uiObject.Start();

        return uiObject;
    }

    public void CloseUI(string name)
    {
        Stone_UIObject uiObject = GetUI(name);

        if (uiObject == null)
        {
            LogHelper.Error?.Log(Stone_UIManager.Name, "CloseUI Fail. ui is nil.", name);
            return;
        }

        bool isOpen;
        if (!m_UIObjectOpenDict.TryGetValue(uiObject, out isOpen))
        {
            return;
        }

        if (!isOpen)
        {
            return;
        }

        m_UIObjectOpenDict[uiObject] = false;

        uiObject.Close();
    }

    public void DestroyUI(string name)
    {
        Stone_UIObject uiObject = GetUI(name);

        if (uiObject == null)
        {
            LogHelper.Error?.Log(Stone_UIManager.Name, "DestroyUI Fail. ui is nil.", name);
            return;
        }

        GameObject gameObject = uiObject.GetGameObject();

        List<Stone_UIObject> allUI = new List<Stone_UIObject>();

        GetAllChildByTrace(uiObject, allUI);

        for(int index = 0;index< allUI.Count;index++)
        {
            _DestroyUI(allUI[index]);
        }

        ResourceManager.DestroyGameObject(gameObject);
    }

    private void GetAllChildByTrace(Stone_UIObject parent, List<Stone_UIObject> childTrace)
    {
        List<Stone_UIObject> childs = parent.GetAllChild();
        int childCount = childs == null ? 0 : childs.Count;

        if (childCount == 0)
        {
            childTrace.Add(parent);
        }
        else
        {
            for (int index = 0; index < childs.Count; index++)
            {
                Stone_UIObject child = childs[index];

                List<Stone_UIObject> subChilds = child.GetAllChild();
                for (int j = 0; j < subChilds.Count; j++)
                {
                    GetAllChildByTrace(subChilds[j], childTrace);
                }

                childTrace.Add(child);
            }

            childTrace.Add(parent);
        }
    }

    private void _DestroyUI(Stone_UIObject uiObject)
    {
        bool isOpen;
        if (m_UIObjectOpenDict.TryGetValue(uiObject, out isOpen))
        {
            if (isOpen)
            {
                uiObject.Close();
            }
            m_UIObjectOpenDict.Remove(uiObject);
        }

        m_UIName2UIObjectDict.Remove(uiObject.GetUIName());
        List<Stone_UIObject> resourceList;
        if (m_ResourceName2UIObjectDict.TryGetValue(uiObject.GetResourceName(), out resourceList))
        {
            resourceList.Remove(uiObject);
        }

        uiObject.Destroy();
        uiObject.UnInit();
    }


    public Stone_UIObject GetUI(string name)
    {
        Stone_UIObject uiObject;

        do
        {
            uiObject = GetUIByResourceName(name);
            if(uiObject != null)
            {
                break;
            }

            uiObject = GetUIByUIName(name);

        } while (false);

        return uiObject;
    }


    public Stone_UIObject GetUIByUIName(string uiName)
    {
        Stone_UIObject uiObject;
        if(m_UIName2UIObjectDict.TryGetValue(uiName, out uiObject))
        {
            return uiObject;
        }
        return null;
    }

    public Stone_UIObject GetUIByResourceName(string resourceName)
    {
        List<Stone_UIObject> resourceList;
        if (!m_ResourceName2UIObjectDict.TryGetValue(resourceName, out resourceList))
        {
            return null;
        }

        if(resourceList .Count == 0)
        {
            return null;
        }

        return resourceList[0];
    }
}
