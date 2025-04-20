using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveButton : MonoBehaviour
{
    [SerializeField] private GameObject indicator; // GameObject que actúa como indicador
    [SerializeField] private GameObject objectToDeactivate; // Objeto a desactivar cuando TriggerCount llegue a 3
    [SerializeField] private AudioClip pressSound; // Sonido al presionar el botón

    [Header("Sprite Settings")]
    [SerializeField] private Sprite spriteForTrigger1; // Sprite cuando TriggerCount = 1
    [SerializeField] private Sprite spriteForTrigger2; // Sprite cuando TriggerCount = 2
    [SerializeField] private Sprite spriteForTrigger3; // Sprite cuando TriggerCount = 3

    private static int TriggerCount = 0; // Contador global de triggers
    private static List<GameObject> ObjectsToDeactivate = new List<GameObject>(); // Lista de objetos a desactivar
    private static bool HasDeactivatedObjects = false; // Indica si los objetos ya fueron desactivados
    private static List<ActiveButton> AllButtons = new List<ActiveButton>(); // Lista de todos los botones

    private bool isPlayerNearby; // Indica si el jugador está dentro del trigger
    private bool isUsed; // Indica si el botón ya fue usado
    private AudioSource audioSource; // Componente para reproducir el sonido
    private SpriteRenderer spriteRenderer; // Componente para cambiar el sprite

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

        // Validar y añadir objeto a desactivar
        if (objectToDeactivate != null)
        {
            if (!ObjectsToDeactivate.Contains(objectToDeactivate))
            {
                ObjectsToDeactivate.Add(objectToDeactivate);
                Debug.Log($"Objeto {objectToDeactivate.name} añadido a la lista de desactivación por el botón {gameObject.name}.");
            }
        }
        else
        {
            Debug.LogWarning($"ObjectToDeactivate no asignado en el botón {gameObject.name}.");
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

        // Añadir este botón a la lista estática
        AllButtons.Add(this);

        // Validar sprites
        if (spriteForTrigger1 == null) Debug.LogWarning($"spriteForTrigger1 no asignado en el botón {gameObject.name}.");
        if (spriteForTrigger2 == null) Debug.LogWarning($"spriteForTrigger2 no asignado en el botón {gameObject.name}.");
        if (spriteForTrigger3 == null) Debug.LogWarning($"spriteForTrigger3 no asignado en el botón {gameObject.name}.");
    }

    private void OnDestroy()
    {
        // Remover este botón de la lista al ser destruido
        AllButtons.Remove(this);
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

        // Incrementar el contador global
        TriggerCount++;
        Debug.Log($"TriggerCount incrementado a {TriggerCount} por el botón {gameObject.name}.");

        // Actualizar sprites de todos los botones
        UpdateAllButtonSprites();

        // Verificar si el contador alcanzó 3
        if (TriggerCount == 3 && !HasDeactivatedObjects)
        {
            DeactivateObjects();
            StartCoroutine(ResetTriggerCountAfterDelay());
        }
    }

    private static void UpdateAllButtonSprites()
    {
        foreach (ActiveButton button in AllButtons)
        {
            if (button != null && button.spriteRenderer != null)
            {
                switch (TriggerCount)
                {
                    case 1:
                        if (button.spriteForTrigger1 != null)
                        {
                            button.spriteRenderer.sprite = button.spriteForTrigger1;
                            Debug.Log($"Botón {button.gameObject.name} cambió a spriteForTrigger1.");
                        }
                        break;
                    case 2:
                        if (button.spriteForTrigger2 != null)
                        {
                            button.spriteRenderer.sprite = button.spriteForTrigger2;
                            Debug.Log($"Botón {button.gameObject.name} cambió a spriteForTrigger2.");
                        }
                        break;
                    case 3:
                        if (button.spriteForTrigger3 != null)
                        {
                            button.spriteRenderer.sprite = button.spriteForTrigger3;
                            Debug.Log($"Botón {button.gameObject.name} cambió a spriteForTrigger3.");
                        }
                        break;
                    default:
                        // Mantener el sprite base (no cambiar si TriggerCount es 0 o > 3)
                        break;
                }
            }
        }
    }

    private static void DeactivateObjects()
    {
        HasDeactivatedObjects = true;
        foreach (GameObject obj in ObjectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Objeto {obj.name} desactivado porque TriggerCount alcanzó 3.");
            }
        }
    }

    private IEnumerator ResetTriggerCountAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        TriggerCount = 0;
        HasDeactivatedObjects = false; // Permitir futuras desactivaciones
        UpdateAllButtonSprites(); // Restaurar sprites base
        Debug.Log("TriggerCount reiniciado a 0 después de 0.5 segundos.");
    }

    // Método para reiniciar el contador global y el estado (útil para reiniciar el nivel)
    public static void ResetTriggerCount()
    {
        TriggerCount = 0;
        HasDeactivatedObjects = false;
        ObjectsToDeactivate.Clear();
        AllButtons.Clear();
        Debug.Log("TriggerCount, lista de objetos y lista de botones reiniciados.");
    }
}