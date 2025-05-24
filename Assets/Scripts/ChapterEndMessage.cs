using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class ChapterEndMessage : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The image that will move to the center of the canvas")]
    [SerializeField] private Image targetImage;
    [Tooltip("TextMeshPro text that will be typed character by character")]
    [SerializeField] private TMP_Text typewriterText;
    [Tooltip("TextMeshPro text that will fade in and scale down")]
    [SerializeField] private TMP_Text fadeText;

    [Header("Animation Settings")]
    [Tooltip("Delay before the image starts moving (in seconds)")]
    [SerializeField] private float delayBeforeImageMove = 0.85f;
    [Tooltip("Duration of the image movement to (0,0,0) in seconds")]
    [SerializeField] private float moveDuration = 1f;
    [Tooltip("Duration of the fade-in and scale effect for the fade text in seconds")]
    [SerializeField] private float fadeDuration = 1f;
    [Tooltip("Initial scale for the fade text (applied to X and Y)")]
    [SerializeField] private float initialFadeTextScale = 3.2066f;
    [Tooltip("Target scale for the fade text (applied to X and Y)")]
    [SerializeField] private float targetFadeTextScale = 1.4066f;
    [Tooltip("Delay after fade text animation before final fade-out (in seconds)")]
    [SerializeField] private float delayBeforeFinalFade = 3.25f;
    [Tooltip("Duration of the final fade-out for all elements (in seconds)")]
    [SerializeField] private float finalFadeDuration = 1.5f;

    [Header("Timing Settings")]
    [Tooltip("Delay after image movement before typing starts (in seconds)")]
    [SerializeField] private float delayBeforeTyping = 0.85f;
    [Tooltip("Delay after typing before fade-in starts (in seconds)")]
    [SerializeField] private float delayBeforeFade = 0.95f;
    [Tooltip("Time between each character appearance in typewriter effect (in seconds)")]
    [SerializeField] private float typeSpeed = 0.08f;

    [Header("Sounds")]
    [Tooltip("Sound played every 2 non-space characters during typewriter effect")]
    [SerializeField] private AudioClip typewriterSound;
    [Tooltip("Sound played when fade text animation completes")]
    [SerializeField] private AudioClip fadeTextCompleteSound;

    private Vector3 originalImagePosition;
    private string fullTypewriterText;
    private AudioSource audioSource;

    void Start()
    {
        // Verificar componentes
        if (targetImage == null) Debug.LogError("TargetImage no está asignado.", this);
        if (typewriterText == null) Debug.LogError("TypewriterText no está asignado.", this);
        if (fadeText == null) Debug.LogError("FadeText no está asignado.", this);

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource no estaba asignado. Se añadió uno automáticamente.", audioSource);
        }

        // Guardar la posición original de la imagen
        originalImagePosition = targetImage.transform.localPosition;
        
        // Guardar el texto completo del typewriter
        fullTypewriterText = typewriterText.text;
        typewriterText.text = ""; // Limpiar el texto inicialmente
        
        // Establecer la escala inicial del texto de fade
        fadeText.transform.localScale = new Vector3(initialFadeTextScale, initialFadeTextScale, 1f);
        
        // Asegurarse que el texto de fade esté invisible
        Color fadeColor = fadeText.color;
        fadeText.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        // Iniciar la secuencia
        StartCoroutine(AnimationSequence());
    }

    IEnumerator AnimationSequence()
    {
        // Esperar antes de mover la imagen
        yield return new WaitForSeconds(delayBeforeImageMove);

        // Mover la imagen a (0,0,0) con interpolación suave
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            // Usar SmoothStep para un movimiento suave
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            targetImage.transform.localPosition = Vector3.Lerp(originalImagePosition, Vector3.zero, smoothT);
            yield return null;
        }
        targetImage.transform.localPosition = Vector3.zero; // Asegurar posición final exacta

        // Esperar antes de empezar a escribir
        yield return new WaitForSeconds(delayBeforeTyping);

        // Escribir texto caracter a caracter
        int nonSpaceCharCount = 0;
        for (int i = 0; i < fullTypewriterText.Length; i++)
        {
            typewriterText.text = fullTypewriterText.Substring(0, i + 1);
            char currentChar = fullTypewriterText[i];
            if (currentChar != ' ')
            {
                nonSpaceCharCount++;
                if (nonSpaceCharCount % 2 == 0 && typewriterSound != null)
                {
                    audioSource.PlayOneShot(typewriterSound);
                }
            }
            yield return new WaitForSeconds(typeSpeed);
        }

        // Esperar antes de empezar el fade
        yield return new WaitForSeconds(delayBeforeFade);

        // Fade-in y escalado del segundo texto
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            // Usar SmoothStep para fade y escala
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Actualizar transparencia
            Color fadeColor = fadeText.color;
            fadeText.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, smoothT);

            // Actualizar escala
            float currentScale = Mathf.Lerp(initialFadeTextScale, targetFadeTextScale, smoothT);
            fadeText.transform.localScale = new Vector3(currentScale, currentScale, 1f);

            yield return null;
        }
        // Asegurar que el texto esté completamente visible y en la escala final
        Color finalColor = fadeText.color;
        fadeText.color = new Color(finalColor.r, finalColor.g, finalColor.b, 1f);
        fadeText.transform.localScale = new Vector3(targetFadeTextScale, targetFadeTextScale, 1f);

        // Reproducir sonido al completar la animación del fadeText
        if (fadeTextCompleteSound != null)
        {
            audioSource.PlayOneShot(fadeTextCompleteSound);
        }

        // Esperar 3.25 segundos antes del fade-out final
        yield return new WaitForSeconds(delayBeforeFinalFade);

        // Fade-out de todos los elementos
        elapsedTime = 0f;
        Color initialImageColor = targetImage.color;
        Color initialTypewriterColor = typewriterText.color;
        Color initialFadeTextColor = fadeText.color;
        Color finalImageColor = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, 0f);
        Color finalTypewriterColor = new Color(initialTypewriterColor.r, initialTypewriterColor.g, initialTypewriterColor.b, 0f);
        Color finalFadeTextColor = new Color(initialFadeTextColor.r, initialFadeTextColor.g, initialFadeTextColor.b, 0f);

        while (elapsedTime < finalFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / finalFadeDuration;

            targetImage.color = Color.Lerp(initialImageColor, finalImageColor, t);
            typewriterText.color = Color.Lerp(initialTypewriterColor, finalTypewriterColor, t);
            fadeText.color = Color.Lerp(initialFadeTextColor, finalFadeTextColor, t);

            yield return null;
        }

        // Asegurar que todos los elementos estén completamente transparentes
        targetImage.color = finalImageColor;
        typewriterText.color = finalTypewriterColor;
        fadeText.color = finalFadeTextColor;

        // Desactivar elementos
        targetImage.gameObject.SetActive(false);
        typewriterText.gameObject.SetActive(false);
        fadeText.gameObject.SetActive(false);

        // Cambiar a la escena "MenuPrincipal" después del fade-out final
        Debug.Log("Cambiando a la escena MenuPrincipal...");
        SceneManager.LoadScene("MenuPrincipal");

        // Destruir el objeto para limpiar la escena (opcional, ya que el cambio de escena destruirá el objeto)
        Destroy(gameObject);
    }
}