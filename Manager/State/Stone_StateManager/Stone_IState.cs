using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Stone_IState 
{
    string GetName();

    void Init();

    void UnInit();

    void Enter();

    void Exist();
}
