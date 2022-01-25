using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Stone_ResourceManager;

public class Stone_AssetBundleResourceLoader : Stone_IResourceLoader
{
    private AssetBundleManifest m_AssetBundleManifest;

    private Dictionary<string, AssetBundle> m_LoadAssetBundleDict;
    private Dictionary<string, ConfigureInfo> m_AssetBundleNameToInfoDict;

    public void Init()
    {
        m_LoadAssetBundleDict = new Dictionary<string, AssetBundle>();
        m_AssetBundleNameToInfoDict = new Dictionary<string, ConfigureInfo>();

        AssetBundle assetbundle = AssetBundle.LoadFromFile(Stone_RunTimeTool.GetPlatformFilePath("Main"));
        m_AssetBundleManifest = assetbundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

        Stone_UserConfigManager userConfigManager = Stone_RunTime.GetManager<Stone_UserConfigManager>(Stone_UserConfigManager.Name);
        AssetbundleConfigure configureInfos = userConfigManager.GetConfig<AssetbundleConfigure>();
        if (configureInfos == null)
        {
            LogHelper.Error?.Log(Stone_ResourceManager.Name, "Stone_AssetBundleResourceLoader", "get configureInfos Fail");
        }
        else
        {
            ConfigureInfo[] assetBundleConfigures = configureInfos.Configures;
            for (int index = 0; index < assetBundleConfigures.Length; index++)
            {
                ConfigureInfo info = assetBundleConfigures[index];

                if (!string.IsNullOrEmpty(info.AssetBundlePackagePath))
                {
                    m_AssetBundleNameToInfoDict.Add(info.AssetBundlePackagePath, info);
                }
                if (!string.IsNullOrEmpty(info.AssetBundlePackageUserName))
                {
                    m_AssetBundleNameToInfoDict.Add(info.AssetBundlePackageUserName, info);
                }
            }
        }
    }

    public void UnInit()
    {

    }

    public T LoadResource<T>(string resourcePath, string assetbundle) where T : Object
    {
        if (string.IsNullOrEmpty(assetbundle))
        {
            assetbundle = resourcePath;
        }

        string assetBundleName = assetbundle;
        assetBundleName = GetAssetBundleName(assetBundleName);

        AssetBundle resAssetBundle = null;
        if (!m_LoadAssetBundleDict.TryGetValue(assetBundleName, out resAssetBundle))
        {
            //加载依赖
            string[] dependences = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            for (int i = 0; i < dependences.Length; i++)
            {
                string dependenceName = dependences[i];

                LoadAssetBundle(dependenceName);
            }

            //加载ab
            resAssetBundle = LoadAssetBundle(assetBundleName);
        }

        if (resAssetBundle == null)
        {
            LogHelper.Error?.Log(Stone_ResourceManager.Name, "AB模式下找不到AssetBundle。", assetbundle);
            return null;
        }

        resourcePath = resourcePath.ToLower();

        string assetbundleName = string.Empty;
        string[] assetBundleNames = resAssetBundle.GetAllAssetNames();
        for (int index = 0; index < assetBundleNames.Length; index++)
        {
            string name = assetBundleNames[index];
            string nameWithoutExtension = name;
            int extensionIndex = nameWithoutExtension.LastIndexOf('.');
            if (extensionIndex != -1)
            {
                nameWithoutExtension = nameWithoutExtension.Substring(0, extensionIndex);
            }

            if (name.EndsWith(resourcePath) || nameWithoutExtension.EndsWith(resourcePath))
            {
                assetbundleName = name;
                break;
            }
        }

        if (!string.IsNullOrEmpty(assetbundleName))
        {
            //实例化
            T instance = resAssetBundle.LoadAsset<T>(assetbundleName);
            return instance;
        }

        LogHelper.Error?.Log(Stone_ResourceManager.Name, "AB模式下找不到资源。", resourcePath);
        return null;
    }

    private string GetAssetBundleName(string inputName)
    {
        string assetBundleName = string.Empty;

        string assetBundleDirectoryName = IOHelper.GetDirectoryName(inputName);
        do
        {
            AssetBundle resAssetBundle;
            if (m_LoadAssetBundleDict.TryGetValue(inputName, out resAssetBundle))
            {
                break;
            }

            if (m_LoadAssetBundleDict.TryGetValue(assetBundleDirectoryName, out resAssetBundle))
            {
                break;
            }

            ConfigureInfo configureInfo;
            if (!m_AssetBundleNameToInfoDict.TryGetValue(inputName, out configureInfo))
            {
                m_AssetBundleNameToInfoDict.TryGetValue(assetBundleDirectoryName, out configureInfo);
            }

            if (configureInfo == null)
            {
                break;
            }

            string fileName = configureInfo.AssetBundlePackageUserName;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = configureInfo.AssetBundlePackagePath;
            }

            assetBundleName = fileName.Replace("/", "@");
            assetBundleName += ".unity3d";
            assetBundleName = assetBundleName.ToLower();
        } while (false);

        if(string.IsNullOrEmpty(assetBundleName))
        {
            LogHelper.Error?.Log(Stone_ResourceManager.Name, "AB模式下找不到AssetBundle。", inputName);
        }

        return assetBundleName;
    }

    private AssetBundle LoadAssetBundle(string assetBundlePackageName)
    {
        AssetBundle resAssetBundle;
        do
        {
            if (m_LoadAssetBundleDict.TryGetValue(assetBundlePackageName, out resAssetBundle))
            {
                break;
            }

            string fullPath = Stone_RunTimeTool.GetPlatformFilePath(assetBundlePackageName);
            resAssetBundle = AssetBundle.LoadFromFile(fullPath);
            if (resAssetBundle != null)
            {
                m_LoadAssetBundleDict.Add(assetBundlePackageName, resAssetBundle);
                break;
            }
        } while (false);

        return resAssetBundle;
    }

}
