using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Stone_UserConfigManager : Stone_Manager
{
    public const string Name = "Stone_UserConfigManager";
    public override string GetName()
    {
        return Stone_UserConfigManager.Name;
    }


    private string m_ConfigFolderPath;
    private Dictionary<string, string> m_UserConfigFilePathDict;               //配置文件路径列表 dict
    private Dictionary<string, Stone_BaseUserConfigData> m_UserConfigDataDict; //配置文件列表 dict

    public Stone_UserConfigManager(Stone_IManagerLifeControl stone_ManagerLifeControl) : base(stone_ManagerLifeControl)
    {
    }

    public override void Init()
    {
        m_UserConfigFilePathDict = new Dictionary<string, string>();
        m_UserConfigDataDict = new Dictionary<string, Stone_BaseUserConfigData>();
    }

    /// <summary>
    /// 设置配置文件文件夹路径
    /// </summary>
    /// <param name="path"></param>
    public void SetConfigFolderPath(string path)
    {
        m_ConfigFolderPath = path;
    }

    /// <summary>
    /// 添加配置文件的路径
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="filePath"></param>
    public void AddConfigFilePath(string configName,string filePath)
    {
        m_UserConfigFilePathDict.Add(configName, filePath);
    }

    //获取配置文件名称
    private string GetConfigName<T>() where T : Stone_BaseUserConfigData
    {
        Type configType = typeof(T);
        return configType.Name;
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <param name="configName"></param>
    /// <returns></returns>
    private string GetConfigFilePath(string configName)
    {
        string filePath;
        if (m_UserConfigFilePathDict.TryGetValue(configName, out filePath))
        {
            return m_ConfigFolderPath + "/" + filePath;
        }
        else
        {
            return m_ConfigFolderPath + "/" + configName + ".txt";
        }
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configName"></param>
    /// <returns></returns>
    private T LoadConfig<T>(string configName) where T : Stone_BaseUserConfigData
    {
        Stone_BaseUserConfigData configData = null;

        if (m_UserConfigDataDict.TryGetValue(configName, out configData))
        {
            return (T)configData;
        }

        string configFilePath = GetConfigFilePath(configName);
        if (File.Exists(configFilePath))
        {
            string configContent = File.ReadAllText(configFilePath);
            if (!string.IsNullOrEmpty(configContent))
            {
                configData = LitJson.JsonMapper.ToObject<T>(configContent);
            }
        }
        m_UserConfigDataDict.Add(configName, configData);

        if (configData == null)
        {
            return null;
        }
        else
        {
            return (T)configData;
        }
    }

    /// <summary>
    /// 获取配置类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configName"></param>
    /// <returns></returns>
    public T GetConfig<T>() where T : Stone_BaseUserConfigData
    {
        string configName = GetConfigName<T>();

        T configData = LoadConfig<T>(configName);

        return configData;
    }

    public T GetConfig<T>(string name) where T : Stone_BaseUserConfigData
    {
        T configData = LoadConfig<T>(name);

        return configData;
    }

    /// <summary>
    /// 穿入字符串得到获得配置类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <returns></returns>
    public T GetConfigByUserContent<T>(string content)
    {
        T configData = LitJson.JsonMapper.ToObject<T>(content);
        return configData;
    }
}
