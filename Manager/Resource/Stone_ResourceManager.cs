using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Stone_ResourceManager : Stone_Manager
{
    public const string Name = "Stone_ResourceManager";
    public override string GetName()
    {
        return Stone_ResourceManager.Name;
    }

    private Stone_IResourceLoader m_ResourceLoader;
    private Dictionary<string, Object> m_LoadObjectDict;

    public Stone_ResourceManager(Stone_IManagerLifeControl stone_ManagerLifeControl) : base(stone_ManagerLifeControl)
    {
    }

    public override void Init()
    {
        m_LoadObjectDict = new Dictionary<string, Object>();
    }

    public void SetResourceLoader(Stone_IResourceLoader stone_IResourceLoader)
    {
        if (m_ResourceLoader != null)
        {
            return;
        }

        m_ResourceLoader = stone_IResourceLoader;
        m_ResourceLoader.Init();
    }

    //------------ GameObject加载 ------------

    public GameObject Instance(string path, string assetbundle = null)
    {
        GameObject prefab = LoadResource<GameObject>(path, assetbundle);
        if (prefab != null)
        {
            return GameObject.Instantiate(prefab);
        }

        return null;
    }

    //------------ 资源加载 ------------

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResource<T>(string path,string assetbundle=null) where T : UnityEngine.Object
    {
        Object resObject = null;

        //判断是否已经加载
        if (m_LoadObjectDict.TryGetValue(path, out resObject))
        {
            return resObject as T;
        }

        resObject = m_ResourceLoader.LoadResource<T>(path, assetbundle);

        if (resObject == null)
        {
            Debug.LogError(string.Format("资源（{0}）加载失败", path));
            return null;
        }
        else
        {
            m_LoadObjectDict.Add(path, resObject);
        }

        return resObject as T;
    }

    /// <summary>
    /// 从Resources文件夹加载自愿
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResourceByResourcesFolder<T>(string path) where T : UnityEngine.Object
    {
        T resObject = Resources.Load<T>(path);
        return resObject;
    }

    /// <summary>
    /// 从文件路径加载字符串
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string LoadStringByFilePath(string path)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        string content = File.ReadAllText(path);
        return content;
    }

    //------------ 安全销毁 ------------

    public void DestroyGameObject(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("资源失败失败！因为资源为空");
            return;
        }
        GameObject.Destroy(go);
    }
}
