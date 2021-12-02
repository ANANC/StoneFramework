using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stone_Manager : Stone_UpdateListHelper.UpdateObjcect
{

    public Stone_Manager()
    {
    }

    //---生命周期函数

    //初始化 只执行一次
    public virtual void Init()
    {
    }

    //激活执行
    public virtual void Active()
    {
    }

    public virtual void Update()
    {
    }

    //休眠执行
    public virtual void Dormancy()
    {
    }

    //销毁 只执行一次
    public virtual void Destroy()
    {
    }

    //---功能

    public abstract string GetName();

}
