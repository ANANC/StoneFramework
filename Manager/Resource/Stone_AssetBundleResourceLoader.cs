using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_AssetBundleResourceLoader : Stone_IResourceLoader
{
    private AssetBundleManifest m_AssetBundleManifest;

    private Dictionary<string, AssetBundle> m_LoadAssetBundleDict = new Dictionary<string, AssetBundle>();

    public void Init()
    {
        AssetBundle assetbundle = AssetBundle.LoadFromFile(Stone_RunTimeTool.GetRealPath("Main"));
        m_AssetBundleManifest = assetbundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
    }

    public void UnInit()
    {

    }

    public T LoadResource<T>(string resourcePath, string assetbundle) where T : Object
    {
        if(string.IsNullOrEmpty(assetbundle))
        {
            assetbundle = resourcePath;
        }

        string assetBundleName = assetbundle + ".unity3d";

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

        if (resAssetBundle != null)
        {
            resourcePath = resourcePath.ToLower();

            string[] assetBundleNames = resAssetBundle.GetAllAssetNames();
            for (int index = 0; index < assetBundleNames.Length; index++)
            {
                string assetBunldNameWithoutExtension = assetBundleNames[index];
                int extensionIndex = assetBunldNameWithoutExtension.LastIndexOf('.');
                if (extensionIndex != -1)
                {
                    assetBunldNameWithoutExtension = assetBunldNameWithoutExtension.Substring(0, extensionIndex);
                }

                if (assetBunldNameWithoutExtension.EndsWith(resourcePath))
                {
                    //实例化
                    T instance = resAssetBundle.LoadAsset<T>(assetBundleNames[index]);
                    return instance;
                }
            }
        }

        return null;
    }

    private AssetBundle LoadAssetBundle(string assetBundleName)
    {
        AssetBundle resAssetBundle = null;
        if (!m_LoadAssetBundleDict.TryGetValue(assetBundleName, out resAssetBundle))
        {
            string fullPath = Stone_RunTimeTool.GetRealPath(assetBundleName);
            resAssetBundle = AssetBundle.LoadFromFile(fullPath);
            if (resAssetBundle != null)
            {
                m_LoadAssetBundleDict.Add(assetBundleName, resAssetBundle);
            }
            else
            {
                Debug.Log("【assetbundle load】 fail! path:" + fullPath);
            }
        }

        return resAssetBundle;
    }

}
