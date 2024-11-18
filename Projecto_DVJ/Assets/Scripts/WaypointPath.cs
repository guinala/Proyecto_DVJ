using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public Transform GetWayPoint(int index)
    {
        return transform.GetChild(index);
    }

    public int GetNextWaypointIndex(int currentIndex)
    {
        int nextWaypointIndex = currentIndex + 1;
        if (nextWaypointIndex == transform.childCount)
        {
            nextWaypointIndex = 0;
        }
        
        return nextWaypointIndex;
    }
}
