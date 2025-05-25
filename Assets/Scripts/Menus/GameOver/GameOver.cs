using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class GameOver : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Image blackPanel; // Panel negro
    [SerializeField] private Image gameOverImage; // Imagen asociada a text2 y text3
    [SerializeField] private float fadeDuration = 1f; // Duración del fade del panel

    [Header("Game Over Animation Settings")]
    [SerializeField] private GameObject gameOverObject1; // Primer GameObject a activar
    [SerializeField] private GameObject gameOverObject2; // Segundo GameObject a activar
    [SerializeField] private GameObject protagonist; // GameObject del protagonista
    [SerializeField] private Vector3 targetPosition; // Posición objetivo del protagonista
    [SerializeField] private float movementDuration = 1f; // Duración del movimiento
    [SerializeField] private TextMeshProUGUI text1; // Primer texto TMPro
    [SerializeField] private TextMeshProUGUI text2; // Segundo texto TMPro
    [SerializeField] private TextMeshProUGUI text3; // Tercer texto TMPro
    [SerializeField] private float textFadeDuration = 1.15f; // Duración del fade-in de textos e imagen

    [Header("Audio Settings")]
    [SerializeField] private AudioClip initialSound; // Sonido inicial
    [SerializeField] private AudioClip fallSound; // Sonido de caída
    [SerializeField] private AudioClip finalSound; // Sonido al llegar a posición
    [SerializeField] private AudioClip gameOverMusic; // Canción final
    [SerializeField] private AudioSource audioSource; // AudioSource para sonidos
    [SerializeField] private AudioSource musicSource; // AudioSource para música
    [SerializeField] private AudioMixerGroup outputMixerGroup; // Output de audio
    [SerializeField] private float musicFadeInDuration = 1.3f; // Duración del fade-in música
    [SerializeField, Range(0f, 1f)] private float maxMusicVolume = 1f; // Volumen máximo música

    private bool fadeCompleted = false;
    private float fadeTimer = 0f; // Timer for blackPanel fade

    void Start()
    {
        // Configurar panel inicial
        if (blackPanel != null)
        {
            Color startColor = blackPanel.color;
            startColor.a = 1f;
            blackPanel.color = startColor;
        }

        // Configurar imagen y textos con alpha 0 (invisibles pero activos)
        if (gameOverImage != null)
        {
            Color imageColor = gameOverImage.color;
            imageColor.a = 0f;
            gameOverImage.color = imageColor;
        }
        if (text1 != null)
        {
            Color textColor = text1.color;
            textColor.a = 0f;
            text1.color = textColor;
        }
        if (text2 != null)
        {
            Color textColor = text2.color;
            textColor.a = 0f;
            text2.color = textColor;
        }
        if (text3 != null)
        {
            Color textColor = text3.color;
            textColor.a = 0f;
            text3.color = textColor;
        }

        // Configurar AudioSources
        if (audioSource != null && outputMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = outputMixerGroup;
        }
        if (musicSource != null)
        {
            musicSource.clip = gameOverMusic;
            musicSource.loop = true;
            musicSource.volume = 0f; // Start at 0 for fade-in
            if (outputMixerGroup != null)
            {
                musicSource.outputAudioMixerGroup = outputMixerGroup;
            }
        }

        // Iniciar la secuencia de animación
        StartCoroutine(GameOverSequence());
    }

    void Update()
    {
        // Fade-out del panel
        if (blackPanel != null && !fadeCompleted)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (fadeTimer / fadeDuration));
            Color currentColor = blackPanel.color;
            currentColor.a = alpha;
            blackPanel.color = currentColor;

            if (fadeTimer >= fadeDuration)
            {
                fadeCompleted = true;
            }
        }

        // Reiniciar escena con tecla C
        if (fadeCompleted && Input.GetKeyDown(KeyCode.C))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Perihelion");
        }
    }

    private IEnumerator GameOverSequence()
    {
        // 1. Esperar a que termine el fade-out del panel
        while (!fadeCompleted)
        {
            yield return null; // Wait for fade to complete
        }

        // 2. Esperar 0.2s adicionales
        yield return new WaitForSeconds(0.2f);

        // 3. Activar GameObjects y reproducir sonido inicial
        if (gameOverObject1 != null) gameOverObject1.SetActive(true);
        if (gameOverObject2 != null) gameOverObject2.SetActive(true);
        if (audioSource != null && initialSound != null)
        {
            audioSource.PlayOneShot(initialSound);
        }

        // 4. Esperar 0.45s y reproducir sonido de caída
        yield return new WaitForSeconds(0.45f);
        if (audioSource != null && fallSound != null && protagonist != null)
        {
            audioSource.PlayOneShot(fallSound);
            yield return new WaitForSeconds(fallSound.length - 1.25f); // Esperar hasta 1.25s antes de que termine

            // Mover protagonista a posición objetivo
            Vector3 startPos = protagonist.transform.position;
            float timer = 0f;
            while (timer < movementDuration)
            {
                timer += Time.deltaTime;
                float t = timer / movementDuration;
                protagonist.transform.position = Vector3.Lerp(startPos, targetPosition, t);
                yield return null;
            }
            protagonist.transform.position = targetPosition; // Asegurar posición final
        }

        // 5. Reproducir sonido final
        if (audioSource != null && finalSound != null)
        {
            audioSource.PlayOneShot(finalSound);
        }

        // 6. Esperar 0.65s después del movimiento y comenzar música
        yield return new WaitForSeconds(0.65f);
        if (musicSource != null && gameOverMusic != null)
        {
            StartCoroutine(FadeInMusic());
            yield return new WaitForSeconds(musicFadeInDuration / 2f); // Esperar la mitad del fade-in de la música
            StartCoroutine(FadeInUIElements());
        }
    }

    private IEnumerator FadeInMusic()
    {
        if (musicSource == null || gameOverMusic == null)
        {
            Debug.LogWarning("MusicSource o GameOverMusic no asignados en el Inspector.");
            yield break;
        }

        musicSource.Play();
        float timer = 0f;
        while (timer < musicFadeInDuration)
        {
            timer += Time.deltaTime;
            float t = timer / musicFadeInDuration;
            musicSource.volume = Mathf.Lerp(0f, maxMusicVolume, t);
            yield return null;
        }
        musicSource.volume = maxMusicVolume;
    }

    private IEnumerator FadeInUIElements()
    {
        float timer = 0f;
        Color text1Initial = text1 != null ? text1.color : Color.clear;
        Color text2Initial = text2 != null ? text2.color : Color.clear;
        Color text3Initial = text3 != null ? text3.color : Color.clear;
        Color imageInitial = gameOverImage != null ? gameOverImage.color : Color.clear;

        Color text1Final = text1 != null ? new Color(text1Initial.r, text1Initial.g, text1Initial.b, 1f) : Color.clear;
        Color text2Final = text2 != null ? new Color(text2Initial.r, text2Initial.g, text2Initial.b, 1f) : Color.clear;
        Color text3Final = text3 != null ? new Color(text3Initial.r, text3Initial.g, text3Initial.b, 1f) : Color.clear;
        Color imageFinal = gameOverImage != null ? new Color(imageInitial.r, imageInitial.g, imageInitial.b, 1f) : Color.clear;

        while (timer < textFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / textFadeDuration;

            if (text1 != null)
                text1.color = Color.Lerp(text1Initial, text1Final, t);
            if (text2 != null)
                text2.color = Color.Lerp(text2Initial, text2Final, t);
            if (text3 != null)
                text3.color = Color.Lerp(text3Initial, text3Final, t);
            if (gameOverImage != null)
                gameOverImage.color = Color.Lerp(imageInitial, imageFinal, t);

            yield return null;
        }

        // Asegurar valores finales
        if (text1 != null)
            text1.color = text1Final;
        if (text2 != null)
            text2.color = text2Final;
        if (text3 != null)
            text3.color = text3Final;
        if (gameOverImage != null)
            gameOverImage.color = imageFinal;
    }
}