using UnityEngine;
using Cinemachine;

public class CinemachineCollisionHandler : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public LayerMask collisionLayers;
    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;
    public float smoothSpeed = 10.0f;

    private Vector3 dollyDir;
    private float distance;

    void Start()
    {
        dollyDir = freeLookCamera.transform.localPosition.normalized;
        distance = freeLookCamera.transform.localPosition.magnitude;
    }

    void LateUpdate()
    {
        Vector3 desiredCameraPos = freeLookCamera.transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if (Physics.Linecast(freeLookCamera.LookAt.position, desiredCameraPos, out hit, collisionLayers))
        {
            float hitDistance = Vector3.Distance(freeLookCamera.LookAt.position, hit.point);
            if (hitDistance < maxDistance && hitDistance > minDistance)
            {
                //Debug.Log("Colisión detectada con: " + hit.collider.name + " en la posición: " + hit.point);
                distance = Mathf.Clamp(hitDistance * 0.9f, minDistance, maxDistance);
            }
            else
            {
                //Debug.Log("Colisión fuera del rango permitido.");
                distance = maxDistance;
            }
        }
        else
        {
            //Debug.Log("No se detectaron colisiones.");
            distance = maxDistance;
        }

        freeLookCamera.transform.localPosition = Vector3.Lerp(freeLookCamera.transform.localPosition, dollyDir * distance, Time.deltaTime * smoothSpeed);
    }
}
