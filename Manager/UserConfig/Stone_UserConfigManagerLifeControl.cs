using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Stone_UserConfigManagerLifeControl : Stone_IManagerLifeControl
{
    private const string ConfigFolder = "Config";

    public void InitAfter(Stone_Manager manager)
    {
        Stone_UserConfigManager userConfigManager = (Stone_UserConfigManager)manager;

        //配置文件文件夹
        string configFolderFullPath = Stone_RunTimeTool.GetPlatformDirectoryPath(ConfigFolder);
        userConfigManager.SetConfigFolderPath(configFolderFullPath);

        //配置文件
        //后续：在打包的时候，生成列表文件读取
        string configFloderFullPath = Stone_RunTimeTool.GetPlatformDirectoryPath(ConfigFolder);
        string[] filePaths = Directory.GetFiles(configFloderFullPath, "*.txt", SearchOption.AllDirectories);
        for (int index = 0; index < filePaths.Length; index++)
        {
            string filePath = filePaths[index];

            filePath = filePath.Replace("\\", "/").Replace(configFloderFullPath, string.Empty);
            if (filePath.StartsWith("/"))
            {
                filePath = filePath.Substring(1, filePath.Length - 1);
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            userConfigManager.AddConfigFilePath(fileNameWithoutExtension, filePath);
        }
    }
}
