using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_SoundManagerLifeControl : Stone_IManagerLifeControl
{
    public void InitAfter(Stone_Manager manager)
    {
        Stone_SoundManager soundManager = (Stone_SoundManager)manager;

        soundManager.SetResourcePath("Sound");
        soundManager.SetSoundPrefabPath("Model/200001_Sound/Prefab/200001_Sound.prefab");
    }
}
