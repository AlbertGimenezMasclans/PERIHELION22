using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chapter1EndingCinematic : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The container for dialogue text")]
    [SerializeField] private GameObject textBox; // Contenedor del texto de diálogo
    [Tooltip("TextMeshPro text for dialogues")]
    [SerializeField] private TMP_Text dialogueText; // Texto para diálogos
    [Tooltip("TextMeshPro text for the introduction")]
    [SerializeField] private TMP_Text introText; // Texto para la introducción
    [Tooltip("Icon to advance dialogue")]
    [SerializeField] private Image inputIcon; // Icono para avanzar diálogo

    [Header("Dialogue Settings")]
    [Tooltip("Lines of dialogue to display")]
    [SerializeField, TextArea(2, 6)] private string[] dialogueLines; // Líneas de diálogo
    [Tooltip("Introduction text to display")]
    [SerializeField, TextArea(3, 10)] private string introTextLines; // Texto de la introducción
    [Tooltip("Typing speed for dialogues (seconds per character)")]
    [SerializeField] private float dialogueTypingSpeed = 0.05f; // Velocidad de escritura

    [Header("Sounds")]
    [Tooltip("Sound played during dialogue typing")]
    [SerializeField] private AudioClip dialogueTypingSound; // Sonido para diálogos
    [Tooltip("Sound played during intro text typing")]
    [SerializeField] private AudioClip introTypingSound; // Sonido para introducción
    [Tooltip("Sound played when advancing dialogue")]
    [SerializeField] private AudioClip advanceDialogueSound; // Sonido de avance
    [Tooltip("Sound played when dialogue ends")]
    [SerializeField] private AudioClip endDialogueSound; // Sonido de fin

    [Header("Post-Cinematic")]
    [Tooltip("List of GameObjects to toggle active state (active becomes inactive and vice versa)")]
    [SerializeField] private List<GameObject> objectsToToggle; // Lista de GameObjects a alternar

    // Pausas fijas
    private const float DIALOGUE_COMMA_PAUSE = 0.25f;
    private const float DIALOGUE_PERIOD_PAUSE = 0.45f;
    private const float DIALOGUE_GREATER_THAN_PAUSE = 0.45f;

    private bool cinematicFinished = false;
    private PlayerMovement playerMovement;
    private AudioSource audioSource;

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

        // Alternar el estado active de los GameObjects en la lista
        if (objectsToToggle != null && objectsToToggle.Count > 0)
        {
            foreach (GameObject obj in objectsToToggle)
            {
                if (obj != null)
                {
                    bool newState = !obj.activeSelf;
                    obj.SetActive(newState);
                    Debug.Log($"GameObject {obj.name} cambiado a active={newState}", obj);
                }
                else
                {
                    Debug.LogWarning("Un GameObject en objectsToToggle es null.", this);
                }
            }
        }
        else
        {
            Debug.LogWarning("objectsToToggle está vacío o no asignado en el Inspector.", this);
        }

        // Restaurar movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("Movimiento del jugador restaurado.", playerMovement);
        }

        // Limpiar cinemática
        cinematicFinished = true;
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
            int nonSpaceCharCount = 0;

            while (currentVisible < totalVisible)
            {
                currentVisible++;
                targetText.maxVisibleCharacters = currentVisible;

                char currentChar = GetCharAtVisibleIndex(line, currentVisible - 1);
                PlayTypingSound(currentChar, ref nonSpaceCharCount, typingSound);

                if (currentChar == ',')
                    yield return new WaitForSecondsRealtime(DIALOGUE_COMMA_PAUSE);
                else if (".:!?".Contains(currentChar.ToString()))
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

        targetText.text = text;
        targetText.maxVisibleCharacters = 0;
        targetText.ForceMeshUpdate();

        int totalVisible = GetVisibleCharacterCount(text);
        int currentVisible = 0;
        int nonSpaceCharCount = 0;

        while (currentVisible < totalVisible)
        {
            currentVisible++;
            targetText.maxVisibleCharacters = currentVisible;

            char currentChar = GetCharAtVisibleIndex(text, currentVisible - 1);
            PlayTypingSound(currentChar, ref nonSpaceCharCount, typingSound);

            if (currentChar == ',')
                yield return new WaitForSecondsRealtime(DIALOGUE_COMMA_PAUSE);
            else if (".:!?".Contains(currentChar.ToString()))
                yield return new WaitForSecondsRealtime(DIALOGUE_PERIOD_PAUSE);
            else
                yield return new WaitForSecondsRealtime(dialogueTypingSpeed);
        }

        yield return new WaitForSeconds(0.75f);
        yield return StartCoroutine(FadeOutText(targetText));

        targetText.gameObject.SetActive(false);
        if (useTextBox && textBox != null)
        {
            Debug.Log("Desactivando TextBox...", textBox);
            textBox.SetActive(false);
        }
    }

    private void PlayTypingSound(char currentChar, ref int nonSpaceCharCount, AudioClip typingSound)
    {
        if (currentChar != ' ')
        {
            nonSpaceCharCount++;
            if (nonSpaceCharCount % 2 == 0 && typingSound != null)
            {
                audioSource.PlayOneShot(typingSound);
            }
        }
    }

    private IEnumerator FadeOutText(TMP_Text text)
    {
        float duration = 1f;
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