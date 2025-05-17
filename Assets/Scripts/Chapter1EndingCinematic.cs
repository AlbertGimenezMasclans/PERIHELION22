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
    [SerializeField] private Image inputIcon; // Icono para avanzar diálogo

    [Header("Diálogo Final")]
    [SerializeField, TextArea(2, 6)] private string[] dialogueLines; // Líneas de diálogo
    [SerializeField] private float dialogueTypingSpeed = 0.05f; // Velocidad de escritura diálogo

    [Header("Sonidos")]
    [SerializeField] private AudioClip dialogueTypingSound; // Sonido para diálogos
    [SerializeField] private AudioClip advanceDialogueSound; // Sonido de avance
    [SerializeField] private AudioClip endDialogueSound; // Sonido de fin
    private AudioSource audioSource;

    // Pausas fijas
    private const float DIALOGUE_COMMA_PAUSE = 0.25f;
    private const float DIALOGUE_PERIOD_PAUSE = 0.45f;

    private int lineIndex = 0;
    private bool cinematicFinished = false;
    private PlayerMovement playerMovement;

    private void Start()
    {
        // Verificar componentes
        if (textBox == null) Debug.LogError("TextBox no está asignado.", this);
        if (dialogueText == null) Debug.LogError("DialogueText no está asignado.", this);
        if (dialogueLines == null || dialogueLines.Length == 0) Debug.LogWarning("DialogueLines está vacío.", this);

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

        // Mostrar diálogos (si los hay)
        if (dialogueText != null && textBox != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            Debug.Log("Preparando para mostrar diálogos...", dialogueText);
            yield return StartCoroutine(PlayDialogue());
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

    private IEnumerator PlayDialogue()
    {
        if (textBox == null || dialogueText == null)
        {
            Debug.LogError("TextBox o DialogueText es null, no se puede reproducir el diálogo.", this);
            yield break;
        }

        Debug.Log("Activando TextBox y DialogueText...", textBox);
        textBox.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        lineIndex = 0;

        while (lineIndex < dialogueLines.Length)
        {
            Debug.Log($"Mostrando diálogo {lineIndex + 1}: {dialogueLines[lineIndex]}", dialogueText);
            dialogueText.text = dialogueLines[lineIndex];
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            int totalVisible = GetVisibleCharacterCount(dialogueLines[lineIndex]);
            int currentVisible = 0;

            while (currentVisible < totalVisible)
            {
                currentVisible++;
                dialogueText.maxVisibleCharacters = currentVisible;

                char c = GetCharAtVisibleIndex(dialogueLines[lineIndex], currentVisible - 1);
                if (c != ' ' && currentVisible % 2 == 0 && dialogueTypingSound != null)
                {
                    audioSource.PlayOneShot(dialogueTypingSound);
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

            lineIndex++;
        }

        if (endDialogueSound != null)
        {
            audioSource.PlayOneShot(endDialogueSound);
        }

        Debug.Log("Desactivando TextBox y DialogueText...", textBox);
        dialogueText.gameObject.SetActive(false);
        textBox.SetActive(false);
        cinematicFinished = true;
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