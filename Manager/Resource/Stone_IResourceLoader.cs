using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Stone_IResourceLoader 
{
    void Init();

    void UnInit();

    T LoadResource<T>(string resourcePath, string assetbundle) where T : Object;
}
