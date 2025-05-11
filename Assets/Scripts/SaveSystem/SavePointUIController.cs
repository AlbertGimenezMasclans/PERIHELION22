using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SavePointUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject interactionMarker; // Marca de interacción (ej. un signo de exclamación)
    [SerializeField] private GameObject saveDialogue; // Diálogo del sistema de guardado
    [SerializeField] private TMP_Text saveText; // Texto de guardado que se muestra letra por letra
    [SerializeField] private GameObject[] saveOptions; // Opciones iniciales ("Sí", "No")
    [SerializeField] private GameObject[] continueExitOptions; // Opciones tras guardar ("Continuar", "Salir")

    [Header("Dialogue Settings")]
    [SerializeField, TextArea(3, 5)] private string initialDialogueText; // Texto inicial ("¿Deseas guardar?")
    [SerializeField] private float typingTime = 0.05f; // Tiempo por carácter
    [SerializeField] private Color selectedColor = Color.yellow; // Color para la opción seleccionada
    [SerializeField] private Color unselectedColor = Color.white; // Color para la opción no seleccionada
    [SerializeField] private float optionChangeCooldown = 0.5f; // Cooldown para cambiar opciones

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typingSound; // Sonido para cada 2 caracteres del texto
    [SerializeField] private AudioClip optionChangeSound; // Sonido al cambiar de opción
    [SerializeField] private AudioClip yesSound; // Sonido para "Sí"
    [SerializeField] private AudioClip confirmSound; // Sonido para "Continuar"
    [SerializeField] private AudioClip cancelSound; // Sonido para "No" y "Salir"

    [Header("References")]
    [SerializeField] private CoinControllerUI coinController; // Referencia al controlador de monedas
    [SerializeField] private Animator savePointAnimator; // Referencia al Animator del SavePoint

    private bool isPlayerNearby = false; // Indica si el jugador está dentro del trigger
    private bool isDialogueActive = false; // Indica si el diálogo está activo
    private bool isTyping = false; // Indica si se está mostrando el texto letra por letra
    private int selectedOptionIndex = 0; // Índice de la opción seleccionada
    private PlayerMovement playerMovement; // Referencia al componente PlayerMovement
    private AudioSource audioSource; // Componente para reproducir sonidos
    private float lastOptionChangeTime = 0f; // Tiempo del último cambio de opción
    private bool isPostSaveState = false; // Indica si estamos en el estado tras guardar

    public bool IsDialogueActive => isDialogueActive; // Propiedad para compatibilidad con PlayerMovement

    void Start()
    {
        // Asegurarse de que los GameObjects estén desactivados al inicio
        if (interactionMarker != null)
            interactionMarker.SetActive(false);
        if (saveDialogue != null)
            saveDialogue.SetActive(false);
        if (saveText != null)
            saveText.gameObject.SetActive(false);
        foreach (var option in saveOptions)
        {
            if (option != null)
                option.SetActive(false);
        }
        foreach (var option in continueExitOptions)
        {
            if (option != null)
                option.SetActive(false);
        }

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Asegurarse de que el Animator esté configurado
        if (savePointAnimator == null)
        {
            savePointAnimator = GetComponent<Animator>();
            if (savePointAnimator == null)
            {
                Debug.LogError("Animator component not found on SavePoint. Please assign it in the Inspector.");
            }
        }

        // Configurar el Animator para usar tiempo real (no afectado por Time.timeScale)
        if (savePointAnimator != null)
        {
            savePointAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // Establecer la animación por defecto
        TriggerAnimation("DefaultTrigger");
    }

    void Update()
    {
        // Activar/desactivar la marca de interacción según la proximidad
        if (interactionMarker != null)
            interactionMarker.SetActive(isPlayerNearby && !isDialogueActive);

        // Si el jugador está dentro del trigger y presiona la tecla "C"
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.C))
        {
            if (!isDialogueActive)
            {
                // Bloquear movimiento del jugador inmediatamente
                if (playerMovement != null)
                {
                    playerMovement.enabled = false; // Deshabilitar componente
                    Time.timeScale = 0f;
                }
                else
                {
                    Debug.LogWarning("PlayerMovement component not found. Cannot block player movement.");
                }

                // Iniciar la animación "SavePoint-Entrance"
                StartCoroutine(StartSavePointInteraction());
            }
            else if (!isTyping)
            {
                // Manejar selección de opción
                if (isPostSaveState)
                {
                    // Opciones "Continuar" o "Salir"
                    if (selectedOptionIndex == 0) // Continuar
                    {
                        if (audioSource != null && confirmSound != null)
                        {
                            audioSource.PlayOneShot(confirmSound);
                        }
                        StartCoroutine(EndSavePointInteraction());
                    }
                    else if (selectedOptionIndex == 1) // Salir
                    {
                        if (audioSource != null && cancelSound != null)
                        {
                            audioSource.PlayOneShot(cancelSound);
                        }
                        // Ejecutar "SavePoint-Leave" antes de salir
                        StartCoroutine(ExitToMenu());
                    }
                }
                else
                {
                    // Opciones "Sí" o "No"
                    if (selectedOptionIndex == 0) // Sí
                    {
                        if (audioSource != null && yesSound != null)
                        {
                            audioSource.PlayOneShot(yesSound);
                        }
                        // Guardar el juego
                        SaveSystem.SaveGame(playerMovement, coinController);
                        // Mostrar "Partida Guardada." y manejar animaciones
                        StartCoroutine(ShowSavedMessage());
                    }
                    else // No
                    {
                        if (audioSource != null && cancelSound != null)
                        {
                            audioSource.PlayOneShot(cancelSound);
                        }
                        StartCoroutine(EndSavePointInteraction());
                    }
                }
            }
            else
            {
                // Si se está escribiendo, mostrar todo el texto de una vez
                StopAllCoroutines();
                saveText.maxVisibleCharacters = saveText.text.Length;
                isTyping = false;
                StartCoroutine(ShowOptions(isPostSaveState ? continueExitOptions : saveOptions));
            }
        }

        // Navegación entre opciones con flechas (con cooldown)
        if (isDialogueActive && !isTyping && Time.unscaledTime >= lastOptionChangeTime + optionChangeCooldown)
        {
            GameObject[] currentOptions = isPostSaveState ? continueExitOptions : saveOptions;
            if (currentOptions.Length > 0)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && selectedOptionIndex < currentOptions.Length - 1)
                {
                    UpdateOptionSelection(selectedOptionIndex + 1);
                    lastOptionChangeTime = Time.unscaledTime;
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) && selectedOptionIndex > 0)
                {
                    UpdateOptionSelection(selectedOptionIndex - 1);
                    lastOptionChangeTime = Time.unscaledTime;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobar si el objeto que entró en el trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("PlayerMovement component not found on Player GameObject.");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Comprobar si el objeto que salió del trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            StartCoroutine(EndSavePointInteraction());
        }
    }

    private IEnumerator StartSavePointInteraction()
    {
        // Ejecutar animación "SavePoint-Entrance"
        TriggerAnimation("EntranceTrigger");

        // Esperar a que termine "SavePoint-Entrance" y la espera de 0.75s (manejado por el Animator)
        float entranceDuration = GetAnimationLength("SavePoint-Entrance") + 0.75f; // La espera de 0.75s está en el Animator, pero sumamos aquí para el tiempo total
        yield return new WaitForSecondsRealtime(entranceDuration);

        // Iniciar el diálogo (el Animator ya debería haber pasado a "SavePoint-Spin")
        isDialogueActive = true;
        isPostSaveState = false;
        if (saveDialogue != null)
            saveDialogue.SetActive(true);
        if (interactionMarker != null)
            interactionMarker.SetActive(false);
        StartCoroutine(ShowText(initialDialogueText, saveOptions));
    }

    private IEnumerator ShowText(string text, GameObject[] optionsToShow)
    {
        if (saveText == null || string.IsNullOrEmpty(text))
            yield break;

        isTyping = true;
        saveText.gameObject.SetActive(true);
        saveText.text = text;
        saveText.maxVisibleCharacters = 0;
        saveText.ForceMeshUpdate();

        int totalVisibleChars = text.Length;
        int visibleCount = 0;
        int nonSpaceCharCount = 0; // Contador para caracteres no espaciados

        while (visibleCount < totalVisibleChars)
        {
            visibleCount++;
            saveText.maxVisibleCharacters = visibleCount;
            // Contar caracteres no espaciados y reproducir sonido cada 2
            if (typingSound != null && audioSource != null)
            {
                char currentChar = text[visibleCount - 1];
                if (currentChar != ' ')
                {
                    nonSpaceCharCount++;
                    if (nonSpaceCharCount >= 2)
                    {
                        audioSource.PlayOneShot(typingSound);
                        nonSpaceCharCount = 0; // Reiniciar contador
                    }
                }
            }
            yield return new WaitForSecondsRealtime(typingTime);
        }

        isTyping = false;
        yield return new WaitForSecondsRealtime(0.55f);
        StartCoroutine(ShowOptions(optionsToShow));
    }

    private IEnumerator ShowSavedMessage()
    {
        // Ocultar opciones actuales
        foreach (var option in saveOptions)
        {
            if (option != null)
            {
                option.SetActive(false);
                TMP_Text optionText = option.GetComponent<TMP_Text>();
                if (optionText != null)
                    optionText.color = unselectedColor;
            }
        }

        // Mostrar "Partida Guardada." mientras se mantiene "SavePoint-Spin" (sin activar LeaveTrigger aquí)
        isTyping = true;
        saveText.text = "Partida Guardada.";
        saveText.maxVisibleCharacters = 0;
        saveText.ForceMeshUpdate();

        int totalVisibleChars = saveText.text.Length;
        int visibleCount = 0;
        int nonSpaceCharCount = 0;

        while (visibleCount < totalVisibleChars)
        {
            visibleCount++;
            saveText.maxVisibleCharacters = visibleCount;
            if (typingSound != null && audioSource != null)
            {
                char currentChar = saveText.text[visibleCount - 1];
                if (currentChar != ' ')
                {
                    nonSpaceCharCount++;
                    if (nonSpaceCharCount >= 2)
                    {
                        audioSource.PlayOneShot(typingSound);
                        nonSpaceCharCount = 0;
                    }
                }
            }
            yield return new WaitForSecondsRealtime(typingTime);
        }

        isTyping = false;
        yield return new WaitForSecondsRealtime(0.55f);

        // Mostrar "¿Quieres continuar o salir?" con nuevas opciones, manteniendo "SavePoint-Spin"
        isPostSaveState = true;
        StartCoroutine(ShowText("¿Quieres continuar o salir?", continueExitOptions));
    }

    private IEnumerator ShowOptions(GameObject[] options)
    {
        // Restablecer colores de todas las opciones antes de activarlas
        foreach (var option in options)
        {
            if (option != null)
            {
                TMP_Text optionText = option.GetComponent<TMP_Text>();
                if (optionText != null)
                    optionText.color = unselectedColor;
                option.SetActive(true);
            }
        }
        // Seleccionar la primera opción por defecto
        if (options.Length > 0)
        {
            selectedOptionIndex = 0;
            UpdateOptionSelection(selectedOptionIndex);
        }
        yield return null;
    }

    private void UpdateOptionSelection(int newIndex)
    {
        // Determinar qué opciones están activas
        GameObject[] currentOptions = isPostSaveState ? continueExitOptions : saveOptions;

        // Restaurar el color de la opción anterior
        if (currentOptions[selectedOptionIndex] != null)
        {
            TMP_Text previousText = currentOptions[selectedOptionIndex].GetComponent<TMP_Text>();
            if (previousText != null)
                previousText.color = unselectedColor;
        }

        // Actualizar el índice y cambiar el color de la nueva opción
        selectedOptionIndex = newIndex;
        if (currentOptions[selectedOptionIndex] != null)
        {
            TMP_Text selectedText = currentOptions[selectedOptionIndex].GetComponent<TMP_Text>();
            if (selectedText != null)
                selectedText.color = selectedColor;
        }

        // Reproducir sonido al cambiar de opción
        if (optionChangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(optionChangeSound);
        }
    }

    private void TriggerAnimation(string triggerName)
    {
        if (savePointAnimator != null)
        {
            savePointAnimator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning("Animator not assigned in SavePointUIController. Cannot trigger animation: " + triggerName);
        }
    }

    private float GetAnimationLength(string animationName)
    {
        if (savePointAnimator != null)
        {
            foreach (AnimationClip clip in savePointAnimator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    return clip.length;
                }
            }
        }
        Debug.LogWarning("Animation " + animationName + " not found. Returning default length of 1 second.");
        return 1f;
    }

    private IEnumerator EndSavePointInteraction()
    {
        // Ejecutar "SavePoint-Leave"
        TriggerAnimation("LeaveTrigger");

        // Esperar a que termine "SavePoint-Leave"
        float leaveDuration = GetAnimationLength("SavePoint-Leave");
        yield return new WaitForSecondsRealtime(leaveDuration);

        // Volver a la animación por defecto "SavePoint-Save"
        TriggerAnimation("DefaultTrigger");

        // Cerrar el diálogo
        CloseDialogue();
    }

    private IEnumerator ExitToMenu()
    {
        // Ejecutar "SavePoint-Leave"
        TriggerAnimation("LeaveTrigger");

        // Esperar a que termine "SavePoint-Leave"
        float leaveDuration = GetAnimationLength("SavePoint-Leave");
        yield return new WaitForSecondsRealtime(leaveDuration);

        // Volver a la animación por defecto "SavePoint-Save"
        TriggerAnimation("DefaultTrigger");

        // Cambiar escena
        Time.timeScale = 1f; // Restaurar tiempo antes de cambiar escena
        SceneManager.LoadScene("MenuPrincipal");
    }

    private void CloseDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        isTyping = false;
        isPostSaveState = false;
        if (saveDialogue != null)
            saveDialogue.SetActive(false);
        if (saveText != null)
            saveText.gameObject.SetActive(false);
        foreach (var option in saveOptions)
        {
            if (option != null)
            {
                option.SetActive(false);
                TMP_Text optionText = option.GetComponent<TMP_Text>();
                if (optionText != null)
                    optionText.color = unselectedColor;
            }
        }
        foreach (var option in continueExitOptions)
        {
            if (option != null)
            {
                option.SetActive(false);
                TMP_Text optionText = option.GetComponent<TMP_Text>();
                if (optionText != null)
                    optionText.color = unselectedColor;
            }
        }
        if (playerMovement != null)
        {
            playerMovement.enabled = true; // Rehabilitar componente
            Time.timeScale = 1f;
        }
        selectedOptionIndex = 0;
    }
}