using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum TimelineType
{
    PlayOnAwake,
    PlayOnTrigger,
}

[CreateAssetMenu(fileName = "Timeline", menuName = "ScriptableObjects/TimelineSO")]
public class TimelineSO : ScriptableObject
{
    public TimelineAsset timeline;
    public TimelineType Type;
    public bool played;
    public bool Once;
}
