using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudios : MonoBehaviour
{
    public AudioSource audioSource; // El AudioSource que reproducirá los clips
    public AudioClip[] audioClips;  // El array de clips de audio

    [SerializeField] private float waitingTime;

    private void Start()
    {
        // Iniciar la corrutina que reproducirá un clip cada 5 segundos
        StartCoroutine(PlayRandomClip());
    }

    private IEnumerator PlayRandomClip()
    {
        while (true)
        {
            // Esperar 5 segundos antes de reproducir el siguiente clip
            yield return new WaitForSeconds(waitingTime);

            // Seleccionar un clip aleatorio
            AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];

            // Reproducir el clip seleccionado
            audioSource.clip = randomClip;
            audioSource.Play();
        }
    }
}
