using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePush : MonoBehaviour
{
    [SerializeField] private float forceMagnitude;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Player"))
        {
            Debug.Log("Golpeo");
            Animator animator = hit.gameObject.GetComponent<Animator>();
            animator.SetTrigger("Hit");
        }
        
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null)
        {
            Vector3 forceDirection = hit.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();
            
            rb.AddForceAtPosition(forceDirection*forceMagnitude, transform.position, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        
    }
}
