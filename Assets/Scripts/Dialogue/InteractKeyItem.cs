using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractKeyItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject textBox;
    [SerializeField] private TMP_Text textField1;
    [SerializeField] private TMP_Text textField2;
    [SerializeField] private Image Input_TB;

    [Header("Dialogue Content")]
    [SerializeField, TextArea(1, 4)] private string[] dialogueLines;
    [SerializeField, TextArea(1, 4)] private string[] headlessDialogueLines;

    [Header("Position Settings")]
    [SerializeField] private bool useAlternativePosition = false;
    [SerializeField] private Vector2 alternativeTextBoxPosition = new Vector2(100, 100);

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typingSound; // Sonido al escribir el texto
    [SerializeField] private AudioClip dialogueAdvanceSound; // Sonido al avanzar al siguiente mensaje
    [SerializeField] private AudioClip dialogueEndSound; // Sonido al finalizar el diálogo

    [Header("Post-Dialogue Object")]
    [SerializeField] private GameObject objectToActivate; // GameObject a activar al finalizar

    private float typingTime = 0.05f;
    private float commaPauseTime = 0.25f; // Pausa después de una coma
    private float periodPauseTime = 0.48f; // Pausa después de un punto o signo de puntuación
    private bool isPlayerRange;
    private bool didDialogueStart;
    private bool isUsed; // Nueva variable para rastrear si ya se usó
    private int lineIndex;
    private Vector2 originalTextBoxPosition;
    private AudioSource audioSource;
    private string[] activeDialogueLines;
    private TMP_Text dialogueText;

    private PlayerMovement playerMovement;
    private GameObject playerObject;
    private CoinControllerUI coinControllerUI;
    private GameObject playerHead;
    private Rigidbody2D headRigidbody;
    private Rigidbody2D playerRigidbody; // Referencia al Rigidbody2D del jugador

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        isUsed = false; // Inicializar como no usado

        // Cargar los clips de audio desde Resources si no están asignados
        if (dialogueAdvanceSound == null) dialogueAdvanceSound = Resources.Load<AudioClip>("SFX/DialogueNEXT");
        if (dialogueEndSound == null) dialogueEndSound = Resources.Load<AudioClip>("SFX/DialogueEND");

        coinControllerUI = FindObjectOfType<CoinControllerUI>();
        if (coinControllerUI == null) Debug.LogError("CoinControllerUI not found in the scene.");

        if (textBox == null) { Debug.LogError("TextBox is not assigned."); return; }
        RectTransform textBoxRect = textBox.GetComponent<RectTransform>();
        originalTextBoxPosition = textBoxRect.anchoredPosition;

        dialogueText = textField1 != null ? textField1 : textField2;
        if (textField2 != null && dialogueText == textField1) textField2.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);

        if (Input_TB != null) Input_TB.gameObject.SetActive(false);

        // Asegurarse de que el objeto a activar esté desactivado al inicio
        if (objectToActivate != null) objectToActivate.SetActive(false);
    }

    void Update()
    {
        // Solo permitir interacción si el objeto no ha sido usado
        if (isPlayerRange && !isUsed && Input.GetKeyDown(KeyCode.C))
        {
            if (!didDialogueStart)
            {
                // Verificar si el jugador está quieto antes de permitir la interacción
                if (playerRigidbody != null && playerRigidbody.velocity.magnitude <= 0.01f) // Tolerancia pequeña para flotantes
                {
                    StartDialogue();
                }
                // Si el jugador está en movimiento, no hace nada
            }
            else if (dialogueText != null && dialogueText.maxVisibleCharacters >= GetVisibleCharacterCount(activeDialogueLines[lineIndex]))
            {
                NextDialogueLine();
            }
            else if (dialogueText != null)
            {
                StopAllCoroutines();
                dialogueText.maxVisibleCharacters = GetVisibleCharacterCount(activeDialogueLines[lineIndex]);
                if (Input_TB != null) Input_TB.gameObject.SetActive(true);
            }
        }
    }

    private void StartDialogue()
    {
        if (textBox == null || dialogueText == null || dialogueLines.Length == 0) return;

        didDialogueStart = true;
        textBox.SetActive(true);
        if (dialogueMark != null) dialogueMark.SetActive(false); // Desactivar el marcador de diálogo
        dialogueText.gameObject.SetActive(true);
        lineIndex = 0;
        Time.timeScale = 0f; // Pausa el juego

        bool isDismembered = false;
        if (playerMovement != null)
        {
            isDismembered = playerMovement.isDismembered;
            playerMovement.enabled = false; // Desactiva movimiento del cuerpo
        }

        if (playerObject.CompareTag("PlayerHead"))
        {
            if (headlessDialogueLines.Length > 0)
            {
                activeDialogueLines = headlessDialogueLines;
            }
            else
            {
                activeDialogueLines = dialogueLines; // Fallback a los diálogos normales
            }
        }
        else
        {
            activeDialogueLines = dialogueLines;
        }

        RectTransform textBoxRect = textBox.GetComponent<RectTransform>();
        if (textBoxRect != null)
            textBoxRect.anchoredPosition = useAlternativePosition ? alternativeTextBoxPosition : originalTextBoxPosition;

        if (playerMovement != null)
            playerMovement.SetDialogueActive(this);

        if (coinControllerUI != null)
            coinControllerUI.gameObject.SetActive(false);

        if (Input_TB != null)
            Input_TB.gameObject.SetActive(false);

        StartCoroutine(ShowLine());
    }

    private void NextDialogueLine()
    {
        lineIndex++;
        if (lineIndex < activeDialogueLines.Length)
        {
            PlayDialogueSound(dialogueAdvanceSound); // Reproducir sonido al avanzar
            if (Input_TB != null) Input_TB.gameObject.SetActive(false);
            StartCoroutine(ShowLine());
        }
        else
        {
            PlayDialogueSound(dialogueEndSound); // Reproducir sonido al finalizar
            didDialogueStart = false;
            textBox.SetActive(false);
            if (dialogueMark != null) dialogueMark.SetActive(false); // Asegurarse de que el marcador no vuelva a aparecer
            dialogueText.gameObject.SetActive(false);
            Time.timeScale = 1f; // Reanuda el juego
            isUsed = true; // Marcar como usado al finalizar el diálogo

            if (playerMovement != null)
            {
                playerMovement.SetDialogueActive(null);
                playerMovement.enabled = true; // Reactiva el movimiento del jugador
            }

            if (playerHead != null && headRigidbody != null)
            {
                headRigidbody.constraints = RigidbodyConstraints2D.None; // Permite movimiento nuevamente
            }

            if (coinControllerUI != null)
                coinControllerUI.gameObject.SetActive(true);

            if (Input_TB != null)
                Input_TB.gameObject.SetActive(false);

            // Activar el objeto especificado si existe
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
    }

    private IEnumerator ShowLine()
    {
        if (dialogueText == null) yield break;

        dialogueText.gameObject.SetActive(true);
        dialogueText.text = activeDialogueLines[lineIndex];
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalVisibleChars = GetVisibleCharacterCount(activeDialogueLines[lineIndex]);
        int visibleCount = 0;
        string currentLine = activeDialogueLines[lineIndex];
        int nonSpaceCharCount = 0;

        while (visibleCount < totalVisibleChars)
        {
            visibleCount++;
            dialogueText.maxVisibleCharacters = visibleCount;

            char currentChar = GetCharAtVisibleIndex(currentLine, visibleCount - 1);

            if (currentChar != ' ')
            {
                nonSpaceCharCount++;
                if (nonSpaceCharCount % 2 == 0)
                    PlayDialogueSound(typingSound);
            }

            if (currentChar == ',')
                yield return new WaitForSecondsRealtime(commaPauseTime);
            else if (currentChar == '.' || currentChar == '?' || currentChar == '!' || currentChar == ':' || currentChar == '¿' || currentChar == '¡')
                yield return new WaitForSecondsRealtime(periodPauseTime);
            else
                yield return new WaitForSecondsRealtime(typingTime);
        }

        if (Input_TB != null) Input_TB.gameObject.SetActive(true);
    }

    private char GetCharAtVisibleIndex(string line, int visibleIndex)
    {
        int visibleCount = 0;
        bool inTag = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '<') inTag = true;
            else if (c == '>') inTag = false;
            else if (!inTag)
            {
                if (visibleCount == visibleIndex) return c;
                visibleCount++;
            }
        }
        return '\0';
    }

    private int GetVisibleCharacterCount(string line)
    {
        int count = 0;
        bool inTag = false;
        foreach (char c in line)
        {
            if (c == '<') inTag = true;
            else if (c == '>') inTag = false;
            else if (!inTag) count++;
        }
        return count;
    }

    private void PlayDialogueSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead"))
        {
            isPlayerRange = true;
            // Mostrar el marcador solo si no se ha usado
            if (dialogueMark != null && !isUsed)
                dialogueMark.SetActive(true);
            playerObject = collision.gameObject;
            playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>(); // Asignar el Rigidbody2D al entrar
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead"))
        {
            isPlayerRange = false;
            // Desactivar el marcador solo si no se ha usado
            if (dialogueMark != null && !isUsed)
                dialogueMark.SetActive(false);
            playerMovement = null;
            playerObject = null;
            playerRigidbody = null; // Limpiar la referencia al salir
        }
    }
}