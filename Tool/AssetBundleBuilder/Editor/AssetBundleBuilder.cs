#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Stone_ResourceManager;

public class AssetBundleBuilder
{
    private const string AssetBundleDirectory = "../output/AssetBundles";
    private readonly string ProjectPath = Application.dataPath.Replace("Assets", string.Empty);
    private readonly string AssetPath = Application.dataPath + "/";
    private const string ManifestName = "Main";

    public class BuildInfo
    {
        public AssetBundleBuild AssetBundleBuild;
        public string Path;
        public string Name;

        public BuildInfo(string path, string name)
        {
            Path = path;
            Name = name;
            AssetBundleBuild = new AssetBundleBuild();
        }
    }

    private AssetbundleConfigure m_AssetbundleConfigure;

    private string m_OutputPath;

    private BuildTarget m_BuildTarget;
    private BuildAssetBundleOptions m_BuildAssetBundleOptions;
    private string m_BuildTargetName;

    private Dictionary<string, BuildInfo> m_BuildInfoDict;

    [MenuItem("Stone/Build AssetBundle/Clean All", false, 200)]
    public static void CleanAll()
    {
        AssetBundleBuilder assetBundleBuilder = new AssetBundleBuilder();
        assetBundleBuilder.ClearAssetBundle();
    }


    [MenuItem("Stone/Build AssetBundle/Windows", false, 200)]
    public static void BuildWindowsResource()
    {
        AssetBundleBuilder assetBundleBuilder = new AssetBundleBuilder();
        assetBundleBuilder.Init();
        assetBundleBuilder.Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Stone/Build AssetBundle/Android", false, 200)]
    public static void BuildAndroidResource()
    {
        AssetBundleBuilder assetBundleBuilder = new AssetBundleBuilder();
        assetBundleBuilder.Init();
        assetBundleBuilder.Build(BuildTarget.Android);
    }

    [MenuItem("Stone/Build AssetBundle/iOS", false, 200)]
    public static void BuildiOSResource()
    {
        AssetBundleBuilder assetBundleBuilder = new AssetBundleBuilder();
        assetBundleBuilder.Init();
        assetBundleBuilder.Build(BuildTarget.iOS);
    }

    private void Init()
    {
        Stone_RunTime.Current = new Stone_RunTime();
        Stone_RunTime.AddManager(new Stone_UserConfigManager(new Stone_UserConfigManagerLifeControl()));

        Stone_UserConfigManager userConfigManager = Stone_RunTime.GetManager<Stone_UserConfigManager>(Stone_UserConfigManager.Name);
        m_AssetbundleConfigure = userConfigManager.GetConfig<AssetbundleConfigure>();
        if (m_AssetbundleConfigure == null)
        {
            LogHelper.Error?.Log("AssetBundleBuilder", "Get configureInfos Fail");
        }

        m_BuildInfoDict = new Dictionary<string, BuildInfo>();
    }

    public void Build(BuildTarget buildTarget,
        BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.ChunkBasedCompression |
                                               BuildAssetBundleOptions.DisableWriteTypeTree |
                                               BuildAssetBundleOptions.DisableLoadAssetByFileName)
    {
        m_BuildTarget = buildTarget;
        m_BuildTargetName = GetBuildTargetName(buildTarget);
        m_BuildAssetBundleOptions = buildOptions;

        m_OutputPath = Application.dataPath + "/" + AssetBundleDirectory + "/" + m_BuildTargetName;
        IOHelper.SafeCreateDirectory(m_OutputPath);

        LogHelper.Trace?.Log("AssetBundleBuilder", "[Building..]", "平台(", m_BuildTargetName, ") \nOutputPath:", m_OutputPath);

        _Build();
    }

    private void _Build(bool clear = false)
    {
        //清理旧资源
        if (clear)
        {
            ClearAssetBundle();
        }

        //AssetBundle Build
        try
        {
            ConfigureInfo[] configureInfos = m_AssetbundleConfigure.Configures;
            if (configureInfos == null || configureInfos.Length == 0)
            {
                LogHelper.Trace?.Log("AssetBundleBuilder", "[Building..]", "ConfigureInfo is null");
                return;
            }

            float infoCount = configureInfos.Length;
            for (int i = 0; i < infoCount; i++)
            {
                ConfigureInfo configureInfo = configureInfos[i];
                EditorUtility.DisplayProgressBar("Build AssetBundle", configureInfo.AssetBundlePackagePath, i / infoCount);

                ConfigureControl(configureInfo);
            }
        }
        catch (Exception e)
        {
            LogHelper.Error?.Log("AssetBundleBuilder", "[Building..]", "生成异常！ message:", e.Message);
            return;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetBundleBuild[] builds = new AssetBundleBuild[m_BuildInfoDict.Count];
        int index = 0;
        Dictionary<string, BuildInfo>.Enumerator enumerator = m_BuildInfoDict.GetEnumerator();
        while (enumerator.MoveNext())
        {
            BuildInfo info = enumerator.Current.Value;
            info.AssetBundleBuild.assetBundleName = info.AssetBundleBuild.assetBundleName.Replace("/", "@");
            builds[index++] = info.AssetBundleBuild;
        }
        m_BuildInfoDict.Clear();

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(m_OutputPath, builds, m_BuildAssetBundleOptions, m_BuildTarget);

        string mainManifest = m_OutputPath + "/" + ManifestName;
        string curManifest = m_OutputPath + "/" + m_BuildTargetName;

        if (File.Exists(mainManifest))
        {
            File.Delete(mainManifest);
        }
        if (File.Exists(curManifest))
        {
            //清理旧资源
            DeleteUnusedAssets();

            // Manifest改名
            File.Move(curManifest, mainManifest);

            //更新StreamingAssets
            IOHelper.SafeDirectoryCopy(true, m_OutputPath, Application.streamingAssetsPath);
        }

        LogHelper.Trace.Log("AssetBundleBuilder", "[Building..]", "Build AssetBundle Success");

        AssetDatabase.Refresh();
    }

    private void ClearAssetBundle()
    {
        string fullOutputAssetBundlePath = Application.dataPath + "/" + AssetBundleDirectory;
        if (Directory.Exists(fullOutputAssetBundlePath))
        {
            Directory.Delete(fullOutputAssetBundlePath, true);
        }

        if (Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.Delete(Application.streamingAssetsPath, true);
        }

        AssetDatabase.Refresh();

        LogHelper.Trace?.Log("AssetBundleBuilder","[Building..]"," Clear Finish");
    }

    private void ConfigureControl(ConfigureInfo info)
    {
        string resourcePath = Application.dataPath + "/" + info.AssetBundlePackagePath;
        string userName = info.AssetBundlePackageUserName;

        switch ((ConfigureType)info.ConfigureType)
        {
            case ConfigureType.File:
                BuildConfigure_File(resourcePath, userName);
                break;
            case ConfigureType.Directory:
                BuildConfigure_Directory(resourcePath, userName);
                break;
            case ConfigureType.F2O:
                BuildConfigure_FileToAB(info.IsBuildDependent, resourcePath, userName);
                break;
            case ConfigureType.EF2O:
                BuildConfigure_DirectoryEventFileToAB(resourcePath, userName);
                break;
            case ConfigureType.D2O:
                BuildConfigure_DirectoryToAB(resourcePath, userName);
                break;
            case ConfigureType.ED2O:
                BuildConfigure_EventDirectoryToAB(resourcePath, userName);
                break;
        }
    }


    // -- build Configure --

    private void BuildConfigure_File(string filePath, string userName = null) //"Game/Resource/Config/a.txt"
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = filePath;
        }
        userName = userName.Replace(AssetPath, string.Empty);

        string newPath = m_OutputPath + "/" + userName;
        IOHelper.SafeCreateDirectory(IOHelper.GetDirectoryPath(newPath));

        File.Copy(filePath, newPath);
    }

    private void BuildConfigure_Directory(string directoryPath, string userName = null) //"Game/Resource/Config"
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = directoryPath;
        }

        List<string> fileList = new List<string>();
        GetDirectoryFiles(fileList, string.Empty, directoryPath, new[] { "*" }, SearchOption.AllDirectories);

        for (int index = 0; index < fileList.Count; index++)
        {
            string filePath = fileList[index];
            if (filePath.EndsWith(".meta") || filePath.EndsWith(".nametip") || filePath.StartsWith("."))
            {
                continue;
            }

            string fileName = filePath.Replace(userName, string.Empty);
            string newPath = m_OutputPath + "/" + fileName;

            IOHelper.SafeFileCopy(true, AssetPath + filePath, newPath);
        }

    }

    private void BuildConfigure_FileToAB(bool dependent, string filePath, string userName = null) //"Game/Reousrce/Prefab/A"
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = filePath;
        }

        if (dependent)
        {
            string[] dependencies = AssetDatabase.GetDependencies(filePath);
            for (int index = 0; index < dependencies.Length; index++)
            {
                string dependPath = dependencies[index].Replace("\\", "/");
                if (Path.GetExtension(dependPath) == "cs")
                {
                    continue;
                }

                string dependenceUserName = dependPath.Replace(AssetPath, string.Empty);
                dependenceUserName = dependenceUserName.Replace(m_OutputPath+"/", string.Empty);
                AssetBundleBuild_FileToAssetBundle(dependPath, dependenceUserName);
            }
        }

        AssetBundleBuild_FileToAssetBundle(filePath, userName);
    }

    private void BuildConfigure_DirectoryEventFileToAB(string directoryPath, string userName = null) //"Game/Reousrce/Prefab"
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = directoryPath;
        }

        List<string> fileList = new List<string>();
        GetDirectoryFiles(fileList, directoryPath + "/", directoryPath, new[] { "*" }, SearchOption.AllDirectories);

        for (int index = 0; index < fileList.Count; index++)
        {
            string curFilePath = fileList[index].Replace("\\", "/");
            string curUserName = fileList[index].Replace("\\", "/").Replace(directoryPath, userName);

            AssetBundleBuild_FileToAssetBundle(curFilePath, curUserName);
        }
    }

    private void AssetBundleBuild_FileToAssetBundle(string filePath, string userName)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        string extension = Path.GetExtension(userName);
        string assetBundleName = userName.Replace(extension, string.Empty);
        assetBundleName = assetBundleName.Replace(AssetPath, string.Empty);

        BuildInfo assetBundleBuild;
        if (!m_BuildInfoDict.TryGetValue(assetBundleName, out assetBundleBuild))
        {
            assetBundleBuild = new BuildInfo(filePath, userName);
            assetBundleBuild.AssetBundleBuild.assetBundleName = assetBundleName + ".unity3d";
            assetBundleBuild.AssetBundleBuild.assetNames = new string[] { filePath.Replace(AssetPath, string.Empty) };
            m_BuildInfoDict.Add(assetBundleName, assetBundleBuild);
        }
        else
        {
            LogHelper.Error?.Log("AssetBundleBuilder", "[Building..]", "【Build AssetBundle】AB名（", assetBundleName, "）重复！\nAB资源路径：", assetBundleBuild.Path, "\n 新资源路径：", filePath);
        }
    }

    private void BuildConfigure_DirectoryToAB(string directoryPath, string userName = null) //"Game/Reource/Font"
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = directoryPath;
        }

        AssetBundleBuild_DirectoryToAssetBundle(directoryPath, userName);
    }

    private void BuildConfigure_EventDirectoryToAB(string directoryPath, string userName = null) //"Game/Reource"
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = directoryPath;
        }

        string[] directories = Directory.GetDirectories(directoryPath);
        for (int index = 0; index < directories.Length; index++)
        {
            string curDirectoryPath = directories[index].Replace("\\", "/");
            string curUserName = directories[index].Replace("\\", "/").Replace(directoryPath, userName);

            AssetBundleBuild_DirectoryToAssetBundle(curDirectoryPath, curUserName);
        }
    }

    private void AssetBundleBuild_DirectoryToAssetBundle(string directoryPath, string directoryName)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        if (Path.GetFileName(directoryPath)[0] == '.')
        {
            return;
        }

        string assetBundleName = directoryName;
        assetBundleName = assetBundleName.Replace(AssetPath, string.Empty);

        BuildInfo assetBundleBuild;
        if (!m_BuildInfoDict.TryGetValue(assetBundleName, out assetBundleBuild))
        {
            List<string> fileList = new List<string>();
            GetDirectoryFiles(fileList, ProjectPath, directoryPath, new[] { "*" }, SearchOption.AllDirectories);

            assetBundleBuild = new BuildInfo(directoryPath, directoryName);
            assetBundleBuild.AssetBundleBuild.assetBundleName = assetBundleName + ".unity3d";
            assetBundleBuild.AssetBundleBuild.assetNames = fileList.ToArray();
            m_BuildInfoDict.Add(assetBundleName, assetBundleBuild);
        }
        else
        {
            LogHelper.Error?.Log("AssetBundleBuilder", "[Building..]", "【Build AssetBundle】AB名（", assetBundleName,"）重复！\nAB资源路径：", assetBundleBuild.Path, "\n新资源路径：", directoryPath);
        }
    }


    private void GetDirectoryFiles(List<string> fileList, string rootPath, string path, string[] pattern, SearchOption searchOption)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] == "*")
            {
                string[] searchFiles = Directory.GetFiles(path, pattern[i], searchOption);
                for (int index = 0; index < searchFiles.Length; index++)
                {
                    string fileName = Path.GetFileName(searchFiles[index]);
                    if (fileName.EndsWith(".meta") || fileName.EndsWith(".nametip") || fileName.StartsWith("."))
                    {
                        continue;
                    }

                    string file = searchFiles[index].Replace("\\", "/");
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        file = file.Replace(rootPath, string.Empty);
                    }
                    file = file.Replace(AssetPath, string.Empty);

                    fileList.Add(file);
                }
            }
            else
            {
                string[] files = Directory.GetFiles(path, "*." + pattern[i], searchOption);
                for(int index = 0;index<files.Length;index++)
                {
                    string file = files[index];

                    string fileName = Path.GetFileName(file);
                    if (fileName.EndsWith(".meta") || fileName.EndsWith(".nametip") || fileName.StartsWith("."))
                    {
                        continue;
                    }

                    file = file.Replace("\\", "/");
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        file = file.Replace(rootPath, string.Empty);
                    }
                    file = file.Replace(AssetPath, string.Empty);

                    fileList.Add(file);
                }
            }
        }
    }

    private string GetBuildTargetName(BuildTarget target)
    {
        if (target == BuildTarget.iOS)
        {
            return "ios";
        }
        else if (target == BuildTarget.Android)
        {
            return "android";
        }
        else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            return "windows";
        }
        else
        {
            return Enum.GetName(typeof(BuildTarget), target);
        }
    }

    private void DeleteUnusedAssets()
    {
        string[] originBundleList = Directory.GetFiles(m_OutputPath, "*.unity3d");
        if (originBundleList.Length > 0)
        {
            string path = m_OutputPath + "/" + m_BuildTargetName;
            AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
            AssetBundleManifest assetBundleManifest = assetbundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

            string[] files = assetBundleManifest.GetAllAssetBundles();
            HashSet<string> newFileHash = new HashSet<string>();
            foreach (string file in files)
            {
                newFileHash.Add(file);
            }

            foreach (string file in originBundleList)
            {
                string fileName = Path.GetFileName(file);
                if (!newFileHash.Contains(fileName))
                {
                    LogHelper.Trace.Log("AssetBundleBuilder", "[Building..]", "delete an unused assset , " + file);
                    File.Delete(file);
                    File.Delete(file + ".manifest");
                }
            }
            assetbundle.Unload(true);
        }
    }
}


#endif