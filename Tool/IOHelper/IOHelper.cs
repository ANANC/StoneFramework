using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IOHelper 
{
    /// <summary>
    /// 创建文件夹(遍历父节点）
    /// </summary>
    /// <param name="directoryPath"></param>
    public static void SafeCreateDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
        {
            return;
        }

        if (!Directory.Exists(directoryPath))
        {
            string parent = GetDirectoryPath(directoryPath);
            if (!string.IsNullOrEmpty(parent))
            {
                SafeCreateDirectory(parent);
            }

            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 获得文件夹路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetDirectoryPath(string path)
    {
        int index = path.LastIndexOf("/");
        if (index != -1)
        {
            string parent = path.Remove(index, path.Length - index);
            return parent;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获得文件夹路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetDirectoryName(string path)
    {
        int index = path.IndexOf("/");
        if (index != -1)
        {
            string parent = path.Remove(index, path.Length - index);
            return parent;
        }

        return string.Empty;
    }

    /// <summary>
    /// 安全拷贝文件
    /// </summary>
    /// <param name="recover"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public static void SafeFileCopy(bool recover, string source, string target)
    {
        if (!File.Exists(source))
        {
            return;
        }

        if (recover && File.Exists(target))
        {
            File.Delete(target);
        }

        string targetDirectory = GetDirectoryPath(target);
        SafeCreateDirectory(targetDirectory);

        File.Copy(source, target);
    }

    /// <summary>
    /// 安全文件夹拷贝
    /// </summary>
    /// <param name="recover"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="searchPattern"></param>
    /// <param name="searchOption"></param>
    public static void SafeDirectoryCopy(bool recover, string source, string target, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(source))
        {
            return;
        }

        if (recover && Directory.Exists(target))
        {
            Directory.Delete(target, true);
        }

        string[] files = Directory.GetFiles(source, searchPattern, SearchOption.TopDirectoryOnly);
        for (int index = 0; index < files.Length; index++)
        {
            string name = Path.GetFileName(files[index]);
            SafeFileCopy(recover, files[index], target + "/" + name);
        }

        if (searchOption == SearchOption.AllDirectories)
        {
            string[] directorys = Directory.GetDirectories(source, searchPattern, SearchOption.TopDirectoryOnly);
            for (int index = 0; index < directorys.Length; index++)
            {
                string name = Path.GetFileName(directorys[index]);
                SafeDirectoryCopy(recover, directorys[index], target + "/" + name);
            }
        }
    }
}
