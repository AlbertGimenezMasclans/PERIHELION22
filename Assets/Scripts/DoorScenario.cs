using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DoorScenario : MonoBehaviour
{
    [Header("Teleport Destinations")]
    [Tooltip("The GameObject representing the destination door (where the player will teleport to)")]
    public GameObject destinationDoor;
    [Tooltip("The GameObject representing the origin door (this door's position)")]
    public GameObject originDoor;

    [Header("Zone Management")]
    [Tooltip("The zone (GameObject) to activate when teleporting (destination zone)")]
    public GameObject zoneToActivate;
    [Tooltip("The zone (GameObject) to deactivate when teleporting (origin zone)")]
    public GameObject zoneToDeactivate;

    [Header("Indicator Object")]
    [Tooltip("The GameObject to show while the player is on the door")]
    public GameObject indicatorObject;

    [Header("Fade Settings")]
    [Tooltip("The UI Image (Fade Panel) used for the fade effect")]
    public Image fadePanel;
    [Tooltip("Duration of the fade-in effect (in seconds)")]
    public float fadeInTime = 0.45f;
    [Tooltip("Duration of the black screen (in seconds)")]
    public float blackScreenDuration = 1.10f;
    [Tooltip("Duration of the fade-out effect (in seconds)")]
    public float fadeOutTime = 0.45f;

    [Header("Deactivation Settings")]
    [Tooltip("Check to deactivate the door (disables collider and changes sprite)")]
    public bool isDeactivated = false;
    [Tooltip("The sprite to use when the door is deactivated")]
    public Sprite deactivatedSprite;
    [Tooltip("The sprite to use when the door is activated (optional, defaults to initial sprite)")]
    public Sprite activatedSprite;

    private bool isPlayerOnDoor = false;
    private GameObject player;
    private CameraController cameraController;
    private PlayerDeath playerDeath;
    private bool isTeleporting = false;
    private BoxCollider2D doorCollider;
    private SpriteRenderer doorSpriteRenderer;
    private Sprite originalSprite; // Para guardar el sprite original
    private bool lastDeactivationState; // Para detectar cambios en isDeactivated

    void Start()
    {
        // Obtener componentes necesarios
        cameraController = Camera.main.GetComponent<CameraController>();
        doorCollider = GetComponent<BoxCollider2D>();
        doorSpriteRenderer = GetComponent<SpriteRenderer>();

        if (cameraController == null)
        {
            Debug.LogError("No se encontró el script CameraController en la cámara principal.");
        }
        if (doorCollider == null)
        {
            Debug.LogError("No se encontró BoxCollider2D en la puerta.");
        }
        if (doorSpriteRenderer == null)
        {
            Debug.LogError("No se encontró SpriteRenderer en la puerta.");
        }
        else
        {
            originalSprite = doorSpriteRenderer.sprite; // Guardar sprite original
        }

        if (indicatorObject != null)
        {
            indicatorObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se asignó un Indicator Object en el Inspector.");
        }

        if (destinationDoor == null || originDoor == null)
        {
            Debug.LogError("Destination Door o Origin Door no están asignados en el Inspector.");
        }

        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel no asignado en el Inspector.");
        }
        else
        {
            fadePanel.color = new Color(0, 0, 0, 0);
        }

        if (zoneToActivate == null)
        {
            Debug.LogWarning($"ZoneToActivate no asignado en la puerta {gameObject.name}.");
        }
        if (zoneToDeactivate == null)
        {
            Debug.LogWarning($"ZoneToDeactivate no asignado en la puerta {gameObject.name}.");
        }

        // Aplicar estado inicial de desactivación
        lastDeactivationState = isDeactivated;
        UpdateDoorState();
    }

    void Update()
    {
        // Detectar cambios en el checkbox del Inspector (en modo Editor)
        if (lastDeactivationState != isDeactivated)
        {
            UpdateDoorState();
            lastDeactivationState = isDeactivated;
        }

        // Solo permitir teletransporte si la puerta está activa
        if (!isDeactivated && isPlayerOnDoor && Input.GetKeyDown(KeyCode.C) && !isTeleporting)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null && Mathf.Abs(playerRb.velocity.x) < 0.01f)
            {
                StartCoroutine(TeleportWithFade());
            }
            else
            {
                Debug.Log("El jugador debe estar quieto para entrar en la puerta.");
            }
        }
    }

    private void UpdateDoorState()
    {
        if (doorSpriteRenderer != null)
        {
            if (isDeactivated)
            {
                if (deactivatedSprite != null)
                {
                    doorSpriteRenderer.sprite = deactivatedSprite;
                }
                else
                {
                    Debug.LogWarning("No se asignó un Deactivated Sprite en el Inspector.");
                }
            }
            else
            {
                doorSpriteRenderer.sprite = activatedSprite != null ? activatedSprite : originalSprite;
            }
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = !isDeactivated;
        }

        // Desactivar indicador si la puerta está desactivada
        if (isDeactivated && indicatorObject != null)
        {
            indicatorObject.SetActive(false);
        }

        Debug.Log($"Puerta {gameObject.name} {(isDeactivated ? "desactivada" : "activada")}.");
    }

    // Método público para activar/desactivar la puerta programáticamente
    public void SetDoorActive(bool active)
    {
        isDeactivated = !active;
        UpdateDoorState();
        lastDeactivationState = isDeactivated;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDeactivated && collision.CompareTag("Player"))
        {
            isPlayerOnDoor = true;
            player = collision.gameObject;
            playerDeath = player.GetComponent<PlayerDeath>();
            if (playerDeath == null)
            {
                Debug.LogError("PlayerDeath no encontrado en el jugador.");
            }
            if (player.GetComponent<Rigidbody2D>() == null)
            {
                Debug.LogError("Rigidbody2D no encontrado en el jugador.");
            }
            Debug.Log("Jugador encima de la puerta. Debe estar quieto y presionar 'C'.");

            if (indicatorObject != null)
            {
                indicatorObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerOnDoor = false;
            player = null;
            playerDeath = null;
            Debug.Log("Jugador salió de la puerta.");

            if (indicatorObject != null)
            {
                indicatorObject.SetActive(false);
            }
        }
    }

    private IEnumerator TeleportWithFade()
    {
        if (player == null || playerDeath == null)
        {
            Debug.LogError("No se encontró al jugador o el componente PlayerDeath.");
            yield break;
        }

        if (cameraController == null)
        {
            Debug.LogError("No se encontró el script CameraController.");
            yield break;
        }

        if (destinationDoor == null || originDoor == null)
        {
            Debug.LogError("Destination Door o Origin Door no están asignados.");
            yield break;
        }

        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel no asignado.");
            yield break;
        }

        isTeleporting = true;

        try
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeInTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadePanel.color = new Color(0, 0, 0, 1f);

            Vector3 destinationPosition = destinationDoor.transform.position;
            destinationPosition.y += 0.2f;
            Debug.Log($"Teletransportando al jugador a: {destinationPosition}");
            player.transform.position = new Vector3(destinationPosition.x, destinationPosition.y, player.transform.position.z);

            cameraController.TeleportCamera(new Vector2(destinationPosition.x, destinationPosition.y));

            if (zoneToDeactivate != null)
            {
                zoneToDeactivate.SetActive(false);
                Debug.Log($"Zona desactivada: {zoneToDeactivate.name}");
            }
            if (zoneToActivate != null)
            {
                zoneToActivate.SetActive(true);
                Debug.Log($"Zona activada: {zoneToActivate.name}");
            }

            yield return new WaitForSeconds(blackScreenDuration);

            elapsedTime = 0f;
            while (elapsedTime < fadeOutTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutTime);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadePanel.color = new Color(0, 0, 0, 0f);
        }
        finally
        {
            isTeleporting = false;
            Debug.Log("Teletransporte completado.");
        }
    }
}