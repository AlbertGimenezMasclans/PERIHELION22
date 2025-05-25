using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class GameOver : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Image blackPanel; // Referencia al panel negro en la UI
    [SerializeField] private float fadeDuration = 1f; // Duración del fade en segundos

    [Header("Audio Settings")]
    [SerializeField] private AudioClip gameOverMusic; // Canción a reproducir
    [SerializeField] private AudioSource audioSource; // AudioSource para reproducir la música
    [SerializeField] private AudioMixerGroup outputMixerGroup; // Output de audio opcional
    [SerializeField] private float musicFadeInDuration = 1.3f; // Duración del fade-in en segundos
    [SerializeField, Range(0f, 1f)] private float maxMusicVolume = 1f; // Volumen máximo de la música

    private float fadeTimer = 0f; // Contador de tiempo para el fade del panel
    private bool fadeCompleted = false; // Para saber si el fade del panel terminó
    private bool hasStartedMusic = false; // Para rastrear si la música ya comenzó

    void Start()
    {
        // Asegurarse de que el panel empiece completamente opaco
        if (blackPanel != null)
        {
            Color startColor = blackPanel.color;
            startColor.a = 1f;
            blackPanel.color = startColor;
        }

        // Configurar el AudioSource
        if (audioSource != null)
        {
            audioSource.clip = gameOverMusic;
            audioSource.loop = true; // Activar reproducción en bucle
            if (outputMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = outputMixerGroup; // Asignar salida de audio
            }
            audioSource.volume = 0f; // Iniciar con volumen 0 para el fade-in
        }

        // Asegurar que maxMusicVolume esté en un rango válido
        maxMusicVolume = Mathf.Clamp(maxMusicVolume, 0f, 1f);
    }

    void Update()
    {
        // Manejar el fade-out del panel
        if (blackPanel != null && fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (fadeTimer / fadeDuration));
            Color currentColor = blackPanel.color;
            currentColor.a = alpha;
            blackPanel.color = currentColor;

            // Iniciar la música cuando el alpha llegue a 0.5
            if (!hasStartedMusic && alpha <= 0.5f && audioSource != null && gameOverMusic != null)
            {
                hasStartedMusic = true;
                StartCoroutine(FadeInMusic());
            }

            // Marcar cuando el fade se completa
            if (fadeTimer >= fadeDuration)
            {
                fadeCompleted = true;
            }
        }

        // Verificar si se presiona C después de completar el fade
        if (fadeCompleted && Input.GetKeyDown(KeyCode.C))
        {
            SceneManager.LoadScene("Perihelion");
        }
    }

    // Coroutine para el fade-in de la música
    private IEnumerator FadeInMusic()
    {
        if (audioSource == null || gameOverMusic == null)
        {
            Debug.LogWarning("AudioSource o AudioClip no asignados en el Inspector.");
            yield break;
        }

        audioSource.Play(); // Iniciar la reproducción
        float timer = 0f;
        float startVolume = 0f;
        float targetVolume = maxMusicVolume; // Usar el volumen máximo especificado

        while (timer < musicFadeInDuration)
        {
            timer += Time.deltaTime;
            float t = timer / musicFadeInDuration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume; // Asegurar que el volumen final sea el correcto
    }
}