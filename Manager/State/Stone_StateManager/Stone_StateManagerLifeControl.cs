using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_StateManagerLifeControl : Stone_IManagerLifeControl
{
    public void InitAfter(Stone_Manager manager)
    {
        Stone_StateManager stateManager = (Stone_StateManager)manager;

        stateManager.AddStateType<TestState>(TestState.Name);
    }
}
