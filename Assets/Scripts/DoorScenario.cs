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
    public GameObject zoneToActivate; // Zona que se activará (destino)
    [Tooltip("The zone (GameObject) to deactivate when teleporting (origin zone)")]
    public GameObject zoneToDeactivate; // Zona que se desactivará (origen)

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

    private bool isPlayerOnDoor = false;
    private GameObject player;
    private CameraController cameraController;
    private PlayerDeath playerDeath;
    private bool isTeleporting = false;

    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("No se encontró el script CameraController en la cámara principal.");
        }

        if (indicatorObject != null)
        {
            indicatorObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se asignó un Indicator Object en el Inspector. No se mostrará ningún indicador.");
        }

        if (destinationDoor == null || originDoor == null)
        {
            Debug.LogError("Destination Door o Origin Door no están asignados en el Inspector.");
        }

        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel no asignado en el Inspector. Por favor asigna un componente Image para el efecto de fade.");
        }
        else
        {
            fadePanel.color = new Color(0, 0, 0, 0);
        }

        // Verificar que las zonas estén asignadas
        if (zoneToActivate == null)
        {
            Debug.LogWarning($"ZoneToActivate no asignado en la puerta {gameObject.name}. No se activará ninguna zona al teletransportarse.");
        }
        if (zoneToDeactivate == null)
        {
            Debug.LogWarning($"ZoneToDeactivate no asignado en la puerta {gameObject.name}. No se desactivará ninguna zona al teletransportarse.");
        }
    }

    void Update()
    {
        if (isPlayerOnDoor && Input.GetKeyDown(KeyCode.C) && !isTeleporting)
        {
            StartCoroutine(TeleportWithFade());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerOnDoor = true;
            player = collision.gameObject;
            playerDeath = player.GetComponent<PlayerDeath>();
            if (playerDeath == null)
            {
                Debug.LogError("PlayerDeath no encontrado en el jugador.");
            }
            Debug.Log("Jugador encima de la puerta. Presiona 'C' para teletransportarte.");

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
            Debug.LogError("No se encontró al jugador o el componente PlayerDeath para teletransportar.");
            yield break;
        }

        if (cameraController == null)
        {
            Debug.LogError("No se encontró el script CameraController para teletransportar la cámara.");
            yield break;
        }

        if (destinationDoor == null || originDoor == null)
        {
            Debug.LogError("Destination Door o Origin Door no están asignados.");
            yield break;
        }

        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel no asignado. No se puede realizar el efecto de fade.");
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

            // Activar y desactivar las zonas
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