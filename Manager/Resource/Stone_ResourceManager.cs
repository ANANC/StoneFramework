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

    public enum ConfigureType
    {
        File = 1, //原文件拷贝
        Directory = 2, //原目录拷贝
        F2O = 3, //单独文件打包成一个AB
        EF2O = 4, //文件夹下的每一个文件打包成一个AB   递归
        D2O = 5, //整个文件夹打包成一个AB  递归
        ED2O = 6, //目录下每个文件夹达成一个AB 不递归
    }

    [SerializeField]
    public class AssetbundleConfigure : Stone_BaseUserConfigData
    {
        public ConfigureInfo[] Configures;
    }

    public class ConfigureInfo
    {
        public int ConfigureType;                    //配置类型 对应枚举ConfigureType
        public string AssetBundlePackagePath;        //ab包路径
        public string AssetBundlePackageUserName;    //ab包使用别名
        public bool IsBuildDependent;                //是否打包依赖
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
    public T LoadResource<T>(string path, string assetbundle = null) where T : UnityEngine.Object
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

    public void DestroyGameObject(Transform transform)
    {
        if (transform != null)
        {
            DestroyGameObject(transform.gameObject);
        }
        else
        {
            LogHelper.Trace?.Log("资源失败失败！因为资源为空");
        }
    }

    public void DestroyGameObject(GameObject go)
    {
        if (go == null)
        {
            LogHelper.Trace?.Log("资源失败失败！因为资源为空");
            return;
        }
        GameObject.Destroy(go);
    }
}
