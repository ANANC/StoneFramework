using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_UIManagerLifeControl : Stone_IManagerLifeControl
{
    public void InitAfter(Stone_Manager manager)
    {
        Stone_UIManager uiManager = (Stone_UIManager)manager;

        uiManager.SetResourceRootPath("UI");
        uiManager.SetUICanvasResourcesPath("UICanvas");

        uiManager.AddCreateUIObjectFunc(GameMainPanel.Name, GameMainPanel.CreateUI);
    }
}
