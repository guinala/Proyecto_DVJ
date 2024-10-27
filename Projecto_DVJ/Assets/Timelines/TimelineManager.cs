using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineManager : MonoBehaviour
{
    [SerializeField] private TimelineSO[] timelines;
    private PlayableDirector director;
    
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        CheckAwake();
    }


    public void FadeEffect()
    {
        Fade();
        Invoke("FadeOut", 1.3f);
    }
    private void Fade()
    {
        StartCoroutine(Helper.Fade(fadeCanvasGroup, 1, 1f));
    }

    private void FadeOut()
    {
        StartCoroutine(Helper.Fade(fadeCanvasGroup, 0, 1f));
    }
    
    private void CheckAwake()
    {
        for(int i = 0; i < timelines.Length; i++)
        {
            if (timelines[i].Type == TimelineType.PlayOnAwake && !timelines[i].played && timelines[i].Once)
            {
                director.playableAsset = timelines[i].timeline;
                director.Play();
                timelines[i].played = true;
            }
        }
    }
}
