using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class CinematicDialogueSystem : MonoBehaviour
{
    [Header("Elementos de UI")]
    [SerializeField] private GameObject textBox; // El panel de la caja de diálogo
    [SerializeField] private TMP_Text textField; // El componente de texto (TextMeshPro)
    [SerializeField] private GameObject inputIndicator; // Indicador visual para pulsar 'C'
    [SerializeField] private GameObject portraitBox; // El GameObject que contiene el retrato
    [SerializeField] private Image portraitImage; // La imagen del retrato

    [Header("Contenido del Diálogo")]
    [SerializeField, TextArea(3, 5)] private string dialogueLine; // La única línea de diálogo
    [SerializeField] private Sprite portraitSprite; // El sprite del retrato para esta línea

    [Header("Configuración de Audio")]
    [SerializeField] private AudioClip typingSound; // Sonido al escribir cada carácter
    [SerializeField] private AudioClip advanceSound; // Sonido al avanzar el texto
    [SerializeField] private AudioClip endSound; // Sonido al terminar el diálogo
    [SerializeField] private SoundType soundOnC = SoundType.Advance; // Tipo de sonido al pulsar 'C'

    [Header("Configuración de Tiempo")]
    [SerializeField] private float typingSpeed = 0.05f; // Velocidad de escritura por carácter
    [SerializeField] private float commaPause = 0.25f; // Pausa en comas
    [SerializeField] private float periodPause = 0.48f; // Pausa en puntos

    private AudioSource audioSource;
    private bool isDialogueActive;
    private bool isTypingComplete;
    private Coroutine typingCoroutine;

    // Enum para seleccionar el tipo de sonido en el Inspector
    private enum SoundType
    {
        Advance, // Reproduce advanceSound al pulsar 'C'
        End      // Reproduce endSound al pulsar 'C'
    }

    void Awake()
    {
        // Configurar el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Asegurarse de que los elementos de UI estén desactivados al inicio
        if (textBox != null)
        {
            textBox.SetActive(false);
            if (textField != null)
            {
                textField.gameObject.SetActive(false);
                // Asegurar que el texto sea visible
                textField.color = new Color(textField.color.r, textField.color.g, textField.color.b, 1f);
            }
        }
        if (portraitBox != null)
        {
            portraitBox.SetActive(false);
            if (portraitImage != null)
            {
                portraitImage.gameObject.SetActive(false);
                // Asegurar que la imagen sea visible
                portraitImage.color = new Color(portraitImage.color.r, portraitImage.color.g, portraitImage.color.b, 1f);
            }
        }
        if (inputIndicator != null)
        {
            inputIndicator.SetActive(false);
        }

        // Validaciones
        if (textBox == null) Debug.LogError("textBox no está asignado en CinematicDialogueSystem.");
        if (textField == null) Debug.LogError("textField no está asignado en CinematicDialogueSystem.");
        if (portraitBox != null && portraitImage == null) Debug.LogError("portraitImage no está asignado en CinematicDialogueSystem.");
        if (portraitSprite == null && portraitImage != null) Debug.LogWarning("portraitSprite no está asignado; no se mostrará el retrato.");
    }

    void Update()
    {
        // Si el diálogo está activo y se pulsa 'C'
        if (isDialogueActive && Input.GetKeyDown(KeyCode.C))
        {
            // Seleccionar el sonido según la configuración
            AudioClip soundToPlay = soundOnC == SoundType.Advance ? advanceSound : endSound;

            if (!isTypingComplete)
            {
                // Si no ha terminado de escribir, completar el texto inmediatamente
                StopCoroutine(typingCoroutine);
                textField.maxVisibleCharacters = GetVisibleCharacterCount(dialogueLine);
                isTypingComplete = true;
                if (inputIndicator != null) inputIndicator.SetActive(true);
                if (soundToPlay != null) audioSource.PlayOneShot(soundToPlay);
            }
            else
            {
                // Si el texto está completo, cerrar el diálogo
                EndDialogue();
                if (soundToPlay != null) audioSource.PlayOneShot(soundToPlay);
            }
        }
    }

    // Método público para ser llamado por Timeline (Signal Receiver)
    public void StartDialogue()
    {
        if (isDialogueActive || string.IsNullOrEmpty(dialogueLine))
        {
            Debug.LogWarning("No se puede iniciar el diálogo: ya está activo o dialogueLine está vacío.");
            return;
        }

        isDialogueActive = true;
        isTypingComplete = false;

        // Activar y validar la caja de diálogo
        if (textBox != null && textField != null)
        {
            textBox.SetActive(true);
            textField.gameObject.SetActive(true);
            textField.text = dialogueLine;
            textField.maxVisibleCharacters = 0;
            textField.ForceMeshUpdate();
            Debug.Log($"Diálogo activado: textBox active={textBox.activeSelf}, textField active={textField.gameObject.activeSelf}, text={dialogueLine}");
        }
        else
        {
            Debug.LogError("No se puede mostrar el diálogo: textBox o textField no están asignados.");
        }

        // Activar y validar el retrato
        if (portraitBox != null && portraitImage != null && portraitSprite != null)
        {
            portraitBox.SetActive(true);
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = portraitSprite;
            Debug.Log($"Retrato activado: portraitBox active={portraitBox.activeSelf}, portraitImage active={portraitImage.gameObject.activeSelf}, sprite={portraitSprite.name}");
        }
        else
        {
            Debug.LogWarning("No se muestra el retrato: portraitBox, portraitImage o portraitSprite no están asignados.");
        }

        if (inputIndicator != null) inputIndicator.SetActive(false);

        // Pausar el Timeline para que la cinemática espere al diálogo
        Time.timeScale = 0f;

        typingCoroutine = StartCoroutine(ShowDialogue());
    }

    private IEnumerator ShowDialogue()
    {
        if (textField == null) yield break;

        textField.ForceMeshUpdate();
        int totalVisibleChars = GetVisibleCharacterCount(dialogueLine);
        int visibleCount = 0;
        int nonSpaceCharCount = 0;

        while (visibleCount < totalVisibleChars)
        {
            visibleCount++;
            textField.maxVisibleCharacters = visibleCount;

            char currentChar = GetCharAtVisibleIndex(dialogueLine, visibleCount - 1);
            if (currentChar != ' ')
            {
                nonSpaceCharCount++;
                if (nonSpaceCharCount % 2 == 0 && typingSound != null)
                    audioSource.PlayOneShot(typingSound);
            }

            if (currentChar == ',')
                yield return new WaitForSecondsRealtime(commaPause);
            else if (currentChar == '.' || currentChar == '?' || currentChar == '!' || currentChar == ':' || currentChar == '¿' || currentChar == '¡')
                yield return new WaitForSecondsRealtime(periodPause);
            else
                yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTypingComplete = true;
        if (inputIndicator != null) inputIndicator.SetActive(true);
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        if (textBox != null)
        {
            textBox.SetActive(false);
            if (textField != null)
            {
                textField.gameObject.SetActive(false);
            }
        }
        if (portraitBox != null)
        {
            portraitBox.SetActive(false);
            if (portraitImage != null)
            {
                portraitImage.gameObject.SetActive(false);
            }
        }
        if (inputIndicator != null) inputIndicator.SetActive(false);
        Time.timeScale = 1f; // Reanudar el Timeline
        Debug.Log("Diálogo finalizado.");
    }

    // Cuenta los caracteres visibles (ignorando etiquetas de formato)
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

    // Obtiene el carácter en un índice visible
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
}