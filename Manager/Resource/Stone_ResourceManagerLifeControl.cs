using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Stone_ResourceManagerLifeControl : Stone_IManagerLifeControl
{

    // 后期改成读配置
    private bool m_UseAssetBundle = false;
    public bool UseAssetBundle
    {
        set { m_UseAssetBundle = value; }
    }

    public void InitAfter(Stone_Manager manager)
    {
        Stone_IResourceLoader loader = null;

        if (!m_UseAssetBundle && Application.isEditor)
        {
            string path = Application.dataPath + "/../Library/ScriptAssemblies/Assembly-CSharp-Editor.dll";
            Assembly assembly = Assembly.LoadFile(path);
            Type type = assembly.GetType("Stone_EditorResourceLoader");
            System.Object editorLoader = Activator.CreateInstance(type, null);

            loader = editorLoader as Stone_IResourceLoader;
        }
        else
        {
            loader = new Stone_AssetBundleResourceLoader();
        }

        Stone_ResourceManager resourceManager = (Stone_ResourceManager)manager;
        resourceManager.SetResourceLoader(loader);
    }
}
