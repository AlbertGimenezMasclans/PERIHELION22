using UnityEngine;
using System.Collections;

public class ActiveButton : MonoBehaviour
{
    [SerializeField] private GameObject indicator; // GameObject que actúa como indicador
    [SerializeField] private AudioClip pressSound; // Sonido al presionar el botón

    [Header("Sprite Settings")]
    [SerializeField] private Sprite spriteForTrigger1; // Sprite cuando TriggerCount = 1
    [SerializeField] private Sprite spriteForTrigger2; // Sprite cuando TriggerCount = 2
    [SerializeField] private Sprite spriteForTrigger3; // Sprite cuando TriggerCount = 3

    private bool isPlayerNearby; // Indica si el jugador está dentro del trigger
    private bool isUsed; // Indica si el botón ya fue usado
    private AudioSource audioSource; // Componente para reproducir el sonido
    private SpriteRenderer spriteRenderer; // Componente para cambiar el sprite
    private ButtonController controller; // Referencia al controlador

    // Método para asignar el controlador
    public void SetController(ButtonController ctrl)
    {
        controller = ctrl;
    }

    private void Start()
    {
        // Obtener componentes
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer no encontrado en el botón {gameObject.name}.");
        }

        // Asegurarse de que el indicador esté desactivado al inicio
        if (indicator != null)
        {
            indicator.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Indicator no asignado en el botón {gameObject.name}.");
        }

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pressSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Inicializar estado
        isUsed = false;
        isPlayerNearby = false;

        // Validar sprites
        if (spriteForTrigger1 == null) Debug.LogWarning($"spriteForTrigger1 no asignado en el botón {gameObject.name}.");
        if (spriteForTrigger2 == null) Debug.LogWarning($"spriteForTrigger2 no asignado en el botón {gameObject.name}.");
        if (spriteForTrigger3 == null) Debug.LogWarning($"spriteForTrigger3 no asignado en el botón {gameObject.name}.");
    }

    private void Update()
    {
        // Solo procesar si el botón no ha sido usado y el jugador está cerca
        if (isPlayerNearby && !isUsed && Input.GetKeyDown(KeyCode.C))
        {
            ActivateButton();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el que entra es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // Activar el indicador si el botón no ha sido usado
            if (indicator != null && !isUsed)
            {
                indicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Verificar si el que sale es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // Desactivar el indicador si el botón no ha sido usado
            if (indicator != null && !isUsed)
            {
                indicator.SetActive(false);
            }
        }
    }

    private void ActivateButton()
    {
        // Marcar el botón como usado
        isUsed = true;

        // Desactivar el indicador permanentemente
        if (indicator != null)
        {
            indicator.SetActive(false);
        }

        // Reproducir sonido
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        // Notificar al controlador
        if (controller != null)
        {
            controller.RegisterButtonPress();
        }
        else
        {
            Debug.LogWarning($"No se ha asignado un controlador al botón {gameObject.name}.");
        }
    }

    // Método para actualizar el sprite basado en el conteo del controlador
    public void UpdateSpriteBasedOnCount(int count)
    {
        if (spriteRenderer != null)
        {
            switch (count)
            {
                case 1:
                    if (spriteForTrigger1 != null)
                    {
                        spriteRenderer.sprite = spriteForTrigger1;
                        Debug.Log($"Botón {gameObject.name} cambió a spriteForTrigger1.");
                    }
                    break;
                case 2:
                    if (spriteForTrigger2 != null)
                    {
                        spriteRenderer.sprite = spriteForTrigger2;
                        Debug.Log($"Botón {gameObject.name} cambió a spriteForTrigger2.");
                    }
                    break;
                case 3:
                    if (spriteForTrigger3 != null)
                    {
                        spriteRenderer.sprite = spriteForTrigger3;
                        Debug.Log($"Botón {gameObject.name} cambió a spriteForTrigger3.");
                    }
                    break;
                default:
                    // Restaurar sprite base (no cambiar si count es 0 o > 3)
                    break;
            }
        }
    }
}