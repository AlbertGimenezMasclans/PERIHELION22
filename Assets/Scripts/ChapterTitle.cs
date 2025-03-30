using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChapterTitle : MonoBehaviour
{
    // Referencias a los objetos
    public Image imageToFade1;                // Primera imagen que hará fade
    public Image imageToFade2;                // Segunda imagen que hará fade
    public TextMeshProUGUI chapterText;       // Texto del capítulo
    public TextMeshProUGUI titleText;         // Texto del título
    public AudioClip typingSound;             // Sonido de tecleo
    private AudioSource audioSource;

    // Variables de velocidad configurables
    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;       // Duración del Fade-In de las imágenes
    public float fadeOutDuration = 0.5f;      // Duración del Fade-Out de las imágenes y textos

    [Header("Typing Settings")]
    public float chapterTypingSpeed = 0.05f;  // Velocidad de tipeo del capítulo (segundos por caracter)
    public float titleTypingSpeed = 0.1f;     // Velocidad de tipeo del título (segundos por caracter)

    [Header("Delay Settings")]
    public float delayBeforeTitle = 0.6f;     // Retraso antes de mostrar el título
    public float delayBeforeFadeOut = 0.4f;   // Retraso antes de iniciar el Fade-Out de textos
    public float delayBetweenFades = 0.4f;    // Retraso entre Fade-Out de textos e imágenes

    [Header("Text Content")]
    [TextArea(1, 3)] public string chapterContent; // Texto del capítulo especificado en el Inspector
    [TextArea(1, 3)] public string titleContent;   // Texto del título especificado en el Inspector

    // Variables de control
    private bool isTriggered = false;
    private PlayerMovement playerMovement;    // Referencia al PlayerMovement

    void Awake()
    {
        // Hacer las imágenes invisibles al inicio
        if (imageToFade1 != null)
        {
            Color color = imageToFade1.color;
            color.a = 0f;
            imageToFade1.color = color;
        }

        if (imageToFade2 != null)
        {
            Color color = imageToFade2.color;
            color.a = 0f;
            imageToFade2.color = color;
        }
    }

    void Start()
    {
        // Inicializar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = typingSound;

        // Asegurarse de que los textos estén invisibles al inicio
        chapterText.alpha = 0f;
        titleText.alpha = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerHead")) && !isTriggered)
        {
            Debug.Log("Trigger activado por: " + other.gameObject.name); // Depuración
            isTriggered = true;

            // Obtener referencia al PlayerMovement
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement == null && other.CompareTag("PlayerHead"))
            {
                playerMovement = FindPlayerMovementForHead(other.gameObject);
            }

            if (playerMovement != null)
            {
                Debug.Log("PlayerMovement encontrado: " + playerMovement.gameObject.name); // Depuración
            }
            else
            {
                Debug.LogError("No se encontró PlayerMovement en el objeto que activó el trigger.");
            }

            // Iniciar la secuencia
            StartCoroutine(ShowSequence());
        }
    }

    IEnumerator ShowSequence()
    {
        // Pausar el juego completamente
        Time.timeScale = 0f;
        if (playerMovement != null)
        {
            playerMovement.SetDialogueActive(this); // Bloquear movimiento
            playerMovement.SetMovementLocked(true); // Forzar bloqueo adicional
            Debug.Log("Movimiento bloqueado: activeDialogueSystem = " + (playerMovement.activeDialogueSystem != null) + ", Time.timeScale = " + Time.timeScale);
        }

        // 1. Fade-In de ambas imágenes (de alpha 0 a 1)
        yield return StartCoroutine(FadeImages(0f, 1f, fadeInDuration));

        // 2. Texto 1 (Capítulo) - Tecleo letra a letra
        chapterText.alpha = 1f;
        chapterText.text = "";

        foreach (char c in chapterContent)
        {
            chapterText.text += c;
            if (typingSound != null && c != ' ')
            {
                audioSource.Play();
            }
            yield return new WaitForSecondsRealtime(chapterTypingSpeed); // Usar tiempo real
        }

        // 3. Espera antes de Texto 2
        yield return new WaitForSecondsRealtime(delayBeforeTitle);

        // 4. Texto 2 (Título) - Tecleo más lento
        titleText.alpha = 1f;
        titleText.text = "";

        foreach (char c in titleContent)
        {
            titleText.text += c;
            if (typingSound != null && c != ' ')
            {
                audioSource.Play();
            }
            yield return new WaitForSecondsRealtime(titleTypingSpeed); // Usar tiempo real
        }

        // 5. Fade-Out de los textos al mismo tiempo
        yield return StartCoroutine(FadeText(chapterText, 1f, 0f, fadeOutDuration)); // Fade-Out del Capítulo
        yield return StartCoroutine(FadeText(titleText, 1f, 0f, fadeOutDuration)); // Fade-Out del Título

        // 6. Espera para hacer Fade-Out de las imágenes
        yield return new WaitForSecondsRealtime(delayBeforeFadeOut); // Retraso antes de empezar el Fade-Out

        // 7. Fade-Out de las imágenes
        yield return StartCoroutine(FadeImages(1f, 0f, fadeOutDuration));

        // 8. Desbloquear movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.SetDialogueActive(null); // Liberar el bloqueo
            playerMovement.SetMovementLocked(false); // Liberar bloqueo adicional
            Debug.Log("Movimiento desbloqueado: activeDialogueSystem = " + (playerMovement.activeDialogueSystem != null) + ", Time.timeScale = " + Time.timeScale);
        }

        // Restaurar el tiempo
        Time.timeScale = 1f;

        // 9. Destruir el GameObject (ChapterTrigger)
        Destroy(gameObject); // Destruir este GameObject
    }


    // Corrutina para fade de ambas imágenes
    private IEnumerator FadeImages(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Usar tiempo real
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetImageAlpha(imageToFade1, alpha);
            SetImageAlpha(imageToFade2, alpha);
            yield return null;
        }
        SetImageAlpha(imageToFade1, endAlpha);
        SetImageAlpha(imageToFade2, endAlpha);
    }

    // Corrutina para fade de Textos
    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Usar tiempo real
            text.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        text.alpha = endAlpha;
    }

    // Método auxiliar para establecer alpha en una imagen
    private void SetImageAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    // Método para encontrar PlayerMovement asociado a la cabeza
    private PlayerMovement FindPlayerMovementForHead(GameObject head)
    {
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement pm in players)
        {
            if (pm.isDismembered && pm.headObject == head)
            {
                return pm;
            }
        }
        return null;
    }
}