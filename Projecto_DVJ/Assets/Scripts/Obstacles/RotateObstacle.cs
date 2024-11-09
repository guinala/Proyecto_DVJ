using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObstacle : MonoBehaviour
{
    [SerializeField] private GameObject objectToRotate;
    [SerializeField] private float rotationSpeed = 45f;
    
    void Update() 
    { 
        objectToRotate.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0); 
    }
}
