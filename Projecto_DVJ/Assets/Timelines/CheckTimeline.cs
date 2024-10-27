using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CheckTimeline : MonoBehaviour
{
    [SerializeField] private TimelineSO[] timelines;
    private PlayableDirector director;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        CheckAwake();
    }
    
    private void CheckAwake()
    {
        for(int i = 0; i < timelines.Length; i++)
        {
            if (timelines[i].Type == TimelineType.PlayOnAwake && !timelines[i].played && timelines[i].Once)
            {
                director.playableAsset = timelines[i].timeline;
                director.Play();
            }
        }
    }
}
