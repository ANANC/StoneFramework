using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Stone_ResourceManager;

public class Stone_EditorResourceLoader : Stone_IResourceLoader
{
    private string m_ResourceRootFullPath;
    private string m_ProjectPath;

    private Dictionary<string, ConfigureInfo> m_AssetBundleNameToInfoDict;
    private Dictionary<ConfigureInfo, bool> m_AssetBundleLoadDict;
    private Dictionary<string, List<string>> m_FullPathDict ;
    private Dictionary<string, List<string>> m_WithoutExtensionPathDict ;

    public void Init()
    {
        m_ResourceRootFullPath = Application.dataPath+"/";
        m_ProjectPath = Application.dataPath.Replace("Assets", string.Empty);

        Stone_UserConfigManager userConfigManager = Stone_RunTime.GetManager<Stone_UserConfigManager>(Stone_UserConfigManager.Name);

        AssetbundleConfigure configureInfos = userConfigManager.GetConfig<AssetbundleConfigure>();
        if(configureInfos == null)
        {
            LogHelper.Error?.Log(Stone_ResourceManager.Name, "Stone_EditorResourceLoader", "get configureInfos Fail");
            return;
        }

        ConfigureInfo[] assetBundleConfigures = configureInfos.Configures;

        int length = assetBundleConfigures.Length;
        m_AssetBundleNameToInfoDict = new Dictionary<string, ConfigureInfo>(length);
        m_AssetBundleLoadDict = new Dictionary<ConfigureInfo, bool>(length);
        m_FullPathDict = new Dictionary<string, List<string>>(length);
        m_WithoutExtensionPathDict = new Dictionary<string, List<string>>(length);

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

            m_AssetBundleLoadDict.Add(info, false);
        }

    }

    public void UnInit()
    {

    }


    public T LoadResource<T>(string resourcePath, string assetbundle = null) where T : Object
    {
        T resource = null;

        if (string.IsNullOrEmpty(assetbundle))
        {
            assetbundle = resourcePath;
        }

        ConfigureInfo info = null;
        do
        {
            info = GetConfigureInfo(assetbundle);
            if(info!=null)
            {
                break;
            }

            assetbundle = IOHelper.GetDirectoryName(resourcePath);
            info = GetConfigureInfo(assetbundle);
            if (info != null)
            {
                break;
            }

        } while (false);

        if (info == null)
        {
            LogHelper.Error?.Log(Stone_ResourceManager.Name, "编辑器模式下找不到资源。", assetbundle); 
            return resource;
        }

        LoadResourcesByAssetbundle(assetbundle);

        bool isWithoutExtension = string.IsNullOrEmpty(Path.GetExtension(resourcePath));

        string resourceFullPath = null;
        if (isWithoutExtension)
        {
            List<string> withoutExtensionPathList;
            if (m_WithoutExtensionPathDict.TryGetValue(info.AssetBundlePackagePath, out withoutExtensionPathList))
            {
                int count = withoutExtensionPathList.Count;
                for (int index = 0; index < count; index++)
                {
                    string withoutExtensionPath = withoutExtensionPathList[index];
                    if (withoutExtensionPath.EndsWith(resourcePath))
                    {
                        List<string> fullPathList;
                        if (m_FullPathDict.TryGetValue(info.AssetBundlePackagePath, out fullPathList))
                        {
                            resourceFullPath = fullPathList[index];
                        }
                        break;
                    }
                }
            }
        }
        else
        {
            List<string> fullPathList;
            if (m_FullPathDict.TryGetValue(info.AssetBundlePackagePath, out fullPathList))
            {
                int count = fullPathList.Count;
                for (int index = 0; index < count; index++)
                {
                    string fullPath = fullPathList[index];
                    if (fullPath.EndsWith(resourcePath))
                    {
                        resourceFullPath = fullPath;
                        break;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(resourceFullPath))
        {
            resource = AssetDatabase.LoadAssetAtPath<T>(resourceFullPath);
        }

        return resource;
    }


    /// <summary>
    /// 获取打包配置信息
    /// </summary>
    /// <param name="assetbundle">别名/路径</param>
    /// <returns></returns>
    private ConfigureInfo GetConfigureInfo(string assetbundle)
    {
        ConfigureInfo info;

        //参数=别名/路径，根据名称获取配置
        if (!m_AssetBundleNameToInfoDict.TryGetValue(assetbundle, out info))
        {
            return null;
        }

        return info;
    }

    /// <summary>
    /// 通过ab信息加载全部包资源
    /// </summary>
    /// <param name="assetbundle"></param>
    private void LoadResourcesByAssetbundle(string assetbundle)
    {
        ConfigureInfo info = GetConfigureInfo(assetbundle);

        if(info == null)
        {
            LogHelper.Error?.Log(Stone_ResourceManager.Name, "编辑器模式下找不到资源。", assetbundle);
            return;
        }

        //已经加载了
        bool isLoad = m_AssetBundleLoadDict[info];
        if (isLoad)
        {
            return;
        }

        m_AssetBundleLoadDict[info] = true;

        //得到资料目录
        string editorPath =  m_ResourceRootFullPath + info.AssetBundlePackagePath;

        //文件类型
        if (File.Exists(editorPath))
        {
            editorPath = editorPath.Replace(m_ProjectPath, string.Empty);

            string fullPath = editorPath;
            string withoutExtensionPath = editorPath;

            m_FullPathDict.Add(info.AssetBundlePackagePath, new List<string> { fullPath });
            m_WithoutExtensionPathDict.Add(info.AssetBundlePackagePath, new List<string> { withoutExtensionPath });
        }
        //文件夹类型
        else
        {
            string[] files = Directory.GetFiles(editorPath, "*", SearchOption.AllDirectories);

            List<string> withoutExtensionPathList = new List<string>(files.Length / 2);
            List<string> fullPathList = new List<string>();


            for (int index = 0; index < files.Length; index++)
            {
                string fileName = Path.GetFileName(files[index]);
                if (fileName.StartsWith(".") || fileName.EndsWith("meta") || fileName.EndsWith("nametip"))
                {
                    continue;
                }
                string filePath = files[index].Replace("\\", "/").Replace(m_ProjectPath,string.Empty);
                fullPathList.Add(filePath);

                int extensionIndex = filePath.LastIndexOf('.');
                if (extensionIndex != -1)
                {
                    filePath = filePath.Substring(0, extensionIndex);
                }
                withoutExtensionPathList.Add(filePath);
            }

            m_FullPathDict.Add(info.AssetBundlePackagePath, fullPathList);
            m_WithoutExtensionPathDict.Add(info.AssetBundlePackagePath, withoutExtensionPathList);
        }
    }

}
