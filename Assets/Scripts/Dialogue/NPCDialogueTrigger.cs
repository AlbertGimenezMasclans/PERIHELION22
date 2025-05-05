using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("NPC Reference")]
    [SerializeField] private GameObject npcWithDialogue; // Referencia al GameObject del NPC con DialogueSystem

    private DialogueSystem dialogueSystem; // Referencia al componente DialogueSystem del NPC
    private bool hasTriggered = false; // Para evitar reactivaciones
    private GameObject playerObject; // Para almacenar el jugador que colisionó
    private PlayerMovement playerMovement; // Para configurar el movimiento

    void Start()
    {
        // Obtener el componente DialogueSystem del NPC
        if (npcWithDialogue != null)
        {
            dialogueSystem = npcWithDialogue.GetComponent<DialogueSystem>();
            if (dialogueSystem == null)
            {
                Debug.LogError("El GameObject asignado como NPC no tiene un componente DialogueSystem.");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado un NPC en el Inspector.");
        }

        // Asegurarse de que el GameObject tiene un Rigidbody2D para colisiones físicas
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; // No afectado por física, solo para colisiones
            rb.useFullKinematicContacts = true; // Detectar colisiones incluso si es cinemático
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si colisiona con el jugador y no se ha activado antes
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead")) && !hasTriggered)
        {
            hasTriggered = true; // Evita reactivaciones
            playerObject = collision.gameObject;
            playerMovement = playerObject.GetComponent<PlayerMovement>();

            if (dialogueSystem != null && !dialogueSystem.IsDialogueActive)
            {
                // Configurar DialogueSystem para que reconozca al jugador
                SetDialogueSystemFields();
                // Inicializar el estado del retrato
                InitializePortraitState();
                // Iniciar el diálogo automáticamente
                dialogueSystem.StartDialogue();
                // Destruir el dialogueMark del NPC
                DestroyDialogueMark();
                // Forzar animación de Idle del jugador
                ForcePlayerIdleAnimation();
            }
        }
    }

    void Update()
    {
        // Cuando el diálogo termine, realizar las acciones finales
        if (hasTriggered && dialogueSystem != null && !dialogueSystem.IsDialogueActive)
        {
            // Desactivar el BoxCollider2D del NPC
            BoxCollider2D npcCollider = npcWithDialogue.GetComponent<BoxCollider2D>();
            if (npcCollider != null)
            {
                npcCollider.enabled = false;
            }
            else
            {
                Debug.LogWarning("El NPC no tiene un BoxCollider2D para desactivar.");
            }

            // Establecer isPlayerRange a false y reiniciar el retrato
            ResetDialogueSystemState();

            // Destruir este GameObject
            Destroy(gameObject);
        }
    }

    private void SetDialogueSystemFields()
    {
        // Usar reflexión para acceder a los campos privados de DialogueSystem
        System.Reflection.FieldInfo isPlayerRangeField = typeof(DialogueSystem).GetField("isPlayerRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo playerObjectField = typeof(DialogueSystem).GetField("playerObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo playerMovementField = typeof(DialogueSystem).GetField("playerMovement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (isPlayerRangeField != null)
            isPlayerRangeField.SetValue(dialogueSystem, true);
        else
            Debug.LogError("No se pudo acceder al campo isPlayerRange en DialogueSystem.");

        if (playerObjectField != null)
            playerObjectField.SetValue(dialogueSystem, playerObject);
        else
            Debug.LogError("No se pudo acceder al campo playerObject en DialogueSystem.");

        if (playerMovementField != null)
            playerMovementField.SetValue(dialogueSystem, playerMovement);
        else
            Debug.LogError("No se pudo acceder al campo playerMovement en DialogueSystem.");
    }

    private void InitializePortraitState()
    {
        // Usar reflexión para acceder a portraitImage, idleSprite y blinkSprite
        System.Reflection.FieldInfo portraitImageField = typeof(DialogueSystem).GetField("portraitImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo idleSpriteField = typeof(DialogueSystem).GetField("idleSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo blinkSpriteField = typeof(DialogueSystem).GetField("blinkSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (portraitImageField != null && idleSpriteField != null && blinkSpriteField != null)
        {
            Image portraitImage = (Image)portraitImageField.GetValue(dialogueSystem);
            Sprite idleSprite = (Sprite)idleSpriteField.GetValue(dialogueSystem);
            Sprite blinkSprite = (Sprite)blinkSpriteField.GetValue(dialogueSystem);

            if (portraitImage != null && idleSprite != null && blinkSprite != null)
            {
                // Asegurar que portraitImage comience con idleSprite
                portraitImage.sprite = idleSprite;
            }
            else
            {
                Debug.LogWarning("portraitImage, idleSprite o blinkSprite no están configurados en DialogueSystem.");
            }
        }
        else
        {
            Debug.LogWarning("No se pudo acceder a los campos portraitImage, idleSprite o blinkSprite en DialogueSystem.");
        }
    }

    private void ResetDialogueSystemState()
    {
        // Establecer isPlayerRange a false
        System.Reflection.FieldInfo isPlayerRangeField = typeof(DialogueSystem).GetField("isPlayerRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (isPlayerRangeField != null)
            isPlayerRangeField.SetValue(dialogueSystem, false);

        // Reiniciar el retrato a idleSprite
        System.Reflection.FieldInfo portraitImageField = typeof(DialogueSystem).GetField("portraitImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo idleSpriteField = typeof(DialogueSystem).GetField("idleSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (portraitImageField != null && idleSpriteField != null)
        {
            Image portraitImage = (Image)portraitImageField.GetValue(dialogueSystem);
            Sprite idleSprite = (Sprite)idleSpriteField.GetValue(dialogueSystem);

            if (portraitImage != null && idleSprite != null)
            {
                portraitImage.sprite = idleSprite;
            }
        }
        else
        {
            Debug.LogWarning("No se pudo acceder a los campos portraitImage o idleSprite en DialogueSystem.");
        }
    }

    private void DestroyDialogueMark()
    {
        // Usar reflexión para acceder al campo dialogueMark
        System.Reflection.FieldInfo dialogueMarkField = typeof(DialogueSystem).GetField("dialogueMark", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (dialogueMarkField != null)
        {
            GameObject dialogueMark = (GameObject)dialogueMarkField.GetValue(dialogueSystem);
            if (dialogueMark != null)
            {
                Destroy(dialogueMark); // Destruir el dialogueMark
                dialogueMarkField.SetValue(dialogueSystem, null); // Establecer a null
            }
        }
        else
        {
            Debug.LogWarning("No se pudo acceder al campo dialogueMark en DialogueSystem.");
        }
    }

    private void ForcePlayerIdleAnimation()
    {
        if (playerObject != null)
        {
            Animator playerAnimator = playerObject.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                // Asumimos que la animación de Idle se activa seteando isMoving a false
                playerAnimator.SetBool("MoveRight", false);
                playerAnimator.SetBool("MoveLeft", false);
            }
            else
            {
                Debug.LogWarning("El jugador no tiene un componente Animator para forzar la animación de Idle.");
            }
        }
    }
}