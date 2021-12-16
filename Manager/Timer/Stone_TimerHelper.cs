using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Stone_TimerHelper 
{
    public static void StopAllTimer(this Dictionary<string, int> timerDict)
    {
        Dictionary<string, int>.Enumerator enumerator = timerDict.GetEnumerator();

        while (enumerator.MoveNext())
        {
            int timer = enumerator.Current.Value;

            Stone_TimerManager timerManager = Stone_RunTime.GetManager<Stone_TimerManager>(Stone_TimerManager.Name);
            timerManager.StopTimer(timer);
        }
        timerDict.Clear();
    }

    public static void AddSequenceByOnlyRun(this Dictionary<string, int> timerDict, string name, int timer)
    {
        StopAllTimer(timerDict);

        timerDict.Add(name, timer);
    }
}
