﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Stone_RunTimeTool
{

    /// <summary>
    /// 目錄名称
    /// </summary>
    public const string FRAMEWORK_NAME = "Stone";

    /// <summary>
    /// 取得数据存放目录
    /// </summary>
    private static string m_DataPath = string.Empty;

    public static string DataPath
    {
        get
        {
            if (!string.IsNullOrEmpty(m_DataPath))
            {
                return m_DataPath;
            }
            else
            {
                string folder = FRAMEWORK_NAME.ToLower();
                if (Application.isMobilePlatform)
                {
                    m_DataPath = Application.persistentDataPath + "/" + folder + "/";
                }
                else
                {
                    m_DataPath = Application.dataPath + "/../c/" + folder + "/";
                }

                return m_DataPath;
            }
        }
    }


    /// <summary>
    /// 应用程序内容路径
    /// </summary>
    private static string m_AppContentPath = string.Empty;
    public static string AppContentPath
    {
        get
        {
            if (!string.IsNullOrEmpty(m_AppContentPath))
            {
                return m_AppContentPath;
            }
            else
            {
                m_AppContentPath = Application.streamingAssetsPath + "/";
                return m_AppContentPath;
            }
        }
    }

    /// <summary>
    /// win编辑器内容路径
    /// </summary>
    private static string m_EditorPath = string.Empty;
    public static string EditorPath
    {
        get
        {
            if (!string.IsNullOrEmpty(m_EditorPath))
            {
                return m_EditorPath;
            }
            else
            {
                m_EditorPath = Application.dataPath + "/";
                return m_EditorPath;
            }
        }
    }

    public static bool DoublePath
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return false;
            }
            return true;
        }
    }

    public static string GetRealPath(string filename)
    {
        if (DoublePath)
        {
            string persistentPath = DataPath + filename;
            if (File.Exists(persistentPath))
            {
                return persistentPath;
            }
            else
            {
                persistentPath = AppContentPath + filename;
                if(File.Exists(persistentPath))
                {
                    return persistentPath;
                }
                return EditorPath + filename;
            }
        }
        else
        {
            return DataPath + filename;
        }
    }
}
