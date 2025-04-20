using UnityEngine;

public class ShootableButton : MonoBehaviour
{
    [SerializeField] private Sprite unpressedSprite; // Sprite cuando no está presionado
    [SerializeField] private Sprite pressedSprite; // Sprite cuando está presionado
    [SerializeField] private GameObject[] switchObjects; // Objetos a desactivar cuando se presiona
    [SerializeField] private GameObject[] objectsToActivate; // Objetos a activar cuando se presiona
    [SerializeField] private DoorScenario[] doorsToActivate; // Puertas a activar cuando se presiona
    [SerializeField] private AudioClip pressSound; // Sonido al presionar el botón

    private SpriteRenderer buttonSprite; // Referencia al SpriteRenderer del botón
    private AudioSource audioSource; // Componente para reproducir el sonido
    private bool isPressed; // Estado del botón (presionado o no)
    private bool isUsed; // Indica si el botón ya fue usado

    private void Start()
    {
        // Obtener componentes
        buttonSprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Configurar AudioSource si es necesario
        if (audioSource == null && pressSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Establecer sprite inicial
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }

        // Inicializar estado
        isPressed = false;
        isUsed = false;

        // Configurar objetos iniciales
        foreach (GameObject obj in switchObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Validar puertas
        foreach (DoorScenario door in doorsToActivate)
        {
            if (door == null)
            {
                Debug.LogWarning($"Una puerta en doorsToActivate no está asignada en el botón {gameObject.name}.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el objeto que colisiona es una bala y el botón no ha sido usado
        if (!isUsed && other.CompareTag("Bullet"))
        {
            PressButtonAction();
            isUsed = true; // Marcar el botón como usado
        }
    }

    private void PressButtonAction()
    {
        isPressed = true;

        // Cambiar sprite
        if (buttonSprite != null && pressedSprite != null)
        {
            buttonSprite.sprite = pressedSprite;
        }

        // Reproducir sonido
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        // Desactivar switchObjects
        foreach (GameObject obj in switchObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Activar objectsToActivate
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // Activar puertas
        foreach (DoorScenario door in doorsToActivate)
        {
            if (door != null)
            {
                door.SetDoorActive(true); // Activar la puerta (cambia sprite y habilita collider)
                Debug.Log($"Puerta {door.gameObject.name} activada por el botón {gameObject.name}.");
            }
        }
    }
}