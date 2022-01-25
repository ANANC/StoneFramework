using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DoTweenHelper
{
    public static void StopAllSequence(this Dictionary<string, Sequence> sequenceDict)
    {
        Dictionary<string, Sequence>.Enumerator enumerator = sequenceDict.GetEnumerator();

        while (enumerator.MoveNext())
        {
            Sequence tween = enumerator.Current.Value;
            tween.Kill();
        }
        sequenceDict.Clear();
    }

    public static void AddSequenceByOnlyRun(this Dictionary<string, Sequence> sequenceDict, string name, Sequence sequence)
    {
        StopAllSequence(sequenceDict);

        sequenceDict.Add(name, sequence);
    }

    public static void AddSequenceBySingleRun(this Dictionary<string, Sequence> sequenceDict, string name, Sequence sequence)
    {
        Sequence tween;
        if (sequenceDict.TryGetValue(name,out tween))
        {
            tween.Kill();
            sequenceDict.Remove(name);
        }

        sequenceDict.Add(name, sequence);
    }
}
