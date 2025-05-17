using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chapter1EndingCinematic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject textBox; // Contenedor del texto de diálogo
    [SerializeField] private TMP_Text dialogueText; // Texto para diálogos
    [SerializeField] private TMP_Text introText; // Texto para la introducción
    [SerializeField] private Image inputIcon; // Icono para avanzar diálogo

    [Header("Diálogo Final")]
    [SerializeField, TextArea(2, 6)] private string[] dialogueLines; // Líneas de diálogo
    [SerializeField, TextArea(4, 10)] private string introTextLines; // Texto de la introducción
    [SerializeField] private float dialogueTypingSpeed = 0.05f; // Velocidad de escritura

    [Header("Sonidos")]
    [SerializeField] private AudioClip dialogueTypingSound; // Sonido para diálogos
    [SerializeField] private AudioClip introTypingSound; // Sonido para introducción
    [SerializeField] private AudioClip advanceDialogueSound; // Sonido de avance
    [SerializeField] private AudioClip endDialogueSound; // Sonido de fin
    private AudioSource audioSource;

    // Pausas fijas
    private const float DIALOGUE_COMMA_PAUSE = 0.25f;
    private const float DIALOGUE_PERIOD_PAUSE = 0.45f;
    private const float DIALOGUE_GREATER_THAN_PAUSE = 0.45f;

    private bool cinematicFinished = false;
    private PlayerMovement playerMovement;

    private void Start()
    {
        // Verificar componentes
        if (textBox == null) Debug.LogError("TextBox no está asignado.", this);
        if (dialogueText == null) Debug.LogError("DialogueText no está asignado.", this);
        if (introText == null) Debug.LogError("IntroText no está asignado.", this);
        if (string.IsNullOrEmpty(introTextLines)) 
            Debug.LogWarning("IntroTextLines está vacío.", this);
        if (dialogueLines == null || dialogueLines.Length == 0) 
            Debug.LogWarning("DialogueLines está vacío.", this);

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource no estaba asignado. Se añadió uno automáticamente.", audioSource);
        }

        // Inicializar UI, forzando el estado inicial
        if (textBox != null)
        {
            Debug.Log($"Estado inicial de TextBox: {textBox.activeSelf}", textBox);
            textBox.SetActive(false);
        }

        if (dialogueText != null)
        {
            Debug.Log($"Estado inicial de DialogueText: {dialogueText.gameObject.activeSelf}", dialogueText.gameObject);
            dialogueText.gameObject.SetActive(false);
            dialogueText.text = "";
            Debug.Log("DialogueText inicializado. Posición: " + dialogueText.GetComponent<RectTransform>().anchoredPosition, dialogueText);
        }

        if (introText != null)
        {
            Debug.Log($"Estado inicial de IntroText: {introText.gameObject.activeSelf}", introText.gameObject);
            introText.gameObject.SetActive(false);
            introText.text = "";
            introText.color = new Color(introText.color.r, introText.color.g, introText.color.b, 1f); // Visible
            Debug.Log("IntroText inicializado. Posición: " + introText.GetComponent<RectTransform>().anchoredPosition, introText);
        }

        if (inputIcon != null)
        {
            Debug.Log($"Estado inicial de InputIcon: {inputIcon.gameObject.activeSelf}", inputIcon.gameObject);
            inputIcon.gameObject.SetActive(false);
        }

        // Bloquear movimiento del jugador
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("Movimiento del jugador desactivado.", playerMovement);
        }
        else
        {
            Debug.LogWarning("PlayerMovement no encontrado.", this);
        }

        // Iniciar cinemática
        StartCoroutine(PlayCinematic());
    }

    private IEnumerator PlayCinematic()
    {
        Debug.Log("Iniciando cinemática de Chapter1-Ending...", this);

        // Mostrar introducción (si hay texto)
        if (introText != null && !string.IsNullOrEmpty(introTextLines))
        {
            Debug.Log("Preparando para mostrar introducción...", introText);
            yield return StartCoroutine(PlayDialogue(introText, introTextLines, introTypingSound, false));
            yield return new WaitForSeconds(1f); // Retraso de 1 segundo tras el fade-out
        }
        else
        {
            Debug.LogWarning("No se pueden mostrar líneas de introducción: IntroText o IntroTextLines no están configurados.", this);
        }

        // Mostrar diálogos (si los hay)
        if (dialogueText != null && textBox != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            Debug.Log("Preparando para mostrar diálogos...", dialogueText);
            yield return StartCoroutine(PlayDialogue(dialogueText, dialogueLines, dialogueTypingSound, true));
        }
        else
        {
            Debug.LogWarning("No se pueden mostrar diálogos: DialogueText, TextBox o DialogueLines no están configurados.", this);
        }

        yield return new WaitForSeconds(1f);

        // Restaurar movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("Movimiento del jugador restaurado.", playerMovement);
        }

        // Limpiar cinemática
        Destroy(gameObject);
        Debug.Log("Cinemática finalizada.", this);
    }

    private IEnumerator PlayDialogue(TMP_Text targetText, string[] lines, AudioClip typingSound, bool useTextBox)
    {
        if (targetText == null)
        {
            Debug.LogError("TargetText es null, no se puede reproducir el diálogo.", this);
            yield break;
        }

        Debug.Log("Activando TargetText...", targetText);
        if (useTextBox && textBox != null)
        {
            Debug.Log("Activando TextBox...", textBox);
            textBox.SetActive(true);
        }
        targetText.gameObject.SetActive(true);

        foreach (string line in lines)
        {
            Debug.Log($"Mostrando línea: {line}", targetText);
            targetText.text = line;
            targetText.maxVisibleCharacters = 0;
            targetText.ForceMeshUpdate();

            int totalVisible = GetVisibleCharacterCount(line);
            int currentVisible = 0;

            while (currentVisible < totalVisible)
            {
                currentVisible++;
                targetText.maxVisibleCharacters = currentVisible;

                char c = GetCharAtVisibleIndex(line, currentVisible - 1);
                if (c != ' ' && currentVisible % 2 == 0 && typingSound != null)
                {
                    audioSource.PlayOneShot(typingSound);
                }

                if (c == ',')
                    yield return new WaitForSecondsRealtime(DIALOGUE_COMMA_PAUSE);
                else if (".:!?".Contains(c.ToString()))
                    yield return new WaitForSecondsRealtime(DIALOGUE_PERIOD_PAUSE);
                else
                    yield return new WaitForSecondsRealtime(dialogueTypingSpeed);
            }

            if (inputIcon != null)
            {
                Debug.Log("Activando InputIcon...", inputIcon);
                inputIcon.gameObject.SetActive(true);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.C));

            if (advanceDialogueSound != null)
            {
                audioSource.PlayOneShot(advanceDialogueSound);
            }

            if (inputIcon != null)
            {
                Debug.Log("Desactivando InputIcon...", inputIcon);
                inputIcon.gameObject.SetActive(false);
            }
        }

        if (endDialogueSound != null)
        {
            audioSource.PlayOneShot(endDialogueSound);
        }

        Debug.Log("Desactivando TargetText...", targetText);
        targetText.gameObject.SetActive(false);
        if (useTextBox && textBox != null)
        {
            Debug.Log("Desactivando TextBox...", textBox);
            textBox.SetActive(false);
        }
        cinematicFinished = true;
    }

    private IEnumerator PlayDialogue(TMP_Text targetText, string text, AudioClip typingSound, bool useTextBox)
    {
        if (targetText == null)
        {
            Debug.LogError("TargetText es null, no se puede reproducir el texto.", this);
            yield break;
        }

        Debug.Log("Activando TargetText...", targetText);
        if (useTextBox && textBox != null)
        {
            Debug.Log("Activando TextBox...", textBox);
            textBox.SetActive(true);
        }
        targetText.gameObject.SetActive(true);

        bool isIntro = !useTextBox; // La introducción no usa textBox

        if (isIntro)
        {
            // Modo introducción: escribe carácter por carácter sin input
            targetText.text = "";
            int soundCounter = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                targetText.text += c;

                soundCounter++;
                if (soundCounter % 2 == 0 && typingSound != null)
                {
                    audioSource.PlayOneShot(typingSound);
                }

                if (c == ',')
                    yield return new WaitForSecondsRealtime(DIALOGUE_COMMA_PAUSE);
                else if (c == '.' || c == ':')
                    yield return new WaitForSecondsRealtime(DIALOGUE_PERIOD_PAUSE);
                else
                    yield return new WaitForSecondsRealtime(dialogueTypingSpeed);
            }

            // Aplicar fade-out al texto de la introducción
            yield return new WaitForSeconds(0.75f); // Retraso antes del fade-out
            yield return StartCoroutine(FadeOutText(targetText));
        }
        else
        {
            // Modo diálogo: este caso no se usa aquí, pero se mantiene por compatibilidad
            Debug.LogError("PlayDialogue con string no debe usarse para diálogos.", this);
            yield break;
        }

        Debug.Log("Desactivando TargetText...", targetText);
        targetText.gameObject.SetActive(false);
        if (useTextBox && textBox != null)
        {
            Debug.Log("Desactivando TextBox...", textBox);
            textBox.SetActive(false);
        }
        cinematicFinished = true;
    }

    private IEnumerator FadeOutText(TMP_Text text)
    {
        float duration = 1f; // Duración del fade-out, como en el script de referencia
        float time = 0f;
        Color initialColor = text.color;
        Color finalColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        while (time < duration)
        {
            text.color = Color.Lerp(initialColor, finalColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        text.color = finalColor;
        text.gameObject.SetActive(false);
    }

    private int GetVisibleCharacterCount(string text)
    {
        int count = 0;
        bool inTag = false;
        foreach (char c in text)
        {
            if (c == '<') inTag = true;
            else if (c == '>') inTag = false;
            else if (!inTag) count++;
        }
        return count;
    }

    private char GetCharAtVisibleIndex(string text, int index)
    {
        int count = 0;
        bool inTag = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<') inTag = true;
            else if (text[i] == '>') inTag = false;
            else if (!inTag)
            {
                if (count == index) return text[i];
                count++;
            }
        }
        return '\0';
    }
}