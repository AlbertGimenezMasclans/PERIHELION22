using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [Header("Zonas de Música")]
    public List<GameObject> musicZones = new List<GameObject>();

    [Header("Música")]
    public AudioClip musicClip; // La pista que se reproducirá

    private AudioSource audioSource;

    private void Start()
    {
        // Buscamos o creamos un AudioSource en este objeto
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = musicClip;
    }

    private void Update()
    {
        // Revisa si el jugador está dentro de alguna zona
        foreach (var zone in musicZones)
        {
            if (zone != null)
            {
                Collider2D zoneCollider = zone.GetComponent<Collider2D>();
                if (zoneCollider != null)
                {
                    Collider2D[] hits = Physics2D.OverlapBoxAll(zoneCollider.bounds.center, zoneCollider.bounds.size, 0f);

                    foreach (var hit in hits)
                    {
                        if (hit.CompareTag("Player"))
                        {
                            PlayMusic();
                            return; // Ya encontramos un Player, no hace falta seguir buscando
                        }
                    }
                }
            }
        }

        // Si no estamos en ninguna zona, paramos la música
        StopMusic();
    }

    private void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnValidate()
    {
        // Validamos que todos los GameObjects tengan BoxCollider2D y que sea isTrigger
        foreach (var zone in musicZones)
        {
            if (zone != null)
            {
                var collider = zone.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    Debug.LogWarning($"El objeto {zone.name} no tiene un BoxCollider2D.");
                }
                else if (!collider.isTrigger)
                {
                    Debug.LogWarning($"El BoxCollider2D de {zone.name} no está marcado como 'IsTrigger'.");
                }
            }
        }
    }
}
